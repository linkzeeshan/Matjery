using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services.QueuedPushNotificationTask;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Messages
{
    public interface IQueuedPushNotificationService
    {
        void DeleteAllPushNotifications();
        IList<QueuedPushNotification> GetQueuedPushNotificationByIds(int[] queuedPushNotificationIds);
        QueuedPushNotification GetQueuedPushNotificationById(int queuedPushNotificationId);
        void DeleteQueuedPushNotification(IList<QueuedPushNotification> queuedPushNotifications);
        void DeleteQueuedPushNotification(QueuedPushNotification queuedPushNotification);
        void UpdatePushNotificationQueue(QueuedPushNotification pushNotificationQueue);
        void QueueVendorAddProductNotification(Vendor vendor, Product product, Customer customer, string registerationId);

        void QueueOrderPlaceCustomerNotification(Order order);

        void QueueOrderPlaceVendorNotification(Order order, Vendor vendor);

        void QueueOrderCompletedCustomerNotification(Order order);
        void QueueOrderCustomerNotification(Order order, string templateType = "");

        void QueueNewCampaignCustomerNotification(Campaign campaign);

        ResponseModel SendPushNotification(QueuedPushNotification queuedPushNotification);
        void QueuePushNotificationData(string title, string message, string registerationId = "", string email = "", string extraData = "",Customer customer=null);
        void InsertPushNotificationQueue(QueuedPushNotification pushNotificationQueue);
        IPagedList<QueuedPushNotification> SearchPushNotifications(bool loadNotSentItemsOnly, int maxSendTries, bool loadNewest, int? pushNotificationUserStatus = null, string registerationId = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<QueuedPushNotification> SearchPushNotifications(bool loadNotSentItemsOnly, int maxSendTries);
    }
}
