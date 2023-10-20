using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public interface IReviewPluginService
    {
        ProductReviewsModelResult AddProductReview(ParamsModel.ProductReviewParamsModel model);
        ProductReviewsModelResult AddBulkProductReview(List<ParamsModel.ProductReviewParamsModel> model);
        ProductReviewsModelResult GetProductReviews(int productId);
        List<ProductForReviewsModelResult> GetProductForReviews();
        double GetVendorRating(int vendorId);

    }
}
