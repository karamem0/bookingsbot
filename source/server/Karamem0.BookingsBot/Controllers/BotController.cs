//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Protocols.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Controllers;

[ApiController()]
[Authorize(AuthenticationSchemes = "BotAuthentication")]
[Route("api/messages")]
public class BotController(IBotHttpAdapter adapter, IBot bot) : ControllerBase
{

    private readonly IBotHttpAdapter adapter = adapter;

    private readonly IBot bot = bot;

    [HttpGet()]
    [HttpPost()]
    public async Task PostAsync()
    {
        await this.adapter.ProcessAsync(this.Request, this.Response, this.bot);
    }

}
