using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    /// <summary>
    /// Represents a vendor search model
    /// </summary>
    public partial class VendorSearchModel : BaseSearchModel
    {
        public VendorSearchModel()
        {
            SupportedByFoundations = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.Vendors.List.SearchName")]
        public string SearchName { get; set; }
     
        [NopResourceDisplayName("Admin.Foundations")]
        public int SupportedByFoundationsLabel { get; set; }
        public int sbfid { get; set; }

        public IList<SelectListItem> SupportedByFoundations { get; set; }


        #endregion
    }
}