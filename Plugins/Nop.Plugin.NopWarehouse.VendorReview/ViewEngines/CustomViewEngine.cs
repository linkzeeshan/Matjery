using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.ViewEngines
{
    public class CustomViewEngine : IViewLocationExpander
    {

        private const string THEME_KEY = "nop.urban";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {

            if (context.AreaName == AreaNames.Admin)
            {
                viewLocations = new string[] {
                    $"~/Plugins/NopWarehouse.VendorReview/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"~/Plugins/NopWarehouse.VendorReview/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                //viewLocations = new string[] {
                //    $"~/Plugins/YOUR_PLUGIN_SYSTEM_NAME/Views/Shared/{{0}}.cshtml",
                //    $"~/Plugins/YOUR_PLUGIN_SYSTEM_NAME/Views/{{1}}/{{0}}.cshtml"
                //}.Concat(viewLocations);

                //if (context.Values.TryGetValue(THEME_KEY, out string theme))
                //{
                //    viewLocations = new string[] {
                //        $"~/Plugins/YOUR_PLUGIN_SYSTEM_NAME/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                //        $"~/Plugins/YOUR_PLUGIN_SYSTEM_NAME/Themes/{theme}/Views/{{1}}/{{0}}.cshtml"
                //    }.Concat(viewLocations);
                //}
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
