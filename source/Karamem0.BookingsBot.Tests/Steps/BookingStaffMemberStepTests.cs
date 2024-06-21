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
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps.Tests;

public class BookingStaffMemberStepTests
{

    [Test()]
    public async Task BookingStaffMemberStep_Succeeded()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var graphService = Substitute.For<IGraphService>();
        _ = graphService.GetBookingStaffMembersAsync("1")
            .Returns([
                new()
                {
                    Id = "1",
                    DisplayName = "Staff 1"
                },
                new()
                {
                    Id = "2",
                    DisplayName = "Staff 2"
                },
                new()
                {
                    Id = "3",
                    DisplayName = "Staff 3"
                }
            ]);
        var step = new BookingStaffMemberStep(userState, graphService);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
                    var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
                    var profile = await accessor.GetAsync(stepContext.Context, () => new BookingProfile(), cancellationToken);
                    profile.BookingBusinessId = "1";
                    await userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
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
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "Staff 1"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var content = actual.Attachments[0].Content as HeroCard;
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(content?.Text, Is.EqualTo(StringResources.ChooseBookingStaffMemberMessage));
            Assert.That(profile.BookingStaffMemberId, Is.EqualTo("1"));
        });
    }

    [Test()]
    public async Task BookingStaffMemberStep_Failed_InvalidValue()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var graphService = Substitute.For<IGraphService>();
        _ = graphService.GetBookingStaffMembersAsync("1")
            .Returns([
                new()
                {
                    Id = "1",
                    DisplayName = "Staff 1"
                },
                new()
                {
                    Id = "2",
                    DisplayName = "Staff 2"
                },
                new()
                {
                    Id = "3",
                    DisplayName = "Staff 3"
                }
            ]);
        var step = new BookingStaffMemberStep(userState, graphService);
        var dialog = new ComponentDialog();
        _ = dialog.AddDialog(new WaterfallDialog(
            nameof(WaterfallDialog),
            [
                async (stepContext, cancellationToken) =>
                {
                    var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
                    var profile = await accessor.GetAsync(stepContext.Context, () => new BookingProfile(), cancellationToken);
                    profile.BookingBusinessId = "1";
                    await userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
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
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "Staff 4"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var content = actual.Attachments[0].Content as HeroCard;
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(content?.Text, Is.EqualTo(StringResources.RetryBookingStaffMemberMessage));
            Assert.That(profile.BookingStaffMemberId, Is.Null);
        });
    }

}
