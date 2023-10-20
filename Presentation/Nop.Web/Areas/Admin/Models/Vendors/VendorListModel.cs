using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    /// <summary>
    /// Represents a vendor list model
    /// </summary>
    public partial class VendorListModel : BasePagedListModel<VendorModel>
    {

        public VendorListModel()
        {
            this.SupportedByFoundations = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Vendors.List.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Admin.Foundations")]
        public string SupportedByFoundationsLabel { get; set; }
        public int sbfid { get; set; }

        public IList<SelectListItem> SupportedByFoundations { get; set; }
    }
}