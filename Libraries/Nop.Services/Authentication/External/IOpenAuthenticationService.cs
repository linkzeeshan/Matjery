//Contributor:  Nicholas Mayne

using System.Collections.Generic;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// Open authentication service
    /// </summary>
    public partial interface IOpenAuthenticationService
    {
        /// <summary>
        /// Load active external authentication methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Payment methods</returns>
        IList<IExternalAuthenticationMethod> LoadActiveExternalAuthenticationMethods(int storeId = 0);

        /// <summary>
        /// Load external authentication method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found external authentication method</returns>
        IExternalAuthenticationMethod LoadExternalAuthenticationMethodBySystemName(string systemName);

        /// <summary>
        /// Load all external authentication methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>External authentication methods</returns>
        IList<IExternalAuthenticationMethod> LoadAllExternalAuthenticationMethods(int storeId = 0);


        bool AccountExists(OpenAuthenticationParameters parameters);

        void AssociateExternalAccountWithUser(Customer customer, OpenAuthenticationParameters parameters);

        void UpdateExternalAccountWithUser(Customer customer);

        Customer GetUser(OpenAuthenticationParameters parameters);

        IList<ExternalAuthenticationRecord> GetExternalIdentifiersFor(Customer customer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalAuthenticationRecord"></param>
        void DeletExternalAuthenticationRecord(ExternalAuthenticationRecord externalAuthenticationRecord);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        void RemoveAssociation(OpenAuthenticationParameters parameters);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="externalIdentifier"></param>
        /// <param name="providerSystemName"></param>
        /// <returns></returns>
        UaePassUserExistModel UaePassUserExist(string email = null, string externalIdentifier = null, string providerSystemName = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="providerSystemName"></param>
        /// <returns></returns>
        int GetUserIdFromUaePassLinkedUser(string uuid, string providerSystemName = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        string GetProviderSystemName(int CustomerId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        bool UserExist(int customerId, string email);
    }
}