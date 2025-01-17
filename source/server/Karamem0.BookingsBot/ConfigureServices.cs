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
using Microsoft.Agents.Authentication;
using Microsoft.Agents.Authentication.Msal;
using Microsoft.Agents.BotBuilder;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Memory.Blobs;
using Microsoft.Agents.Protocols.Connector;
using Microsoft.Agents.Protocols.Primitives;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot;

public static class ConfigureServices
{

    private static readonly DefaultAzureCredential defaultAzureCredential = new(new DefaultAzureCredentialOptions()
    {
        ExcludeVisualStudioCodeCredential = true
    });

    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "AzureAD",
        string jwtSchemaName = "ApiAuthencation"
    )
    {
        _ = services.AddMicrosoftIdentityWebApiAuthentication(configuration, configSectionName, jwtSchemaName);
        return services;
    }

    public static IServiceCollection AddBot(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "AzureBot"
    )
    {
        var section = configuration.GetSection(configSectionName) ?? throw new InvalidOperationException();
        var blobContainerUrl = section["StorageUrl"] ?? throw new InvalidOperationException();
        services.AddCloudAdapter<AdapterWithErrorHandler>();
        _ = services.AddSingleton<IConnections, ConfigurationConnections>();
        _ = services.AddSingleton<IChannelServiceClientFactory, RestChannelServiceClientFactory>();
        _ = services.AddSingleton<IStorage>(new BlobsStorage(
            new Uri(blobContainerUrl),
            defaultAzureCredential,
            new StorageTransferOptions()
        ));
        _ = services.AddSingleton<ConversationState>();
        _ = services.AddSingleton<UserState>();
        _ = services.AddTransient<IBot, DialogBot<MainDialog>>();
        return services;
    }

    public static IServiceCollection AddBotAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "Connections:BotServiceConnection:Settings",
        string jwtSchemaName = "BotAuthentication"
    )
    {
        var section = configuration.GetSection(configSectionName) ?? throw new InvalidOperationException();
        var clientId = section["ClientId"] ?? throw new InvalidOperationException();
        var tenantId = section["TenantId"] ?? throw new InvalidOperationException();
        _ = services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtSchemaName, options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    ValidIssuers = [
                        "https://api.botframework.com",
                        "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/",
                        "https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0",
                        "https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/",
                        "https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0",
                        $"https://sts.windows.net/{tenantId}/",
                        $"https://login.microsoftonline.com/{tenantId}/v2.0",
                    ],
                    ValidAudience = clientId,
                    RequireSignedTokens = true,
                    SignatureValidator = (token, parameters) => new JsonWebToken(token),
                };
            });
        services.AddDefaultMsalAuth(configuration);
        return services;
    }

    public static IServiceCollection AddDialogs(this IServiceCollection services)
    {
        _ = services.AddScoped<MainDialog>();
        _ = services.AddScoped<BookingDialog>();
        return services;
    }

    public static IServiceCollection AddDirectLineTokenClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "AzureBot"
    )
    {
        var section = configuration.GetSection(configSectionName) ?? throw new InvalidOperationException();
        _ = services.AddHttpClient("DirectLineToken", (httpClient) =>
        {
            var directLineTokenEndpoint = section["DirectLineTokenUrl"] ?? throw new InvalidOperationException();
            var directLineTokenSecret = section["DirectLineTokenSecret"] ?? throw new InvalidOperationException();
            httpClient.BaseAddress = new Uri(directLineTokenEndpoint);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                JwtBearerDefaults.AuthenticationScheme,
                directLineTokenSecret
            );
        });
        return services;
    }

    public static IServiceCollection AddMicrosoftGraph(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "MicrosoftGraph"
    )
    {
        var section = configuration.GetSection(configSectionName) ?? throw new InvalidOperationException();
        var tenantId = section["TenantId"] ?? throw new InvalidOperationException();
        var clientId = section["ClientId"] ?? throw new InvalidOperationException();
        var clientSecret = section["ClientSecret"] ?? throw new InvalidOperationException();
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _ = services.AddSingleton(provider => new GraphServiceClient(credential));
        _ = services.AddSingleton<IGraphService, GraphService>();
        return services;
    }

    public static IServiceCollection AddSteps(this IServiceCollection services)
    {
        // Main Steps
        _ = services.AddScoped<MainStep>();
        _ = services.AddScoped(provider => new MainStepCollection(
            provider.GetRequiredService<MainStep>()
        ));
        // Booking Steps
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

}
