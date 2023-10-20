using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public interface ISearchPluginService
    {
        object GetSearchProduct();
        object SearchProduct(ParamsModel.SearchParamsModel model);
    }
}
