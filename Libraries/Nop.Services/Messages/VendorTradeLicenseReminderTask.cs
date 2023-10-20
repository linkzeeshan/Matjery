using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using Nop.Services.Logging;
using Nop.Services.Vendors;
using Nop.Core.Domain.Vendors;
using System.Linq;
using Nop.Core;
using Nop.Services.Common;

namespace Nop.Services.Messages
{
    public class VendorTradeLicenseReminderTask : IScheduleTask
    {
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IVendorService _vendorService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IGenericAttributeService _genericAttributeService;

        public VendorTradeLicenseReminderTask(IEmailAccountService emailAccountService,
         IEmailSender emailSender,
         IStoreContext storeContext,
         IMessageTemplateService messageTemplateService,
         ILogger logger,
         IGenericAttributeService genericAttributeService,
         IWorkflowMessageService workflowMessageService,
         IVendorService vendorService,
        IQueuedEmailService queuedEmailService)
        {
            _genericAttributeService = genericAttributeService;
            _workflowMessageService = workflowMessageService;
            _emailAccountService = emailAccountService;
            _storeContext = storeContext;
            _emailSender = emailSender;
            _logger = logger;
            _queuedEmailService = queuedEmailService;
            _vendorService = vendorService;
            _messageTemplateService = messageTemplateService;
        }
        public virtual void Execute()
        {
            var vendors = _vendorService.GetLicenseExpiredVendors();
            foreach (var vendor in vendors)
            {
                var TradelicenseStatus = _vendorService.GetVendorLicenseStatus(vendor);
                string ExpiryDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeExpiryDate);
                if (TradelicenseStatus.Id == (int)VendorLicenseStatus.Expired)
                {
                    var res=_workflowMessageService.SendTradeLicenseExpiryNotificationToVendor(vendor, 0, false, ExpiryDate, "Vendor.LicenseExpired");
                    if (res > 0)
                    {
                        vendor.Active = false;
                        _vendorService.UpdateVendor(vendor);
                    }

                }
                if (TradelicenseStatus.Id == (int)VendorLicenseStatus.AboutToExpire)
                {
                    _workflowMessageService.SendTradeLicenseExpiryNotificationToVendor(vendor, 0, false, ExpiryDate, "Vendor.liceseExpiryReminder");
                }
            }
        }
    }
}
