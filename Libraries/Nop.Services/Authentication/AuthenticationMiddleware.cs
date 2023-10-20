using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication.External;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Services.Authentication
{
    /// <summary>
    /// Represents middleware that enables authentication
    /// </summary>
    public class AuthenticationMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Ctor

        public AuthenticationMiddleware(IAuthenticationSchemeProvider schemes, RequestDelegate next)
        {
            Schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public IAuthenticationSchemeProvider Schemes { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke middleware actions
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task Invoke(HttpContext context)
        {
            var mobile = context.Request.Query["mobile"].ToString();
            if (mobile != null
                && context.Request.Query["usernameOrEmail"].ToString() != null
                && context.Request.Query["password"].ToString() != null)
            {
                bool isMobile = Convert.ToBoolean(context.Request.Query["mobile"]);
                if (isMobile)
                {
                    string usernameOrEmail = context.Request.Query["usernameOrEmail"];
                    string password = context.Request.Query["password"];

                    var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
                    var _openAuthenticationService = EngineContext.Current.Resolve<IOpenAuthenticationService>();
                    var _customerService = EngineContext.Current.Resolve<ICustomerService>();

                    var loginResult = customerRegistrationService.ValidateCustomer(usernameOrEmail, password);

                    string provider = context.Request.Query["provider"];
                    string uuid = context.Request.Query["uuid"];
                    if (provider == "uaepass")
                    {
                        //get user by uuid
                        var userIdFromExternalAccount = _openAuthenticationService.GetUserIdFromUaePassLinkedUser(uuid, Provider.SystemName);
                        if (userIdFromExternalAccount > 0)
                        {
                            var customer = _customerService.GetCustomerById(userIdFromExternalAccount);
                            loginResult = customerRegistrationService.ValidateCustomerByUUID(customer.Id, false);
                        }
                    }
                    switch (loginResult)
                    {
                        case CustomerLoginResults.Successful:
                            {
                                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                                var authenticationService = EngineContext.Current.Resolve<IAuthenticationService>();
                                var customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
                                var customerService = EngineContext.Current.Resolve<ICustomerService>();
                                var _eventPublisher = EngineContext.Current.Resolve<IEventPublisher>(); ;
                                var customer = customerSettings.UsernamesEnabled
                                    ? customerService.GetCustomerByUsername(usernameOrEmail)
                                    : customerService.GetCustomerByEmail(usernameOrEmail);
                                if (provider == "uaepass")
                                {
                                    //get user by uuid
                                    var userIdFromExternalAccount = _openAuthenticationService.GetUserIdFromUaePassLinkedUser(uuid, Provider.SystemName);
                                    if (userIdFromExternalAccount > 0)
                                    {
                                        customer = _customerService.GetCustomerById(userIdFromExternalAccount);
                                    }
                                }


                                //sign in new customer
                                authenticationService.SignIn(customer, true);

                                //create external authentication parameters


                                //authenticate Nop user
                                // return _externalAuthenticationService.Authenticate(authenticationParameters, returnUrl);
                                workContext.CurrentCustomer = customer;
                                //workContext.CurrentCustomer must be called immediatly after signin here in order to properly set current customer, otherwise the user won't be set
                                var fixForSettingCurrentCustomerProperly = workContext.CurrentCustomer;
                                //workContext. = true;


                            }
                            break;
                    }
                }
            }


            // SetMobileLogin(context);
            //context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
            //{
            //    OriginalPath = context.Request.Path,
            //    OriginalPathBase = context.Request.PathBase
            //});

            // Give any IAuthenticationRequestHandler schemes a chance to handle the request
            var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
            {
                try
                {
                    if (await handlers.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
                        return;
                }
                catch
                {
                    // ignored
                }
            }

            //var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            //if (defaultAuthenticate != null)
            //{
            //    var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
            //    if (result?.Principal != null)
            //    {
            //        context.User = result.Principal;
            //    }
            //}
            // Redirect to login if user is not authenticated. This instruction is neccessary for JS async calls, otherwise everycall will return unauthorized without explaining why

            await _next(context);
        }
        public void  SetMobileLogin(HttpContext context)
        {
           // var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
            var mobile = context.Request.Query["mobile"].ToString();
            if (mobile != null
                && context.Request.Query["usernameOrEmail"].ToString() != null
                && context.Request.Query["password"].ToString() != null)
            {
                bool isMobile = Convert.ToBoolean(context.Request.Query["mobile"]);
                if (isMobile)
                {
                    string usernameOrEmail = context.Request.Query["usernameOrEmail"];
                    string password = context.Request.Query["password"];

                    var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
                    var _openAuthenticationService = EngineContext.Current.Resolve<IOpenAuthenticationService>();
                    var _customerService = EngineContext.Current.Resolve<ICustomerService>();

                    var loginResult = customerRegistrationService.ValidateCustomer(usernameOrEmail, password);

                    string provider = context.Request.Query["provider"];
                    string uuid = context.Request.Query["uuid"];
                    if (provider == "uaepass")
                    {
                        //get user by uuid
                        var userIdFromExternalAccount = _openAuthenticationService.GetUserIdFromUaePassLinkedUser(uuid, Provider.SystemName);
                        if (userIdFromExternalAccount > 0)
                        {
                            var customer = _customerService.GetCustomerById(userIdFromExternalAccount);
                            loginResult = customerRegistrationService.ValidateCustomerByUUID(customer.Id, false);
                        }
                    }
                    switch (loginResult)
                    {
                        case CustomerLoginResults.Successful:
                            {
                                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                                var authenticationService = EngineContext.Current.Resolve<IAuthenticationService>();
                                var customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
                                var customerService = EngineContext.Current.Resolve<ICustomerService>();
                                var _eventPublisher = EngineContext.Current.Resolve<IEventPublisher>(); ;
                                var customer = customerSettings.UsernamesEnabled
                                    ? customerService.GetCustomerByUsername(usernameOrEmail)
                                    : customerService.GetCustomerByEmail(usernameOrEmail);
                                if (provider == "uaepass")
                                {
                                    //get user by uuid
                                    var userIdFromExternalAccount = _openAuthenticationService.GetUserIdFromUaePassLinkedUser(uuid, Provider.SystemName);
                                    if (userIdFromExternalAccount > 0)
                                    {
                                        customer = _customerService.GetCustomerById(userIdFromExternalAccount);
                                    }
                                }


                                //sign in new customer
                                authenticationService.SignIn(customer, true);

                                //create external authentication parameters


                                //authenticate Nop user
                               // return _externalAuthenticationService.Authenticate(authenticationParameters, returnUrl);
                                workContext.CurrentCustomer = customer;
                                //workContext.CurrentCustomer must be called immediatly after signin here in order to properly set current customer, otherwise the user won't be set
                                var fixForSettingCurrentCustomerProperly = workContext.CurrentCustomer;
                                //workContext. = true;


                            }
                            break;
                    }
                }
            }
          
        
        }


        #endregion
    }
}