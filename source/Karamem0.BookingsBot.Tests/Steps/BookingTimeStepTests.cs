//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Models;
using Karamem0.BookingsBot.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps.Tests;

public class BookingTimeStepTests
{

    [Test()]
    public async Task BookingTimeStep_Succeeded()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var step = new BookingTimeStep(userState);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
                    stepContext.Values["BookingDateId"] = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    stepContext.Values["BookingAvailableTime"] = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    stepContext.Values["BookingBusinessHours"] = new Dictionary<DayOfWeek, TimeSpan[]>()
                    {
                        [DayOfWeek.Sunday] = [
                            new TimeSpan(9, 0, 0),
                            new TimeSpan(10, 0, 0),
                            new TimeSpan(11, 0, 0),
                            new TimeSpan(13, 0, 0),
                            new TimeSpan(14, 0, 0),
                            new TimeSpan(15, 0, 0),
                            new TimeSpan(16, 0, 0),
                            new TimeSpan(17, 0, 0)
                        ]
                    };
                    stepContext.Values["BookingDuration"] = "00:30:00";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                },
                step.OnBeforeAsync,
                step.OnAfterAsync
            ]
        ));
        _ = dialog.AddDialog(step.Dialog);
        var client = new DialogTestClient(Channels.Msteams, dialog);
        // Execute
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message));
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "10:00"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var content = actual.Attachments[0].Content as HeroCard;
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(content?.Text, Is.EqualTo(StringResources.ChooseBookingTimeMessage));
            Assert.That(profile.BookingStartTime, Is.EqualTo(new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(profile.BookingEndTime, Is.EqualTo(new DateTime(2023, 1, 1, 10, 30, 0, DateTimeKind.Utc)));
        });
    }

    [Test()]
    public async Task BookingTimeStep_Failed_EmptyValue()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var step = new BookingTimeStep(userState);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
                    stepContext.Values["BookingDateId"] = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    stepContext.Values["BookingAvailableTime"] = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    stepContext.Values["BookingBusinessHours"] = new Dictionary<DayOfWeek, TimeSpan[]>()
                    {
                        [DayOfWeek.Sunday] = [
                            new TimeSpan(9, 0, 0),
                            new TimeSpan(10, 0, 0),
                            new TimeSpan(11, 0, 0),
                            new TimeSpan(13, 0, 0),
                            new TimeSpan(14, 0, 0),
                            new TimeSpan(15, 0, 0),
                            new TimeSpan(16, 0, 0),
                            new TimeSpan(17, 0, 0)
                        ]
                    };
                    stepContext.Values["BookingDuration"] = "00:30:00";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                },
                step.OnBeforeAsync,
                step.OnAfterAsync
            ]
        ));
        _ = dialog.AddDialog(step.Dialog);
        var client = new DialogTestClient(Channels.Msteams, dialog);
        // Execute
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message));
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var content = actual.Attachments[0].Content as HeroCard;
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(content?.Text, Is.EqualTo(StringResources.RetryBookingTimeMessage));
            Assert.That(profile.BookingStartTime, Is.Null);
            Assert.That(profile.BookingEndTime, Is.Null);
        });
    }

    [Test()]
    public async Task BookingTimeStep_Failed_InvalidValue()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var step = new BookingTimeStep(userState);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
                    stepContext.Values["BookingDateId"] = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    stepContext.Values["BookingAvailableTime"] = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    stepContext.Values["BookingBusinessHours"] = new Dictionary<DayOfWeek, TimeSpan[]>()
                    {
                        [DayOfWeek.Sunday] = [
                            new TimeSpan(9, 0, 0),
                            new TimeSpan(10, 0, 0),
                            new TimeSpan(11, 0, 0),
                            new TimeSpan(13, 0, 0),
                            new TimeSpan(14, 0, 0),
                            new TimeSpan(15, 0, 0),
                            new TimeSpan(16, 0, 0),
                            new TimeSpan(17, 0, 0)
                        ]
                    };
                    stepContext.Values["BookingDuration"] = "00:30:00";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                },
                step.OnBeforeAsync,
                step.OnAfterAsync
            ]
        ));
        _ = dialog.AddDialog(step.Dialog);
        var client = new DialogTestClient(Channels.Msteams, dialog);
        // Execute
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message));
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "18:00"));
        // Assert
        var content = actual.Attachments[0].Content as HeroCard;
        Assert.That(content?.Text, Is.EqualTo(StringResources.RetryBookingTimeMessage));
    }

}
