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

public static class WaterfallStepContextExtensions
{

    public static void SetValue<T>(this WaterfallStepContext target, string key, T? value)
    {
        target.Values[key] = JsonSerializer.Serialize(value);
    }

    public static T? GetValue<T>(this WaterfallStepContext target, string key)
    {
        if (target.Values.TryGetValue(key, out var value))
        {
            if (value is string jsonStr)
            {
                return JsonSerializer.Deserialize<T>(jsonStr);
            }
            else
            if (value is JsonElement element)
            {
                var jsonObj = element.GetString();
                if (jsonObj is not null)
                {
                    return JsonSerializer.Deserialize<T>(jsonObj);
                }
            }
        }
        return default;
    }

}
