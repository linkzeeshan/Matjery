using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class VendorModel : BaseNopEntityModel
    {
        public VendorModel()
        {
            PictureModel = new PictureModel();
            SupportedByFoundationPictureModel = new PictureModel();
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            SearchedVendors = new List<VendorModel>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public bool AllowCustomersToContactVendors { get; set; }
        public bool ShowFollowbutton { get; set; }
        public bool imFollowing { get; set; }
        public string LicenseNo { get; set; }
        public bool HasUploadedLicense { get; set; }
        public string LicensedBy { get; set; }
        public int? SupportedByFoundationId { get; set; }
        public string SupportedByFoundationDescription { get; set; }
        public string VendorUrl { get; set; }
        public string SupportedBySeName { get; set; }

        //social media
        public string WhatsApp { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Googleplus { get; set; }
        public string BBM { get; set; }
        public string PhoneNumber { get; set; }

        //vendor picture model
        public PictureModel PictureModel { get; set; }
        //supported by picture model
        public PictureModel SupportedByFoundationPictureModel { get; set; }

        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

        public IList<ProductOverviewModel> Products { get; set; }
        public IList<VendorModel> SearchedVendors { get; set; }
    }
}