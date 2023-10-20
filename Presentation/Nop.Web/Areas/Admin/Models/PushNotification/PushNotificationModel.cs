

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.PushNotification
{
    public partial class PushNotificationModel : BaseNopEntityModel, ILocalizedModel<PushNotificationLocalizedModel>
    {
        public PushNotificationModel()
        {
            Locales = new List<PushNotificationLocalizedModel>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.Message")]
        public string Message { get; set; }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.SentOnUtc")]
        public DateTime? SentOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.CreatedOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.Priority")]
        public string Priority { get; set; }
        public int PriorityId { get; set; }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.Status")]
        public string Status { get; set; }
        public int StatusId { get; set; }
        public IList<PushNotificationLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.List.Stores")]
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
    public partial class PushNotificationLocalizedModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Promotions.PushNotification.Fields.Message")]
        public string Message { get; set; }
    }
}