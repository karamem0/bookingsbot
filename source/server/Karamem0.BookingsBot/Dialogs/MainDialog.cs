//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Karamem0.BookingsBot.Steps;
using Microsoft.Agents.BotBuilder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Dialogs;

public class MainDialog : ComponentDialog
{

    public MainDialog(MainStepCollection collection)
        : base(nameof(MainDialog))
    {
        _ = this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), collection.Actions));
        foreach (var dialog in collection.Dialogs)
        {
            _ = this.AddDialog(dialog);
        }
    }

}
