using Nop.Core.Domain.Catalog;
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
    public class ProductAttributePluginService : BasePluginService, IProductAttributePluginService
    {
        public List<ProductDetailsResult.ProductAttributeModel> GetProductAttributes(int productId)
        {
            var productAttributes = new List<ProductDetailsResult.ProductAttributeModel>();
            Product product = this._productService.GetProductById(productId);
            if (product != null)
            {
                //performance optimization
                //We cache a value indicating whether a product has attributes
                IList<ProductAttributeMapping> productAttributeMapping = null;
                var cacheKey = _cacheKeyService.PrepareKey(NopCatalogDefaults.PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY, product.Id);

                var hasProductAttributesCache = _staticCacheManager.Get<bool?>(cacheKey,()=>true);//TODO
                if (!hasProductAttributesCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load attributes and cache the result (true/false)
                    productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                    hasProductAttributesCache = productAttributeMapping.Any();
                    _staticCacheManager.Set(cacheKey, hasProductAttributesCache);
                }
                if (hasProductAttributesCache.Value && productAttributeMapping == null)
                {
                    //cache indicates that the product has attributes
                    //let's load them
                    productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                }
                if (productAttributeMapping == null)
                {
                    productAttributeMapping = new List<ProductAttributeMapping>();
                }
                foreach (var attribute in productAttributeMapping)
                {
                    var attributeModel = new ProductDetailsResult.ProductAttributeModel
                    {
                        Id = attribute.Id,
                        ProductId = product.Id,
                        ProductAttributeId = attribute.ProductAttributeId,
                        Name = _localizationService.GetLocalized(product,x => x.Name),//TODO
                        Description = _localizationService.GetLocalized(product,x => x.ShortDescription),//TODO
                        TextPrompt = attribute.TextPrompt,
                        IsRequired = attribute.IsRequired,
                        AttributeControlType = attribute.AttributeControlType,
                        DefaultValue = attribute.DefaultValue,
                        HasCondition = !String.IsNullOrEmpty(attribute.ConditionAttributeXml)
                    };
                    if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                    {
                        attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }

                    if (attribute.ShouldHaveValues())
                    {
                        //values
                        var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                        foreach (var attributeValue in attributeValues)
                        {
                            var valueModel = new ProductDetailsResult.ProductAttributeValueModel
                            {
                                Id = attributeValue.Id,
                                Name =_localizationService.GetLocalized(attributeValue,x => x.Name),
                                ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                                IsPreSelected = attributeValue.IsPreSelected
                            };
                            attributeModel.Values.Add(valueModel);

                            //display price if allowed
                            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                            {
                                decimal taxRate;
                                decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(product,attributeValue,_workContext.CurrentCustomer);
                                decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out taxRate);
                                decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

                                valueModel.PriceAdjustmentValue = priceAdjustment;
                            }

                        }

                    }
                    productAttributes.Add(attributeModel);
                }
            }
            return productAttributes;
        }
    }
}
