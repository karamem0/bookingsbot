//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Models;
using Karamem0.BookingsBot.Resources;
using Karamem0.BookingsBot.Steps.Abstraction;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps;

public class BookingCustomerNameStep : TextStep
{

    private readonly UserState userState;

    public BookingCustomerNameStep(UserState userState)
    {
        this.userState = userState;
    }

    public override string DialogId => "3d5cfc47-596d-4558-ba85-af05014614ab";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Prompt = MessageFactory.Text(StringResources.EnterBookingCustomerNameMessage),
                RetryPrompt = MessageFactory.Text(StringResources.RetryBookingCustomerNameMessage),
                Validations = stepContext.Values
            },
            cancellationToken
        );
    }

    public override async Task<DialogTurnResult> OnAfterCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // プロファイルを取得する
        var bookingProfileAccessor = this.userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
        var bookingProfile = await bookingProfileAccessor.GetAsync(stepContext.Context, () => new BookingProfile(), cancellationToken);
        // ダイアログで入力された名前を取得する
        var bookingCustomerName = stepContext.Result as string;
        // 名前の情報をプロファイルに格納する
        bookingProfile.BookingCustomerName = bookingCustomerName;
        // 次のステップに進む
        return await stepContext.NextAsync(cancellationToken: cancellationToken);
    }

    public override Task<bool> OnValidateAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken = default)
    {
        var value = promptContext.Recognized.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

}
