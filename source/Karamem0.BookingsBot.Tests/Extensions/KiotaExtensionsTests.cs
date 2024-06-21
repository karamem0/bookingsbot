//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Microsoft.Kiota.Abstractions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Extensions.Tests;

public class KiotaExtensionsTests
{

    [Test()]
    public void KiotaExtensions_ToTimeSpan()
    {
        // Setup
        var value = new Time(1, 30, 15);
        var actual = new TimeSpan(1, 30, 15);
        // Execute
        var expected = KiotaExtensions.ToTimeSpan(value);
        // Assert
        Assert.That(expected, Is.EqualTo(actual));
    }

}
