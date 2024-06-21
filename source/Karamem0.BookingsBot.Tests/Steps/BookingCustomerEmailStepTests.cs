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

public class BookingCustomerEmailStepTests
{

    [Test()]
    public async Task BookingCustomerEmailStep_Succeeded()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var step = new BookingCustomerEmailStep(userState);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                step.OnBeforeAsync,
                step.OnAfterAsync
            ]
        ));
        _ = dialog.AddDialog(step.Dialog);
        var client = new DialogTestClient(Channels.Msteams, dialog);
        // Execute
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message));
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "someone@example.com"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(actual.Text, Is.EqualTo(StringResources.EnterBookingCustomerEmailMessage));
            Assert.That(profile.BookingCustomerEmail, Is.EqualTo("someone@example.com"));
        });
    }

    [Test()]
    public async Task BookingCustomerEmailStep_Failed_EmptyValue()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var step = new BookingCustomerEmailStep(userState);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
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
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(actual.Text, Is.EqualTo(StringResources.RetryBookingCustomerEmailMessage));
            Assert.That(profile.BookingCustomerEmail, Is.Null);
        });
    }

    [Test()]
    public async Task BookingCustomerEmailStep_Failed_InvalidValue()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
        var step = new BookingCustomerEmailStep(userState);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                step.OnBeforeAsync,
                step.OnAfterAsync
            ]
        ));
        _ = dialog.AddDialog(step.Dialog);
        var client = new DialogTestClient(Channels.Msteams, dialog);
        // Execute
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message));
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "someone"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(actual.Text, Is.EqualTo(StringResources.RetryBookingCustomerEmailMessage));
            Assert.That(profile.BookingCustomerEmail, Is.Null);
        });
    }

}
