using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Plugin.Payments.CheckMoneyOrder;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class CheckOutPluginService : BasePluginService, ICheckOutPluginService
    {
        public OrderSummaryResult GetOrderSummary()
        {
            //List<ShoppingCartItem> cart = (from sci in this._workContext.CurrentCustomer.ShoppingCartItems
            //                               where sci.ShoppingCartType == ShoppingCartType.ShoppingCart
            //                               select sci).Where(x=>x.StoreId ==  this._storeContext.CurrentStore.Id).ToList<ShoppingCartItem>();


            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);



            OrderSummaryResult orderSummaryResult = new OrderSummaryResult();
            if (cart.Count > 0)
            {
                bool subTotalIncludingTax = _workContext.TaxDisplayType != null ? false : !this._taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var orderSubTotalDiscountAmountBase = new decimal();
                List<Discount> orderSubTotalAppliedDiscounts = null;
                var subTotalWithoutDiscountBase = new decimal();
                var subTotalWithDiscountBase = new decimal();
                this._orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                decimal subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = this._currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, this._workContext.WorkingCurrency);
                orderSummaryResult.SubTotal = this._priceFormatter.FormatPrice(subtotal, true, this._workContext.WorkingCurrency.CurrencyCode, this._workContext.WorkingLanguage.Id, subTotalIncludingTax);
                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = this._currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, this._workContext.WorkingCurrency);
                    orderSummaryResult.SubTotalDiscount = this._priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, this._workContext.WorkingCurrency.CurrencyCode,
                        this._workContext.WorkingLanguage.Id, subTotalIncludingTax);
                }
                orderSummaryResult.RequiresShipping =_shoppingCartService.ShoppingCartRequiresShipping(cart);
                if (orderSummaryResult.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = this._orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = this._currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, this._workContext.WorkingCurrency);
                        orderSummaryResult.Shipping = this._priceFormatter.FormatShippingPrice(shoppingCartShipping, true);
                        //ShippingOption shippingOption = this._workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption,
                        //    this._storeContext.CurrentStore.Id);
                        //if (shippingOption != null)
                        //{
                        //    orderSummaryResult.SelectedShippingMethod = shippingOption.Name;
                        //}
                    }
                }
                string paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(this._workContext.CurrentCustomer,NopCustomerDefaults.SelectedPaymentMethodAttribute);
                decimal paymentMethodAdditionalFee = this._paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = this._taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, this._workContext.CurrentCustomer);
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = this._currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, this._workContext.WorkingCurrency);
                    orderSummaryResult.PaymentMethodAdditionalFee = this._priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
                }
                bool displayTax = true;
                bool displayTaxRates = true;
                if (!this._taxSettings.HideTaxInOrderSummary || this._workContext.TaxDisplayType != 0)
                {
                    SortedDictionary<decimal, decimal> taxRates = null;
                    decimal shoppingCartTaxBase = this._orderTotalCalculationService.GetTaxTotal(cart, out taxRates, true);
                    decimal shoppingCartTax = this._currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, this._workContext.WorkingCurrency);
                    if (shoppingCartTaxBase != decimal.Zero || !this._taxSettings.HideZeroTax)
                    {
                        displayTaxRates = this._taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;
                        orderSummaryResult.Tax = this._priceFormatter.FormatPrice(shoppingCartTax, true, false);
                        foreach (KeyValuePair<decimal, decimal> tr in taxRates)
                        {
                            orderSummaryResult.TaxRates.Add(new OrderSummaryResult.TaxRate()
                            {
                                Rate = this._priceFormatter.FormatTaxRate(tr.Key),
                                Value = this._priceFormatter.FormatPrice(this._currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, this._workContext.WorkingCurrency), true, false)
                            });
                        }
                    }
                    else
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                }
                else
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                orderSummaryResult.DisplayTaxRates = displayTaxRates;
                orderSummaryResult.DisplayTax = displayTax;
                var orderTotalDiscountAmountBase = new decimal();
                List<Discount> orderTotalAppliedDiscounts = null;
                List<AppliedGiftCard> appliedGiftCards = null;
                var redeemedRewardPoints = 0;
                var redeemedRewardPointsAmount = new decimal();
                decimal? shoppingCartTotalBase = this._orderTotalCalculationService.GetShoppingCartTotal(cart,
                    out orderTotalDiscountAmountBase, out orderTotalAppliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, false);
                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = this._currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, this._workContext.WorkingCurrency);
                    orderSummaryResult.OrderTotal = this._priceFormatter.FormatPrice(shoppingCartTotal, true, false);
                }
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = this._currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, this._workContext.WorkingCurrency);
                    orderSummaryResult.OrderTotalDiscount = this._priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, false);
                }
                if (appliedGiftCards != null && appliedGiftCards.Count > 0)
                {
                    foreach (AppliedGiftCard appliedGiftCard in appliedGiftCards)
                    {
                        OrderSummaryResult.GiftCard gcModel = new OrderSummaryResult.GiftCard()
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode
                        };
                        decimal amountCanBeUsed = this._currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, this._workContext.WorkingCurrency);
                        gcModel.Amount = this._priceFormatter.FormatPrice(-amountCanBeUsed, true, false);
                        decimal remainingAmountBase = _giftCardService.GetGiftCardRemainingAmount(appliedGiftCard.GiftCard) - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = this._currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, this._workContext.WorkingCurrency);
                        gcModel.Remaining = this._priceFormatter.FormatPrice(remainingAmount, true, false);
                        orderSummaryResult.GiftCards.Add(gcModel);
                    }
                }
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = this._currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, this._workContext.WorkingCurrency);
                    orderSummaryResult.RedeemedRewardPoints = redeemedRewardPoints;
                    orderSummaryResult.RedeemedRewardPointsAmount = this._priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }
                if (this._rewardPointsSettings.Enabled && this._rewardPointsSettings.DisplayHowMuchWillBeEarned && shoppingCartTotalBase.HasValue)
                {
                    int willEarnRewardPoints = this._orderTotalCalculationService.CalculateRewardPoints(this._workContext.CurrentCustomer, shoppingCartTotalBase.Value);
                    if (willEarnRewardPoints > 0)
                    {
                        orderSummaryResult.WillEarnRewardPointsText = string.Format(this._localizationService.GetResource("ShoppingCart.Totals.RewardPoints.WillEarn.Point"), willEarnRewardPoints);
                    }
                }
            }
            return orderSummaryResult;
        }

        public List<PaymentMethodResult> GetStandardPaymentMethods()
        {
            var standardPayments = new List<PaymentMethodResult>();
            var currencyCode = this._currencyService.GetCurrencyById(this._workContext.WorkingCurrency.Id).CurrencyCode;
            Customer currentCustomer = this._workContext.CurrentCustomer;

            //List<ShoppingCartItem> list = (from sci in this._workContext.CurrentCustomer.ShoppingCartItems
            //                               where sci.ShoppingCartType == ShoppingCartType.ShoppingCart
            //                               select sci).Where(x=>x.StoreId == this._storeContext.CurrentStore.Id).ToList<ShoppingCartItem>();
            var list = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            int filterByCountryId = 0;
            if (this._addressSettings.CountryEnabled
                && currentCustomer.BillingAddress != null
                && currentCustomer.BillingAddress.CountryId != null)
            {
                filterByCountryId = (int)this._workContext.CurrentCustomer.BillingAddress.CountryId;
            }
            List<IPaymentMethod> paymentMethods = (
                from pm in this._paymentService.LoadActivePaymentMethods(new int?(currentCustomer.Id), this._storeContext.CurrentStore.Id, filterByCountryId)
                where pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection
                //where !pm.HidePaymentMethod(list)
                select pm).ToList<IPaymentMethod>();
            IList<string> standardSystemNames = new List<string>()
                {
                    "NopAdvance.Plugin.COD"
                };
            foreach (IPaymentMethod paymentMethod in paymentMethods)
            {
                if (standardSystemNames.Any<string>((string x) => x == paymentMethod.PluginDescriptor.SystemName))
                {
                    if (!_shoppingCartService.ShoppingCartIsRecurring(list) || paymentMethod.RecurringPaymentType != 0)
                    {
                        var paymentMethodResult = new PaymentMethodResult
                        {
                            SystemName = paymentMethod.PluginDescriptor.SystemName,
                            FriendlyName = _localizationService.GetLocalizedFriendlyName<IPaymentMethod>(paymentMethod,this._workContext.WorkingLanguage.Id, true),
                            LogoUrl = _pluginService.GetPluginLogoUrl(paymentMethod.PluginDescriptor),
                        };
                        if (string.Equals(paymentMethodResult.SystemName, "NopAdvance.Plugin.COD"))
                        {
                            var checkMoneyOrderPaymentSettings = this._settingService.LoadSetting<CheckMoneyOrderPaymentSettings>(_storeContext.CurrentStore.Id);
                            paymentMethodResult.Description =_localizationService.GetLocalizedSetting(checkMoneyOrderPaymentSettings,x => x.DescriptionText,
                                    _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
                        }
                        standardPayments.Add(paymentMethodResult);
                    }
                }
            }

            return standardPayments;
        }
    }
}
