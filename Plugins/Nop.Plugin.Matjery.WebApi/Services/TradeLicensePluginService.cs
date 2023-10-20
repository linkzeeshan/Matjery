using Nop.Core.Domain.TradeLicense;
using Nop.Plugin.Matjery.WebApi.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class TradeLicensePluginService : BasePluginService, ITradeLicensePluginService
    {
        public async Task<TradeLicenseResponse> GetTradeLicenseResponse(string licenseNumber)
        {
            return await _tradeLicenseService.GetTradeLicenseResponse(licenseNumber);
        }
    }
}
