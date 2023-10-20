

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    public partial class PushNotificationTemplateModel : BaseNopEntityModel, ILocalizedModel<PushNotificationTemplateLocalizedModel>
    {
        public PushNotificationTemplateModel()
        {
            Locales = new List<PushNotificationTemplateLocalizedModel>();
        }


        [NopResourceDisplayName("Admin.ContentManagement.PushNotificationTemplate.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.PushNotificationTemplate.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.PushNotificationTemplate.Fields.Message")]
        public string Message { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.PushNotificationTemplate.Fields.IsActive")]
        public bool IsActive { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.PushNotificationTemplate.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; } = true;
        public int Total { get; set; }
        public IList<PushNotificationTemplateLocalizedModel> Locales { get; set; }
    }

    public partial class PushNotificationTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public PushNotificationTemplateLocalizedModel()
        {

        }
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.PushNotificationTemplate.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.PushNotificationTemplate.Fields.Message")]
        public string Message { get; set; }
    }
}