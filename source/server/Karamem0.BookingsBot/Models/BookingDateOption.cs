//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Models;

public class BookingDateOption
{

    [JsonPropertyName("id")]
    public DateTime? Id { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

}
