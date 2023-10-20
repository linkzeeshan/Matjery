using Nop.Core.Domain.BlackPoints;
using Nop.Web.Areas.Admin.Models.BlackPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IBlackPointModelFactory
    {
        /// <summary>
        /// Prepare black point search model
        /// </summary>
        /// <param name="searchModel">black point  search model</param>
        /// <returns>black point  search model</returns>
        BlackPointSearchModel PrepareBlackPointSearchModel(BlackPointSearchModel searchModel);

        /// <summary>
        /// Prepare paged black point list model
        /// </summary>
        /// <param name="searchModel">black point search model</param>
        /// <returns>black point list model</returns>
        BlackPointListModel PrepareBlackPointListModel(BlackPointSearchModel searchModel);

        /// <summary>
        /// Prepare black point model
        /// </summary>
        /// <param name="model">black point model</param>
        /// <param name="customerRole">black point</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>black point model</returns>
        BlackPointModel PrepareCustomerRoleModel(BlackPointModel model, BlackPoint customerRole, bool excludeProperties = false);
    }
}
