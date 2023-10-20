using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Customer
{

    public class UaePassAccessToken
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }
    public class UaePassLinkingModel
    {
        public string idn { get; set; }
        public string access_token { get; set; }
        public string uuid { get; set; }
        public string userType { get; set; }
        public string nationality { get; set; }
    }
    public class UaePassAuthenticationType
    {
        public string Type { get; set; }
        public string RegistrationType { get; set; }
    }

    public class UaePassRegistrationTempData
    {
        public string type { get; set; }
        public string flag { get; set; }
        public string ignoreCode { get; set; }
    }
}
