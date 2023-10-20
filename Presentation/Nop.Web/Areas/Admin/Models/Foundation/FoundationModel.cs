using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Foundation
{
    public partial class FoundationModel : BaseNopEntityModel, ILocalizedModel<FoundationLocalizedModel>
    {
        public FoundationModel()
        {
            Locales = new List<FoundationLocalizedModel>();
        }
        public IList<FoundationLocalizedModel> Locales { get; set; }
        [NopResourceDisplayName("Admin.Foundations.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.Foundations.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Foundations.Fields.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Admin.Foundations.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Foundations.Fields.Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Admin.Foundations.Fields.Deleted")]
        public bool Deleted { get; set; }
        
    }
    public partial class FoundationLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Foundations.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Foundations.Fields.Description")]
        public string Description { get; set; }
    }
}