using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.TradeLicense
{
    public class TradeLicenseResponse
    {
        public Getlicensedetailsresponse GetLicenseDetailsResponse { get; set; }
    }

    public class Getlicensedetailsresponse
    {
        public string TradeLicenseNumber { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string MobileNo { get; set; }
        public string NameArb { get; set; }
        public string NameEng { get; set; }
        public string Status { get; set; }
        public string ErrorMessageAr { get; set; }
        public string ErrorMessageEn { get; set; }
    }
    public class TradeLicenseRequest
    {
        public LicenseDetailsRequest GetLicenseDetailsRequest { get; set; }
    }

    public class LicenseDetailsRequest
    {
        public string LicenseNumber { get; set; }
    }
}
