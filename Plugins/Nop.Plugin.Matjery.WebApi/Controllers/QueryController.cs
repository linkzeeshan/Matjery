using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Constant;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Plugin.Matjery.WebApi.Services;
using Nop.Services;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Constants;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Nop.Plugin.Matjery.WebApi.Models.ParamsModel;

namespace Nop.Plugin.Matjery.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : BaseController
    {
        #region Initialization

        private readonly ICategoryService _categoryService;
        private readonly ICategoryPluginService _categoryPluginService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductPluginService _productService;
        private readonly IUAEPassPluginService _uAEPassPluginService;
        private readonly ICustomerPluginService _customerPluginService;
        private readonly IAddressPluginService _addressPluginService;
        private readonly IRegionPluginService _regionPluginService;
        private readonly IBlackpointPluginService _blackpointPluginService;
        private readonly ICartPluginService _cartPluginService;
        private readonly IOrderPluginService _orderPluginService;
        private readonly IProductAttributePluginService _productAttributePluginService;
        private readonly IWishListPluginService _wishListPluginService;
        private readonly IConstantService _constantService;
        private readonly ICheckOutPluginService _checkOutPluginService;
        private readonly ICampaignPluginService _campaignPluginService;
        private readonly IPushNotificationPluginService _pushNotificationPluginService;
        private readonly IReviewPluginService _reviewPluginService;
        private readonly IVendorPluginService _vendorPluginService;
        private readonly ISearchPluginService _searchPluginService;
        private readonly VendorSettings _vendorSettings;
        protected readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly IFollowersPluginService _followersPluginService;
        private readonly ITopicsPluginService _topicsPluginService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        protected readonly CustomerSettings _customerSettings;
        protected readonly CommonSettings _commonsettings;
        protected readonly IGenericAttributeService _genericAttributeService;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILookupPluginService _lookupPluginService;
        private readonly ITradeLicensePluginService _tradeLicensePluginService;
        private readonly Nop.Services.Logging.ILogger _logger;
        public QueryController(
            IExternalAuthenticationService externalAuthenticationService,
            ICategoryPluginService categoryPluginService,
            IGenericAttributeService genericAttributeService,
            ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            IProductPluginService productService,
            IUAEPassPluginService uAEPassPluginService,
            ICustomerPluginService customerPluginService,
            IAddressPluginService addressPluginService,
            IRegionPluginService regionPluginService,
            IBlackpointPluginService blackpointPluginService,
            ICartPluginService cartPluginService,
            IOrderPluginService orderPluginService,
            IProductAttributePluginService productAttributePluginService,
            IWishListPluginService wishListPluginService,
            IConstantService constantService,
            ICheckOutPluginService checkOutPluginService,
            ICampaignPluginService campaignPluginService,
            IPushNotificationPluginService pushNotificationPluginService,
            IReviewPluginService reviewPluginService,
            ISearchPluginService searchPluginService, IVendorPluginService vendorPluginService, VendorSettings vendorSettings,
            ICustomerService customerService, IVendorService vendorService, IFollowersPluginService followersPluginService,
            ITopicsPluginService topicsPluginService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILookupPluginService lookupPluginService,
            Nop.Services.Logging.ILogger logger,
            ITradeLicensePluginService tradeLicensePluginService

            )
        {
            _genericAttributeService = genericAttributeService;
            _externalAuthenticationService = externalAuthenticationService;
            _categoryService = categoryService;
            _commonsettings= EngineContext.Current.Resolve<CommonSettings>();
            _customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
            _categoryPluginService = categoryPluginService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _uAEPassPluginService = uAEPassPluginService;
            _customerPluginService = customerPluginService;
            _addressPluginService = addressPluginService;
            _regionPluginService = regionPluginService;
            _blackpointPluginService = blackpointPluginService;
            _cartPluginService = cartPluginService;
            _orderPluginService = orderPluginService;
            _productAttributePluginService = productAttributePluginService;
            _wishListPluginService = wishListPluginService;
            _constantService = constantService;
            _checkOutPluginService = checkOutPluginService;
            _campaignPluginService = campaignPluginService;
            _pushNotificationPluginService = pushNotificationPluginService;
            _searchPluginService = searchPluginService;
            _reviewPluginService = reviewPluginService;
            _vendorSettings = vendorSettings;
            _customerService = customerService;
            _vendorPluginService = vendorPluginService;
            _vendorService = vendorService;
            _followersPluginService = followersPluginService;
            _topicsPluginService = topicsPluginService;
            _languageService = languageService;
            _localizationService = localizationService;
            _lookupPluginService = lookupPluginService;
            _logger = logger;
            _tradeLicensePluginService = tradeLicensePluginService;
        }
        #endregion

        #region Public

        #region Category

        /// <summary>
        /// To Get Top menu Category
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTopMenuCategories", Name = "Get Top Menu Categories")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetTopMenuCategories(int rootCategoryId)
        {
            try
            {
                IList<Category> categories = _categoryService.GetAllCategories().Where(c => c.ParentCategoryId == rootCategoryId && c.IncludeInTopMenu).ToList<Category>();
                var topMenuCategories = _categoryPluginService.TopMenuCategory(categories);
                var result = Prepare(topMenuCategories);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// To Get Home Page Category
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomepageCategories", Name = "Get Homepage Categories")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetHomepageCategories()
        {
            try
            {
                IList<Category> categories = _categoryService.GetAllCategoriesDisplayedOnHomepage();
                var homePageCategories = _categoryPluginService.TopMenuCategory(categories);
                var result = Prepare(homePageCategories);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion

        #region Product

        /// <summary>
        /// To Get Slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetIdBySlug", Name = "Get Slug")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetIdBySlug(string slug)
        {
            try
            {
                var entity = _urlRecordService.GetBySlug(slug) ?? new Core.Domain.Seo.UrlRecord();

                var obj = new { Id = entity.EntityId, Name = entity.EntityName };

                var result = Prepare(obj);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// Get Homepage Products
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomepageProducts", Name = "Get Homepage Products")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetHomepageProducts()
        {
            try
            {
                var homepageProducts = _productService.GetHomePageProducts();
                var result = Prepare(homepageProducts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// Get Homepage Best Sellers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomepageBestSellers", Name = "Get Homepage Best Sellers")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetHomepageBestSellers()
        {
            try
            {

                var bestSellerProducts = _productService.GetHomePageBestSeller();
                var result = Prepare(bestSellerProducts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get Homepage Discounted Products
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomepageDiscountedProducts", Name = "Get Homepage Discounted Products")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetHomepageDiscountedProducts([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var bestSellerProducts = _productService.GetHomePageDiscountedPage(pageIndex, pageSize);
                var result = Prepare(bestSellerProducts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// Get ProductDetails
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductDetails", Name = "Get ProductDetails")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetProductDetails(int productId)
        {
            try
            {
                var model = _productService.GetProductDetails(productId);
                var result = Prepare(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

       

        #endregion

        #region External
        [HttpPost]
        [Route("GetExternalAuthUser", Name = "Get External AuthUser")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetExternalAuthUser(ParamsModel.ExtAuthParamsModel model)
        {
            try
            {
                var apiValidationResult = new ApiValidationResultResponse();
                var apiresult = new ExternalAuthRecordResult();
                var objmodel = new ExternalAuthRecordResult();
                var response = _externalAuthenticationService.GetExternalAuthUser(model.ProviderSystemName,model.Email);
                if (response != null)
                {
                   
                    var customer = _customerService.GetCustomerById(response.CustomerId);
                    objmodel.IsDeleted = customer.Deleted;
                    objmodel.IsActive = customer.Active;
                    if (!customer.Deleted)
                    {
                        objmodel.Status = HttpStatusCode.OK;
                        objmodel.PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
                        objmodel.Name = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute) + ' ' + _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
                        objmodel.ExternalAuthenticationRecord = response;
                        objmodel.isUserExist = true;
                        var result = this.Prepare(objmodel, apiValidationResult);
                        return Ok(result);
                    }
                    else
                    {
                        objmodel.Status = HttpStatusCode.OK;
                        objmodel.isUserExist = false;
        
                        objmodel.ExternalAuthenticationRecord = null;
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription = "Your account is deleted as per your request. Kindly contact administrator for any further queries.",
                            fieldName = "",

                        });
                        var result = Prepare(objmodel, apiValidationResult);
                        result.status = HttpStatusCode.OK;
                        return Ok(result);

                    }
                }
                else
                {
 
                    objmodel.Status = HttpStatusCode.OK;
                    objmodel.isUserExist  = false;
                    objmodel.ExternalAuthenticationRecord = null;
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = "Email id does'nt exists",
                        fieldName = "",

                    });
                    var result = Prepare(objmodel, apiValidationResult);
                    result.status = HttpStatusCode.OK; 
                    return Ok(result);
                }
               
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion
        #region uaepass
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UaePassUserExist", Name = "Uae Pass UserExist")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult UaePassUserExist(ParamsModel.UaePassParamsModel model)
        {
            try
            {
                var response = _uAEPassPluginService.UAEPassExists(model);
                var result = this.Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateUserFromUaePass", Name = "Update User From UaePass")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult UpdateUserFromUaePass(ParamsModel.UaePassParamsModel model)
        {
            try
            {
                var response = _uAEPassPluginService.UpdateUAEPassUser(model);
                var result = this.Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion

        #region Customer

        [HttpPost]
        [Route("ValidateCustomer", Name = "Validate Customer")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult ValidateCustomer(ParamsModel.LoginParamsModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UsernameOrEmail))
                    return BadRequest();

                var result = _customerPluginService.ValidateCustomer(model);

                return Ok(Prepare(true));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetGuestCustomer
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetGuestCustomer", Name = "GetGuestCustomer")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetGuestCustomer(bool IsUpgradedVersion)
        {
            try
            {
                var loginAsGuestResult = _customerPluginService.GetGuestCustomer();
                if (IsUpgradedVersion == true)
                {
                    var result = Prepare(loginAsGuestResult);
                    return Ok(result);
                }
                else
                {
                    var result = PrepareStringResponse(loginAsGuestResult);
                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login", Name = "Login")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult Login(ParamsModel.LoginParamsModel model)
        {
            try
            {
                var (loginResult, apiValidationResult) = _customerPluginService.Login(model);
                var result = Prepare(loginResult, apiValidationResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// LoginExternal
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LoginExternal", Name = "LoginExternal")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult LoginExternal([FromForm]LoginExternalParamsModel model,IFormFile file)
        {
            try
            {
                if (string.IsNullOrEmpty(model.RegistrationType))
                {
                    return BadRequest("Registration type should be either seller or buyer");
                }
                if (model.RegistrationType.ToLower() == "seller")
                {
                    model.VendorParamsString = model.VendorParamsString.Trim('"');
                    if (!string.IsNullOrEmpty(model.VendorParamsString))
                    {
                        var obj = JsonConvert.DeserializeObject<VendorApplyParamsModel>(model.VendorParamsString);
                        model.VendorApplyParamsModel = obj;
                    }

                    if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                        return BadRequest("can't create seller account ");
                }
                var (loginResult, apiValidationResult) = _customerPluginService.LoginExternal(model);
                var result = Prepare(loginResult, apiValidationResult);

                //var currentVendor = _vendorService.GetAllVendors(name: model.VendorApplyParamsModel.Name, pageIndex: 1, pageSize: 20, showHidden: false);

                if (model.RegistrationType.ToLower() == "seller" && apiValidationResult.fieldValidationResult.Count <= 0)
                {
                    var customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
                    _workContext.CurrentCustomer = customer;
                    var (vendorResult, vendorApiValidationResult) = _vendorPluginService.ApplyVendor(model.VendorApplyParamsModel, file);
                    vendorResult.Customer = _customerPluginService.GetCustomerInfo();
                    var result2 = base.Prepare(vendorResult, vendorApiValidationResult);
                    return Ok(result2);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GetCustomerInfo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerSettings", Name = "GetCustomerSettings")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]

        public IActionResult GetCustomerSettings()
        {
            try
            {
                var PasswordComplexity = new PasswordComplexity()
                {
                    PasswordMinLength = _customerSettings.PasswordMinLength,
                    PasswordRequireDigit=_customerSettings.PasswordRequireDigit,
                    PasswordRequireLowercase=_customerSettings.PasswordRequireLowercase,
                    PasswordRequireNonAlphanumeric=_customerSettings.PasswordRequireNonAlphanumeric,
                    PasswordRequireUppercase=_customerSettings.PasswordRequireUppercase,
                    RegExpUppercase = "(?=.*?[A-Z])",
                    RegExpLowercase = "(?=.*?[a-z])",
                    RegExpDigit = "(?=.*?[0-9])",
                    RegExpNonAlphanumeric = "(?=.*?[#?!@$%^&*-])",
                };
                var result = Prepare(PasswordComplexity);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetCustomerInfo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerInfo", Name = "GetCustomerInfo")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetCustomerInfo()
        {
            try
            {

                var customerInfoResult = _customerPluginService.GetCustomerInfo();
                var result = Prepare(customerInfoResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// l
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("Register", Name = "Register")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult Register([FromForm]RegistrationParamsModel model, [FromForm] IFormFile file)
        {
            try
            {

               
                if (string.IsNullOrEmpty(model.RegistrationType))
                {
                     return BadRequest("Registration type should be either seller or buyer");
                }
                if (model.RegistrationType.ToLower() == "seller")
                {
                    model.VendorParamsString = model.VendorParamsString.Trim('"');
                    if (!string.IsNullOrEmpty(model.VendorParamsString))
                    {
                        var obj = JsonConvert.DeserializeObject<VendorApplyParamsModel>(model.VendorParamsString);
                        model.VendorApplyParamsModel = obj;
                    }

                    if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                        return BadRequest("can't create seller account ");
                }
                if (_workContext.CurrentCustomer.VendorId > 0)
                {
                    //already applied for vendor accountsave
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = _localizationService.GetResource("Vendors.ApplyAccount.AlreadyApplied") });
                }
               
                var (registrationResult, apiValidationResult) = _customerPluginService.Register(model);
                var result = Prepare(registrationResult, apiValidationResult);
           
                if (model.RegistrationType.ToLower() == "seller" && apiValidationResult.fieldValidationResult.Count<=0)
                {
                    //var httpPostedFile = Request.Form.Files.FirstOrDefault();
                    var customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
                    _workContext.CurrentCustomer = customer;
                    var response = _vendorPluginService.ApplyVendor(model.VendorApplyParamsModel, file);
                    response.Item1.Customer = _customerPluginService.GetCustomerInfo();
                    var result2 = base.Prepare(response.Item1, response.Item2);
                    return Ok(result2);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        [HttpPost]
        [Route("SendSmsCode", Name = "SendSmsCode")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult SendSmsCode([FromForm] RegistrationParamsModel model)
        {
            try
            {
                //model.VendorApplyParamsModel.parse

                if (model.RegistrationType.ToLower() == "seller")
                {
                    if (!string.IsNullOrEmpty(model.VendorParamsString))
                    {
                        model.VendorParamsString = model.VendorParamsString.Trim('"');
                        var obj = JsonConvert.DeserializeObject<VendorApplyParamsModel>(model.VendorParamsString);
                        model.VendorApplyParamsModel = obj;
                    }
             
                }
                var (registrationResult, apiValidationResult) = _customerPluginService.SendSmsCode(model);
                var result = Prepare(registrationResult, apiValidationResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// ActivateAccountBySms_Latest
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ActivateAccountBySms_Latest", Name = "Activate AccountBySms_Latest")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult ActivateAccountBySms_Latest(ParamsModel.AccountActivationParamsModel model)
        {
            try
            {
                var (success, apiValidationResult) = _customerPluginService.ActivateAccountBySms_Latest(model);

                var result = Prepare(success, apiValidationResult);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// ActivateAccountBySms
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ActivateAccountBySms", Name = "ActivateAccountBySms")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult ActivateAccountBySms(ParamsModel.AccountActivationParamsModel model)
        {
            try
            {
                var (success, apiValidationResult) = _customerPluginService.ActivateAccountBySms(model);
                var result = Prepare(success, apiValidationResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// ResendActivationToken
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ResendActivationToken", Name = "ResendActivationToken")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult ResendActivationToken(ParamsModel.AccountActivationParamsModel model)
        {
            try
            {
                var reponse = _customerPluginService.ResendActivationToken(model);
                var result = Prepare(reponse);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PasswordRecoverySend
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PasswordRecoverySend", Name = "PasswordRecoverySend")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult PasswordRecoverySend(ParamsModel.PasswordRecoveryParamsModel model)
        {
            try
            {
                var (passwordRecoveryResult, validationResult) = _customerPluginService.PasswordRecoverySend(model);
                var result = Prepare(passwordRecoveryResult, validationResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// ChangePassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("ChangePassword", Name = "ChangePassword")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult ChangePassword(ParamsModel.ChangePasswordParamsModel model)
        {
            try
            {
                var changePasswordResult = _customerPluginService.ChangePassword(model);
                var result = Prepare(changePasswordResult);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {

                return Unauthorized();

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// SaveCustomerInfo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveCustomerInfo", Name = "SaveCustomerInfo")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult SaveCustomerInfo(ParamsModel.CustomerInfoParamsModel model)
        {
            try
            {
                var (editCustomerInfoResult, apiValidationResult) = _customerPluginService.SaveCustomerInfo(model);
                var result = Prepare(editCustomerInfoResult, apiValidationResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// SaveCustomerInfo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteCustomerAccountRequest", Name = "DeleteCustomerAccountRequest")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult DeleteCustomerAccountRequest(DeleteCustomerAccountParamsModel model)
        {
            try
            {
                var apiValidationResult = new ApiValidationResult();
                var customer = _workContext.CurrentCustomer;

                (DeleteCustomerAccountResult, ApiValidationResultResponse) response = _customerPluginService.DeleteCustomerAccountRequest(model);

                var result = Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// UpdateTermsAndConditions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateTermsAndConditions", Name = "UpdateTermsAndConditions")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult UpdateTermsAndConditions(ParamsModel.CustomerTermsAndConditionsParamsModel model)
        {
            try
            {
                var response = _customerPluginService.UpdateTermsAndConditions(model);

                var result = Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Address
        /// <summary>
        /// GetAddresses
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAddresses", Name = "GetAddresses")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetAddresses()
        {
            try
            {
                var addressResults = _addressPluginService.GetAddresses();
                if (addressResults.Count > 0)
                {
                    addressResults.OrderBy(x => x.IsDefault).ToList();
                }
                var results = base.Prepare(addressResults);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetAddress
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAddress", Name = "GetAddress")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetAddress(int addressId)
        {
            try
            {
                var addressResult = _addressPluginService.GetAddress(addressId);
                var result = base.Prepare(addressResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }
        /// <summary>
        /// AddressAdd
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddressAdd", Name = "AddressAdd")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult AddressAdd(ParamsModel.AddressParamsModel model)
        {
            try
            {
                var response = _addressPluginService.AddressAdd(model);
                var result = base.Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// UpdateAddress
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateAddress", Name = "UpdateAddress")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult UpdateAddress(ParamsModel.AddressParamsModel model)
        {
            try
            {
                var response = _addressPluginService.UpdateAddress(model);
                var result = base.Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }

        /// <summary>
        /// AddressDelete
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddressDelete", Name = "AddressDelete")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult AddressDelete(ParamsModel.AddressParamsModel model)
        {
            try
            {
                var response = _addressPluginService.AddressDelete(model);
                if (!response)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = "Default Address can't delete.change default address" });
                }
                else
                {
                    var result = base.Prepare(response);
                    return Ok(result);
                }
              
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Region
        /// <summary>
        /// GetCountryList
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCountryList", Name = "GetCountryList")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetCountryList()
        {
            try
            {
                var countryResultList = _regionPluginService.GetCountryList();
                var result = base.Prepare(countryResultList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetStateProvinceList
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetStateProvinceList", Name = "GetStateProvinceList")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetStateProvinceList(int countryId)
        {
            try
            {
                var stateProviceResultList = _regionPluginService.GetStateProvinceList(countryId);
                var result = base.Prepare(stateProviceResultList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }
        #endregion

        #region BlackPoint
        /// <summary>
        /// AddBlackPoint
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddBlackPoint", Name = "AddBlackPoint")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult AddBlackPoint(ParamsModel.BlackPointParamsModel model)
        {
            try
            {
                bool result = false;
                if (!ModelState.IsValid)
                    result = false;
                else
                    result = _blackpointPluginService.AddBlackPoint(model);
                    var resultvalue = base.Prepare(result);
               
                return Ok(resultvalue);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion

        #region Cart
        /// <summary>
        /// AddToCart
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddToCart", Name = "AddToCart")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult AddToCart(ParamsModel.CartParamsModel model)
        {
            try
            {
                var warnnings = _cartPluginService.AddToCart(model);
                var apiValidationResult = new ApiValidationResultResponse();
                var result = Prepare(true);
                if (warnnings.Count > 0)
                {
                    foreach (string warnning in warnnings)
                    {
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription = warnning
                        });
                    }
                    result = Prepare(false, apiValidationResult);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// UpdatCartQuantity
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatCartQuantity", Name = "UpdatCartQuantity")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult UpdatCartQuantity(ParamsModel.CartParamsModel model)
        {
            try
            {
                var warnnings = _cartPluginService.UpdatCartQuantity(model);
                var apiValidationResult = new ApiValidationResultResponse();
                ApiResultResponse result;
                if (warnnings.Count > 0)
                {
                    foreach (string warnning in warnnings)
                    {
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription = warnning
                        });
                    }
                    result = Prepare(false, apiValidationResult);
                }
                else
                {
                    result = Prepare(true);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message= ex.Message});
            }
        }
        /// <summary>
        /// DeleteFromCart
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteFromCart", Name = "DeleteFromCart")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult DeleteFromCart(ParamsModel.CartParamsModel model)
        {

            try
            {
                var response = _cartPluginService.DeleteFromCart(model);
                var result = Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetShoppingCart
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetShoppingCart", Name = "GetShoppingCart")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetShoppingCart()
        {
            try
            {

                var response = _cartPluginService.GetShoppingCart();
                var result = Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Order
        /// <summary>
        /// PlaceOrder
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PlaceOrder", Name = "PlaceOrder")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult PlaceOrder(ParamsModel.PlaceOrderParamsModel model)
        {

            try
            {
                var (finishOrderResult, validationResult) = _orderPluginService.PlaceOrder(model);
                var result = base.Prepare(finishOrderResult, validationResult);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {

                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetCustomerOrders
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerOrders", Name = "GetCustomerOrders")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetCustomerOrders()
        {
            try
            {
                var customerOrders = _orderPluginService.GetCustomerOrders();
                 var result = base.Prepare(customerOrders); //prepare
                return Ok(result);
            }
            catch (Exception ex)
            {
                 _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetOrderstatuses
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("GetMatjeryConfig", Name = "GetMatjeryConfig")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetMatjeryConfig()
        {
            MatjeryApiconfigs Configs = new MatjeryApiconfigs();
            Configs.ResendOTPTimer = _commonsettings.ResendTokenTimer<=0?2: _commonsettings.ResendTokenTimer;
            var availableStatusItems = OrderStatus.Pending.ToSelectListInServices(false, useLocalization:true).ToList();

            foreach (var item in availableStatusItems)
            {
                OrderStatusModel model = new OrderStatusModel();
                model.Id = Convert.ToInt32(item.Value);
                model.StatusName = item.Text;
                Configs.OrderStatusModel.Add(model);
            }
            var result = base.Prepare(Configs);
            return Ok(result);

        }

        
        /// <summary>
        /// GetOrderDetails
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetOrderDetails", Name = "GetOrderDetails")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetOrderDetails(int orderId)
        {
            try
            {
                var customerOrder = _orderPluginService.GetOrderDetails(orderId);
                var result = base.Prepare(customerOrder);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// GetOrderDetails
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CancelOrder", Name = "CancelOrder")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult CancelOrder(int orderId)
        {
            try
            {
                var response = _orderPluginService.CancelOrder(orderId);
                var result = base.Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion

        #region Product Attributes
        /// <summary>
        /// GetProductAttributes
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductAttributes", Name = "GetProductAttributes")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetProductAttributes(int productId)
        {
            try
            {
                  var productAttributes= _productAttributePluginService.GetProductAttributes(productId);
                var result = base.Prepare(productAttributes);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });

            }
        }

        #endregion

        #region Wishlist
        /// <summary>
        /// GetWishlist
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetWishlist", Name = "GetWishlist")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetWishlist()
        {
            try
            {
               var list= _wishListPluginService.GetWishlist();
                var result = Prepare(list);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// AddToWhishlist
        /// </summary>
        /// <param name="paramsModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddToWhishlist", Name = "AddToWhishlist")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult AddToWhishlist(ParamsModel.WishlistParams paramsModel)
        {
            try
            {

                var warnnings= _wishListPluginService.AddToWhishlist(paramsModel);
                var apiValidationResult = new ApiValidationResultResponse();
                ApiResultResponse result;
                if (warnnings.Count > 0)
                {
                    foreach (string warnning in warnnings)
                    {
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription = warnning
                        });
                    }
                    result = Prepare(false, apiValidationResult);
                }
                else
                {
                    result = Prepare(true);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// DeleteFromWishlist
        /// </summary>
        /// <param name="paramsModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteFromWishlist", Name = "DeleteFromWishlist")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult DeleteFromWishlist(ParamsModel.WishlistParams paramsModel)
        {
            try
            {
               var response = _wishListPluginService.DeleteFromWishlist(paramsModel);

                 ApiResultResponse result = base.Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Constants
        /// <summary>
        /// GetConstants
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetConstants", Name = "GetConstants")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetConstants()
        {
            try
            {
                IPagedList<Constant> constants = _constantService.GetAllConstants();
                ApiResultResponse result = base.Prepare(constants.ToList());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region CheckOut
        /// <summary>
        /// GetStandardPaymentMethods
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetStandardPaymentMethods", Name = "GetStandardPaymentMethods")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetStandardPaymentMethods()
        {
            try
            {
               var standardPayments=  _checkOutPluginService.GetStandardPaymentMethods();
                 var result = base.Prepare(standardPayments);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }
        /// <summary>
        /// GetOrderSummary
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetOrderSummary", Name = "GetOrderSummary")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetOrderSummary()
        {
            try
            {
                var orderSummaryResult = _checkOutPluginService.GetOrderSummary();
                var result = base.Prepare(orderSummaryResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion
        #region Reviews

        [HttpPost]
        [Route("AddProductReview", Name = "Add Product Review")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult AddProductReview(ParamsModel.ProductReviewParamsModel model)
        {
            try
            {
                var productReviewsModelResult = _reviewPluginService.AddProductReview(model);
                if(productReviewsModelResult.Status == HttpStatusCode.BadRequest)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = productReviewsModelResult.Message });
                }
                else
                {
                    var result = base.Prepare(productReviewsModelResult);
                    return Ok(result);
                }

               
        
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddBulkProductReview", Name = "Add Bulk Product Review")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult AddBulkProductReview(List<ParamsModel.ProductReviewParamsModel> model)
        {
            try
            {
                var productReviewsModelResult = _reviewPluginService.AddBulkProductReview(model);
                if (productReviewsModelResult.Status == HttpStatusCode.BadRequest)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = "" });
                }
                else
                {
                    var result = base.Prepare(productReviewsModelResult);
                    return Ok(result);
                }



            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetProductReviews", Name = "Get Product Reviews")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetProductReviews(int productId)
        {
            try
            {

                var productReviewModel = _reviewPluginService.GetProductReviews(productId);
                var result = base.Prepare(productReviewModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetProductForReviews", Name = "Get Product For Reviews")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetProductForReviews()
        {
            try
            {

                var productForReviewModel = _reviewPluginService.GetProductForReviews();
                var result = base.Prepare(productForReviewModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("GetVendorRating", Name = "Get Vendor Rating")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetVendorRating(int vendorId)
        {
            try
            {

                var vendorRating = _reviewPluginService.GetVendorRating(vendorId);
                var result = base.Prepare(vendorRating);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #region Campaign
        /// <summary>
        /// GetAllCampaigns
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllCampaigns", Name = "GetAllCampaigns")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetAllCampaigns()
        {
            try
            {
                var campaignResultList =_campaignPluginService.GetAllCampaigns();
                var result = base.Prepare(campaignResultList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Push Notifcation
        /// <summary>
        /// UpdateRegisterationId
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateRegisterationId", Name = "UpdateRegisterationId")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult UpdateRegisterationId(ParamsModel.PushNotificationParamsModel model)
        {
            try
            {
                var response = _pushNotificationPluginService.UpdateRegisterationId(model);
                return Ok(Prepare(response));
            }
            catch (ArithmeticException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// MarkPushNotificationAsRead
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MarkPushNotificationAsRead", Name = "MarkPushNotificationAsRead")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult MarkPushNotificationAsRead(ParamsModel.QueuedPushNotificationParamsModel model)
        {
            try
            {
                var response = _pushNotificationPluginService.MarkPushNotificationAsRead(model);

                return Ok(Prepare(response));
            }
            catch (ArithmeticException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        /// <summary>
        /// MarkAllPushNotificationAsRead
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MarkAllPushNotificationAsRead", Name = "MarkAllPushNotificationAsRead")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult MarkAllPushNotificationAsRead(ParamsModel.QueuedPushNotificationParamsModel model)
        {
            try
            {

                var response = _pushNotificationPluginService.MarkAllPushNotificationAsRead(model);

                return Ok(Prepare(response));
            }
            catch (ArithmeticException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("DeleteDeviceInfo", Name = "DeleteDeviceInfo")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult DeleteDeviceInfo(ParamsModel.DeviceInfoParamsModel model)
        {
            try
            {
                var response = _pushNotificationPluginService.DeviceDelete(model);
                var result = base.Prepare(response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion


        #endregion
        #region Search
        [HttpGet]
        [Route("GetSearchProduct", Name = "Get Search Product")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetSearchProduct()
        {
            try
            {
                var model = _searchPluginService.GetSearchProduct();
                var result = Prepare(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("SearchProduct", Name = "Search Product")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult SearchProduct(ParamsModel.SearchParamsModel searchModel)
        {
            try
            {
                var model = _searchPluginService.SearchProduct(searchModel);
                if (model != null)
                {
                    var result = Prepare(model);
                    return Ok(result);
                }
                return StatusCode(StatusCodes.Status404NotFound, new { message = "No products to display" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion
        #region Vendor
        [HttpGet]
        [Route("GetVendorLicenseStatus", Name = "Get Vendor License Status")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetVendorLicenseStatus()
        {
            try
            {
                var vendorModel = _vendorPluginService.GetVendorLicenseStatus();
                var result = base.Prepare(vendorModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }
        [HttpGet]
        [Route("GetVendorById", Name = "Get Vendor ById")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetVendorById(int vendorId)
        {
            try
            {

                var vendorModel = _vendorPluginService.GetVendorById(vendorId);

                var result = base.Prepare(vendorModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            
        }
        [HttpGet]
        [Route("GetVendorSupportedVendors", Name = "Get Vendor Supported Vendors")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetVendorSupportedVendors(int vendorId)
        {
            try
            {

                var vendorModel = _vendorPluginService.GetVendorSupportedVendors(vendorId);
                //if(vendorModel.Count() == 0)
                //    return NotFound();

                var result = base.Prepare(vendorModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }
        [HttpPost]
        [Route("ApplyVendor", Name = "Apply Vendor")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult ApplyVendor([FromForm]VendorApplyParamsModel model)
        {
            try
            {
                if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                    return null;

                if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                    return BadRequest("Not allowed");

                if (!ModelState.IsValid)
                    return BadRequest();
                if (_workContext.CurrentCustomer.VendorId > 0)
                {
                    //already applied for vendor account
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = _localizationService.GetResource("Vendors.ApplyAccount.AlreadyApplied") });
                }
                var httpPostedFile = Request.Form.Files.FirstOrDefault();
                var response = _vendorPluginService.ApplyVendor(model,httpPostedFile);
                var result = base.Prepare(response.Item1, response.Item2);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }


        }
        [HttpPost]
        [Route("ApplyFoundation", Name = "Apply Foundation")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult ApplyFoundation(VendorApplyParamsModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                var response = _vendorPluginService.ApplyFoundation(model);
                var result = base.Prepare(response.Item1, response.Item2);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("SaveVendorInfo", Name = "Save VendorInfo")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult SaveVendorInfo([FromForm] ParamsModel.VendorInfoParamsModel model, [FromForm] IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                //we don't allow viewing of vendors if "vendors" block is hidden
                Vendor vendor = _vendorService.GetVendorById(model.Id);
                if (vendor == null || vendor.Deleted || !vendor.Active)
                    return NotFound();

                //Vendor is active?
                if (!vendor.Active)
                    return NotFound();

               // var httpPostedFile = Request.Form.Files.FirstOrDefault();
                var response = _vendorPluginService.SaveVendorInfo(model, file);
                var result = base.Prepare(response.Item1, response.Item2);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("ContactVendor", Name = "Contact Vendor")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult ContactVendor(ParamsModel.ContactVendorParamsModel model)
        {
            try
            {
                var vendor = _vendorService.GetVendorById(model.VendorId);
                if (vendor == null || !vendor.Active || vendor.Deleted)
                    return NotFound();

                var vendorModel = _vendorPluginService.ContactVendor(model);

                var result = base.Prepare(vendorModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }



        [HttpPost]
        [Route("VendorReviewAdd", Name = "Vendor Review Add")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult VendorReviewAdd(ParamsModel.VendorReviewAddParamsModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest();

                var (vendorReviewResult, apiValidationResult) = _vendorPluginService.VendorReviewAdd(model);

                var result = base.Prepare(vendorReviewResult, apiValidationResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        [HttpPost]
        [Route("VendorFollowAndUnfollow", Name = "Vendor Follow And Unfollow")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult VendorFollowAndUnfollow(ParamsModel.VendorFollowAddParamsModel model)
        {
            try
            {
                var vendor = _vendorService.GetVendorById(model.VendorId);
                if (vendor == null || !vendor.Active || vendor.Deleted)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return BadRequest();

                var apiValidationResult = new ApiValidationResult();
                if (_customerService.IsGuest(_workContext.CurrentCustomer))
                    return BadRequest();
                var vendorModel = _vendorPluginService.VendorFollowAndUnfollow(model);

                var result = base.Prepare(vendorModel.Item1, vendorModel.Item2);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("GetAllVendorReviews", Name = "Get All Vendor Reviews")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetAllVendorReviews([FromQuery] int vendorId)
        {
            try
            {

                var vendorModel = _vendorPluginService.GetAllVendorReviews(vendorId);
                var result = base.Prepare(vendorModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("VendorSupportedByAdd", Name = "Vendor Supported By Add")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult VendorSupportedByAdd(ParamsModel.SupportedByVendorAddParamsModel model)
        {
            try
            {
              

                if (!ModelState.IsValid)
                    return BadRequest();

                var apiValidationResult = new ApiValidationResult();
                if (_customerService.IsGuest(_workContext.CurrentCustomer))
                    return BadRequest();
                var status = _vendorPluginService.VendorSupportedByAdd(model);
                if(status == false)
                    return BadRequest();

                var result = base.Prepare(status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        [HttpGet]
        [Route("FillVendors", Name = "FillVendors")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult FillVendors()
        {
            try
            {
                List<SellerParamsModel> sellers = new List<SellerParamsModel>();
                var vendors = _vendorService.GetAllVendors(showHidden: false,customer:_workContext.CurrentCustomer).ToList();
                foreach(var vendor in vendors)
                {
                    var seller = new SellerParamsModel();
                    seller.Id = vendor.Id;
                    seller.Name= _localizationService.GetLocalized(vendor, x => x.Name, _workContext.WorkingLanguage.Id);
                    sellers.Add(seller);
                }
                var result = base.Prepare(sellers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("GetAllVendors", Name = "GetAllVendors")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetAllVendors([FromQuery] int pageIndex, int pageSize, bool displayEntities)
        {
            try
            {
                var vendorResult = _vendorPluginService.GetAllVendors(pageIndex-1, pageSize,  displayEntities);
                var result = base.Prepare(vendorResult);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        #endregion

        #region Followers
        [HttpGet]
        [Route("GetAllFollowedVendors", Name = "Get All Followed Vendors")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetAllFollowedVendors()
        {
            try
            {
                if (_customerService.IsGuest(_workContext.CurrentCustomer))
                    return BadRequest("Not allowed");

                var followers = _followersPluginService.GetAllFollowedVendors();
                var result = base.Prepare(followers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion

        #region TradeLicense
        [HttpGet]
        [Route("GetTradeLicenseDetail", Name = "Get Trade License Detail")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public async Task<IActionResult> GetTradeLicenseDetail([FromQuery] string LicenseNumber)
        {
            try
            {
                var followers =await _tradeLicensePluginService.GetTradeLicenseResponse(LicenseNumber);
                var result = base.Prepare(followers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion
        #endregion

        #region Topics
        [HttpGet]
        [Route("GetAllTopics", Name = "Get All Topics")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetAllTopics()
        {
            try
            {

                var topics = _topicsPluginService.GetAllTopics();
                var result = base.Prepare(topics);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("GetTopicDetail", Name = "Get Topic Detail")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetTopicDetail(string systemName)
        {
            try
            {

                var topics = _topicsPluginService.GetTopicDetail(systemName);
                var result = base.Prepare(topics);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion
        #region Lookup
        /// <summary>
        /// GetMatjeryLookups
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [Route("GetMatjeryLookups", Name = "GetMatjeryLookups")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetMatjeryLookups([FromBody] TypesModel model)
        {
           
            List<Dictionary<string, object>> MatjeryLookupModelgroup = new List<Dictionary<string,object>>();
            ApiResultResponse result = new ApiResultResponse();

            if (model.types != null)
            {
                string[] types = model.types.ToArray();
                var resposne = _lookupPluginService
                    .GetAllLookups(types)
                    .Select(x => new MatjeryLookupModel
                    {
                        Id = x.Id,
                        Name = x.Value,
                        Type = x.Type,
                        Sequence = x.Sequence
                    }).GroupBy(g => g.Type).ToList();
                  foreach(var lookgroup in resposne)
                {
                    var dic = new Dictionary<string, object>();
                    dic.Add(lookgroup.Key, lookgroup);
                    MatjeryLookupModelgroup.Add(dic);
                }
                result = base.Prepare(MatjeryLookupModelgroup);
                return Ok(result);
            }

            result = base.Prepare(MatjeryLookupModelgroup);
            return Ok(result);

        }
        #endregion
        #region Localization

        [HttpGet]
        [Route("GetAllResourceValues", Name = "Get All Resource Values")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]

        public IActionResult GetAllResourceValues()
        {
            try
            {
                var ignoredKeys = new List<string>
                {
                    "Admin.",
                    "ActivityLog.",
                    "Blog.",
                    "Forum.",
                    "Plugins."
                };
                var langaugeListModel = new List<LocalizationModelResult>();
                foreach (Language language in _languageService.GetAllLanguages())
                {
                    var values = _localizationService.GetAllResourceValues(language.Id, loadPublicLocales:null)
                        .Where(x => ignoredKeys.All(e => !x.Key.StartsWith(e, StringComparison.InvariantCultureIgnoreCase)))
                        .Select(x => new
                        {
                            Key = x.Key,
                            Value = CommonHelper.StripHTML(x.Value.Value)
                        })
                        .OrderBy(x => x.Key)
                        .ToList();

                    var model = new LocalizationModelResult()
                    {
                        Language = language.UniqueSeoCode,
                        Values = values
                    };
                    langaugeListModel.Add(model);
                }
                var result = base.Prepare(langaugeListModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        #endregion
    }
}
