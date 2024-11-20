//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Controllers;

[ApiController()]
[Authorize(AuthenticationSchemes = "ApiAuthencation")]
[Route("api/token")]
public class TokenController(IHttpClientFactory httpClientFactory) : ControllerBase
{

    private readonly HttpClient httpClient = httpClientFactory.CreateClient("DirectLineToken");

    [HttpPost()]
    public async Task<IActionResult> PostAsync()
    {
        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
        };
        var responseMessage = await this.httpClient.SendAsync(requestMessage);
        var responseContent = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>();
        return this.Ok(responseContent);
    }

}
