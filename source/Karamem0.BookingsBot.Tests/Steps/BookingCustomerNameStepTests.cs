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

public class BookingCustomerNameStepTests
{

    [Test()]
    public async Task BookingCustomerNameStep_Succeeded()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var step = new BookingCustomerNameStep(userState);
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
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "Hikaru Hoshi"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(actual.Text, Is.EqualTo(StringResources.EnterBookingCustomerNameMessage));
            Assert.That(profile.BookingCustomerName, Is.EqualTo("Hikaru Hoshi"));
        });
    }

    [Test()]
    public async Task BookingCustomerNameStep_Failed_EmptyValue()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var step = new BookingCustomerNameStep(userState);
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
            Assert.That(actual.Text, Is.EqualTo(StringResources.RetryBookingCustomerNameMessage));
            Assert.That(profile.BookingCustomerName, Is.Null);
        });
    }

}
