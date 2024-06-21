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
using Karamem0.BookingsBot.Services;
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

public class BookingBusinessStep : ChoiceStep
{

    private readonly UserState userState;

    private readonly IGraphService graphService;

    public BookingBusinessStep(UserState userState, IGraphService graphService)
    {
        this.userState = userState;
        this.graphService = graphService;
    }

    public override string DialogId => "d0f27525-35ca-455e-b87e-206a4561249b";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // ビジネスの一覧を取得する
        var bookingBusinesses = await this.graphService
            .GetBookingBusinessesAsync(cancellationToken)
            .ContinueWith(
                task => task.Result
                    .Where(item => item.Id is not null)
                    .Select(item => new KeyValuePair<string, string?>(item.Id!, item.DisplayName))
                    .ToArray()
            )
            .ConfigureAwait(false);
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingBusinesses"] = bookingBusinesses;
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(bookingBusinesses.Select(item => item.Value).ToList()),
                Prompt = MessageFactory.Text(StringResources.ChooseBookingBusinessMessage),
                RetryPrompt = MessageFactory.Text(StringResources.RetryBookingBusinessMessage),
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
        // ダイアログで選択されたビジネスを取得する
        var bookingBusinesses = stepContext.Values["BookingBusinesses"] as KeyValuePair<string, string?>[];
        if (bookingBusinesses is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingBusinesses)
            ));
        }
        var bookingBusinessId = bookingBusinesses[foundChoice.Index].Key;
        var bookingBusiness = await this.graphService
            .GetBookingBusinessAsync(bookingBusinessId, cancellationToken)
            .ConfigureAwait(false);
        // ビジネスの情報をプロファイルに格納する
        bookingProfile.BookingBusinessId = bookingBusiness.Id;
        bookingProfile.BookingBusinessName = bookingBusiness.DisplayName;
        var timeSlotInterval = bookingBusiness.SchedulingPolicy?.TimeSlotInterval;
        if (timeSlotInterval is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(timeSlotInterval)
            ));
        }
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingAvailableTime"] = DateTime.Now.AddTicks(bookingBusiness.SchedulingPolicy?.MinimumLeadTime?.Ticks ?? 0);
        stepContext.Values["BookingBusinessHours"] = bookingBusiness.BusinessHours?
            .Where(item => item.Day != null)
            .ToDictionary(
                item => (DayOfWeek)item.Day!.Value,
                item =>
                {
                    var timeSpans = new List<TimeSpan>();
                    var timeSlots = item.TimeSlots?
                        .Select(slot => new
                        {
                            item.Day,
                            StartTime = slot.StartTime.GetValueOrDefault().ToTimeSpan(),
                            EndTime = slot.EndTime.GetValueOrDefault().ToTimeSpan(),
                        })
                        .ToArray();
                    if (timeSlots != null && timeSlots.Length > 0)
                    {
                        var startTime = timeSlots.Select(slot => slot.StartTime).Min();
                        var endTime = timeSlots.Select(slot => slot.EndTime).Min();
                        for (var currentTime = startTime; currentTime < endTime; currentTime = currentTime.Add(timeSlotInterval!.Value))
                        {
                            if (Array.Exists(timeSlots, time => currentTime >= time.StartTime || currentTime < time.EndTime))
                            {
                                timeSpans.Add(currentTime);
                            }
                        }
                    }
                    return timeSpans.ToArray();
                }
            );
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
