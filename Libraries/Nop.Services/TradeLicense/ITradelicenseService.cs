using Nop.Core.Domain.TradeLicense;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.TradeLicense
{
    public interface ITradelicenseService
    {
        Task<TradeLicenseResponse> GetTradeLicenseResponse(string licenseNumber);
    }
}
