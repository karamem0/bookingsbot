//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Dialogs;
using Karamem0.BookingsBot.Steps.Abstraction;
using Microsoft.Agents.BotBuilder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps;

public class MainStep(BookingDialog dialog) : DialogStep<BookingDialog>(dialog)
{

    public override string DialogId => nameof(BookingDialog);

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        return await stepContext.BeginDialogAsync(this.DialogId, cancellationToken: cancellationToken);
    }

    public override async Task<DialogTurnResult> OnAfterCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }

}
