using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class ReviewPluginService : BasePluginService, IReviewPluginService
    {
        public ProductReviewsModelResult AddBulkProductReview(List<ParamsModel.ProductReviewParamsModel> model)
        {
            ProductReviewsModelResult productReviewsModelResults = new ProductReviewsModelResult();
           
            foreach (var productReview in model)
            {
                if (productReview.Rating >0)
                {
                    var response = this.AddBulkProductReview(productReview);
                    productReviewsModelResults = (response);
                }
                   
            }
            //update Order Item IsReviewed
            
            return productReviewsModelResults;
        }
        #region Method
        public ProductReviewsModelResult AddProductReview(ParamsModel.ProductReviewParamsModel model)
        {
            ProductReviewsModelResult productReviewsModelResult = new ProductReviewsModelResult();
            Product product = this._productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
            {
                productReviewsModelResult.Message=("Product not found");
                productReviewsModelResult.Status = HttpStatusCode.BadRequest;
            }
            else if (!_customerService.IsGuest(this._workContext.CurrentCustomer) || this._catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                //var orders = _orderService.SearchOrders(productId: product.Id, customerId: _workContext.CurrentCustomer.Id, pageIndex: 0, pageSize: 1);
                //int totalCustomerOrders = orders.TotalCount;
                //var userReviews = _productService.GetAllProductReviews(customerId: _workContext.CurrentCustomer.Id, productId: product.Id);
                //int totalCustomerReviews = userReviews.TotalCount;


                var orders = _orderService.OrderCountforproduct(productId: product.Id, customerId: _workContext.CurrentCustomer.Id);
                var userReviews = _productService.GetAllProductReviews(customerId: _workContext.CurrentCustomer.Id, productId: product.Id);
                int totalCustomerReviews = userReviews.TotalCount;
                if (orders <= totalCustomerReviews)
                {
                    var apiValidationResult = new ApiValidationResult();
                    apiValidationResult.FieldValidationResult.Add(new ApiValidationResultEntry
                    {
                        ErrorDescription = _localizationService.GetResource("reviews.onlycustomerwhopurchasedcanreview")
                    });
                    productReviewsModelResult.Message = (this._localizationService.GetResource("reviews.onlycustomerwhopurchasedcanreview"));
                    productReviewsModelResult.Status = HttpStatusCode.BadRequest;
                }

                ////  int totalCustomerReviews = product.ProductReviews.Count(c => c.CustomerId == _workContext.CurrentCustomer.Id);
                //if (totalCustomerOrders <= totalCustomerReviews || totalCustomerOrders == 0)
                //{
                //    var apiValidationResult = new ApiValidationResult();
                //    apiValidationResult.FieldValidationResult.Add(new ApiValidationResultEntry
                //    {
                //        ErrorDescription = _localizationService.GetResource("reviews.onlycustomerwhopurchasedcanreview")
                //    });
                //    productReviewsModelResult.Message=(this._localizationService.GetResource("reviews.onlycustomerwhopurchasedcanreview"));
                //    productReviewsModelResult.Status = HttpStatusCode.BadRequest;
                //    //return Ok(base.Prepare(false, apiValidationResult));
                //}

                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.Fields.Title.Required"));
                }
                if (string.IsNullOrWhiteSpace(model.ReviewText))
                {
                    productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.Fields.ReviewText.Required"));
                }
                if (model.Title.Length > 200 || model.Title.Length < 1)
                {
                    productReviewsModelResult.Message=(string.Format(this._localizationService.GetResource("Reviews.Fields.Title.MaxLengthValidation"), 200));
                }
                if (string.IsNullOrEmpty(productReviewsModelResult.Message))
                {
                    if (model.Rating < 1 || model.Rating > 5)
                    {
                        model.Rating = this._catalogSettings.DefaultProductRatingValue;
                    }
                    bool isApproved = !this._catalogSettings.ProductReviewsMustBeApproved;
                    ProductReview productReview = new ProductReview();
                    productReview.ProductId = product.Id;
                    productReview.CustomerId = this._workContext.CurrentCustomer.Id;
                    productReview.Title = model.Title;
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

                    if (this._catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    {
                        this._workflowMessageService.SendProductReviewNotificationMessage(productReview, this._localizationSettings.DefaultAdminLanguageId);
                    }
                    this._customerActivityService.InsertActivity("PublicStore.AddProductReview", this._localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product);
                    if (productReview.IsApproved)
                    {
                        this._eventPublisher.Publish<ProductReviewApprovedEvent>(new ProductReviewApprovedEvent(productReview));
                    }
                    productReviewsModelResult.Status = HttpStatusCode.OK;
                    if (isApproved)
                    {
                        productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.SuccessfullyAdded"));
                        //productReviewsModelResult.ProductId = product.Id;
                        //productReviewsModelResult.ProductName = product.Name;
                        //productReviewsModelResult.ProductSeName = product.GetSeName();
                        productReviewsModelResult.CanCurrentCustomerLeaveReview = true;


                    }
                    else
                    {
                        productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.SeeAfterApproving"));
                    }
                }
                else
                {
                    productReviewsModelResult.Status = HttpStatusCode.BadRequest;
                }
            }
            else
            {
                productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
                productReviewsModelResult.Status = HttpStatusCode.BadRequest;
            }
            return productReviewsModelResult;
        }
        //copy method without text
        public ProductReviewsModelResult AddBulkProductReview(ParamsModel.ProductReviewParamsModel model)
        {
            ProductReviewsModelResult productReviewsModelResult = new ProductReviewsModelResult();
            Product product = this._productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
            {
                productReviewsModelResult.Message="Product not found";
                productReviewsModelResult.Status = HttpStatusCode.BadRequest;
            }
            else if (!_customerService.IsGuest(this._workContext.CurrentCustomer) || this._catalogSettings.AllowAnonymousUsersToReviewProduct)
            {

                var orders = _orderService.OrderCountforproduct(productId: product.Id, customerId: _workContext.CurrentCustomer.Id);
                var userReviews = _productService.GetAllProductReviews(customerId: _workContext.CurrentCustomer.Id, productId: product.Id);
                int totalCustomerReviews = userReviews.TotalCount;
                //  int totalCustomerReviews = product.ProductReviews.Count(c => c.CustomerId == _workContext.CurrentCustomer.Id);
                if (orders <= totalCustomerReviews)
                {
                    var apiValidationResult = new ApiValidationResult();
                    apiValidationResult.FieldValidationResult.Add(new ApiValidationResultEntry
                    {
                        ErrorDescription = _localizationService.GetResource("reviews.onlycustomerwhopurchasedcanreview")
                    });
                    productReviewsModelResult.Message =(this._localizationService.GetResource("reviews.onlycustomerwhopurchasedcanreview"));
                    productReviewsModelResult.Status = HttpStatusCode.BadRequest;
                    //return Ok(base.Prepare(false, apiValidationResult));
                }

               
                if (string.IsNullOrEmpty(productReviewsModelResult.Message))
                {
                    if (model.Rating < 1 || model.Rating > 5)
                    {
                        model.Rating = this._catalogSettings.DefaultProductRatingValue;
                    }
                    bool isApproved = !this._catalogSettings.ProductReviewsMustBeApproved;
                    ProductReview productReview = new ProductReview();
                    productReview.ProductId = product.Id;
                    productReview.CustomerId = this._workContext.CurrentCustomer.Id;
                    productReview.Title = model.Title;
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

                    if (this._catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    {
                        this._workflowMessageService.SendProductReviewNotificationMessage(productReview, this._localizationSettings.DefaultAdminLanguageId);
                    }
                    this._customerActivityService.InsertActivity("PublicStore.AddProductReview", this._localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product);
                    if (productReview.IsApproved)
                    {
                        this._eventPublisher.Publish<ProductReviewApprovedEvent>(new ProductReviewApprovedEvent(productReview));
                    }
                    productReviewsModelResult.Status = HttpStatusCode.OK;
                    if (isApproved)
                    {
                        productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.SuccessfullyAdded"));
                        //productReviewsModelResult.ProductId = product.Id;
                        //productReviewsModelResult.ProductName = product.Name;
                        //productReviewsModelResult.ProductSeName = product.GetSeName();
                        //productReviewsModelResult.CanCurrentCustomerLeaveReview = true;
                        //productReviewsModelResult.DefaultProductRatingValue = model.Rating;


                    }
                    else
                    {
                        productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.SeeAfterApproving"));
                    }
                }
                else
                {
                    productReviewsModelResult.Status = HttpStatusCode.BadRequest;
                }
            }
            else
            {
                productReviewsModelResult.Message=(this._localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
                productReviewsModelResult.Status = HttpStatusCode.BadRequest;
            }
            return productReviewsModelResult;
        }
        public List<ProductForReviewsModelResult> GetProductForReviews()
        {
            var products = _productService.GetCustomerLastProductOrderItem(customerId: _workContext.CurrentCustomer.Id, approved: true, storeId: _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : 0).AsEnumerable();
            List<ProductForReviewsModelResult> productForReviews = new List<ProductForReviewsModelResult>();
            foreach (var _orderItem in products)
            {
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

                    ProductForReviewsModelResult vm = new ProductForReviewsModelResult();

                    string imageUrl = "";
                
                    if (pictureById != null)
                    {
                        imageUrl = _pictureService.GetPictureUrl(pictureById.Id, storeLocation: _storeContext.CurrentStore.Url, targetSize: _mediaSettings.ProductThumbPictureSize);
                    }
                    else
                    {
                        var model = _productModelFactory.PrepareProductDetailsModel(_productService.GetProductById(_orderItem.ProductId));
                        imageUrl = model.DefaultPictureModel.ImageUrl;
                    }
                    vm.Status = HttpStatusCode.OK;
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

        public ProductReviewsModelResult GetProductReviews(int productId)
        {
            var productReviewModel = new ProductReviewsModelResult();
            try
            {
                Product product = this._productService.GetProductById(productId);
                if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                {
                    throw new ApplicationException("Not found");
                }
                //IOrderedEnumerable<ProductReview> productReviews = from pr in product.ProductReviews
                //                                                   where pr.IsApproved
                //                                                   orderby pr.CreatedOnUtc
                //                                                   select pr;
             

                    var productReviews = _productService.GetAllProductReviews(
                     approved: true,
                     productId: product.Id,
                     storeId: _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : 0).AsEnumerable();

                productReviews = _catalogSettings.ProductReviewsSortByCreatedDateAscending
                    ? productReviews.OrderBy(pr => pr.CreatedOnUtc)
                    : productReviews.OrderByDescending(pr => pr.CreatedOnUtc);

                productReviewModel = new ProductReviewsModelResult
                {
                    ProductId = product.Id,
                    ProductName = _localizationService.GetLocalized(product, p => p.Name),
                    ProductSeName = _urlRecordService.GetSeName(product),
                };

                foreach (ProductReview productReview in productReviews)
                {
                    //Customer customer = productReview.Customer;
                    var customer = _customerService.GetCustomerById(productReview.CustomerId);
                    DateTime userTime = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
                    productReviewModel.Reviews.Add(new ProductReviewsModelResult.ProductReviews()
                    {
                        ProductReviewId = productReview.Id,
                        CustomerId = productReview.CustomerId,
                        CustomerName = _customerService.IsGuest(this._workContext.CurrentCustomer) == true ? _localizationService.GetResource("Customer.Guest") : _customerService.FormatUsername(customer),
                        AllowViewingProfiles = this._customerSettings.AllowViewingProfiles && customer != null && !!_customerService.IsGuest(this._workContext.CurrentCustomer),
                        Title = productReview.Title,
                        ReviewText = productReview.ReviewText,
                        Rating = productReview.Rating,
                        HelpfulYesTotal = productReview.HelpfulYesTotal,
                        HelpfulNoTotal = productReview.HelpfulNoTotal,
                        CreatedON = userTime.ToShortDateString()
                    }); ;
                }
                productReviewModel.CanCurrentCustomerLeaveReview = this._catalogSettings.AllowAnonymousUsersToReviewProduct || !_customerService.IsGuest(this._workContext.CurrentCustomer);
                //if (this._workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                //{
                //    throw new ApplicationException(this._localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
                //}
                productReviewModel.DefaultProductRatingValue = this._catalogSettings.DefaultProductRatingValue;
                return productReviewModel;

            }
            catch (Exception ex)
            {

            }
            return productReviewModel;
        }

        public List<ProductReview>  GetProductReviewsByProductId(int productId, int customerId=0)
        {
            List<ProductReview> productReviews = new List<ProductReview>();
            try
            {

                productReviews = _productService.GetAllProductReviews(
                approved: true,
                productId: productId,
                customerId: customerId,
                storeId: _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : 0).AsEnumerable().ToList();

                return productReviews;
            }
            catch (Exception ex)
            {

            }
            return productReviews;
        }

        public double GetVendorRating(int vendorId)
        {
            var customerId = _customerService.GetCustomerByVendorId(vendorId);

            var products = _productService.SearchProducts(vendorId: vendorId);

            double vendorRating = 0;
            List<CustomersRating> CustomersRating = new List<CustomersRating>();
            foreach(var product in products)
            {
                CustomersRating customerRating = new CustomersRating();
                customerRating.Ratings = _productService.GetAllProductReviews(productId: product.Id).Sum(x => x.Rating);
                customerRating.Customers = _productService.GetAllProductReviews(productId: product.Id).Count();

                CustomersRating.Add(customerRating);
            }

            decimal totalCustomers = 0;
            decimal totalRating = 0;
            foreach(var customerrating in CustomersRating)
            {
                totalCustomers = totalCustomers + customerrating.Customers;
                totalRating = totalRating + customerrating.Ratings;
            }
            if (totalCustomers > 0)
            {
                vendorRating = (double)decimal.Round((totalRating / totalCustomers), 2, MidpointRounding.AwayFromZero);
            }

            return vendorRating;
        }
        #endregion
    }
}
