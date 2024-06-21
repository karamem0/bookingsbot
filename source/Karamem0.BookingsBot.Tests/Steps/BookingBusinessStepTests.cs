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

public class BookingBusinessStepTests
{

    [Test()]
    public async Task BookingBusinessStep_Succeeded()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var graphService = Substitute.For<IGraphService>();
        _ = graphService.GetBookingBusinessAsync("1")
            .Returns(new BookingBusiness()
            {
                Id = "1",
                DisplayName = "Business 1",
                BusinessHours = [
                    new()
                    {
                        Day = DayOfWeekObject.Sunday,
                        TimeSlots = [
                            new()
                            {
                                StartTime = new Time(9, 0, 0),
                                EndTime = new Time(18, 0, 0)
                            }
                        ]
                    },
                    new()
                    {
                        Day = DayOfWeekObject.Saturday,
                        TimeSlots = [
                            new()
                            {
                                StartTime = new Time(9, 0, 0),
                                EndTime = new Time(18, 0, 0)
                            }
                        ]
                    }
                ],
                SchedulingPolicy = new BookingSchedulingPolicy()
                {
                    MinimumLeadTime = TimeSpan.FromDays(1),
                    TimeSlotInterval = TimeSpan.FromMinutes(30)
                }
            });
        _ = graphService.GetBookingBusinessesAsync()
            .Returns([
                new()
                {
                    Id = "1",
                    DisplayName = "Business 1"
                },
                new()
                {
                    Id = "2",
                    DisplayName = "Business 2"
                },
                new()
                {
                    Id = "3",
                    DisplayName = "Business 3"
                }
            ]);
        var step = new BookingBusinessStep(userState, graphService);
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
        _ = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "Business 1"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var content = actual.Attachments[0].Content as HeroCard;
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(content?.Text, Is.EqualTo(StringResources.ChooseBookingBusinessMessage));
            Assert.That(profile.BookingBusinessId, Is.EqualTo("1"));
            Assert.That(profile.BookingBusinessName, Is.EqualTo("Business 1"));
        });
    }

    [Test()]
    public async Task BookingBusinessStep_Failed_InvalidValue()
    {
        // Setup
        var userState = new UserState(new MemoryStorage());
        var graphService = Substitute.For<IGraphService>();
        _ = graphService.GetBookingBusinessAsync("1").Returns(new BookingBusiness()
        {
            Id = "1",
            DisplayName = "Business 1",
            BusinessHours = [
                new()
                {
                    Day = DayOfWeekObject.Sunday,
                    TimeSlots = [
                        new()
                        {
                            StartTime = new Time(9, 0, 0),
                            EndTime = new Time(18, 0, 0)
                        }
                    ]
                },
                new()
                {
                    Day = DayOfWeekObject.Saturday,
                    TimeSlots = [
                        new()
                        {
                            StartTime = new Time(9, 0, 0),
                            EndTime = new Time(18, 0, 0)
                        }
                    ]
                }
            ],
            SchedulingPolicy = new BookingSchedulingPolicy()
            {
                MinimumLeadTime = TimeSpan.FromDays(1),
                TimeSlotInterval = TimeSpan.FromMinutes(30)
            }
        });
        _ = graphService.GetBookingBusinessesAsync().Returns([
            new()
            {
                Id = "1",
                DisplayName = "Business 1"
            },
            new()
            {
                Id = "2",
                DisplayName = "Business 2"
            },
            new()
            {
                Id = "3",
                DisplayName = "Business 3"
            }
        ]);
        var step = new BookingBusinessStep(userState, graphService);
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
        var actual = await client.SendActivityAsync<IMessageActivity>(new Activity(ActivityTypes.Message, text: "Business 4"));
        // Assert
        await Assert.MultipleAsync(async () =>
        {
            var content = actual.Attachments[0].Content as HeroCard;
            var accessor = userState.CreateProperty<BookingProfile>(nameof(BookingProfile));
            var profile = await accessor.GetAsync(client.DialogContext.Context, () => new BookingProfile());
            Assert.That(content?.Text, Is.EqualTo(StringResources.RetryBookingBusinessMessage));
            Assert.That(profile.BookingBusinessId, Is.Null);
            Assert.That(profile.BookingBusinessName, Is.Null);
        });
    }

}
