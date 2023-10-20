using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ProductDetailsResult
    {
        public AddToCartModel AddToCart
        {
            get;
            set;
        }

        public IList<ProductDetailsResult> AssociatedProducts
        {
            get;
            set;
        }

        public bool CompareProductsEnabled
        {
            get;
            set;
        }

        public PictureModel DefaultPictureModel
        {
            get;
            set;
        }

        public bool DefaultPictureZoomEnabled
        {
            get;
            set;
        }

        public string DeliveryDate
        {
            get;
            set;
        }

        public bool DisplayBackInStockSubscription
        {
            get;
            set;
        }

        public bool DisplayDiscontinuedMessage
        {
            get;
            set;
        }

        public bool EmailAFriendEnabled
        {
            get;
            set;
        }

        public bool FreeShippingNotificationEnabled
        {
            get;
            set;
        }

        public string FullDescription
        {
            get;
            set;
        }

        public GiftCardModel GiftCard
        {
            get;
            set;
        }

        public string Gtin
        {
            get;
            set;
        }

        public bool HasSampleDownload
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public bool IsFreeShipping
        {
            get;
            set;
        }

        public bool IsRental
        {
            get;
            set;
        }

        public bool IsShipEnabled
        {
            get;
            set;
        }

        public string MetaDescription
        {
            get;
            set;
        }

        public string MetaKeywords
        {
            get;
            set;
        }

        public string MetaTitle
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string PageShareCode
        {
            get;
            set;
        }

        public IList<PictureModel> PictureModels
        {
            get;
            set;
        }

        public IList<ProductAttributeModel> ProductAttributes
        {
            get;
            set;
        }

        public ProductPriceModel ProductPrice
        {
            get;
            set;
        }

        public ProductReviewOverviewModel ProductReviewOverview
        {
            get;
            set;
        }

        public IList<ProductSpecificationModel> ProductSpecifications
        {
            get;
            set;
        }

        public IList<ProductTagModel> ProductTags
        {
            get;
            set;
        }

        public DateTime? RentalEndDate
        {
            get;
            set;
        }

        public DateTime? RentalStartDate
        {
            get;
            set;
        }

        public string SeName
        {
            get;
            set;
        }

        public string ShortDescription
        {
            get;
            set;
        }

        public bool ShowGtin
        {
            get;
            set;
        }

        public bool ShowSku
        {
            get;
            set;
        }

        public bool ShowVendor
        {
            get;
            set;
        }

        public string Sku
        {
            get;
            set;
        }

        public string StockAvailability
        {
            get;
            set;
        }

        public IList<TierPriceModel> TierPrices
        {
            get;
            set;
        }

        public VendorResult VendorModel { get; set; }
        public string ProductUrl { get; set; }

        public ProductDetailsResult()
        {
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            GiftCard = new GiftCardModel();
            ProductPrice = new ProductPriceModel();
            AddToCart = new AddToCartModel();
            ProductAttributes = new List<ProductAttributeModel>();
            AssociatedProducts = new List<ProductDetailsResult>();
            VendorModel = new VendorResult();
            ProductTags = new List<ProductTagModel>();
            ProductSpecifications = new List<ProductSpecificationModel>();
            ProductReviewOverview = new ProductReviewOverviewModel();
            TierPrices = new List<TierPriceModel>();
        }

        public class AddToCartModel
        {
            public List<SelectListItem> AllowedQuantities
            {
                get;
                set;
            }

            public bool AvailableForPreOrder
            {
                get;
                set;
            }

            public decimal CustomerEnteredPrice
            {
                get;
                set;
            }

            public string CustomerEnteredPriceRange
            {
                get;
                set;
            }

            public bool CustomerEntersPrice
            {
                get;
                set;
            }

            public bool DisableBuyButton
            {
                get;
                set;
            }

            public bool DisableWishlistButton
            {
                get;
                set;
            }

            public int EnteredQuantity
            {
                get;
                set;
            }

            public bool IsRental
            {
                get;
                set;
            }

            public string MinimumQuantityNotification
            {
                get;
                set;
            }

            public DateTime? PreOrderAvailabilityStartDateTimeUtc
            {
                get;
                set;
            }

            public int ProductId
            {
                get;
                set;
            }

            public int UpdatedShoppingCartItemId
            {
                get;
                set;
            }

            public AddToCartModel()
            {
                AllowedQuantities = new List<SelectListItem>();
            }
        }

        public class GiftCardModel
        {
            public GiftCardType GiftCardType
            {
                get;
                set;
            }

            public bool IsGiftCard
            {
                get;
                set;
            }

            public string Message
            {
                get;
                set;
            }

            public string RecipientEmail
            {
                get;
                set;
            }

            public string RecipientName
            {
                get;
                set;
            }

            public string SenderEmail
            {
                get;
                set;
            }

            public string SenderName
            {
                get;
                set;
            }

            public GiftCardModel()
            {
            }
        }

        public class PictureModel
        {
            public string AlternateText
            {
                get;
                set;
            }

            public string FullSizeImageUrl
            {
                get;
                set;
            }

            public string ImageUrl
            {
                get;
                set;
            }

            public string Title
            {
                get;
                set;
            }

            public string WaterMarkImageUrl { get; set; }

            public PictureModel()
            {
            }
        }

        public class ProductAttributeModel
        {
            public IList<string> AllowedFileExtensions
            {
                get;
                set;
            }

            public AttributeControlType AttributeControlType
            {
                get;
                set;
            }

            public string DefaultValue
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public bool HasCondition
            {
                get;
                set;
            }

            public int Id
            {
                get;
                set;
            }

            public bool IsRequired
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public int ProductAttributeId
            {
                get;
                set;
            }

            public int ProductId
            {
                get;
                set;
            }

            public int? SelectedDay
            {
                get;
                set;
            }

            public int? SelectedMonth
            {
                get;
                set;
            }

            public int? SelectedYear
            {
                get;
                set;
            }

            public string TextPrompt
            {
                get;
                set;
            }

            public IList<ProductAttributeValueModel> Values
            {
                get;
                set;
            }

            public ProductAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<ProductAttributeValueModel>();
            }
        }

        public class ProductAttributeValueModel
        {
            public string ColorSquaresRgb
            {
                get;
                set;
            }

            public int Id
            {
                get;
                set;
            }

            public bool IsPreSelected
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public PictureModel PictureModel
            {
                get;
                set;
            }

            public string PriceAdjustment
            {
                get;
                set;
            }

            public decimal PriceAdjustmentValue
            {
                get;
                set;
            }

            public ProductAttributeValueModel()
            {
                PictureModel = new PictureModel();
            }
        }

        public class ProductPriceModel
        {
            public string BasePricePAngV
            {
                get;
                set;
            }

            public bool CallForPrice
            {
                get;
                set;
            }

            public string CurrencyCode
            {
                get;
                set;
            }

            public bool CustomerEntersPrice
            {
                get;
                set;
            }

            public bool DisplayTaxShippingInfo
            {
                get;
                set;
            }

            public bool HidePrices
            {
                get;
                set;
            }

            public bool IsRental
            {
                get;
                set;
            }

            public string OldPrice
            {
                get;
                set;
            }

            public string Price
            {
                get;
                set;
            }

            public decimal PriceValue
            {
                get;
                set;
            }

            public string PriceWithDiscount
            {
                get;
                set;
            }

            public int ProductId
            {
                get;
                set;
            }

            public string RentalPrice
            {
                get;
                set;
            }
            public decimal CachedOldPriceValue { get; set; }
            public decimal CachedPriceValue { get; set; }
            public decimal CachedDiscountPriceValue { get; set; }

            public ProductPriceModel()
            {
            }
        }

        public class ProductReviewOverviewModel
        {
            public bool AllowCustomerReviews
            {
                get;
                set;
            }

            public int ProductId
            {
                get;
                set;
            }

            public int RatingSum
            {
                get;
                set;
            }

            public int TotalReviews
            {
                get;
                set;
            }
            public bool CanCurrentCustomerReview { get; set; }

            public ProductReviewOverviewModel()
            {
            }
        }

        public class ProductSpecificationModel
        {
            public int SpecificationAttributeId
            {
                get;
                set;
            }

            public string SpecificationAttributeName
            {
                get;
                set;
            }

            public string ValueRaw
            {
                get;
                set;
            }

            public string ColorSquaresRgb { get; set; }

            public ProductSpecificationModel()
            {
            }
        }

        public class ProductTagModel
        {
            public int Id
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public int ProductCount
            {
                get;
                set;
            }

            public string SeName
            {
                get;
                set;
            }

            public ProductTagModel()
            {
            }
        }

        public class TierPriceModel
        {
            public string Price
            {
                get;
                set;
            }

            public int Quantity
            {
                get;
                set;
            }

            public TierPriceModel()
            {
            }
        }

        public class VendorBriefInfoModel
        {
            public int Id
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public string SeName
            {
                get;
                set;
            }

            public VendorBriefInfoModel()
            {
            }
        }
    }
}