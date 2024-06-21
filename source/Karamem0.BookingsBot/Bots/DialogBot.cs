//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Dialogs;
using Karamem0.BookingsBot.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Bots;

public class DialogBot : ActivityHandler
{

    private readonly BotState conversationState;

    private readonly BotState userState;

    private readonly DialogSet dialogs;

    public DialogBot(ConversationState conversationState, UserState userState, BookingDialog bookingDialog)
    {
        this.conversationState = conversationState;
        this.userState = userState;
        this.dialogs = new DialogSet(this.userState.CreateProperty<DialogState>(nameof(DialogState)));
        _ = this.dialogs.Add(bookingDialog);
    }

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        var dialogContext = await this.dialogs.CreateContextAsync(turnContext, cancellationToken);
        if (turnContext.Activity.Type == ActivityTypes.Message)
        {
            var message = turnContext.Activity.Text;
            if (message == StringResources.CancelText)
            {
                _ = await turnContext.SendActivityAsync(MessageFactory.Text(StringResources.CancelBookingMessage), cancellationToken);
                _ = await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }
            else
            {
                var result = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (result.Status == DialogTurnStatus.Empty)
                {
                    _ = await turnContext.SendActivityAsync(MessageFactory.Text(StringResources.HelloMessage), cancellationToken);
                    _ = await dialogContext.BeginDialogAsync(nameof(BookingDialog), null, cancellationToken);
                }
            }
        }
        if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
        {
            _ = await turnContext.SendActivityAsync(MessageFactory.Text(StringResources.HelloMessage), cancellationToken);
            _ = await dialogContext.BeginDialogAsync(nameof(BookingDialog), null, cancellationToken);
        }
        await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

}
