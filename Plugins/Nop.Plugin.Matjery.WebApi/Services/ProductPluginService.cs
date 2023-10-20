using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class ProductPluginService : BasePluginService,IProductPluginService
    {
        public List<ProductResult> GetHomePageProducts()
        {

            IList<Product> products = _productService.GetAllProductsDisplayedOnHomePage();

            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize<Product>(p) && _productService.ProductIsAvailable(p)).ToList();

            var homepageProducts = new List<ProductResult>();
            foreach (Product product in products)
            {
                var vendor = _vendorService.GetVendorById(product.VendorId);
                if (vendor != null)
                {
                    if (vendor.Active)
                    {
                        var item = this.PrepareProductOverview(product);
                        //if (string.IsNullOrEmpty(item.ImageUrl))
                        //    continue;

                        foreach (Language language in _languageService.GetAllLanguages())
                        {
                            item.CustomProperties.Add(language.UniqueSeoCode, new
                            {
                                Name = _localizationService.GetLocalized(product, x => x.Name, language.Id),
                                ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription, language.Id),
                                FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription, language.Id)
                            });
                        }
                        homepageProducts.Add(item);
                    }
                }
               
            }
            return homepageProducts;
        }

        public List<ProductResult> GetHomePageBestSeller()
        {

            var bestSellerProducts = new List<ProductResult>();
            if (_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage > 0)
            {
                var products = this.HomepageBestSellers();
                foreach (Product product in products)
                {
                    var vendor = _vendorService.GetVendorById(product.VendorId);
                    if (vendor.Active)
                    {
                        ProductResult item = this.PrepareProductOverview(product);
                        //if (string.IsNullOrEmpty(item.ImageUrl))
                        //    continue;

                        foreach (Language language in _languageService.GetAllLanguages())
                        {
                            item.CustomProperties.Add(language.UniqueSeoCode, new
                            {
                                Name = _localizationService.GetLocalized(product, x => x.Name, language.Id),
                                ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription, language.Id),
                                FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription, language.Id)
                            });
                        }
                        bestSellerProducts.Add(item);
                    }
                   
                }
            }
            return bestSellerProducts;
        }



        public List<ProductResult> GetHomePageDiscountedPage(int pageIndex, int pageSize)
        {
            var bestSellerProducts = new List<ProductResult>();
            IPagedList<Product> products = _productService.GetAllProductsDiscounted(pageIndex - 1, pageSize);
            foreach (Product product in products)
            {
                //var vendor = _vendorService.GetVendorById(product.VendorId);
                //if (vendor != null)
                //{
                    //if (vendor.Active)
                    //{
                        ProductResult item = this.PrepareProductOverview(product);
                        foreach (Language language in _languageService.GetAllLanguages())
                        {
                            item.CustomProperties.Add(language.UniqueSeoCode, new
                            {
                                Name = _localizationService.GetLocalized(product, x => x.Name, language.Id),
                                ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription, language.Id),
                                FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription, language.Id)
                            });
                        }
                        bestSellerProducts.Add(item);
                    //}
                //}
         
            }
                return bestSellerProducts;
        }

        public ProductDetailsModel GetProductDetails(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                return null;

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !_aclService.Authorize(product) ||
                //Store mapping
                !_storeMappingService.Authorize(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            if (notAvailable && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                throw new ApplicationException("Not found");

            //prepare the model
            var model = _productModelFactory.PrepareProductDetailsModel(product);

         
            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewProduct", _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product);

            model.ShortDescription = CommonHelper.StripHTML(model.ShortDescription);
            return model;
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
            {
                imageUrl = _pictureService.GetPictureUrl(pictureById.Id, storeLocation: _storeContext.CurrentStore.Url,
                                targetSize: _mediaSettings.ProductThumbPictureSize);
            }
            //else
            //{
            //    var pictureThumbnailUrl = _pictureService.GetDefaultPictureUrl(targetSize: 1);
            //    imageUrl = "";
            //}
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
            string productUrl = string.Format("{0}{1}", _webHelper.GetStoreLocation(),
                _urlRecordService.GetSeName(product));
          
            bool available = true;
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                available = product.StockQuantity > 0 ? true : false;
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
                MarkAsNew = product.MarkAsNew,
                InStock = available,
                Featured = product.ShowOnHomepage,
            };
            var shoppingCartItems = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
            var currentProductInWishList = shoppingCartItems
                 .Where(sci => sci.ProductId == item.Id)
                 .LimitPerStore(_storeContext.CurrentStore.Id)
                 .FirstOrDefault();
            item.IsAddedToWishlist = currentProductInWishList != null ? true : false;
            item.IsInWishlist = item.IsAddedToWishlist;
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
    }
}
