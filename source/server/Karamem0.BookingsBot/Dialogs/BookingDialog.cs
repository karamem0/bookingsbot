//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Resources;
using Karamem0.BookingsBot.Steps;
using Microsoft.Agents.BotBuilder.Dialogs;
using Microsoft.Agents.Protocols.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Dialogs;

public class BookingDialog : ComponentDialog
{

    public BookingDialog(BookingStepCollection collection)
        : base(nameof(BookingDialog))
    {
        _ = this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), collection.Actions));
        foreach (var dialog in collection.Dialogs)
        {
            _ = this.AddDialog(dialog);
        }
    }

    protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken = default)
    {
        if (dialogContext.Context.Activity.Type == ActivityTypes.Message)
        {
            var message = dialogContext.Context.Activity.Text.Trim();
            if (message == StringResources.CancelText)
            {
                _ = await dialogContext.Context.SendActivityAsync(MessageFactory.Text(StringResources.CancelBookingMessage), cancellationToken);
                _ = await dialogContext.CancelAllDialogsAsync(cancellationToken);
                return await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }
        }
        return await base.OnContinueDialogAsync(dialogContext, cancellationToken);
    }

}
