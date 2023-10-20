using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Foundation
{
    public class FoundationInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Foundations.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.Foundations.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Foundations.Fields.Description")]
        public string Description { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Foundations.Fields.Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Admin.Foundations.Fields.Deleted")]
        public bool Deleted { get; set; }
    }
}
