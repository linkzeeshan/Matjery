using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Sms
{
    public partial class SmsTemplateModel : BaseNopEntityModel, ILocalizedModel<SmsTemplateLocalizedModel>
    {
        public SmsTemplateModel()
        {
            Locales = new List<SmsTemplateLocalizedModel>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.ContentManagement.SmsTemplates.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.SmsTemplates.Fields.Message")]
        public string Message { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.SmsTemplates.Fields.IsActive")]
        public bool IsActive { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.ContentManagement.SmsTemplates.Fields.LimitedToStores")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.SmsTemplates.Fields.LimitedToStores")]
        public string ListOfStores { get; set; }

        public IList<SmsTemplateLocalizedModel> Locales { get; set; }
    }

    public partial class SmsTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.SmsTemplates.Fields.Message")]
        public string Message { get; set; }
    }
}