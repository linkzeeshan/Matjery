using Nop.Core.Domain.Catalog;
using Nop.Plugin.Matjery.WebApi.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Matjery.WebApi.Extensions
{
    public static class HelperExtensions
    {

        public static DateTime? ToDateTimeNullable(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }
            var cn = new CultureInfo("en-GB");
            DateTime result;
            if (DateTime.TryParse(s, cn, DateTimeStyles.None, out result))
            {
                return result;
            }
            else if (DateTime.TryParse(s, new CultureInfo("en-US"), DateTimeStyles.None, out result))
            {
                return result;
            }
            return null;
        }

        public static async Task<string> ToBase64String(this string url)
        {
            using (var webClient = new WebClient())
            {
                byte[] data = await webClient.DownloadDataTaskAsync(url);
                using (MemoryStream mem = new MemoryStream(data))
                {
                    using (var image = Image.FromStream(mem))
                    {
                        image.Save(mem, image.RawFormat);
                        byte[] imageBytes = mem.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        return base64String;
                    }
                }

            }
        }
        public static string TruncateToShort(this string textToTruncate, int maxChars = 170)
        {
            if (!string.IsNullOrEmpty(textToTruncate))
            {
                return textToTruncate.Length <= maxChars
                    ? textToTruncate
                    : textToTruncate.Substring(0, maxChars) + "...";
            }
            return "";
        }
       
    }
}
