using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface IVendorPluginService
    {
        List<VendorResult> GetAllVendors(DateTime? syncDate = null);
        VendorResult GetVendorById(int vendorId);
        TradelicenseStatus GetVendorLicenseStatus();
        List<VendorResult> GetVendorSupportedVendors(int vendorId);
        (VendorApplyResult, ApiValidationResultResponse) ApplyVendor(ParamsModel.VendorApplyParamsModel model, IFormFile license);
        (VendorApplyResult, ApiValidationResultResponse) ApplyFoundation(ParamsModel.VendorApplyParamsModel model);
        (VendorResult, ApiValidationResultResponse) SaveVendorInfo(ParamsModel.VendorInfoParamsModel model, IFormFile license);
        VendorContactResult ContactVendor(ParamsModel.ContactVendorParamsModel model);
        (VendorReviewResultModel, ApiValidationResultResponse) VendorReviewAdd(ParamsModel.VendorReviewAddParamsModel model);
        List<VendorReviewListResultModel> GetAllVendorReviews(int vendorId);
        (VendorResult, ApiValidationResultResponse) VendorFollowAndUnfollow(ParamsModel.VendorFollowAddParamsModel model);
        bool VendorSupportedByAdd(ParamsModel.SupportedByVendorAddParamsModel model);
        List<VendorResult> GetAllVendors(int pageIndex = 0, int pageSize = int.MaxValue,bool displayEntities=false);

    }
}
