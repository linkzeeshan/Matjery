//Contributor:  Nicholas Mayne

using System;
using System.Collections.Generic;

namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// Open authentication parameters
    /// </summary>
    [Serializable]
    public abstract partial class OpenAuthenticationParameters
    {
        public abstract string ProviderSystemName { get; }
        public string ExternalIdentifier { get; set; }
        public string ExternalDisplayIdentifier { get; set; }
        public string OAuthToken { get; set; }
        public string OAuthAccessToken { get; set; }
        public string RegistrationType { get; set; }
        public string VendorParamsString { get; set; }

        public virtual IList<UserClaims> UserClaims
        {
            get { return new List<UserClaims>(0); }
        }
    }
    public class UaePassUserExistModel
    {
        public bool IsEmailExist { get; set; }
        public bool IsUuidExist { get; set; }
        public bool IsSeller { get; set; }
        public int CustomerId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }
}