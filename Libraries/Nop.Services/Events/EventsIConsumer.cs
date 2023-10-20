using Nop.Services.Events;
using System;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Orders;
using Nop.Services.Logging;
using Nop.Core.Domain.Sms;
using Nop.Services.Common;
using Nop.Services.Localization;

namespace Nop.Services.Events
{
    public class EventsIConsumer : IConsumer<CustomerRegisteredEvent>
    {
        private readonly IWorkContext _workContext;
        private readonly ISmsService _smsService;
        private readonly ISmsTemplateService _smsTemplateService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;

        public EventsIConsumer()
        {
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _smsService = EngineContext.Current.Resolve<ISmsService>();
            _smsTemplateService = EngineContext.Current.Resolve<ISmsTemplateService>();
            _storeContext = EngineContext.Current.Resolve<IStoreContext>();
            _logger = EngineContext.Current.Resolve<ILogger>();
            _genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        }

        //customer registration
        public void HandleEvent(CustomerRegisteredEvent eventMessage)
        {
            Customer customer = eventMessage.Customer;
            string message = "";
            //don't send welcome message if customer's sms verification is pending through mobile registration
            if (!eventMessage.IsAproved)
            {
                string randomNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.AccountSmsActivationToken);
                if (string.IsNullOrWhiteSpace(randomNumber))
                {
                    _logger.Information("Couldn't find random number.", null, _workContext.CurrentCustomer);
                    return;
                }

                //Your {0} activation code is {1} 
                SmsTemplate smsTemplate = _smsTemplateService.GetSmsTemplateByName("Customer.SmsVerificationMessage.SMS", _storeContext.CurrentStore.Id);
                //no template found
                if (smsTemplate == null)
                {
                    _logger.Information("Couldn't find sms template.", null, _workContext.CurrentCustomer);
                    return;
                }

                //ensure it's active
                var isActive = smsTemplate.IsActive;
                if (!isActive)
                {
                    _logger.Information("Sms template isn't active.", null, _workContext.CurrentCustomer);
                    return;

                }

                message = _localizationService.GetLocalized(smsTemplate,x => x.Message);
                message = String.Format(message, _localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name), randomNumber);
            }
            else
            {
                //Welcome to {0}. Thanks for choosing our eCommerce store for shopping.
                SmsTemplate smsTemplate = _smsTemplateService.GetSmsTemplateByName("Customer.WelcomeMessage.SMS", _storeContext.CurrentStore.Id);
                //no template found
                if (smsTemplate == null)
                    return;

                //ensure it's active
                var isActive = smsTemplate.IsActive;
                if (!isActive)
                    return;

                message = _localizationService.GetLocalized(smsTemplate,x => x.Message);
                message = String.Format(message, _localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name));
            }

            _smsService.SendSms(message, customer);
        }

    }
}
