using Nop.Core;
using Nop.Core.Domain.Constant;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Constants
{
    public interface IConstantService
    {
        void DeleteConstant(Constant constant);
        IPagedList<Constant> GetAllConstants(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a constant
        /// </summary>
        /// <param name="constantId">Constant identifier</param>
        /// <returns>Constant</returns>
        Constant GetConstantById(int constantId);

        /// <summary>
        /// Inserts constant
        /// </summary>
        /// <param name="constant">Constant</param>
        void InsertConstant(Constant constant);

        /// <summary>
        /// Updates the constant
        /// </summary>
        /// <param name="constant">Constant</param>
        void UpdateConstant(Constant constant);
    }
}
