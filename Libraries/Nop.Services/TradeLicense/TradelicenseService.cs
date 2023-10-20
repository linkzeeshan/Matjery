using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.TradeLicense;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.TradeLicense
{
    public class TradelicenseService : ITradelicenseService
    {
        private readonly CommonSettings _commonSettings;
        public TradelicenseService(CommonSettings commonSettings)
        {
            _commonSettings = commonSettings;
        }


        public async Task<TradeLicenseResponse> GetTradeLicenseResponse(string licenseNumber)
        {

            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri(_commonSettings.TradeLicenseApi)
            };

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "GetDEDLicenceDetails");
            httpRequestMessage.Headers.Add("client_id", _commonSettings.ClientId);
            httpRequestMessage.Headers.Add("client_secret", _commonSettings.ClientSecret);

            TradeLicenseRequest tradeLicense = new TradeLicenseRequest
            {
                GetLicenseDetailsRequest = new LicenseDetailsRequest() { LicenseNumber = licenseNumber }
            };

            var jsonData = JsonConvert.SerializeObject(tradeLicense);

            httpRequestMessage.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var result =await httpClient.SendAsync(httpRequestMessage);

            result.EnsureSuccessStatusCode();

            var jsonStr =await result.Content.ReadAsStringAsync();

            return await Task.FromResult(JsonConvert.DeserializeObject<TradeLicenseResponse>(jsonStr));
        }
    }
}
