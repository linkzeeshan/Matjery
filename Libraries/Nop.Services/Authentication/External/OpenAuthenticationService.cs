//Contributor:  Nicholas Mayne

using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Customers;

namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// Open authentication service
    /// </summary>
    public partial class OpenAuthenticationService : IOpenAuthenticationService
    {
        private readonly ICustomerService _customerService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IRepository<ExternalAuthenticationRecord> _externalAuthenticationRecordRepository;

        public OpenAuthenticationService(IRepository<ExternalAuthenticationRecord> externalAuthenticationRecordRepository,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            ICustomerService customerService)
        {
            this._externalAuthenticationRecordRepository = externalAuthenticationRecordRepository;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._customerService = customerService;
        }

        /// <summary>
        /// Load active external authentication methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Payment methods</returns>
        public virtual IList<IExternalAuthenticationMethod> LoadActiveExternalAuthenticationMethods(int storeId = 0)
        {
            return LoadAllExternalAuthenticationMethods(storeId)
                   .Where(provider => _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Contains(provider.PluginDescriptor.SystemName, StringComparer.InvariantCultureIgnoreCase))
                   .ToList();
        }

        /// <summary>
        /// Load external authentication method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found external authentication method</returns>
        public virtual IExternalAuthenticationMethod LoadExternalAuthenticationMethodBySystemName(string systemName)
        {
            //var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IExternalAuthenticationMethod>(systemName);
            //if (descriptor != null)
            //    return descriptor.Instance<IExternalAuthenticationMethod>();

            //return null;
            throw new NotImplementedException();

        }

        /// <summary>
        /// Load all external authentication methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>External authentication methods</returns>
        public virtual IList<IExternalAuthenticationMethod> LoadAllExternalAuthenticationMethods(int storeId = 0)
        {
            //return _pluginFinder
            //    .GetPlugins<IExternalAuthenticationMethod>(storeId: storeId)
            //    .ToList();
            throw new NotImplementedException();
        }
        public virtual void AssociateExternalAccountWithUser(Customer customer, OpenAuthenticationParameters parameters)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            //find email
            string email = null;
            if (parameters.UserClaims != null)
                foreach (var userClaim in parameters.UserClaims
                    .Where(x => x.Contact != null && !String.IsNullOrEmpty(x.Contact.Email)))
                {
                    //found
                    email = userClaim.Contact.Email;
                    break;
                }

            var externalAuthenticationRecord = new ExternalAuthenticationRecord
            {
                CustomerId = customer.Id,
                Email = email,
                ExternalIdentifier = parameters.ExternalIdentifier,
                ExternalDisplayIdentifier = parameters.ExternalDisplayIdentifier,
                OAuthToken = parameters.OAuthToken,
                OAuthAccessToken = parameters.OAuthAccessToken,
                ProviderSystemName = parameters.ProviderSystemName,
            };

            _externalAuthenticationRecordRepository.Insert(externalAuthenticationRecord);
        }

        public virtual bool AccountExists(OpenAuthenticationParameters parameters)
        {
            return GetUser(parameters) != null;
        }

        public virtual Customer GetUser(OpenAuthenticationParameters parameters)
        {
            var record = _externalAuthenticationRecordRepository.Table
                .Where(o => o.ExternalIdentifier == parameters.ExternalIdentifier &&
                    o.ProviderSystemName == parameters.ProviderSystemName).FirstOrDefault();

            if (record != null)
            {
                var customer = _customerService.GetCustomerById(record.CustomerId);
                
                if (customer.Deleted)
                {
                    //customer.Deleted = false;
                    //customer.Active = customer.Active == false ? true : customer.Active;
                    //Ensure if Email format is with deleted_
                    customer.Email = CommonHelper.CustomerEmailFormat(customer.Email);
                    customer.Username = CommonHelper.CustomerNameFormat(customer.Username);
                    
                    _customerService.UpdateCustomer(customer);
                }
                return customer;
            }

            return null;
        }

        public virtual IList<ExternalAuthenticationRecord> GetExternalIdentifiersFor(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            return _externalAuthenticationRecordRepository.Table.Where(x => x.Email == customer.Email).ToList();
            //return customer.ExternalAuthenticationRecords.ToList();
        }

        public virtual int GetUserIdFromUaePassLinkedUser(string uuid, string providerSystemName = null)
        {
            var userId = 0;
            if (string.IsNullOrEmpty(uuid))
                return 0;
            var query = _externalAuthenticationRecordRepository.Table
                .FirstOrDefault(o => o.ExternalIdentifier == uuid &&
                    o.ProviderSystemName == providerSystemName);
            if (query != null)
                userId = query.CustomerId;

            return userId;

        }
        public virtual void DeletExternalAuthenticationRecord(ExternalAuthenticationRecord externalAuthenticationRecord)
        {
            if (externalAuthenticationRecord == null)
                throw new ArgumentNullException("externalAuthenticationRecord");

            _externalAuthenticationRecordRepository.Delete(externalAuthenticationRecord);
        }

        public virtual void RemoveAssociation(OpenAuthenticationParameters parameters)
        {
            var record = _externalAuthenticationRecordRepository.Table
                .FirstOrDefault(o => o.ExternalIdentifier == parameters.ExternalIdentifier &&
                    o.ProviderSystemName == parameters.ProviderSystemName);

            if (record != null)
                _externalAuthenticationRecordRepository.Delete(record);
        }
        public virtual UaePassUserExistModel UaePassUserExist(string email = null, string externalIdentifier = null, string providerSystemName = null)
        {
            var model = new UaePassUserExistModel();

            var Uuid = _externalAuthenticationRecordRepository.Table
                .FirstOrDefault(o => o.ExternalIdentifier == externalIdentifier &&
                    o.ProviderSystemName == providerSystemName);

                if (Uuid != null)
                {
                    model.IsUuidExist = true;
                    model.CustomerId = Uuid.CustomerId;
                }
         
            var emailExist = _externalAuthenticationRecordRepository.Table
                .FirstOrDefault(o => o.Email == email &&
                    o.ProviderSystemName == providerSystemName);

            if (emailExist != null)
                model.IsEmailExist = true;

            return model;
        }

        public virtual string GetProviderSystemName(int CustomerId )
        {
            var user = _externalAuthenticationRecordRepository.Table
                .FirstOrDefault(o => o.CustomerId == CustomerId);
            if (user != null)
                return user.ProviderSystemName;
            else return string.Empty;
        }
        public virtual void UpdateExternalAccountWithUser(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");


            var externalAuthenticationRecord = new ExternalAuthenticationRecord
            {
                CustomerId = customer.Id,
                Email = customer.Email,
            };

            _externalAuthenticationRecordRepository.Update(externalAuthenticationRecord);
        }
        public virtual bool UserExist(int customerId, string email)
        {
            var user = _externalAuthenticationRecordRepository.Table
                .FirstOrDefault(o => o.CustomerId == customerId);
            if (user != null)
                return true;

            return false;
        }
    }
}