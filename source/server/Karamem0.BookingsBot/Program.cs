//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;
_ = services.AddControllers();
_ = services.AddApplicationInsightsTelemetry();
_ = services.AddBot(configuration);
_ = services.AddApiAuthentication(configuration);
_ = services.AddBotAuthentication(configuration);
_ = services.AddDialogs();
_ = services.AddSteps();
_ = services.AddDirectLineTokenClient(configuration);
_ = services.AddMicrosoftGraph(configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
}
else
{
    _ = app.UseWebSockets();
}
_ = app.UseDefaultFiles();
_ = app.UseStaticFiles();
_ = app.UseRouting();
_ = app.UseAuthorization();
_ = app.MapControllers();

await app.RunAsync();
