using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Customers;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class CustomerPluginService : BasePluginService, ICustomerPluginService
    {
        public LoginGuestResult GetGuestCustomer()
        {
            Customer guestCustomer = _customerService.InsertGuestCustomer();
            var loginAsGuestResult = new LoginGuestResult();
            loginAsGuestResult.Token = guestCustomer.CustomerGuid.ToString();
            loginAsGuestResult.AvatarUrl = _pictureService.GetPictureUrl(_genericAttributeService.GetAttribute<int>(guestCustomer, NopCustomerDefaults.AvatarPictureIdAttribute),
                    _mediaSettings.AvatarPictureSize, storeLocation: _storeContext.CurrentStore.Url);
            loginAsGuestResult.FormattedUserName = _customerService.FormatUsername(guestCustomer);
            return loginAsGuestResult;
        }

        public (LoginResult, ApiValidationResultResponse) Login(ParamsModel.LoginParamsModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UsernameOrEmail))
                throw new ArgumentNullException(nameof(model.UsernameOrEmail));

            if (string.IsNullOrWhiteSpace(model.UserPassword))
                throw new ArgumentNullException(nameof(model.UserPassword));

            var validationResultEntry = new ApiValidationResultEntryResponse();
            var loginResult = new LoginResult();
            if (_customerSettings.UsernamesEnabled)
            {
                model.UsernameOrEmail = model.UsernameOrEmail.Trim();
            }
            switch (_customerRegistrationService.ValidateCustomer(model.UsernameOrEmail, model.UserPassword, ignorePasswordHash: model.ignoreHashPassword, model.IsExternal))
            {
                case CustomerLoginResults.Successful:
                    {
                        Customer customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(model.UsernameOrEmail)
                            : _customerService.GetCustomerByEmail(model.UsernameOrEmail);
                        if (!string.IsNullOrWhiteSpace(model.GuestToken))
                        {
                            Customer guestCustomer = _customerService.GetCustomerByGuid(Guid.Parse(model.GuestToken));
                            if (guestCustomer != null)
                            {
                                _shoppingCartService.MigrateShoppingCart(guestCustomer, customer, true);
                            }
                        }
                        _authenticationService.SignIn(customer, true);
                        _eventPublisher.Publish<CustomerLoggedinEvent>(new CustomerLoggedinEvent(customer));
                        _customerActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);
                        loginResult.Status = HttpStatusCode.OK;

                        CustomerInfoResult customerInfoResult =GetCustomerInfo(customer);
                        loginResult.Customer = customerInfoResult;
                        break;
                    }
                case CustomerLoginResults.CustomerNotExist:
                    {
                        loginResult.Status = HttpStatusCode.NotFound;
                        loginResult.Message = _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist");
                        validationResultEntry = new ApiValidationResultEntryResponse
                        {
                            errorType = CustomerLoginResults.CustomerNotExist.ToString(),
                            errorDescription = _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist")
                        };
                        break;
                    }
                case CustomerLoginResults.NotActive:
                    {
                        loginResult.Status = HttpStatusCode.NotFound;
                        loginResult.Message = _localizationService.GetResource("Account.Login.WrongCredentials.NotActive");
                        validationResultEntry = new ApiValidationResultEntryResponse
                        {
                            errorType = CustomerLoginResults.NotActive.ToString(),
                            errorDescription = _localizationService.GetResource("Account.Login.WrongCredentials.NotActive")
                        };
                        break;
                    }
                case CustomerLoginResults.Deleted:
                    {
                        loginResult.Status = HttpStatusCode.NotFound;
                        loginResult.Message = _localizationService.GetResource("Account.Login.WrongCredentials.Deleted");
                        validationResultEntry = new ApiValidationResultEntryResponse
                        {
                            errorType = CustomerLoginResults.Deleted.ToString(),
                            errorDescription = _localizationService.GetResource("Account.Login.WrongCredentials.Deleted")
                        };

                        break;
                    }
                case CustomerLoginResults.NotRegistered:
                    {
                        loginResult.Status = HttpStatusCode.NotFound;
                        loginResult.Message = _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered");
                        validationResultEntry = new ApiValidationResultEntryResponse
                        {
                            errorType = CustomerLoginResults.NotRegistered.ToString(),
                            errorDescription = _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered")
                        };
                        break;
                    }
                default:
                    {
                        loginResult.Status = HttpStatusCode.NotFound;
                        loginResult.Message = _localizationService.GetResource("Account.Login.WrongCredentials");
                        validationResultEntry = new ApiValidationResultEntryResponse
                        {
                            errorType = CustomerLoginResults.WrongPassword.ToString(),
                            errorDescription = _localizationService.GetResource("Account.Login.WrongCredentials")
                        };
                        break;
                    }
            }
            var apiValidationResult = new ApiValidationResultResponse();
            if (validationResultEntry.errorDescription != null)
            {
                apiValidationResult.fieldValidationResult.Add(validationResultEntry);
            }
            return (loginResult, apiValidationResult);
        }

        public bool ValidateCustomer(ParamsModel.LoginParamsModel model)
        {

            bool result = true;
            _customerActivityService.InsertActivity("ValidateCustomer", "Customer Token: " + model.UsernameOrEmail);
            Guid customerGuid = Guid.Parse(model.UsernameOrEmail);
            Customer customer = _customerService.GetCustomerByGuid(customerGuid);
            if (customer == null || !customer.Active || customer.Deleted)
                result = false;
            return result;

        }


        private CustomerInfoResult GetCustomerInfo(Customer customer)
        {
            var vendor = _vendorService.GetVendorById(customer.VendorId);
            var customerInfoResult = new CustomerInfoResult()
            {
                Idpk = customer.Id,
                Id = customer.CustomerGuid.ToString(),
                FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute),
                Gender = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.GenderAttribute),
                PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                VendorId =customer.VendorId,
                FullName =_customerService.GetCustomerFullName(customer),
                FormattedUserName = _customerService.FormatUsername(customer),
                Email = customer.Email,
                IsVendor =_customerService.IsVendor(customer) && IsVendorActive(customer.VendorId),
                IsAdmin = _customerService.IsAdmin(customer),
                IsTranslator = _customerService.IsTranslator(customer),
                IsFoundation = _customerService.IsFoundation(customer) && IsFoundationActive(customer.VendorId),
                NationalityCountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.NationalityCountryId),
                HasAcceptedTermsAndConditions = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions),
                IsRequestedforDelete = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute),

                //IsRequestedforDelete = vendor != null ? _genericAttributeService.GetAttribute<bool>(vendor, NopCustomerDefaults.customerDeleteAccountAttribute) :
                //_genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute),
                //VendorId = customer.VendorId
            };
            DateTime? defDate = null;
            customerInfoResult.DateOfBirth = string.IsNullOrEmpty(_genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.DateOfBirthAttribute)) ? defDate : _genericAttributeService.GetAttribute<DateTime>(customer, NopCustomerDefaults.DateOfBirthAttribute);
            if (vendor != null)
            {
                customerInfoResult.VendorId = vendor.FoundationAprovalStatusId == (int)FoundationAprovalStatus.Rejected ? 0 : customer.VendorId;
            }
            else
            {
                customerInfoResult.VendorId = 0;
            }
            DateTime? dateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(customer, NopCustomerDefaults.DateOfBirthAttribute);
            if (dateOfBirth.HasValue)
            {
                customerInfoResult.DateOfBirth = new DateTime(dateOfBirth.Value.Year, dateOfBirth.Value.Month,
                    dateOfBirth.Value.Day, 0, 0, 0, DateTimeKind.Utc);
            }
            customerInfoResult.Username = customer.Email;
            if (_customerSettings.UsernamesEnabled)
            {
                customerInfoResult.Username = customer.Username;
            }
            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                customerInfoResult.AvatarUrl = _pictureService.GetPictureUrl(_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                    _mediaSettings.AvatarPictureSize, storeLocation: _storeContext.CurrentStore.Url);
            }
            var externalProvider = _openAuthenticationService.GetProviderSystemName(customer.Id);
            if (!string.IsNullOrEmpty(externalProvider))
                customerInfoResult.ProviderSystemName = externalProvider;

            return customerInfoResult;
        }

        private bool IsVendorActive(int vendorId)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return false;

            return vendor.Active;
        }

        private bool IsFoundationActive(int vendorId)
        {
            var foundation = _vendorService.GetFoundationById(vendorId);
            if (foundation == null)
                return false;

            return foundation.Active;
        }

        public (LoginResult, ApiValidationResultResponse) LoginExternal(ParamsModel.LoginExternalParamsModel model)
        {
            var apiValidationResult = new ApiValidationResultResponse();
            var loginResult = new LoginResult();
            Customer userFound = _openAuthenticationService.GetUser(model);
            if (model.RegistrationType.ToLower() == "seller" && userFound==null)
            {
                var currentVendor = _vendorService.GetAllVendors(name: model.VendorApplyParamsModel.Name, pageIndex: 1, pageSize: 20, showHidden: false);
                if (currentVendor.TotalCount > 0)
                {
                    if (currentVendor.Where(x => x.Name.ToLower() == model.VendorApplyParamsModel.Name.ToLower()).Any())
                    {
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            fieldName = _localizationService.GetResource("Vendors.ApplyAccount.Name"),
                            errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                        });
                        return (loginResult, apiValidationResult);
                    }
                   
                }
              

                if (!string.IsNullOrEmpty(model.VendorApplyParamsModel.PhoneNumber))
                {
                    currentVendor = _vendorService.GetAllVendors(phoneNumber: model.VendorApplyParamsModel.PhoneNumber, pageIndex: 1, pageSize: 20);
                    if (currentVendor.TotalCount > 0)
                    {
                        if (currentVendor.Where(x => x.PhoneNumber.ToLower() == model.VendorApplyParamsModel.PhoneNumber.ToLower()).Any())
                        {
                            apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                            {
                                fieldName = _localizationService.GetResource("Vendors.ApplyAccount.PhoneNumber"),
                                errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                            });
                            return (loginResult, apiValidationResult);
                        }
                    }
                     
                }

            }

            if (string.IsNullOrWhiteSpace(model.ExternalIdentifier))
                throw new Exception("Authentication result does not contain id data");

            if (string.IsNullOrWhiteSpace(model.OAuthAccessToken))
                throw new Exception("Authentication result does not contain accesstoken data");

            var claim = new UserClaims();
            var claims = new List<ExternalAuthenticationClaim>();
            if (!string.IsNullOrEmpty(model.Email))
            {
                claim.Contact = new ContactClaims
                {
                    Email = model.Email
                };
            }
            if (!string.IsNullOrEmpty(model.Name))
            {
                string[] strArray = model.Name.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                claim.Name = new NameClaims();
                if (strArray.Length >= 2)
                {
                    claim.Name.First = strArray[0];
                    claim.Name.Last = strArray[1];
                    if (strArray.Length > 2)
                        claim.Name.Last = strArray[1] + " " + strArray[2];
                    claim.Name.Last = claim.Name.Last.Trim();
                }
                else
                    claim.Name.First = strArray[0];
            }
            if (!string.IsNullOrEmpty(model.Gender))
            {
                claim.Person = new PersonClaims
                {
                    Gender = model.Gender
                };
            }
            if (!string.IsNullOrEmpty(model.Nationality))
            {
                claim.Contact.Address = new AddressClaims
                {
                    Country = model.Nationality
                };
            }
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                claim.Contact.Phone = new TelephoneClaims
                {
                    Mobile = model.PhoneNumber
                };
            }
            if (!string.IsNullOrEmpty(model.DateOfBirth))
            {
                DateTime dateOfBirth = DateTime.Parse(model.DateOfBirth, CultureInfo.InvariantCulture);
                claim.BirthDate = new BirthDateClaims
                {
                   DayOfMonth= dateOfBirth.Day,
                   Month= dateOfBirth.Month,
                   Year= dateOfBirth.Year
                };
            }
            model.UserClaims.Add(claim);

           // Customer userFound = _openAuthenticationService.GetUser(model);
      


            if (userFound != null)
            {
                //if user is deleted, bring it back! so user can login in next step!
                if (userFound.Deleted)
                    userFound.Deleted = false;
                //information may not be updated...
                //if (model.ProviderSystemName != "ExternalAuth.UaePass")
                //{
                     //_genericAttributeService.SaveAttribute<bool>(userFound, NopCustomerDefaults.customerDeleteAccountAttribute, false);

                    _genericAttributeService.SaveAttribute<string>(userFound, NopCustomerDefaults.FirstNameAttribute, claim.Name.First);
                    _genericAttributeService.SaveAttribute<string>(userFound, NopCustomerDefaults.LastNameAttribute, claim.Name.Last);
                    if (claim.Person!=null)
                    {
                        if (!string.IsNullOrEmpty(claim.Person.Gender))
                        {
                            _genericAttributeService.SaveAttribute<string>(userFound, NopCustomerDefaults.GenderAttribute, claim.Person.Gender);
                        }
                    }
                    if (claim.Contact.Address!=null)
                    {
                        if (!string.IsNullOrEmpty(claim.Contact.Address.Country))
                        {
                            var country = _countryService.GetAllCountries().Where(x => x.ThreeLetterIsoCode.Equals(claim.Contact.Address.Country.Trim())).FirstOrDefault();
                            if (country != null)
                                _genericAttributeService.SaveAttribute<int>(userFound, NopCustomerDefaults.NationalityCountryId, country.Id);
                        }
                    }
                    if (claim.Contact!=null)
                    {
                        if (claim.Contact.Phone!=null)
                        {
                            if (string.IsNullOrEmpty(claim.Contact.Phone.Mobile))
                            {
                                _genericAttributeService.SaveAttribute<string>(userFound, NopCustomerDefaults.PhoneAttribute, claim.Contact.Phone.Mobile);
                            }
                        }
                    }
                    if (claim.BirthDate!=null)
                    {
                        _genericAttributeService.SaveAttribute<DateTime>(userFound, NopCustomerDefaults.DateOfBirthAttribute, claim.BirthDate.GeneratedBirthDate);
                    }
        
                //}
                //immediatly login user
                var loginParamsModel = new ParamsModel.LoginParamsModel
                {
                    //current customer is guest at this point set by ParseAuthenticationFilter attribute. so let's retrieve it from there
                    GuestToken = _workContext.CurrentCustomer.CustomerGuid.ToString(),
                    UsernameOrEmail = userFound.Username,
                    UserPassword = _customerService.GetCurrentPassword(userFound.Id).Password,
                    ignoreHashPassword =true ,
                    IsExternal=true
                };
                return Login(loginParamsModel);
            }
            else
            {

                AuthorizationResult authorizationResult = this._externalAuthorizer.Authorize(model);
                var authorizeState = new AuthorizeState("", authorizationResult);

                switch (authorizeState.AuthenticationStatus)
                {
                    case OpenAuthenticationStatus.Authenticated:
                    case OpenAuthenticationStatus.AssociateOnLogon:
                    case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    case OpenAuthenticationStatus.AutoRegisteredStandard:
                        var customer = _workContext.CurrentCustomer;
                        _customerService.UpdateCustomer(customer);
                        loginResult.Status = HttpStatusCode.OK;
                        loginResult.Customer = this.GetCustomerInfo(customer);
                        if (_customerSettings.UsernamesEnabled)
                        {
                            loginResult.Customer.Username = customer.Username;
                        }
                        if (_customerSettings.AllowCustomersToUploadAvatars)
                        {
                            loginResult.Customer.AvatarUrl = _pictureService.GetPictureUrl(_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                                _mediaSettings.AvatarPictureSize, storeLocation: _storeContext.CurrentStore.Url);
                        }
                        loginResult.Message = "";
                        break;
                    case OpenAuthenticationStatus.Error:
                        {
                            if (!authorizeState.Success)
                                foreach (var error in authorizeState.Errors)
                                {
                                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                                    {
                                        fieldName = "",
                                        errorDescription = error
                                    });
                                }
                            break;
                        }
                    
                    default:
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            fieldName = "",
                            errorDescription = "Unknown error"
                        });
                        break;
                }
            }
            return (loginResult, apiValidationResult);
        }

        public CustomerInfoResult GetCustomerInfo()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            Customer customer = _workContext.CurrentCustomer;
            return this.GetCustomerInfo(customer);
        }


        public (RegistrationResult, ApiValidationResultResponse) Register(ParamsModel.RegistrationParamsModel model)
        {
            var apiValidationResult = new ApiValidationResultResponse();
            var registrationResult = new RegistrationResult();
            Customer newCustomer = null;
            //Already registered customer 
            if (_customerService.IsRegistered(_workContext.CurrentCustomer))
                _authenticationService.SignOut();

            if (!string.IsNullOrWhiteSpace(model.GuestToken))
                newCustomer = _customerService.GetCustomerByGuid(Guid.Parse(model.GuestToken));
            if (newCustomer == null)
                newCustomer = _customerService.InsertGuestCustomer();
            //uae pass
            if (model.Provider == "ExternalAuth.UaePass")
                model.Password = CommonHelper.GenerateRandomDigitCode(20);

            bool isApproved = false;
            CustomerRegistrationRequest registrationRequest = new CustomerRegistrationRequest(newCustomer, model.Email,
              _customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password, _customerSettings.DefaultPasswordFormat,
              _storeContext.CurrentStore.Id, isApproved, registrationType: model.RegistrationType,licenseNumber:model.VendorApplyParamsModel.TradeLicenseNumber);
            registrationRequest.Phonenumber = model.PhoneNumber;

            #region otp validation
            string smsToken = _genericAttributeService.GetAttribute<string>(newCustomer, NopCustomerDefaults.AccountSmsActivationToken);

            if (!registrationRequest.IsExternal)
            {
                if (string.IsNullOrEmpty(smsToken) || !string.Equals(model.SmsToken, smsToken))
                {
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        fieldName = "",
                        errorDescription = _localizationService.GetResource("Activation.WrongActivationCode", _workContext.WorkingLanguage.Id)
                    });
                    return (new RegistrationResult() { Status = HttpStatusCode.Unauthorized }, apiValidationResult);
                }
            }
           
            #endregion






            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                registrationResult.Status = HttpStatusCode.BadRequest;
                registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.FirstName.Required")));
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.FirstName.Required")),
                    fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.FirstName"))
                });
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                registrationResult.Status = HttpStatusCode.BadRequest;
                registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.LastName.Required")));
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.LastName.Required")),
                    fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.LastName"))
                });
            }
            if (model.Password.Length < _customerSettings.PasswordMinLength)
            {
                registrationResult.Status = HttpStatusCode.BadRequest;
                registrationResult.Messages.Add(_localizationService.GetResource(string.Format(_localizationService.GetResource("Account.Fields.Password.LengthValidation"),
                    _customerSettings.PasswordMinLength)));
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = _localizationService.GetResource(string.Format(_localizationService.GetResource("Account.Fields.Password.LengthValidation"),
                    _customerSettings.PasswordMinLength)),
                    fieldName = _localizationService.GetResource("Account.Fields.Password")
                });
            }

            if (model.RegistrationType.ToLower() == "seller")
            {
                var currentVendor = _vendorService.GetAllVendors(name: model.VendorApplyParamsModel.Name, pageIndex: 1, pageSize: 20, showHidden: false);
                if (currentVendor.TotalCount > 0)
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        fieldName = _localizationService.GetResource("Vendors.ApplyAccount.Name"),
                        errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                    });
                if (!string.IsNullOrEmpty(model.VendorApplyParamsModel.TradeLicenseNumber))
                {
                    if (_vendorService.GetAllVendors(pageIndex: 1, pageSize: int.MaxValue, showHidden: false).Where(x => x.LicenseNo == model.VendorApplyParamsModel.TradeLicenseNumber).Any())
                    {
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            fieldName = _localizationService.GetResource("account.licensenumber"),
                            errorDescription = _localizationService.GetResource("Address.Fields.vendor.license.AlreadyExist")
                        });
                    }
                }
                 
                    if (!string.IsNullOrEmpty(model.VendorApplyParamsModel.PhoneNumber))
                {
                    currentVendor = _vendorService.GetAllVendors(phoneNumber: model.PhoneNumber, pageIndex: 1, pageSize: 20);
                    if (currentVendor.TotalCount > 0)
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            fieldName = _localizationService.GetResource("Vendors.ApplyAccount.PhoneNumber"),
                            errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                        });
                }

            }



            //don't proceed. immediatly return
            if (apiValidationResult.fieldValidationResult.Any())
                return (registrationResult, apiValidationResult);
            if (_customerSettings.UsernamesEnabled && !string.IsNullOrEmpty(model.Username))
                model.Username = model.Username.Trim();
            //by default to false as sms validation is required on mobile devices
          
          
            CustomerRegistrationResult customerRegistrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
            if (!customerRegistrationResult.Success)
            {
                if (customerRegistrationResult.Errors.Count > 0)
                {
                    registrationResult.Status = HttpStatusCode.NotFound;
                    foreach (string error in customerRegistrationResult.Errors)
                    {
                        registrationResult.Messages.Add(error);
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription = error,
                            fieldName = ""
                        });
                    }
                }
            }
            else
            {
                _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.CityAttribute, model.CityCode);
                _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.Area, model.AreaCode);
                _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.GenderAttribute, "");
                _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                _genericAttributeService.SaveAttribute<bool>(newCustomer, NopCustomerDefaults.HasAcceptedTermsAndConditions, model.HasAcceptedTermsAndConditions);
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                    _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.PhoneAttribute, model.PhoneNumber);
                if (model.CountryId > 0)
                    _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.CountryIdAttribute, model.CountryId.ToString());
                if (model.StateProvinceId > 0)
                    _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId.ToString());

                _genericAttributeService.SaveAttribute<bool>(newCustomer, NopCustomerDefaults.customerDeleteAccountAttribute, false);

                if (_customerSettings.DateOfBirthEnabled)
                {
                    try
                    {
                        DateTime? dateOfBirth;
                        if (model.Provider == "ExternalAuth.UaePass")
                        {
                            var newDate = !string.IsNullOrEmpty(model.DateOfBirth) ? model.DateOfBirth.Split('/') : null;
                            if (newDate != null)
                            {
                                int? day = Convert.ToInt32(newDate[0]);
                                int? month = Convert.ToInt32(newDate[1]);
                                int? year = Convert.ToInt32(newDate[2]);
                                dateOfBirth = new DateTime(year.Value, month.Value, day.Value);
                                _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                            }
                            if (!string.IsNullOrEmpty(model.Nationality))
                            {
                                var country = _countryService.GetCountryByThreeLetterIsoCode(model.Nationality);
                                if (country != null)
                                {
                                    _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.CountryIdAttribute, country.Id);
                                    _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.NationalityCountryId, country.Id);
                                }
                            }
                        }
                        else
                        {
                            dateOfBirth = DateTime.Parse(model.DateOfBirth, CultureInfo.InvariantCulture);
                            _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                        }
                    }
                    catch { }
                }
                //uae pass
                if (!string.IsNullOrEmpty(model.Gender))
                    _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.GenderAttribute, model.Gender);
                if (isApproved)
                    _authenticationService.SignIn(newCustomer, true);
                _customerService.UpdateCustomer(newCustomer);
                //}
                //notifications
                if (_customerSettings.NotifyNewCustomerRegistration)
                    _eventPublisher.Publish(new CustomerRegisteredEvent(newCustomer));
                //_workflowMessageService.SendCustomerRegisteredNotificationMessage(newCustomer, _localizationSettings.DefaultAdminLanguageId);



                //if (string.IsNullOrEmpty(model.SmsToken))
                //{
                //    string randomNumber = RandomNumberService.RandomDigits(6);
                //    _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.AccountSmsActivationToken, randomNumber);
                //}
                //if (model.Provider != "ExternalAuth.UaePass" && string.IsNullOrEmpty(model.SmsToken))
                //    _eventPublisher.Publish<CustomerRegisteredEvent>(new CustomerRegisteredEvent(newCustomer, RegistrationMethod.Mobile, isAproved: false));

                registrationResult.Status = HttpStatusCode.OK;
                CustomerInfoResult customerInfoResult = this.GetCustomerInfo(newCustomer);
                registrationResult.Customer = customerInfoResult;
            }
            //link with uae pass
            if (registrationResult.Status == HttpStatusCode.OK)
            {
                var customer = registrationRequest.Customer;
                if (customer != null)
                {
                    if (!string.IsNullOrEmpty(model.ExternalIdentifier) && 
                        //!string.IsNullOrEmpty(model.Idn) && 
                        !string.IsNullOrEmpty(model.Provider) && model.Provider == Provider.SystemName)
                    {
                        UserClaims claim = new UserClaims();
                        claim.Contact = new ContactClaims();
                        claim.Contact.Email = model.Email;
                        var paramaters = new UaePassAuthenticationParameters(Provider.SystemName)
                        {
                            ExternalIdentifier = model.ExternalIdentifier,
                            ExternalDisplayIdentifier = model.Idn,
                            OAuthAccessToken = model.OAuthAccessToken
                        };
                        paramaters.AddClaim(claim);
                        _openAuthenticationService.AssociateExternalAccountWithUser(customer, paramaters);
                        //in case of uae pass escape the sms validation screen in mobile app
                        registrationResult.Customer.SmsValidated = true;
                        customer.Active = true;
                    }
                    else if (!string.IsNullOrEmpty(model.SmsToken))
                    {
                        registrationResult.Customer.SmsValidated = true;
                        customer.Active = true;
                    }
                    _customerService.UpdateCustomer(customer);
                }
            }

            return (registrationResult, apiValidationResult);
        }




        public (RegistrationResult, ApiValidationResultResponse) SendSmsCode(ParamsModel.RegistrationParamsModel model)
        {
            var apiValidationResult = new ApiValidationResultResponse();
            var registrationResult = new RegistrationResult();
            Customer newCustomer = null;             //Already registered customer 
            if (_customerService.IsRegistered(_workContext.CurrentCustomer))
                _authenticationService.SignOut();

            if (newCustomer == null)
            {
                newCustomer = _customerService.InsertGuestCustomer();
                newCustomer.Email = model.Email;
            }

            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                registrationResult.Status = HttpStatusCode.BadRequest;
                registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.FirstName.Required")));
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.FirstName.Required")),
                    fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.FirstName"))
                });
            }
            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                registrationResult.Status = HttpStatusCode.BadRequest;
                registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.LastName.Required")));
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.LastName.Required")),
                    fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.LastName"))
                });
            }
            if (model.Password.Length < _customerSettings.PasswordMinLength)
            {
                registrationResult.Status = HttpStatusCode.BadRequest;
                registrationResult.Messages.Add(_localizationService.GetResource(string.Format(_localizationService.GetResource("Account.Fields.Password.LengthValidation"),
                _customerSettings.PasswordMinLength)));
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = _localizationService.GetResource(string.Format(_localizationService.GetResource("Account.Fields.Password.LengthValidation"),
                _customerSettings.PasswordMinLength)),
                    fieldName = _localizationService.GetResource("Account.Fields.Password")
                });
            }
            #region validation
            if (_customerSettings.UsernamesEnabled)
            {
                if (String.IsNullOrEmpty(model.Username))
                {
                    registrationResult.Status = HttpStatusCode.BadRequest;
                    registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided")));
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided")),
                        fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"))
                    });
                }
                var isArabic = Regex.IsMatch(model.Username, @"\p{IsArabic}");
                var isSpaces = model.Username.Any(x => Char.IsWhiteSpace(x));
                if (isArabic || isSpaces)
                {

                    registrationResult.Status = HttpStatusCode.BadRequest;
                    registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameEnlgishAndSpaces")));
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameEnlgishAndSpaces")),
                        fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameEnlgishAndSpaces"))
                    });
                }
            }

            ////validate unique user
            if (_customerService.GetCustomerByEmail(model.Email) != null)
            {
                registrationResult.Status = HttpStatusCode.BadRequest;
                registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists")));
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists")),
                    fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.Email"))
                });
            }

                if (_customerSettings.UsernamesEnabled && _customerService.GetCustomerByUsername(model.Username) != null)
                {

                    registrationResult.Status = HttpStatusCode.BadRequest;
                    registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists")));
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists")),
                        fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Account.Fields.Username"))
                    });

                }
            //unique phone number

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var customers = _customerService.GetAllCustomers(phone: model.PhoneNumber).Where(x => !string.IsNullOrEmpty(x.Username)).ToList();
                    if (customers.Count > 0)
                    {
                        registrationResult.Status = HttpStatusCode.BadRequest;
                        registrationResult.Messages.Add(CommonHelper.StripHTML(_localizationService.GetResource("Address.Fields.PhoneNumber.PhonenumberAlreadyExist")));
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription = CommonHelper.StripHTML(_localizationService.GetResource("Address.Fields.PhoneNumber.PhonenumberAlreadyExist")),
                            fieldName = CommonHelper.StripHTML(_localizationService.GetResource("Address.Fields.PhoneNumber.PhonenumberAlreadyExist"))
                        });
                    }
                }

            //vendor

            if (model.RegistrationType.ToLower() == "seller")
            {
                var currentVendor = _vendorService.GetAllVendors(name: model.VendorApplyParamsModel.Name, pageIndex: 1, pageSize: 20, showHidden: false);
                if (currentVendor.TotalCount > 0)
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        fieldName = _localizationService.GetResource("Vendors.ApplyAccount.Name"),
                        errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                    });

                if (!string.IsNullOrEmpty(model.VendorApplyParamsModel.PhoneNumber))
                {
                    currentVendor = _vendorService.GetAllVendors(phoneNumber: model.PhoneNumber, pageIndex: 1, pageSize: 20);
                    if (currentVendor.TotalCount > 0)
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            fieldName = _localizationService.GetResource("Vendors.ApplyAccount.PhoneNumber"),
                            errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                        });
                }
                //if (!string.IsNullOrEmpty(model.VendorApplyParamsModel.ExpiryDate))
                //{
                //    DateTime expiryDate = DateTime.Parse(model.VendorApplyParamsModel.ExpiryDate);
                //    DateTime currentDate = DateTime.Now;

                //    if (expiryDate < currentDate)
                //    {
                //        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                //        {
                //            fieldName = _localizationService.GetResource("vendors.applyaccount.expirydate"),
                //            errorDescription = _localizationService.GetResource("account.register.errors.licenseexpiry")
                //        });
                       
                //    }

                //}

            }




            #endregion
            //don't proceed. immediatly return
            if (apiValidationResult.fieldValidationResult.Any())
                return (registrationResult, apiValidationResult); 

            if (_customerSettings.UsernamesEnabled && !string.IsNullOrEmpty(model.Username))
                model.Username = model.Username.Trim();              //by default to false as sms validation is required on mobile devices
            bool isApproved = false;
            //CustomerRegistrationRequest registrationRequest = new CustomerRegistrationRequest(newCustomer, model.Email,
            //_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password, _customerSettings.DefaultPasswordFormat,
            //_storeContext.CurrentStore.Id, isApproved,categories:null); 
            //registrationRequest.Phonenumber = model.PhoneNumber;



  



            string randomNumber = RandomNumberService.RandomDigits(6);
            if (!string.IsNullOrEmpty(model.PhoneNumber))
                _genericAttributeService.SaveAttribute<string>(newCustomer, NopCustomerDefaults.PhoneAttribute, model.PhoneNumber);

            newCustomer.Email = string.IsNullOrEmpty(newCustomer.Email) ? model.Email : newCustomer.Email;
            _genericAttributeService.SaveAttribute(newCustomer, NopCustomerDefaults.AccountSmsActivationToken, randomNumber);
            _eventPublisher.Publish<CustomerRegisteredEvent>(new CustomerRegisteredEvent(newCustomer, RegistrationMethod.Mobile, isAproved: false)); 
            registrationResult.Status = HttpStatusCode.OK;
            //Customer guestCustomer = _customerService.GetCustomerByGuid(Guid.Parse(model.GuestToken));
            CustomerInfoResult customerInfoResult = this.GetCustomerInfo(newCustomer);
            registrationResult.Customer = customerInfoResult;
            return (registrationResult, apiValidationResult);
        }
       
        public (bool, ApiValidationResultResponse) ActivateAccountBySms_Latest(ParamsModel.AccountActivationParamsModel model)
        {
            var apiValidationResult = new ApiValidationResultResponse();

            if (string.IsNullOrWhiteSpace(model.UserName))
                throw new Exception("Not allowed");

            if (string.IsNullOrWhiteSpace(model.SmsToken))
                throw new Exception("Not allowed");

            Customer customer = _customerService.GetCustomerByUsername(model.UserName);
            if (customer == null)
            {
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    fieldName = "",
                    errorDescription = _localizationService.GetResource("account.login.wrongcredentials.customernotexist", _workContext.WorkingLanguage.Id)
                });
                return (false, apiValidationResult);
            }

            string smsToken = _genericAttributeService.GetAttribute<string>(customer,NopCustomerDefaults.AccountSmsActivationToken);
            if (string.IsNullOrEmpty(smsToken) || !string.Equals(model.SmsToken, smsToken))
            {
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    fieldName = "",
                    errorDescription = _localizationService.GetResource("Activation.WrongActivationCode", _workContext.WorkingLanguage.Id)
                });
                return (false, apiValidationResult);
            }

            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            _workContext.CurrentCustomer = customer;

            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountSmsActivationToken, "");
            _eventPublisher.Publish<CustomerRegisteredEvent>(new CustomerRegisteredEvent(customer));
            //send customer welcome message
            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);
            return (true, apiValidationResult);
        }

        public (bool, ApiValidationResultResponse) ActivateAccountBySms(ParamsModel.AccountActivationParamsModel model)
        {
            var apiValidationResult = new ApiValidationResultResponse();

            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(model.CustomerId))
                customer = _customerService.GetCustomerByGuid(new Guid(model.CustomerId));

            if (!string.IsNullOrWhiteSpace(model.UserName))
                customer = _customerService.GetCustomerByUsername(model.UserName);

            if (string.IsNullOrWhiteSpace(model.SmsToken))
                throw new ApplicationException("Not allowed");

            //Customer customer = _customerService.GetCustomerByGuid(new Guid(model.CustomerId));
            if (customer == null)
                throw new ApplicationException("Not allowed");

            string smsToken = _genericAttributeService.GetAttribute<string>(customer,NopCustomerDefaults.AccountSmsActivationToken);
            if (string.IsNullOrEmpty(smsToken) || !string.Equals(model.SmsToken, smsToken))
            {
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    fieldName = "",
                    errorDescription = _localizationService.GetResource("Activation.WrongActivationCode", _workContext.WorkingLanguage.Id)
                });
                return (false, apiValidationResult);
            }

            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            _workContext.CurrentCustomer = customer;

            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountSmsActivationToken, "");
            _eventPublisher.Publish<CustomerRegisteredEvent>(new CustomerRegisteredEvent(customer));
            //send customer welcome message
            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            return (true, apiValidationResult);

        }

        public bool ResendActivationToken(ParamsModel.AccountActivationParamsModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CustomerId))
                throw new ApplicationException("Not allowed");

            Customer customer = _customerService.GetCustomerByGuid(new Guid(model.CustomerId));
            if (customer == null)
                throw new ApplicationException("Not allowed");

            string randomNumber = RandomNumberService.RandomDigits(6);
            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountSmsActivationToken, randomNumber);

            _workContext.CurrentCustomer = customer;
            _eventPublisher.Publish<CustomerRegisteredEvent>(new CustomerRegisteredEvent(customer, RegistrationMethod.Mobile, isAproved: false));

            return true;
        }

        public (PasswordRecoveryResult, ApiValidationResultResponse) PasswordRecoverySend(ParamsModel.PasswordRecoveryParamsModel model)
        {
            var passwordRecoveryResult = new PasswordRecoveryResult();
            var validationResult = new ApiValidationResultResponse();

            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(model.Phone)
                || !string.IsNullOrWhiteSpace(model.Email))
            {
                if (!string.IsNullOrWhiteSpace(model.Phone))
                {
                    customer = _customerService.GetAllCustomers(phone: model.Phone)
                        .FirstOrDefault();
                }
                else if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    customer = _customerService.GetCustomerByEmail(model.Email);
                }

                if (customer == null || !customer.Active || customer.Deleted)
                {
                    passwordRecoveryResult.Message = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                    passwordRecoveryResult.Status = HttpStatusCode.NotFound;
                    validationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        fieldName = _localizationService.GetResource("Account.Fields.Email"),
                        errorDescription = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound")
                    });
                }
                else
                {
                    Guid passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute, passwordRecoveryToken.ToString());
                    _genericAttributeService.SaveAttribute<DateTime?>(customer, NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, DateTime.UtcNow);
                    _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer, _workContext.WorkingLanguage.Id);
                    passwordRecoveryResult.Message = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                    passwordRecoveryResult.Status = HttpStatusCode.OK;
                }
            }
            else
            {
                passwordRecoveryResult.Status = HttpStatusCode.NotFound;
                passwordRecoveryResult.Message = _localizationService.GetResource("Account.PasswordRecovery.Email.Required");
                validationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    fieldName = _localizationService.GetResource("Account.Fields.Email"),
                    errorDescription = _localizationService.GetResource("Account.PasswordRecovery.Email.Required")
                });
                validationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    fieldName = _localizationService.GetResource("address.fields.phonenumber"),
                    errorDescription = _localizationService.GetResource("address.fields.phonenumber.required")
                });
            }

            return (passwordRecoveryResult, validationResult);
        }

        public Models.ChangePasswordResult ChangePassword(ParamsModel.ChangePasswordParamsModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new UnauthorizedAccessException();

            var changePasswordResult = new Models.ChangePasswordResult();
            var apiValidtionResult = new ApiValidationResult();
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                Customer customer = _workContext.CurrentCustomer;
                var currentPassword= _customerService.GetCurrentPassword(customer.Id);
                switch (currentPassword.PasswordFormat)
                {
                    case PasswordFormat.Clear:
                        {
                            currentPassword.Password = model.NewPassword;
                            break;
                        }
                    case PasswordFormat.Hashed:
                        {
                            string saltKey = _encryptionService.CreateSaltKey(5);
                            currentPassword.PasswordSalt = saltKey;
                            currentPassword.Password = _encryptionService.CreatePasswordHash(model.NewPassword, saltKey, _customerSettings.HashedPasswordFormat);
                            break;
                        }
                    case PasswordFormat.Encrypted:
                        {
                            currentPassword.Password = _encryptionService.EncryptText(model.NewPassword);
                            break;
                        }
                    default:
                        break;
                }
                _customerService.UpdateCustomerPassword(currentPassword);
                changePasswordResult.Status = HttpStatusCode.OK;
                changePasswordResult.Message = _localizationService.GetResource("Account.ChangePassword.Success");
            }
            else
            {
                changePasswordResult.Status = HttpStatusCode.BadRequest;
                changePasswordResult.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordIsNotProvided"));
                apiValidtionResult.FieldValidationResult.Add(new ApiValidationResultEntry
                {
                    ErrorDescription = _localizationService.GetResource("Account.ChangePassword.Errors.PasswordIsNotProvided"),
                    FieldName = "NewPassword"
                });
            }
            return changePasswordResult;
        }

        public (CustomerInfoResult.CustomerInfoEditResult, ApiValidationResultResponse) SaveCustomerInfo(ParamsModel.CustomerInfoParamsModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            Customer customer = _workContext.CurrentCustomer;
            var apiValidationResult = new ApiValidationResultResponse();
            var editCustomerInfoResult = new CustomerInfoResult.CustomerInfoEditResult();
            if (customer.CustomerGuid.ToString().ToLower() == model.CustomerId.ToLower())
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    editCustomerInfoResult.Messages.Add(_localizationService.GetResource("Account.Fields.Email.Required"));
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = _localizationService.GetResource("Account.Fields.Email.Required"),
                        fieldName = _localizationService.GetResource("Account.Fields.Email")
                    });
                }
                if (string.IsNullOrWhiteSpace(model.FirstName))
                {
                    editCustomerInfoResult.Messages.Add(_localizationService.GetResource("Account.Fields.FirstName.Required"));
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = _localizationService.GetResource("Account.Fields.FirstName.Required"),
                        fieldName = _localizationService.GetResource("Account.Fields.FirstName")
                    });
                }
               
                if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames
                    && string.IsNullOrWhiteSpace(model.Username))
                {
                    editCustomerInfoResult.Messages.Add(_localizationService.GetResource("Account.Fields.Username.Required"));
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = _localizationService.GetResource("Account.Fields.Username.Required"),
                        fieldName = _localizationService.GetResource("Account.Fields.Username")
                    });
                }
                if (editCustomerInfoResult.Messages.Count <= 0)
                {
                    try
                    {
                        //username 
                        if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                        {
                            if (!customer.Username.Equals(model.Username, StringComparison.InvariantCultureIgnoreCase))
                            {
                                //change username
                                _customerRegistrationService.SetUsername(customer, model.Username.Trim());
                                //re-authenticate
                                _authenticationService.SignIn(customer, true);
                            }
                        }
                        //email
                        if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change email
                            _customerRegistrationService.SetEmail(customer, model.Email.Trim(),false);
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled)
                            {
                                _authenticationService.SignIn(customer, true);
                            }
                        }
                        //form fields
                        if (_customerSettings.GenderEnabled)
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);

                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.NationalityCountryId, model.NationalityCountryId);
                        _genericAttributeService.SaveAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                        _genericAttributeService.SaveAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                        if (!string.IsNullOrEmpty(model.PhoneNumber))
                        {
                            _genericAttributeService.SaveAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute, model.PhoneNumber);
                        }
                        if (_customerSettings.DateOfBirthEnabled)
                        {
                            try
                            {
                                var dateOfBirth = DateTime.Parse(model.DateOfBirth);
                                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                            }
                            catch { }
                        }
                        editCustomerInfoResult.Status = HttpStatusCode.OK;
                        editCustomerInfoResult.Messages.Add(_localizationService.GetResource("Admin.Configuration.Updated"));
                        editCustomerInfoResult.Customer = this.GetCustomerInfo(customer);

                        if (_customerSettings.UsernamesEnabled)
                        {
                            editCustomerInfoResult.Customer.Username = customer.Username;
                        }
                        if (_customerSettings.AllowCustomersToUploadAvatars)
                        {
                            editCustomerInfoResult.Customer.AvatarUrl = _pictureService.GetPictureUrl(_genericAttributeService.GetAttribute<int>(customer,NopCustomerDefaults.AvatarPictureIdAttribute),
                                _mediaSettings.AvatarPictureSize, storeLocation: _storeContext.CurrentStore.Url);
                        }

                    }
                    catch (Exception ex)
                    {
                        //catch all exception thrown by email _customerRegistrationService when setting email or any other service
                        editCustomerInfoResult.Status = HttpStatusCode.BadRequest;
                        editCustomerInfoResult.Messages.Add(ex.Message);
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription = ex.Message,
                            fieldName = ""
                        });
                    }
                }
                else
                {
                    editCustomerInfoResult.Status = HttpStatusCode.BadRequest;
                }
            }
            else
            {
                editCustomerInfoResult.Status = HttpStatusCode.BadRequest;
                editCustomerInfoResult.Messages.Add("Invalid User");
            }
            return (editCustomerInfoResult, apiValidationResult);
        }
        public bool UpdateTermsAndConditions(ParamsModel.CustomerTermsAndConditionsParamsModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            if (model.HasAcceptedTermsAndConditions)
            {
                Customer customer = _workContext.CurrentCustomer;
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions, true);
            }

            return true;
        }

        public (Models.DeleteCustomerAccountResult, ApiValidationResultResponse) DeleteCustomerAccountRequest(ParamsModel.DeleteCustomerAccountParamsModel model)
        {
            var deleteCustomerAccountResult = new Models.DeleteCustomerAccountResult();
            var apiValidationResult = new ApiValidationResultResponse();
            try
            {
                var customer = _workContext.CurrentCustomer;
                var vendor = _vendorService.GetVendorById(customer.VendorId);


                if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                    throw new UnauthorizedAccessException();

                var reason = _lookupService.GetlookupById(model.ReasonId, _workContext.WorkingLanguage.Id);

                if (model.ReasonId == 5)
                {
                    reason.Value = model.Comments;
                }
               
               if(reason == null)
                {
                    deleteCustomerAccountResult.Status = HttpStatusCode.OK;
                    deleteCustomerAccountResult.Message = _localizationService.GetResource("Reason Id is required");
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {

                        errorDescription = _localizationService.GetResource("Reason Id is required"),
                        fieldName = _localizationService.GetResource("ReasonId")
                    });
                }
                var isDeleteRequestProcessed = false;
                //if (vendor == null)
                //    isDeleteRequestProcessed = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute);
                //else
                //    isDeleteRequestProcessed = _genericAttributeService.GetAttribute<bool>(vendor, NopCustomerDefaults.customerDeleteAccountAttribute);

                isDeleteRequestProcessed = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.customerDeleteAccountAttribute);

                if (isDeleteRequestProcessed)
                {
                    deleteCustomerAccountResult.Status = HttpStatusCode.OK;
                    deleteCustomerAccountResult.Message = _localizationService.GetResource("account.deleteaccountrequest.processed");
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = _localizationService.GetResource("account.deleteaccountrequest.processed"),
                    });
                }
                else
                {
                    if (vendor == null)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountAttribute, true);
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountCommentsAttribute, model.Comments);
                       // _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DeleteAccountType, model.UserType);
                        
                        deleteCustomerAccountResult.Status = HttpStatusCode.OK;

                        _workflowMessageService.SendCustomerAccountDeleteNotification(customer: customer, languageId: _workContext.WorkingLanguage.Id, null, email: customer.Email.ToString()
                    , reason.ToString());
                    }
                    else
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountAttribute, true);
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                        _genericAttributeService.SaveAttribute(vendor, NopCustomerDefaults.customerDeleteAccountReasonAttribute, reason.Value);
                        _genericAttributeService.SaveAttribute(vendor, NopCustomerDefaults.customerDeleteAccountAttribute, true);
                        //_genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DeleteAccountType, model.UserType);
                        deleteCustomerAccountResult.Status = HttpStatusCode.OK;

                        _workflowMessageService.SendVendorAccountDeleteNotification(vendor: vendor, languageId: _workContext.WorkingLanguage.Id, null, email: vendor.Email.ToString(), reason.ToString());
                       
                    }
                    //messge 
                    deleteCustomerAccountResult.Message = _localizationService.GetResource("Account.DeleteAccountRequest.Message");
                    //activity log
                    _customerActivityService.InsertActivity("DeleteCustomerRequest",
                        string.Format(_localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id), customer);

                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.customerDeleteAccountAttribute, true,_storeContext.CurrentStore.Id);
                }

                
            }
            catch (Exception ex)
            {
                //catch all exception thrown by email _customerRegistrationService when setting email or any other service
                deleteCustomerAccountResult.Status = HttpStatusCode.BadRequest;
                deleteCustomerAccountResult.Message = ex.Message;
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription = ex.Message,
                    fieldName = ""
                });
            }

            return (deleteCustomerAccountResult, apiValidationResult);
        }

    }
}
