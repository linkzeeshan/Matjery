using System.Collections.Generic;
using System.Net;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class RegistrationResult
	{
		public IList<string> Messages { get; set; }

		public HttpStatusCode Status { get; set; }
	    public CustomerInfoResult Customer { get; set; }

	    public RegistrationResult()
		{
			this.Messages = new List<string>();
		}
	}
}