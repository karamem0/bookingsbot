//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Resources;
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

public class BookingDateStepTests
{

    [Test()]
    public async Task BookingDateStep_Succeeded()
    {
        // Setup
        var step = new BookingDateStep();
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
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
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "Janurary 1"));
        // Assert
        var content = actual.Attachments[0].Content as HeroCard;
        Assert.That(content?.Text, Is.EqualTo(StringResources.ChooseBookingDateMessage));
    }

    [Test()]
    public async Task BookingDateStep_Failed_EmptyValue()
    {
        // Setup
        var step = new BookingDateStep();
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
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
        var content = actual.Attachments[0].Content as HeroCard;
        Assert.That(content?.Text, Is.EqualTo(StringResources.RetryBookingDateMessage));
    }

    [Test()]
    public async Task BookingDateStep_Failed_InvalidValue()
    {
        // Setup
        var step = new BookingDateStep();
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
                    stepContext.Values["BookingAvailableTime"] = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    stepContext.Values["BookingBusinessHours"] = new Dictionary<DayOfWeek, TimeSpan[]>()
                    {
                        [DayOfWeek.Sunday] = []
                    };
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
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "Janurary 1"));
        // Assert
        var content = actual.Attachments[0].Content as HeroCard;
        Assert.That(content?.Text, Is.EqualTo(StringResources.RetryBookingDateMessage));
    }

}
