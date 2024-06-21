//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps.Abstraction;

public abstract class ConfirmStep : Step
{

    protected ConfirmStep()
    {
    }

    public override Dialog Dialog => new ConfirmPrompt(this.DialogId, this.OnValidateAsync);

    public virtual Task<bool> OnValidateAsync(PromptValidatorContext<bool> promptContext, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

}

