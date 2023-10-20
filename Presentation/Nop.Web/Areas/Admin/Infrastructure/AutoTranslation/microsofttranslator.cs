using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Infrastructure.AutoTranslation
{
    public class Microsofttranslator
    {
        private readonly CommonSettings _commonSettings;

        public static async Task<HttpResponseMessage> Translate(string textToTranslate)
        {
            string Res = "";
            var _commonSettings = EngineContext.Current.Resolve<CommonSettings>();
            string key = _commonSettings.Autotranslation_key;
            string endpoint = _commonSettings.Autotranslation_endpoint;
            string location = _commonSettings.Autotranslation_location;
            string route = _commonSettings.Autotranslation_route;
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(endpoint + route); request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                    // location required if you're using a multi-service or regional (not global) resource.     
                    request.Headers.Add("Ocp-Apim-Subscription-Region", location);
                    // Send the request and get response.         
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    return response;

                }
            }
            return null;
        }
    }
}
