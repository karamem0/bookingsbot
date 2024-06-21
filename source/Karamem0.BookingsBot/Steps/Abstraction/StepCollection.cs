//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karamem0.BookingsBot.Steps.Abstraction;

public abstract class StepCollection : IEnumerable<Step>
{

    private readonly Step[] collection;

    protected StepCollection(params Step[] collection)
    {
        this.collection = collection;
    }

    public IEnumerable<WaterfallStep> Actions
    {
        get
        {
            foreach (var provider in this.collection)
            {
                yield return provider.OnBeforeAsync;
                yield return provider.OnAfterAsync;
            }
        }
    }

    public IEnumerable<Dialog> Dialogs => this.collection.Select(item => item.Dialog);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public IEnumerator<Step> GetEnumerator()
    {
        return this.collection.AsEnumerable().GetEnumerator();
    }

}
