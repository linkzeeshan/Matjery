using Nop.Core.Domain.TradeLicense;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface ITradeLicensePluginService
    {
        Task<TradeLicenseResponse> GetTradeLicenseResponse(string licenseNumber);
    }
}
