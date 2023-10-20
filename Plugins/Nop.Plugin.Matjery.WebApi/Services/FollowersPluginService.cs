using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Extensions;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class FollowersPluginService : IFollowersPluginService
    {
        private readonly IVendorFollowerService _vendorFollowerService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        protected readonly IWebHelper _webHelper;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly VendorSettings _vendorSettings;
        protected readonly IPictureService _pictureService;
        protected readonly MediaSettings _mediaSettings;
        protected readonly IProductService _productService;
        private readonly IVendorRatingService _vendorRatingService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly IVendorService _vendorService;

        public FollowersPluginService()
        {
            _vendorFollowerService = EngineContext.Current.Resolve<IVendorFollowerService>();
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _dateTimeHelper = EngineContext.Current.Resolve<IDateTimeHelper>();
            _webHelper = EngineContext.Current.Resolve<IWebHelper>();
            _urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
            _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            _languageService = EngineContext.Current.Resolve<ILanguageService>();
            _vendorSettings = EngineContext.Current.Resolve<VendorSettings>();
            _pictureService = EngineContext.Current.Resolve<IPictureService>();
            _mediaSettings = EngineContext.Current.Resolve<MediaSettings>();
            _productService = EngineContext.Current.Resolve<IProductService>();
            _vendorRatingService = EngineContext.Current.Resolve<IVendorRatingService>();
            _customerService = EngineContext.Current.Resolve<ICustomerService>();
            _downloadService = EngineContext.Current.Resolve<IDownloadService>();
            _vendorService = EngineContext.Current.Resolve<IVendorService>();

        }
        public IEnumerable<object> GetAllFollowedVendors()
        {
            IEnumerable<object> followers = new List<VendorFollower>();
            try
            {
                var followerList = _vendorFollowerService.GetAllFollowers(customerId: _workContext.CurrentCustomer.Id)
                   .Where(f => f.VendorId != 0);


                followers = followerList.Select(f => new
                {
                    Id = f.Id,
                    FollowedOn = this._dateTimeHelper.ConvertToUserTime(f.FollowOnUtc, DateTimeKind.Utc).ToString("D",
                        new CultureInfo(_workContext.WorkingLanguage.LanguageCulture)),
                    Vendor = this.PrepareVendorModel(_vendorService.GetVendorById(f.VendorId))
                });

            }
            catch (Exception ex)
            {
                throw;
            }
            return followers;
        }
        public VendorResult PrepareVendorModel(Vendor vendor)
        {
            string vendorUrl = string.Format("{0}v/{1}", _webHelper.GetStoreLocation(),
               _urlRecordService.GetSeName(vendor, _workContext.WorkingLanguage.Id));

            var vendorModel = new VendorResult
            {
                Id = vendor.Id,
                Name = _localizationService.GetLocalized(vendor, x => x.Name),
                Email = _localizationService.GetLocalized(vendor, x => x.Email),
                VendorUrl = vendorUrl,
                Description = _localizationService.GetLocalized(vendor, x => x.Description),
                SeName = _urlRecordService.GetSeName(vendor),
                RegisterationTypeId = vendor.RegisterationTypeId,
                AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors,
                WhatsApp = vendor.WhatsApp,
                EmiratesId = vendor.EmiratesId,
                PictureId = vendor.PictureId,
                PhoneNumber = vendor.PhoneNumber,
                Instagram = vendor.Instagram,
                Twitter = vendor.Twitter,
                Facebook = vendor.Facebook,
                Googleplus = vendor.Googleplus,
                BBM = vendor.BBM,
                EnrollForTraining = vendor.EnrollForTraining,
                FoundationAprovalStatusId = vendor.FoundationAprovalStatusId,
                FoundationTypeId = vendor.FoundationTypeId,
                SupportedByFoundationId = vendor.SupportedByFoundationId,
                DisplayOrder = vendor.DisplayOrder,
                Active = vendor.Active
            };
            if (vendor.FoundationAprovalStatusId.HasValue)
            {
                vendorModel.FoundationAprovalStatusText = _localizationService.GetLocalizedEnum(vendor.FoundationAprovalStatus, _workContext.WorkingLanguage.Id);
            }
            vendorModel.ShortDescription = vendorModel.Description.TruncateToShort();
            foreach (Language language in _languageService.GetAllLanguages())
            {
                var localeModel = new
                {
                    Name = _localizationService.GetLocalized(vendor, x => x.Name, language.Id),
                    Description = _localizationService.GetLocalized(vendor, x => x.Description, language.Id),// vendor.GetLocalized(x => x.Description, language.Id),
                    ShortDescription = _localizationService.GetLocalized(vendor, x => x.Description, language.Id).TruncateToShort(), //vendor.GetLocalized(x => x.Description, language.Id).TruncateToShort(),
                    LicensedBy = _localizationService.GetLocalized(vendor, x => x.LicensedBy, language.Id), // vendor.GetLocalized(v => v.LicensedBy, language.Id)
                };
                vendorModel.CustomProperties.Add(language.UniqueSeoCode, localeModel);
            }

            //prepare picture model
            //prepare picture model
            int pictureSize = _mediaSettings.VendorThumbPictureSize;
            var picture = _pictureService.GetPictureById(vendor.PictureId);

            vendorModel.FullSizeImageUrl = picture!=null? _pictureService.GetPictureUrl(picture.Id, defaultPictureType: PictureType.VendorOrEntity):_pictureService.GetDefaultPictureUrl(defaultPictureType: PictureType.VendorOrEntity);
            vendorModel.ImageUrl = picture != null ? _pictureService.GetPictureUrl(picture.Id, pictureSize, defaultPictureType: PictureType.VendorOrEntity): _pictureService.GetDefaultPictureUrl(defaultPictureType: PictureType.VendorOrEntity);
            //vendorModel.WaterMarkImageUrl = _miscWatermarkPictureService.GetPictureUrlWithWatermark(picture, pictureSize, defaultPictureType: PictureType.VendorOrEntity);
            vendorModel.TotalProducts = _productService.GetNumberOfProductsByVendorId(vendor.Id);

            //reviews
            var vendorReviews = _vendorRatingService.GetAllVendorReviews(vendorId: vendor.Id);
            if (vendorReviews.TotalCount != 0)
            {
                double percentage = vendorReviews.Sum(vr => vr.Rating) * 100 / vendorReviews.TotalCount / 5;
                //5= maximut rating values
                vendorModel.VendorReview.TotalRating = percentage * 5 / 100;
            }
            //followers
            var vendorFollowers = _vendorFollowerService.GetAllFollowers(vendorId: vendor.Id);
            foreach (VendorFollower vr in vendorFollowers)
            {
                var vendorfollowerstModel = new VendorResult.VendorFollowerModel.FollowersModel();
                vendorfollowerstModel.CustomerId = vr.CustomerId;
                vendorfollowerstModel.CustomerName = _customerService.FormatUsername(vr.Customer);
                DateTime userTime = _dateTimeHelper.ConvertToUserTime(vr.FollowOnUtc, DateTimeKind.Utc);
                vendorfollowerstModel.FollowOnStr = userTime.ToShortDateString();
                vendorModel.VendorFollower.Followers.Add(vendorfollowerstModel);
            }
            //license
            vendorModel.LicenseNo = vendor.LicenseNo;
            if (vendor.LicenseCopyId > 0)
            {
                var licenseCopyDownload = _downloadService.GetDownloadById(vendor.LicenseCopyId);
                if (licenseCopyDownload != null)
                    vendorModel.HasUploadedLicense = true;
            }

            vendorModel.VendorFollower.TotalFollowers = vendorFollowers.TotalCount;
            return vendorModel;
        }
    }
}
