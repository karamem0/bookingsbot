//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using AdaptiveCards;
using Karamem0.BookingsBot.Models;
using Karamem0.BookingsBot.Resources;
using Karamem0.BookingsBot.Services;
using Karamem0.BookingsBot.Steps.Abstraction;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps;

public class BookingConfirmStep : ConfirmStep
{

    private readonly UserState userState;

    private readonly IGraphService graphService;

    public BookingConfirmStep(UserState userState, IGraphService graphService)
    {
        this.userState = userState;
        this.graphService = graphService;
    }

    public override string DialogId => "c5edb76e-35d6-456f-b06b-9a72b03e3351";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // プロファイルを取得する
        var bookingProfileAccessor = this.userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
        var bookingProfile = await bookingProfileAccessor.GetAsync(stepContext.Context, () => new BookingProfile(), cancellationToken);
        // アダプティブ カードを作成する
        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
        {
            Body =
            [
                new AdaptiveFactSet()
                {
                    Facts =
                    [
                        new(StringResources.BookingBusinessLabel, bookingProfile.BookingBusinessName),
                        new(StringResources.BookingServiceLabel, bookingProfile.BookingServiceName),
                        new(StringResources.BookingStartTimeLabel, bookingProfile.BookingStartTime?.ToString("g")),
                        new(StringResources.BookingEndTimeLabel, bookingProfile.BookingEndTime?.ToString("g")),
                        new(StringResources.BookingCustomerNameLabel, bookingProfile.BookingCustomerName),
                        new(StringResources.BookingCustomerEmailLabel, bookingProfile.BookingCustomerEmail),
                    ]
                }
            ]
        };
        // アダプティブ カードを送信する
        _ = await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(
            new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(card.ToJson())
            }),
            cancellationToken
        );
        // ダイアログを作成する
        return await stepContext.PromptAsync(
            this.DialogId,
            new PromptOptions
            {
                Prompt = MessageFactory.Text(StringResources.ConfirmBookingMessage)
            },
            cancellationToken
        );
    }

    public override async Task<DialogTurnResult> OnAfterCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // プロファイルを取得する
        var bookingProfileAccessor = this.userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
        var bookingProfile = await bookingProfileAccessor.GetAsync(stepContext.Context, () => new BookingProfile(), cancellationToken);
        // ダイアログで入力された結果を取得する
        var confirm = stepContext.Result as bool?;
        if (confirm is true)
        {
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
            // ダイアログで選択されたサービスIDを取得する
            var bookingServiceId = bookingProfile.BookingServiceId;
            if (bookingServiceId is null)
            {
                throw new InvalidOperationException(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                    nameof(bookingServiceId)
                ));
            }
            // ダイアログで入力された名前を取得する
            var bookingCustomerName = bookingProfile.BookingCustomerName;
            // ダイアログで入力された電子メール アドレスを取得する
            var bookingCustomerEmail = bookingProfile.BookingCustomerEmail;
            if (bookingCustomerEmail is null)
            {
                throw new InvalidOperationException(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                    nameof(bookingCustomerEmail)
                ));
            }
            // ダイアログで選択されたスタッフを取得する
            var bookingStaffMemberId = bookingProfile.BookingStaffMemberId;
            if (bookingStaffMemberId is null)
            {
                throw new InvalidOperationException(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                    nameof(bookingStaffMemberId)
                ));
            }
            // 予約を作成する
            _ = await this.graphService
                .CreateBookingAppointmentAsync(
                    bookingBusinessId,
                    new BookingAppointment()
                    {
                        CustomerName = bookingCustomerName,
                        CustomerEmailAddress = bookingCustomerEmail,
                        EndDateTime = new DateTimeTimeZone()
                        {
                            DateTime = bookingProfile.BookingEndTime?.ToUniversalTime().ToString("s"),
                            TimeZone = "UTC",
                        },
                        ServiceId = bookingServiceId,
                        ServiceName = bookingProfile.BookingServiceName,
                        StartDateTime = new DateTimeTimeZone()
                        {
                            DateTime = bookingProfile.BookingStartTime?.ToUniversalTime().ToString("s"),
                            TimeZone = "UTC",
                        },
                        StaffMemberIds = [bookingStaffMemberId]
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
            // メッセージを送信する
            _ = await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(StringResources.CompleteBookingMessage),
                cancellationToken
            );
        }
        else
        {
            // メッセージを送信する
            _ = await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(StringResources.CancelBookingMessage),
                cancellationToken
            );
        }
        // ダイアログを終了する
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

}
