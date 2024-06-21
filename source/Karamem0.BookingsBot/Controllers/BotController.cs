//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Controllers;

[ApiController()]
[Route("api/messages")]
public class BotController : ControllerBase
{

    private readonly IBotFrameworkHttpAdapter adapter;

    private readonly IBot bot;

    public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
    {
        this.adapter = adapter;
        this.bot = bot;
    }

    [HttpGet()]
    [HttpPost()]
    public async Task PostAsync()
    {
        await this.adapter.ProcessAsync(this.Request, this.Response, this.bot);
    }

}
