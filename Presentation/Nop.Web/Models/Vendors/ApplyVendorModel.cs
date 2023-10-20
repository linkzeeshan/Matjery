using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Web.Models.Vendors
{
    public partial class ApplyVendorModel : BaseNopModel
    {
        public ApplyVendorModel()
        {
            VendorAttributes = new List<VendorAttributeModel>();
            Categories = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Vendors.ApplyAccount.Name")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Vendors.ApplyAccount.Email")]
        public string Email { get; set; }
        public int PictureId { get; set; }

        [NopResourceDisplayName("Vendors.ApplyAccount.Description")]
        public string Description { get; set; }

        public IList<VendorAttributeModel> VendorAttributes { get; set; }
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
        [NopResourceDisplayName("Admin.Vendors.Fields.PhoneNumber")]

        public string PhoneNumber { get; set; }
        [NopResourceDisplayName("Admin.Vendors.Fields.EmiratesId")]
      
        public string EmiratesId { get; set; }
        public bool DisplayCaptcha { get; set; }

        public bool TermsOfServiceEnabled { get; set; }
        public bool TermsOfServicePopup { get; set; }

        public bool DisableFormInput { get; set; }
        public string Result { get; set; }
        public bool IsUaePass { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.CategoryId")]
        public IEnumerable<int> CategoryId { get; set; }

        public string[] SelectedValues { get; set; }
        public IList<SelectListItem> Categories { get; set; }


        [NopResourceDisplayName("Account.VendorInfo.IssueDate")]
        public string IssueDate { get; set; }
        [NopResourceDisplayName("Account.VendorInfo.ExpiryDate")]
        public string ExpiryDate { get; set; }
        [NopResourceDisplayName("Account.VendorInfo.TradeLicenseNumber")]
        public string TradeLicenseNumber { get; set; }
        [NopResourceDisplayName("Account.VendorInfo.TradeLicenseFile")]
        public string TradeLicenseFile { get; set; }

    }
}