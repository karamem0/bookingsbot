//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Resources;
using Karamem0.BookingsBot.Steps.Abstraction;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps;

public class BookingDateStep : ChoiceStep
{

    public BookingDateStep()
    {
    }

    public override string DialogId => "8d896d64-7ee7-4be0-b3ff-ab7c821519e0";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // 日付の一覧を取得する
        var bookingAvailableTime = stepContext.Values["BookingAvailableTime"] as DateTime?;
        if (bookingAvailableTime is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingAvailableTime)
            ));
        }
        var bookingDates = Enumerable.Range(0, 6)
            .Select(item => bookingAvailableTime!.Value.AddDays(item).Date)
            .Select(item => new KeyValuePair<DateTime, string>(item, item.ToString("m")));
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingDates"] = bookingDates.ToArray();
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(bookingDates.Select(item => item.Value).ToArray()),
                Prompt = MessageFactory.Text(StringResources.ChooseBookingDateMessage),
                RetryPrompt = MessageFactory.Text(StringResources.RetryBookingDateMessage),
                Validations = stepContext.Values
            },
            cancellationToken
        );
    }

    public override async Task<DialogTurnResult> OnAfterCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
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
        // ダイアログで選択された日付を取得する
        var bookingDates = stepContext.Values["BookingDates"] as KeyValuePair<DateTime, string>[];
        if (bookingDates is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingDates)
            ));
        }
        var bookingDateId = bookingDates[foundChoice.Index].Key;
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingDateId"] = bookingDateId;
        // 次のステップに進む
        return await stepContext.NextAsync(cancellationToken: cancellationToken);
    }

    public override Task<bool> OnValidateAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken = default)
    {
        // ダイアログで選択された日付を取得する
        var value = promptContext.Recognized.Value;
        if (value is null)
        {
            return Task.FromResult(false);
        }
        var contextValues = promptContext.Options.Validations as IDictionary<string, object>;
        if (contextValues is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(contextValues)
            ));
        }
        var bookingDates = contextValues["BookingDates"] as KeyValuePair<DateTime, string>[];
        if (bookingDates is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingDates)
            ));
        }
        var bookingDateId = bookingDates[value.Index].Key;
        // 時刻の一覧を取得する
        var bookingAvailableTime = contextValues["BookingAvailableTime"] as DateTime?;
        var bookingBusinessHours = contextValues["BookingBusinessHours"] as IDictionary<DayOfWeek, TimeSpan[]>;
        if (bookingBusinessHours is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingBusinessHours)
            ));
        }
        var bookingTimes = bookingBusinessHours[bookingDateId.DayOfWeek]
            .Select(item => bookingDateId.Date.AddTicks(item.Ticks))
            .Where(item => item > bookingAvailableTime);
        if (!bookingTimes.Any())
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

}
