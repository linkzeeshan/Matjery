
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Notification
{
    public partial class NotificationModel : BaseNopEntityModel , ILocalizedModel<NotificationLocalizedModel>
    {
        public NotificationModel()
        {
            Locales = new List<NotificationLocalizedModel>();
            AvailableStores = new List<SelectListItem>();
            AvailableTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.Message")]
        public string Message { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.CreatedOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.Type")]
        public string Type { get; set; }
        public string TypeName { get; set; }
        public int TypeId { get; set; }

        public IList<NotificationLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.List.Stores")]
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableTypes { get; set; }
    }
    public partial class NotificationLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Notification.Fields.Message")]
        public string Message { get; set; }
    }
}