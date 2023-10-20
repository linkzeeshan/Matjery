using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    public class NotificationSearchModel : BaseSearchModel
    {
        public NotificationSearchModel()
        {
            AvailableTypes = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.Notification.Fields.Name")]
        public string SearchName { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.Type")]
        public string Type { get; set; }
        public string TypeName { get; set; }
        public int TypeId { get; set; }
        public bool IsAdmin { get; set; }

        //[NopResourceDisplayName("Admin.Promotions.Campaigns.List.Stores")]
        //public int StoreId { get; set; }
        //public IList<SelectListItem> AvailableStores { get; set; }
        //  public bool IsAdmin { get; set; }
        public IList<SelectListItem> AvailableTypes { get; set; }
    }
}
