//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

#pragma warning disable IDE0053

using Karamem0.BookingsBot.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Adapters;

public class AdapterWithErrorHandler : CloudAdapter
{

    public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<AdapterWithErrorHandler> logger)
        : base(auth, logger)
    {
        this.OnTurnError = async (turnContext, exception) =>
        {
            _ = await turnContext.SendActivityAsync(string.Format(
                null,
                CompositeFormat.Parse(StringResources.ErrorUnexpectedMessage),
                exception.Message
            ));
        };
    }

}

#pragma warning restore IDE0053
