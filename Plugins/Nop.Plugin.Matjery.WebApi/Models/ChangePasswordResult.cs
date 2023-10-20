using System.Collections.Generic;
using System.Net;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ChangePasswordResult
    {
        public IList<string> Errors { get; set; }
        public HttpStatusCode Status { get; set; }
        public string Message { get; internal set; }

        public ChangePasswordResult()
        {
            this.Errors = new List<string>();
        }

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }
}