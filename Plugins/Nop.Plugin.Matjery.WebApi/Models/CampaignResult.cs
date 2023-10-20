
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class CampaignResult : BaseNopEntityModel
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string PictureUrl { get; set; }
        public string PictureMobileUrl { get; set; }
        public string Body { get; set; }
        public decimal MinDiscountPercentage { get; set; }
        public int DisplayAreaId { get; set; }
        public bool Active { get; set; }
    }
}