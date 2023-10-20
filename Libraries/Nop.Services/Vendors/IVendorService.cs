using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Vendors
{
    /// <summary>
    /// Vendor service interface
    /// </summary>
    public partial interface IVendorService
    {
        /// <summary>
        /// Gets a vendor by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor</returns>
        Vendor GetVendorById(int vendorId);
        Vendor GetVendorByEmail(string email, bool isdeleted= false);
        Vendor GetVendorByOldSystemId(int oldSystemId);
        Vendor GetFoundationById(int vendorId);

        TradelicenseStatus GetVendorLicenseStatus(Vendor vendor);
        /// <summary>
        /// Gets a vendors by product identifiers
        /// </summary>
        /// <param name="productIds">Array of product identifiers</param>
        /// <returns>Vendors</returns>
        IList<Vendor> GetVendorsByProductIds(int[] productIds);

        /// <summary>
        /// Gets a vendor by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Vendor</returns>
        Vendor GetVendorByProductId(int productId);

        /// <summary>
        /// Delete a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void DeleteVendor(Vendor vendor);

        void RemoveVendor(Vendor vendor);

        List<Vendor> GetLicenseExpiredVendors();

        List<Vendor> FillVendors(bool displayEntities = false);

        IPagedList<Vendor> GetAllVendors(VendorRegisterationType vendorRegisterationType, string name = "", string phoneNumber = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showNotActive = false,
            bool showSupportedVendors = false, int supportedByFoundationId = 0, int languageId = 0, DateTime? syncDate = null,Customer customer=null);

        IPagedList<Vendor> GetAllEntities(VendorRegisterationType vendorRegisterationType,
            int pageIndex = 0, int pageSize = int.MaxValue);

        IPagedList<Vendor> GetAllEntitiesMod(VendorRegisterationType vendorRegisterationType,
            int pageIndex = 0, int pageSize = int.MaxValue, Customer customer = null);

        IPagedList<Vendor> GetAllVendorsSupportedByFoundation(int SupportedByFoundationId, int FoundationAprovalStatusId,
            int pageIndex = 0, int pageSize = int.MaxValue);
        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="email">Vendor email</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        IPagedList<Vendor> GetAllVendors(string name = "", string email = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false,Customer customer=null);
   
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showNotActive">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        IPagedList<Vendor> GetAllVendors(string name = "", string phoneNumber = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showNotActive = false,
            bool showSupportedVendors = false, int supportedByFoundationId = 0, int languageid = 0, DateTime? syncDate = null, VendorSortingEnum OrderBy = VendorSortingEnum.Position, bool getDeleted = false);

        IPagedList<Vendor> GetAllVendorsIncludeSBF(string name = "", string phoneNumber = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showNotActive = false,
            bool showSupportedVendors = false, int supportedByFoundationId = 0, int languageid = 0, DateTime? syncDate = null, VendorSortingEnum OrderBy = VendorSortingEnum.Position, bool getDeleted = false, int sbfid = 0);

        /// <summary>
        /// Gets vendors
        /// </summary>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <returns>Vendors</returns>
        IList<Vendor> GetVendorsByIds(int[] vendorIds);

        /// <summary>
        /// Inserts a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void InsertVendor(Vendor vendor);

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void UpdateVendor(Vendor vendor);

        /// <summary>
        /// Gets a vendor note
        /// </summary>
        /// <param name="vendorNoteId">The vendor note identifier</param>
        /// <returns>Vendor note</returns>
        VendorNote GetVendorNoteById(int vendorNoteId);

        /// <summary>
        /// Gets all vendor notes
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Vendor notes</returns>
        IPagedList<VendorNote> GetVendorNotesByVendor(int vendorId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Deletes a vendor note
        /// </summary>
        /// <param name="vendorNote">The vendor note</param>
        void DeleteVendorNote(VendorNote vendorNote);

        /// <summary>
        /// Inserts a vendor note
        /// </summary>
        /// <param name="vendorNote">Vendor note</param>
        void InsertVendorNote(VendorNote vendorNote);

        /// <summary>
        /// Formats the vendor note text
        /// </summary>
        /// <param name="vendorNote">Vendor note</param>
        /// <returns>Formatted text</returns>
        string FormatVendorNoteText(VendorNote vendorNote);
    }
}