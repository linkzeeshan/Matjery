using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Constant;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Constants
{
    public partial class ConstantService : IConstantService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : constant ID
        /// </remarks>
        private const string CONSTANTS_BY_ID_KEY = "Nop.constant.id-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Constant> _constantRepository;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ConstantService(IStaticCacheManager cacheManager, IRepository<Constant> constantRepository,
            IWorkContext workContext)
        {
            this._cacheManager = cacheManager;
            _constantRepository = constantRepository;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual void DeleteConstant(Constant constant)
        {
            if (constant == null)
                throw new ArgumentNullException("constant");

            constant.IsDeleted = true;
            UpdateConstant(constant);
        }

        public virtual IPagedList<Constant> GetAllConstants(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _constantRepository.Table;
            query = query.Where(c => !c.IsDeleted);

            if (_workContext.WorkingLanguage.Rtl)
            {
                query = query.OrderBy(c => c.ArabicName);
            }
            else
            {
                query = query.OrderBy(c => c.EnglishName);
            }

            //List<Constant> constants = query.ToList();

            //paging
            return new PagedList<Constant>(query, pageIndex, pageSize);

        }

        /// <summary>
        /// Gets a constant
        /// </summary>
        /// <param name="constantId">Constant identifier</param>
        /// <returns>Constant</returns>
        public virtual Constant GetConstantById(int constantId)
        {
            if (constantId == 0)
                return null;
            CacheKey key = new CacheKey(CONSTANTS_BY_ID_KEY, constantId); //added as per new customization to 4.3
            //string key =  string.Format(CONSTANTS_BY_ID_KEY, constantId);
            return _cacheManager.Get(key, () => _constantRepository.GetById(constantId));
        }

        /// <summary>
        /// Inserts constant
        /// </summary>
        /// <param name="constant">Constant</param>
        public virtual void InsertConstant(Constant constant)
        {
            if (constant == null)
                throw new ArgumentNullException("constant");

            _constantRepository.Insert(constant);
        }

        /// <summary>
        /// Updates the constant
        /// </summary>
        /// <param name="constant">Constant</param>
        public virtual void UpdateConstant(Constant constant)
        {
            if (constant == null)
                throw new ArgumentNullException("constant");

            _constantRepository.Update(constant);
        }

        #endregion
    }
}
