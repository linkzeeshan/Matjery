using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Sms
{
    public partial class SmsTemplateListModel :  BasePagedListModel<SmsTemplateModel>
    {
        public SmsTemplateListModel()
        {
            //AvailableStores = new List<SelectListItem>();
        }

        //[NopResourceDisplayName("Admin.ContentManagement.SmsTemplates.List.SearchStore")]
        //public int SearchStoreId { get; set; }
        //public IList<SelectListItem> AvailableStores { get; set; }
    }
}