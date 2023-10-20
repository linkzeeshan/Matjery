using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Caching.Extensions;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;

namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// Represents external authentication service implementation
    /// </summary>
    public partial class ExternalAuthenticationService : IExternalAuthenticationService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<ExternalAuthenticationRecord> _externalAuthenticationRecordRepository;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;


        #endregion

        #region Ctor

        public ExternalAuthenticationService(CustomerSettings customerSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            IAuthenticationPluginManager authenticationPluginManager,
            IAuthenticationService authenticationService,
            ICustomerActivityService customerActivityService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IRepository<ExternalAuthenticationRecord> externalAuthenticationRecordRepository,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings)
        {
            _customerSettings = customerSettings;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _authenticationPluginManager = authenticationPluginManager;
            _authenticationService = authenticationService;
            _customerActivityService = customerActivityService;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _externalAuthenticationRecordRepository = externalAuthenticationRecordRepository;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Authenticate user with existing associated external account
        /// </summary>
        /// <param name="associatedUser">Associated with passed external authentication parameters user</param>
        /// <param name="currentLoggedInUser">Current logged-in user</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        protected virtual IActionResult AuthenticateExistingUser(Customer associatedUser, Customer currentLoggedInUser, string returnUrl)
        {
            //log in guest user
            if (currentLoggedInUser == null)
                return LoginUser(associatedUser, returnUrl);

            //account is already assigned to another user
            if (currentLoggedInUser.Id != associatedUser.Id)
                return ErrorAuthentication(new[] { _localizationService.GetResource("Account.AssociatedExternalAuth.AccountAlreadyAssigned") }, returnUrl);

            //or the user try to log in as himself. bit weird
            return SuccessfulAuthentication(returnUrl);
        }

        /// <summary>
        /// Authenticate current user and associate new external account with user
        /// </summary>
        /// <param name="currentLoggedInUser">Current logged-in user</param>
        /// <param name="parameters">Authentication parameters received from external authentication method</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        protected virtual IActionResult AuthenticateNewUser(Customer currentLoggedInUser, ExternalAuthenticationParameters parameters, string returnUrl)
        {
            //associate external account with logged-in user
            if (currentLoggedInUser != null)
            {
                AssociateExternalAccountWithUser(currentLoggedInUser, parameters);
                return SuccessfulAuthentication(returnUrl);
            }

            //or try to register new user
            if (_customerSettings.UserRegistrationType != UserRegistrationType.Disabled)
                return RegisterNewUser(parameters, returnUrl);

            //registration is disabled
            return ErrorAuthentication(new[] { "Registration is disabled" }, returnUrl);
        }

    
        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="parameters">Authentication parameters received from external authentication method</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        protected virtual IActionResult RegisterNewUser(ExternalAuthenticationParameters parameters, string returnUrl)
        {
            //check whether the specified email has been already registered
            var Extuser = this.GetExternalAuthUser(parameters.ProviderSystemName, parameters.Email);
            if (Extuser!=null)
            {
                var alreadyExistsError = string.Format(_localizationService.GetResource("Account.AssociatedExternalAuth.EmailAlreadyExists"),
                    !string.IsNullOrEmpty(parameters.ExternalDisplayIdentifier) ? parameters.ExternalDisplayIdentifier : parameters.ExternalIdentifier);
                return ErrorAuthentication(new[] { alreadyExistsError }, returnUrl);
            }

            //registration is approved if validation isn't required
            var registrationIsApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard ||
                (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation && !_externalAuthenticationSettings.RequireEmailValidation);

            //create registration request
            var IsExternal = true;
            var registrationRequest = new CustomerRegistrationRequest(_workContext.CurrentCustomer,
                parameters.Email, parameters.Email,
                CommonHelper.GenerateRandomDigitCode(20),
                PasswordFormat.Hashed,
                _storeContext.CurrentStore.Id,
                registrationIsApproved, IsExternal
                );

            //whether registration request has been completed successfully
            var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
            if (!registrationResult.Success)
                return ErrorAuthentication(registrationResult.Errors, returnUrl);


            var currlang = _workContext.WorkingLanguage.Id;
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.LanguageIdAttribute, currlang, _storeContext.CurrentStore.Id);

            //allow to save other customer values by consuming this event
            _eventPublisher.Publish(new CustomerAutoRegisteredByExternalMethodEvent(_workContext.CurrentCustomer, parameters));

            //raise customer registered event
            _eventPublisher.Publish(new CustomerRegisteredEvent(_workContext.CurrentCustomer));

            //store owner notifications
            if (_customerSettings.NotifyNewCustomerRegistration)
                _workflowMessageService.SendCustomerRegisteredNotificationMessage(_workContext.CurrentCustomer, _localizationSettings.DefaultAdminLanguageId);

            //associate external account with registered user
            AssociateExternalAccountWithUser(_workContext.CurrentCustomer, parameters);

            //authenticate
            if (registrationIsApproved)
            {
                _authenticationService.SignIn(_workContext.CurrentCustomer, false);
                _workflowMessageService.SendCustomerWelcomeMessage(_workContext.CurrentCustomer, _workContext.WorkingLanguage.Id);

                return new RedirectToRouteResult("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
            }

            //registration is succeeded but isn't activated
            if (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation)
            {
                //email validation message
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                _workflowMessageService.SendCustomerEmailValidationMessage(_workContext.CurrentCustomer, _workContext.WorkingLanguage.Id);

                return new RedirectToRouteResult("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
            }

            //registration is succeeded but isn't approved by admin
            if (_customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval)
                return new RedirectToRouteResult("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });

            return ErrorAuthentication(new[] { "Error on registration" }, returnUrl);
        }

        /// <summary>
        /// Login passed user
        /// </summary>
        /// <param name="user">User to login</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        protected virtual IActionResult LoginUser(Customer user, string returnUrl)
        {
            //migrate shopping cart
            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, user, true);

            //authenticate
            _authenticationService.SignIn(user, false);

            //raise event       
            _eventPublisher.Publish(new CustomerLoggedinEvent(user));

            //activity log
            _customerActivityService.InsertActivity(user, "PublicStore.Login",
                _localizationService.GetResource("ActivityLog.PublicStore.Login"), user);

            return SuccessfulAuthentication(returnUrl);
        }

        /// <summary>
        /// Add errors that occurred during authentication
        /// </summary>
        /// <param name="errors">Collection of errors</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        protected virtual IActionResult ErrorAuthentication(IEnumerable<string> errors, string returnUrl)
        {
            foreach (var error in errors)
                ExternalAuthorizerHelper.AddErrorsToDisplay(error);

            return new RedirectToActionResult("Login", "Customer", !string.IsNullOrEmpty(returnUrl) ? new { ReturnUrl = returnUrl } : null);
        }

        /// <summary>
        /// Redirect the user after successful authentication
        /// </summary>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        protected virtual IActionResult SuccessfulAuthentication(string returnUrl)
        {
            //redirect to the return URL if it's specified
            if (!string.IsNullOrEmpty(returnUrl))
                return new RedirectResult(returnUrl);

            return new RedirectToRouteResult("Homepage", null);
        }

        #endregion

        #region Methods

        #region Authentication

        /// <summary>
        /// Authenticate user by passed parameters
        /// </summary>
        /// <param name="parameters">External authentication parameters</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        public virtual IActionResult Authenticate(ExternalAuthenticationParameters parameters, string returnUrl = null)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (!_authenticationPluginManager.IsPluginActive(parameters.ProviderSystemName, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id))
                return ErrorAuthentication(new[] { "External authentication method cannot be loaded" }, returnUrl);

            //get current logged-in user
            var currentLoggedInUser = _customerService.IsRegistered(_workContext.CurrentCustomer) ? _workContext.CurrentCustomer : null;

            //authenticate associated user if already exists
            var associatedUser = GetUserByExternalAuthenticationParameters(parameters);
            if (associatedUser != null)
                return AuthenticateExistingUser(associatedUser, currentLoggedInUser, returnUrl);

            //or associate and authenticate new user
            return AuthenticateNewUser(currentLoggedInUser, parameters, returnUrl);
        }

        #endregion

        /// <summary>
        /// Associate external account with customer
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="parameters">External authentication parameters</param>
        public virtual void AssociateExternalAccountWithUser(Customer customer, ExternalAuthenticationParameters parameters)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var externalAuthenticationRecord = new ExternalAuthenticationRecord
            {
                CustomerId = customer.Id,
                Email = parameters.Email,
                ExternalIdentifier = parameters.ExternalIdentifier,
                ExternalDisplayIdentifier = parameters.ExternalDisplayIdentifier,
                OAuthAccessToken = parameters.AccessToken,
                ProviderSystemName = parameters.ProviderSystemName
            };

            _externalAuthenticationRecordRepository.Insert(externalAuthenticationRecord);
        }

        /// <summary>
        /// Get the particular user with specified parameters
        /// </summary>
        /// <param name="parameters">External authentication parameters</param>
        /// <returns>Customer</returns>
        public virtual Customer GetUserByExternalAuthenticationParameters(ExternalAuthenticationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var associationRecord = _externalAuthenticationRecordRepository.Table.FirstOrDefault(record =>
                record.ExternalIdentifier.Equals(parameters.ExternalIdentifier) && record.ProviderSystemName.Equals(parameters.ProviderSystemName));
            if (associationRecord == null)
                return null;

            return _customerService.GetCustomerById(associationRecord.CustomerId);
        }

        /// <summary>
        /// Get the external authentication records by identifier
        /// </summary>
        /// <param name="externalAuthenticationRecordId">External authentication record identifier</param>
        /// <returns>Result</returns>
        public virtual ExternalAuthenticationRecord GetExternalAuthenticationRecordById(int externalAuthenticationRecordId)
        {
            if (externalAuthenticationRecordId == 0)
                return null;

            return _externalAuthenticationRecordRepository.ToCachedGetById(externalAuthenticationRecordId);
        }

        /// <summary>
        /// Get list of the external authentication records by customer
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public virtual IList<ExternalAuthenticationRecord> GetCustomerExternalAuthenticationRecords(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var associationRecords = _externalAuthenticationRecordRepository.Table.Where(ear => ear.CustomerId == customer.Id);

            return associationRecords.ToList();
        }

        /// <summary>
        /// Remove the association
        /// </summary>
        /// <param name="parameters">External authentication parameters</param>
        public virtual void RemoveAssociation(ExternalAuthenticationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var associationRecord = _externalAuthenticationRecordRepository.Table.FirstOrDefault(record =>
                record.ExternalIdentifier.Equals(parameters.ExternalIdentifier) && record.ProviderSystemName.Equals(parameters.ProviderSystemName));

            if (associationRecord != null)
                _externalAuthenticationRecordRepository.Delete(associationRecord);
        }

        /// <summary>
        /// Delete the external authentication record
        /// </summary>
        /// <param name="externalAuthenticationRecord">External authentication record</param>
        public virtual void DeleteExternalAuthenticationRecord(ExternalAuthenticationRecord externalAuthenticationRecord)
        {
            if (externalAuthenticationRecord == null)
                throw new ArgumentNullException(nameof(externalAuthenticationRecord));

            _externalAuthenticationRecordRepository.Delete(externalAuthenticationRecord);
        }

        public virtual AuthorizationResult AuthorizeUaePass(string uuid, string idn, string oauthAccessToken, string oauthToken, string email, string name)
        {
            var parameters = new UaePassAuthenticationParameters(Provider.SystemName)
            {
                ExternalIdentifier = uuid,
                ExternalDisplayIdentifier = idn,
                OAuthAccessToken = oauthAccessToken,
                OAuthToken = oauthToken
            };
            UserClaims claim = new UserClaims();
            claim.Contact = new ContactClaims();
            claim.Contact.Email = CommonHelper.CustomerEmailFormat(email) ;
            claim.Name = new NameClaims();
            string str = name;
            if (!string.IsNullOrEmpty(str))
            {
                string[] strArray = str.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (strArray.Length >= 2)
                {
                    claim.Name.First = strArray[0];
                    claim.Name.Last = strArray[1];
                }
                else
                    claim.Name.Last = strArray[0];
            }

            parameters.AddClaim(claim);
            var result = this.Authorize(parameters);
            if (result.Errors.Count > 0)
            {
                var errorsToCheck = new List<string>
                {
                    _localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists"),
                    _localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists")
                };

                foreach (var error in result.Errors)
                {
                    if (errorsToCheck.Contains(error))
                    {
                        //redirect to manual linking
                        result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                        result.AddError(error);
                        break;
                    }
                }
            }

            return result;
        }
        public virtual AuthorizationResult Authorize(OpenAuthenticationParameters parameters)
        {
            var _openAuthenticationService = EngineContext.Current.Resolve<IOpenAuthenticationService>();
            var userFound = _openAuthenticationService.GetUser(parameters);

            var userLoggedIn = _customerService.IsRegistered(_workContext.CurrentCustomer)? _workContext.CurrentCustomer : null;

            if (AccountAlreadyExists(userFound, userLoggedIn))
            {
                if (AccountIsAssignedToLoggedOnAccount(userFound, userLoggedIn))
                {
                    // The person is trying to log in as himself.. bit weird
                    return new AuthorizationResult(OpenAuthenticationStatus.Authenticated);
                }

                var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                result.AddError("Account is already assigned");
                return result;
            }
            if (AccountDoesNotExistAndUserIsNotLoggedOn(userFound, userLoggedIn))
            {
               // ExternalAuthorizerHelper.StoreParametersForRoundTrip(parameters);

                if (AutoRegistrationIsEnabled())
                {
                    #region Register user

                    var currentCustomer =_workContext.CurrentCustomer;
                    var details = new RegistrationDetails(parameters);
                    var randomPassword = CommonHelper.GenerateRandomDigitCode(20);


                    bool isApproved =
                        //standard registration
                        (_customerSettings.UserRegistrationType == UserRegistrationType.Standard) ||
                        //skip email validation?
                        (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation &&
                         !_externalAuthenticationSettings.RequireEmailValidation);

                    bool IsExternal = string.IsNullOrEmpty(parameters.ProviderSystemName) ? false : true;
                    string RegType= string.IsNullOrEmpty(parameters.RegistrationType) ? "buyer" : parameters.RegistrationType;
                    var registrationRequest = new CustomerRegistrationRequest(currentCustomer,
                        details.EmailAddress,
                        _customerSettings.UsernamesEnabled ? details.UserName : details.EmailAddress,
                        randomPassword,
                        PasswordFormat.Clear,
                        _storeContext.CurrentStore.Id,
                        isApproved, IsExternal,
                        registrationType: RegType);

                    // registrationRequest.Customer = _customerService.GetCustomerByEmail(details.EmailAddress).Id > 0 ? _customerService.GetCustomerByEmail(details.EmailAddress) : registrationRequest.Customer;
                       // var customerinfo = _customerService.GetCustomerByEmail(registrationRequest.Email);
                   
                        userFound = currentCustomer;
                        _openAuthenticationService.AssociateExternalAccountWithUser(currentCustomer, parameters);
                        //ExternalAuthorizerHelper.RemoveParameters();

                        //code below is copied from CustomerController.Register method

                        //authenticate
                        if (isApproved)
                            _authenticationService.SignIn(userFound ?? userLoggedIn, false);

                        //notifications
                        if (_customerSettings.NotifyNewCustomerRegistration)
                            _workflowMessageService.SendCustomerRegisteredNotificationMessage(currentCustomer, _localizationSettings.DefaultAdminLanguageId);

                        //raise event       
                        _eventPublisher.Publish(new CustomerRegisteredEvent(currentCustomer));
                    
                    
                        var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                        if (registrationResult.Success)
                        {
                            //store other parameters (form fields)
                            if (!String.IsNullOrEmpty(details.FirstName))
                                _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.FirstNameAttribute, details.FirstName);
                            if (!String.IsNullOrEmpty(details.LastName))
                                _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.LastNameAttribute, details.LastName);
                            if (!String.IsNullOrEmpty(details.Gender))
                                _genericAttributeService.SaveAttribute<string>(currentCustomer, NopCustomerDefaults.GenderAttribute, details.Gender);
                            if (!String.IsNullOrEmpty(details.PhoneNumber))
                                _genericAttributeService.SaveAttribute<string>(currentCustomer, NopCustomerDefaults.PhoneAttribute, details.PhoneNumber);

                        var currlang = _workContext.WorkingLanguage.Id;
                        _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.LanguageIdAttribute, currlang, _storeContext.CurrentStore.Id);

                        if (!String.IsNullOrEmpty(details.DateOfBirth))
                            {
                                DateTime dateOfBirth = DateTime.Parse(details.DateOfBirth, CultureInfo.InvariantCulture);
                                _genericAttributeService.SaveAttribute<DateTime>(currentCustomer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                            }


                            if (!String.IsNullOrEmpty(details.Nationality))
                            {
                            var _countryService = EngineContext.Current.Resolve<ICountryService>();
                            var country = _countryService.GetAllCountries().Where(x => x.ThreeLetterIsoCode.Equals(details.Nationality.Trim())).FirstOrDefault();
                                if (country != null)
                                    _genericAttributeService.SaveAttribute<int>(currentCustomer, NopCustomerDefaults.NationalityCountryId, country.Id);
                            }

                            userFound = currentCustomer;
                            _openAuthenticationService.AssociateExternalAccountWithUser(currentCustomer, parameters);
                            //ExternalAuthorizerHelper.RemoveParameters();

                            //code below is copied from CustomerController.Register method

                            //authenticate
                            if (isApproved)
                                _authenticationService.SignIn(userFound ?? userLoggedIn, false);

                            //notifications
                            if (_customerSettings.NotifyNewCustomerRegistration)
                                _workflowMessageService.SendCustomerRegisteredNotificationMessage(currentCustomer, _localizationSettings.DefaultAdminLanguageId);

                            //raise event       
                            _eventPublisher.Publish(new CustomerRegisteredEvent(currentCustomer));

                            if (isApproved)
                            {
                                //standard registration
                                //or
                                //skip email validation

                                //send customer welcome message
                                _workflowMessageService.SendCustomerWelcomeMessage(currentCustomer, _workContext.WorkingLanguage.Id);

                                //result
                                return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredStandard);
                            }
                            else if (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation)
                            {
                                //email validation message
                                _genericAttributeService.SaveAttribute(currentCustomer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                _workflowMessageService.SendCustomerEmailValidationMessage(currentCustomer, _workContext.WorkingLanguage.Id);

                                //result
                                return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredEmailValidation);
                            }
                            else if (_customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval)
                            {
                                //result
                                return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredAdminApproval);
                            }
                        }
                        else
                        {
                            // ExternalAuthorizerHelper.RemoveParameters();

                            var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                            foreach (var error in registrationResult.Errors)
                                result.AddError(string.Format(error));
                            return result;
                        }
                    
                 

                    #endregion
                }
                else if (RegistrationIsEnabled())
                {
                    return new AuthorizationResult(OpenAuthenticationStatus.AssociateOnLogon);
                }
                else
                {
                    //ExternalAuthorizerHelper.RemoveParameters();

                    var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                    result.AddError("Registration is disabled");
                    return result;
                }
            }
            if (userFound == null)
            {
                _openAuthenticationService.AssociateExternalAccountWithUser(userLoggedIn, parameters);
            }

            //migrate shopping cart
            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, userFound ?? userLoggedIn, true);
            //authenticate
            _authenticationService.SignIn(userFound ?? userLoggedIn, false);
            //raise event       
            _eventPublisher.Publish(new CustomerLoggedinEvent(userFound ?? userLoggedIn));
            //activity log
            _customerActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"),
                userFound ?? userLoggedIn);

            return new AuthorizationResult(OpenAuthenticationStatus.Authenticated);
        }
        private bool AccountDoesNotExistAndUserIsNotLoggedOn(Customer userFound, Customer userLoggedIn)
        {
            return userFound == null && userLoggedIn == null;
        }
        private bool AccountAlreadyExists(Customer userFound, Customer userLoggedIn)
        {
            return userFound != null && userLoggedIn != null;
        }

        private bool AutoRegistrationIsEnabled()
        {
            return _customerSettings.UserRegistrationType != UserRegistrationType.Disabled && _externalAuthenticationSettings.AutoRegisterEnabled;
        }
        private bool RegistrationIsEnabled()
        {
            return _customerSettings.UserRegistrationType != UserRegistrationType.Disabled && !_externalAuthenticationSettings.AutoRegisterEnabled;
        }

        private bool AccountIsAssignedToLoggedOnAccount(Customer userFound, Customer userLoggedIn)
        {
            return userFound.Id.Equals(userLoggedIn.Id);
        }

        public ExternalAuthenticationRecord GetExternalAuthUser(string ProviderSytem, string Email)
        {
            if (string.IsNullOrEmpty(ProviderSytem)||string.IsNullOrEmpty(Email))
                throw new ArgumentNullException(nameof(ProviderSytem));

            var AuthenticationRecord = _externalAuthenticationRecordRepository.Table.FirstOrDefault(record =>
            record.Email.Equals(Email) && record.ProviderSystemName.Equals(ProviderSytem));

            return AuthenticationRecord;
        }
        #endregion
    }
}