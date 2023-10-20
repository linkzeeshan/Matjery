using System.Net;

namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class PasswordRecoveryResult
	{
		public string Message { get; set; }
		public HttpStatusCode Status { get; set; }
	}
}