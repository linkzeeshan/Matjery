using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using static Nop.Plugin.Matjery.WebApi.Models.ParamsModel;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class SearchPluginService : BasePluginService, ISearchPluginService
    {
        #region initialization
        private readonly ICampaignService _campaignService;
        private readonly VendorSettings _vendorSettings;
        private readonly ISearchTermService _searchTermService;
        #endregion
        public SearchPluginService()
        {
            _campaignService = EngineContext.Current.Resolve<ICampaignService>();
            _vendorSettings = EngineContext.Current.Resolve<VendorSettings>();
            _searchTermService = EngineContext.Current.Resolve<ISearchTermService>();
        }

        public object GetSearchProduct()
        {
            
                var model = new
                {
                    AllowSearchByVendor = _vendorSettings.AllowSearchByVendor,
                    AvailableCategories = new List<object>(),
                    AvailableVendors = new List<object>(),
                };
                var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopCatalogDefaults.SearchCategoryCacheKey,
                _workContext.WorkingLanguage.Id,
               _customerService.GetCustomerRoleIds(_workContext.CurrentCustomer),
                 _storeContext.CurrentStore.Id);

                var categories = _staticCacheManager.Get(cacheKey, () =>
                {
                    var categoriesModel = new List<dynamic>();
                    //all categories
                    var allCategories = _categoryService.GetAllCategories(storeId: _storeContext.CurrentStore.Id);
                    foreach (var c in allCategories)
                    {
                        //generate full category name (breadcrumb)
                        string categoryBreadcrumb = "";
                        var breadcrumb = GetCategoryBreadCrumb(c, allCategories, _aclService, _storeMappingService);
                        for (int i = 0; i <= breadcrumb.Count - 1; i++)
                        {
                            categoryBreadcrumb += _localizationService.GetLocalized(breadcrumb[i], x => x.Name); //breadcrumb[i].GetLocalized(x => x.Name);
                            if (i != breadcrumb.Count - 1)
                                categoryBreadcrumb += " >> ";
                        }
                        categoriesModel.Add(new
                        {
                            Id = c.Id,
                            Breadcrumb = categoryBreadcrumb
                        });
                    }
                    return categoriesModel;
                });
                if (categories.Any())
                {
                    //first empty entry
                    model.AvailableCategories.Add(new
                    {
                        Value = "0",
                        Text = _localizationService.GetResource("Search.Category")
                    });
                    //all other categories
                    foreach (var c in categories)
                    {
                        model.AvailableCategories.Add(new
                        {
                            Value = c.Id.ToString(),
                            Text = c.Breadcrumb
                        });
                    }
                }

                if (model.AllowSearchByVendor)
                {
                    var vendors = _vendorService.GetAllVendors(showHidden: false);
                    if (vendors.Any())
                    {
                        model.AvailableVendors.Add(new
                        {
                            Value = "0",
                            Text = _localizationService.GetResource("Search.Vendor")
                        });
                        foreach (var vendor in vendors)
                            model.AvailableVendors.Add(new
                            {
                                Value = vendor.Id.ToString(),
                                Text = _localizationService.GetLocalized(vendor, x=>x.Name)
                            });
                    }
                }

            return model;


        }

        public object SearchProduct(ParamsModel.SearchParamsModel model)
        {
                if (model.VendorId > 0)
                {
                    Vendor objvendor = _vendorService.GetVendorById(Convert.ToInt32(model.VendorId));
                if (!objvendor.Active)
                    {
                        return null;
                    }
                }
                if (model.SearchTerms == null)
                    model.SearchTerms = "";

                model.PageIndex = model.PageIndex <= 0 ? 1 : model.PageIndex;

                IPagedList<Product> products = null;
                if (model.FeaturedProducts != null && model.FeaturedProducts.Value)
                {
                    products = _productService.GetAllProductsDisplayedOnHomePage();
                }
                else if (model.BestSellers != null && model.BestSellers.Value)
                {
                    products = (IPagedList<Product>)this.HomepageBestSellers();
                }
                else if (model.DiscountedProducts != null && model.DiscountedProducts.Value)
                {
                    products = _productService.GetAllProductsDiscounted(model.PageIndex, model.PageSize);
                }
                else if (model.CampaignId != null && model.CampaignId.HasValue)
                {
                  
                    products = this.CampaignProducts(model.CampaignId.Value, model.PageIndex, model.PageSize);
                }
                else
                {
                model.SearchTerms = model.SearchTerms.Trim();
                if (model.SearchTerms.Length < this._catalogSettings.ProductSearchTermMinimumLength && model.ValdiateLength)
                {
                    var emptyResult = new List<ProductResult>();
                    object EmptyviewModeModel = PrepareViewModes(model.ViewMode);
                    object EmptysortingOptions = PrepareSortingOptions(model.OrderBy);
                    var Emptymodel = new
                    {
                        Products = emptyResult,
                        TotalProducts = 0,
                        ViewMode = EmptyviewModeModel,
                        SortingOptions = EmptysortingOptions
                    };
                    return Emptymodel;
                }
                if (model.CategoryIds==null &&model.VendorId<=0)
                {
                    var emptyResult = new List<ProductResult>();
                    object EmptyviewModeModel = PrepareViewModes(model.ViewMode);
                    object EmptysortingOptions = PrepareSortingOptions(model.OrderBy);
                    var Emptymodel = new
                    {
                        Products = emptyResult,
                        TotalProducts = 0,
                        ViewMode = EmptyviewModeModel,
                        SortingOptions = EmptysortingOptions
                    };
                    return Emptymodel;
                }
               

                List<int> categoryIds = new List<int>();
                    decimal? minPriceConverted = null;
                    decimal? maxPriceConverted = null;
                    if (model.AdvancedSearch)
                    {
                        if (model.CategoryIds != null)
                        {
                            foreach (int? categoryId in model.CategoryIds)
                            {
                                if (categoryId.HasValue && categoryId.Value > 0)
                                {
                                    categoryIds.Add(categoryId.Value);
                                    if (model.IncludeSubCategories)
                                        categoryIds.AddRange(this.GetChildCategoryIds(categoryId.Value));
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(model.PriceFrom))
                        {
                            decimal minPrice;
                            if (decimal.TryParse(model.PriceFrom, out minPrice))
                            {
                                minPriceConverted = new decimal?(this._currencyService.ConvertToPrimaryStoreCurrency(minPrice, this._workContext.WorkingCurrency));
                            }
                        }
                        if (!string.IsNullOrEmpty(model.PriceTo))
                        {
                            decimal maxPrice;
                            if (decimal.TryParse(model.PriceTo, out maxPrice))
                            {
                                maxPriceConverted = new decimal?(this._currencyService.ConvertToPrimaryStoreCurrency(maxPrice, this._workContext.WorkingCurrency));
                            }
                        }
                    }
                    bool searchInProductTags = model.SearchInDescriptions;
                    ProductSortingEnum orderBy = ProductSortingEnum.CreatedOn;
                    if (model.OrderBy.HasValue)
                        orderBy = (ProductSortingEnum)model.OrderBy.Value;

                    //products = this._productService.SearchProducts(pageIndex: model.PageIndex - 1, pageSize: model.PageSize, categoryIds: categoryIds,
                    //     manufacturerId: model.ManufacturerId, storeId: _storeContext.CurrentStore.Id, vendorId: model.VendorId, warehouseId: 0, productType: null,
                    //     visibleIndividuallyOnly: true, markedAsNewOnly: false, featuredProducts: model.FeaturedProducts, priceMin: minPriceConverted, priceMax: maxPriceConverted,
                    //     productTagId: 0, keywords: model.SearchTerms, searchDescriptions: model.SearchInDescriptions, searchManufacturerPartNumber: true,
                    //     languageId: _workContext.WorkingLanguage.Id,
                    //     searchSku: model.SearchInDescriptions, searchProductTags: searchInProductTags, orderBy: orderBy);

                    products = this._productService.SearchProducts(keywords: model.SearchTerms,languageId:_workContext.WorkingLanguage.Id, categoryIds: categoryIds, vendorId: model.VendorId!=null?Convert.ToInt32(model.VendorId):0,
                        markedAsNewOnly: false, featuredProducts: model.FeaturedProducts, priceMin: minPriceConverted, priceMax: maxPriceConverted,
                        searchDescriptions: model.SearchInDescriptions, searchSku: model.SearchInDescriptions, searchProductTags: searchInProductTags, orderBy: orderBy,
                        pageIndex: model.PageIndex - 1, pageSize: model.PageSize);


                    if (!string.IsNullOrEmpty(model.SearchTerms))
                    {
                        SearchTerm searchTerm = this._searchTermService.GetSearchTermByKeyword(model.SearchTerms, this._storeContext.CurrentStore.Id);
                        if (searchTerm == null)
                        {
                            searchTerm = new SearchTerm();
                            searchTerm.Keyword = model.SearchTerms;
                            searchTerm.StoreId = this._storeContext.CurrentStore.Id;
                            searchTerm.Count = 1;
                            this._searchTermService.InsertSearchTerm(searchTerm);
                        }
                        else
                        {
                            searchTerm.Count = searchTerm.Count + 1;
                            this._searchTermService.UpdateSearchTerm(searchTerm);
                        }
                        var productSearchEvent = new ProductSearchEvent
                        {
                            SearchTerm = model.SearchTerms,
                            SearchInDescriptions = model.SearchInDescriptions,
                            CategoryIds = categoryIds,
                            ManufacturerId = model.ManufacturerId!=null? Convert.ToInt32(model.ManufacturerId):0,
                            WorkingLanguageId = this._workContext.WorkingLanguage.Id
                        };
                        _eventPublisher.Publish<ProductSearchEvent>(productSearchEvent);
                    }
                }
                var productsResult = new List<ProductResult>();
                //int numberOfProductsWihtoutImages = 0;
                foreach (Product product in products)
                {
                    ProductResult productResult = this.PrepareProductOverview(product);


                foreach (Language language in _languageService.GetAllLanguages())
                {
                    productResult.CustomProperties.Add(language.UniqueSeoCode, new
                    {
                        Name = _localizationService.GetLocalized(product, x => x.Name, language.Id),
                        ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription, language.Id),
                        FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription, language.Id)
                    });
                }
                //if (string.IsNullOrEmpty(productResult.ImageUrl))
                //{
                //    //decrease total count products also otherwise mobile app will keep loading as totalproducts will be more than the returned products
                //    numberOfProductsWihtoutImages++;
                //    continue;
                //}


                PrepareProductPrice(productResult, product);

                    productsResult.Add(productResult);
                }

                object viewModeModel = PrepareViewModes(model.ViewMode);
                object sortingOptions = PrepareSortingOptions(model.OrderBy);
                var resultModel = new
                {
                    Products = productsResult,
                    TotalProducts = products.TotalCount,
                    ViewMode = viewModeModel,
                    SortingOptions = sortingOptions
                };
                return resultModel;
        }


        private IList<Category> GetCategoryBreadCrumb(Category category,
            IList<Category> allCategories,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            var result = new List<Category>();

            //used to prevent circular references
            var alreadyProcessedCategoryIds = new List<int>();

            while (category != null && //not null
                !category.Deleted && //not deleted
                (showHidden || category.Published) && //published
                (showHidden || aclService.Authorize(category)) && //ACL
                (showHidden || storeMappingService.Authorize(category)) && //Store mapping
                !alreadyProcessedCategoryIds.Contains(category.Id)) //prevent circular references
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = (from c in allCategories
                            where c.Id == category.ParentCategoryId
                            select c).FirstOrDefault();
            }
            result.Reverse();
            return result;
        }
        private IList<Product> HomepageBestSellers()
        {

            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopCatalogDefaults.Homepage_Bestsellers_Ids_Key, _storeContext.CurrentStore.Id);

            //load and cache report
            var report =
                _staticCacheManager.Get(cacheKey,
                    () => _orderReportService.BestSellersReport(storeId: _storeContext.CurrentStore.Id,
                        pageSize: _catalogSettings.NumberOfBestsellersOnHomepage).ToList());

            //load products
            var products = _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            return products;
        }
        private IPagedList<Product> CampaignProducts(int campaignId, int pageIndex, int pageSize)
        {
            IPagedList<ProductCampaign> productCampaigns = _campaignService.GetProductCampaignsByCampaignId(campaignId,
                pageIndex: pageIndex - 1, pageSize: pageSize);

            var products = new List<Product>();
            foreach (ProductCampaign productCampaign in productCampaigns)
            {
                Product product = productCampaign.Product;
                products.Add(product);
            }
            return new PagedList<Product>(products, pageIndex - 1, pageSize, productCampaigns.TotalCount);
        }
        private ProductResult PrepareProductOverview(Product product)
        {
            int pictureId = 0;
            if (product.ProductPictures.Count > 0)
            {
                pictureId = product.ProductPictures.FirstOrDefault().PictureId;
            }
            Picture pictureById = _pictureService.GetPictureById(pictureId);
            if (pictureById == null)
            {
                pictureById = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
            }
            string imageUrl = "";
            if (pictureById != null)
                imageUrl = _pictureService.GetPictureUrl(pictureById.Id, storeLocation: _storeContext.CurrentStore.Url,
                    targetSize: _mediaSettings.ProductThumbPictureSize);
            //string waterMarkImageUrl = _miscWatermarkPictureService.GetPictureUrlWithWatermark(pictureById,
            //    storeLocation: _storeContext.CurrentStore.Url, targetSize: _mediaSettings.ProductThumbPictureSize);

            //}
            string categoryName = "", categorySeName = "";
            int categoryId = 0;
            foreach (ProductCategory category in product.ProductCategories)
            {
                categoryId = category.CategoryId;
                categoryName = _localizationService.GetLocalized(category.Category, x => x.Name);
                categorySeName = _urlRecordService.GetSeName(category.Category);
            }
            int manufacturerId = 0;
            string manufacturerName = "";
            string productUrl = string.Format("{0}p/{1}", _webHelper.GetStoreLocation(),
                _urlRecordService.GetSeName(product));

            bool available = true;
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                 available = product.StockQuantity > 0?true:false;
            }

            var item = new ProductResult()
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = _localizationService.GetLocalized(product, x => x.Name),
                ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription),
                FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription),
                ImageUrl = imageUrl,
                //WaterMarkImageUrl = waterMarkImageUrl,
                ProductUrl = productUrl,
                CategoryId = categoryId,
                CategoryName = categoryName,
                categorySeName = categorySeName,
                ManufacturerId = manufacturerId,
                ManufacturerName = manufacturerName,
                Available = available,
                InStock = available,
                MarkAsNew = product.MarkAsNew,
                Featured = product.ShowOnHomepage,
                
            };
            var shoppingCartItems = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
            var currentProductInWishList = shoppingCartItems
                 .Where(sci => sci.ProductId == item.Id)
                 .LimitPerStore(_storeContext.CurrentStore.Id)
                 .FirstOrDefault();
            item.IsInWishlist= currentProductInWishList != null ? true : false;
            item.IsAddedToWishlist= currentProductInWishList != null ? true : false;
            this.PrepareProductPrice(item, product);

            return item;
        }
        private void PrepareProductPrice(ProductResult productResult, Product product)
        {
            //decimal taxRate;
            if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                productResult.OldPrice = null;
                productResult.Price = null;
            }
            else if (!product.CustomerEntersPrice)
            {
                if (!product.CallForPrice)
                {
                    decimal taxRate;
                    decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                    decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: false), out taxRate);
                    decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: true), out taxRate);

                    decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                    decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                    decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                    //cache the orignal prices before changing
                    productResult.CachedPriceValue = finalPriceWithDiscount;
                    productResult.CachedOldPriceValue = oldPrice;
                    productResult.CachedDiscountPriceValue = finalPriceWithoutDiscount;

                    if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                        productResult.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                    if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                        productResult.PriceWithoutDiscount = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

                    productResult.Price = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                    List<TierPrice> tierPrices = new List<TierPrice>();
                    if (product.HasTierPrices)
                    {
                        tierPrices.AddRange((
                            from tp in product.TierPrices
                            orderby tp.Quantity
                            select tp).ToList<TierPrice>().FilterByStore(_storeContext.CurrentStore.Id).FilterByCustomerRole(_customerService.GetCustomerRoleIds(_workContext.CurrentCustomer)).RemoveDuplicatedQuantities());
                    }

                    bool flag;
                    if (tierPrices.Count <= 0)
                        flag = false;
                    else
                        flag = tierPrices.Count != 1 || tierPrices[0].Quantity > 1;

                    if (flag)
                    {
                        productResult.OldPrice = null;
                        var priceFormattedString = _localizationService.GetResource("Products.PriceRangeFrom");
                        productResult.Price = string.Format(priceFormattedString, _priceFormatter.FormatPrice(finalPriceWithDiscount));
                    }
                    if (product.IsRental)
                    {
                        productResult.OldPrice = _priceFormatter.FormatRentalProductPeriod(product, productResult.OldPrice);
                        productResult.Price = _priceFormatter.FormatRentalProductPeriod(product, productResult.Price);
                    }

                }
                else
                {
                    productResult.OldPrice = null;
                    productResult.Price = _localizationService.GetResource("Products.CallForPrice");
                }
            }
        }
        private object PrepareViewModes(string viewMode)
        {
          
            var model = new 
            {
                ViewMode = !string.IsNullOrEmpty(viewMode) ? viewMode : _catalogSettings.DefaultViewMode,
                AvailableViewModes = new List<ViewModes>()
            };
            if (_catalogSettings.AllowProductViewModeChanging)
            {
                ViewModes grid = new ViewModes { Id = 1, Value = _localizationService.GetResource("Catalog.ViewMode.Grid") };
                ViewModes list = new ViewModes { Id = 2, Value = _localizationService.GetResource("Catalog.ViewMode.List") };
                model.AvailableViewModes.Add(grid);
                model.AvailableViewModes.Add(list);
            }
            return model;
        }
        private object PrepareSortingOptions(int? orderBy)
        {
            var model = new
            {
                AvailableSortOptions = new List<object>()
            };
            var allDisabled = _catalogSettings.ProductSortingEnumDisabled.Count == Enum.GetValues(typeof(ProductSortingEnum)).Length;
            var allowProductSorting = _catalogSettings.AllowProductSorting && !allDisabled;

            var activeOptions = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled)
                .Select((idOption) =>
                {
                    int order;
                    return new KeyValuePair<int, int>(idOption, _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out order) ? order : idOption);
                })
                .OrderBy(x => x.Value);

            if (orderBy == null)
                orderBy = allDisabled ? 0 : activeOptions.First().Key;

            if (allowProductSorting)
            {
                foreach (var option in activeOptions)
                {
                    var sortValue = _localizationService.GetLocalizedEnum(((ProductSortingEnum)option.Key), _workContext.WorkingLanguage.Id); //((ProductSortingEnum)option.Key).GetLocalizedEnum(_localizationService, _workContext);
                    model.AvailableSortOptions.Add(new
                    {
                        Text = sortValue,
                        Value = option.Key,
                        Selected = option.Key == orderBy
                    });
                }
            }
            return model;
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
