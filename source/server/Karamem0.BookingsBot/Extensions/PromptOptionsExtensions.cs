//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Microsoft.Agents.BotBuilder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Extensions;

public static class PromptOptionsExtensions
{

    public static T? GetValidation<T>(this PromptOptions target, string key)
    {
        if (target.Validations is JsonElement element)
        {
            var json = element.GetProperty(key).GetString();
            if (json is not null)
            {
                return JsonSerializer.Deserialize<T>(json);
            }
        }
        return default;
    }

}
