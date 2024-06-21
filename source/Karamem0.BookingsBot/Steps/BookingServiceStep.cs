//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

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

public class BookingServiceStep : ChoiceStep
{

    private readonly UserState userState;

    private readonly IGraphService graphService;

    public BookingServiceStep(UserState userState, IGraphService graphService)
    {
        this.userState = userState;
        this.graphService = graphService;
    }

    public override string DialogId => "e369e4d7-815c-4331-809d-6336cd85d9c5";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // プロファイルを取得する
        var bookingProfileAccessor = this.userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
        var bookingProfile = await bookingProfileAccessor.GetAsync(stepContext.Context, () => new BookingProfile(), cancellationToken);
        // ダイアログで選択されたビジネスIDを取得する
        var bookingBusinessId = bookingProfile.BookingBusinessId;
        if (bookingBusinessId is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingBusinessId)
            ));
        }
        // サービスの一覧を取得する
        var bookingServices = await this.graphService
            .GetBookingServicesAsync(bookingBusinessId, cancellationToken)
            .ContinueWith(
                task => task.Result
                    .Select(item => new KeyValuePair<string, string?>(item.Id!, item.DisplayName))
                    .ToArray()
            )
            .ConfigureAwait(false);
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingServices"] = bookingServices;
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(bookingServices.Select(item => item.Value).ToList()),
                Prompt = MessageFactory.Text(StringResources.ChooseBookingServiceMessage),
                RetryPrompt = MessageFactory.Text(StringResources.RetryBookingServiceMessage),
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
        // ダイアログで選択されたビジネスIDを取得する
        var bookingBusinessId = bookingProfile.BookingBusinessId;
        if (bookingBusinessId is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingBusinessId)
            ));
        }
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
        // ダイアログで選択されたサービスを取得する
        var bookingServices = stepContext.Values["BookingServices"] as KeyValuePair<string, string?>[];
        var bookingServiceId = bookingServices?[foundChoice.Index].Key;
        if (bookingServiceId is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingServiceId)
            ));
        }
        var bookingService = await this.graphService
            .GetBookingServiceAsync(bookingBusinessId, bookingServiceId, cancellationToken)
            .ConfigureAwait(false);
        // サービスの情報をプロファイルに格納する
        bookingProfile.BookingServiceId = bookingService.Id;
        bookingProfile.BookingServiceName = bookingService.DisplayName;
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingDuration"] = bookingService.DefaultDuration;
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
