using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Constants;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.lookup;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Services.TradeLicense;
using Nop.Services.Vendors;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Validators;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class CustomerController : BasePublicController
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly IDownloadService _downloadService;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IExportManager _exportManager;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IConstantService _constantService;
        private readonly CommonSettings _commonSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IVendorFollowerService _vendorFollowerService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IPermissionService _permissionService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly INotificationService _notificationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly ILookupService _lookupService;
        // private readonly IExternalAuthorizer _authorizer;
        private readonly ICategoryService _categoryService;
        private readonly INopFileProvider _fileProvider;
        private readonly ITradelicenseService _tradelicenseService;

        #endregion

        #region Ctor

        public CustomerController(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            IDownloadService downloadService,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ILogger logger,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOrderService orderService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            StoreInformationSettings storeInformationSettings,
            CommonSettings commonSettings,
            IConstantService constantService,
            //IExternalAuthorizer authorizer,
            IStaticCacheManager cacheManager,
            IVendorService vendorService,
            VendorSettings vendorSettings,
            IUrlRecordService urlRecordService,
            IStaticCacheManager staticCacheManager,
            ICacheKeyService cacheKeyService,
            IPermissionService permissionService,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
        TaxSettings taxSettings, IVendorModelFactory vendorModelFactory,
        INotificationService notificationService, ILookupService lookupService,
        ICategoryService categoryService,
        INopFileProvider fileProvider,
        ITradelicenseService tradelicenseService
        )
        {
            _vendorModelFactory = vendorModelFactory;
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _downloadService = downloadService;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _orderService = orderService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;

            this._commonSettings = commonSettings;

            this._constantService = constantService;
            this._cacheManager = cacheManager;
            this._vendorService = vendorService;
            _vendorSettings = vendorSettings;
            _urlRecordService = urlRecordService;
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;
            _permissionService = permissionService;
            _openAuthenticationService = openAuthenticationService;

            _externalAuthenticationSettings = externalAuthenticationSettings;
            _lookupService = lookupService;
            //this._authorizer = authorizer;
            // this._logger = logger;
            this._vendorFollowerService = EngineContext.Current.Resolve<IVendorFollowerService>();

            this._notificationService = notificationService;
            _categoryService = categoryService;
            _fileProvider = fileProvider;
            _tradelicenseService = tradelicenseService;
        }

        #endregion

        #region Utilities

        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
                {
                    ModelState.AddModelError("", consent.RequiredMessage);
                }
            }
        }

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual void LogGdpr(Customer customer, CustomerInfoModel oldCustomerInfoModel,
            CustomerInfoModel newCustomerInfoModel, IFormCollection form)
        {
            try
            {
                //consents
                var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var previousConsentValue = _gdprService.IsConsentAccepted(consent.Id, _workContext.CurrentCustomer.Id);
                    var controlId = $"consent{consent.Id}";
                    var cbConsent = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                        {
                            _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value)
                        {
                            _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                //newsletter subscriptions
                if (_gdprSettings.LogNewsletterConsent)
                {
                    if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentDisagree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                    if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                }

                //user profile changes
                if (!_gdprSettings.LogUserProfileChanges)
                    return;

                if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

                if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

                if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

                if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

                if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

                if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

                if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

                if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

                if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

                if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.City")} = {newCustomerInfoModel.City}");

                if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.County")} = {newCustomerInfoModel.County}");

                if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
                {
                    var countryName = _countryService.GetCountryById(newCustomerInfoModel.CountryId)?.Name;
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Country")} = {countryName}");
                }

                if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
                {
                    var stateProvinceName = _stateProvinceService.GetStateProvinceById(newCustomerInfoModel.StateProvinceId)?.Name;
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.StateProvince")} = {stateProvinceName}");
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message, exception, customer);
            }
        }

        protected virtual UaePassAuthenticationParameters GetUaePassUserAttributesValue(string uuid, string idn, string accessToken, string accessTokenAuth, string email)
        {
            UserClaims claim = new UserClaims();
            claim.Contact = new ContactClaims();
            claim.Contact.Email = email;
            var paramaters = new UaePassAuthenticationParameters(Provider.SystemName)
            {
                ExternalIdentifier = uuid,
                ExternalDisplayIdentifier = idn,
                OAuthAccessToken = accessToken,
                OAuthToken = accessToken
            };
            paramaters.AddClaim(claim);
            return paramaters;
        }
        #endregion

        #region Methods

        #region k / logout


        public virtual ActionResult CustomerLinking(string ignoreCode = null)
        {
            var model = new UaePassLinkingModel();
            if (TempData["uaePassAttr"] != null)
            {
                if (!string.IsNullOrEmpty(TempData["uaePassAttr"].ToString()))
                {
                    var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                    TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);

                    var accessToken = JsonConvert.DeserializeObject<UaePassAccessToken>(TempData["uaePassaccessToken"].ToString());
                    TempData["uaePassaccessToken"] = JsonConvert.SerializeObject(accessToken); ;

                    if (uaePassAttr != null)
                    {
                        model.uuid = uaePassAttr.uuid;
                        model.idn = uaePassAttr.idn ?? uaePassAttr.idnNotVerified;

                        if (!string.IsNullOrEmpty(uaePassAttr.nationalityEN))
                            model.nationality = uaePassAttr.nationalityEN;

                        if (!string.IsNullOrEmpty(uaePassAttr.userType))
                            model.userType = uaePassAttr.userType;

                        if (string.IsNullOrEmpty(ignoreCode))
                        {
                            return LogOutFromUaePass(ignoreCode: "no");
                        }
                    }


                }
            }


            return View(model);
        }
        public virtual ActionResult LogOutFromUaePass(string ignoreCode)
        {
            //call the rest api to logout the user
            try
            {
                //var o = TempData["uaePassAttr"];
                var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);
                var authType = JsonConvert.DeserializeObject<UaePassAuthenticationType>(TempData["uaePassAuthType"].ToString());
                TempData["uaePassAuthType"] = JsonConvert.SerializeObject(authType);

                if (!string.IsNullOrEmpty(ignoreCode) && ignoreCode == "no")
                {
                    _customerActivityService.InsertActivity(Provider.SystemName, _localizationService.GetResource("ActivityLog.UaePass.LagOut"), _workContext.CurrentCustomer);
                    var uaePassLogOutUrl = string.Format(_commonSettings.UaePassUserInfoLogout + "?redirect_uri={0}", _storeContext.CurrentStore.Url + "LogOutFromUaePass");
                    return Redirect(uaePassLogOutUrl);
                }
                return RedirectToRoute("uaepasslinking", new { ignoreCode = "yes" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult UaePass(string type = null, string registrationType = null)
        {
            TempData["fromuaepass"] = "true";

            var UaePassAuthenticationType = new UaePassAuthenticationType
            {
                Type = type,
                RegistrationType = registrationType
            };

            TempData["uaePassAuthType"] = JsonConvert.SerializeObject(UaePassAuthenticationType);
            string fromautologin = TempData["fromAutologin"] != null ? TempData["fromAutologin"] as string : "";
            if (fromautologin == "true")
            {
                if (TempData["uaePassAttr"] != null && TempData["uaePassaccessToken"] != null)
                {
                    var uaePassLogOutUrl = string.Format(_commonSettings.UaePassUserInfoLogout + "?redirect_uri={0}", _storeContext.CurrentStore.Url + "uaepasslogout");
                    var ccustomer = _workContext.CurrentCustomer;
                    if (ccustomer != null)
                    {
                        return Redirect(uaePassLogOutUrl);
                    }
                }
            }
         

            var storeUrl = string.Format("{0}{1}", _storeContext.CurrentStore.Url, _commonSettings.UaePassCallBackUrl);
            var uaePassUrl = string.Format("{0}?redirect_uri={1}&client_id={2}&response_type=code&state={3}&scope=urn:uae:digitalid:profile:general&ui_locales={4}&acr_values=urn:safelayer:tws:policies:authentication:level:low",
                            _commonSettings.UaePassAuthorizationEndPoint,
                             storeUrl,
                            _commonSettings.UaePassClientId,
                             UaePassMode.web,
                            _workContext.WorkingLanguage.UniqueSeoCode
                            );


            //var uaePassUrl = string.Format("{0}?redirect_uri={1}&client_id={2}&response_type=code&state={3}&scope=urn:uae:digitalid:profile:general&ui_locales={4}&acr_values=urn:safelayer:tws:policies:authentication:level:low",
            //              "https://stg-id.uaepass.ae/idshub/authorize",
            //               "http://localhost:15536/uaepass/callback",
            //              "sandbox_stage",
            //               UaePassMode.web,
            //              "en"
            //              );


         
            //logOutFrom Uae every time clear the uae pass session

            return Redirect(uaePassUrl);
        }
        public async Task<ActionResult> UaePassCallBack(string code)
        {

            var UaePassAuthenticationType = new UaePassAuthenticationType();



            if (TempData["uaePassAuthType"] != null)
            {
                UaePassAuthenticationType = JsonConvert.DeserializeObject<UaePassAuthenticationType>(TempData["uaePassAuthType"].ToString());
            }
            if (string.IsNullOrEmpty(code))
            {
                if (!string.IsNullOrEmpty(UaePassAuthenticationType.Type) &&
                    UaePassAuthenticationType.Type == "login")

                    return RedirectToAction("Login", "Customer", new { type = "uaepassauthenticationcancelled" });

                if (!string.IsNullOrEmpty(UaePassAuthenticationType.Type) &&
                                UaePassAuthenticationType.Type == "register")

                    return RedirectToAction("register", new { flage = UaePassAuthenticationType.RegistrationType, type = "uaepassauthenticationcancelled" });
            }

        
            var uaePassAttr = new UaePassUserProfileAttributes();
            var storeUrl = string.Format("{0}{1}", _storeContext.CurrentStore.Url, _commonSettings.UaePassCallBackUrl);
            string uaePassUrl = string.Empty;

            try
            {
                uaePassUrl = string.Format("{0}?grant_type=authorization_code&redirect_uri={1}&code={2}"
                    , _commonSettings.UaePassTokenEndPoint
                    , storeUrl,
                    code);

                //get the access token
                var uaePassUserAccessToken = await this.GetUaePassAccessToken(uaePassUrl);

                if (uaePassUserAccessToken == null)
                    return RedirectToRoute("HomePage");
              
                //get the uae pass user profile
                uaePassAttr = await this.GetUaePassUserProfileAttribute(uaePassUserAccessToken);

                if (uaePassAttr == null)
                    return RedirectToRoute("HomePage");

                if (uaePassAttr != null)
                    TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);

                if (uaePassUserAccessToken != null)
                    TempData["uaePassaccessToken"] = JsonConvert.SerializeObject(uaePassUserAccessToken);
                var customer = _customerService.GetCustomerByEmail(uaePassAttr.email);

                var externalAccountExist = _openAuthenticationService.UaePassUserExist(email: uaePassAttr.email, externalIdentifier: uaePassAttr.uuid, providerSystemName: Provider.SystemName);

                if (externalAccountExist != null)
                {
                    if (customer != null)
                    {
                        if (customer.Email.Equals(uaePassAttr.email))
                        {
                            externalAccountExist.IsEmailExist = true;
                        }
                        if (externalAccountExist.IsEmailExist && !externalAccountExist.IsUuidExist)
                        {
                            if (!string.IsNullOrEmpty(UaePassAuthenticationType.Type) &&
                                UaePassAuthenticationType.Type == "register")
                            {
                                return RedirectToAction("register", new { flage = UaePassAuthenticationType.RegistrationType, type = "uaePass" });
                            }
                            else if (!string.IsNullOrEmpty(UaePassAuthenticationType.Type) &&
                                UaePassAuthenticationType.Type == "login")
                            {
                                return RedirectToRoute("uaepasslinking");
                            }

                        }
                        if (externalAccountExist.IsUuidExist)
                        {
                            var userIdFromExternalAccount = _openAuthenticationService.GetUserIdFromUaePassLinkedUser(uaePassAttr.uuid, Provider.SystemName);
                            if (userIdFromExternalAccount > 0)
                            {
                                customer = _customerService.GetCustomerById(userIdFromExternalAccount);
                                var password = _customerService.GetCurrentPassword(customer.Id);
                                if (UaePassAuthenticationType.Type == "register")
                                {
                                    customer.Deleted = false;
                                    _customerService.UpdateCustomer(customer);
                                }
                                return UaePassAutoLogin(customer.Username, customer.Email, password.Password);
                            }
                        }
                    }
                    else if (customer == null)
                    {
                        if (!externalAccountExist.IsUuidExist)
                        {
                            if (!string.IsNullOrEmpty(UaePassAuthenticationType.Type) &&
                                UaePassAuthenticationType.Type == "register")
                            {
                                return RedirectToAction("register", new { flage = UaePassAuthenticationType.RegistrationType, type = "uaePass", ignoreCode = "no" });
                            }
                            else if (!string.IsNullOrEmpty(UaePassAuthenticationType.Type) &&
                                UaePassAuthenticationType.Type == "login")
                            {
                                return RedirectToRoute("uaepasslinking");
                            }
                        }
                        if (externalAccountExist.IsUuidExist)
                        {
                            var userIdFromExternalAccount = _openAuthenticationService.GetUserIdFromUaePassLinkedUser(uaePassAttr.uuid, Provider.SystemName);
                            if (userIdFromExternalAccount > 0)
                            {
                                customer = _customerService.GetCustomerById(userIdFromExternalAccount);
                                var password = _customerService.GetCurrentPassword(customer.Id);

                                return UaePassAutoLogin(customer.Username, customer.Email, password.Password);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("UaePassCallBack" + ex.Message);
            }
            return RedirectToRoute("HomePage");
        }
        [NonAction]
        public async Task<UaePassAccessToken> GetUaePassAccessToken(string accessTokenUrl)
        {
            var accessToken = new UaePassAccessToken();
            if (string.IsNullOrEmpty(accessTokenUrl))
                return accessToken;

            try
            {
                var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_commonSettings.UaePassClientId}:{_commonSettings.UaePassSecretKey}")));
                var client = new HttpClient()
                {
                    DefaultRequestHeaders = { Authorization = authValue }
                };
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                var requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(accessTokenUrl),
                    Content = new StringContent(string.Empty,
                                    Encoding.UTF8,
                                    "multipart/form-data")//CONTENT-TYPE header
                };
                var response = await client.SendAsync(requestMessage);
                var jsonObject = await response.Content.ReadAsStringAsync();
                accessToken = JsonConvert.DeserializeObject<UaePassAccessToken>(jsonObject);
            }
            catch (Exception ex)
            {
                _logger.Error("GetUaePassAccessToken" + ex.Message);
            }
            return accessToken;

        }
        [NonAction]
        public async Task<UaePassUserProfileAttributes> GetUaePassUserProfileAttribute(UaePassAccessToken accessTokenValue)
        {
            var model = new UaePassUserProfileAttributes();
            try
            {
                var authValue = new AuthenticationHeaderValue("Bearer", accessTokenValue.access_token);
                var client = new HttpClient()
                {
                    DefaultRequestHeaders = { Authorization = authValue }
                };
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                var requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(_commonSettings.UaePassUserInfoEndPoint)
                };
                var response = await client.SendAsync(requestMessage);
                var jsonObject = await response.Content.ReadAsStringAsync();

                model = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(jsonObject);
            }
            catch (Exception ex)
            {
                _logger.Error("GetUaePassUserProfileAttribute" + ex.Message);
            }
            return model;
        }
        [HttpPost]
        public IActionResult Upload()
        {
            var fileData = Request.Form["fileData"];

            // Process the file data

            return View();
        }
        public virtual ActionResult UaePassLogout()
        {
            //external authentication
            ExternalAuthorizerHelper.RemoveParameters();
            //activity log
            _customerActivityService.InsertActivity("PublicStore.Logout", _localizationService.GetResource("ActivityLog.PublicStore.Logout"));
            //standard logout 
            var customer = _workContext.CurrentCustomer;
            _authenticationService.SignOut();
            _workContext.WorkingLanguage.Id = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.LanguageIdAttribute, _storeContext.CurrentStore.Id);
            TempData["WorkingLanguage"] = _workContext.WorkingLanguage.Id;

            //EU Cookie
            if (_storeInformationSettings.DisplayEuCookieLawWarning)
            {
                //the cookie law message should not pop up immediately after logout.
                //otherwise, the user will have to click it again...
                //and thus next visitor will not click it... so violation for that cookie law..
                //the only good solution in this case is to store a temporary variable
                //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
                //but it'll be displayed for further page loads
                TempData["nop.IgnoreEuCookieLawWarning"] = true;
            }
            TempData["uaePassAttr"] = null;
            TempData["uaePassaccessToken"] = null;
            TempData["ignoreCode"] = null;
            TempData["uaePassAuthType"] = null;
            string fromuaepass = TempData["fromuaepass"] as string;
            if (fromuaepass == "true")
            {
                var storeUrl = string.Format("{0}{1}", _storeContext.CurrentStore.Url, _commonSettings.UaePassCallBackUrl);
                var uaePassUrl = string.Format("{0}?redirect_uri={1}&client_id={2}&response_type=code&state={3}&scope=urn:uae:digitalid:profile:general&ui_locales={4}&acr_values=urn:safelayer:tws:policies:authentication:level:low",
                                _commonSettings.UaePassAuthorizationEndPoint,
                                 storeUrl,
                                _commonSettings.UaePassClientId,
                                 UaePassMode.web,
                                _workContext.WorkingLanguage.UniqueSeoCode
                                );

                return Redirect(uaePassUrl);
            }
            else
            {
                return RedirectToRoute("HomePage");
            }
           
        }
        [NonAction]
        public Services.Authentication.External.AuthorizationResult VerifyAuthentication(string returnUrl, UaePassUserProfileAttributes uaePassAttr, string accessToken)
        {
            string idn = uaePassAttr.idn ?? uaePassAttr.idnNotVerified;
            var authorizationResult = new Services.Authentication.External.AuthorizationResult(OpenAuthenticationStatus.Error);
            try
            {
                if (string.IsNullOrEmpty(idn))
                    return null;

                if (string.IsNullOrEmpty(uaePassAttr.email))
                    return null;

                if (string.IsNullOrEmpty(uaePassAttr.firstnameEN))
                    return null;

                if (_externalAuthenticationSettings.AutoRegisterEnabled)
                {
                    authorizationResult = _externalAuthenticationService.AuthorizeUaePass(uaePassAttr.uuid, idn, accessToken, accessToken, uaePassAttr.email, uaePassAttr.firstnameEN);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return authorizationResult;
        }
        public virtual ActionResult UaePassRegister(string type)
        {
            string error = string.Empty;

            if (string.IsNullOrEmpty(type))
                return RedirectToRoute("HomePage");
            try
            {
                var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                var accessToken = JsonConvert.DeserializeObject<UaePassAccessToken>(TempData["uaePassaccessToken"].ToString());
                if (uaePassAttr == null || accessToken == null)
                    return RedirectToRoute("HomePage");

                var authorize = this.VerifyAuthentication(string.Empty, uaePassAttr, accessToken.access_token);
                if (authorize.Success)
                {
                    var customer = _customerService.GetCustomerByEmail(uaePassAttr.email);
                    if (customer == null)
                        return RedirectToRoute("HomePage");
                    var phone = string.Empty;
                    if (uaePassAttr.mobile.Contains("971"))
                    {
                        if (uaePassAttr.mobile.Length >= 12)
                            phone = uaePassAttr.mobile.Substring(3, uaePassAttr.mobile.Length - 3);
                        else
                            phone = uaePassAttr.mobile;
                    }
                    else
                    {
                        phone = uaePassAttr.mobile;
                    }
                    phone = string.Format("{0}{1}", "0", phone);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, phone);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, uaePassAttr.gender == "Male" ? "M" : "F");
                    if (!string.IsNullOrEmpty(uaePassAttr.dob))
                    {
                        DateTime? DateOfBirth = DateTime.ParseExact(uaePassAttr.dob, "dd/MM/yyyy", null); //14/03/1987
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, DateOfBirth);
                    }

                    var password = _customerService.GetCurrentPassword(customer.Id);

                    return this.UaePassAutoLogin(customer.Username, customer.Email, password.Password);
                }
                if (authorize.Errors.Count > 0)
                {
                    foreach (var e in authorize.Errors)
                    {
                        error += e;
                    }
                }
            }
            catch (Exception ex) { throw ex; }
            return RedirectToRoute("HomePage");
        }


        [HttpsRequirement]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Login(bool? checkoutAsGuest, string type)
        {
            //_workContext.WorkingLanguage.Id = 1;
            var model = new LoginModel();

            model = _customerModelFactory.PrepareLoginModel(checkoutAsGuest);
            if (TempData["LoginModel"] != null)
            {
                model = JsonConvert.DeserializeObject<LoginModel>(TempData["LoginModel"].ToString());
            }

            
            model.HasAcceptedTermsAndConditions = true;
            var activationMessage = TempData["ActivationMessage"];
            if (activationMessage != null && !string.IsNullOrEmpty(activationMessage.ToString()))
                model.Result = TempData["ActivationMessage"].ToString();

            if (!string.IsNullOrEmpty(type))
            {
                model.isUaePass = true;
                if (TempData["uaePassAttr"] != null)
                {
                    var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                    TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);
                }
                if (TempData["uaePassaccessToken"] != null)
                {
                    var accessToken = JsonConvert.DeserializeObject<UaePassAccessToken>(TempData["uaePassaccessToken"].ToString());
                    TempData["uaePassaccessToken"] = JsonConvert.SerializeObject(accessToken);
                }
              
            }
             var fromAutologin= TempData["fromAutologin"] as string;
             TempData["fromAutologin"] = fromAutologin;


            return View(model);
        }

        protected virtual ActionResult UaePassAutoLogin(string Username, string Email, string Password,
        string returnUrl = null, bool HasAcceptedTermsAndConditions = true, bool RememberMe = true, string requestType = null)
        {
            string error = string.Empty;
            try
            {
                if (_customerSettings.UsernamesEnabled && Username != null)
                {
                    Username = Username.Trim();
                }
                var loginResult = _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? Username : Email, Password, ignorePasswordHash: true, IsExternal: true);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(Username) : _customerService.GetCustomerByEmail(Email);
                            //has accepted terms and conditions

                            var hasAcceptedTermsAndConditions = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions);
                            if (!hasAcceptedTermsAndConditions && !HasAcceptedTermsAndConditions)
                            {
                                HasAcceptedTermsAndConditions = false;
                                error += _localizationService.GetResource("checkout.termsofservice.pleaseaccept");
                                break;
                            }
                            else
                                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions, HasAcceptedTermsAndConditions);

                            //migrate shopping cart
                            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                            //sign in new customer
                            _authenticationService.SignIn(customer, RememberMe);

                            //raise event       
                            _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                            //activity log
                            _customerActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                            if (!string.IsNullOrEmpty(requestType))
                            {
                                if (requestType == "merchant")
                                    return RedirectToRoute("HomePage");
                                // return Redirect("vendor/ApplyVendor?type=uaePass");
                                else if (requestType == "customer")
                                    return RedirectToRoute("HomePage");
                            }
                            if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("HomePage");
                            return Redirect(returnUrl);
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        error += _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist");
                        break;
                    case CustomerLoginResults.Deleted:
                        error += _localizationService.GetResource("Account.Login.WrongCredentials.Deleted");
                        break;
                    case CustomerLoginResults.NotActive:
                        error += _localizationService.GetResource("Account.Login.WrongCredentials.NotActive");
                        break;
                    case CustomerLoginResults.NotRegistered:
                        error += _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered");
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        error += _localizationService.GetResource("Account.Login.WrongCredentials");
                        break;
                }
            }

            catch (Exception ex) { throw ex; }
            TempData["ErrorMessage"] = error;
            TempData["fromAutologin"] = "true";
            ExternalAuthorizerHelper.RemoveParameters();

            return RedirectToAction("login");
        }


        [HttpPost]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            //RegisterModel regmodel = model.RegisterModel;
            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult = _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled
                                ? _customerService.GetCustomerByUsername(model.Username)
                                : _customerService.GetCustomerByEmail(model.Email);
                            //has accepted terms and conditions
                            var hasAcceptedTermsAndConditions = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions);
                            var currlang = _workContext.WorkingLanguage.Id;
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LanguageIdAttribute, currlang, _storeContext.CurrentStore.Id);

                            if (!hasAcceptedTermsAndConditions && !model.HasAcceptedTermsAndConditions)
                            {
                                model.HasAcceptedTermsAndConditions = false;
                                ModelState.AddModelError("", _localizationService.GetResource("checkout.termsofservice.pleaseaccept"));
                                break;
                            }
                            else if (hasAcceptedTermsAndConditions)
                            {
                                //do nothing if terms have been accepted
                            }
                            else
                                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions, model.HasAcceptedTermsAndConditions);

                            //uae pass linking when the user enter his cred for the local system of GWU
                            if (model.isUaePass)
                            {
                               // var uaePassAttr = TempData["uaePassAttr"] as UaePassUserProfileAttributes;
                                //var accessToken = TempData["uaePassaccessToken"] as UaePassAccessToken;
                                var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                                var accessToken = JsonConvert.DeserializeObject<UaePassAccessToken>(TempData["uaePassaccessToken"].ToString());

                                if (uaePassAttr != null && accessToken != null)
                                {
                                    var paramaters = GetUaePassUserAttributesValue(uaePassAttr.uuid, uaePassAttr.idn ?? uaePassAttr.idnNotVerified,
                                        accessToken.access_token, accessToken.access_token, uaePassAttr.email);

                                    var externalAccountExist = _openAuthenticationService.GetUser(paramaters);
                                    if (externalAccountExist == null)
                                        _openAuthenticationService.AssociateExternalAccountWithUser(customer, paramaters);
                                }
                            }

                            //migrate shopping cart
                            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                            //sign in new customer
                            _authenticationService.SignIn(customer, model.RememberMe);

                            //raise event       
                            _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                            //activity log
                            _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                                _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);
                            //set default language arabic
                            // _workContext.WorkingLanguage.Id = 2;
                            string request = TempData["Flage"] as string;
                            if(request!=null) //(Request.Cookies["Flage"] != null)
                            {

                                //HttpCookie aCookie = new HttpCookie("RequestType");


                                CookieOptions aCookie = new CookieOptions();

                               // string request = Request.Cookies["Flage"];
                                //string request = HttpContext.Request.Query["Flage"];
                                if (request != null)
                                {
                                    if (request == "merchant")
                                    {
                                        return RedirectToRoute("HomePage");
                                        //return Redirect("vendor/ApplyVendor");
                                    }
                                    else if (request == "customer")
                                    {

                                        if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                            return RedirectToRoute("HomePage");
                                    }
                                }

                            }

                            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                            {
                                return Redirect(returnUrl + "Admin");
                            }


                            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("Homepage");

                            return Redirect(returnUrl);
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                TempData["ErrorMessage"] = error.ErrorMessage;
                // If you only want to store the first error, break the loop here
                // break;
            }
            model = _customerModelFactory.PrepareLoginModel(model.CheckoutAsGuest);
            TempData["LoginModel"] = JsonConvert.SerializeObject(model);
            return RedirectToAction("login");
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Logout()
        {
            TempData["step"] = "2";
            if (TempData["uaePassAttr"] != null && TempData["uaePassaccessToken"] != null)
            {
                TempData["fromuaepass"] = null;
                var uaePassLogOutUrl = string.Format(_commonSettings.UaePassUserInfoLogout + "?redirect_uri={0}", _storeContext.CurrentStore.Url + "uaepasslogout");
                var ccustomer = _workContext.CurrentCustomer;
                if (ccustomer != null)
                {
                    var externalAccountWithUaePassExist = _openAuthenticationService.GetExternalIdentifiersFor(ccustomer).Where(x => x.ProviderSystemName == Provider.SystemName).FirstOrDefault();
                    if (externalAccountWithUaePassExist != null)
                    {
                        //call the rest api to logout the user
                        try
                        {
                            _customerActivityService.InsertActivity(Provider.SystemName, _localizationService.GetResource("ActivityLog.UaePass.LagOut"), ccustomer);
                            return Redirect(uaePassLogOutUrl);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            //external authentication
            ExternalAuthorizerHelper.RemoveParameters();
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //activity log
                _customerActivityService.InsertActivity(_workContext.OriginalCustomerIfImpersonated, "Impersonation.Finished",
                    string.Format(_localizationService.GetResource("ActivityLog.Impersonation.Finished.StoreOwner"),
                        _workContext.CurrentCustomer.Email, _workContext.CurrentCustomer.Id),
                    _workContext.CurrentCustomer);

                _customerActivityService.InsertActivity("Impersonation.Finished",
                    string.Format(_localizationService.GetResource("ActivityLog.Impersonation.Finished.Customer"),
                        _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                    _workContext.OriginalCustomerIfImpersonated);

                //logout impersonated customer
                _genericAttributeService
                    .SaveAttribute<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

                //redirect back to customer details page (admin area)
                return RedirectToAction("Edit", "Customer", new { id = _workContext.CurrentCustomer.Id, area = AreaNames.Admin });
            }

            //activity log
            _customerActivityService.InsertActivity(_workContext.CurrentCustomer, "PublicStore.Logout",
                _localizationService.GetResource("ActivityLog.PublicStore.Logout"), _workContext.CurrentCustomer);

            var customer = _workContext.CurrentCustomer;
            //standard logout 
            _authenticationService.SignOut();

            _workContext.WorkingLanguage.Id = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.LanguageIdAttribute, _storeContext.CurrentStore.Id);
            TempData["WorkingLanguage"] = _workContext.WorkingLanguage.Id;

            //raise logged out event       
            _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

            //EU Cookie
            if (_storeInformationSettings.DisplayEuCookieLawWarning)
            {
                //the cookie law message should not pop up immediately after logout.
                //otherwise, the user will have to click it again...
                //and thus next visitor will not click it... so violation for that cookie law..
                //the only good solution in this case is to store a temporary variable
                //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
                //but it'll be displayed for further page loads
                TempData[$"{NopCookieDefaults.Prefix}{NopCookieDefaults.IgnoreEuCookieLawWarning}"] = true;
            }

            return RedirectToRoute("Homepage");
        }

        #endregion

        #region Password recovery

        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecovery()
        {
            var model = new PasswordRecoveryModel();
            model = _customerModelFactory.PreparePasswordRecoveryModel(model);

            return View(model);
        }

        [ValidateCaptcha]
        [HttpPost, ActionName("PasswordRecovery")]
        [FormValueRequired("send-email")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecoverySend(PasswordRecoveryModel model, bool captchaValid)
        {
            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnForgotPasswordPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                var customer = _customerService.GetCustomerByEmail(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    //save token and current date
                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    _genericAttributeService.SaveAttribute(customer,
                        NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                    //send email
                    _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer,
                        _workContext.WorkingLanguage.Id);
                    _notificationService.SuccessNotification("Account.PasswordRecovery.EmailHasBeenSent");
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                }
                else
                {
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                    _notificationService.WarningNotification("Account.PasswordRecovery.EmailNotFound");
                }
            }

            model = _customerModelFactory.PreparePasswordRecoveryModel(model);
            return View(model);
        }

        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecoveryConfirm(string token, string email, Guid guid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                customer = _customerService.GetCustomerByGuid(guid);

            if (customer == null)
                return RedirectToRoute("Homepage");

            if (string.IsNullOrEmpty(_genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute)))
            {
                return base.View(new PasswordRecoveryConfirmModel
                {
                    DisablePasswordChanging = true,
                    Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordAlreadyHasBeenChanged")
                });
            }

            var model = _customerModelFactory.PreparePasswordRecoveryConfirmModel();

            //validate token
            if (!_customerService.IsPasswordRecoveryTokenValid(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
            }

            //validate token expiration date
            if (_customerService.IsPasswordRecoveryLinkExpired(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
            }

            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [FormValueRequired("set-password")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecoveryConfirmPOST(string token, string email, Guid guid, PasswordRecoveryConfirmModel model)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                customer = _customerService.GetCustomerByGuid(guid);

            if (customer == null)
                return RedirectToRoute("Homepage");

            //validate token
            if (!_customerService.IsPasswordRecoveryTokenValid(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
                return View(model);
            }

            //validate token expiration date
            if (_customerService.IsPasswordRecoveryLinkExpired(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(customer.Email,
                    false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
                if (response.Success)
                {
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute, "");

                    model.DisablePasswordChanging = true;
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordHasBeenChanged");
                    _notificationService.WarningNotification("Account.PasswordRecovery.PasswordHasBeenChanged");
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Register

        public virtual IActionResult UaePassRegisterationLogOut(string flage = null, string type = null, string ignoreCode = null)
        {
            //call the rest api to logout the user
            try
            {
                UaePassRegistrationTempData regTempData = null;
                var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);
                var authType = JsonConvert.DeserializeObject<UaePassAuthenticationType>(TempData["uaePassAuthType"].ToString());
                TempData["uaePassAuthType"] = JsonConvert.SerializeObject(authType);
                if (!string.IsNullOrEmpty(ignoreCode) && ignoreCode.ToLower() == "no")
                {
                    _customerActivityService.InsertActivity(Provider.SystemName, _localizationService.GetResource("ActivityLog.UaePass.LagOut"), _workContext.CurrentCustomer);
                    regTempData = new UaePassRegistrationTempData
                    {
                        flag = flage,
                        type = type,
                    };
                    TempData["ignoreCode"] = JsonConvert.SerializeObject(regTempData);
                    var uaePassLogOutUrl = string.Format(_commonSettings.UaePassUserInfoLogout + "?redirect_uri={0}", _storeContext.CurrentStore.Url + "uaepassregisterationlogOut");
                    return Redirect(uaePassLogOutUrl);
                }
                regTempData = JsonConvert.DeserializeObject<UaePassRegistrationTempData>(TempData["ignoreCode"].ToString());
                var redirectUrl = string.Format("~/register?flage={0}&type={1}&ignoreCode={2}", regTempData.flag, regTempData.type, "yes");
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register(string flage, string type, string ignoreCode = null)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            //uae pass logout
            if (string.IsNullOrEmpty(ignoreCode) &&
              !string.IsNullOrEmpty(type) && type.ToLower() == "uaepass")
            {
                return RedirectToRoute("uaepassregisterationlogOut", new { flage, type, ignoreCode = "no" });
            }

            var model = new RegisterModel();
            model = _customerModelFactory.PrepareRegisterModel(model, false);
            model.RequestType = flage;
            if (!string.IsNullOrEmpty(type))
            {
                if (type.ToLower() == "uaepass")
                {
                    model.IsUaePass = true;
                }
                if (TempData["uaePassAttr"] != null)
                {
                    var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                    if (uaePassAttr != null)
                    {
                        model.Gender = uaePassAttr.gender == "Male" ? "M" : "F";
                        model.FirstName = uaePassAttr.firstnameEN;
                        model.LastName = uaePassAttr.lastnameEN;
                        model.Email = uaePassAttr.email;
                        model.Username = uaePassAttr.email;

                        if (!string.IsNullOrEmpty(uaePassAttr.mobile))
                        {
                            model.Phone = uaePassAttr.mobile.Contains("971") ? uaePassAttr.mobile.Replace("971", "0").Trim() : uaePassAttr.mobile;
                        }
                        if (!string.IsNullOrEmpty(uaePassAttr.dob))
                        {
                            var dob = uaePassAttr.dob.Split('/');
                            model.DateOfBirthDay = Convert.ToInt32(dob[0]);
                            model.DateOfBirthMonth = Convert.ToInt32(dob[1]);
                            model.DateOfBirthYear = Convert.ToInt32(dob[2]);
                        }
                        //assign again its should be null after second request
                        TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);
                    }
                }

            }
            //enable newsletter by default
            model.Newsletter = _customerSettings.NewsletterTickedByDefault;



            return View(model);
        }

        [HttpsRequirement]
        //available even when navigation is not allowed
        //[PublicStoreAllowNavigation(true)]
        public IActionResult RegisterInit(string type)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new RegisterModel();
            if (!string.IsNullOrEmpty(type))
            {
                if (type.ToLower().Contains("uaepass"))
                {
                    model.IsUaePass = true;
                }
                var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);

                if (!string.IsNullOrEmpty(uaePassAttr.userType))
                    model.userType = uaePassAttr.userType;

                if (!string.IsNullOrEmpty(uaePassAttr.nationalityEN))
                    model.nationalityEN = uaePassAttr.nationalityEN;


            }
            //  PrepareCustomerRegisterModel(model, false);
            //enable newsletter by default
            // model.Newsletter = _customerSettings.NewsletterTickedByDefault;
            model.RequestType = type;
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public async virtual Task<IActionResult> Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form, IFormFile TradeLicenseFile)
        {
            TempData["MATRequestType"] = "register";
            TempData["Flage"] ="";
            TempData["email"] = "";
            TempData["step"] = "1";
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (_customerService.IsRegistered(_workContext.CurrentCustomer))
            {
                //Already registered customer. 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }
            //uae Pass Information
            if (model.IsUaePass)
            {
                var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                if (uaePassAttr != null)
                {
                    model.Gender = uaePassAttr.gender == "Male" ? "M" : "F";
                    model.FirstName = uaePassAttr.firstnameEN;
                    model.LastName = uaePassAttr.lastnameEN;
                    if (!string.IsNullOrEmpty(uaePassAttr.dob))
                    {
                        var dob = uaePassAttr.dob.Split('/');
                        model.DateOfBirthDay = Convert.ToInt32(dob[0]);
                        model.DateOfBirthMonth = Convert.ToInt32(dob[1]);
                        model.DateOfBirthYear = Convert.ToInt32(dob[2]);
                    }
                    model.Phone = uaePassAttr.mobile.StartsWith("971") ? uaePassAttr.mobile.Replace("971", "0") : uaePassAttr.mobile;
                    model.Email = uaePassAttr.email;
                    model.Username = uaePassAttr.email;
                    var randomPassword = CommonHelper.GenerateRandomDigitCode(20);
                    model.Password = randomPassword;
                    model.ConfirmPassword = randomPassword;
                    ModelState.Remove("Password");
                    ModelState.Remove("ConfirmPassword");
                }
            }
            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = _gdprService
                    .GetAllConsents().Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }
            if (!string.IsNullOrEmpty(model.DateOfBirth))
            {
                var date_of_birth = model.DateOfBirth.Split('/');
                model.DateOfBirthDay = Convert.ToInt32(date_of_birth[0]);
                model.DateOfBirthMonth = Convert.ToInt32(date_of_birth[1]);
                model.DateOfBirthYear = Convert.ToInt32(date_of_birth[2]);
            }
            //
            if (model.RequestType.ToLower().Equals("customer"))
            {
                ModelState.Remove("CategoryId");
                ModelState.Remove("SellerName");
            }

            //if (TradeLicenseFile == null || TradeLicenseFile.Length == 0)
            //{
            //    ModelState.AddModelError("file", "Please select a file to upload.");
            //}
            if (TradeLicenseFile != null && TradeLicenseFile.Length != 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".doc" };
                var fileExtension = Path.GetExtension(TradeLicenseFile.FileName);

                if (!allowedExtensions.Contains(fileExtension.ToLower()))
                {
                    ModelState.AddModelError("file", "Invalid file type.");
                }

                var maxSize = 5048576; // 1MB

                if (TradeLicenseFile.Length > maxSize)
                {
                    ModelState.AddModelError("file", "File size must be less than 5MB.");
                }
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                //var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                bool isApproved = model.IsUaePass ? true : false;
                bool IsExternal = model.IsUaePass ? true : false;

                List<string> _Categories = new List<string>();
                if (model.CategoryId != null)
                {
                    foreach (var cat in model.CategoryId)
                    {
                        _Categories.Add(cat.ToString());
                    }
                }
                int _vendorID = 0;


                var registrationRequest = new CustomerRegistrationRequest(customer,
              model.Email,
              _customerSettings.UsernamesEnabled ? model.Username : model.Email,
              model.Password,
              _customerSettings.DefaultPasswordFormat,
              _storeContext.CurrentStore.Id,
              isApproved, IsExternal, categories: _Categories,
              tradeLicenseFile: TradeLicenseFile != null ? true : false,
              registrationType: model.RequestType,
              licenseNumber: model.TradeLicenseNumber,
              _ExpiryDate: model.ExpiryDate, _LicenseNo: model.TradeLicenseNumber, vendorID: _vendorID);




                if (!IsExternal)
                {

                    var validationresult = _customerRegistrationService.ValidateRegister(registrationRequest);
                    if (validationresult.Success)
                    {
                        if (model.RequestType.ToLower().Equals("merchant"))
                        {
                            bool Expired = false;

                            _vendorID = await TradeLicenseAdd(model, form, TradeLicenseFile);
                        }
                        model.VendorId = _vendorID;
                        string randomNumber = RandomNumberService.RandomDigits(6);
                        if (!string.IsNullOrEmpty(model.Phone))
                            _genericAttributeService.SaveAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);

                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountSmsActivationToken, randomNumber);
                        customer.Email = model.Email;
                        _eventPublisher.Publish(new CustomerRegisteredEvent(customer, isAproved: false));


                        switch (_customerSettings.UserRegistrationType)
                        {
                            case UserRegistrationType.EmailValidation:
                                {
                                    //email validation message
                                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                    if (!model.IsUaePass)
                                        _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                    //result
                                    return RedirectToRoute("RegisterResult",
                                        new { resultId = (int)UserRegistrationType.EmailValidation });
                                }
                            case UserRegistrationType.AdminApproval:
                                {
                                    return RedirectToRoute("RegisterResult",
                                        new { resultId = (int)UserRegistrationType.AdminApproval });
                                }
                            case UserRegistrationType.Standard:
                                {
                                    _eventPublisher.Publish(new CustomerActivatedEvent(customer));

                                    //var aCookie = new CookieOptions
                                    //{
                                    //    Expires = DateTime.Now.AddDays(7)
                                    //};

                                    //Response.Cookies.Append("RequestType", "register", aCookie);
                                    //Response.Cookies.Append("Flage", model.RequestType, aCookie);
                                    //Response.Cookies.Append("email", model.Email, aCookie);
                                    //Response.Cookies.Append("step", 1.ToString(), aCookie);

                                    TempData["MATRequestType"] = "register";
                                    TempData["Flage"] = model.RequestType;
                                    TempData["email"] = model.Email;
                                    TempData["step"] = "1";


                                    TempData["RegistrationData"] = null;
                                    TempData["RegistrationData"] = JsonConvert.SerializeObject(model);
                                    // return RedirectToAction("login", "customer", model);
                                    return RedirectToAction("login");
                                }
                            default:
                                {
                                    return RedirectToRoute("Homepage");
                                }
                        }

                    }
                    foreach (var error in validationresult.Errors)
                        ModelState.AddModelError("", error);
                }
                else
                {
                    var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                    if (registrationResult.Success)
                    {
                        if (model.RequestType.ToLower().Equals("merchant"))
                        {
                            bool Expired = false;

                            _vendorID = await TradeLicenseAdd(model, form, TradeLicenseFile);
                        }
                        model.VendorId = _vendorID;

                        customer.VendorId = _vendorID;
                        _customerService.UpdateCustomer(customer);

                        //properties
                        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        {
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                        }
                        //VAT number
                        if (_taxSettings.EuVatEnabled)
                        {
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute, model.VatNumber);

                            var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out _, out var vatAddress);
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }

                        //form fields
                        _genericAttributeService.SaveAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute, false);
                        if (_customerSettings.GenderEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                        if (_customerSettings.FirstNameEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                        if (_customerSettings.LastNameEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);

                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions, model.HasAcceptedTermsAndConditions);

                        if (_customerSettings.DateOfBirthEnabled)
                        {
                            var dateOfBirth = model.ParseDateOfBirth();
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                        }
                        if (_customerSettings.CompanyEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                        if (_customerSettings.StreetAddressEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                        if (_customerSettings.StreetAddress2Enabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                        if (_customerSettings.ZipPostalCodeEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                        if (_customerSettings.CityEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, model.City);
                        if (_customerSettings.CountyEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, model.County);
                        if (_customerSettings.CountryEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute,
                                model.StateProvinceId);
                        if (_customerSettings.PhoneEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                        if (_customerSettings.FaxEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                        //newsletter
                        if (_customerSettings.NewsletterEnabled)
                        {
                            //save newsletter value
                            var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                            if (newsletter != null)
                            {
                                if (model.Newsletter)
                                {
                                    newsletter.Active = true;
                                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);

                                    //GDPR
                                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                    {
                                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                                    }
                                }
                                //else
                                //{
                                //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                                //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                                //}
                            }
                            else
                            {
                                if (model.Newsletter)
                                {
                                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = model.Email,
                                        Active = true,
                                        StoreId = _storeContext.CurrentStore.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });

                                    //GDPR
                                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                    {
                                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                                    }
                                }
                            }
                        }

                        if (_customerSettings.AcceptPrivacyPolicyEnabled)
                        {
                            //privacy policy is required
                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                            {
                                _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.PrivacyPolicy"));
                            }
                        }

                        //GDPR
                        if (_gdprSettings.GdprEnabled)
                        {
                            var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayDuringRegistration).ToList();
                            foreach (var consent in consents)
                            {
                                var controlId = $"consent{consent.Id}";
                                var cbConsent = form[controlId];
                                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                                {
                                    //agree
                                    _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                                }
                                else
                                {
                                    //disagree
                                    _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                                }
                            }
                        }

                        //save customer attributes
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                        //login customer now
                        if (isApproved)
                            _authenticationService.SignIn(customer, true);

                        //insert default address (if possible)
                        var defaultAddress = new Address
                        {
                            FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                            LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute),
                            Email = customer.Email,
                            Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute),
                            CountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute) > 0
                                ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute)
                                : null,
                            StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute) > 0
                                ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute)
                                : null,
                            County = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CountyAttribute),
                            City = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CityAttribute),
                            Address1 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddressAttribute),
                            Address2 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddress2Attribute),
                            ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute),
                            PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                            FaxNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FaxAttribute),
                            CreatedOnUtc = customer.CreatedOnUtc
                        };
                        if (_addressService.IsAddressValid(defaultAddress))
                        {
                            //some validation
                            if (defaultAddress.CountryId == 0)
                                defaultAddress.CountryId = null;
                            if (defaultAddress.StateProvinceId == 0)
                                defaultAddress.StateProvinceId = null;
                            //set default address
                            //customer.Addresses.Add(defaultAddress);

                            _addressService.InsertAddress(defaultAddress);

                            _customerService.InsertCustomerAddress(customer, defaultAddress);

                            customer.BillingAddressId = defaultAddress.Id;
                            customer.ShippingAddressId = defaultAddress.Id;

                            _customerService.UpdateCustomer(customer);
                        }


                        string randomNumber = RandomNumberService.RandomDigits(6);
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountSmsActivationToken, randomNumber);

                        //notifications
                        //if (_customerSettings.NotifyNewCustomerRegistration)
                        //    _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                        //        _localizationSettings.DefaultAdminLanguageId);

                        //raise event       
                        _eventPublisher.Publish(new CustomerRegisteredEvent(customer, isAproved: false));

                        switch (_customerSettings.UserRegistrationType)
                        {
                            case UserRegistrationType.EmailValidation:
                                {
                                    //email validation message
                                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                    if (!model.IsUaePass)
                                        _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                    //result
                                    return RedirectToRoute("RegisterResult",
                                        new { resultId = (int)UserRegistrationType.EmailValidation });
                                }
                            case UserRegistrationType.AdminApproval:
                                {
                                    return RedirectToRoute("RegisterResult",
                                        new { resultId = (int)UserRegistrationType.AdminApproval });
                                }
                            case UserRegistrationType.Standard:
                                {
                                    //send customer welcome message
                                    // _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);
                                    //send customer registration message
                                    //_workflowMessageService.SendCustomerRegisteredNotificationMessage(customer, _workContext.WorkingLanguage.Id);
                                    //raise event       
                                    _eventPublisher.Publish(new CustomerActivatedEvent(customer));

                                    //var redirectUrl = Url.RouteUrl("RegisterResult",
                                    //    new { resultId = (int)UserRegistrationType.Standard, returnUrl }, _webHelper.CurrentRequestProtocol);
                                    //return Redirect(redirectUrl);

                                    if (model.IsUaePass)
                                    {
                                        //link uae pass user in external table
                                        var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                                        var accessToken = JsonConvert.DeserializeObject<UaePassAccessToken>(TempData["uaePassaccessToken"].ToString());
                                        if (uaePassAttr != null && accessToken != null)
                                        {
                                            var paramaters = GetUaePassUserAttributesValue(uaePassAttr.uuid, uaePassAttr.idn ?? uaePassAttr.idnNotVerified,
                                                accessToken.access_token, accessToken.access_token, uaePassAttr.email);

                                            var externalAccountExist = _openAuthenticationService.GetUser(paramaters);
                                            if (externalAccountExist == null)
                                                _openAuthenticationService.AssociateExternalAccountWithUser(customer, paramaters);
                                        }
                                        if (!string.IsNullOrEmpty(uaePassAttr.nationalityEN))
                                        {
                                            var country = _countryService.GetCountryByThreeLetterIsoCode(uaePassAttr.nationalityEN);
                                            if (country != null)
                                            {
                                                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, country.Id);
                                                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.NationalityCountryId, country.Id);
                                            }
                                        }
                                        customer.Active = true;
                                        customer.Deleted = false;
                                        _customerService.UpdateCustomer(customer);
                                        var currentPassword = _customerService.GetCurrentPassword(customer.Id);
                                        return this.UaePassAutoLogin(customer.Username, customer.Email, currentPassword.Password, requestType: model.RequestType);
                                    }

                                    TempData["RequestType"] = "register";
                                    TempData["Flage"] = model.RequestType;
                                    TempData["email"] = model.Email;
                                    TempData["step"] = "1";
                                    //var aCookie = new CookieOptions();
                                    //Response.Cookies.Append("RequestType", "register", aCookie);
                                    //Response.Cookies.Append("Flage", model.RequestType, aCookie);
                                    //Response.Cookies.Append("email", model.Email, aCookie);
                                    //Response.Cookies.Append("step", 1.ToString(), aCookie);



                                    // _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedPushNotification.Requeued"));
                                    return Redirect("~/login");
                                }
                            default:
                                {
                                    return RedirectToRoute("Homepage");
                                }
                        }
                    }
                    foreach (var error in registrationResult.Errors)
                        ModelState.AddModelError("", error);
                }
                //errors

            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareRegisterModel(model, true, customerAttributesXml);

            if (model.CategoryId != null)
            {
                List<int> Vcategories = new List<int>();
                foreach (var c in model.CategoryId)
                {
                    Vcategories.Add(Convert.ToInt32(c));
                }
                model.CategoryId = Vcategories;
            }
            if (!string.IsNullOrEmpty(model.RequestType))
            {
                if (model.RequestType.ToLower() == "uaepass")
                {
                    model.IsUaePass = true;
                }
                if (TempData["uaePassAttr"] != null)
                {
                    var uaePassAttr = JsonConvert.DeserializeObject<UaePassUserProfileAttributes>(TempData["uaePassAttr"].ToString());
                    if (uaePassAttr != null)
                    {
                        model.Gender = uaePassAttr.gender == "Male" ? "M" : "F";
                        model.FirstName = uaePassAttr.firstnameEN;
                        model.LastName = uaePassAttr.lastnameEN;
                        model.Email = uaePassAttr.email;
                        model.Username = uaePassAttr.email;

                        if (!string.IsNullOrEmpty(uaePassAttr.mobile))
                        {
                            model.Phone = uaePassAttr.mobile.Contains("971") ? uaePassAttr.mobile.Replace("971", "0").Trim() : uaePassAttr.mobile;
                        }
                        if (!string.IsNullOrEmpty(uaePassAttr.dob))
                        {
                            var dob = uaePassAttr.dob.Split('/');
                            model.DateOfBirthDay = Convert.ToInt32(dob[0]);
                            model.DateOfBirthMonth = Convert.ToInt32(dob[1]);
                            model.DateOfBirthYear = Convert.ToInt32(dob[2]);
                        }
                        //assign again its should be null after second request
                        TempData["uaePassAttr"] = JsonConvert.SerializeObject(uaePassAttr);
                    }
                }

            }

            return View(model);
        }

        private async Task<int> TradeLicenseAdd(RegisterModel model, IFormCollection form, IFormFile formFile)
        {
            int vendorID = 0;
            int TradeLicenseFileId = 0;
            if (!string.IsNullOrEmpty(model.TradeLicenseNumber))
            {
                var result = await _tradelicenseService.GetTradeLicenseResponse(model.TradeLicenseNumber);
                if (result.GetLicenseDetailsResponse.Status == "SUCCESS" &&
                    !string.IsNullOrEmpty(model.TradeLicenseNumber))
                {


                }
            }
            if (formFile != null)
            {
                var fileBinary = _downloadService.GetDownloadBits(formFile);
                var file = formFile;
                var download = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    UseDownloadUrl = false,
                    DownloadUrl = string.Empty,
                    DownloadBinary = fileBinary,
                    ContentType = file.ContentType,
                    Filename = _fileProvider.GetFileNameWithoutExtension(file.FileName),
                    Extension = _fileProvider.GetFileExtension(file.FileName),
                    IsNew = true
                };
                _downloadService.InsertDownload(download);
                TradeLicenseFileId = download.Id > 0 ? download.Id : 0;
            }

            var vendors = _vendorService.GetAllEntities(VendorRegisterationType.Merchant);

            var vendor = new Vendor()
            {
                Name = model.SellerName,
                LicenseNo = model.TradeLicenseNumber,
                Email = model.Email,
                PhoneNumber = model.Phone,
                LicenseCopyId = TradeLicenseFileId,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                CreatedBy = _workContext.CurrentCustomer.Id,
                CreatedOnUtc = DateTime.UtcNow,
            };
            _vendorService.InsertVendor(vendor);
            vendorID = vendor.Id;
            _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeIssueDate, model.IssueDate);

            //jaseefar commented for testing
            _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeExpiryDate, model.ExpiryDate);

            //  _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeExpiryDate, result.GetLicenseDetailsResponse.ExpiryDate);

            if (model.CategoryId != null)
                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeCategory, string.Join(",", model.CategoryId));




            return vendorID;
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult RegisterResult(int resultId)
        {
            var model = _customerModelFactory.PrepareRegisterResultModel(resultId);
            return View(model);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult RegisterResult(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToRoute("Homepage");

            return Redirect(returnUrl);
        }


        [HttpPost]
        //[PublicAntiForgery]
        [PublicStoreAllowNavigation(true)]
        [ActionName("Activate")]
        [FormValueRequired("activate-Skip")]
        public ActionResult ActivateAccountSkip(LoginModel model)
        {
            try
            {
                RegisterModel regmodel = JsonConvert.DeserializeObject<RegisterModel>(TempData["RegistrationData"].ToString());
                if (regmodel.VendorId > 0)
                {
                    var vendor = _vendorService.GetVendorById(regmodel.VendorId);
                    _vendorService.RemoveVendor(vendor);
                }

                string MATRequestType = TempData["email"] as string;
               // if (Request.Cookies["RequestType"] != null)
                if(MATRequestType!=null)
                {

                    //var aCookie = new CookieOptions();
                    //Response.Cookies.Append("Flage", Request.Cookies["Flage"], aCookie);
                    //Response.Cookies.Append("RequestType", "register", aCookie);
                    //Response.Cookies.Append("step", 2.ToString(), aCookie);


                    TempData["step"] = "2";
                    return RedirectToAction("Login");
                }
                else
                {
                    return Redirect("~/Login");
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message.ToString());
            }
            return View(model);
        }

        [HttpPost]
        //[PublicAntiForgery]
        [PublicStoreAllowNavigation(true)]
        [ActionName("Activate")]
        [FormValueRequired("activate-account")]
        public ActionResult ActivateAccount(LoginModel model)
        {
            try
            {
                string RequestType = TempData["Flage"] as string;
                if (1==1)
                {
                    string cokkieemail = TempData["email"]==null?"": TempData["email"].ToString();// Request.Cookies["email"];
                    //  Customer customer = _customerService.GetCustomerByEmail(cokkieemail);
                    Customer customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
                    if (customer == null)
                        return Redirect("~/login");

                    var cToken = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.AccountSmsActivationToken);
                    if (String.IsNullOrEmpty(cToken))
                        return Redirect("~/HomePage");

                    if (!cToken.Equals(model.ActivationCode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        TempData["ActivationMessage"] = _localizationService.GetResource("Activation.WrongActivationCode");

                        TempData["MATRequestType"] = "register";
                        TempData["Flage"] = TempData["Flage"];
                        TempData["email"] = cokkieemail;
                        TempData["step"] = "1";
                        return RedirectToAction("Login");
                    }

                    //activate user account
                    RegisterModel regmodel = JsonConvert.DeserializeObject<RegisterModel>(TempData["RegistrationData"].ToString());
                    bool isApproved = regmodel.IsUaePass ? true : false;
                    bool IsExternal = regmodel.IsUaePass ? true : false;

                    string RequestType_ = TempData["Flage"] == null ? "" : TempData["Flage"].ToString();// Request.Cookies["Flage"];


                    var registrationRequest = new CustomerRegistrationRequest(customer,
                       regmodel.Email,
                       _customerSettings.UsernamesEnabled ? regmodel.Username : regmodel.Email,
                       regmodel.Password,
                       _customerSettings.DefaultPasswordFormat,
                       _storeContext.CurrentStore.Id,
                       isApproved, IsExternal, registrationType: RequestType_, vendorID: regmodel.VendorId);
                    var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);


                    if (registrationResult.Success)
                    {
                        //properties
                        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        {
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute, regmodel.TimeZoneId);
                        }
                        //VAT number
                        if (_taxSettings.EuVatEnabled)
                        {
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute, regmodel.VatNumber);

                            var vatNumberStatus = _taxService.GetVatNumberStatus(regmodel.VatNumber, out _, out var vatAddress);
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(regmodel.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, regmodel.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }

                        //form fields
                        if (_customerSettings.GenderEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, regmodel.Gender);
                        if (_customerSettings.FirstNameEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, regmodel.FirstName);
                        if (_customerSettings.LastNameEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, regmodel.LastName);

                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions, regmodel.HasAcceptedTermsAndConditions);

                        if (_customerSettings.DateOfBirthEnabled)
                        {
                            var dateOfBirth = regmodel.ParseDateOfBirth();
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                        }
                        if (_customerSettings.CompanyEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, regmodel.Company);
                        if (_customerSettings.StreetAddressEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, regmodel.StreetAddress);
                        if (_customerSettings.StreetAddress2Enabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, regmodel.StreetAddress2);
                        if (_customerSettings.ZipPostalCodeEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, regmodel.ZipPostalCode);
                        if (_customerSettings.CityEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, regmodel.City);
                        if (_customerSettings.CountyEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, regmodel.County);
                        if (_customerSettings.CountryEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, regmodel.CountryId);
                        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute,
                                regmodel.StateProvinceId);
                        if (_customerSettings.PhoneEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, regmodel.Phone);
                        if (_customerSettings.FaxEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, regmodel.Fax);

                        //newsletter
                        if (_customerSettings.NewsletterEnabled)
                        {
                            //save newsletter value
                            var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                            if (newsletter != null)
                            {
                                if (regmodel.Newsletter)
                                {
                                    newsletter.Active = true;
                                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);

                                    //GDPR
                                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                    {
                                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                                    }
                                }
                                //else
                                //{
                                //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                                //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                                //}
                            }
                            else
                            {
                                if (regmodel.Newsletter)
                                {
                                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = model.Email,
                                        Active = true,
                                        StoreId = _storeContext.CurrentStore.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });

                                    //GDPR
                                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                    {
                                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                                    }
                                }
                            }
                        }

                        if (_customerSettings.AcceptPrivacyPolicyEnabled)
                        {
                            //privacy policy is required
                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                            {
                                _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.PrivacyPolicy"));
                            }
                        }

                        //GDPR


                        //save customer attributes
                        //_genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                        //login customer now
                        if (isApproved)
                            _authenticationService.SignIn(customer, true);


                    }

                    customer.Active = true;
                    _customerService.UpdateCustomer(customer);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, "");




                    //send welcome message
                    _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);
                    _eventPublisher.Publish(new CustomerRegisteredEvent(customer));
                    var aCookie = new CookieOptions()
                    {
                        Expires = DateTime.Now.AddDays(7)
                    };
                   // Response.Cookies.Append("RequestType", "register", aCookie);
                    //Response.Cookies.Append("Flage", Request.Cookies["Flage"], aCookie);
                    //Response.Cookies.Append("email", Request.Cookies["email"], aCookie);
                    //Response.Cookies.Append("step", 2.ToString(), aCookie);





                    TempData["step"] = "2";

                    TempData["ActivationMessage"] = _localizationService.GetResource("Account.AccountActivation.Activated");
                    _notificationService.SuccessNotification("Account.AccountActivation.Activated");
                    TempData["ShowTimer"] = "";
                    return RedirectToAction("login");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message.ToString());
            }
            TempData["ActivationMessage"] = _localizationService.GetResource("Activation.WrongActivationCode");
            //Response.Cookies.Append("step", 1.ToString(), aCookie1);
            return RedirectToAction("login");
        }

        [HttpPost]
        //[PublicAntiForgery]
        [PublicStoreAllowNavigation(true)]
        [ActionName("Activate")]
        [FormValueRequired("activate-resettoken")]
        public ActionResult ActivateResendToken(LoginModel model)
        {
            try
            {
                string MATRequestType = TempData["email"] as string;
                if (MATRequestType != null)
                {
                    string cokkieemail = Request.Cookies["email"];
                    //Customer customer = _customerService.GetCustomerByEmail(cokkieemail);
                    Customer customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
                    //if (string.IsNullOrWhiteSpace(customer.Email))
                    //    throw new ApplicationException("Not allowed");
                    if (customer == null)
                        throw new ApplicationException("Not allowed");

                    string randomNumber = RandomNumberService.RandomDigits(6);
                    customer.Email = cokkieemail != null ? cokkieemail.Trim() : "";
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountSmsActivationToken, randomNumber);



                    _eventPublisher.Publish<CustomerRegisteredEvent>(new CustomerRegisteredEvent(customer, RegistrationMethod.Mobile, isAproved: false));

                    TempData["ActivationMessage"] = _localizationService.GetResource("Activation.ResentActivationCode");
                    TempData["ShowTimer"] = 1;

                    TempData["MATRequestType"] = "register";
                    TempData["Flage"] = TempData["Flage"];
                    TempData["email"] = cokkieemail;
                    TempData["step"] = "1";
                    return RedirectToAction("login");
                }
                else
                {
                    return RedirectToAction("login");
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message.ToString());
            }
            return View(model);
        }

        [HttpPost]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult CheckUsernameAvailability(string username)
        {
            var usernameAvailable = false;
            var statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.NotAvailable");

            if (!UsernamePropertyValidator.IsValid(username, _customerSettings))
            {
                statusText = _localizationService.GetResource("Account.Fields.Username.NotValid");
            }
            else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
            {
                if (_workContext.CurrentCustomer != null &&
                    _workContext.CurrentCustomer.Username != null &&
                    _workContext.CurrentCustomer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.CurrentUsername");
                }
                else
                {
                    var customer = _customerService.GetCustomerByUsername(username);
                    if (customer == null)
                    {
                        statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            return Json(new { Available = usernameAvailable, Text = statusText });
        }

        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult AccountActivation(string token, string email, Guid guid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                customer = _customerService.GetCustomerByGuid(guid);

            if (customer == null)
                return Redirect("~/Homepage");

            var cToken = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.AccountActivationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
                return
                    View(new AccountActivationModel
                    {
                        Result = _localizationService.GetResource("Account.AccountActivation.AlreadyActivated")
                    });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return Redirect("~/Homepage");

            //activate user account
            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, "");
            //send welcome message
            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            //raise event       
            _eventPublisher.Publish(new CustomerActivatedEvent(customer));

            var model = new AccountActivationModel
            {
                Result = _localizationService.GetResource("Account.AccountActivation.Activated")
            };
            return View(model);
        }

        #endregion

        #region My account / Info

        [HttpsRequirement]
        public virtual IActionResult Info()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CustomerInfoModel();

            model = _customerModelFactory.PrepareCustomerInfoModel(model, _workContext.CurrentCustomer, false);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Info(CustomerInfoModel model, IFormCollection form)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var oldCustomerModel = new CustomerInfoModel();

            var customer = _workContext.CurrentCustomer;

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel = _customerModelFactory.PrepareCustomerInfoModel(oldCustomerModel, customer, false);

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = _gdprService
                    .GetAllConsents().Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            _customerRegistrationService.SetUsername(customer, model.Username.Trim());

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                _authenticationService.SignIn(customer, true);
                        }
                    }
                    //email
                    if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        _customerRegistrationService.SetEmail(customer, model.Email.Trim(), requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                _authenticationService.SignIn(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute,
                            model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.VatNumberAttribute);

                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute,
                            model.VatNumber);
                        if (prevVatNumber != model.VatNumber)
                        {
                            var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out _, out var vatAddress);
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                    }

                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);

                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        var dateSplit = model.DateOfBirth.Split('/');
                        model.DateOfBirthYear = model.DateOfBirthYear == null ? Convert.ToInt32( dateSplit[2]) : model.DateOfBirthYear;
                        model.DateOfBirthMonth = model.DateOfBirthMonth == null ? Convert.ToInt32( dateSplit[1]) : model.DateOfBirthMonth;
                        model.DateOfBirthDay = model.DateOfBirthDay == null ? Convert.ToInt32( dateSplit[0]) : model.DateOfBirthDay;

                        var dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, model.City);
                    if (_customerSettings.CountyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, model.County);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.NationalityCountryId, model.CountryNationalityId);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId);


                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                var wasActive = newsletter.Active;
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            else
                            {
                                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                    //save customer attributes
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);
                    model.customerInfoStatus = true;
                    ViewBag.customerInfoStatus = true;
                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                        LogGdpr(customer, oldCustomerModel, model, form);

                    //return RedirectToRoute("CustomerInfo");
                    model = _customerModelFactory.PrepareCustomerInfoModel(model, customer, true, customerAttributesXml);

                    return View(model);
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerInfoModel(model, customer, true, customerAttributesXml);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult RemoveExternalAssociation(int id)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            //ensure it's our record
            var ear = _externalAuthenticationService.GetExternalAuthenticationRecordById(id);

            if (ear == null)
            {
                return Json(new
                {
                    redirect = Url.Action("Info"),
                });
            }

            _externalAuthenticationService.DeleteExternalAuthenticationRecord(ear);

            return Json(new
            {
                redirect = Url.Action("Info"),
            });
        }

        [HttpsRequirement]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult EmailRevalidation(string token, string email, Guid guid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                customer = _customerService.GetCustomerByGuid(guid);

            if (customer == null)
                return RedirectToRoute("Homepage");

            var cToken = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
                return View(new EmailRevalidationModel
                {
                    Result = _localizationService.GetResource("Account.EmailRevalidation.AlreadyChanged")
                });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToRoute("Homepage");

            if (string.IsNullOrEmpty(customer.EmailToRevalidate))
                return RedirectToRoute("Homepage");

            if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
                return RedirectToRoute("Homepage");

            //change email
            try
            {
                _customerRegistrationService.SetEmail(customer, customer.EmailToRevalidate, false);
            }
            catch (Exception exc)
            {
                return View(new EmailRevalidationModel
                {
                    Result = _localizationService.GetResource(exc.Message)
                });
            }
            customer.EmailToRevalidate = null;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute, "");

            //re-authenticate (if usernames are disabled)
            if (!_customerSettings.UsernamesEnabled)
            {
                _authenticationService.SignIn(customer, true);
            }

            var model = new EmailRevalidationModel()
            {
                Result = _localizationService.GetResource("Account.EmailRevalidation.Changed")
            };
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult DeleteCustomerAccount(DeleteCustomerAccountModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();
            var customer = _workContext.CurrentCustomer;
            var response = _customerModelFactory.PrepareDeleteCustomerAccountModel(model, customer);
            var reason = _lookupService.GetlookupById(model.ReasonId, _workContext.WorkingLanguage.Id);
            if (model.ReasonId == 5)
            {
                reason.Value = model.Comments;
            }
            if (_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                var vendor = _vendorService.GetVendorById(customer.VendorId);
                _workflowMessageService.SendVendorAccountDeleteNotification(vendor: vendor, languageId: _workContext.WorkingLanguage.Id, null, email: customer.Email.ToString()
               , reason.Value);
            }
            else
                _workflowMessageService.SendCustomerAccountDeleteNotification(customer: customer, languageId: _workContext.WorkingLanguage.Id, null, email: customer.Email.ToString()
                    , reason.Value);

            //activity log
            _customerActivityService.InsertActivity("DeleteCustomerRequest",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id), customer);
            return Json(new
            {
                redirect = Url.RouteUrl("CustomerInfo"),
            });
            //return RedirectToAction("info");
        }
        #endregion

        #region My account / Addresses

        [HttpsRequirement]
        public virtual IActionResult Addresses()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = _customerModelFactory.PrepareCustomerAddressListModel();
            return View(model);
        }

        [HttpPost]
        [HttpsRequirement]
        public virtual IActionResult SetDefaultAddress(int addressId)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            int CurrentDefAddress = customer.BillingAddressId > 0 ? Convert.ToInt32(customer.BillingAddressId) : 0;

            //find address (ensure that it belongs to the current customer)
            var address = _customerService.GetCustomerAddress(customer.Id, CurrentDefAddress);
            if (address == null)
            {
                //inserting current def address if it s not there customer address table
                var Newaddress = _addressService.GetAddressById(CurrentDefAddress);
                _customerService.InsertCustomerAddress(customer, Newaddress);
            }
            //setting the new def address in customer table
            customer.BillingAddressId = addressId;
            customer.ShippingAddressId = addressId;
            _customerService.UpdateCustomer(customer);

            //redirect to the address list page
            return Json(new
            {
                redirect = Url.RouteUrl("CustomerAddresses"),
            });
        }

        [HttpPost]
        [HttpsRequirement]
        public virtual IActionResult AddressDelete(int addressId)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            //find address (ensure that it belongs to the current customer)
            var address = _customerService.GetCustomerAddress(customer.Id, addressId);

            if (address != null)
            {
                if (address.Id == customer.BillingAddressId)
                {
                    var newBillingAddRef = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id)
                        .Where(x => x.Id != address.Id).OrderByDescending(o => o.CreatedOnUtc).FirstOrDefault();

                    if (newBillingAddRef != null)
                    {
                        customer.BillingAddressId = newBillingAddRef.Id;
                        customer.ShippingAddressId = newBillingAddRef.Id;
                        _customerService.UpdateCustomer(customer);
                    }
                    else
                    {
                        customer.BillingAddressId = null;
                        customer.ShippingAddressId = null;
                        _customerService.UpdateCustomer(customer);
                    }
                }
                _customerService.RemoveCustomerAddress(customer, address);
                //_customerService.UpdateCustomer(customer);
                //now delete the address record
                _addressService.DeleteAddress(address);
            }

            //redirect to the address list page
            return Json(new
            {
                redirect = Url.RouteUrl("CustomerAddresses"),
            });
        }

        [HttpsRequirement]
        public virtual IActionResult AddressAdd()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,

                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressAdd(CustomerAddressEditModel model, IFormCollection form)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //custom address attributes
            var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                address.IsDefault = model.Address.IsDefault;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                if (model.Address.IsDefault)
                {
                    var addresss = _customerService.GetAddressesByCustomerId(customer.Id).ToList();
                    foreach (var add in addresss)
                    {
                        add.IsDefault = false;
                        _addressService.UpdateAddress(add);
                    }
                }
                _addressService.InsertAddress(address);
                _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);
                if (customer.BillingAddressId == null || customer.BillingAddressId == 0)
                {
                    customer.BillingAddressId = address.Id;
                    customer.ShippingAddressId = address.Id;
                    _customerService.UpdateCustomer(customer);
                }

                //if (model.Address.IsDefault)
                //{

                //    int CurrentDefAddress = customer.BillingAddressId > 0 ? Convert.ToInt32(customer.BillingAddressId) : 0;
                //    var Newaddress = _addressService.GetAddressById(CurrentDefAddress);
                //    _customerService.InsertCustomerAddress(customer, Newaddress);
                //    _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);
                //    customer.BillingAddressId = address.Id;
                //    customer.ShippingAddressId = address.Id;
                //    _customerService.UpdateCustomer(customer);
                //}
                //else
                //{
                //    _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);
                //}
                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);

            return View(model);
        }

        [HttpsRequirement]
        public virtual IActionResult AddressEdit(int addressId)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = _customerService.GetCustomerAddress(customer.Id, addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: false,
                customer: _workContext.CurrentCustomer,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressEdit(CustomerAddressEditModel model, int addressId, IFormCollection form)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = _customerService.GetCustomerAddress(customer.Id, addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            //custom address attributes
            var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                if (model.Address.IsDefault && address.IsDefault == false)
                {
                    var addresss = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id).ToList();
                    foreach (var add in addresss)
                    {
                        add.IsDefault = false;
                        _addressService.UpdateAddress(add);
                    }
                }
                address.IsDefault = model.Address.IsDefault;
                _addressService.UpdateAddress(address);
                //if (model.Address.IsDefault)
                //{
                //    int CurrentDefAddress = customer.BillingAddressId > 0 ? Convert.ToInt32(customer.BillingAddressId) : 0;
                //    var Newaddress = _addressService.GetAddressById(CurrentDefAddress);
                //    _customerService.InsertCustomerAddress(customer, Newaddress);
                //    customer.BillingAddressId = address.Id;
                //    customer.ShippingAddressId = address.Id;
                //    _customerService.UpdateCustomer(customer);
                //}
                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);
            return View(model);
        }

        #endregion

        #region My account / Downloadable products

        [HttpsRequirement]
        public virtual IActionResult DownloadableProducts()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (_customerSettings.HideDownloadableProductsTab)
                return RedirectToRoute("CustomerInfo");

            var model = _customerModelFactory.PrepareCustomerDownloadableProductsModel();
            return View(model);
        }

        public virtual IActionResult UserAgreement(Guid orderItemId)
        {
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return RedirectToRoute("Homepage");

            var product = _productService.GetProductById(orderItem.ProductId);

            if (product == null || !product.HasUserAgreement)
                return RedirectToRoute("Homepage");

            var model = _customerModelFactory.PrepareUserAgreementModel(orderItem, product);
            return View(model);
        }

        #endregion

        #region My account / Change password

        [HttpsRequirement]
        public virtual IActionResult ChangePassword()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = _customerModelFactory.PrepareChangePasswordModel();

            //display the cause of the change password 
            if (_customerService.PasswordIsExpired(_workContext.CurrentCustomer))
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region My account / Avatar

        [HttpsRequirement]
        public virtual IActionResult Avatar()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var model = new CustomerAvatarModel();
            model = _customerModelFactory.PrepareCustomerAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [FormValueRequired("upload-avatar")]
        public virtual IActionResult UploadAvatar(CustomerAvatarModel model, IFormFile uploadedFile)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                try
                {
                    var customerAvatar = _pictureService.GetPictureById(_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
                    if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
                    {
                        var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                        if (uploadedFile.Length > avatarMaxSize)
                            throw new NopException(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                        var customerPictureBinary = _downloadService.GetDownloadBits(uploadedFile);
                        if (customerAvatar != null)
                            customerAvatar = _pictureService.UpdatePicture(customerAvatar.Id, customerPictureBinary, uploadedFile.ContentType, null);
                        else
                            customerAvatar = _pictureService.InsertPicture(customerPictureBinary, uploadedFile.ContentType, null);
                    }

                    var customerAvatarId = 0;
                    if (customerAvatar != null)
                        customerAvatarId = customerAvatar.Id;

                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AvatarPictureIdAttribute, customerAvatarId);

                    model.AvatarUrl = _pictureService.GetPictureUrl(
                        _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                        _mediaSettings.AvatarPictureSize,
                        false);
                    return View(model);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [FormValueRequired("remove-avatar")]
        public virtual IActionResult RemoveAvatar(CustomerAvatarModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;

            var customerAvatar = _pictureService.GetPictureById(_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
            if (customerAvatar != null)
                _pictureService.DeletePicture(customerAvatar);
            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AvatarPictureIdAttribute, 0);

            return RedirectToRoute("CustomerAvatar");
        }

        #endregion

        #region GDPR tools

        [HttpsRequirement]
        public virtual IActionResult GdprTools()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (!_gdprSettings.GdprEnabled)
                return RedirectToRoute("CustomerInfo");

            var model = _customerModelFactory.PrepareGdprToolsModel();
            return View(model);
        }

        [HttpPost, ActionName("GdprTools")]
        [FormValueRequired("export-data")]
        public virtual IActionResult GdprToolsExport()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (!_gdprSettings.GdprEnabled)
                return RedirectToRoute("CustomerInfo");

            //log
            _gdprService.InsertLog(_workContext.CurrentCustomer, 0, GdprRequestType.ExportData, _localizationService.GetResource("Gdpr.Exported"));

            //export
            var bytes = _exportManager.ExportCustomerGdprInfoToXlsx(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            return File(bytes, MimeTypes.TextXlsx, "customerdata.xlsx");
        }

        [HttpPost, ActionName("GdprTools")]
        [FormValueRequired("delete-account")]
        public virtual IActionResult GdprToolsDelete()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (!_gdprSettings.GdprEnabled)
                return RedirectToRoute("CustomerInfo");

            //log
            _gdprService.InsertLog(_workContext.CurrentCustomer, 0, GdprRequestType.DeleteCustomer, _localizationService.GetResource("Gdpr.DeleteRequested"));

            var model = _customerModelFactory.PrepareGdprToolsModel();
            model.Result = _localizationService.GetResource("Gdpr.DeleteRequested.Success");
            return View(model);
        }

        #endregion

        #region Check gift card balance

        //check gift card balance page
        [HttpsRequirement]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult CheckGiftCardBalance()
        {
            if (!(_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance))
            {
                return RedirectToRoute("CustomerInfo");
            }

            var model = _customerModelFactory.PrepareCheckGiftCardBalanceModel();
            return View(model);
        }

        [HttpPost, ActionName("CheckGiftCardBalance")]
        [FormValueRequired("checkbalancegiftcard")]
        [ValidateCaptcha]
        public virtual IActionResult CheckBalance(CheckGiftCardBalanceModel model, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                var giftCard = _giftCardService.GetAllGiftCards(giftCardCouponCode: model.GiftCardCode).FirstOrDefault();
                if (giftCard != null && _giftCardService.IsGiftCardValid(giftCard))
                {
                    var remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_giftCardService.GetGiftCardRemainingAmount(giftCard), _workContext.WorkingCurrency);
                    model.Result = _priceFormatter.FormatPrice(remainingAmount, true, false);
                }
                else
                {
                    model.Message = _localizationService.GetResource("CheckGiftCardBalance.GiftCardCouponCode.Invalid");
                }
            }

            return View(model);
        }

        #endregion

        #endregion
        #region Product Review
        [HttpGet]
        public IActionResult CustomerBulkProductReview()
        {
            return Json("");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult CustomerBulkProductReview([FromBody] List<CustomerProductReviewModelRequest> Reviews)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var customerProductReviewModelRequests = new List<CustomerProductReviewModelRequest>();

            foreach (var model in Reviews)
            {
                var review = new CustomerProductReviewModelRequest
                {
                    Rating = model.Rating,
                    ReviewText = model.ReviewText,
                    ProductId = model.ProductId
                };
                if (review.Rating != null)
                    customerProductReviewModelRequests.Add(review);
            }
            var response = _customerModelFactory.AddCustomerBulkProductReview(customerProductReviewModelRequests);
            return Json(response);

        }
        #endregion

        #region My account / Followed Vendors

        [HttpsRequirement]
        public ActionResult FollowedVendors(CatalogPagingFilteringModel command)
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return RedirectToRoute("HomePage");

            var model = _vendorModelFactory.PrepareFollowedVendor(command);
            return View(model);
        }

        #endregion
    }
}