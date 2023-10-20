using Nop.Web.Framework.Models;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ProductResult : BaseNopModel
    {
        public string categorySeName { get; set; }

        public bool Available { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string FullDescription { get; set; }

        public int Id { get; set; }

        public int ManufacturerId { get; set; }

        public string ManufacturerName { get; set; }

        public string Name { get; set; }

        public string OldPrice { get; set; }

        public string Price { get; set; }

        public string ShortDescription { get; set; }

        public string Sku { get; set; }

        public string ImageUrl { get; set; }

        public string ProductUrl { get; set; }

        public decimal Vat { get; set; }

        public bool MarkAsNew { get; set; }
        public bool IsInWishlist { get; set; }
        public bool Featured { get; set; }
        public string DiscountPrice { get; set; }
        public string PriceWithoutDiscount { get; set; }
        public bool IsAddedToWishlist { get; set; }
        public decimal CachedPriceValue { get; set; }
        public decimal CachedOldPriceValue { get; set; }
        public decimal CachedDiscountPriceValue { get; set; }
        public string PriceWithQuantity { get; set; }
        public string WaterMarkImageUrl { get; set; }
        public bool InStock { get; set; }
    }
}
