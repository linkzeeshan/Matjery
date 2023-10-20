using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents a campaign model
    /// </summary>
    public partial class CampaignModel : BaseNopEntityModel, ILocalizedModel<CampaignLocalizedModel>
    {
        #region Ctor

        public CampaignModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCustomerRoles = new List<SelectListItem>();
            AvailableEmailAccounts = new List<SelectListItem>();
            Locales = new List<CampaignLocalizedModel>();
            CampaignProductSearchModel = new CampaignProductSearchModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Subject")]
        public string Subject { get; set; }

        [UIHint("PictureString")]
        [NopResourceDisplayName("Admin.Catalog.Campaigns.Fields.PictureString")]
        public string PictureId { get; set; }

        [UIHint("PictureString")]
        [NopResourceDisplayName("Admin.Catalog.Campaigns.Fields.PictureMobile")]
        public string PictureIdMobile { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Body")]
        public string Body { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Store")]
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CustomerRole")]
        public int CustomerRoleId { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.DontSendBeforeDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? DontSendBeforeDate { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.EmailAccount")]
        public int EmailAccountId { get; set; }
        public IList<SelectListItem> AvailableEmailAccounts { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.TestEmail")]
        public string TestEmail { get; set; }
        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.DiscountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.MinDiscountPercentage")]
        public int MinDiscountPercentage { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.DisplayAreaId")]
        public int DisplayAreaId { get; set; }

        public IList<CampaignLocalizedModel> Locales { get; set; }
        
        public CampaignProductSearchModel CampaignProductSearchModel { get; set; }
        #region Nested

        public partial class CampaignProductModel : BaseNopEntityModel
        {
            public int CampaignId { get; set; }

            public int ProductId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Campaigns.Products.Fields.Product")]
            public string ProductName { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Campaigns.Products.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public string PictureThumbnailUrl { get; set; }
        }

       
        #endregion

        #endregion

    }

    public partial class CampaignLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Campaigns.Fields.Name")]
        public string Name { get; set; }

        [UIHint("PictureString")]
        [NopResourceDisplayName("Admin.Catalog.Campaigns.Fields.PictureString")]
        public string PictureId { get; set; }

        [UIHint("PictureString")]
        [NopResourceDisplayName("Admin.Catalog.Campaigns.Fields.PictureMobile")]
        public string PictureIdMobile { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Campaigns.Fields.Subject")]
        public string Subject { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Campaigns.Fields.Body")]
        public string Body { get; set; }
    }

    public partial class AddCampaignProductModel : BaseNopModel
    {

        public int CampaignId { get; set; }
        public IList<int> SelectedProductIds { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}