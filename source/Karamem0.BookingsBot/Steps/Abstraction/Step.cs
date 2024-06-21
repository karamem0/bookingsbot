//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps.Abstraction;

public abstract class Step
{

    protected Step()
    {
    }

    public abstract string DialogId { get; }

    public abstract Dialog Dialog { get; }

    public async Task<DialogTurnResult> OnBeforeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        try
        {
            return await this.OnBeforeCoreAsync(stepContext, cancellationToken);
        }
        catch (Exception ex)
        {
            _ = await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorUnexpectedMessage),
                    ex.Message
                )),
                cancellationToken: cancellationToken
            );
            // ダイアログを終了する
            return await stepContext.EndDialogAsync(ex, cancellationToken);
        }
    }

    public virtual async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        return await stepContext.NextAsync(cancellationToken: cancellationToken);
    }

    public async Task<DialogTurnResult> OnAfterAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        try
        {
            return await this.OnAfterCoreAsync(stepContext, cancellationToken);
        }
        catch (Exception ex)
        {
            _ = await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorUnexpectedMessage),
                    ex.Message
                )),
                cancellationToken: cancellationToken
            );
            // ダイアログを終了する
            return await stepContext.EndDialogAsync(ex, cancellationToken);
        }
    }

    public virtual async Task<DialogTurnResult> OnAfterCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        return await stepContext.NextAsync(cancellationToken: cancellationToken);
    }

}
