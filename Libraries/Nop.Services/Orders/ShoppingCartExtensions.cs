using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Orders
{
    public static class ShoppingCartExtensions
    {
        public static IEnumerable<ShoppingCartItem> LimitPerStore(this IEnumerable<ShoppingCartItem> cart, int storeId)
        {
            //simply replace the following code with "return cart"
            //if you want to share shopping carts between stores

            return cart.Where(x => x.StoreId == storeId);
        }
    }
}
