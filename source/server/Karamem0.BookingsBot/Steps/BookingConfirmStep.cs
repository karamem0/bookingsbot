//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using AdaptiveCards.Templating;
using Karamem0.BookingsBot.Models;
using Karamem0.BookingsBot.Resources;
using Karamem0.BookingsBot.Services;
using Karamem0.BookingsBot.Steps.Abstraction;
using Microsoft.Agents.BotBuilder;
using Microsoft.Agents.BotBuilder.Dialogs;
using Microsoft.Agents.Protocols.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Graph = Microsoft.Graph.Models;

namespace Karamem0.BookingsBot.Steps;

public class BookingConfirmStep(UserState userState, IGraphService graphService) : ConfirmStep
{

    private static readonly string confirmCardName = "Karamem0.BookingsBot.Resources.Cards.BookingConfirmCard.json";

    private readonly UserState userState = userState;

    private readonly IGraphService graphService = graphService;

    public override string DialogId => "c5edb76e-35d6-456f-b06b-9a72b03e3351";

    public override async Task<DialogTurnResult> OnBeforeCoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
    {
        // プロファイルを取得する
        var bookingProfileAccessor = this.userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
        var bookingProfile = await bookingProfileAccessor.GetAsync(stepContext.Context, () => new BookingProfile(), cancellationToken);
        // アダプティブ カードを作成する
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(confirmCardName);
        if (stream is null)
        {
            throw new InvalidOperationException(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                nameof(stream)
            ));
        }
        using var reader = new StreamReader(stream);
        var card = new AdaptiveCardTemplate(await reader.ReadToEndAsync(cancellationToken)).Expand(bookingProfile);
        // アダプティブ カードを送信する
        _ = await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(
            new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card
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
            var bookingBusinessId = bookingProfile.BusinessId;
            if (bookingBusinessId is null)
            {
                throw new InvalidOperationException(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                    nameof(bookingBusinessId)
                ));
            }
            // ダイアログで選択されたサービスIDを取得する
            var bookingServiceId = bookingProfile.ServiceId;
            if (bookingServiceId is null)
            {
                throw new InvalidOperationException(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                    nameof(bookingServiceId)
                ));
            }
            // ダイアログで入力された名前を取得する
            var bookingCustomerName = bookingProfile.CustomerName;
            // ダイアログで入力された電子メール アドレスを取得する
            var bookingCustomerEmail = bookingProfile.CustomerEmail;
            if (bookingCustomerEmail is null)
            {
                throw new InvalidOperationException(string.Format(
                    null,
                    CompositeFormat.Parse(StringResources.ErrorNotFoundMessage),
                    nameof(bookingCustomerEmail)
                ));
            }
            // ダイアログで選択されたスタッフを取得する
            var bookingStaffMemberId = bookingProfile.StaffMemberId;
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
                    new Graph.BookingAppointment()
                    {
                        CustomerName = bookingCustomerName,
                        CustomerEmailAddress = bookingCustomerEmail,
                        EndDateTime = new Graph.DateTimeTimeZone()
                        {
                            DateTime = bookingProfile.EndTime?.ToUniversalTime().ToString("s"),
                            TimeZone = "UTC",
                        },
                        ServiceId = bookingServiceId,
                        ServiceName = bookingProfile.ServiceName,
                        StartDateTime = new Graph.DateTimeTimeZone()
                        {
                            DateTime = bookingProfile.StartTime?.ToUniversalTime().ToString("s"),
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
