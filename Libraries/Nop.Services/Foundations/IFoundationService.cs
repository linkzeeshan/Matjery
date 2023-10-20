using Nop.Core;
using Nop.Core.Domain.Foundations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Foundations
{
    public partial interface IFoundationService
    {
        /// <summary>
        /// Delete a Foundation
        /// </summary>
        /// <param name="foundation"></param>
        void DeleteFoundation(Foundation foundation);
        /// <summary>
        /// Get All Foundation
        /// </summary>
        /// <param name="Foundation name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        IPagedList<Foundation> GetAllFoundations(string name = "", string phoneNumber = "",
           int pageIndex = 0, int pageSize = int.MaxValue, bool showDeleted = false);
        /// <summary>
        /// Get Foundation by Id
        /// </summary>
        /// <param name="foundationid"></param>
        /// <returns></returns>
        Foundation GetFoundationById(int foundationid);
        /// <summary>
        /// Inserts a foundation
        /// </summary>
        /// <param name="foundation">foundation</param>
        void InsertFoundation(Foundation foundation);

        /// <summary>
        /// Updates the foundation
        /// </summary>
        /// <param name="foundation">foundation</param>
        void UpdateFoundation(Foundation foundation);
    }
}
