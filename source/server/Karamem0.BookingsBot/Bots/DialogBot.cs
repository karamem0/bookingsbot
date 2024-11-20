//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Resources;
using Microsoft.Agents.BotBuilder;
using Microsoft.Agents.BotBuilder.Dialogs;
using Microsoft.Agents.Protocols.Adapter;
using Microsoft.Agents.Protocols.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Bots;

public class DialogBot<T>(ConversationState conversationState, UserState userState, T dialog) : ActivityHandler where T : Dialog
{

    private readonly ConversationState conversationState = conversationState;

    private readonly UserState userState = userState;

    private readonly T dialog = dialog;

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);
        await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(
        IList<ChannelAccount> membersAdded,
        ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken = default
    )
    {
        foreach (var member in turnContext.Activity.MembersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                _ = await turnContext.SendActivityAsync(
                    MessageFactory.Text(StringResources.HelloMessage),
                    cancellationToken
                );
                await this.dialog.RunAsync(
                    turnContext,
                    this.conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                    cancellationToken
                );
            }
        }
    }

    protected override async Task OnMessageActivityAsync(
        ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken = default
    )
    {
        await this.dialog.RunAsync(
            turnContext,
            this.conversationState.CreateProperty<DialogState>(nameof(DialogState)),
            cancellationToken
        );
    }

}
