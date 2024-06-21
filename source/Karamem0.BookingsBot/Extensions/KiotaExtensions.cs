//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Microsoft.Kiota.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Extensions;

public static class KiotaExtensions
{

    public static TimeSpan ToTimeSpan(this Time value)
    {
        return value.DateTime.TimeOfDay;
    }

}
