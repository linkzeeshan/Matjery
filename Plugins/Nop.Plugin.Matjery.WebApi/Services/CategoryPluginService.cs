using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Catalog;
using System.Collections.Generic;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class CategoryPluginService : BasePluginService,ICategoryPluginService
    {
        public IList<CategoryResult> TopMenuCategory(IList<Category> categories)
        {
            var topMenuCategories = new List<CategoryResult>();

            foreach (Category category in categories)
            {
                Picture picture = _pictureService.GetPictureById(category.PictureId) ?? new Picture(); ;
                int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                var webServiceCategory = new CategoryResult()
                {
                    Id = category.Id,
                    Name = _localizationService.GetLocalized(category, x => x.Name),
                    SeName = _urlRecordService.GetSeName(category), //seo friendly name can be use as a css class to menus
                    Description = _localizationService.GetLocalized(category, x => x.Description),
                    ParentCategoryId = category.ParentCategoryId,
                    Published = category.Published,
                    FullSizeImageUrl = _pictureService.GetPictureUrl(picture.Id, storeLocation: _storeContext.CurrentStore.Url),
                    ImageUrl = _pictureService.GetPictureUrl(picture.Id, pictureSize, storeLocation: _storeContext.CurrentStore.Url),
                    TradeLicenseRequired=category.TradeLicenseRequired
                };
                foreach (Language language in _languageService.GetAllLanguages())
                {
                    var webServiceCategorySub = new
                    {
                        Name = _localizationService.GetLocalized(category, x => x.Name, language.Id),
                        Description = _localizationService.GetLocalized(category, x => x.Description, language.Id),
                    };
                    webServiceCategory.CustomProperties.Add(language.UniqueSeoCode, webServiceCategorySub);
                }
                //nubmer of products in each category
                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopCatalogDefaults.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY, string.Join(",", _customerService.GetCustomerRoleIds(_workContext.CurrentCustomer)),
                        _storeContext.CurrentStore.Id,
                        category.Id);

                    webServiceCategory.NumberOfProducts = _staticCacheManager.Get(cacheKey, () =>
                    {
                        var categoryIds = new List<int>();
                        categoryIds.Add(category.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(GetChildCategoryIds(category.Id));
                        return _productService.GetNumberOfProductsInCategory(categoryIds, _storeContext.CurrentStore.Id);
                    });
                }
                topMenuCategories.Add(webServiceCategory);
            }
            return topMenuCategories;

        }

        private List<int> GetChildCategoryIds(int parentCategoryId)
        {
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopCatalogDefaults.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY,
                parentCategoryId, string.Join(",", _customerService.GetCustomerRoleIds(_workContext.CurrentCustomer)),
                _storeContext.CurrentStore.Id);
            return _staticCacheManager.Get(cacheKey, () =>
            {
                var categoriesIds = new List<int>();
                var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId);
                foreach (var category in categories)
                {
                    categoriesIds.Add(category.Id);
                    categoriesIds.AddRange(GetChildCategoryIds(category.Id));
                }
                return categoriesIds;
            });
        }



    }
}
