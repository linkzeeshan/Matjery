using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Vendors
{
   
    public enum VendorLicenseStatus
    {
        Active = 1,
        Expired = 2,
        AboutToExpire=3,
        Nolicense=0
    }
}
