using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Foundation
{
    public class FoundationSearchModel: BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Foundations.Fields.Name")]
        public string SearchName { get; set; }
    }
}
