using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Customers
{
    public partial interface ICustomerTranslatorService
    {
        /// <summary>
        /// Deletes a customerTranslationMapping
        /// </summary>
        /// <param name="customerTranslationMapping">customerTranslationMapping</param>
        void DeleteCustomerTranslationMapping<T>(T entity, int id) where T : BaseEntity;

        /// <summary>
        /// Gets all CustomerTranslatorService
        /// </summary>
        /// <returns>CustomerTranslatorService</returns>
        IList<CustomerTranslationMapping> GetAllCustomerTranslationMapping();
        /// <summary>
        /// Gets a CustomerTranslatorService
        /// </summary>
        /// <param name="CustomerTranslatorService">CustomerTranslatorService identifier</param>
        /// <returns>CustomerTranslatorService</returns>
        CustomerTranslationMapping GetCustomerTranslatorById(int CustomerTranslatorServiceId);
        CustomerTranslationMapping GetCustomerTranslatorByMapping<T>(T entity) where T : BaseEntity;
        /// <summary>
        /// Gets Customer traslation mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        ///  /// <param name="customerid">Entity</param>
        ///   /// <param name="pageindex">pageindex</param>
        ///    /// <param name="pagesize">pagesize</param>
        /// <returns>Store mapping records</returns>
        IList<CustomerTranslationMapping> GetAllCustomerTranslationByMappings<T>(T entity) where T : BaseEntity;
        /// <summary>
        /// Gets Customer traslation mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Store mapping records</returns>
        IPagedList<CustomerTranslationMapping> GetAllCustomerTranslationListByMappings<T>(T entity, int customerid, int pageIndex = 0, int pageSize = int.MaxValue) where T : BaseEntity;
        IPagedList<Product> GetAllProductCustomerTranslationListByMappings<T>(T entity, int customerid, string produtname, int languageid = 0,
            bool showNotTranslatedOnly = false, int pageIndex = 0, int pageSize = int.MaxValue) where T : BaseEntity;
        /// <summary>
        /// Inserts a CustomerTranslatorService
        /// </summary>
        /// <param name="customerTranslation">Customer translation</param>
        void InsertCustomerTranslation(CustomerTranslationMapping customerTranslation);
        /// <summary>
        /// Inserts a customer translation mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="customerId">custoemrid id</param>
        /// <param name="entity">Entity</param>
        void InsertCustomerTranslationMapping<T>(T entity, int customerid, bool Translated) where T : BaseEntity;
        /// <summary>
        /// Updates the CustomerTranslatorService
        /// </summary>
        /// <param name="CustomerTranslatorService">Customer attribute</param>
        void UpdateCustomerTranslator<T>(T entity, int customerid, int id, bool translated) where T : BaseEntity;
        /// <summary>
        /// Find store identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>entity identifiers</returns>
        int[] GetMappingEntityIds<T>(T entity, int customerId) where T : BaseEntity;
    }
}
