using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ExternalAuthRecordResult
    {
        public HttpStatusCode Status { get; set; }
        public ExternalAuthenticationRecord ExternalAuthenticationRecord { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public bool isUserExist { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public ExternalAuthRecordResult()
        {
            this.ExternalAuthenticationRecord = new ExternalAuthenticationRecord();
        }
    }
}
