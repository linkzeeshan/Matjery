using System.Net;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class LoginResult
	{
		public string Message { get; set; }
		public HttpStatusCode Status { get; set; }
	    public CustomerInfoResult Customer { get; set; }
	}
}