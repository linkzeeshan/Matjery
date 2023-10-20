using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Models.Vendors;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Catalog;

namespace Nop.Web.Components
{
    public class VendorReviewViewComponent : NopViewComponent
    {
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IVendorRatingService _vendorRatingService;
        private readonly CustomerSettings _customerSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        public VendorReviewViewComponent(
            IVendorService vendorService,
            IStoreContext storeContext,
            IProductService productService,
            IWorkContext workContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            IVendorRatingService vendorRatingService,
            CustomerSettings customerSettings,
            IDateTimeHelper DateTimeHelper,
            ICustomerService customerService
            )
        {
            _productService = productService;
            _dateTimeHelper = DateTimeHelper;
            _customerSettings = customerSettings;
            _vendorRatingService = vendorRatingService;
            _vendorService = vendorService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _settingService = settingService;
            _customerService = customerService;
            _localizationService = localizationService;
            _storeService = storeService;
            _storeContext = storeContext;
        }
        public IViewComponentResult Invoke(int vendorId,  bool isEmptyModel = true)
        {
             
            var model = new VendorReviewsModel();
            Vendor vendor = _vendorService.GetVendorById(vendorId);
            if (vendor != null && !vendor.Deleted)
            {
                if (isEmptyModel)
                {
                    model.AddVendorReview.Title = null;
                    model.AddVendorReview.ReviewText = null;
                    model.Items.Clear();
                    ModelState.Clear();
                }

                int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
                VendorReviewSettings vendorReviewSettings = _settingService.LoadSetting<VendorReviewSettings>(storeScope);
                PrepareVendorReviewsModel(model, vendor, vendorReviewSettings);
                if (_customerService.IsGuest(_workContext.CurrentCustomer) && !vendorReviewSettings.AllowAnonymousUsersToReviewVendor)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.OnlyRegisteredUsersCanWriteReviews"));
                }
                model.AddVendorReview.Rating = vendorReviewSettings.DefaultVendorRatingValue;
                var message = this.TempData["message"];
                model.IsDisplayed = message == null ? false : true;
                model.RatingPercentage = GetVendorRating(vendorId) > 0 ? ((GetVendorRating(vendorId) * 100)/5) : 0;
                return View(model);
            }

            else
            {
                return Content("");
            }
        }
        public double GetVendorRating(int vendorId)
        {
            var customerId = _customerService.GetCustomerByVendorId(vendorId);

            var products = _productService.SearchProducts(vendorId: vendorId);

            double vendorRating = 0;
            List<CustomersRating> CustomersRating = new List<CustomersRating>();
            foreach (var product in products)
            {
                CustomersRating customerRating = new CustomersRating();
                customerRating.Ratings = _productService.GetAllProductReviews(productId: product.Id).Sum(x => x.Rating);
                customerRating.Customers = _productService.GetAllProductReviews(productId: product.Id).Count();

                CustomersRating.Add(customerRating);
            }

            decimal totalCustomers = 0;
            decimal totalRating = 0;
            foreach (var customerrating in CustomersRating)
            {
                totalCustomers = totalCustomers + customerrating.Customers;
                totalRating = totalRating + customerrating.Ratings;
            }

            vendorRating = totalCustomers>0?(double)decimal.Round((totalRating / totalCustomers), 2, MidpointRounding.AwayFromZero):0;

            return vendorRating;
        }
        public virtual int GetActiveStoreScopeConfiguration(IStoreService storeService, IWorkContext workContext)
        {
            //ensure that we have 2 (or more) stores
            if (storeService.GetAllStores().Count < 2)
                return 0;


            var storeId = _genericAttributeService.GetAttribute<int>(workContext.CurrentCustomer, NopCustomerDefaults.AdminAreaStoreScopeConfigurationAttribute);
            var store = storeService.GetStoreById(storeId);
            return store != null ? store.Id : 0;
        }
        protected void PrepareVendorReviewsModel(VendorReviewsModel model, Vendor vendor, VendorReviewSettings vendorReviewSettings)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            string submitButtonText = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.SubmitButton");
            Guid code = Guid.NewGuid();
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, string.Concat("VendorRatingForm-", vendor.Id.ToString()), code.ToString(), _storeContext.CurrentStore.Id);
            model.Button = string.Format("<input type=\"submit\" name=\"add-vendor-review\" class=\"button-1 write-product-review-button\" value=\"{0}\" /><input type=\"hidden\" name=\"validation-code\" value=\"{1}\" />", submitButtonText, code);
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            _settingService.LoadSetting<VendorReviewSettings>(storeScope);
            model.VendorId = vendor.Id;
            model.VendorName = _localizationService.GetLocalized(vendor, x => x.Name);
            DateTime? nullable = null;
            DateTime? nullable1 = null;
            nullable = null;
            IPagedList<Nop.Core.Domain.Vendors.VendorReview> vendorReviews = _vendorRatingService.GetAllVendorReviews(0, vendor.Id, _storeContext.CurrentStore.Id, true);
            model.RatingSum = 0;
            model.TotalReviews = 0;
            foreach (Nop.Core.Domain.Vendors.VendorReview vr in vendorReviews)
            {
                Customer customer = vr.Customer;
                IList<VendorReviewModel> items = model.Items;
                VendorReviewModel vendorReviewModel = new VendorReviewModel();
                ((BaseNopEntityModel)vendorReviewModel).Id = vr.Id;
                vendorReviewModel.CustomerId = vr.CustomerId;
                vendorReviewModel.CustomerName = _customerService.FormatUsername(customer, false, 0);
                vendorReviewModel.AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !_customerService.IsGuest(customer);
                vendorReviewModel.Title = vr.Title;
                vendorReviewModel.ReviewText = vr.ReviewText;
                vendorReviewModel.Rating = vr.Rating;
                vendorReviewModel.Helpfulness = new VendorReviewHelpfulnessModel
                {
                    VendorReviewId = vr.Id,
                    HelpfulYesTotal = vr.HelpfulYesTotal,
                    HelpfulNoTotal = vr.HelpfulNoTotal
                };
                DateTime userTime = _dateTimeHelper.ConvertToUserTime(vr.CreatedOnUtc, DateTimeKind.Utc);
                vendorReviewModel.WrittenOnStr = userTime.ToString("g");
                items.Add(vendorReviewModel);
                model.RatingSum = model.RatingSum + vr.Rating;
                VendorReviewsModel totalReviews = model;
                totalReviews.TotalReviews = totalReviews.TotalReviews + 1;
            }
            model.AddVendorReview.CanCurrentCustomerLeaveReview = vendorReviewSettings.AllowAnonymousUsersToReviewVendor || !_customerService.IsGuest(_workContext.CurrentCustomer);
       

        }

    }
}
