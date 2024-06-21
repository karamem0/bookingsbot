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
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps;

public class BookingStaffMemberStep : ChoiceStep
{

    private readonly UserState userState;

    private readonly IGraphService graphService;

    public BookingStaffMemberStep(UserState userState, IGraphService graphService)
    {
        this.userState = userState;
        this.graphService = graphService;
    }

    public override string DialogId => "b6cdce8e-4c10-4fc8-9493-1e9eb06f7c7a";

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
        // スタッフの一覧を取得する
        var bookingStaffMembers = await this.graphService
            .GetBookingStaffMembersAsync(bookingBusinessId, cancellationToken)
            .ContinueWith(
                task => task.Result
                    .Select(item => new KeyValuePair<string, string?>(item.Id!, item.DisplayName))
                    .ToArray()
            )
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(StringResources.ErrorNoBookingServiceMessage);
        // 値を一時的なプロパティに格納する
        stepContext.Values["BookingStaffMembers"] = bookingStaffMembers;
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(bookingStaffMembers.Select(item => item.Value).ToArray()),
                Prompt = MessageFactory.Text(StringResources.ChooseBookingStaffMemberMessage),
                RetryPrompt = MessageFactory.Text(StringResources.RetryBookingStaffMemberMessage),
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
        // ダイアログで選択されたスタッフを取得する
        var bookingStaffMembers = stepContext.Values["BookingStaffMembers"] as KeyValuePair<string, string?>[];
        if (bookingStaffMembers is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(bookingStaffMembers)
            ));
        }
        var bookingStaffMemberId = bookingStaffMembers[foundChoice.Index].Key;
        // スタッフの情報をプロファイルに格納する
        bookingProfile.BookingStaffMemberId = bookingStaffMemberId;
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
