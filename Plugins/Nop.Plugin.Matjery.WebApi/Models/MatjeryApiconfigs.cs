using System;
using System.Collections.Generic;
using System.Text;
using static Nop.Plugin.Matjery.WebApi.Models.ParamsModel;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class MatjeryApiconfigs
    {
        public MatjeryApiconfigs()
        {
            OrderStatusModel = new List<OrderStatusModel>();
        }
        public List<OrderStatusModel> OrderStatusModel { get; set; }
        public int ResendOTPTimer { get; set; }
    }
}
