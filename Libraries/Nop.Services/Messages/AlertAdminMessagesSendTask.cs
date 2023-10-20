using Microsoft.Extensions.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Messages
{
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class AlertAdminMessagesSendTask : IScheduleTask
    {
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IOrderService _orderService;
        private readonly IVendorService _vendorService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        string trsl;
        string OrdId;
        string ordDate;
        public AlertAdminMessagesSendTask(IEmailSender emailSender,
            ILogger logger,
            IOrderService orderService,
            IVendorService vendorService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService)
        {

            this._emailSender = emailSender;
            this._logger = logger;
            this._orderService = orderService;
            this._vendorService = vendorService;
            this._messageTemplateService = messageTemplateService;
            this._emailAccountService = emailAccountService;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual void Execute()
        {

            var NotExecuteedOrder = _orderService.GetNotExcutedOrders();

            if (NotExecuteedOrder.Count > 0)
            {
                List<VendorsNotExcuteOrders> vendList = new List<VendorsNotExcuteOrders>();
                foreach (var item in NotExecuteedOrder)
                {
                    var VeID = item.OrderItems.Select(p => p.Product.VendorId).FirstOrDefault();

                    var existvendor = vendList.Find(d => d.venId == VeID);

                    if (existvendor != null)
                    {
                        existvendor.ordId = existvendor.ordId + "<br />" + item.Id.ToString();
                        existvendor.dt = existvendor.dt + "<br />" + item.CreatedOnUtc.ToString();

                    }
                    else
                    {
                        VendorsNotExcuteOrders vend = new VendorsNotExcuteOrders();
                        var vendoData = _vendorService.GetVendorById(VeID);
                        vend.venId = VeID;
                        if (VeID != 0)
                        {
                            vend.name = vendoData.Name;
                            vend.email = vendoData.Email;
                            vend.phone = vendoData.PhoneNumber != null ? vendoData.PhoneNumber : "No number";
                        }
                        else
                        {
                            vend.name = "No Vendor";
                        }
                        vend.ordId = item.Id.ToString();
                        vend.dt = item.CreatedOnUtc.ToString();
                        vendList.Add(vend);
                    }

                }

                foreach (var vel in vendList)
                {
                    trsl = trsl + "<tr>" +
                             "<td>" + vel.name + "</td>" +
                             "<td>" + vel.email + "</td>" +
                              "<td>" + vel.phone + "</td>" +
                              "<td>" + vel.ordId + "</td>" +
                              "<td>" + vel.dt + "</td>" +
                                                   "</tr>";

                }
                var dd = trsl;
                var Temp = _messageTemplateService.GetMessageTemplatesByName("OrderNotExtcuted.AlertAdmin", 0).FirstOrDefault();
                var body = string.Format(Temp.Body, trsl);
                var emailAccount = _emailAccountService.GetEmailAccountById(Temp.EmailAccountId);
                var toemail = _emailAccountService.GetEmailAccountById(5);
                try
                {

                    _emailSender.SendEmail(emailAccount,
                        Temp.Subject,
                       body, emailAccount.Email,//sender 
                       emailAccount.Username,
                       toemail.Email,// receiver 
                       toemail.Username
                      );

                }
                catch (Exception exc)
                {
                    _logger.LogError(string.Format("Error sending e-mail. {0}", exc.Message), exc);
                }

            }
        }
    }
    public class VendorsNotExcuteOrders
    {
        public int venId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string ordId { get; set; }
        public string dt { get; set; }

    }

}
