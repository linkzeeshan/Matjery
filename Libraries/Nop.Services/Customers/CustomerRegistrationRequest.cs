
using Nop.Core.Domain.Customers;
using System.Collections.Generic;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer registration request
    /// </summary>
    public class CustomerRegistrationRequest
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="email">Email</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="passwordFormat">Password format</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="isApproved">Is approved</param>
        public CustomerRegistrationRequest(Customer customer, string email, string username,
            string password,
            PasswordFormat passwordFormat,
            int storeId,
            bool isApproved = true,
            bool isExternal=false,
            IList<string> categories =null,
            bool tradeLicenseFile=false,
            string licenseNumber=null,
            string registrationType=null,
            string _ExpiryDate=null,
            string _LicenseNo=null,
            int vendorID=0)
        {
            vendorId = vendorID;
            Customer = customer;
            Email = email;
            Username = username;
            Password = password;
            PasswordFormat = passwordFormat;
            StoreId = storeId;
            IsApproved = isApproved;
            IsExternal = isExternal;
            Categories = categories;
            TradeLicenseFile = tradeLicenseFile;
            LicenseNumber = licenseNumber;
            RegistrationType = registrationType;
            ExpiryDate = _ExpiryDate;
            LicenseNo = _LicenseNo;
        }

        public int vendorId { get; set; }
        /// <summary>
        /// Customer
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Password format
        /// </summary>
        public PasswordFormat PasswordFormat { get; set; }

        /// <summary>
        /// Store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Is approved
        /// </summary>
        public bool IsApproved { get; set; }
        public string Phonenumber { get; set; }
        public bool IsExternal { get; set; }
        public IList<string> Categories  { get; set; }
        public bool TradeLicenseFile { get; set; }
        public string LicenseNumber { get; set; }
        public string RegistrationType { get; set; }
        public string ExpiryDate { get; set; }
        public string LicenseNo { get; set; }
    }
}
