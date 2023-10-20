using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;
using Nop.Services.Authentication.External;
using Nop.Core.Domain.BlackPoints;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ParamsModel
    {
        public class DeviceInfoParamsModel
        {
            public string CustomerId { get; set; }
            public string DeviceId { get; set; }
            public string RegisterationId { get; set; }
        }
        public class OrderStatusModel
        {
            public int Id { get; set; }
            public string StatusName { get; set; }
        }
        public class MatjeryLookupModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public int Sequence { get; set; }
        }
        public class ExtAuthParamsModel
        {
            public string Email { get; set; }
            public string ProviderSystemName { get; set; }
        }
    
        public class VendorApplyParamsModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Description { get; set; }
            public string Logo { get; set; }
            public string PhoneNumber { get; set; }
            public string emiratesId { get; set; }
            public string facebook { get; set; }
            public string googleplus { get; set; }
            public string instagram { get; set; }
            public string twitter { get; set; }
            public string whatsApp { get; set; }
            public string bbm { get; set; }
            public VendorRegisterationType RegisterationType { get; set; }
            //added for DED
            public string[] CategoryId { get; set; }
            public string IssueDate { get; set; }
            public string ExpiryDate { get; set; }
            public string TradeLicenseNumber { get; set; }
 
        }

        public class VendorInfoParamsModel
        {
            public int Id { get; set; }
            public int PictureId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string PhoneNumber { get; set; }
            public string EmiratesId { get; set; }
            public string WhatsApp { get; set; }
            public string Instagram { get; set; }
            public string Twitter { get; set; }
            public string Facebook { get; set; }
            public string Googleplus { get; set; }
            public string BBM { get; set; }
            public bool EnrollForTraining { get; set; }
            public string[] CategoryId { get; set; }
            public string IssueDate { get; set; }
            public string ExpiryDate { get; set; }
            public string TradeLicenseNumber { get; set; }
            public string TradeLicenseFile { get; set; }
            public int LicenseId { get; set; }
           
        }

        public class ContactVendorParamsModel
        {
            public string Email { get; set; }
            public string Enquiry { get; set; }
            public int VendorId { get; set; }
            public string FullName { get; set; }
            public string Subject { get; set; }
        }

        public class VendorFollowAddParamsModel
        {
            public int VendorId { get; set; }

        }
        public class SupportedByVendorAddParamsModel
        {
            public int VendorId { get; set; }
            public int SupportedByFoundationId { get; set; }
        }
        public class ViewModes
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public ViewModes()
            {
             
            }
        }
        public class VendorReviewAddParamsModel
        {
            public string Title { get; set; }
            public string ReviewText { get; set; }
            public int VendorId { get; set; }
            public int Rating { get; set; }
        }

        public class AccountActivationParamsModel
        {
            public string UserName { get; set; }
            public string SmsToken { get; set; }
            public string CustomerId { get; set; }
        }

        public class CartParamsModel
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public int NewQuantity { get; set; }
            public List<ProductAttributes> ProductAttributeses { get; set; }
        }

        public class WishlistParams
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public List<ProductAttributes> ProductAttributeses { get; set; }
        }

        public class ProductReviewParamsModel
        {
            public int ProductId { get; set; }
            public int Rating { get; set; }
            public string Title { get; set; } = String.Empty;
            public string ReviewText { get; set; }
        }
        public class SearchParamsModel
        {

            public int? CampaignId { get; set; }
            public string SearchTerms { get; set; }
            public bool IncludeSubCategories { get; set; }
            public bool SearchInDescriptions { get; set; }
            public string PriceFrom { get; set; }
            public string PriceTo { get; set; }
            public bool AdvancedSearch { get; set; }
            public int? ManufacturerId { get; set; }
            public int?[] CategoryIds { get; set; }
            public int? VendorId { get; set; }
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public string ViewMode { get; set; }
            public int? OrderBy { get; set; }
            public bool? FeaturedProducts { get; set; }
            public bool? BestSellers { get; set; }
            public bool? DiscountedProducts { get; set; }
            public bool ValdiateLength { get; set; }
        }
        public class ChangePasswordParamsModel
        {
            public string NewPassword { get; set; }
        }

        public class BlackPointParamsModel
        {
            public int OrderId { get; set; }
            public string Message { get; set; }
        }

        public class PlaceOrderParamsModel
        {
            public string PaymentMethodName { get; set; }
            public string ShippingMethodName { get; set; }
            public int BillingAddressId { get; set; }
            public int? ShippingAddressId { get; set; }
            public bool PickUpInStore { get; set; }
            public string PickupPointValue { get; set; }
            public string CreditCardType { get; set; }
            public string CreditCardName { get; set; }
            public string CreditCardNumber { get; set; }
            public int? CreditCardExpireMonth { get; set; }
            public int? CreditCardExpireYear { get; set; }
            public string CreditCardCvv2 { get; set; }
        }

        public class AddressParamsModel
        {
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Area { get; set; }
            public string City { get; set; }
            public int? CountryId { get; set; }
            public string Email { get; set; }
            public string FaxNumber { get; set; }
            public string FirstName { get; set; }
            public int Id { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public int? StateProvinceId { get; set; }
            public bool IsDefault { get; set; }
        }
        public class AvatarParamsModel
        {
            public string Avatar { get; set; }
        }
        public class PasswordRecoveryParamsModel
        {
            public string Email { get; set; }
            public string Phone { get; set; }
        }
        public class LoginParamsModel
        {
            public string UsernameOrEmail { get; set; }
            public string UserPassword { get; set; }
            public string GuestToken { get; set; }
            public bool ignoreHashPassword { get; set; }
            public bool IsExternal { get; set; }
        }
        public class SellerParamsModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class QueuedPushNotificationParamsModel
        {
            public string RegistrationId { get; set; }
            public int? QueuedPushNotificationId { get; set; }
            public int? CustomerId { get; set; }
        }

        public class PushNotificationParamsModel
        {
            public string RegistrationId { get; set; }
            public string DeviceId { get; set; }
        }

        public class LoginExternalParamsModel : OpenAuthenticationParameters
        {
            public LoginExternalParamsModel()
            {
                UserClaims = new List<UserClaims>();
                VendorApplyParamsModel = new VendorApplyParamsModel();
            }
            public string GuestToken { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Gender { get; set; }
            public string Nationality { get; set; }
            public string DateOfBirth { get; set; }
            public string PhoneNumber { get; set; }
            public string Provider { get; set; }
            public VendorApplyParamsModel VendorApplyParamsModel { get; set; }

            public override string ProviderSystemName
            {
                get { return this.Provider; }
            }
            public override IList<UserClaims> UserClaims { get; }
        }

       
        public class RegistrationParamsModel
        {
            public RegistrationParamsModel()
            {
                VendorApplyParamsModel = new VendorApplyParamsModel();
            }
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DateOfBirth { get; set; }
            public string Username { get; set; }
            public string GuestToken { get; set; }
            public string PhoneNumber { get; set; }
            public int? CountryId { get; set; }
            public int? StateProvinceId { get; set; }
            public string CityCode { get; set; }
            public string AreaCode { get; set; }
            public string StreetNameorNo { get; set; }
            public string BuildingNameOrNo { get; set; }
            public bool HasAcceptedTermsAndConditions { get; set; }
            public string SmsToken { get; set; }
            public string RegistrationType { get; set; }
            public string VendorParamsString { get; set; }
            public VendorApplyParamsModel VendorApplyParamsModel { get; set; }
            #region uaePass
            public string Provider { get; set; }
            public string ExternalIdentifier { get; set; }
            public string OAuthAccessToken { get; set; }
            public string Idn { get; set; }
            public string Gender { get; set; }
            public string Nationality { get; set; }
            #endregion
        }
        public class CustomerInfoParamsModel
        {
            public string CustomerId { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Username { get; set; }
            public string Gender { get; set; }
            public string DateOfBirth { get; set; }
            public string PhoneNumber { get; set; }
            public int? NationalityCountryId { get; set; }
        }

        public class DeleteCustomerAccountParamsModel
        {
            public int ReasonId { get; set; }
            public string Comments { get; set; }
            public string UserType { get; set; }
        }
        public class CustomerTermsAndConditionsParamsModel
        {
            public bool HasAcceptedTermsAndConditions { get; set; }
        }

        public class UaePassParamsModel
        {
            public string email { get; set; }
            public string uuid { get; set; }
            public string idn { get; set; }
            public string userEmail { get; set; }
            public string accessToken { get; set; }
            public string RegistrationType { get; set; }
         
        }
    }
}
