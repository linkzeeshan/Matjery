using Nop.Core;
using Nop.Core.Domain.Foundations;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Foundations
{
    public partial class FoundationService : IFoundationService
    {
        #region Fields
        private readonly IRepository<Foundation> _foundationRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion
        #region Ctor
        public FoundationService(IRepository<Foundation> foundationRepository, IRepository<LocalizedProperty> localizedPropertyRepository, IEventPublisher eventPublisher)
        {
            this._foundationRepository = foundationRepository;
            this._localizedPropertyRepository = localizedPropertyRepository;
            this._eventPublisher = eventPublisher;
        }
        #endregion
        #region Method
        /// <summary>
        /// Get Foundation ById
        /// </summary>
        /// <param name="foundationId"></param>
        /// <returns></returns>
        public virtual Foundation GetFoundationById(int foundationId)
        {
            if (foundationId == 0)
                return null;

            return _foundationRepository.GetById(foundationId);
        }
        /// <summary>
        /// Delete Foundation
        /// </summary>
        /// <param name="foundation"></param>
        public void DeleteFoundation(Foundation foundation)
        {
            if (foundation == null)
                throw new ArgumentNullException("foundation");

            foundation.Deleted = true;
            UpdateFoundation(foundation);
        }
        /// <summary>
        /// Gets all Foundation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="languageid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        public IPagedList<Foundation> GetAllFoundations(string name = "", string phoneNumber = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showDeleted = false)
        {
            var query = _foundationRepository.Table;
            if (!String.IsNullOrWhiteSpace(name))
            {
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Foundation" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        where j.LocaleValue.Contains(name) || q.Name.Contains(name)
                        select q;
            }
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                phoneNumber = phoneNumber.Trim();
                query = query.Where(f => f.PhoneNumber == phoneNumber);
            }
            if (showDeleted)
            {
                query = query.Where(v => v.Deleted);
            }
            else
            {
                query = query.Where(v => !v.Deleted);
                query = query.OrderByDescending(v => v.CreatedOnUtc)
                    .ThenBy(v => v.Name);
            }



            var foundations = new PagedList<Foundation>(query, pageIndex, pageSize);
            return foundations;
        }
        /// <summary>
        /// Insert Foundation
        /// </summary>
        /// <param name="foundation"></param>
        public void InsertFoundation(Foundation foundation)
        {
            if (foundation == null)
                throw new ArgumentNullException("foundation");

            _foundationRepository.Insert(foundation);

            //event notification
            _eventPublisher.EntityInserted(foundation);
        }
        /// <summary>
        /// Update Foundation
        /// </summary>
        /// <param name="foundation"></param>
        public void UpdateFoundation(Foundation foundation)
        {
            if (foundation == null)
                throw new ArgumentNullException("foundation");

            _foundationRepository.Update(foundation);

            //event notification
            _eventPublisher.EntityUpdated(foundation);
        }
        #endregion


    }
}
