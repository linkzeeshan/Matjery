using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Catalog;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class CartPluginService : BasePluginService, ICartPluginService
    {
        public IList<string> AddToCart(ParamsModel.CartParamsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                throw new ApplicationException(_localizationService.GetResource("Admin.AccessDenied.Description"));

            Product product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("The product not found");

            string attributesXml = "";
            if (model.ProductAttributeses != null && model.ProductAttributeses.Any())
            {
                var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var attribute in productAttributes)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                ProductAttributes ctrlAttributes = model.ProductAttributeses.FirstOrDefault(pa => pa.ProductAttributeId == attribute.Id);
                                if (ctrlAttributes != null && !String.IsNullOrEmpty(ctrlAttributes.ProductAttributeValue))
                                {
                                    string enteredText = ctrlAttributes.ProductAttributeValue.Trim();
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, enteredText);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            IList<string> warnnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer, product, ShoppingCartType.ShoppingCart,
                _storeContext.CurrentStore.Id, attributesXml, decimal.Zero, null, null, model.Quantity, false);
            return warnnings;
        }

        public bool DeleteFromCart(ParamsModel.CartParamsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
            {
                throw new ApplicationException(_localizationService.GetResource("Admin.AccessDenied.Description"));
            }

            Product product = _productService.GetProductById(model.ProductId);
            if (product == null)
            {
                throw new ApplicationException("The product not found");
            }
            var list = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            foreach (ShoppingCartItem item in list)
            {
                if (product.Id == item.ProductId)
                {
                    _shoppingCartService.DeleteShoppingCartItem(item);
                }
            }
            return true;

        }

        public List<CartResult> GetShoppingCart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
            {
                throw new ApplicationException(_localizationService.GetResource("Admin.AccessDenied.Description"));
            }
            List<CartResult> list = new List<CartResult>();
           var shoppingCartItems = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            //List<ShoppingCartItem> shoppingCartItems = (from sci in _workContext.CurrentCustomer.ShoppingCartItems
            //                                            where sci.ShoppingCartType == ShoppingCartType.ShoppingCart
            //                                            select sci).Where(x=>x.StoreId == _storeContext.CurrentStore.Id).ToList<ShoppingCartItem>();

            foreach (ShoppingCartItem item in shoppingCartItems)
            {
                Product product = _productService.GetProductById(item.ProductId);
                Picture itemPicture = _pictureService.GetProductPicture(product,item.AttributesXml);
                CartResult cartResult = new CartResult()
                {
                    ProductId = product.Id
                };

                cartResult.VendorId = product.VendorId;
                if (cartResult.VendorId > 0)
                {
                    Vendor cartVendor = _vendorService.GetVendorById(cartResult.VendorId);
                    if (cartVendor != null)
                        cartResult.VendorName =_localizationService.GetLocalized(cartVendor,v => v.Name);
                }

                var productResult = new ProductResult();
                this.PrepareProductPrice(productResult, product);
                item.Product = product;
                bool available = true;
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                {
                    available = product.StockQuantity > 0 ? true : false;
                }

                cartResult.ProductName =_localizationService.GetLocalized(product,x => x.Name);
                cartResult.Price = productResult.Price;
                cartResult.OldPrice = productResult.OldPrice;
                cartResult.SKU = product.Sku;
                cartResult.Quantity = item.Quantity;
                cartResult.InStock = available;
                cartResult.ImageUrl = _pictureService.GetPictureUrl(itemPicture == null ? 0 : itemPicture.Id, _mediaSettings.CartThumbPictureSize, storeLocation: _storeContext.CurrentStore.Url);
                cartResult.ImageTitle = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), product.Name);
                PrepareShoppingCartPrice(cartResult, item);

                IList<string> itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer, item.ShoppingCartType,
                    item.Product, item.StoreId, item.AttributesXml, item.CustomerEnteredPrice, item.RentalStartDateUtc, item.RentalEndDateUtc, item.Quantity,
                    false, item.Id, false, true, true, false);
                foreach (string warning in itemWarnings)
                {
                    cartResult.Warnings.Add(warning);
                }
                list.Add(cartResult);
            }
            return list;
        }

        public IList<string> UpdatCartQuantity(ParamsModel.CartParamsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                throw new ApplicationException(_localizationService.GetResource("Admin.AccessDenied.Description"));

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id, model.ProductId).FirstOrDefault();


            if (cart == null)
                throw new ApplicationException("Cart item not found");

            IList<string> warnnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    cart.Id, cart.AttributesXml, cart.CustomerEnteredPrice, cart.RentalStartDateUtc, cart.RentalEndDateUtc, model.NewQuantity);
            return warnnings;
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

        private void PrepareShoppingCartPrice(CartResult cart, ShoppingCartItem sci)
        {
            decimal taxRate;
            if (!sci.Product.CallForPrice)
            {
                decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, _shoppingCartService.GetUnitPrice(sci), out taxRate);
                decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);

                cart.CachedPriceValue = shoppingCartUnitPriceWithDiscount;
                cart.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount * cart.Quantity);
            }
            else
            {
                cart.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
            }
            if (!sci.Product.CallForPrice)
            {
                var shoppingCartItemDiscountBase = new decimal();
                List<Discount> scDiscounts = null;
                int? maximumDiscountQty = null;
                decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product,
                    _shoppingCartService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts,out maximumDiscountQty), out taxRate);//TODO
                decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase,
                    _workContext.WorkingCurrency);
                cart.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cart.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }
            }
            else
            {
                cart.SubTotal = _localizationService.GetResource("Products.CallForPrice");
            }
        }

    }
}
