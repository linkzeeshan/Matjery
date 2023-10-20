using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public partial class VendorResult : BaseNopModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string FullSizeImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string SeName { get; set; }
        public bool AllowCustomersToContactVendors { get; set; }
        public object PictureModel { get; set; }
        public int RegisterationTypeId { get; set; }
        public string ShortDescription { get; set; }
        public string WhatsApp { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Googleplus { get; set; }

        public VendorReviewModel VendorReview { get; set; }
        public VendorFollowerModel VendorFollower { get; set; }
        public int TotalProducts { get; set; }
        public int? FoundationTypeId { get; set; }
        public int? FoundationAprovalStatusId { get; set; }
        public int? SupportedByFoundationId { get; set; }
        public string FoundationAprovalStatusText { get; set; }
        public string EmiratesId { get; set; }
        public int PictureId { get; set; }
        public string VendorUrl { get; set; }
        public int DisplayOrder { get; internal set; }
        public bool Active { get; set; }
        public string LicenseNo { get; set; }
        public bool HasUploadedLicense { get; set; }
        public string LicensedBy { get; set; }
        public string BBM { get; set; }
        public bool EnrollForTraining { get; set; }
        public string WaterMarkImageUrl { get; set; }

        public string[] CategoryId { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string TradeLicenseNumber { get; set; }
        public string TradeLicenseFile { get; set; }
        public string FileName { get; set; }
        public int LicenseId { get; set; }

        public VendorResult()
        {
            this.VendorFollower = new VendorFollowerModel();
            this.VendorReview = new VendorReviewModel();
        }

        public class VendorReviewModel
        {
            public double TotalRating { get; set; }
        }

        public class VendorFollowerModel
        {
            public VendorFollowerModel()
            {
                this.Followers = new List<FollowersModel>();
            }
            public double TotalFollowers { get; set; }
            public IList<FollowersModel> Followers { get; set; }

            public class FollowersModel
            {
                public int CustomerId { get; set; }
                public string CustomerName { get; set; }
                public int VendorId { get; set; }
                public string VendorName { get; set; }
                public string FollowOnStr { get; set; }
            }
        }
    }

}
