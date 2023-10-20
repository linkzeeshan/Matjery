using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Lookups;

namespace Nop.Services.lookup
{
    /// <summary>
    /// lookup service interface
    /// </summary>
    public partial interface ILookupService
    {
        #region lookup

        /// <summary>
        /// Deletes a lookup
        /// </summary>
        /// <param name="Lookup">lookup item</param>
        void Deletelookup(Lookup lookup);

        /// <summary>
        /// Gets a lookup
        /// </summary>
        /// <param name="lookupId">The lookup identifier</param>
        /// <returns>lookup</returns>
        Lookup GetlookupById(int lookupId, int language = 0, string type="");

        /// <summary>
        /// Gets lookup
        /// </summary>
        /// <param name="lookupIds">The lookup identifiers</param>
        /// <returns>lookup</returns>
        IList<Lookup> GetlookupByIds(int[] lookupIds, int language = 0);

        /// <summary>
        /// Gets lookup
        /// </summary>
        /// <param name="lookupIds">The lookup identifiers</param>
        /// <returns>lookup</returns>
        IList<Lookup> GetlookupByLanguageId(int langugaeId);

        /// <summary>
        /// Gets lookup
        /// </summary>
        /// <returns>lookup</returns>
        List<Lookup> GetAllLookups(string[] types, int languageId = 2);

        /// <summary>
        /// Gets lookup
        /// </summary>
        /// <param name="type">The lookup identifiers</param>
        /// <returns>lookup</returns>
        IList<Lookup> GetAllLookupsBytype(string[] type, int languageId = 0);

        /// <summary>
        /// Gets all lookup
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="type">Filter by lookup item title</param>
        /// <returns>lookup items</returns>
        IPagedList<Lookup> GetAllLookup(int languageId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string type = null);

        /// <summary>
        /// Inserts a lookup item
        /// </summary>
        /// <param name="lookup">lookup item</param>
        void Insertlookup(Lookup lookup);

        /// <summary>
        /// Updates the lookup item
        /// </summary>
        /// <param name="lookup">lookup item</param>
        void Updatelookup(Lookup lookup);


        #endregion
    }
}