using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using Nop.Core.Domain.Vendors;

namespace Nop.Web.Models.Vendors
{
    public class VendorInfoModel : BaseNopModel
    {
        public VendorInfoModel()
        {
            AvailableSupportedByFoundations = new List<SelectListItem>();
            VendorAttributes = new List<VendorAttributeModel>();
            Categories = new List<SelectListItem>();
            TradelicenseStatus = new TradelicenseStatus();

        }

        [NopResourceDisplayName("Account.VendorInfo.Name")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.VendorInfo.Email")]
        public string Email { get; set; }


        [NopResourceDisplayName("Account.VendorInfo.CategoryId")]
        public IEnumerable<int> CategoryId { get; set; }
        public IList<int> CategoryName { get; set; }
        public IList<SelectListItem> Categories { get; set; }



        [NopResourceDisplayName("Account.VendorInfo.IssueDate")]
        public string IssueDate { get; set; }
        [NopResourceDisplayName("Account.VendorInfo.ExpiryDate")]
        public string ExpiryDate { get; set; }
        [NopResourceDisplayName("Account.VendorInfo.TradeLicenseNumber")]
        public string TradeLicenseNumber { get; set; }
        [NopResourceDisplayName("Account.VendorInfo.TradeLicenseFile")]
        public string TradeLicenseFile { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.TradeLicenseFile")]
        public string TradeLicenseFileurl { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("account.vendorinfo.phonenumber")]
     
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Picture")]
        public string PictureUrl { get; set; }
        [NopResourceDisplayName("Admin.Foundations.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("SupportedByFoundationId")]
        public int? SupportedByFoundationId { get; set; }
        public IList<SelectListItem> AvailableSupportedByFoundations { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.EmiratesId")]
        public string EmiratesId { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.WhatsApp")]
        public string WhatsApp { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Instagram")]
        public string Instagram { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.BBM")]
        public string BBM { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.EnrollForTraining")]
        public bool EnrollForTraining { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Twitter")]
        public string Twitter { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Facebook")]
        public string Facebook { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Googleplus")]
        public string Googleplus { get; set; }

        public int RegisterationTypeId { get; set; }
        public string FoundationAprovalStatus { get; set; }

        public IList<VendorAttributeModel> VendorAttributes { get; set; }

        public TradelicenseStatus TradelicenseStatus { get; set; }
    }
}