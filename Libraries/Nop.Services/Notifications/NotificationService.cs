using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Notifications;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Notifications
{
    public partial class NotificationService : INotificationService
    {
        #region Fields

        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customerService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="notificationRepository"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="localizedPropertyRepository"></param>
        public NotificationService(
            IRepository<Notification> notificationRepository,
            IEventPublisher eventPublisher,
            ICustomerService customerService,
            IQueuedEmailService queuedEmailService,
            ILocalizationService LocalizationService,
           IRepository<LocalizedProperty> localizedPropertyRepository)
        {
            this._notificationRepository = notificationRepository;
            this._localizedPropertyRepository = localizedPropertyRepository;
            this._eventPublisher = eventPublisher;
            this._customerService = customerService;
            this._queuedEmailService = queuedEmailService;
            _localizationService = LocalizationService;
        }

        #endregion


        public void Delete(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException("notification");
            _notificationRepository.Delete(notification);
            //event notification
            _eventPublisher.EntityDeleted(notification);
        }

        public Notification GetById(int Id)
        {
            if (Id == 0)
                return null;

            return _notificationRepository.GetById(Id); throw new NotImplementedException();
        }

        public void Insert(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException("notification");

            _notificationRepository.Insert(notification);

            //event notification
            _eventPublisher.EntityInserted(notification);
        }

        public IPagedList<Notification> Search(int type = 0, int pageIndex = 0, int pageSize = int.MaxValue, int languageId = 0)
        {
            var query = _notificationRepository.Table;

            query = from q in query
                    join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Notification" select lp)
                    on q.Id equals lpj.EntityId
                     into join1
                    from j in join1.DefaultIfEmpty()
                        //where j.LanguageId == languageId
                    select q;

            if (type > 0)
                query = query.Where(x => x.Type == type);

            query = from c in query
                    group c by c.Id
                       into cGroup
                    orderby cGroup.Key
                    select cGroup.FirstOrDefault();


            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return new PagedList<Notification>(query, pageIndex, pageSize);
        }

        public IList<Notification> Search(int type = 0,  int languageId = 0)
        {
            var query = _notificationRepository.Table;

            query = from q in query
                    join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Notification" select lp)
                    on q.Id equals lpj.EntityId
                     into join1
                    from j in join1.DefaultIfEmpty()
                        //where j.LanguageId == languageId
                    select q;

            if (type > 0)
                query = query.Where(x => x.Type == type);

            query = from c in query
                    group c by c.Id
                       into cGroup
                    orderby cGroup.Key
                    select cGroup.FirstOrDefault();


            //query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return query.ToList();
            //return new PagedList<Notification>(query, pageIndex, pageSize);
        }
        public void Update(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException("notification");

            _notificationRepository.Update(notification);
            //event notification
            _eventPublisher.EntityUpdated(notification);
        }

        public int SendEmailNotification(Notification notification, EmailAccount emailAccount, IEnumerable<NewsLetterSubscription> subscriptions)
        {
            if (notification == null)
                throw new ArgumentNullException("notification");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            int totalEmailsSent = 0;

            foreach (var subscription in subscriptions)
            {
                var customer = _customerService.GetCustomerByEmail(subscription.Email);
                //ignore deleted or inactive customers when sending newsletter campaigns
                if (customer != null && (!customer.Active || customer.Deleted))
                    continue;

                string subject = _localizationService.GetLocalized(notification,t => t.Title);
                string body = _localizationService.GetLocalized(notification,t => t.Message);

                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.Low,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = subscription.Email,
                    Subject = subject,
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id,
                    DontSendBeforeDateUtc = (DateTime?)null
                };
                _queuedEmailService.InsertQueuedEmail(email);
                totalEmailsSent++;
            }
            return totalEmailsSent;
        }
    }
}
