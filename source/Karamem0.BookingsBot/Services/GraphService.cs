//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Resources;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Services;

public interface IGraphService
{

    Task<BookingAppointment> CreateBookingAppointmentAsync(string bookingBusinessId, BookingAppointment bookingAppointment, CancellationToken cancellationToken = default);

    Task<BookingBusiness> GetBookingBusinessAsync(string bookingBusinessId, CancellationToken cancellationToken = default);

    Task<IEnumerable<BookingBusiness>> GetBookingBusinessesAsync(CancellationToken cancellationToken = default);

    Task<BookingService> GetBookingServiceAsync(string bookingBusinessId, string bookingServiceId, CancellationToken cancellationToken = default);

    Task<IEnumerable<BookingService>> GetBookingServicesAsync(string bookingBusinessId, CancellationToken cancellationToken = default);

    Task<IEnumerable<BookingStaffMember>> GetBookingStaffMembersAsync(string bookingBusinessId, CancellationToken cancellationToken = default);

}

public class GraphService : IGraphService
{

    private readonly GraphServiceClient graphServiceClient;

    public GraphService(GraphServiceClient graphServiceClient)
    {
        this.graphServiceClient = graphServiceClient;
    }

    public async Task<BookingAppointment> CreateBookingAppointmentAsync(string bookingBusinessId, BookingAppointment bookingAppointment, CancellationToken cancellationToken = default)
    {
        var response = await this.graphServiceClient
            .Solutions
            .BookingBusinesses[bookingBusinessId]
            .Appointments
            .PostAsync(bookingAppointment, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response ?? throw new InvalidOperationException(StringResources.ErrorNoBookingAppointmentMessage);
    }

    public async Task<BookingBusiness> GetBookingBusinessAsync(string bookingBusinessId, CancellationToken cancellationToken = default)
    {
        var response = await this.graphServiceClient
            .Solutions
            .BookingBusinesses[bookingBusinessId]
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response ?? throw new InvalidOperationException(StringResources.ErrorNoBookingBusinessMessage);
    }

    public async Task<IEnumerable<BookingBusiness>> GetBookingBusinessesAsync(CancellationToken cancellationToken = default)
    {
        var response = await this.graphServiceClient
            .Solutions
            .BookingBusinesses
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response?.Value ?? throw new InvalidOperationException(StringResources.ErrorNoBookingBusinessMessage);
    }

    public async Task<BookingService> GetBookingServiceAsync(string bookingBusinessId, string bookingServiceId, CancellationToken cancellationToken = default)
    {
        var response = await this.graphServiceClient
            .Solutions
            .BookingBusinesses[bookingBusinessId]
            .Services[bookingServiceId]
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response ?? throw new InvalidOperationException(StringResources.ErrorNoBookingServiceMessage);
    }

    public async Task<IEnumerable<BookingService>> GetBookingServicesAsync(string bookingBusinessId, CancellationToken cancellationToken = default)
    {
        var response = await this.graphServiceClient
            .Solutions
            .BookingBusinesses[bookingBusinessId]
            .Services
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response?.Value ?? throw new InvalidOperationException(StringResources.ErrorNoBookingServiceMessage);
    }

    public async Task<IEnumerable<BookingStaffMember>> GetBookingStaffMembersAsync(string bookingBusinessId, CancellationToken cancellationToken = default)
    {
        var response = await this.graphServiceClient
            .Solutions
            .BookingBusinesses[bookingBusinessId]
            .StaffMembers
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response?.Value?.OfType<BookingStaffMember>() ?? throw new InvalidOperationException(StringResources.ErrorNoBookingStaffMemberMessage);
    }

}
