
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.BlackPoint
{
    public partial class BlackPointModel : BaseNopEntityModel, ILocalizedModel<BlackPointLocalizedModel>
    {
        public BlackPointModel()
        {
            Locales = new List<BlackPointLocalizedModel>();
        }
        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Customer")]
        public string AddedBy { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.BlackPointType")]
        public string BlackPointType { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.BlackPointType")]
        public int BlackPointTypeId { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.BlackPointStatus")]
        public string PointStatus { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.BlackPointStatus")]
        public int PointStatusId { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Vendor")]
        public string AddedOn { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Note")]
        public string Note { get; set; }

        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Comment")]
        public DateTime CreatedOn { get; set; }
        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.BlackPointCount")]
        public int BlackPointCount { get; set; }

        public IList<BlackPointLocalizedModel> Locales { get; set; }

        /// <summary>
        /// Gets or sets the OrderId
        /// </summary>
        public int? OrderId { get; set; }
    }

    public partial class BlackPointLocalizedModel 
    {
        public int LanguageId { get; set; }
        [NopResourceDisplayName("Admin.Promotions.BlackPoint.Fields.Note")]
        public string Note { get; set; }
    }

}