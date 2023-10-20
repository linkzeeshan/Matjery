using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public partial class VendorApplyResult
    {
        public string Message { get; set; }
        public bool Submitted { get; set; }
        public HttpStatusCode Status { get; set; }
        public CustomerInfoResult Customer { get; set; }
    }

}
