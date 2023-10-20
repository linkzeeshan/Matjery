using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.BlackPoint
{
    public partial class BlackPointSearchModel : BaseSearchModel
    {
        public BlackPointSearchModel()
        {
            AvailablePointType = new List<SelectListItem>();
            AvailablePointStatus = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailablePointType { get; set; }
        public IList<SelectListItem> AvailablePointStatus { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.BlackPointType")]
        public int BlackPointType { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.BlackPointStatus")]
        public int BlackPointStatus { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Vendor")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Customer")]
        public string AddedBy { get; set; } = string.Empty;

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Vendor")]
        public int vendorId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
