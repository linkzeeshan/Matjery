using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class UAEPassPluginService : BasePluginService, IUAEPassPluginService
    {
        public object UAEPassExists(ParamsModel.UaePassParamsModel model)
        {
            object response = null;
            var uaePassModel = new UaePassResponseModel();
            if (string.IsNullOrEmpty(model.email) || string.IsNullOrEmpty(model.uuid))
            {
                response = uaePassModel;
            }

            var customer = _customerService.GetCustomerByEmail(model.email);
            var externalAccountExist = _openAuthenticationService.UaePassUserExist(email: model.email, externalIdentifier: model.uuid, providerSystemName: Provider.SystemName);

            customer = customer == null ? _customerService.GetCustomerById(externalAccountExist.CustomerId) : customer;
            if (customer != null)
            {
                if (externalAccountExist != null)
                {

                    if (!externalAccountExist.IsEmailExist)
                    {
                        externalAccountExist.IsEmailExist = true;
                    }
                }
                externalAccountExist.IsDeleted = customer.Deleted;
                externalAccountExist.IsActive = customer.Active;
                response = externalAccountExist;

            }
            if (customer == null)
            {
                externalAccountExist.IsSeller = false;
                response = externalAccountExist;
             
            }
            else if (_customerService.IsVendor(customer))
            {
                externalAccountExist.IsSeller = true;
            }
            return response;
        }

        public bool UpdateUAEPassUser(ParamsModel.UaePassParamsModel model) {

            var flag = false;
            if (string.IsNullOrEmpty(model.uuid) || string.IsNullOrEmpty(model.email))
            {
                return flag;
            }

            var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(model.userEmail)
                                : _customerService.GetCustomerByEmail(model.userEmail);

            UserClaims claim = new UserClaims();
            claim.Contact = new ContactClaims();
            claim.Contact.Email = model.email;
            var paramaters = new UaePassAuthenticationParameters(Provider.SystemName)
            {
                ExternalIdentifier = model.uuid,
                ExternalDisplayIdentifier = model.idn,
                OAuthAccessToken = model.accessToken,
                OAuthToken = model.accessToken
            };
            paramaters.AddClaim(claim);
            if (customer != null)
            {
                //link the account
                _openAuthenticationService.AssociateExternalAccountWithUser(customer, paramaters);
                flag = true;
            }
            else flag = false;

            return flag;
        }
    }
}
