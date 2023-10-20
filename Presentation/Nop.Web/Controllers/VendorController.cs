using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.TradeLicense;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Vendors;

namespace Nop.Web.Controllers
{
    public partial class VendorController : BasePublicController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ICategoryService _categoryService;
        private readonly ITradelicenseService _tradelicenseService;
        private readonly CommonSettings _commonSettings;
        private readonly INopFileProvider _fileProvider;

        #endregion

        #region Ctor

        public VendorController(CaptchaSettings captchaSettings,
            ICustomerService customerService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IVendorModelFactory vendorModelFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            VendorSettings vendorSettings,
            ICategoryService categoryService,
            ITradelicenseService tradelicenseService,
            INopFileProvider fileProvider,
            CommonSettings commonSettings)
        {
            _fileProvider = fileProvider;
            _captchaSettings = captchaSettings;
            _customerService = customerService;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _vendorModelFactory = vendorModelFactory;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _vendorSettings = vendorSettings;
            _categoryService = categoryService;
            _tradelicenseService = tradelicenseService;
            _commonSettings = commonSettings;
        }

        #endregion

        #region Utilities

        protected virtual void UpdatePictureSeoNames(Vendor vendor)
        {
            var picture = _pictureService.GetPictureById(vendor.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(vendor.Name));
        }

        protected virtual string ParseVendorAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = _vendorAttributeService.GetAllVendorAttributes();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopVendorDefaults.VendorAttributePrefix}{attribute.Id}";
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
                                    attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                )
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _vendorAttributeService.GetVendorAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
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
                                attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported vendor attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        #endregion

        #region Methods

        //[HttpsRequirement]..
        public virtual IActionResult ApplyVendor(string type)
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return RedirectToRoute("Homepage");

            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var model = new ApplyVendorModel();
            model = _vendorModelFactory.PrepareApplyVendorModel(model, true, false, null);
            if (!string.IsNullOrEmpty(type))
            {
                if (type.ToLower() == "uaepass")
                {
                    model.IsUaePass = true;
                    var uaePassAttr = TempData["uaePassAttr"] as UaePassUserProfileAttributes;
                    TempData["uaePassAttr"] = uaePassAttr;
                    var accessToken = TempData["uaePassaccessToken"] as UaePassAccessToken;
                    TempData["uaePassaccessToken"] = accessToken;
                    if (uaePassAttr != null)
                    {
                        if (!string.IsNullOrEmpty(uaePassAttr.idn) ||
                             !string.IsNullOrEmpty(uaePassAttr.idnNotVerified))
                        {
                            model.EmiratesId = uaePassAttr.idn ?? uaePassAttr.idnNotVerified;
                        }
                    }

                }
            }
            var customer = _workContext.CurrentCustomer;
            model.PhoneNumber = _genericAttributeService.GetAttribute<Customer, string>(customer.Id, NopCustomerDefaults.PhoneAttribute);


            return View(model);
        }

        [HttpPost, ActionName("ApplyVendor")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual IActionResult ApplyVendorSubmit(ApplyVendorModel model, bool captchaValid, IFormFile uploadedFile, IFormFile tradeLicenseFile, IFormCollection form)
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return RedirectToRoute("Homepage");

            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (_customerService.IsAdmin(_workContext.CurrentCustomer))
                ModelState.AddModelError("", _localizationService.GetResource("Vendors.ApplyAccount.IsAdmin"));

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnApplyVendorPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

                if (model.CategoryId != null)
                {
                    List<int> filterTags = new List<int>() { 65, 66, 67 };
                    bool containsValues = model.CategoryId.Any(x => filterTags.Contains(x));
                    if (containsValues && tradeLicenseFile== null)

                    {
                        ModelState.AddModelError("", _localizationService.GetResource("account.register.errors.license"));
                    }
                    if (containsValues && string.IsNullOrEmpty(model.TradeLicenseNumber))
                    {
                      ModelState.AddModelError("", _localizationService.GetResource("account.register.errors.tradelicensenumber"));
                    }
                }
            

            int pictureId = 0,licenseId =0;

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    var contentType = uploadedFile.ContentType;
                    var vendorPictureBinary = _downloadService.GetDownloadBits(uploadedFile);
                    var picture = _pictureService.InsertPicture(vendorPictureBinary, contentType, null);

                    if (picture != null)
                        pictureId = picture.Id;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Vendors.ApplyAccount.Picture.ErrorMessage"));
                }
            }


            if (tradeLicenseFile != null && !string.IsNullOrEmpty(tradeLicenseFile.FileName))
            {
                var contentType = tradeLicenseFile.ContentType;
                var vendorLicenseBinary = _downloadService.GetDownloadBits(tradeLicenseFile);
                var file = tradeLicenseFile;
                var download = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    UseDownloadUrl = false,
                    DownloadUrl = string.Empty,
                    DownloadBinary = vendorLicenseBinary,
                    ContentType = tradeLicenseFile.ContentType,
                    Filename = _fileProvider.GetFileNameWithoutExtension(tradeLicenseFile.FileName),
                    Extension = _fileProvider.GetFileExtension(tradeLicenseFile.FileName),
                    IsNew = true
                };
                _downloadService.InsertDownload(download);
                licenseId = download.Id > 0 ? download.Id : 0;

            }



            //vendor attributes
            var vendorAttributesXml = ParseVendorAttributes(form);
            _vendorAttributeParser.GetAttributeWarnings(vendorAttributesXml).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            if (ModelState.IsValid)
            {
                var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false, false, false);
                //disabled by default
                var vendor = new Vendor
                {
                    Name = model.Name,
                    Email = model.Email,
                    WhatsApp = model.WhatsApp,
                    PhoneNumber = model.PhoneNumber,
                    Googleplus = model.Googleplus,
                    Facebook = model.Facebook,
                    Twitter = model.Twitter,
                    Instagram = model.Instagram,
                    EmiratesId = model.EmiratesId,
                    //some default settings
                    //some default settings
                    PageSize = 6,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                    PictureId = pictureId,
                    LicenseCopyId = licenseId,
                    LicenseNo = model.TradeLicenseNumber,
                    Description = description,
                    CreatedBy = _workContext.CurrentCustomer.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                };
                if (Request.Cookies["RequestType"] != null)
                {
                    if (Request.Cookies["flage"] == "merchant")
                    {
                        vendor.RegisterationType = VendorRegisterationType.Merchant;
                        vendor.FoundationAprovalStatusId = (int)FoundationAprovalStatus.Pending;
                    }
                    else if (Request.Cookies["flage"] == "foundation")
                    {
                        vendor.RegisterationType = VendorRegisterationType.Foundation;
                        vendor.FoundationTypeId = (int)FoundationTypeStatus.CannotAproveOrRejectRequest;
                    }
                }
                vendor.LicenseNo = model.TradeLicenseNumber;

                

                
                _vendorService.InsertVendor(vendor);
                //HttpCookie aCookie = new HttpCookie("RequestType");
                string key = "RequestType";
                string value = "";
                CookieOptions aCookie = new CookieOptions();
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append(key, value, aCookie);
                Response.Cookies.Append("flage", "", aCookie);
                Response.Cookies.Append("email", "", aCookie);
                Response.Cookies.Delete("Flage", aCookie);
                // Request.Cookies["Flage"] = "";
                //search engine name (the same as vendor name)
                var seName = _urlRecordService.ValidateSeName(vendor, vendor.Name, vendor.Name, true);
                _urlRecordService.SaveSlug(vendor, seName, 0);

                //associate to the current customer
                //but a store owner will have to manually add this customer role to "Vendors" role
                //if he wants to grant access to admin area
                var CustomerRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.VendorsRoleName);
                _workContext.CurrentCustomer.VendorId = vendor.Id;
                _workContext.CurrentCustomer.CustomerRoles.Add(CustomerRole);
                _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = _workContext.CurrentCustomer.Id, CustomerRoleId = CustomerRole.Id });

                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                //update picture seo file name
                UpdatePictureSeoNames(vendor);

                //save vendor attributes
                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeIssueDate, model.IssueDate);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeExpiryDate, model.ExpiryDate);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeCategory, string.Join(",", model.CategoryId));


                //notify store owner here (email)
                _workflowMessageService.SendNewVendorAccountApplyStoreOwnerNotification(_workContext.CurrentCustomer,
                    vendor, _localizationSettings.DefaultAdminLanguageId);

                model.DisableFormInput = true;
                model.Result = _localizationService.GetResource("Vendors.ApplyAccount.Submitted");
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = _vendorModelFactory.PrepareApplyVendorModel(model, false, true, vendorAttributesXml);
            return View(model);
        }
        public virtual IActionResult DownloadFile(Guid downloadGuid)
        {
            var download = _downloadService.GetDownloadByGuid(downloadGuid);
            if (download == null)
                return Content("No download record found with the specified id");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //use stored data
            if (download.DownloadBinary == null)
                return Content($"Download data is not available any more. Download GD={download.Id}");

            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType)
            {
                FileDownloadName = fileName + download.Extension
            };
        }

        [HttpsRequirement]
        public virtual IActionResult LicenseInfo()
            {
            //VendorLicenseStatus
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (_customerService.IsVendor(_workContext.CurrentCustomer) == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            var model = new VendorInfoModel();
            var vendor = _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId);
            model = _vendorModelFactory.PrepareVendorInfoModel(model, false);
            GetAllVendorsWhoCanAproveRequest(model, vendor);

            var VendorCategory = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeCategory);
            if (VendorCategory != null)
            {
                IList<string> _categories = VendorCategory.Split(',');
                List<int> Vcategories = new List<int>();
                foreach (var c in _categories)
                {
                    Vcategories.Add(Convert.ToInt32(c));
                }
                model.CategoryId = Vcategories;
            }
            return View(model);
        }

        [HttpsRequirement]
        public virtual IActionResult Info()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (_customerService.IsVendor(_workContext.CurrentCustomer) == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            var model = new VendorInfoModel();
            var vendor = _workContext.CurrentVendor;
            model = _vendorModelFactory.PrepareVendorInfoModel(model, false);
            GetAllVendorsWhoCanAproveRequest(model, vendor);
           
            return View(model);
        }
        [HttpPost, ActionName("LicenseInfo")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("save-LicenseInfo-button")]
        public virtual IActionResult LicenseInfo(VendorInfoModel model, IFormFile uploadedFile, IFormFile tradeLicenseFile, IFormCollection form)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            var vendor = _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId);
            if (vendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            if (tradeLicenseFile != null && tradeLicenseFile.Length != 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".doc" };
                var fileExtension = Path.GetExtension(tradeLicenseFile.FileName);

                if (!allowedExtensions.Contains(fileExtension.ToLower()))
                {
                    ModelState.AddModelError("file", "Invalid file type.");
                }

                var maxSize = 5048576; // 1MB

                if (tradeLicenseFile.Length > maxSize)
                {
                    ModelState.AddModelError("file", "File size must be less than 5MB.");
                }
            }
            if (TempData["NewLicenseAdded"] != null)
            {
                var Added = TempData["NewLicenseAdded"];
                if (TempData["NewLicenseAdded"].ToString().ToLower() == "yes" && tradeLicenseFile == null)
                {
                    ModelState.AddModelError("TradeLicenseFile", "Upload New License copy");
                }
                else
                {
                    TempData["NewLicenseAdded"] = "";
                }
            }
          

            if (tradeLicenseFile != null && !string.IsNullOrEmpty(tradeLicenseFile.FileName))
            {
                var contentType = tradeLicenseFile.ContentType;
                var vendorLicenseBinary = _downloadService.GetDownloadBits(tradeLicenseFile);
            }
            //var vendor = _workContext.CurrentVendor;
            ModelState.Remove("Name");
            ModelState.Remove("Email");

            if (model.CategoryId != null)
            {
                List<int> filterTags = new List<int>() { 65, 66, 67 };
                bool containsValues = model.CategoryId.Any(x => filterTags.Contains(x));
                if (containsValues && tradeLicenseFile == null && string.IsNullOrEmpty(model.TradeLicenseFileurl))

                {
                    ModelState.AddModelError("", _localizationService.GetResource("account.register.errors.license"));
                }
                if (containsValues && string.IsNullOrEmpty(model.TradeLicenseNumber))
                {
                    ModelState.AddModelError("", _localizationService.GetResource("account.register.errors.tradelicensenumber"));
                }
            }

            if (ModelState.IsValid)
            {
                    int TradeLicenseFileId = 0;
                    if (tradeLicenseFile != null)
                    {
                        var fileBinary = _downloadService.GetDownloadBits(tradeLicenseFile);
                        var file = tradeLicenseFile;
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
                        TradeLicenseFileId = download.Id;
                    }
                   
                vendor.LicenseNo = model.TradeLicenseNumber;
                if (tradeLicenseFile != null)
                {
                    vendor.LicenseCopyId = TradeLicenseFileId;
                }
                 vendor.Active = true;
                _vendorService.UpdateVendor(vendor);
                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeIssueDate, model.IssueDate);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeExpiryDate, model.ExpiryDate);

                if (model.CategoryId != null)
                    _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeCategory, string.Join(",", model.CategoryId));


                //notifications
                if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                    _workflowMessageService.SendVendorInformationChangeNotification(vendor, _localizationSettings.DefaultAdminLanguageId);

                return RedirectToAction("LicenseInfo");
            }

            //If we got this far, something failed, redisplay form
            model = _vendorModelFactory.PrepareVendorInfoModel(model, true);
            GetAllVendorsWhoCanAproveRequest(model, vendor);
            if (model.CategoryId != null)
            {
                foreach (var selected in model.CategoryId)
                {
                    model.Categories.FirstOrDefault(x => x.Value == selected.ToString()).Selected = true;
                }
            }
            return View(model);
        }


        [HttpPost, ActionName("Info")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("save-info-button")]
        public virtual IActionResult Info(VendorInfoModel model, IFormFile uploadedFile, IFormFile tradeLicenseFile, IFormCollection form)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (_workContext.CurrentVendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            Picture picture = null,license=null;

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    var contentType = uploadedFile.ContentType;
                    var vendorPictureBinary = _downloadService.GetDownloadBits(uploadedFile);
                    picture = _pictureService.InsertPicture(vendorPictureBinary, contentType, null);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Account.VendorInfo.Picture.ErrorMessage"));
                }
            }

        
            
            var vendor = _workContext.CurrentVendor;
            var prevPicture = _pictureService.GetPictureById(vendor.PictureId);

            //vendor attributes
            var vendorAttributesXml = ParseVendorAttributes(form);
            _vendorAttributeParser.GetAttributeWarnings(vendorAttributesXml).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            if (ModelState.IsValid)
            {
                var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false, false, false);

                vendor.Name = model.Name;
                vendor.Email = model.Email;
                vendor.Description = description;

                if (picture != null)
                {
                    vendor.PictureId = picture.Id;

                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }

                //update picture seo file name
                UpdatePictureSeoNames(vendor);

                _vendorService.UpdateVendor(vendor);

                //notifications
                if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                    _workflowMessageService.SendVendorInformationChangeNotification(vendor, _localizationSettings.DefaultAdminLanguageId);

                return RedirectToAction("Info");
            }

            //If we got this far, something failed, redisplay form
            model = _vendorModelFactory.PrepareVendorInfoModel(model, true, vendorAttributesXml);
            return View(model);
        }

        [HttpPost, ActionName("Info")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("remove-picture")]
        public virtual IActionResult RemovePicture()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Challenge();

            if (_workContext.CurrentVendor == null || !_vendorSettings.AllowVendorsToEditInfo)
                return RedirectToRoute("CustomerInfo");

            var vendor = _workContext.CurrentVendor;
            var picture = _pictureService.GetPictureById(vendor.PictureId);

            if (picture != null)
                _pictureService.DeletePicture(picture);

            vendor.PictureId = 0;
            _vendorService.UpdateVendor(vendor);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                _workflowMessageService.SendVendorInformationChangeNotification(vendor, _localizationSettings.DefaultAdminLanguageId);

            return RedirectToAction("Info");
        }
        protected void GetAllVendorsWhoCanAproveRequest(VendorInfoModel model, Vendor vendor)
        {
            if (vendor != null)
            {
                var supportedVendors = _vendorService.GetAllVendors(VendorRegisterationType.Foundation, showNotActive: true)
               .ToList()
               .Where(f => f.FoundationTypeId != null && f.FoundationTypeId == (int)FoundationTypeStatus.CanAproveOrRejectRequest);

                model.AvailableSupportedByFoundations.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource("Common.Select"),
                    Value = "0"
                });
                foreach (Vendor sVendor in supportedVendors)
                {
                    model.AvailableSupportedByFoundations.Add(new SelectListItem
                    {
                        Text = _localizationService.GetLocalized(sVendor, v => v.Name),
                        Value = sVendor.Id.ToString(),
                        Selected = sVendor.Id == vendor.SupportedByFoundationId
                    });
                }

                var category = _categoryService.GetAllCategories().Where(c => c.IncludeInTopMenu).ToList<Category>();

                var categories = category.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });

                foreach (var c in _categoryService.GetAllCategories().Where(c => c.IncludeInTopMenu).ToList<Category>())
                {
                    model.Categories.Add(new SelectListItem
                    {
                        Text = _localizationService.GetLocalized(c, x => x.Name),
                        Value = c.Id.ToString()
                    });
                }
            }
           
        }

        [HttpGet, ActionName("GetTradeLicense")]
        public async virtual Task<IActionResult> GetTradeLicense(string licenseNumber)
        {
            var result = await _tradelicenseService.GetTradeLicenseResponse(licenseNumber);

            if (result.GetLicenseDetailsResponse.Status == "Failed")
                return BadRequest(result.GetLicenseDetailsResponse.ErrorMessageEn);

            return Json(result);
        }
        [HttpGet, ActionName("RenewTradeLicense")]
        public async virtual Task<IActionResult> RenewTradeLicense(string licenseNumber,string ExpiryDate)
        {
            var result =await  _tradelicenseService.GetTradeLicenseResponse(licenseNumber);

            if(result.GetLicenseDetailsResponse.ExpiryDate!= ExpiryDate.Trim())
            {
                TempData["NewLicenseAdded"] = "yes";
            }
            else
            {
                TempData["NewLicenseAdded"] = "no";
            }

            if (result.GetLicenseDetailsResponse.Status == "Failed")
                return BadRequest(result.GetLicenseDetailsResponse.ErrorMessageEn);

            return Json(result);
        }

        #endregion
    }
}