using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Sms
{
    public class SmartSmsGatewayResponse
    {
        public SmartSmsGatewayResponse()
        {
            this.Data = new SmartSmsGatewayResponseData();
            this.Errors = new List<SmartSmsGatewayResponseErrors>();
        }

        public SmartSmsGatewayResponseData Data { get; set; }
        public IList<SmartSmsGatewayResponseErrors> Errors { get; set; }
    }

    public class SmartSmsGatewayResponseData
    {
        public string Id { get; set; }
        public string Status { get; set; }
    }

    public class SmartSmsGatewayResponseErrors
    {
        public int Code { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
    }
}
