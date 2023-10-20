using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Shipping;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class OrderPluginService : BasePluginService, IOrderPluginService
    {
        public (FinishOrderResult, ApiValidationResultResponse) PlaceOrder(ParamsModel.PlaceOrderParamsModel model)
        {
            Customer currentCustomer = this._workContext.CurrentCustomer;

            //List<ShoppingCartItem> cart = (from sci in currentCustomer.ShoppingCartItems
            //                               where sci.ShoppingCartType == ShoppingCartType.ShoppingCart
            //                               select sci).Where(x=>x.StoreId == this._storeContext.CurrentStore.Id).ToList<ShoppingCartItem>();


            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            var finishOrderResult = new FinishOrderResult();
            var validationResult = new ApiValidationResultResponse();

            if (cart.Count == 0)
                throw new ApplicationException("cart is empty!");

            Address billingAddressQ = (from x in currentCustomer.Addresses
                                      where x.Id == model.BillingAddressId
                                      select x).FirstOrDefault();


            Address billingAddress = _addressService.GetAddressById(model.BillingAddressId);

            if (billingAddress == null)
            {
                throw new ApplicationException("Address billing doens't exists");
            }
            currentCustomer.BillingAddress = billingAddress;
            if (!_shoppingCartService.ShoppingCartRequiresShipping(cart))
            {
                currentCustomer.ShippingAddress = null;
                this._genericAttributeService.SaveAttribute<ShippingOption>(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, _storeContext.CurrentStore.Id);
            }
            else if (!(_shippingSettings.AllowPickupInStore & model.PickUpInStore))
            {
                this._genericAttributeService.SaveAttribute<PickupPoint>(currentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, null, _storeContext.CurrentStore.Id);
                Nop.Core.Domain.Common.Address shippingAddress = null;
                if (model.ShippingAddressId.HasValue)
                {
                    //shippingAddress = (
                    //    from x in currentCustomer.Addresses
                    //    where x.Id == model.ShippingAddressId.Value
                    //    select x).FirstOrDefault<Nop.Core.Domain.Common.Address>();

                    shippingAddress = _addressService.GetAddressById(model.ShippingAddressId.Value);
                }
                if (shippingAddress == null)
                {
                    throw new ApplicationException("Address shipping doens't exists");
                }
                currentCustomer.ShippingAddress = shippingAddress;
                var shippingMathod = this._shippingService.GetAllShippingMethods();

                //TODO
                //get shipping providers
                var shippingRateComputations = _shippingPluginManager.LoadAllPlugins().ToPagedList(new ShippingProviderSearchModel() { });
                //IList<IShippingRateComputationMethod> shippingRateComputations = this._shippingService.LoadAllShippingRateComputationMethods(
                string allowedShippingRateComputationMethodSystemName = "";
                foreach (IShippingRateComputationMethod shippingRateComputation in shippingRateComputations)
                {
                    if (_shippingPluginManager.IsPluginActive(shippingRateComputation))
                    {
                        allowedShippingRateComputationMethodSystemName = shippingRateComputation.PluginDescriptor.SystemName;
                    }
                }


                List<ShippingOption> shippingOptions = this._shippingService.GetShippingOptions(cart, currentCustomer.ShippingAddress,currentCustomer, allowedShippingRateComputationMethodSystemName)
                    .ShippingOptions.ToList<ShippingOption>();
                if (shippingOptions == null)
                {
                    throw new ApplicationException("Shipping System name doens't exists");
                }
                ShippingOption option = shippingOptions.Find((ShippingOption so) => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(model.ShippingMethodName,
                    StringComparison.InvariantCultureIgnoreCase));
                if (option == null)
                {
                    option = shippingOptions.Count() > 0 ? shippingOptions[0] : option;
                    //if (option == null)
                        //throw new ApplicationException("Shipping name doens't exists");
                }
                this._genericAttributeService.SaveAttribute<ShippingOption>(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, option, _storeContext.CurrentStore.Id);
            }
            else
            {
                currentCustomer.ShippingAddress = null;
                string[] strArrays = model.PickupPointValue.Split(new string[] { "___" }, StringSplitOptions.None);
                List<PickupPoint> pickupPoints = this._shippingService.GetPickupPoints(billingAddress.Id, this._workContext.CurrentCustomer, strArrays[1],
                    _storeContext.CurrentStore.Id).PickupPoints.ToList<PickupPoint>();
                PickupPoint selectedPoint = pickupPoints.FirstOrDefault<PickupPoint>((PickupPoint x) => x.Id.Equals(strArrays[0]));
                ShippingOption shippingOption = new ShippingOption();
                shippingOption.Name = string.Format(this._localizationService.GetResource("Checkout.PickupPoints.Name"), selectedPoint.Name);
                shippingOption.Rate = selectedPoint.PickupFee;
                shippingOption.Description = selectedPoint.Description;
                shippingOption.ShippingRateComputationMethodSystemName = selectedPoint.ProviderSystemName;
                ShippingOption pickUpInStoreShippingOption = shippingOption;
                this._genericAttributeService.SaveAttribute<ShippingOption>(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute,
                    pickUpInStoreShippingOption, _storeContext.CurrentStore.Id);
                this._genericAttributeService.SaveAttribute<PickupPoint>(currentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute,
                    selectedPoint, this._storeContext.CurrentStore.Id);
            }
            this._customerService.UpdateCustomer(currentCustomer);
            bool isPaymentWorkflowRequired = this.IsPaymentWorkflowRequired(cart, true);
            if (isPaymentWorkflowRequired)
            {
                //TODO
                //IPaymentMethod paymentMethodInst = this._paymentService.LoadPaymentMethodBySystemName(model.PaymentMethodName);
                //if (paymentMethodInst == null || !paymentMethodInst.IsPaymentMethodActive(this._paymentSettings)
                //    || !this._pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, this._storeContext.CurrentStore.Id))
                //{
                //    throw new ApplicationException("Payment method name doens't exists");
                //}
                this._genericAttributeService.SaveAttribute<string>(currentCustomer, NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    model.PaymentMethodName, this._storeContext.CurrentStore.Id);
            }
            else
            {
                this._genericAttributeService.SaveAttribute<string>(currentCustomer, NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    null, this._storeContext.CurrentStore.Id);
            }

            ProcessPaymentRequest processPaymentRequest;
            if (!isPaymentWorkflowRequired)
            {
                processPaymentRequest = new ProcessPaymentRequest
                {
                    PaymentMethodSystemName = null
                };
            }
            else
            {
                processPaymentRequest = this.GetPaymentInfo(model.PaymentMethodName, model.CreditCardType, model.CreditCardName, model.CreditCardNumber,
                    model.CreditCardExpireMonth, model.CreditCardExpireYear, model.CreditCardCvv2);
            }
            processPaymentRequest.StoreId = this._storeContext.CurrentStore.Id;
            processPaymentRequest.CustomerId = currentCustomer.Id;

            PlaceOrderResult placeOrderResult = this._orderProcessingService.PlaceOrder(processPaymentRequest);
            if (placeOrderResult.Success)
            {
                //finishOrderResult.Id = placeOrderResult.PlacedOrder.Id;
                finishOrderResult.Message = "true";
                finishOrderResult.Status = HttpStatusCode.OK;
                //PostProcessPaymentRequest postProcessPaymentRequest = new PostProcessPaymentRequest();
                //postProcessPaymentRequest.Order = placeOrderResult.PlacedOrder;
            }
            else
            {
                string message = "";
                foreach (string error in placeOrderResult.Errors)
                {
                    message = string.Concat(message, error, " ");
                    validationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = message
                    });
                }
                finishOrderResult.Status = HttpStatusCode.BadRequest;
                finishOrderResult.Message = message;
                if (cart.Count == 0)
                {
                    finishOrderResult.Message = string.Concat(finishOrderResult.Message, " ", this._localizationService.GetResource("ShoppingCart.CartIsEmpty"));
                    validationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = finishOrderResult.Message
                    });
                }
            }
            return (finishOrderResult, validationResult);
        }


        private bool IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool ignoreRewardPoints = false)
        {
            bool result = true;
            decimal? shoppingCartTotalBase = this._orderTotalCalculationService.GetShoppingCartTotal(cart, ignoreRewardPoints, true);
            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
            {
                result = false;
            }
            return result;
        }

        private ProcessPaymentRequest GetPaymentInfo(string paymentMethodSystemName, string creditCardTypeOrPurchaseNumber, string creditCardName,
        string creditCardNumber, int? creditCardExpireMonth, int? creditCardExpireYear, string creditCardCvv2)
        {
            ProcessPaymentRequest paymentInfo = new ProcessPaymentRequest();
            paymentInfo.PaymentMethodSystemName = paymentMethodSystemName;
            string str = paymentMethodSystemName;
            if (str == "Payments.Manual" || str == "Payments.PayPalDirect")
            {
                this.ValidatePaymentForm(creditCardName, creditCardNumber, creditCardExpireMonth, creditCardExpireYear, creditCardCvv2);
                this.PopulatePaymentInfo(paymentInfo, creditCardTypeOrPurchaseNumber, creditCardName, creditCardNumber, creditCardExpireYear, creditCardExpireMonth, creditCardCvv2);
            }
            else if (str == "Payments.AuthorizeNet")
            {
                this.ValidatePaymentForm(creditCardName, creditCardNumber, creditCardExpireMonth, creditCardExpireYear, creditCardCvv2);
                this.PopulatePaymentInfo(paymentInfo, creditCardTypeOrPurchaseNumber, creditCardName, creditCardNumber, creditCardExpireYear, creditCardExpireMonth, creditCardCvv2, true);
            }
            else if (str == "Payments.PurchaseOrder")
            {
                string purchaseNumber = creditCardTypeOrPurchaseNumber;
                paymentInfo.CustomValues.Add(this._localizationService.GetResource("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber"), purchaseNumber);
            }
            else if (str != "Payments.CheckMoneyOrder")
            {
            }
            return paymentInfo;
        }
        private void PopulatePaymentInfo(ProcessPaymentRequest paymentInfo, string creditCardType, string creditCardName, string creditCardNumber,
           int? creditCardExpireMonth, int? creditCardExpireYear, string creditCardCvv2, bool isAuthorizeNetPayment = false)
        {
            if (!isAuthorizeNetPayment)
            {
                paymentInfo.CreditCardType = creditCardType;
            }
            paymentInfo.CreditCardName = creditCardName;
            paymentInfo.CreditCardNumber = creditCardNumber;
            if (creditCardExpireMonth != null) paymentInfo.CreditCardExpireMonth = creditCardExpireMonth.Value;
            if (creditCardExpireYear != null) paymentInfo.CreditCardExpireYear = creditCardExpireYear.Value;
            paymentInfo.CreditCardCvv2 = creditCardCvv2;
        }

        private void ValidatePaymentForm(string creditCardName, string creditCardNumber, int? creditCardExpireMonth, int? creditCardExpireYear, string creditCardCvv2)
        {
            if (string.IsNullOrWhiteSpace(creditCardName))
            {
                throw new ApplicationException(this._localizationService.GetResource("Payment.CardholderName.Required"));
            }
            if (!this.IsCreditCard(creditCardNumber))
            {
                throw new ApplicationException(this._localizationService.GetResource("Payment.CardNumber.Wrong"));
            }
            if (!creditCardExpireMonth.HasValue)
            {
                throw new ApplicationException(this._localizationService.GetResource("Payment.ExpireMonth.Required"));
            }
            if (!creditCardExpireYear.HasValue)
            {
                throw new ApplicationException(this._localizationService.GetResource("Payment.ExpireYear.Required"));
            }
            if (!new Regex("^[0-9]{3,4}$").IsMatch(creditCardCvv2))
            {
                throw new ApplicationException(this._localizationService.GetResource("Payment.CardCode.Wrong"));
            }
        }
        protected bool IsCreditCard(string cardNumber)
        {
            bool flag;
            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                cardNumber = cardNumber.Replace(" ", "");
                cardNumber = cardNumber.Replace("-", "");
                int checksum = 0;
                bool evenDigit = false;
                foreach (char digit in cardNumber.Reverse<char>())
                {
                    if (char.IsDigit(digit))
                    {
                        int digitValue = (digit - 48) * (evenDigit ? 2 : 1);
                        evenDigit = !evenDigit;
                        while (digitValue > 0)
                        {
                            checksum = checksum + digitValue % 10;
                            digitValue = digitValue / 10;
                        }
                    }
                    else
                    {
                        flag = false;
                        return flag;
                    }
                }
                flag = checksum % 10 == 0;
            }
            else
            {
                flag = false;
            }
            return flag;
        }

        public List<OrderResult> GetCustomerOrders()
        {
            if (!_customerService.IsRegistered(this._workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            IPagedList<Order> orders = this._orderService.SearchOrders(_storeContext.CurrentStore.Id, 0, _workContext.CurrentCustomer.Id);
            List<OrderResult> customerOrders = new List<OrderResult>();

            foreach (Order order in orders.OrderBy(o => o.Id))
            {
                var customerOrder = new OrderResult();
                customerOrder.BillingAddressId = order.BillingAddressId;
                customerOrder.CreatedOnUtc = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D",
                    new CultureInfo(_workContext.WorkingLanguage.LanguageCulture));
                customerOrder.CurrencyRate = order.CurrencyRate;
                Currency currency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
                customerOrder.CustomerCurrencyCode = _localizationService.GetLocalized(currency,c => c.Name);
                customerOrder.CustomerId = order.CustomerId;
                customerOrder.CustomerTaxDisplayTypeId = order.CustomerTaxDisplayTypeId;
                customerOrder.Deleted = order.Deleted;
                customerOrder.OrderDiscount = order.OrderDiscount;
                customerOrder.OrderId = order.Id;
                customerOrder.OrderGuid = order.OrderGuid;
                customerOrder.OrderShippingExclTax = order.OrderShippingExclTax;
                customerOrder.OrderShippingInclTax = order.OrderShippingInclTax;
                customerOrder.OrderStatus = order.OrderStatus;
                customerOrder.OrderStatusText = _localizationService.GetLocalizedEnum(order.OrderStatus);
                customerOrder.OrderSubtotalExclTax = order.OrderSubtotalExclTax;
                customerOrder.OrderSubtotalInclTax = order.OrderSubtotalInclTax;
                customerOrder.OrderTax = order.OrderTax;
                customerOrder.OrderTotal = order.OrderTotal;
                customerOrder.PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax;
                customerOrder.PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax;
                customerOrder.PaymentMethodSystemName = order.PaymentMethodSystemName;
                customerOrder.PaymentStatus = order.PaymentStatus;
                customerOrder.ShippingAddressId = order.ShippingAddressId;
                customerOrder.ShippingMethod = order.ShippingMethod;
                customerOrder.ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName;
                customerOrder.ShippingStatus = order.ShippingStatus;
                customerOrder.TaxRates = order.TaxRates;
                customerOrder.VatNumber = order.VatNumber;
                customerOrder.CheckoutAttributeDescription = order.CheckoutAttributeDescription;
                customerOrder.CheckoutAttributesXml = order.CheckoutAttributesXml;
                customerOrder.ShippingAddress = new AddressResult();
                if (order.ShippingAddress != null)
                {
                    customerOrder.ShippingAddress.Address1 = order.ShippingAddress.Address1;
                    customerOrder.ShippingAddress.Address2 = order.ShippingAddress.Address2;
                    customerOrder.ShippingAddress.City = order.ShippingAddress.City;
                    customerOrder.ShippingAddress.Company = order.ShippingAddress.Company;
                    var country = _countryService.GetCountryById((int)order.ShippingAddress.CountryId);
                    customerOrder.ShippingAddress.CountryName = country == null ? "" : country.Name;
                    customerOrder.ShippingAddress.Email = order.ShippingAddress.Email;
                    customerOrder.ShippingAddress.FirstName = order.ShippingAddress.FirstName;
                    customerOrder.ShippingAddress.LastName = order.ShippingAddress.LastName;
                    var state  = _stateProvinceService.GetStateProvinceById((int)order.ShippingAddress.StateProvinceId);
                    customerOrder.ShippingAddress.StateProvinceName = state == null ? "" : state.Name;
                    customerOrder.ShippingAddress.ZipPostalCode = order.ShippingAddress.ZipPostalCode;
                    customerOrder.ShippingAddress.PhoneNumber = order.ShippingAddress.PhoneNumber;
                    customerOrder.ShippingAddress.FaxNumber = order.ShippingAddress.FaxNumber;
                }
                if (order.BillingAddress != null)
                {
                    customerOrder.BillingAddress = new AddressResult();
                    customerOrder.BillingAddress.Address1 = order.BillingAddress.Address1;
                    customerOrder.BillingAddress.City = order.BillingAddress.City;
                    customerOrder.BillingAddress.Company = order.BillingAddress.Company;
                    var country = _countryService.GetCountryById((int)order.BillingAddress.CountryId);
                    customerOrder.BillingAddress.CountryName = order.BillingAddress.CountryId == null ? "" : country.Name;
                    customerOrder.BillingAddress.Email = order.BillingAddress.Email;
                    customerOrder.BillingAddress.FirstName = order.BillingAddress.FirstName;
                    customerOrder.BillingAddress.LastName = order.BillingAddress.LastName;
                    var state = _stateProvinceService.GetStateProvinceById((int)order.BillingAddress.StateProvinceId);
                    customerOrder.BillingAddress.StateProvinceName = state == null ? "" : state.Name;
                    customerOrder.BillingAddress.ZipPostalCode = order.BillingAddress.ZipPostalCode;
                }
                customerOrders.Add(customerOrder);
            }
            return customerOrders;
        }

        public OrderResult GetOrderDetails(int orderId)
        {
            if (!_customerService.IsRegistered(this._workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            Order order = this._orderService.GetOrderById(orderId);
            if (!this.IsValidOrder(order))
                throw new ApplicationException("Not found");
            var orderItems = _orderService.GetOrderItems(order.Id);
            List<OrderProductResult> products = new List<OrderProductResult>();
            foreach (OrderItem item in orderItems)
            {
                item.Product = _productService.GetProductById(item.ProductId);
                //Picture itemPicture = _pictureService.GetProductPicture(item.Product, item.AttributesXml);

                var orderProductResult = new OrderProductResult
                {
                    OrderId = item.OrderId,
                    ProductId = item.ProductId
                };
                int pictureId = 0;
                if (item.Product.ProductPictures.Count > 0)
                {
                    pictureId = item.Product.ProductPictures.FirstOrDefault().PictureId;
                }
                Picture pictureById = _pictureService.GetPictureById(pictureId);
                if (pictureById == null)
                {
                    pictureById = _pictureService.GetPicturesByProductId(item.Product.Id, 1).FirstOrDefault();
                }
                string imageUrl = "";
                if (pictureById != null)
                {
                    imageUrl = _pictureService.GetPictureUrl(pictureById.Id, storeLocation: _storeContext.CurrentStore.Url,
                        targetSize: _mediaSettings.ProductThumbPictureSize);
                }
                orderProductResult.ImageUrl = imageUrl;
                orderProductResult.ProductName =_localizationService.GetLocalized(item.Product,x => x.Name);
                orderProductResult.Quantity = item.Quantity;
                orderProductResult.SKU = item.Product.Sku;
                this.PrepareOrderProductPrice(orderProductResult, order, item);
                if (item.Product.VendorId > 0)
                {
                    Vendor vendor = _vendorService.GetVendorById(item.Product.VendorId);
                    if (vendor != null)
                    {
                        var vendorResult = new VendorResult();
                        vendorResult.Name = vendor.Name;
                        vendorResult.Email = vendor.Email;
                        //vendorResult.PhoneNumber = vendor.PhoneNumber;
                        orderProductResult.Vendor = vendorResult;
                    }
                }
                products.Add(orderProductResult);
            }
            Currency currency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
            string currencyCode = _localizationService.GetLocalized(currency,c => c.Name);
            string orderShipping;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //order shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                orderShipping = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, currencyCode, _workContext.WorkingLanguage.Id, true);
            }
            else
            {
                //excluding tax

                //order shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                orderShipping = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, currencyCode, _workContext.WorkingLanguage.Id, false);
            }

            var createdOnUtc = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            order.ShippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);
            order.BillingAddress = _addressService.GetAddressById(order.BillingAddressId);

            //var country = _countryService.GetCountryById((int)order.ShippingAddress.CountryId);
            //var state = _stateProvinceService.GetStateProvinceById((int)order.BillingAddress.StateProvinceId);
            //string area = _constantService.GetAllConstants().Where(c =>
            //c.GroupName == "AR" && c.Id == order.BillingAddress.Area);
            OrderResult customerOrder = new OrderResult
            {
                BillingAddressId = order.BillingAddressId,
                CreatedOnUtc = createdOnUtc.ToString("dd", new CultureInfo(_workContext.WorkingLanguage.LanguageCulture)),
                //CreatedOnUtcMonth = createdOnUtc.ToString("MMM", new CultureInfo(_workContext.WorkingLanguage.LanguageCulture)),
                //CreatedOnUtcYear = createdOnUtc.ToString("yyyy", new CultureInfo(_workContext.WorkingLanguage.LanguageCulture)),
                CurrencyRate = order.CurrencyRate,
                CustomerCurrencyCode = currencyCode,
                CustomerId = order.CustomerId,
                CustomerTaxDisplayTypeId = order.CustomerTaxDisplayTypeId,
                Deleted = order.Deleted,
                OrderDiscount = order.OrderDiscount,
                OrderId = order.Id,
                OrderGuid = order.OrderGuid,
                OrderShippingExclTax = order.OrderShippingExclTax,
                OrderShippingInclTax = order.OrderShippingInclTax,
                OrderShipping = orderShipping,
                OrderStatus = order.OrderStatus,
                OrderStatusText =_localizationService.GetLocalizedEnum(order.OrderStatus),
                OrderSubtotalExclTax = order.OrderSubtotalExclTax,
                OrderSubtotalInclTax = order.OrderSubtotalInclTax,
                OrderTax = order.OrderTax,
                OrderTotal = order.OrderTotal,
                PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax,
                PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax,
                PaymentMethodSystemName = order.PaymentMethodSystemName,
                PaymentStatus = order.PaymentStatus,
                ShippingAddressId = order.ShippingAddressId,
                ShippingMethod = order.ShippingMethod,
                ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName,
                ShippingStatus = order.ShippingStatus,
                TaxRates = order.TaxRates,
                VatNumber = order.VatNumber,
                CheckoutAttributeDescription = order.CheckoutAttributeDescription,
                CheckoutAttributesXml = order.CheckoutAttributesXml,
                ShippingAddress = new AddressResult()
                {
                    Address1 = order.ShippingAddress.Address1,
                    Address2 = order.ShippingAddress.Address2,

                    City = order.ShippingAddress.City,
                    Company = order.ShippingAddress.Company,
                    CountryName = order.ShippingAddress.CountryId == null ? "" :_localizationService.GetLocalized(_countryService.GetCountryById((int)order.ShippingAddress.CountryId),x=>x.Name,_workContext.WorkingLanguage.Id),
                   
                    Email = order.ShippingAddress.Email,
                    FirstName = order.ShippingAddress.FirstName,
                    LastName = order.ShippingAddress.LastName,
                    StateProvinceName = order.ShippingAddress.StateProvinceId == null ? "" : _localizationService.GetLocalized(_stateProvinceService.GetStateProvinceById((int)order.ShippingAddress.StateProvinceId),x=>x.Name,_workContext.WorkingLanguage.Id),
                    ZipPostalCode = order.ShippingAddress.ZipPostalCode,
                    PhoneNumber = order.ShippingAddress.PhoneNumber,
                    FaxNumber = order.ShippingAddress.FaxNumber
                },
                BillingAddress = new AddressResult
                {
                    Address1 = order.BillingAddress.Address1,
                    City = order.BillingAddress.City,
                    Company = order.BillingAddress.Company,
                    CountryName = order.BillingAddress.CountryId == null ? "" : _localizationService.GetLocalized(_countryService.GetCountryById((int)order.BillingAddress.CountryId), x => x.Name, _workContext.WorkingLanguage.Id),
                    Email = order.BillingAddress.Email,
                    FirstName = order.BillingAddress.FirstName,
                    LastName = order.BillingAddress.LastName,
                    StateProvinceName = order.BillingAddress.StateProvinceId == null ? "" : _localizationService.GetLocalized(_stateProvinceService.GetStateProvinceById((int)order.BillingAddress.StateProvinceId),x=>x.Name,_workContext.WorkingLanguage.Id),
                    ZipPostalCode = order.BillingAddress.ZipPostalCode
                },
                OrderProducts = products,
            };
            var shipments = order.Shipments.OrderBy(s => s.CreatedOnUtc)
                .Select(shipment => new ShipmentResult
                {
                    Id = shipment.Id,
                    TrackingNumber = shipment.TrackingNumber,
                        //TotalWeight = shipment.TotalWeight.HasValue ? string.Format("{0:F2} [{1}]", shipment.TotalWeight, baseWeightIn) : "",
                        ShippedDate = shipment.ShippedDateUtc.HasValue
                    ? _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc).ToShortDateString()
                    : _localizationService.GetResource("Admin.Orders.Shipments.ShippedDate.NotYet"),
                    ShippedDateUtc = shipment.ShippedDateUtc,
                    CanShip = !shipment.ShippedDateUtc.HasValue,
                    DeliveryDate = shipment.DeliveryDateUtc.HasValue
                        ? _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc).ToShortDateString()
                        : _localizationService.GetResource("Admin.Orders.Shipments.DeliveryDate.NotYet"),
                    DeliveryDateUtc = shipment.DeliveryDateUtc,
                    CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue,
                    AdminComment = shipment.AdminComment,
                }).ToList();
            if (shipments.Count > 0)
                customerOrder.Shipments = shipments;

            customerOrder.CanPlaceBlackPoint = _blackPointService.CanPlaceBlackPoint(_workContext.CurrentCustomer, order);
            return customerOrder;
        }

        public bool CancelOrder(int orderId)
        {
            if (!_customerService.IsRegistered(this._workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            Order order = this._orderService.GetOrderById(orderId);
            if (order == null)
                throw new ApplicationException("You can just cancel pending order");

            else if (order.OrderStatusId != 10)
                throw new ApplicationException("Not found");


             _orderProcessingService.CancelOrder(order, false);
            OrderResult response = new OrderResult{ };
            return true;
        }
        public virtual void UpdateOrderItemByProductIdAnCustomerId(int productId, int customerId, int orderId)
        {
            _orderService.UpdateOrderItemByProductIdAnCustomerId(productId, customerId, orderId);
        }
        private bool IsValidOrder(Order order)
        {
            return order != null && !order.Deleted && this._workContext.CurrentCustomer.Id == order.CustomerId;
        }

        private void PrepareOrderProductPrice(OrderProductResult orderProduct, Order order, OrderItem orderItem)
        {
            Currency currency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
            if (order.CustomerTaxDisplayType != 0)
            {
                decimal unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                orderProduct.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, _localizationService.GetLocalized(currency,c => c.Name), _workContext.WorkingLanguage.Id, false);
                decimal priceExclTaxInCustomerCurrency = this._currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                orderProduct.SubTotal = this._priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, _localizationService.GetLocalized(currency,c => c.Name), this._workContext.WorkingLanguage.Id, false);
            }
            else
            {
                decimal unitPriceInclTaxInCustomerCurrency = this._currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                orderProduct.UnitPrice = this._priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, _localizationService.GetLocalized(currency,c => c.Name), this._workContext.WorkingLanguage.Id, true);
                decimal priceInclTaxInCustomerCurrency = this._currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                orderProduct.SubTotal = this._priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, _localizationService.GetLocalized(currency,c => c.Name), this._workContext.WorkingLanguage.Id, true);
            }
        }

    }
}
