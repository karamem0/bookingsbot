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
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Models;

public class BookingProfile
{

    public string? BookingBusinessId { get; set; }

    public string? BookingBusinessName { get; set; }

    public string? BookingStaffMemberId { get; set; }

    public string? BookingServiceId { get; set; }

    public string? BookingServiceName { get; set; }

    public DateTime? BookingStartTime { get; set; }

    public DateTime? BookingEndTime { get; set; }

    public string? BookingCustomerName { get; set; }

    public string? BookingCustomerEmail { get; set; }

}
