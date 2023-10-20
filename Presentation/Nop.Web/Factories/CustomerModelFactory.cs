using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Constant;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Constants;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.lookup;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly ICountryService _countryService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IConstantService _constantService;
        private readonly ILookupService _lookupService;
        protected readonly IVendorService _vendorService;
        private readonly ICategoryService _categoryService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        #endregion

        #region Ctor

        public CustomerModelFactory(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressModelFactory addressModelFactory,
            IAuthenticationPluginManager authenticationPluginManager,
            ICountryService countryService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOrderService orderService,
            IPictureService pictureService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            SecuritySettings securitySettings,
            TaxSettings taxSettings,
            IConstantService constantService,
            ICategoryService categoryService,
            VendorSettings vendorSettings, ILookupService lookupService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IVendorService vendorService)
        {
            _localizationSettings = localizationSettings;
            _workflowMessageService = workflowMessageService;
            _categoryService = categoryService;
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _externalAuthenticationService = externalAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _addressModelFactory = addressModelFactory;
            _authenticationPluginManager = authenticationPluginManager;
            _countryService = countryService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _orderService = orderService;
            _pictureService = pictureService;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _securitySettings = securitySettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _constantService = constantService;
            _lookupService = lookupService;
            _vendorService = vendorService;
        }

        #endregion

        #region Utilities

        protected virtual GdprConsentModel PrepareGdprConsentModel(GdprConsent consent, bool accepted)
        {
            if (consent == null)
                throw new ArgumentNullException(nameof(consent));

            var requiredMessage = _localizationService.GetLocalized(consent, x => x.RequiredMessage);
            return new GdprConsentModel
            {
                Id = consent.Id,
                Message = _localizationService.GetLocalized(consent, x => x.Message),
                IsRequired = consent.IsRequired,
                RequiredMessage = !string.IsNullOrEmpty(requiredMessage) ? requiredMessage : $"'{consent.Message}' is required",
                Accepted = accepted
            };
        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare the custom customer attribute models
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="overrideAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <returns>List of the customer attribute model</returns>
        public virtual IList<CustomerAttributeModel> PrepareCustomCustomerAttributes(Customer customer, string overrideAttributesXml = "")
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var result = new List<CustomerAttributeModel>();

            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = _localizationService.GetLocalized(attribute, x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = _localizationService.GetLocalized(attributeValue, x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                //set already selected attributes
                var selectedAttributesXml = !string.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CustomCustomerAttributes);
                 
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(selectedAttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(selectedAttributesXml);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!string.IsNullOrEmpty(selectedAttributesXml))
                            {
                                var enteredText = _customerAttributeParser.ParseValues(selectedAttributesXml, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                result.Add(attributeModel);
            }

            return result;
        }

        /// <summary>
        /// Prepare the customer info model
        /// </summary>
        /// <param name="model">Customer info model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <returns>Customer info model</returns>
        public virtual CustomerInfoModel PrepareCustomerInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            if (!excludeProperties)
            {
                model.VatNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.VatNumberAttribute);
                model.FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                model.LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
                model.Gender = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.GenderAttribute);
                var dateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(customer, NopCustomerDefaults.DateOfBirthAttribute);
                if (dateOfBirth.HasValue)
                {
                    DateTime dt = (DateTime)dateOfBirth;
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                    model.DateOfBirth = dt.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
                }

                model.Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute);
                model.StreetAddress = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddressAttribute);
                model.StreetAddress2 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddress2Attribute);
                model.ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);
                model.City = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CityAttribute);
                model.Area = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.Area);

                model.County = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CountyAttribute);
                model.CountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute);
                model.StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute);
                model.Phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
                model.Fax = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FaxAttribute);

                //newsletter
                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                model.Newsletter = newsletter != null && newsletter.Active;

                model.Signature = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.SignatureAttribute);

                model.Email = customer.Email;
                model.Username = customer.Username;
            }
            else
            {
                if (_customerSettings.UsernamesEnabled && !_customerSettings.AllowUsersToChangeUsernames)
                    model.Username = customer.Username;
            }

            if (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation)
                model.EmailToRevalidate = customer.EmailToRevalidate;

            //countries and states

            model.CountryNationalityId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.NationalityCountryId);
            model.AvailableNationalities.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
            {
                model.AvailableNationalities.Add(new SelectListItem
                {
                    Text = _localizationService.GetLocalized(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryNationalityId
                });
            }

            //Reasons
            List<string> typeslist = new List<string>();
            typeslist.Add("DeleteAccountList");
            typeslist.Add("DeleteAccountTC");
            string[] types = typeslist.ToArray();
            var lookups = _lookupService.GetAllLookups(types, _workContext.WorkingLanguage.Id);
            var listReason = lookups.Where(x => x.Type == "DeleteAccountList").Select(x => new SelectListItem { 
            
              Text = x.Value,
              Value= x.Id.ToString(),
              Selected = x.Id == model.DeleteCustomerAccountModel.ReasonId
            }).ToList();
            //Term and conditions
            model.DeleteCustomerAccountModel.TermsnCondition = lookups.Where(x => x.Type == "DeleteAccountTC").Select(x => x.Value).ToList();
            //isRequestedforDelete
            model.isRequestedforDelete = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute)==null?false: _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute);
            model.DeleteCustomerAccountModel.AvailableReasons = listReason;

            model.DeleteCustomerAccountModel.IsDeleted = customer.Deleted;
              
            
            //foreach (var reason in _lookupService.GetAllLookups(types, _workContext.WorkingLanguage.Id))
            //{
            //    model.DeleteCustomerAccountModel.AvailableReasons.Add(new SelectListItem
            //    {
            //        Text = reason.Value,
            //        Value = reason.Id.ToString(),
            //        Selected = reason.Id == model.DeleteCustomerAccountModel.ReasonId
            //    });
            //}

            //states
            var stateProvince = _stateProvinceService.GetStateProvincesByCountryId(81, _workContext.WorkingLanguage.Id).ToList();
            if (stateProvince.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                foreach (var s in stateProvince)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetLocalized(s, x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                }
            }
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.CountryId = 81; //uae
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = _localizationService.GetLocalized(c, x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(81, _workContext.WorkingLanguage.Id).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetLocalized(s, x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                            Value = "0"
                        });
                    }

                }
            }
            IPagedList<Constant> constants = _constantService.GetAllConstants();

            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            IEnumerable<Constant> areas = constants.Where(c => c.GroupName == "AR" && c.EmirateId == model.StateProvinceId.ToString());
            foreach (Constant area in areas)
            {
                model.AvailableAreas.Add(new SelectListItem
                {
                    Text = _workContext.WorkingLanguage.Rtl ? area.ArabicName : area.EnglishName,
                    Value = area.Code,
                    Selected = model.Area == area.Code
                });
            }


            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = _localizationService.GetLocalizedEnum((VatNumberStatus)_genericAttributeService
                .GetAttribute<int>(customer, NopCustomerDefaults.VatNumberStatusIdAttribute));
            model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            model.LastNameEnabled = _customerSettings.LastNameEnabled;
            model.FirstNameRequired = _customerSettings.FirstNameRequired;
            model.LastNameRequired = _customerSettings.LastNameRequired;
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountyEnabled = _customerSettings.CountyEnabled;
            model.CountyRequired = _customerSettings.CountyRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.CountryRequired = _customerSettings.CountryRequired;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;

            //external authentication
            model.AllowCustomersToRemoveAssociations = _externalAuthenticationSettings.AllowCustomersToRemoveAssociations;
            model.NumberOfExternalAuthenticationProviders = _authenticationPluginManager
                .LoadActivePlugins(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id)
                .Count;
            foreach (var record in _externalAuthenticationService.GetCustomerExternalAuthenticationRecords(customer))
            {
                if (record.ProviderSystemName == Provider.SystemName)
                {
                    model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel
                    {
                        Id = record.Id,
                        Email = record.Email,
                        ExternalIdentifier = record.ExternalIdentifier,
                        AuthMethodName = record.ProviderSystemName
                    });
                }

                var authMethod = _authenticationPluginManager
                    .LoadPluginBySystemName(record.ProviderSystemName, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id);
                if (!_authenticationPluginManager.IsPluginActive(authMethod))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel
                {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = !string.IsNullOrEmpty(record.ExternalDisplayIdentifier)
                        ? record.ExternalDisplayIdentifier : record.ExternalIdentifier,
                    AuthMethodName = _localizationService.GetLocalizedFriendlyName(authMethod, _workContext.WorkingLanguage.Id)
                });
            }

            //custom customer attributes
            var customAttributes = PrepareCustomCustomerAttributes(customer, overrideCustomCustomerAttributesXml);
            foreach (var attribute in customAttributes)
                model.CustomerAttributes.Add(attribute);

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var accepted = _gdprService.IsConsentAccepted(consent.Id, _workContext.CurrentCustomer.Id);
                    model.GdprConsents.Add(PrepareGdprConsentModel(consent, accepted.HasValue && accepted.Value));
                }
            }
            // get remaing review product
            model.customerProductReviewModels = this.GetProductsForReviews();

            return model;
        }

        /// <summary>
        /// Prepare the customer register model
        /// </summary>
        /// <param name="model">Customer register model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
        /// <returns>Customer register model</returns>
        public virtual RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //form fields
            model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            model.LastNameEnabled = _customerSettings.LastNameEnabled;
            model.FirstNameRequired = _customerSettings.FirstNameRequired;
            model.LastNameRequired = _customerSettings.LastNameRequired;
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountyEnabled = _customerSettings.CountyEnabled;
            model.CountyRequired = _customerSettings.CountyRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.CountryRequired = _customerSettings.CountryRequired;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
            model.AcceptPrivacyPolicyPopup = _commonSettings.PopupForTermsOfServiceLinks;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.HoneypotEnabled = _securitySettings.HoneypotEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;
            model.EnteringEmailTwice = _customerSettings.EnteringEmailTwice;
            if (setDefaultValues)
            {
                //enable newsletter by default
                model.Newsletter = _customerSettings.NewsletterTickedByDefault;
            }

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });

                foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = _localizationService.GetLocalized(c, x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetLocalized(s, x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                            Value = "0"
                        });
                    }

                }
            }

            //custom customer attributes
            var customAttributes = PrepareCustomCustomerAttributes(_workContext.CurrentCustomer, overrideCustomCustomerAttributesXml);
            foreach (var attribute in customAttributes)
                model.CustomerAttributes.Add(attribute);

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayDuringRegistration).ToList();
                foreach (var consent in consents)
                {
                    model.GdprConsents.Add(PrepareGdprConsentModel(consent, false));
                }
            }

            var category = _categoryService.GetAllCategories().Where(c => c.IncludeInTopMenu).ToList<Category>();

           // model.Categories.Add(new SelectListItem { Text = _localizationService.GetResource("register.selectcategory"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories().Where(c => c.IncludeInTopMenu).ToList<Category>())
            {
                model.Categories.Add(new SelectListItem
                {
                    Text = _localizationService.GetLocalized(c, x => x.Name),
                    Value = c.Id.ToString()
                });
            }
            

            return model;
        }

        /// <summary>
        /// Prepare the login model
        /// </summary>
        /// <param name="checkoutAsGuest">Whether to checkout as guest is enabled</param>
        /// <returns>Login model</returns>
        public virtual LoginModel PrepareLoginModel(bool? checkoutAsGuest)
        {
            var model = new LoginModel
            {
                UsernamesEnabled = _customerSettings.UsernamesEnabled,
                RegistrationType = _customerSettings.UserRegistrationType,
                CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault(),
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage

            };

            return model;
        }

        /// <summary>
        /// Prepare the password recovery model
        /// </summary>
        /// <param name="model">Password recovery model</param>
        /// <returns>Password recovery model</returns>
        public virtual PasswordRecoveryModel PreparePasswordRecoveryModel(PasswordRecoveryModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnForgotPasswordPage;
            
            return model;
        }

        /// <summary>
        /// Prepare the password recovery confirm model
        /// </summary>
        /// <returns>Password recovery confirm model</returns>
        public virtual PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel()
        {
            var model = new PasswordRecoveryConfirmModel();
            return model;
        }

        /// <summary>
        /// Prepare the register result model
        /// </summary>
        /// <param name="resultId">Value of UserRegistrationType enum</param>
        /// <returns>Register result model</returns>
        public virtual RegisterResultModel PrepareRegisterResultModel(int resultId)
        {
            var resultText = "";
            switch ((UserRegistrationType)resultId)
            {
                case UserRegistrationType.Disabled:
                    resultText = _localizationService.GetResource("Account.Register.Result.Disabled");
                    break;
                case UserRegistrationType.Standard:
                    resultText = _localizationService.GetResource("Account.Register.Result.Standard");
                    break;
                case UserRegistrationType.AdminApproval:
                    resultText = _localizationService.GetResource("Account.Register.Result.AdminApproval");
                    break;
                case UserRegistrationType.EmailValidation:
                    resultText = _localizationService.GetResource("Account.Register.Result.EmailValidation");
                    break;
                default:
                    break;
            }
            var model = new RegisterResultModel
            {
                Result = resultText
            };
            return model;
        }

        /// <summary>
        /// Prepare the customer navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>Customer navigation model</returns>
        public virtual CustomerNavigationModel PrepareCustomerNavigationModel(int selectedTabId = 0)
        {
            var model = new CustomerNavigationModel();

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerInfo",
                Title = _localizationService.GetResource("Account.CustomerInfo"),
                Tab = CustomerNavigationEnum.Info,
                ItemClass = "customer-info"
            });

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerAddresses",
                Title = _localizationService.GetResource("Account.CustomerAddresses"),
                Tab = CustomerNavigationEnum.Addresses,
                ItemClass = "customer-addresses"
            });

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerOrders",
                Title = _localizationService.GetResource("Account.CustomerOrders"),
                Tab = CustomerNavigationEnum.Orders,
                ItemClass = "customer-orders"
            });

            if (_orderSettings.ReturnRequestsEnabled &&
                _returnRequestService.SearchReturnRequests(_storeContext.CurrentStore.Id,
                    _workContext.CurrentCustomer.Id, pageIndex: 0, pageSize: 1).Any())
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerReturnRequests",
                    Title = _localizationService.GetResource("Account.CustomerReturnRequests"),
                    Tab = CustomerNavigationEnum.ReturnRequests,
                    ItemClass = "return-requests"
                });
            }

            if (!_customerSettings.HideDownloadableProductsTab)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerDownloadableProducts",
                    Title = _localizationService.GetResource("Account.DownloadableProducts"),
                    Tab = CustomerNavigationEnum.DownloadableProducts,
                    ItemClass = "downloadable-products"
                });
            }

            if (!_customerSettings.HideBackInStockSubscriptionsTab)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerBackInStockSubscriptions",
                    Title = _localizationService.GetResource("Account.BackInStockSubscriptions"),
                    Tab = CustomerNavigationEnum.BackInStockSubscriptions,
                    ItemClass = "back-in-stock-subscriptions"
                });
            }

            if (_rewardPointsSettings.Enabled)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerRewardPoints",
                    Title = _localizationService.GetResource("Account.RewardPoints"),
                    Tab = CustomerNavigationEnum.RewardPoints,
                    ItemClass = "reward-points"
                });
            }

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerChangePassword",
                Title = _localizationService.GetResource("Account.ChangePassword"),
                Tab = CustomerNavigationEnum.ChangePassword,
                ItemClass = "change-password"
            });

            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerAvatar",
                    Title = _localizationService.GetResource("Account.Avatar"),
                    Tab = CustomerNavigationEnum.Avatar,
                    ItemClass = "customer-avatar"
                });
            }

            if (_forumSettings.ForumsEnabled && _forumSettings.AllowCustomersToManageSubscriptions)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerForumSubscriptions",
                    Title = _localizationService.GetResource("Account.ForumSubscriptions"),
                    Tab = CustomerNavigationEnum.ForumSubscriptions,
                    ItemClass = "forum-subscriptions"
                });
            }
            if (_catalogSettings.ShowProductReviewsTabOnAccountPage)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerProductReviews",
                    Title = _localizationService.GetResource("Account.CustomerProductReviews"),
                    Tab = CustomerNavigationEnum.ProductReviews,
                    ItemClass = "customer-reviews"
                });
            }
            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "ApplyFoundation",
                Title = _localizationService.GetResource("pagetitle.vendors.applyfoundation"),
                Tab = CustomerNavigationEnum.ApplyFoundation,
                ItemClass = "apply-for-foundation"
            });
            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "FollowedVendors",
                Title = _localizationService.GetResource("account.followedbrands"),
                Tab = CustomerNavigationEnum.FollowedVendors,
                ItemClass = "followed-vendors"
            });
            if (_vendorSettings.AllowVendorsToEditInfo && _workContext.CurrentVendor != null)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerVendorInfo",
                    Title = _localizationService.GetResource("Account.VendorInfo"),
                    Tab = CustomerNavigationEnum.VendorInfo,
                    ItemClass = "customer-vendor-info"
                });
            }
            if (!_customerService.IsAdmin(_workContext.CurrentCustomer))
            {
                if (_vendorSettings.AllowVendorsToEditInfo && _workContext.CurrentCustomer.VendorId != 0)
                {
                    model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                    {
                        RouteName = "Tradelicenseinfo",
                        Title = _localizationService.GetResource("account.licenseinfo"),
                        Tab = CustomerNavigationEnum.LicenseInfo,
                        ItemClass = "vendor-license-info"
                    });
                }
            }
           
            if (_gdprSettings.GdprEnabled)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "GdprTools",
                    Title = _localizationService.GetResource("Account.Gdpr"),
                    Tab = CustomerNavigationEnum.GdprTools,
                    ItemClass = "customer-gdpr"
                });
            }

            if (_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CheckGiftCardBalance",
                    Title = _localizationService.GetResource("CheckGiftCardBalance"),
                    Tab = CustomerNavigationEnum.CheckGiftCardBalance,
                    ItemClass = "customer-check-gift-card-balance"
                });
            }
            bool IsAllowed = true;
            var DeleteRerquest = _genericAttributeService.GetAttribute<bool>(_workContext.CurrentCustomer, NopCustomerDefaults.customerDeleteAccountAttribute);
            var vendor = _workContext.CurrentCustomer.VendorId > 0 ? _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId) : null;
            if (DeleteRerquest)
            {
                IsAllowed = false;
            }
            //if(_customerService.IsVendor(_workContext.CurrentCustomer))
            //{
            //    if (vendor != null)
            //    {
            //        if (vendor.Deleted && !_workContext.CurrentCustomer.Deleted)
            //        {
            //            IsAllowed = true;
            //        }
            //    }

            //}
            if (IsAllowed)
            {
                if (!_customerService.IsAdmin(_workContext.CurrentCustomer))
                {
                    model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                    {
                        RouteName = "DeleteAccount",
                        Title = _localizationService.GetResource("account.deleteaccount"),
                        Tab = CustomerNavigationEnum.DeleteAccount,
                        ItemClass = "delete-account"
                    });
                    model.SelectedTab = (CustomerNavigationEnum)selectedTabId;
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare the customer address list model
        /// </summary>
        /// <returns>Customer address list model</returns>
        public virtual CustomerAddressListModel PrepareCustomerAddressListModel()
        {
            var addresses = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id)
                //enabled for the current store
                .Where(a => a.CountryId == null || _storeMappingService.Authorize(_countryService.GetCountryByAddress(a)))
                .ToList();

            var model = new CustomerAddressListModel();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressModelFactory.PrepareAddressModel(addressModel,
                    address: address,
                    customer:_workContext.CurrentCustomer,
                    excludeProperties: false,
                    addressSettings: _addressSettings,
                    loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));
               
                model.Addresses.Add(addressModel);
            }
            model.Addresses= model.Addresses.OrderByDescending(u => u.IsDefault).ToList();
            return model;
        }

        /// <summary>
        /// Prepare the customer downloadable products model
        /// </summary>
        /// <returns>Customer downloadable products model</returns>
        public virtual CustomerDownloadableProductsModel PrepareCustomerDownloadableProductsModel()
        {
            var model = new CustomerDownloadableProductsModel();
            var items = _orderService.GetDownloadableOrderItems(_workContext.CurrentCustomer.Id);
            foreach (var item in items)
            {
                var order = _orderService.GetOrderById(item.OrderId);
                var product = _productService.GetProductById(item.ProductId);

                var itemModel = new CustomerDownloadableProductsModel.DownloadableProductsModel
                {
                    OrderItemGuid = item.OrderItemGuid,
                    OrderId = order.Id,
                    CustomOrderNumber = order.CustomOrderNumber,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    ProductName = _localizationService.GetLocalized(product, x => x.Name),
                    ProductSeName = _urlRecordService.GetSeName(product),
                    ProductAttributes = item.AttributeDescription,
                    ProductId = item.ProductId
                };
                model.Items.Add(itemModel);

                if (_orderService.IsDownloadAllowed(item))
                    itemModel.DownloadId = product.DownloadId;

                if (_orderService.IsLicenseDownloadAllowed(item))
                    itemModel.LicenseId = item.LicenseDownloadId ?? 0;
            }

            return model;
        }

        /// <summary>
        /// Prepare the user agreement model
        /// </summary>
        /// <param name="orderItem">Order item</param>
        /// <param name="product">Product</param>
        /// <returns>User agreement model</returns>
        public virtual UserAgreementModel PrepareUserAgreementModel(OrderItem orderItem, Product product)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new UserAgreementModel
            {
                UserAgreementText = product.UserAgreementText,
                OrderItemGuid = orderItem.OrderItemGuid
            };

            return model;
        }

        /// <summary>
        /// Prepare the change password model
        /// </summary>
        /// <returns>Change password model</returns>
        public virtual ChangePasswordModel PrepareChangePasswordModel()
        {
            var model = new ChangePasswordModel();
            return model;
        }

        /// <summary>
        /// Prepare the customer avatar model
        /// </summary>
        /// <param name="model">Customer avatar model</param>
        /// <returns>Customer avatar model</returns>
        public virtual CustomerAvatarModel PrepareCustomerAvatarModel(CustomerAvatarModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvatarUrl = _pictureService.GetPictureUrl(
                _genericAttributeService.GetAttribute<int>(_workContext.CurrentCustomer, NopCustomerDefaults.AvatarPictureIdAttribute),
                _mediaSettings.AvatarPictureSize,
                false);

            return model;
        }

        /// <summary>
        /// Prepare the GDPR tools model
        /// </summary>
        /// <returns>GDPR tools model</returns>
        public virtual GdprToolsModel PrepareGdprToolsModel()
        {
            var model = new GdprToolsModel();
            return model;
        }

        /// <summary>
        /// Prepare the check gift card balance madel
        /// </summary>
        /// <returns>Check gift card balance madel</returns>
        public virtual CheckGiftCardBalanceModel PrepareCheckGiftCardBalanceModel()
        {
            var model = new CheckGiftCardBalanceModel();
            return model;
        }

        public DeleteCustomerAccountModel PrepareDeleteCustomerAccountModel(DeleteCustomerAccountModel model, Customer customer)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            bool accountDeleteStatus = false;
            if (_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                var vendor = _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId);
                accountDeleteStatus = _genericAttributeService.GetAttribute<bool>(vendor, NopCustomerDefaults.customerDeleteAccountAttribute);
            }
            else
            {
                accountDeleteStatus = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute);
            }
               

            if (accountDeleteStatus)
                throw new ArgumentNullException(nameof(model));
            else
            {

                var reason = _lookupService.GetlookupById(model.ReasonId, _workContext.WorkingLanguage.Id, "DeleteAccountList");

                if (model.ReasonId == 5)
                {
                    reason.Value = model.Comments;
                }
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountAttribute, true);
                if (_customerService.IsVendor(_workContext.CurrentCustomer))
                {
                    var vendor = _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId);
                    _genericAttributeService.SaveAttribute(vendor, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                    _genericAttributeService.SaveAttribute(vendor, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                    _genericAttributeService.SaveAttribute(vendor, NopCustomerDefaults.customerDeleteAccountCommentsAttribute, model.Comments);

                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountCommentsAttribute, model.Comments);
                }
                else
                {
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountCommentsAttribute, model.Comments);
                }
                
            }
            model.IsDeleted = true;

            return model;
        }
        
       public  List<CustomerProductReviewModel> GetProductsForReviews()
        {
            var products = _productService.GetCustomerLastProductOrderItem(customerId: _workContext.CurrentCustomer.Id, approved: true, storeId: _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : 0).AsEnumerable();
            //fillter products witout Review 

            List<CustomerProductReviewModel> productForReviews = new List<CustomerProductReviewModel>();
            foreach (var _orderItem in products)
            {
                CustomerProductReviewModel vm = new CustomerProductReviewModel();
                var orders = _orderService.OrderCountforproduct(productId: _orderItem.ProductId, customerId: _workContext.CurrentCustomer.Id);
                var userReviews = _productService.GetAllProductReviews(customerId: _workContext.CurrentCustomer.Id, productId: _orderItem.ProductId);
                int totalCustomerReviews = userReviews.TotalCount;

                if (orders <= totalCustomerReviews)
                {
                    continue;
                }
                if (!_orderItem.Isviewed)
                {
                    var pictureById = _pictureService.GetPicturesByProductId(_orderItem.ProductId, 1).FirstOrDefault();

                    string imageUrl = "";
                    if (pictureById != null)
                        imageUrl = _pictureService.GetPictureUrl(pictureById.Id, storeLocation: _storeContext.CurrentStore.Url,
                            targetSize: _mediaSettings.ProductThumbPictureSize);
                    vm.ProductId = _orderItem.ProductId;
                    vm.CustomerId = _workContext.CurrentCustomer.Id;
                    vm.ProductName = _localizationService.GetLocalized(_productService.GetProductById(_orderItem.ProductId), p => p.Name);
                    vm.ProductSeName = _urlRecordService.GetSeName(_productService.GetProductById(_orderItem.ProductId));
                    vm.ProductImage = imageUrl;

                    productForReviews.Add(vm);
                    _orderItem.Isviewed = true;
                    _orderService.UpdateOrderItem(_orderItem);
                }
            }

            return productForReviews;
        }
        bool ICustomerModelFactory.AddCustomerBulkProductReview(List<CustomerProductReviewModelRequest> model)
        {
            foreach(var customerReview in model)
            {
                  this.AddBulkProductReview(customerReview);
            }
            return true;
        }
        public bool AddBulkProductReview(CustomerProductReviewModelRequest model)
        {
            Product product = this._productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
            {
                return false;
            }
            else if (!_customerService.IsGuest(this._workContext.CurrentCustomer) || this._catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
               
                var orders = _orderService.OrderCountforproduct(productId: product.Id, customerId: _workContext.CurrentCustomer.Id);
                var userReviews = _productService.GetAllProductReviews(customerId: _workContext.CurrentCustomer.Id, productId: product.Id);
                int totalCustomerReviews = userReviews.TotalCount;

                //  int totalCustomerReviews = product.ProductReviews.Count(c => c.CustomerId == _workContext.CurrentCustomer.Id);
                if (orders <= totalCustomerReviews)
                {
                    return false;
                }

                    if (model.Rating < 1 || model.Rating > 5)
                    {
                        model.Rating = this._catalogSettings.DefaultProductRatingValue;
                    }
                    bool isApproved = !this._catalogSettings.ProductReviewsMustBeApproved;
                    ProductReview productReview = new ProductReview();
                    productReview.ProductId = product.Id;
                    productReview.CustomerId = this._workContext.CurrentCustomer.Id;
                    //productReview.Title = model.Title;
                    productReview.ReviewText = model.ReviewText;
                    productReview.Rating = model.Rating;
                    productReview.HelpfulYesTotal = 0;
                    productReview.HelpfulNoTotal = 0;
                    productReview.IsApproved = isApproved;
                    productReview.StoreId = _storeContext.CurrentStore.Id;
                    productReview.CreatedOnUtc = DateTime.UtcNow;

                    product.ProductReviews.Add(productReview);
                    _productService.InsertProductReview(productReview);
                    this._productService.UpdateProduct(product);
                    this._productService.UpdateProductReviewTotals(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    _workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);

                return true;
                   
            }

            return false;
        }

        #endregion
    }
}