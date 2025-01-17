//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Extensions;
using Karamem0.BookingsBot.Models;
using Karamem0.BookingsBot.Resources;
using Karamem0.BookingsBot.Steps.Abstraction;
using Microsoft.Agents.BotBuilder.Dialogs;
using Microsoft.Agents.BotBuilder.Dialogs.Choices;
using Microsoft.Agents.Protocols.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps;

public class BookingDateStep : ChoicePromptStep
{

    public BookingDateStep()
    {
    }

    public override string DialogId => "8d896d64-7ee7-4be0-b3ff-ab7c821519e0";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // 日付の一覧を取得する
        var bookingAvailableTime = stepContext.GetValue<DateTime?>("BookingAvailableTime");
        if (bookingAvailableTime is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingAvailableTime)
            ));
        }
        var bookingDates = Enumerable.Range(0, 6)
            .Select(item => bookingAvailableTime!.Value.Date.AddDays(item))
            .Select(item => new BookingDateOption()
            {
                Id = item,
                DisplayName = item.ToString("m", CultureInfo.GetCultureInfo(stepContext.GetLocale()).DateTimeFormat)
            });
        // 値を一時的なプロパティに格納する
        stepContext.SetValue("BookingDates", bookingDates.ToArray());
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(bookingDates.Select(item => item.DisplayName).ToArray()),
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
        if (stepContext.Result is not FoundChoice foundChoice)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(foundChoice)
            ));
        }
        // ダイアログで選択された日付を取得する
        var bookingDates = stepContext.GetValue<BookingDateOption[]?>("BookingDates");
        if (bookingDates is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingDates)
            ));
        }
        var bookingDateId = bookingDates[foundChoice.Index].Id;
        // 値を一時的なプロパティに格納する
        stepContext.SetValue("BookingDateId", bookingDateId);
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
        var bookingDates = promptContext.Options.GetValidation<BookingDateOption[]?>("BookingDates");
        if (bookingDates is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingDates)
            ));
        }
        var bookingDateId = bookingDates[value.Index].Id;
        if (bookingDateId is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingDateId)
            ));
        }
        // 時刻の一覧を取得する
        var bookingAvailableTime = promptContext.Options.GetValidation<DateTime?>("BookingAvailableTime");
        var bookingBusinessHours = promptContext.Options.GetValidation<BookingBusinessHourOption[]?>("BookingBusinessHours");
        if (bookingBusinessHours is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingBusinessHours)
            ));
        }
        var bookingBusinessHour = bookingBusinessHours.FirstOrDefault(item => item.DayOfWeek == bookingDateId.Value.DayOfWeek);
        if (bookingBusinessHour is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingBusinessHour)
            ));
        }
        var bookingTimes = bookingBusinessHour.TimeSlots?
            .Select(item => bookingDateId.Value.AddTicks(item.Ticks))
            .Where(item => item > bookingAvailableTime);
        if (bookingTimes is not null && bookingTimes.Any())
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

}
