using System.Net;

namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class FinishOrderResult
	{
		public int Id
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public HttpStatusCode Status
		{
			get;
			set;
		}

		public FinishOrderResult()
		{
		}
	}
}