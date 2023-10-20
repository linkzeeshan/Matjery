using Nop.Core.Domain.Catalog;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface ICategoryPluginService
    {
        IList<CategoryResult> TopMenuCategory(IList<Category> categories);

    }
}
