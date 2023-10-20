using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Catalog
{
    public class CampaignModel : BaseNopEntityModel
    {
        public CampaignModel()
        {
            this.PictureModel = new PictureModel();
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();

        }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string PictureId { get; set; }
        public string PictureIdMobile { get; set; }
        public string Body { get; set; }
        public decimal MinDiscountPercentage { get; set; }
        public int DisplayAreaId { get; set; }
        public bool Active { get; set; }

        public PictureModel PictureModel { get; set; }

        public IList<ProductOverviewModel> Products { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

    }
}
