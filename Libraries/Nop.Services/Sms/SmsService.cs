using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;

using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Core.Domain.Messages;
using System.Linq;
using Nop.Services.Customers;
using Nop.Core.Domain.Sms;

namespace Nop.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger _logger;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly IWorkContext _workContext;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;


        public SmsService(
            ILogger logger,
            IQueuedSmsService queuedSmsService,
            IQueuedEmailService queuedEmailService,
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            IWorkContext workContext, ICustomerService customerService,
            IGenericAttributeService genericAttributeService)
        {
            _logger = logger;
            _queuedSmsService = queuedSmsService;
            _workContext = workContext;
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
        }

        public int SendSms(string message, Customer customer)
        {
            string phoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                //try from billing address
                Address billAddress = _workContext.CurrentCustomer.BillingAddress;
                if (billAddress != null && !String.IsNullOrWhiteSpace(billAddress.PhoneNumber))
                    phoneNumber = billAddress.PhoneNumber;
                else
                {
                    //try from shipping address
                    Address shippingAddress = _workContext.CurrentCustomer.ShippingAddress;
                    if (shippingAddress != null && !String.IsNullOrWhiteSpace(shippingAddress.PhoneNumber))
                        phoneNumber = shippingAddress.PhoneNumber;
                }
            }
            //re-check
            if (String.IsNullOrWhiteSpace(phoneNumber))
            {
                foreach (Address address in _workContext.CurrentCustomer.Addresses)
                {
                    if (!string.IsNullOrWhiteSpace(address.PhoneNumber))
                    {
                        phoneNumber = address.PhoneNumber;
                        break;
                    }
                }
            }
            //re-check
            if (String.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.Information("Couldn't add sms to queue. Customer phone number not found.", null, _workContext.CurrentCustomer);
                return 0;
            }

            var isRtl = _workContext.WorkingLanguage.Rtl;
            var queuedSms = new QueuedSms
            {
                Message = message,
                CreatedOnUtc = DateTime.UtcNow,
                Priority = QueuedSmsPriority.High,
                PhoneNumber = phoneNumber,
                IsRtl = isRtl
            };
            _queuedSmsService.InsertQueuedSms(queuedSms);

            //return queuedSms.Id;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate();

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = customer.Email,
                ToName = _customerService.GetCustomerFullName(customer),
                ReplyTo = customer.Email,
                ReplyToName = _customerService.GetCustomerFullName(customer),
                CC = null,
                Bcc = null,
                Subject = message,
                Body = message,
                AttachmentFilePath = string.Empty,
                AttachmentFileName = string.Empty,
                AttachedDownloadId = 0,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = null
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }
        protected virtual EmailAccount GetEmailAccountOfMessageTemplate()
        {
            var emailAccountId = 0;
            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;

        }
    }
}
