using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Notifications
{
    public partial interface INotificationService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Notification GetById(int Id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        void Delete(Notification notification);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<Notification> Search(
            int type = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            int languageId = 0);
        IList<Notification> Search(
            int type = 0,
            int languageId = 0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        void Insert(Notification notification);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        void Update(Notification notification);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="emailAccount"></param>
        /// <param name="subscriptions"></param>
        /// <returns></returns>
        int SendEmailNotification(Notification notification, EmailAccount emailAccount,
            IEnumerable<NewsLetterSubscription> subscriptions);
    }
}
