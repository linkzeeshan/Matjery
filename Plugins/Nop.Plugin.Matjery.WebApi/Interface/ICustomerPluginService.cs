using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface ICustomerPluginService
    {
        bool ValidateCustomer(ParamsModel.LoginParamsModel model);
        LoginGuestResult GetGuestCustomer();
        (LoginResult, ApiValidationResultResponse) Login(ParamsModel.LoginParamsModel model);
        (LoginResult, ApiValidationResultResponse) LoginExternal(ParamsModel.LoginExternalParamsModel model);
        CustomerInfoResult GetCustomerInfo();
        (RegistrationResult, ApiValidationResultResponse) Register(ParamsModel.RegistrationParamsModel model);
        (bool, ApiValidationResultResponse) ActivateAccountBySms_Latest(ParamsModel.AccountActivationParamsModel model);
        (bool, ApiValidationResultResponse) ActivateAccountBySms(ParamsModel.AccountActivationParamsModel model);
        bool ResendActivationToken(ParamsModel.AccountActivationParamsModel model);
        (PasswordRecoveryResult, ApiValidationResultResponse) PasswordRecoverySend(ParamsModel.PasswordRecoveryParamsModel model);
        ChangePasswordResult ChangePassword(ParamsModel.ChangePasswordParamsModel model);
        (CustomerInfoResult.CustomerInfoEditResult, ApiValidationResultResponse) SaveCustomerInfo(ParamsModel.CustomerInfoParamsModel model);
        (Models.DeleteCustomerAccountResult, ApiValidationResultResponse) DeleteCustomerAccountRequest(ParamsModel.DeleteCustomerAccountParamsModel model);
        bool UpdateTermsAndConditions(ParamsModel.CustomerTermsAndConditionsParamsModel model);
        (RegistrationResult, ApiValidationResultResponse) SendSmsCode(ParamsModel.RegistrationParamsModel model);
    }
}
