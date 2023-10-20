using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class WishListPluginService : BasePluginService, IWishListPluginService
    {
        public IList<string> AddToWhishlist(ParamsModel.WishlistParams paramsModel)
        {
            if (!this._permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                throw new ApplicationException(this._localizationService.GetResource("Admin.AccessDenied.Description"));

            Product product = this._productService.GetProductById(paramsModel.ProductId);
            if (product == null)
                throw new ApplicationException("The product not found");

            string attributesXml = "";
            if (paramsModel.ProductAttributeses != null && paramsModel.ProductAttributeses.Count > 0)
            {
                attributesXml = "<Attributes>";
                foreach (ProductAttributes attributes in paramsModel.ProductAttributeses)
                {
                    attributesXml = string.Concat(attributesXml, string.Format("<ProductVariantAttribute ID=\"{0}\"><ProductVariantAttributeValue><Value>{1}</Value></ProductVariantAttributeValue></ProductVariantAttribute>", attributes.ProductAttributeId, attributes.ProductAttributeValue));
                }
                attributesXml = string.Concat(attributesXml, "</Attributes>");
            }

            IList<string> warnnings = this._shoppingCartService.AddToCart(this._workContext.CurrentCustomer, product, ShoppingCartType.Wishlist,
                this._storeContext.CurrentStore.Id, attributesXml, decimal.Zero, null, null, paramsModel.Quantity, false);
            return warnnings;
        }

        public bool DeleteFromWishlist(ParamsModel.WishlistParams paramsModel)
        {
            if (!this._permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                throw new ApplicationException(this._localizationService.GetResource("Admin.AccessDenied.Description"));

            Product product = this._productService.GetProductById(paramsModel.ProductId);
            if (product == null)
                throw new ApplicationException("The product not found");

            //List<ShoppingCartItem> list = (from sci in this._workContext.CurrentCustomer.ShoppingCartItems
            //                               where sci.ShoppingCartType == ShoppingCartType.Wishlist
            //                               select sci)
            //                               .Where(x=>x.StoreId == this._storeContext.CurrentStore.Id)
            //                               .ToList();

            var list = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);


            foreach (ShoppingCartItem item in list)
            {
                if (product.Id == item.ProductId)
                    this._shoppingCartService.DeleteShoppingCartItem(item);
            }
            return true;
        }

        public List<WishlistResult> GetWishlist()
        {
            if (!this._permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                throw new ApplicationException(this._localizationService.GetResource("Admin.AccessDenied.Description"));

            var list = new List<WishlistResult>();
            //List<ShoppingCartItem> list2 = (from sci in this._workContext.CurrentCustomer.ShoppingCartItems
            //                                where sci.ShoppingCartType == ShoppingCartType.Wishlist
            //                                select sci)
            //    .Where(x=>x.StoreId ==  this._storeContext.CurrentStore.Id)
            //    .ToList();

            var list2 = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

            foreach (ShoppingCartItem product in list2)
            {
                Product pro = _productService.GetProductById(product.ProductId);

                bool available = true;
                if (pro.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                {
                    available = pro.StockQuantity > 0 ? true : false;
                }

                product.Product = pro;
                Picture itemPicture =_pictureService.GetProductPicture(product.Product,product.AttributesXml);

                var wishlist = new WishlistResult
                {
                    ProductId = product.Product.Id,
                    ProductName = _localizationService.GetLocalized(product.Product, x => x.Name),
                    InStock = available,
                    Sku = product.Product.Sku,
                    Quantity = product.Quantity,
                    ImageUrl = itemPicture!=null?this._pictureService.GetPictureUrl(itemPicture.Id, this._mediaSettings.CartThumbPictureSize):"",
                    ImageTitle =
                        string.Format(this._localizationService.GetResource("Media.Product.ImageLinkTitleFormat"),
                            product.Product.Name)
                };

                this.PrepareWishlistPrice(wishlist, product);

                IList<string> itemWarnings = this._shoppingCartService.GetShoppingCartItemWarnings(this._workContext.CurrentCustomer,
                    product.ShoppingCartType, product.Product, product.StoreId, product.AttributesXml, product.CustomerEnteredPrice, product.RentalStartDateUtc,
                    product.RentalEndDateUtc, product.Quantity, false, product.Id, false, true, true, false);

                foreach (string warning in itemWarnings)
                {
                    wishlist.Warnings.Add(warning);
                }
                list.Add(wishlist);
            }
            return list;
        }

        private void PrepareWishlistPrice(WishlistResult wishlist, ShoppingCartItem sci)
        {
            decimal taxRate = new decimal();
            if (!sci.Product.CallForPrice)
            {
                decimal shoppingCartUnitPriceWithDiscountBase = this._taxService.GetProductPrice(sci.Product, this._shoppingCartService.GetUnitPrice(sci), out taxRate);
                decimal shoppingCartUnitPriceWithDiscount = this._currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, this._workContext.WorkingCurrency);
                wishlist.UnitPrice = this._priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
            }
            else
            {
                wishlist.UnitPrice = this._localizationService.GetResource("Products.CallForPrice");
            }
            if (!sci.Product.CallForPrice)
            {
                decimal shoppingCartItemDiscountBase;
                List<Discount> scDiscounts;
                int? maximumDiscountQty = null;

                decimal shoppingCartItemSubTotalWithDiscountBase = this._taxService.GetProductPrice(sci.Product,
                    this._shoppingCartService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts,out maximumDiscountQty), out taxRate);//TODO
                decimal shoppingCartItemSubTotalWithDiscount = this._currencyService.ConvertFromPrimaryStoreCurrency(
                    shoppingCartItemSubTotalWithDiscountBase, this._workContext.WorkingCurrency);
                wishlist.SubTotal = this._priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    shoppingCartItemDiscountBase = this._taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = this._currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, this._workContext.WorkingCurrency);
                        wishlist.Discount = this._priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }
            }
            else
            {
                wishlist.SubTotal = this._localizationService.GetResource("Products.CallForPrice");
            }
        }
    }
}
