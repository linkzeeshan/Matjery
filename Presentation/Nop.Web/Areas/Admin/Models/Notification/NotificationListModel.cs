

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Notification
{
    public class NotificationListModel : BasePagedListModel<NotificationModel>
    {
        public NotificationListModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableTypes = new List<SelectListItem>();
        }
        public bool IsAdmin { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Campaigns.List.Stores")]
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableTypes { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.Type")]
        public int TypeId { get; set; }
    }
}