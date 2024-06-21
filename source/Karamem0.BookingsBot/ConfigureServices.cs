//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Azure.Identity;
using Azure.Storage;
using Karamem0.BookingsBot.Adapters;
using Karamem0.BookingsBot.Bots;
using Karamem0.BookingsBot.Dialogs;
using Karamem0.BookingsBot.Services;
using Karamem0.BookingsBot.Steps;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot;

public static class ConfigureServices
{

    public static readonly DefaultAzureCredential DefaultAzureCredential = new(new DefaultAzureCredentialOptions()
    {
        ExcludeVisualStudioCodeCredential = true
    });

    public static IServiceCollection AddBots(this IServiceCollection services, IConfiguration configuration)
    {
        var blobContainerUrl = configuration.GetValue<string>("AzureBotStatesStorageUrl") ?? throw new InvalidOperationException();
        _ = services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
        _ = services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
        _ = services.AddSingleton<IStorage>(new BlobsStorage(
            new Uri(blobContainerUrl),
            DefaultAzureCredential,
            new StorageTransferOptions()
        ));
        _ = services.AddSingleton<ConversationState>();
        _ = services.AddSingleton<UserState>();
        _ = services.AddScoped<IBot, DialogBot>();
        return services;
    }

    public static IServiceCollection AddDialogs(this IServiceCollection services)
    {
        _ = services.AddScoped<BookingDialog>();
        return services;
    }

    public static IServiceCollection AddSteps(this IServiceCollection services)
    {
        _ = services.AddScoped<BookingBusinessStep>();
        _ = services.AddScoped<BookingServiceStep>();
        _ = services.AddScoped<BookingDateStep>();
        _ = services.AddScoped<BookingTimeStep>();
        _ = services.AddScoped<BookingStaffMemberStep>();
        _ = services.AddScoped<BookingCustomerNameStep>();
        _ = services.AddScoped<BookingCustomerEmailStep>();
        _ = services.AddScoped<BookingConfirmStep>();
        _ = services.AddScoped(provider => new BookingStepCollection(
            provider.GetRequiredService<BookingBusinessStep>(),
            provider.GetRequiredService<BookingServiceStep>(),
            provider.GetRequiredService<BookingDateStep>(),
            provider.GetRequiredService<BookingTimeStep>(),
            provider.GetRequiredService<BookingStaffMemberStep>(),
            provider.GetRequiredService<BookingCustomerNameStep>(),
            provider.GetRequiredService<BookingCustomerEmailStep>(),
            provider.GetRequiredService<BookingConfirmStep>()
        ));
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddSingleton(provider =>
        {
            var tenantId = configuration.GetValue<string>("MicrosoftGraph:TenantId") ?? throw new InvalidOperationException();
            var clientId = configuration.GetValue<string>("MicrosoftGraph:ClientId") ?? throw new InvalidOperationException();
            var clientSecret = configuration.GetValue<string>("MicrosoftGraph:ClientSecret") ?? throw new InvalidOperationException();
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            return new GraphServiceClient(credential);
        });
        _ = services.AddSingleton<IGraphService, GraphService>();
        return services;
    }

}
