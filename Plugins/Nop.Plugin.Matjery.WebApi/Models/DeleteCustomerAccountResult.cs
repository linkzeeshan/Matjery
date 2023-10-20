using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class DeleteCustomerAccountResult
    {
        public IList<string> Errors { get; set; }
        public HttpStatusCode Status { get; set; }
        public string Message { get; internal set; }

        public DeleteCustomerAccountResult()
        {
            this.Errors = new List<string>();
            
    }

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }
}

