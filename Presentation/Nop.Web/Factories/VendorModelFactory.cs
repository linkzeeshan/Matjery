using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.Vendors;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the vendor model factory
    /// </summary>
    public partial class VendorModelFactory : IVendorModelFactory
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CommonSettings _commonSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IVendorFollowerService _vendorFollowerService;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly IDownloadService _downloadService;
        private readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public VendorModelFactory(CaptchaSettings captchaSettings,
            IVendorFollowerService vendorFollowerService,
            IVendorService vendorService,
            ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            ICacheKeyService cacheKeyService,
            IStaticCacheManager cacheManager,
            IWebHelper webHelper,
            IStoreContext storeContext,
            CommonSettings commonSettings,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            IDownloadService downloadService,
            VendorSettings vendorSettings)
        {
            _vendorFollowerService = vendorFollowerService;
            _categoryService = categoryService;
            _vendorService = vendorService;
            _urlRecordService = urlRecordService;
            _cacheKeyService = cacheKeyService;
            _cacheManager = cacheManager;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _captchaSettings = captchaSettings;
            _commonSettings = commonSettings;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
            _downloadService = downloadService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare vendor attribute models
        /// </summary>
        /// <param name="vendorAttributesXml">Vendor attributes in XML format</param>
        /// <returns>List of the vendor attribute model</returns>
        protected virtual IList<VendorAttributeModel> PrepareVendorAttributes(string vendorAttributesXml)
        {
            var result = new List<VendorAttributeModel>();

            var vendorAttributes = _vendorAttributeService.GetAllVendorAttributes();
            foreach (var attribute in vendorAttributes)
            {
                var attributeModel = new VendorAttributeModel
                {
                    Id = attribute.Id,
                    Name = _localizationService.GetLocalized(attribute, x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _vendorAttributeService.GetVendorAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new VendorAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = _localizationService.GetLocalized(attributeValue, x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(vendorAttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = _vendorAttributeParser.ParseVendorAttributeValues(vendorAttributesXml);
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
                            if (!string.IsNullOrEmpty(vendorAttributesXml))
                            {
                                var enteredText = _vendorAttributeParser.ParseValues(vendorAttributesXml, attribute.Id);
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

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the apply vendor model
        /// </summary>
        /// <param name="model">The apply vendor model</param>
        /// <param name="validateVendor">Whether to validate that the customer is already a vendor</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="vendorAttributesXml">Vendor attributes in XML format</param>
        /// <returns>The apply vendor model</returns>
        public virtual ApplyVendorModel PrepareApplyVendorModel(ApplyVendorModel model,
            bool validateVendor, bool excludeProperties, string vendorAttributesXml)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (validateVendor && _workContext.CurrentCustomer.VendorId > 0)
            {
                //already applied for vendor account
                model.DisableFormInput = true;
                model.Result = _localizationService.GetResource("Vendors.ApplyAccount.AlreadyApplied");
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnApplyVendorPage;
            model.TermsOfServiceEnabled = _vendorSettings.TermsOfServiceEnabled;
            model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;

            if (!excludeProperties)
            {
                model.Email = _workContext.CurrentCustomer.Email;
            }

            //vendor attributes
            model.VendorAttributes = PrepareVendorAttributes(vendorAttributesXml);
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
        /// Prepare the vendor info model
        /// </summary>
        /// <param name="model">Vendor info model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overriddenVendorAttributesXml">Overridden vendor attributes in XML format; pass null to use VendorAttributes of vendor</param>
        /// <returns>Vendor info model</returns>
        public virtual VendorInfoModel PrepareVendorInfoModel(VendorInfoModel model,
            bool excludeProperties, string overriddenVendorAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var vendor = _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId);

            if (vendor != null)
            {
                if (!excludeProperties)
                {

                    model.Description = vendor.Description;
                    model.Email = vendor.Email;
                    model.Name = vendor.Name;
                    model.EmiratesId = vendor.EmiratesId;
                    model.WhatsApp = vendor.WhatsApp;
                    model.PhoneNumber = vendor.PhoneNumber;
                    model.Instagram = vendor.Instagram;
                    model.Twitter = vendor.Twitter;
                    model.Facebook = vendor.Facebook;
                    model.Googleplus = vendor.Googleplus;
                    model.BBM = vendor.BBM;
                    model.EnrollForTraining = vendor.EnrollForTraining;
                    model.RegisterationTypeId = vendor.RegisterationTypeId;
                    model.SupportedByFoundationId = vendor.SupportedByFoundationId;
                    model.TradeLicenseNumber = vendor.LicenseNo;
                    model.FoundationAprovalStatus = _localizationService.GetLocalizedEnum(vendor.FoundationAprovalStatus, _workContext.WorkingLanguage.Id);


                }

                Download download = _downloadService.GetDownloadById(vendor.LicenseCopyId);
                model.TradeLicenseFileurl = download == null ? "" : download.DownloadGuid.ToString();
                var picture = _pictureService.GetPictureById(vendor.PictureId);
                var pictureSize = _mediaSettings.AvatarPictureSize;
                model.PictureUrl = picture != null ? _pictureService.GetPictureUrl(ref picture, pictureSize) : string.Empty;

                //vendor attributes
                if (string.IsNullOrEmpty(overriddenVendorAttributesXml))
                    overriddenVendorAttributesXml = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.VendorAttributes);

                model.ExpiryDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeExpiryDate);

                model.IssueDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeIssueDate);

                model.VendorAttributes = PrepareVendorAttributes(overriddenVendorAttributesXml);

                model.TradelicenseStatus = _vendorService.GetVendorLicenseStatus(vendor);
                model.TradelicenseStatus.Message = LicenseAlert(model.TradelicenseStatus.Id);


            }


            return model;
        }
        public string LicenseAlert(int status)
        {
            string Message = "";
            switch (status)
            {
                case (int)VendorLicenseStatus.Active:
                    Message = _localizationService.GetLocaleStringResourceByName("account.vendorinfo.licenseactive").ResourceValue;
                    break;
                case (int)VendorLicenseStatus.Expired:
                    Message = _localizationService.GetLocaleStringResourceByName("account.register.errors.licenseexpiry").ResourceValue;
                    break;
                case (int)VendorLicenseStatus.AboutToExpire:
                    Message = _localizationService.GetLocaleStringResourceByName("account.vendorinfo.licensenotification").ResourceValue;
                    break;

                default:
                    Message = "";
                    break;
            }
            return Message;
        }

        public VendorListModel PrepareFollowedVendor(CatalogPagingFilteringModel command)
        {

            var model = new VendorListModel();
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            var followerList = _vendorFollowerService.GetAllFollowers(
                _workContext.CurrentCustomer.Id);

            foreach (var vendor in followerList)
            {
                var vendorRow = _vendorService.GetVendorById(vendor.VendorId);
                if (vendorRow != null)
                {
                    var vendorView = new VendorModel
                    {
                        Id = vendorRow.Id,
                        Name = string.IsNullOrEmpty(vendorRow.Name) ? "" : _localizationService.GetLocalized(vendorRow, x => x.Name),
                        Description = string.IsNullOrEmpty(vendorRow.Description) ? "" : _localizationService.GetLocalized(vendorRow, x => x.Description),
                        MetaKeywords = string.IsNullOrEmpty(vendorRow.MetaKeywords) ? "" : _localizationService.GetLocalized(vendorRow, x => x.MetaKeywords),
                        MetaDescription = string.IsNullOrEmpty(vendorRow.MetaDescription) ? "" : _localizationService.GetLocalized(vendorRow, x => x.MetaDescription),
                        MetaTitle = string.IsNullOrEmpty(vendorRow.MetaTitle) ? "" : _localizationService.GetLocalized(vendorRow, x => x.MetaTitle),
                        SeName = string.IsNullOrEmpty(vendorRow.Name) ? "" : _urlRecordService.GetSeName(vendorRow),
                        AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors
                    };
                    //prepare picture model
                    int pictureSize = _mediaSettings.VendorThumbPictureSize;
                    var pictureCacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.VendorPictureModelKey,
                       vendor.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    vendorView.PictureModel = _cacheManager.Get(pictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(vendorRow.PictureId);
                      
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = picture==null? _pictureService.GetDefaultPictureUrl(defaultPictureType: PictureType.VendorOrEntity): _pictureService.GetPictureUrl(picture.Id, defaultPictureType: PictureType.VendorOrEntity),
                            ImageUrl = picture == null ? _pictureService.GetDefaultPictureUrl(defaultPictureType: PictureType.VendorOrEntity) : _pictureService.GetPictureUrl(picture.Id, pictureSize, defaultPictureType: PictureType.VendorOrEntity),
                            Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), vendorView.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), vendorView.Name)
                        };
                        return pictureModel;
                    });

                    model.VendorModel.Add(vendorView);
                }
            }
            return model;
        }


        #endregion
    }
}