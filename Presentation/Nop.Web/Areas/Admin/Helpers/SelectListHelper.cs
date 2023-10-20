using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Infrastructure.Cache;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Caching;
using Nop.Services.Localization;

namespace Nop.Web.Areas.Admin.Helpers
{
    public static class SelectListHelper
    {
        /// <summary>
        /// Get category list
        /// </summary>
        /// <param name = "categoryService" > Category service</param>
        /// <param name = "cacheManager" > Cache manager</param>
        /// <param name = "showHidden" > A value indicating whether to show hidden records</param>
        /// <returns>Category list</returns>
        public static List<SelectListItem> GetCategoryList(ILocalizationService localizationService, ICategoryService categoryService, ICacheKeyService CacheKeyService ,IStaticCacheManager cacheManager, bool showHidden = false, int landuageid = 0)
        {
            //string cacheKey = string.Format(NopModelCacheDefaults.CategoriesListKey, showHidden);
            var cacheKey = CacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoriesListKey, showHidden);
            if (landuageid > 0)
            {
                cacheManager.Remove(NopModelCacheDefaults.CategoriesListKey);
            }
            var categoryListItems = cacheManager.Get(cacheKey, () =>
            {
                var categories = categoryService.GetAllCategories(showHidden: showHidden);
                //var categories = categoryService.GetAllCategories(categoryName: "",
                //                 showHidden: true);
                return categories.Select(c => new SelectListItem
                {
                    //localizationService.GetLocalized(c,x=>x.Name), //
                    Text = categoryService.GetFormattedBreadCrumb(c,categories, languageId: landuageid),
                    Value = c.Id.ToString()
                });
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in categoryListItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }
    }
}
