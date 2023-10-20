using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Infrastructure
{
    public class ValidatePermissionFilter : ActionFilterAttribute
    {
        public ValidatePermissionFilter()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();

            var headers = context.HttpContext.Request.Headers;


            //username
            string usernameOrEmailFound = headers["usernameOrEmail"];

            //password
            string userPasswordFound = headers["userPassword"];

            string userLanguge = headers["workinglanguage"];

            try
            {
                bool allowed = CheckAccess(usernameOrEmailFound, userPasswordFound, workContext, userLanguge);

                if (!allowed)
                {
                    context.Result = new ObjectResult(context.ModelState)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
            catch (Exception ex)
            {
                context.Result = new ObjectResult(context.ModelState)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

        }


        protected bool CheckAccess(string usernameOrEmail, string userPassword, IWorkContext workContext,string userLanguge)
        {
            IAuthenticationService authenticationService = EngineContext.Current.Resolve<IAuthenticationService>();
            ILanguageService languageService = EngineContext.Current.Resolve<ILanguageService>();
            IStoreContext storeContext = EngineContext.Current.Resolve<IStoreContext>();
            IGenericAttributeService genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            ICustomerService customerService = EngineContext.Current.Resolve<ICustomerService>();
            ICustomerRegistrationService customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            CustomerSettings customerSettings = EngineContext.Current.Resolve<CustomerSettings>();

            if (!string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                Customer customer = null;
                if (!string.IsNullOrWhiteSpace(userPassword))
                {
                    if (customerRegistrationService.ValidateCustomer(usernameOrEmail, userPassword) != CustomerLoginResults.Successful)
                        throw new ApplicationException("Not allowed");

                    customer = (customerSettings.UsernamesEnabled ? customerService.GetCustomerByUsername(usernameOrEmail) : customerService.GetCustomerByEmail(usernameOrEmail));
                }
                else
                {
                    customer = customerService.GetCustomerByGuid(Guid.Parse(usernameOrEmail));
                    if (customer == null || customer.Deleted || !customer.Active)
                        throw new ApplicationException("Not allowed");
                }
                workContext.CurrentCustomer = customer;
                authenticationService.SignIn(customer, true);
                userLanguge = string.IsNullOrEmpty(userLanguge) ? "ae" : userLanguge;
                var lang=languageService.GetAllLanguages().Where(x=>x.UniqueSeoCode==userLanguge).SingleOrDefault();
                int RequestLangID = lang!=null? lang.Id:0;
                if (RequestLangID > 0)
                {
                    genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LanguageIdAttribute, RequestLangID, storeContext.CurrentStore.Id);
                }
                else
                {
                    genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LanguageIdAttribute, 2, storeContext.CurrentStore.Id);
                }
               
                var langId = genericAttributeService.GetAttribute<Customer, int>(workContext.CurrentCustomer.Id, NopCustomerDefaults.LanguageIdAttribute, storeContext.CurrentStore.Id);
                Language currentLanguage = languageService.GetLanguageById(langId);
                workContext.WorkingLanguage = currentLanguage;
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
