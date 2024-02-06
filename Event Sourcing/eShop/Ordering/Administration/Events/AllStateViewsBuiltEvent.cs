﻿using vesa.Core.Infrastructure;

namespace eShop.Ordering.Administration.Events;

public class AllStateViewsBuiltEvent : Event
{
    public AllStateViewsBuiltEvent(string triggeredBy, string idempotencyToken) : base(triggeredBy, idempotencyToken)
    {
    }
}
