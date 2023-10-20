using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface IWishListPluginService
    {
        List<WishlistResult> GetWishlist();
        IList<string> AddToWhishlist(ParamsModel.WishlistParams paramsModel);
        bool DeleteFromWishlist(ParamsModel.WishlistParams paramsModel);
    }
}
