using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.BlackPoints;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.Authentication.External;
using Nop.Services.BlackPoints;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class VendorController : BaseAdminController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IVendorService _vendorService;
        private readonly IImportManager _importManager;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IDownloadService _downloadService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IBlackPointService _blackPointService;
        private readonly IWorkContext _workContext;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public VendorController(IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IVendorModelFactory vendorModelFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            IImportManager importManager,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IDownloadService downloadService,
            IDateTimeHelper dateTimeHelper,
            IBlackPointService blackPointService,
            IOpenAuthenticationService openAuthenticationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IOrderService orderService
            )
        {
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _vendorModelFactory = vendorModelFactory;
            _vendorService = vendorService;
            _workContext = workContext;
            _importManager = importManager;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _downloadService = downloadService;
            _dateTimeHelper = dateTimeHelper;
            _blackPointService = blackPointService;
            _openAuthenticationService = openAuthenticationService;
            _orderService = orderService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareAllVendorModel(VendorModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var vendors = _vendorService.GetAllVendors(showHidden: true)
                .Where(x => x.RegisterationTypeId == (int)VendorRegisterationType.Foundation
                    && x.FoundationTypeId == (int)FoundationTypeStatus.CanAproveOrRejectRequest);
            model.AvailableVendor.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("admin.common.select"),
                Value = ""
            });
            foreach (var vendor in vendors)
            {
                model.AvailableVendor.Add(new SelectListItem
                {
                    Text = vendor.Name,
                    Value = vendor.Id.ToString()
                });
            }
            // Foundaton Type
            model.FoundationType.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("admin.common.select"),
                Value = ""
            });
            foreach (var foundationtype in FoundationTypeStatus.CanAproveOrRejectRequest.ToSelectListInServices(false))
            {
                model.FoundationType.Add(new SelectListItem
                {
                    Text = foundationtype.Text,
                    Value = foundationtype.Value,
                    Selected = foundationtype.Value == model.FoundationTypeId.ToString()
                });
            }
            // Foundaton Aproval
            model.AvailibleAprovalStatus.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("admin.common.select"),
                Value = "",
            });
            foreach (var foundationaoroval in FoundationAprovalStatus.Aproved.ToSelectListInServices(false))
            {
                model.AvailibleAprovalStatus.Add(new SelectListItem
                {
                    Text = foundationaoroval.Text,
                    Value = foundationaoroval.Value,
                    Selected = foundationaoroval.Value == model.FoundationAprovalStatusId.ToString()
                });
            }
            _baseAdminModelFactory.PrepareCategories(model.AvailableCategories, languageid: _workContext.WorkingLanguage.Id);
        }

        protected virtual void UpdatePictureSeoNames(Vendor vendor)
        {
            var picture = _pictureService.GetPictureById(vendor.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(vendor.Name));
        }

        protected virtual void UpdateLocales(Vendor vendor, VendorModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(vendor,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                                                         x => x.LicensedBy,
                                                         localized.LicensedBy,
                                                         localized.LanguageId);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(vendor, localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(vendor, seName, localized.LanguageId);
            }
        }

        protected virtual string ParseVendorAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var vendorAttributes = _vendorAttributeService.GetAllVendorAttributes();
            foreach (var attribute in vendorAttributes)
            {
                var controlId = $"{NopVendorDefaults.VendorAttributePrefix}{attribute.Id}";
                StringValues ctrlAttributes;
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
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

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                attribute, enteredText);
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

        #region Vendors

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var model = _vendorModelFactory.PrepareVendorSearchModel(new VendorSearchModel());

            //prepare model
            //var model0 = _vendorModelFactory.PrepareVendorListModel(searchModel);

            return View(model);
        }

        [HttpPost]
        public IActionResult List(VendorSearchModel searchmodel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

         
            var model = _vendorModelFactory.PrepareVendorListModel(searchmodel);
            //if (sort != null && sort.Count() > 0)
            //{
            //    var st = sort.Single();
            //    if (st.Dir == "asc")
            //    {

            //        switch (st.Field)
            //        {
            //            case "Name": orderBy = VendorSortingEnum.NameAsc; break;
            //            case "CreatedOnUtc": orderBy = VendorSortingEnum.CreatedOnAsc; break;
            //        }

            //    }
            //    else if (st.Dir == "desc")
            //    {
            //        switch (st.Field)
            //        {
            //            case "Name": orderBy = VendorSortingEnum.NameDesc; break;
            //            case "CreatedOnUtc": orderBy = VendorSortingEnum.CreatedOnDesc; break;
            //        }
            //    }
            //}

          
          
            //var gridModel = new 
            //{
            //    Data = vendors.Select(x =>
            //    {
            //        var vendorModel = x.ToModel<VendorModel>();
            //        if (vendorModel.FoundationAprovalStatusId != null)
            //            vendorModel.AprovalStatus =_localizationService.GetLocalizedEnum( x.FoundationAprovalStatus);

            //        vendorModel.Name = _localizationService.GetLocalized(x,c => c.Name, _workContext.WorkingLanguage.Id);

            //        var total = _blackPointService.SearchBlackPoints(vendorOrCustomerId: vendorModel.Id
            //            , blackPointType: (int)BlackPointTypeEnum.Vendor, pageSize: 1, blackPointStatus: (int)BlackPointStatusEnum.Approved).TotalCount;
            //        vendorModel.TotalBlackPoints = total;

            //        return vendorModel;
            //    }),
            //    Total = vendors.TotalCount,
            //};

            return Json(model);
        }

        [HttpPost]
        public IActionResult DeletedList(VendorSearchModel model)
        {
            if (!_customerService.IsAdmin(_workContext.CurrentCustomer))
                return AccessDeniedView();

            var orderBy = VendorSortingEnum.CreatedOnDesc;
            //if (sort != null && sort.Count() > 0)
            //{
            //    var st = sort.Single();
            //    if (st.Dir == "asc")
            //    {

            //        switch (st.Field)
            //        {
            //            case "Email": orderBy = VendorSortingEnum.EmailAsc; break;
            //            case "CreatedOnUtc": orderBy = VendorSortingEnum.CreatedOnAsc; break;
            //        }

            //    }
            //    else if (st.Dir == "desc")
            //    {
            //        switch (st.Field)
            //        {
            //            case "Email": orderBy = VendorSortingEnum.EmailDesc; break;
            //            case "CreatedOnUtc": orderBy = VendorSortingEnum.CreatedOnDesc; break;
            //        }
            //    }
            //}

            List<Vendor> vendorslist = new List<Vendor>();

           var vendors = _vendorService.GetAllVendors(model.SearchName, "", pageIndex: model.Page - 1, pageSize: model.PageSize,
                showNotActive: true, languageid: _workContext.WorkingLanguage.Id, OrderBy: orderBy, getDeleted: true);

            foreach(var vendor in vendors)
            {
                if (vendor.Email.Contains("deleted_"))
                {
                    var deletedUserRegisterExist = _vendorService.GetVendorByEmail(CommonHelper.CustomerEmailFormat(vendor.Email));
                    if (deletedUserRegisterExist == null)
                        vendorslist.Add(vendor);
                }
                else
                {
                    vendorslist.Add(vendor);
                }
            }
            vendors = new PagedList<Vendor>(vendorslist, model.Page - 1, model.PageSize);

            var gridModel = new VendorListModel().PrepareToGrid(model, vendors, () =>
            {
                //fill in model values from the entity
                return vendors.Select(x =>
                {


                    var vendorModel = x.ToModel<VendorModel>();
                    if (vendorModel.FoundationAprovalStatusId != null)
                    {
                        vendorModel.AprovalStatus = _localizationService.GetLocalizedEnum(x.FoundationAprovalStatus);
                    }
                    vendorModel.Name = _localizationService.GetLocalized(x, c => c.Name, _workContext.WorkingLanguage.Id);
                    vendorModel.CreatedOnStr = x.CreatedOnUtc.ToString();
                    vendorModel.Email = x.Email;
                    vendorModel.Active = x.Active;
                    return vendorModel;
                });
           
            });
            //var gridModel = new
            //{

            //    Data = vendors.Select(x =>
            //    {
            //        var vendorModel = x.ToModel<VendorModel>();
            //        if (vendorModel.FoundationAprovalStatusId != null)
            //        {
            //            vendorModel.AprovalStatus = _localizationService.GetLocalizedEnum(x.FoundationAprovalStatus);
            //        }
            //        vendorModel.Name = _localizationService.GetLocalized(x, c => c.Name, _workContext.WorkingLanguage.Id);
            //        vendorModel.CreatedOnStr = x.CreatedOnUtc.ToString();
            //        vendorModel.Email = x.Email;
            //        return vendorModel;
            //    }),
            //    Total = vendors.TotalCount,
            //};

            return Json(gridModel);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //prepare model
            var model = _vendorModelFactory.PrepareVendorModel(new VendorModel(), null);
            PrepareAllVendorModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Create(VendorModel model, bool continueEditing, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //parse vendor attributes
            var vendorAttributesXml = ParseVendorAttributes(form);
            _vendorAttributeParser.GetAttributeWarnings(vendorAttributesXml).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            if (ModelState.IsValid)
            {
                var vendor = model.ToEntity<Vendor>();

                if (vendor.FoundationAprovalStatusId != null && vendor.FoundationAprovalStatusId == (int)FoundationAprovalStatus.Aproved)
                {
                    vendor.FoundationAprovalByCustomerId = _workContext.CurrentCustomer.Id;
                }

                vendor.CreatedOnUtc = DateTime.UtcNow;
                vendor.UpdatedOnUtc = DateTime.UtcNow;
                vendor.CreatedBy = _workContext.CurrentCustomer.Id;

                _vendorService.InsertVendor(vendor);

                //activity log
                _customerActivityService.InsertActivity("AddNewVendor",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewVendor"), vendor.Id), vendor);

                //search engine name
                model.SeName = _urlRecordService.ValidateSeName(vendor, model.SeName, vendor.Name, true);
                _urlRecordService.SaveSlug(vendor, model.SeName, 0);

                //address
                var address = model.Address.ToEntity<Address>();
                address.CreatedOnUtc = DateTime.UtcNow;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                _addressService.InsertAddress(address);
                vendor.AddressId = address.Id;
                _vendorService.UpdateVendor(vendor);

                //vendor attributes
                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

                //locales
                UpdateLocales(vendor, model);

                //update picture seo file name
                UpdatePictureSeoNames(vendor);

                var notificationmessag = $"{_localizationService.GetLocalized(vendor, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Vendors.Added")}";
                _notificationService.SuccessNotification(notificationmessag);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = vendor.Id });
            }

            //prepare model
            model = _vendorModelFactory.PrepareVendorModel(model, null, true);
            PrepareAllVendorModel(model);
            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = _vendorService.GetVendorById(id);
            //if (vendor == null || vendor.Deleted)
            //    return RedirectToAction("List");

            //prepare model
            var model = _vendorModelFactory.PrepareVendorModel(null, vendor);

            if (model.FoundationAprovalByCustomerId != null && model.FoundationAprovalByCustomerId > 0)
                model.AprovalByCustomer = _customerService.GetCustomerById(Convert.ToInt32(model.FoundationAprovalByCustomerId)).Email;

            if (vendor.CreatedOnUtc.HasValue)
                model.CreatedOnStr = _dateTimeHelper.ConvertToUserTime(vendor.CreatedOnUtc.Value, DateTimeKind.Utc).ToString();

            if (vendor.UpdatedOnUtc.HasValue)
                model.UpdatedOnStr = _dateTimeHelper.ConvertToUserTime(vendor.UpdatedOnUtc.Value, DateTimeKind.Utc).ToString();

            if (vendor.CreatedBy.HasValue)
                model.CreatedByStr = _customerService.GetCustomerFullName(_customerService.GetCustomerById(vendor.CreatedBy.Value));

            if (vendor.UpdatedBy.HasValue)
                model.UpdatedByStr = _customerService.GetCustomerFullName(_customerService.GetCustomerById(vendor.UpdatedBy.Value));

            model.ExpiryDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeExpiryDate);

            model.IssueDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeIssueDate);

            var registeredCategory = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeCategory);
            if (registeredCategory != null)
            {
                List<string> categories = registeredCategory.Split(',').ToList<string>();
                List<int> Vcategories = new List<int>();
                foreach ( var c in categories)
                {
                    Vcategories.Add(Convert.ToInt32(c));
                }
                model.SelectedVendorCategoryIds = Vcategories;
            }
            PrepareAllVendorModel(model);
            model.VendorFollowerListModel.SearchVendorId = model.Id;
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public virtual IActionResult RetrieveVendor(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null)
                return RedirectToAction("List");

            vendor.Deleted = false;
            vendor.Active = true;

            //change email format
            vendor.Email = CommonHelper.CustomerEmailFormat(vendor.Email);
            //change user name format
            vendor.Name = CommonHelper.CustomerNameFormat(vendor.Name);
            //change phone number format
            vendor.PhoneNumber = CommonHelper.CustomerPhoneNumberFormat(vendor.PhoneNumber);

            _vendorService.UpdateVendor(vendor);

            //seller/vendor is added is customer manager dropdown list.
            // var customer = _workContext.CurrentCustomer;
            var customer = _customerService.GetCustomerByVendorId(vendor.Id);
            customer.VendorId = vendor.Id;
            customer.Active = true;

            //Check vendor Format
            vendor.Email = CommonHelper.CustomerEmailFormat(vendor.Email);
            vendor.Name = CommonHelper.CustomerNameFormat(vendor.Name);
            vendor.PhoneNumber = CommonHelper.CustomerNameFormat(vendor.PhoneNumber);

            //update customer
            //var associatedCustomers = _customerService.GetAllCustomers(vendorId: vendor.Id);
            //foreach (var customerlinked in associatedCustomers)
            //{
            //    customerlinked.VendorId = 0;
            //    _customerService.UpdateCustomer(customer);
            //}
            _customerService.UpdateCustomer(customer);
            var customerVendor = _customerService.GetCustomerByVendorId(vendor.Id);
            _genericAttributeService.SaveAttribute<bool>(customerVendor, NopCustomerDefaults.customerDeleteAccountAttribute, false);

            //_genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountAttribute, vendor.Deleted);
            var notificationmessag = $"{_localizationService.GetLocalized(vendor, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("admin.vendor.retrieved")}";
            _notificationService.SuccessNotification(notificationmessag);
            return RedirectToAction("Edit", new { id = vendor.Id });

        }
        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(VendorModel model, bool continueEditing, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = _vendorService.GetVendorById(model.Id);
            if (vendor == null || vendor.Deleted)
                return RedirectToAction("List");

            //parse vendor attributes
            var vendorAttributesXml = ParseVendorAttributes(form);
            _vendorAttributeParser.GetAttributeWarnings(vendorAttributesXml).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            if (ModelState.IsValid)
            {
                var prevPictureId = vendor.PictureId;
                var prevLicenseCopyId = vendor.LicenseCopyId;
                var prevEmiratesIdCopyId = vendor.EmiratesIdCopyId;
                //cache
                var createdOn = vendor.CreatedOnUtc;
                var Active = vendor.Active;

                vendor = model.ToEntity(vendor);

                vendor.CreatedOnUtc = createdOn;
                if (vendor.FoundationAprovalStatusId != null && vendor.FoundationAprovalStatusId == (int)FoundationAprovalStatus.Aproved)
                {
                    vendor.FoundationAprovalByCustomerId = _workContext.CurrentCustomer.Id;
                }
                vendor.UpdatedOnUtc = DateTime.UtcNow;
                vendor.UpdatedBy = _workContext.CurrentCustomer.Id;


                _vendorService.UpdateVendor(vendor);

                //notify store owner here (email)
                if (model.Active && Active != model.Active)
                {

                    //notify store owner here (email)
                    _workflowMessageService.SendVendorAccountApprovedNotification(vendor, _localizationSettings.DefaultAdminLanguageId);

                }

                //vendor attributes
                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

                //activity log
                _customerActivityService.InsertActivity("EditVendor",
                    string.Format(_localizationService.GetResource("ActivityLog.EditVendor"), vendor.Id), vendor);

                //search engine name
                model.SeName = _urlRecordService.ValidateSeName(vendor, model.SeName, vendor.Name, true);
                _urlRecordService.SaveSlug(vendor, model.SeName, 0);

                //address
                var address = _addressService.GetAddressById(vendor.AddressId);
                if (address == null)
                {
                    address = model.Address.ToEntity<Address>();
                    address.CreatedOnUtc = DateTime.UtcNow;

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    _addressService.InsertAddress(address);
                    vendor.AddressId = address.Id;
                    _vendorService.UpdateVendor(vendor);
                }
                else
                {
                    address = model.Address.ToEntity(address);

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    _addressService.UpdateAddress(address);
                }

                //locales
                UpdateLocales(vendor, model);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != vendor.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }

                //delete an old "download" file (if deleted or updated)
                if (prevLicenseCopyId > 0 && prevLicenseCopyId != vendor.LicenseCopyId)
                {
                    var prevDownload = _downloadService.GetDownloadById(prevLicenseCopyId);
                    if (prevDownload != null)
                        _downloadService.DeleteDownload(prevDownload);
                }
                //delete an old "download" file (if deleted or updated)
                if (prevEmiratesIdCopyId > 0 && prevEmiratesIdCopyId != vendor.EmiratesIdCopyId)
                {
                    var prevDownload = _downloadService.GetDownloadById(prevEmiratesIdCopyId);
                    if (prevDownload != null)
                        _downloadService.DeleteDownload(prevDownload);
                }


                //update picture seo file name
                UpdatePictureSeoNames(vendor);

                var notificationmessag = $"{_localizationService.GetLocalized(vendor, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Vendors.Updated")}";
                _notificationService.SuccessNotification(notificationmessag);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = vendor.Id });
            }

            //prepare model
            model = _vendorModelFactory.PrepareVendorModel(model, vendor, true);

            if (model.FoundationAprovalByCustomerId != null && model.FoundationAprovalByCustomerId > 0)
                model.AprovalByCustomer = _customerService.GetCustomerById(Convert.ToInt32(model.FoundationAprovalByCustomerId)).Email;

            if (vendor.CreatedOnUtc.HasValue)
                model.CreatedOnStr = _dateTimeHelper.ConvertToUserTime(vendor.CreatedOnUtc.Value, DateTimeKind.Utc).ToString();

            if (vendor.UpdatedOnUtc.HasValue)
                model.UpdatedOnStr = _dateTimeHelper.ConvertToUserTime(vendor.UpdatedOnUtc.Value, DateTimeKind.Utc).ToString();

            if (vendor.CreatedBy.HasValue)
                model.CreatedByStr = _customerService.GetCustomerFullName(_customerService.GetCustomerById(vendor.CreatedBy.Value));

            if (vendor.UpdatedBy.HasValue)
                model.UpdatedByStr = _customerService.GetCustomerFullName(_customerService.GetCustomerById(vendor.UpdatedBy.Value));

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = _vendorService.GetVendorById(id);

            if (vendor == null)
                return RedirectToAction("List");
            string vendorEmail = vendor.Email;
            //check the order is in pending or processing
            var vendorOrder = _orderService.SearchOrders(vendorId: vendor.Id, deleted: false)
                .Where(x => x.OrderStatusId == (int)OrderStatus.Processing || x.OrderStatusId == (int)OrderStatus.Pending).ToList();
            if (vendorOrder.Count() > 0)
            {
                var usernotificationmessag = $"{_localizationService.GetLocalized(vendor, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.OrderStatus")}";
                _notificationService.SuccessNotification(usernotificationmessag);

                return RedirectToAction("List");
            }
            //clear associated customer references
            //var associatedCustomers = _customerService.GetAllCustomers(vendorId: vendor.Id);
            //foreach (var customer in associatedCustomers)
            //{
            //    customer.VendorId = 0;
            //    _customerService.UpdateCustomer(customer);
            //}
       //commented 13/apr/2023              

            //delete a vendor
            _vendorService.DeleteVendor(vendor);
            ////verify external login is exist 
            var externalUser = _openAuthenticationService.UserExist(vendor.Id, vendor.Email);
            if (externalUser)
            {
                var customer = new Customer { Email = vendor.Email, Id = vendor.Id };
                _openAuthenticationService.UpdateExternalAccountWithUser(customer);
            }
            var customerVendor = _customerService.GetCustomerByVendorId(vendor.Id);
            _genericAttributeService.SaveAttribute<bool>(customerVendor, NopCustomerDefaults.customerDeleteAccountAttribute, false);
            customerVendor.Active = false;
            _customerService.UpdateCustomer(customerVendor);

            //activity log
            _customerActivityService.InsertActivity("DeleteVendor",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteVendor"), vendor.Id), vendor);

            var notificationmessag = $"{_localizationService.GetLocalized(vendor, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Vendors.Deleted")}";
            _notificationService.SuccessNotification(notificationmessag);

            vendor.Email = vendorEmail;
            _workflowMessageService.SendSellerAccountDeletedNotification(vendor: vendor, _workContext.WorkingLanguage.Id);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ImportExcel(IFormFile importexcelfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportVendorFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        #endregion

        #region Vendor notes

        [HttpPost]
        public virtual IActionResult VendorNotesSelect(VendorNoteSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedDataTablesJson();

            //try to get a vendor with the specified id
            var vendor = _vendorService.GetVendorById(searchModel.VendorId)
                ?? throw new ArgumentException("No vendor found with the specified id");

            //prepare model
            var model = _vendorModelFactory.PrepareVendorNoteListModel(searchModel, vendor);

            return Json(model);
        }

        public virtual IActionResult VendorNoteAdd(int vendorId, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(message))
                return ErrorJson(_localizationService.GetResource("Admin.Vendors.VendorNotes.Fields.Note.Validation"));

            //try to get a vendor with the specified id
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return ErrorJson("Vendor cannot be loaded");

            _vendorService.InsertVendorNote(new VendorNote
            {
                Note = message,
                CreatedOnUtc = DateTime.UtcNow,
                VendorId = vendor.Id
            });

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult VendorNoteDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor note with the specified id
            var vendorNote = _vendorService.GetVendorNoteById(id)
                ?? throw new ArgumentException("No vendor note found with the specified id", nameof(id));

            _vendorService.DeleteVendorNote(vendorNote);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult AprovalUpdate(int Id, string status)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }

            var vendor = _vendorService.GetVendorById(Id);
            if (vendor == null)
                return new NullJsonResult();

            if (vendor.FoundationAprovalStatusId >= (int)FoundationAprovalStatus.Pending)
            {
                if (status == "aprove")
                {
                    vendor.FoundationAprovalStatusId = (int)FoundationAprovalStatus.Aproved;
                    vendor.FoundationAprovalByCustomerId = _workContext.CurrentCustomer.Id;
                }
                else if (status == "reject")
                {
                    vendor.FoundationAprovalStatusId = (int)FoundationAprovalStatus.Rejected;
                    vendor.FoundationAprovalByCustomerId = 0;
                }

                _vendorService.UpdateVendor(vendor);
            }

            return new NullJsonResult();
        }
        #endregion
    }
}