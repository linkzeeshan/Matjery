using System;
using System.Collections.Generic;
using System.Net;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class CustomerInfoResult
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }

        public string AvatarUrl { get; set; }

        public string FullName { get; internal set; }

        public string FormattedUserName { get; internal set; }
        public string Gender { get; internal set; }
        public DateTime? DateOfBirth { get; internal set; }
        public string Password { get; set; }
        public string PhoneNumber { get; internal set; }
        public bool SmsValidated { get; set; }
        public bool IsVendor { get; internal set; }
        public int Idpk { get; set; }
        public int VendorId { get; set; }
        public int NationalityCountryId { get; set; }
        public bool IsAdmin { get; internal set; }
        public bool IsFoundation { get; internal set; }
        public bool IsTranslator { get; internal set; }
        public bool HasAcceptedTermsAndConditions { get; set; }
        public string ProviderSystemName { get; set; }
        public bool IsRequestedforDelete { get; set; }


        public class CustomerInfoEditResult
        {
            public IList<string> Messages { get; set; }
            public HttpStatusCode Status { get; set; }
            public CustomerInfoResult Customer { get; set; }

            public CustomerInfoEditResult()
            {
                this.Messages = new List<string>();
            }
        }
    }
}