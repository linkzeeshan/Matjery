using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    /// <summary>
    /// Represents a vendor model
    /// </summary>
    public partial class VendorModel : BaseNopEntityModel, ILocalizedModel<VendorLocalizedModel>
    {
        #region Ctor

        public VendorModel()
        {
            if (PageSize < 1)
                PageSize = 5;

            Address = new AddressModel();
            VendorAttributes = new List<VendorAttributeModel>();
            Locales = new List<VendorLocalizedModel>();
            AssociatedCustomers = new List<VendorAssociatedCustomerModel>();
            VendorNoteSearchModel = new VendorNoteSearchModel();
            AvailableVendor = new List<SelectListItem>();
            FoundationType = new List<SelectListItem>();
            AvailibleAprovalStatus = new List<SelectListItem>();
            SupportedByFoundations = new List<SelectListItem>();
            VendorProductSearchModel = new ProductSearchModel();
            VendorFollowerListModel = new VendorFollowerListModel();
            VendorReviewListModel = new VendorReviewListModel();
            AvailableCategories= new List<SelectListItem>();
        }

        #endregion

        #region Properties
        public IList<SelectListItem> AvailableVendor { get; set; }
        public IList<SelectListItem> FoundationType { get; set; }
        public IList<SelectListItem> AvailibleAprovalStatus { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.VendorOldSystemId")]
        public int VendorOldSystemId { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.Name")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Vendors.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Description")]
        public string Description { get; set; }
        public int? CreatedBy { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.CreatedBy")]
        public string CreatedByStr { get; set; }
        public int? UpdatedBy { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.UpdatedBy")]
        public string UpdatedByStr { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.LicenseNo")]
        public string LicenseNo { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.ExpiryDate")]
        public string ExpiryDate { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.IssueDate")]
        public string IssueDate { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.LicenseCopyId")]
        [UIHint("Download")]
        public int LicenseCopyId { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.LicensedBy")]
        public string LicensedBy { get; set; }
        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Vendors.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.AdminComment")]
        public string AdminComment { get; set; }

        public AddressModel Address { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Active")]
        public bool Active { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.CanPublish")]
        public bool CanPublish { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.PageSize")]
        public int PageSize { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.WhatsApp")]
        public string WhatsApp { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.Instagram")]
        public string Instagram { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.Twitter")]
        public string Twitter { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.Facebook")]
        public string Facebook { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.Googleplus")]
        public string Googleplus { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.BBM")]
        public string BBM { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.EnrollForTraining")]
        public bool EnrollForTraining { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.EmiratesId")]
        public string EmiratesId { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.EmiratesIdCopyId")]
        [UIHint("Download")]
        public int EmiratesIdCopyId { get; set; }
        public List<VendorAttributeModel> VendorAttributes { get; set; }
        [NopResourceDisplayName("admin.vendors.fields.categories")]
        public IList<int> SelectedVendorCategoryIds { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<VendorLocalizedModel> Locales { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.FoundationAprovalStatusId")]
        public int? FoundationAprovalStatusId { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.AprovalStatus")]
        public string AprovalStatus { get; set; }
        public String AprovalByCustomer { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.RegisterationTypeId")]
        public int RegisterationTypeId { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.SupportedByFoundationId")]
        public int? SupportedByFoundationId { get; set; }
        public int? FoundationAprovalByCustomerId { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.FoundationTypeId")]
        public int? FoundationTypeId { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.CreatedOn")]
        public string CreatedOnStr { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.UpdatedOn")]
        public string UpdatedOnStr { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.AssociatedCustomerEmails")]
        public IList<VendorAssociatedCustomerModel> AssociatedCustomers { get; set; }

        //vendor notes
        [NopResourceDisplayName("Admin.Vendors.VendorNotes.Fields.Note")]
        public string AddVendorNoteMessage { get; set; }
        public int TotalBlackPoints { get; set; }
        public VendorNoteSearchModel VendorNoteSearchModel { get; set; }
        [NopResourceDisplayName("Admin.Vendors.List.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Admin.Foundations")]
        public string SupportedByFoundationsLabel { get; set; }
        public int sbfid { get; set; }
        public IList<SelectListItem> SupportedByFoundations { get; set; }
        public ProductSearchModel VendorProductSearchModel { get; set; }
        public VendorFollowerListModel VendorFollowerListModel { get; set; }
        public VendorReviewListModel VendorReviewListModel { get; set; }
        public bool Deleted { get; set; }
        #endregion

        #region Nested classes

        public partial class VendorAttributeModel : BaseNopEntityModel
        {
            public VendorAttributeModel()
            {
                Values = new List<VendorAttributeValueModel>();
            }

            public string Name { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<VendorAttributeValueModel> Values { get; set; }
        }

        public partial class VendorAttributeValueModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }

        #endregion
    }

    public partial class VendorLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.SeName")]
        public string SeName { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.LicensedBy")]
        public string LicensedBy { get; set; }

    }
}