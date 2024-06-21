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
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps;

public class BookingTimeStep : ChoiceStep
{

    private readonly UserState userState;

    public BookingTimeStep(UserState userState)
    {
        this.userState = userState;
    }

    public override string DialogId => "c34e585e-940b-4815-a2c7-174918392333";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // 日付を取得する
        var bookingDateId = stepContext.Values["BookingDateId"] as DateTime?;
        if (bookingDateId is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingDateId)
            ));
        }
        // 予約開始時間を取得する
        var bookingAvailableTime = stepContext.Values["BookingAvailableTime"] as DateTime?;
        if (bookingAvailableTime is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingAvailableTime)
            ));
        }
        // 予約時間枠の一覧を取得する
        var bookingBusinessHours = stepContext.Values["BookingBusinessHours"] as IDictionary<DayOfWeek, TimeSpan[]>;
        if (bookingBusinessHours is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingBusinessHours)
            ));
        }
        var bookingTimes = bookingBusinessHours[bookingDateId.Value.DayOfWeek]
            .Select(item => bookingDateId.Value.Date.AddTicks(item.Ticks))
            .Where(item => item > bookingAvailableTime)
            .Select(item => new KeyValuePair<DateTime, string>(item, item.ToString("t")));
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingTimes"] = bookingTimes.ToArray();
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(bookingTimes.Select(item => item.Value).ToArray()),
                Prompt = MessageFactory.Text(StringResources.ChooseBookingTimeMessage),
                RetryPrompt = MessageFactory.Text(StringResources.RetryBookingTimeMessage),
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
        // ダイアログの結果を取得する
        var foundChoice = stepContext.Result as FoundChoice;
        if (foundChoice is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(foundChoice)
            ));
        }
        // ダイアログで選択された時刻を取得する
        var bookingTimes = stepContext.Values["BookingTimes"] as KeyValuePair<DateTime, string>[];
        if (bookingTimes is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingTimes)
            ));
        }
        var bookingTimeId = bookingTimes[foundChoice.Index].Key;
        if (!TimeSpan.TryParse(
            stepContext.Values["BookingDuration"] as string,
            CultureInfo.CurrentCulture.DateTimeFormat,
            out var bookingDuration
        ))
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingDuration)
            ));
        }
        // 日時の情報をプロファイルに格納する
        bookingProfile.BookingStartTime = bookingTimeId;
        bookingProfile.BookingEndTime = bookingTimeId.AddTicks(bookingDuration.Ticks);
        // 次のステップに進む
        return await stepContext.NextAsync(cancellationToken: cancellationToken);
    }

    public override Task<bool> OnValidateAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken = default)
    {
        var value = promptContext.Recognized.Value;
        if (value is null)
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

}
