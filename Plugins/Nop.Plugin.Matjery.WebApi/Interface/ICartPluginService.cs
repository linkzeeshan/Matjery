using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface ICartPluginService
    {
        IList<string> AddToCart(ParamsModel.CartParamsModel model);
        IList<string> UpdatCartQuantity(ParamsModel.CartParamsModel model);
        bool DeleteFromCart(ParamsModel.CartParamsModel model);
        List<CartResult> GetShoppingCart();
    }
}
