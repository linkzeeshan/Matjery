using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer registration service
    /// </summary>
    public partial class CustomerRegistrationService : ICustomerRegistrationService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly RewardPointsSettings _rewardPointsSettings;

        #endregion

        #region Ctor

        public CustomerRegistrationService(CustomerSettings customerSettings,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IRewardPointService rewardPointService,
            IStoreService storeService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            RewardPointsSettings rewardPointsSettings)
        {
            _vendorService = vendorService;
            _customerSettings = customerSettings;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _rewardPointService = rewardPointService;
            _storeService = storeService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _rewardPointsSettings = rewardPointsSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check whether the entered password matches with a saved one
        /// </summary>
        /// <param name="customerPassword">Customer password</param>
        /// <param name="enteredPassword">The entered password</param>
        /// <returns>True if passwords match; otherwise false</returns>
        protected bool PasswordsMatch(CustomerPassword customerPassword, string enteredPassword, bool ignorePasswordHash = false)
        {
            if (customerPassword == null || string.IsNullOrEmpty(enteredPassword))
                return false;

            var savedPassword = string.Empty;
            switch (customerPassword.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    savedPassword = enteredPassword;
                    break;
                case PasswordFormat.Encrypted:
                    savedPassword = _encryptionService.EncryptText(enteredPassword);
                    break;
                case PasswordFormat.Hashed:
                    if (ignorePasswordHash)
                        savedPassword = enteredPassword;
                    else
                        savedPassword = _encryptionService.CreatePasswordHash(enteredPassword, customerPassword.PasswordSalt, _customerSettings.HashedPasswordFormat);
                    break;
            }

            if (customerPassword.Password == null)
                return false;

            return customerPassword.Password.Equals(savedPassword);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validate customer
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>Result</returns>
        public virtual CustomerLoginResults ValidateCustomer(string usernameOrEmail, string password, bool ignorePasswordHash = false,bool IsExternal=false)
        {
            var customer = _customerSettings.UsernamesEnabled ?
                _customerService.GetCustomerByUsername(usernameOrEmail) :
                _customerService.GetCustomerByEmail(usernameOrEmail);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;
            //only registered can login
            if (!_customerService.IsRegistered(customer))
                return CustomerLoginResults.NotRegistered;


            //check whether a customer is locked out
            if (customer.CannotLoginUntilDateUtc.HasValue && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return CustomerLoginResults.LockedOut;

            if (!IsExternal)
            {
                if (!PasswordsMatch(_customerService.GetCurrentPassword(customer.Id), password, ignorePasswordHash))
                {
                    //wrong password
                    customer.FailedLoginAttempts++;
                    if (_customerSettings.FailedPasswordAllowedAttempts > 0 &&
                        customer.FailedLoginAttempts >= _customerSettings.FailedPasswordAllowedAttempts)
                    {
                        //lock out
                        customer.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(_customerSettings.FailedPasswordLockoutMinutes);
                        //reset the counter
                        customer.FailedLoginAttempts = 0;
                    }

                    _customerService.UpdateCustomer(customer);

                    return CustomerLoginResults.WrongPassword;
                }
            }
      

            //update login details
            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.RequireReLogin = false;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer);

            return CustomerLoginResults.Successful;
        }
        public virtual CustomerLoginResults ValidateCustomerByUUID(int uuid, bool ignorePasswordHash = false)
        {
            var customer = _customerService.GetCustomerById(uuid);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;
            //only registered can login
            if (!_customerService.IsRegistered(customer))
                return CustomerLoginResults.NotRegistered;

            //save last login date
            customer.LastLoginDateUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer);
            return CustomerLoginResults.Successful;
        }


        public virtual CustomerRegistrationResult ValidateRegister(CustomerRegistrationRequest request)
        {
            var result = new CustomerRegistrationResult();

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Customer == null)
                throw new ArgumentException("Can't load current customer");

            if (request.IsExternal)
            {

            }
            if (request.Customer.IsSearchEngineAccount())
            {
                result.AddError("Search engine can't be registered");
                return result;
            }

            if (request.Customer.IsBackgroundTaskAccount())
            {
                result.AddError("Background task account can't be registered");
                return result;
            }

            if (_customerService.IsRegistered(request.Customer))
            {
                result.AddError("Current customer is already registered");
                return result;
            }
            if (!_customerSettings.UsernamesEnabled)
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided"));
                    return result;
                }

                if (!CommonHelper.IsValidEmail(request.Email))
                {
                    result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                    return result;
                }
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }

            //if (_customerSettings.UsernamesEnabled && string.IsNullOrEmpty(request.Username))
            //{
            //    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
            //    return result;
            //}

            if (_customerSettings.UsernamesEnabled)
            {
                if (String.IsNullOrEmpty(request.Username))
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
                    return result;
                }
                var isArabic = Regex.IsMatch(request.Username, @"\p{IsArabic}");
                var isSpaces = request.Username.Any(x => Char.IsWhiteSpace(x));
                if (isArabic || isSpaces)
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameEnlgishAndSpaces"));
                    return result;
                }
            }

            ////validate unique user
            if (!request.IsExternal)
            {
                if (_customerService.GetCustomerByEmail(request.Email) != null)
                {
                    result.AddError(CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists")));
                    return result;
                }
            }
            if (!request.IsExternal)
            {
                if (_customerSettings.UsernamesEnabled && _customerService.GetCustomerByUsername(request.Username) != null)
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists"));
                    return result;
                }
            }
            //unique phone number
            if (!string.IsNullOrEmpty(request.Phonenumber))
            {
                var customers = _customerService.GetAllCustomers(phone: request.Phonenumber).Where(x => !string.IsNullOrEmpty(x.Username)).ToList();
                if (customers.Count > 0)
                {
                    result.AddError(_localizationService.GetResource("Address.Fields.PhoneNumber.PhonenumberAlreadyExist"));
                    return result;
                }
            }

            if (request.RegistrationType.ToLower() == "merchant")
            {
                var vendors = _vendorService.GetAllVendors(showNotActive:false,getDeleted:false).ToList();
                if (!string.IsNullOrEmpty(request.LicenseNo))
                {
                    if (vendors.Where(x => x.LicenseNo == request.LicenseNo.Trim()).Any())
                    {
                        result.AddError(_localizationService.GetResource("Address.Fields.vendor.license.AlreadyExist"));
                        return result;
                    }
                }
              
                if (request.Categories != null)
                {
                    List<string> filterTags = new List<string>() { "65", "66", "67" };
                    bool containsValues = request.Categories.Any(x => filterTags.Contains(x));
                    if (containsValues && !request.TradeLicenseFile)
                    {
                        result.AddError(_localizationService.GetResource("account.register.errors.license"));
                        return result;
                    }
                    if (containsValues && string.IsNullOrEmpty(request.LicenseNumber))
                    {
                        result.AddError(_localizationService.GetResource("account.register.errors.tradelicensenumber"));
                        return result;
                    }
                }
                if (!string.IsNullOrEmpty(request.ExpiryDate))
                {
                    //DateTime expiryDate = DateTime.Parse(request.ExpiryDate); 
                    //DateTime currentDate = DateTime.Now;

                    //if (expiryDate < currentDate)
                    //{
                    //    result.AddError(_localizationService.GetResource("account.register.errors.licenseexpiry"));
                    //    return result;
                    //}

                }
            }
        

            return result;
        }

        /// <summary>
        /// Register customer
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        {
            var result = new CustomerRegistrationResult();
         
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Customer == null)
                throw new ArgumentException("Can't load current customer");

            if (request.IsExternal)
            {
                if (request.RegistrationType.ToLower() == "merchant" || request.RegistrationType.ToLower() == "seller")
                {
                    if (!string.IsNullOrEmpty(request.LicenseNo))
                    {
                        var vendors = _vendorService.GetAllVendors(showNotActive: false, getDeleted: false).ToList();
                        if (vendors.Where(x => x.LicenseNo == request.LicenseNo.Trim()).Any())
                        {
                            result.AddError(_localizationService.GetResource("Address.Fields.vendor.licenseexists"));
                            return result;
                        }
                    }
                  
                    if (request.Categories != null)
                    {
                        List<string> filterTags = new List<string>() { "65", "66", "67" };
                        bool containsValues = request.Categories.Any(x => filterTags.Contains(x));
                        if (containsValues && !request.TradeLicenseFile)
                        {
                            result.AddError(_localizationService.GetResource("account.register.errors.license"));
                            return result;
                        }
                        if (containsValues && string.IsNullOrEmpty(request.LicenseNumber))
                        {
                            result.AddError(_localizationService.GetResource("account.register.errors.tradelicensenumber"));
                            return result;
                        }
                    }
                    //if (!string.IsNullOrEmpty(request.ExpiryDate))
                    //{
                    //    DateTime expiryDate = DateTime.Parse(request.ExpiryDate);
                    //    DateTime currentDate = DateTime.Now;

                    //    if (expiryDate < currentDate)
                    //    {
                    //        result.AddError(_localizationService.GetResource("account.register.errors.licenseexpiry"));
                    //        return result;
                    //    }

                    //}
                }


            }
            if (request.Customer.IsSearchEngineAccount())
            {
                result.AddError("Search engine can't be registered");
                return result;
            }

            if (request.Customer.IsBackgroundTaskAccount())
            {
                result.AddError("Background task account can't be registered");
                return result;
            }

            if (_customerService.IsRegistered(request.Customer))
            {
                result.AddError("Current customer is already registered");
                return result;
            }
            if (!_customerSettings.UsernamesEnabled)
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided"));
                    return result;
                }

                if (!CommonHelper.IsValidEmail(request.Email))
                {
                    result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                    return result;
                }
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }

            //if (_customerSettings.UsernamesEnabled && string.IsNullOrEmpty(request.Username))
            //{
            //    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
            //    return result;
            //}

            if (_customerSettings.UsernamesEnabled)
            {
                if (String.IsNullOrEmpty(request.Username))
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
                    return result;
                }
                var isArabic = Regex.IsMatch(request.Username, @"\p{IsArabic}");
                var isSpaces = request.Username.Any(x => Char.IsWhiteSpace(x));
                if (isArabic || isSpaces)
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameEnlgishAndSpaces"));
                    return result;
                }
            }

            ////validate unique user
            if (!request.IsExternal)
            {
                if (_customerService.GetCustomerByEmail(request.Email) != null)
                {
                    result.AddError(CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists")));
                    return result;
                }
            }
            if (!request.IsExternal)
            {
                if (_customerSettings.UsernamesEnabled && _customerService.GetCustomerByUsername(request.Username) != null)
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists"));
                    return result;
                }
            }
            //unique phone number
            if (!string.IsNullOrEmpty(request.Phonenumber))
            {
                var customers = _customerService.GetAllCustomers(phone: request.Phonenumber).Where(x=>!string.IsNullOrEmpty(x.Username)).ToList();
                if (customers.Count > 0)
                {
                    result.AddError(_localizationService.GetResource("Address.Fields.PhoneNumber.PhonenumberAlreadyExist"));
                    return result;
                }
            }
            //at this point request is valid
            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;

            var customerPassword = new CustomerPassword
            {
                CustomerId = request.Customer.Id,
                PasswordFormat = request.PasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    customerPassword.Password = request.Password;
                    break;
                case PasswordFormat.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.Password);
                    break;
                case PasswordFormat.Hashed:

                    var saltKey = _encryptionService.CreateSaltKey(NopCustomerServicesDefaults.PasswordSaltKeySize);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _customerSettings.HashedPasswordFormat);
                    break;
            }

            _customerService.InsertCustomerPassword(customerPassword);

            request.Customer.Active = request.IsApproved;

            //add to 'Registered' role
            var registeredRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");

            _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = request.Customer.Id, CustomerRoleId = registeredRole.Id });

            if (request.RegistrationType.ToLower() == "seller"|| request.RegistrationType.ToLower() == "merchant")
            {
                request.Customer.VendorId = request.vendorId;
                var sellerRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.VendorsRoleName);
                if (sellerRole != null)
                    _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = request.Customer.Id, CustomerRoleId = sellerRole.Id });

            }
            //remove from 'Guests' role            
            if (_customerService.IsGuest(request.Customer))
            {
                var guestRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.GuestsRoleName);
                _customerService.RemoveCustomerRoleMapping(request.Customer, guestRole);
            }

            //add reward points for customer registration (if enabled)
            if (_rewardPointsSettings.Enabled && _rewardPointsSettings.PointsForRegistration > 0)
            {
                var endDate = _rewardPointsSettings.RegistrationPointsValidity > 0
                    ? (DateTime?)DateTime.UtcNow.AddDays(_rewardPointsSettings.RegistrationPointsValidity.Value) : null;
                _rewardPointService.AddRewardPointsHistoryEntry(request.Customer, _rewardPointsSettings.PointsForRegistration,
                    request.StoreId, _localizationService.GetResource("RewardPoints.Message.EarnedForRegistration"), endDate: endDate);
            }

            _customerService.UpdateCustomer(request.Customer);

            return result;
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual ChangePasswordResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = new ChangePasswordResult();
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailIsNotProvided"));
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordIsNotProvided"));
                return result;
            }

            var customer = _customerService.GetCustomerByEmail(request.Email);
            if (customer == null)
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailNotFound"));
                return result;
            }

            //request isn't valid
            if (request.ValidateRequest && !PasswordsMatch(_customerService.GetCurrentPassword(customer.Id), request.OldPassword))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.OldPasswordDoesntMatch"));
                return result;
            }

            //check for duplicates
            if (_customerSettings.UnduplicatedPasswordsNumber > 0)
            {
                //get some of previous passwords
                var previousPasswords = _customerService.GetCustomerPasswords(customer.Id, passwordsToReturn: _customerSettings.UnduplicatedPasswordsNumber);

                var newPasswordMatchesWithPrevious = previousPasswords.Any(password => PasswordsMatch(password, request.NewPassword));
                if (newPasswordMatchesWithPrevious)
                {
                    result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordMatchesWithPrevious"));
                    return result;
                }
            }

            //at this point request is valid
            var customerPassword = new CustomerPassword
            {
                CustomerId = customer.Id,
                PasswordFormat = request.NewPasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.NewPasswordFormat)
            {
                case PasswordFormat.Clear:
                    customerPassword.Password = request.NewPassword;
                    break;
                case PasswordFormat.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.NewPassword);
                    break;
                case PasswordFormat.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(NopCustomerServicesDefaults.PasswordSaltKeySize);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey,
                        request.HashedPasswordFormat ?? _customerSettings.HashedPasswordFormat);
                    break;
            }

            _customerService.InsertCustomerPassword(customerPassword);

            //publish event
            _eventPublisher.Publish(new CustomerPasswordChangedEvent(customerPassword));

            return result;
        }

        /// <summary>
        /// Sets a user email
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newEmail">New email</param>
        /// <param name="requireValidation">Require validation of new email address</param>
        public virtual void SetEmail(Customer customer, string newEmail, bool requireValidation)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (newEmail == null)
                throw new NopException("Email cannot be null");

            newEmail = newEmail.Trim();
            var oldEmail = customer.Email;

            if (!CommonHelper.IsValidEmail(newEmail))
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.NewEmailIsNotValid"));

            if (newEmail.Length > 100)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailTooLong"));

            var customer2 = _customerService.GetCustomerByEmail(newEmail);
            if (customer2 != null && customer.Id != customer2.Id)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailAlreadyExists"));

            if (requireValidation)
            {
                //re-validate email
                customer.EmailToRevalidate = newEmail;
                _customerService.UpdateCustomer(customer);

                //email re-validation message
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute, Guid.NewGuid().ToString());
                _workflowMessageService.SendCustomerEmailRevalidationMessage(customer, _workContext.WorkingLanguage.Id);
            }
            else
            {
                customer.Email = newEmail;
                _customerService.UpdateCustomer(customer);

                if (string.IsNullOrEmpty(oldEmail) || oldEmail.Equals(newEmail, StringComparison.InvariantCultureIgnoreCase))
                    return;

                //update newsletter subscription (if required)
                foreach (var store in _storeService.GetAllStores())
                {
                    var subscriptionOld = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(oldEmail, store.Id);

                    if (subscriptionOld == null)
                        continue;

                    subscriptionOld.Email = newEmail;
                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscriptionOld);
                }
            }
        }

        /// <summary>
        /// Sets a customer username
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newUsername">New Username</param>
        public virtual void SetUsername(Customer customer, string newUsername)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (!_customerSettings.UsernamesEnabled)
                throw new NopException("Usernames are disabled");

            newUsername = newUsername.Trim();

            if (newUsername.Length > NopCustomerServicesDefaults.CustomerUsernameLength)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameTooLong"));

            var user2 = _customerService.GetCustomerByUsername(newUsername);
            if (user2 != null && customer.Id != user2.Id)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameAlreadyExists"));

            customer.Username = newUsername;
            _customerService.UpdateCustomer(customer);
        }

       

        #endregion
    }
}