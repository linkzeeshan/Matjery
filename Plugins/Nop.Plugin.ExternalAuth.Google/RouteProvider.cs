using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.ExternalAuth.Google
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    //public class RouteProvider : IRouteProvider
    //{
    //    /// <summary>
    //    /// Register routes
    //    /// </summary>
    //    /// <param name="endpointRouteBuilder">Route builder</param>
    //    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    //    {
    //        //override some of default routes in Admin area
    //        endpointRouteBuilder.MapControllerRoute("Plugin.ExternalAuth.Google.Login", "Plugins/GoogleAuthentication/Login",
    //            new { controller = "GoogleAuthentication", action = "Login" });


    //        endpointRouteBuilder.MapControllerRoute("Plugin.ExternalAuth.Google.LoginCallback", "Plugins/GoogleAuthentication/LoginCallback",
    //                new { controller = "GoogleAuthentication", action = "LoginCallback" });

    //    }

    //    /// <summary>
    //    /// Gets a priority of route provider
    //    /// </summary>
    //    public int Priority => 1; //set a value that is greater than the default one in Nop.Web to override routes
    //}
}