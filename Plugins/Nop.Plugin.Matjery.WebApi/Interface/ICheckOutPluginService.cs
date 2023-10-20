﻿using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
   public interface ICheckOutPluginService
    {
        OrderSummaryResult GetOrderSummary();
        List<PaymentMethodResult> GetStandardPaymentMethods();
    }
}
