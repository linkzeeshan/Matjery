using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Data;
using Nop.Services.Caching.Extensions;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Services.Vendors
{
    /// <summary>
    /// Vendor service
    /// </summary>
    public partial class VendorService : IVendorService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<VendorFollower> _VendorFollower;
        private readonly IRepository<VendorNote> _vendorNoteRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly CommonSettings _commonSettings;
        //private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public VendorService(IEventPublisher eventPublisher,

            IRepository<Product> productRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<VendorFollower> VendorFollower,
            IRepository<VendorNote> vendorNoteRepository,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            CommonSettings commonSettings
            )
        {
            _commonSettings = commonSettings;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _vendorNoteRepository = vendorNoteRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _VendorFollower = VendorFollower;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a vendor by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor</returns>
        public virtual Vendor GetVendorById(int vendorId)
        {
            if (vendorId == 0)
                return null;

            return _vendorRepository.ToCachedGetById(vendorId);
        }
        public virtual Vendor GetVendorByEmail(string email, bool checkForDelete = false)
        {
            

            var query = from c in _vendorRepository.Table
                        orderby c.Id
                        where c.Email == email
                        select c;
            if (checkForDelete)
                query = query.Where(c => !c.Deleted);

            var vendor = query.FirstOrDefault();
            return vendor;
        }
        public virtual TradelicenseStatus GetVendorLicenseStatus(Vendor vendor)
        {
            var TradelicenseStatus = new TradelicenseStatus();
            string ExpiryDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeExpiryDate);
            if (!string.IsNullOrEmpty(ExpiryDate))
            {
                //DateTime expiryDate = DateTime.Parse(ExpiryDate);

                DateTime expiryDate;
                int daysUntilExpiry = 0;
                if (DateTime.TryParseExact(ExpiryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out expiryDate))
                {
                    DateTime currentDate = DateTime.Now;
                    TimeSpan timeUntilExpiry = expiryDate - currentDate;
                    daysUntilExpiry = timeUntilExpiry.Days;
                }
               
                switch (daysUntilExpiry)
                {
                    case var x when x > _commonSettings.LicenseExpiryReminder1Days:
                        TradelicenseStatus.Id = (int)VendorLicenseStatus.Active;
                        TradelicenseStatus.Name = "Active";
                        break;
                    case var x when x > _commonSettings.LicenseExpiryReminder2Days && x< _commonSettings.LicenseExpiryReminder1Days:
                        TradelicenseStatus.Id = (int)VendorLicenseStatus.AboutToExpire;
                        TradelicenseStatus.Name = "Active";
                        break;
                    case var x when x < _commonSettings.LicenseExpiryReminder2Days && x > _commonSettings.LicenseExpiryReminder3Days:
                        TradelicenseStatus.Id = (int)VendorLicenseStatus.AboutToExpire;
                        TradelicenseStatus.Name= "AboutToExpire";
                        break;
                    case var x when x < _commonSettings.LicenseExpiryReminder3Days && x>0:
                        TradelicenseStatus.Id = (int)VendorLicenseStatus.AboutToExpire;
                        TradelicenseStatus.Name = "AboutToExpire";
                        break;
                    case 0:
                        TradelicenseStatus.Id = (int)VendorLicenseStatus.Expired;
                        TradelicenseStatus.Name = "Expired";
                        break;
                    case var x when x < 0:
                        TradelicenseStatus.Id = (int)VendorLicenseStatus.Expired;
                        TradelicenseStatus.Name = "Expired";
                        break;
                    default:
                        TradelicenseStatus.Id = (int)VendorLicenseStatus.Nolicense;
                        TradelicenseStatus.Name = "no";
                        TradelicenseStatus.Message = "";
                        break;
                }

            }
            else
            {
                TradelicenseStatus.Id = (int)VendorLicenseStatus.Nolicense;
                TradelicenseStatus.Name = "Nolicense";
            }
            return TradelicenseStatus;
        }
        public virtual Vendor GetVendorByOldSystemId(int oldSystemId)
        {
            if (oldSystemId == 0)
                return null;

            return _vendorRepository.Table.FirstOrDefault(v => v.VendorOldSystemId == oldSystemId);
        }
        public virtual Vendor GetFoundationById(int vendorId)
        {
            if (vendorId == 0)
                return null;

            var query = _vendorRepository.Table;
            query = query.Where(v => v.Id == vendorId
                && v.RegisterationTypeId == (int)VendorRegisterationType.Foundation);

            var foundation = query.FirstOrDefault();
            return foundation;
        }
        /// <summary>
        /// Gets a vendor by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Vendor</returns>
        public virtual Vendor GetVendorByProductId(int productId)
        {
            if (productId == 0)
                return null;

            return (from v in _vendorRepository.Table
                    join p in _productRepository.Table on v.Id equals p.VendorId
                    where p.Id == productId
                    select v).FirstOrDefault();

        }

        /// <summary>
        /// Gets a vendors by product identifiers
        /// </summary>
        /// <param name="productIds">Array of product identifiers</param>
        /// <returns>Vendors</returns>
        public virtual IList<Vendor> GetVendorsByProductIds(int[] productIds)
        {
            if (productIds is null)
                throw new ArgumentNullException(nameof(productIds));

            return (from v in _vendorRepository.Table
                    join p in _productRepository.Table on v.Id equals p.VendorId
                    where productIds.Contains(p.Id) && !v.Deleted && v.Active
                    group v by p.Id into v
                    select v.First()).ToList();
        }

        /// <summary>
        /// Delete a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// 
        public virtual void RemoveVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            _vendorRepository.Delete(vendor);

            //event notification
            _eventPublisher.EntityUpdated(vendor);
        }
        public virtual void DeleteVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            vendor.Deleted = true;
            //fix: syncing will not work if a vendor is deleted without first making it in-active
            vendor.Active = false;
            
            //change email format
            vendor.Email = CommonHelper.CustomerEmailFormat(vendor.Email, true);
            //change user name format
            vendor.Name = CommonHelper.CustomerNameFormat(vendor.Name, true);
            //change phone number format
            vendor.PhoneNumber = CommonHelper.CustomerPhoneNumberFormat(vendor.PhoneNumber, true);

            vendor.UpdatedOnUtc = DateTime.UtcNow;
            UpdateVendor(vendor);

            //event notification
            _eventPublisher.EntityDeleted(vendor);
        }
        public virtual IPagedList<Vendor> GetAllVendors(string name = "",
          int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _vendorRepository.Table;
            if (!String.IsNullOrWhiteSpace(name))
            {
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        where j.LocaleValue.Contains(name) || q.Name.Contains(name)
                        select q;
            }
            if (!showHidden)
            {
                query = query.Where(v => v.Active);
                var VendorList = _productRepository.Table.Where(d => !d.Deleted && d.Published).Select(ve => ve.VendorId).Distinct();
                query = from l in query
                        where VendorList.Contains(l.Id)
                        select l;
            }
            query = query.Where(v => !v.Deleted);

            query = query.OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.CreatedOnUtc)
                .ThenBy(v => v.Name);

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }
        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        public virtual IPagedList<Vendor> GetAllVendors(string name = "", string phoneNumber = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showNotActive = false,
            bool showSupportedVendors = false, int supportedByFoundationId = 0, int languageId = 0, DateTime? syncDate = null, VendorSortingEnum OrderBy = VendorSortingEnum.Position, bool getDeleted = false)
        {
            var query = _vendorRepository.Table;
            if (!String.IsNullOrWhiteSpace(name))
            {
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        where j.LocaleValue.Contains(name) || q.Name.Contains(name)
                        select q;
            }
            if (!string.IsNullOrEmpty(phoneNumber))
                query = query.Where(v => v.PhoneNumber == phoneNumber);

            if (showSupportedVendors)
            {
                query = query.Where(v => v.RegisterationTypeId == (int)VendorRegisterationType.Merchant);
            }
            if (supportedByFoundationId>0)
            {
                query = query.Where(v => v.SupportedByFoundationId != null && v.SupportedByFoundationId == supportedByFoundationId);
            }
            if (syncDate.HasValue)
                query = query.Where(v => v.UpdatedOnUtc != null && v.UpdatedOnUtc.Value >= syncDate.Value);

            if (!showNotActive)
                query = query.Where(v => v.Active);
            if (getDeleted)
            {
                query = query.Where(v => v.Deleted);
            }
            else
            {
                query = query.Where(v => !v.Deleted);
            }

            if (OrderBy != VendorSortingEnum.Position)
            {
                switch (OrderBy)
                {
                    case VendorSortingEnum.NameAsc: query = query.OrderBy(c => c.Name); break;
                    case VendorSortingEnum.CreatedOnAsc: query = query.OrderBy(c => c.CreatedOnUtc); break;
                    case VendorSortingEnum.NameDesc: query = query.OrderByDescending(c => c.Name); break;
                    case VendorSortingEnum.CreatedOnDesc: query = query.OrderByDescending(c => c.CreatedOnUtc); break;
                }
            }
            else
            {

                query = query.OrderBy(v => v.DisplayOrder)
                    .ThenByDescending(v => v.CreatedOnUtc)
                    .ThenBy(v => v.Name);
            }

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        public virtual IPagedList<Vendor> GetAllVendorsIncludeSBF(string name = "", string phoneNumber = "",
           int pageIndex = 0, int pageSize = int.MaxValue, bool showNotActive = false,
           bool showSupportedVendors = false, int supportedByFoundationId = 0, int languageId = 0, DateTime? syncDate = null, VendorSortingEnum OrderBy = VendorSortingEnum.Position, bool getDeleted = false, int sbfid = 0)
        {
            var query = _vendorRepository.Table;
            if (!String.IsNullOrWhiteSpace(name))
            {
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        where j.LocaleValue.Contains(name) || q.Name.Contains(name)
                        select q;
            }
            else
            {
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        select q;
            }
            if (!string.IsNullOrEmpty(phoneNumber))
                query = query.Where(v => v.PhoneNumber == phoneNumber);

            if (showSupportedVendors && supportedByFoundationId>0)
            {
                query = query.Where(v => v.RegisterationTypeId == (int)VendorRegisterationType.Merchant
                    && v.SupportedByFoundationId != null && v.SupportedByFoundationId == supportedByFoundationId);
            }

            if (sbfid != 0)
            {
                query = query.Where(v => v.SupportedByFoundationId == sbfid || v.Id == sbfid);
            }

            if (syncDate.HasValue)
                query = query.Where(v => v.UpdatedOnUtc != null && v.UpdatedOnUtc.Value >= syncDate.Value);

            if (!showNotActive)
                query = query.Where(v => v.Active);
            if (getDeleted)
            {
                query = query.Where(v => v.Deleted);
            }
            else
            {
                query = query.Where(v => !v.Deleted);
            }

            if (OrderBy != VendorSortingEnum.Position)
            {   
                switch (OrderBy)
                {
                    case VendorSortingEnum.NameAsc: query = query.OrderBy(c => c.Name); break;
                    case VendorSortingEnum.CreatedOnAsc: query = query.OrderBy(c => c.CreatedOnUtc); break;
                    case VendorSortingEnum.NameDesc: query = query.OrderByDescending(c => c.Name); break;
                    case VendorSortingEnum.CreatedOnDesc: query = query.OrderByDescending(c => c.CreatedOnUtc); break;
                }
            }
            else
            {

                query = query.OrderBy(v => v.DisplayOrder)
                    .ThenByDescending(v => v.CreatedOnUtc)
                    .ThenBy(v => v.Name);
            }

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        public List<Vendor> FillVendors(bool displayEntities = false)
        {
            var query = _vendorRepository.Table;
            if (displayEntities)
            {
                return query.Where(x => x.RegisterationTypeId == (int)VendorRegisterationType.Foundation).ToList();
            }
            return query.Where(x => x.RegisterationTypeId == (int)VendorRegisterationType.Merchant).ToList();
        }

        public virtual List<Vendor> GetLicenseExpiredVendors()
        {
            var query = _vendorRepository.Table;
                query = from q in query
                        where q.Active &&!q.Deleted && q.LicenseNo !=null
                        select q;

            return query.ToList();
        }


        public virtual IPagedList<Vendor> GetAllVendors(VendorRegisterationType vendorRegisterationType, string name = "", string phoneNumber = "",
         int pageIndex = 0, int pageSize = int.MaxValue, bool showNotActive = false,
         bool showSupportedVendors = false, int supportedByFoundationId = 0, int languageId = 0, DateTime? syncDate = null, Customer customer = null)
        {
            var query = _vendorRepository.Table;
            var query_follow = _VendorFollower.Table;
          
            if (!String.IsNullOrWhiteSpace(name))
            {

                query = from q in query 
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        where j.LocaleValue.Contains(name) || q.Name.Contains(name)
                        select q;
            }

            if (!string.IsNullOrEmpty(phoneNumber))
                query = query.Where(v => v.PhoneNumber == phoneNumber);

            if (showSupportedVendors)
            {
                query = query.Where(v => v.RegisterationTypeId == (int)VendorRegisterationType.Merchant
                    && v.SupportedByFoundationId != null && v.SupportedByFoundationId == supportedByFoundationId);
            }

            if (syncDate.HasValue)
                query = query.Where(v => v.UpdatedOnUtc != null && v.UpdatedOnUtc.Value >= syncDate.Value);

            if (!showNotActive)
            {
                var VendorList = _productRepository.Table.Where(d => !d.Deleted && d.Published).Select(ve => ve.VendorId).Distinct();
                query = query.Where(v => v.Active);
                query = from l in query
                        where VendorList.Contains(l.Id)
                        select l;
            }
        

            query = query.Where(v => v.RegisterationTypeId == (int)vendorRegisterationType);
            query = query.Where(v => !v.Deleted);
            if (customer != null)
            {
                if (_customerService.IsRegistered(customer))
                {
                    query_follow = from f in query_follow where !f.Unfollowed && f.CustomerId == customer.Id select f;
                    query_follow = query_follow.OrderByDescending(x => x.FollowOnUtc);
                    var followedvendor_Ids = query_follow.Select(x => x.VendorId).ToArray();
                    query = query.Where(v => !followedvendor_Ids.Contains(v.Id));
                }
            }
          
            query = query.OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.CreatedOnUtc)
                .ThenBy(v => v.Name);

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        public virtual IPagedList<Vendor> GetAllEntities(VendorRegisterationType vendorRegisterationType,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _vendorRepository.Table;

            query = query.Where(v => v.RegisterationTypeId == (int)vendorRegisterationType);
            query = query.Where(v => !v.Deleted);
            query = query.Where(v => v.Active);
            query = query.OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.CreatedOnUtc)
                .ThenBy(v => v.Name);

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        public virtual IPagedList<Vendor> GetAllEntitiesMod(VendorRegisterationType vendorRegisterationType,
            int pageIndex = 0, int pageSize = int.MaxValue, Customer customer = null)
        {
            var query = _vendorRepository.Table ;
            var query_follower = _VendorFollower.Table;
            if ((int)vendorRegisterationType == 1)
            {
                if (customer != null)
                {
                    if (_customerService.IsRegistered(customer))
                    {

                        query = (from v in query

                                 join f in _VendorFollower.Table on new { a = v.Id, b = customer.Id } equals new { a = f.VendorId, b = f.CustomerId } into followed
                                 from f in followed.DefaultIfEmpty().OrderByDescending(x => x.CustomerId == customer.Id && x.Unfollowed==false ? x.FollowOnUtc : v.CreatedOnUtc)
                                 join p in _productRepository.Table on v.Id equals p.VendorId into g
                                 from p in g.Take(1)
                                 where (p.Deleted == false)
                                 select v);
                    }
                    else
                    {
                        query = from v in query
                                join p in _productRepository.Table on v.Id equals p.VendorId into g
                                from p in g.Take(1)
                                where (p.Deleted == false)
                                select v;
                    }
                }
               
            }

            query = query.Where(v => v.RegisterationTypeId == (int)vendorRegisterationType);
            query = query.Where(v => !v.Deleted);
            query = query.Where(v => v.Active);
           
            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        public virtual IPagedList<Vendor> GetAllVendorsSupportedByFoundation(int SupportedByFoundationId, int FoundationAprovalStatusId,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _vendorRepository.Table;

            query = query.Where(v => v.SupportedByFoundationId == SupportedByFoundationId);
            query = query.Where(v => v.FoundationAprovalStatusId == FoundationAprovalStatusId);
            query = query.Where(v => !v.Deleted);
            query = query.Where(v => v.Active);
            query = query.OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.CreatedOnUtc)
                .ThenBy(v => v.Name);

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="email">Vendor email</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        public virtual IPagedList<Vendor> GetAllVendors(string name = "", string email = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, Customer customer = null)
        {
            var query = _vendorRepository.Table;
            if (!string.IsNullOrWhiteSpace(name))
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        where j.LocaleValue.Contains(name) || q.Name.Contains(name)
                        select q;

      


            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(v => v.Email.Contains(email));

            if (!showHidden)
            {
              

                if (customer != null)
                {
                    if (_customerService.IsRegistered(customer)&& string.IsNullOrWhiteSpace(email)&& string.IsNullOrWhiteSpace(name))
                    {
                        query = (from v in query

                                 join f in _VendorFollower.Table on new { a = v.Id, b = customer.Id } equals new { a = f.VendorId, b = f.CustomerId } into followed
                                 from f in followed.DefaultIfEmpty().OrderByDescending(x => x.CustomerId == customer.Id && x.Unfollowed == false ? x.FollowOnUtc : v.CreatedOnUtc)
                                 join p in _productRepository.Table on v.Id equals p.VendorId into g
                                 from p in g.Take(1)
                                 where (p.Deleted == false)&&(v.Active && !v.Deleted)
                                 select v);
                       
                    }
                    else
                    {
                        query = query.Where(v => !v.Deleted && v.Active);
                        var VendorList = _productRepository.Table.Where(d => !d.Deleted && d.Published).Select(ve => ve.VendorId).Distinct().ToList();
                        if (query.Any())
                            query = query.Where(x => VendorList.Contains(x.Id));

                        query = query.OrderBy(v => v.DisplayOrder).ThenBy(v => v.CreatedOnUtc).ThenBy(v => v.Name);

                    }
                }
              
              
            }

            query = query.Where(v => v.RegisterationTypeId == (int)VendorRegisterationType.Merchant && !v.Deleted);
            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        /// <summary>
        /// Gets vendors
        /// </summary>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <returns>Vendors</returns>
        public virtual IList<Vendor> GetVendorsByIds(int[] vendorIds)
        {
            var query = _vendorRepository.Table;
            if (vendorIds != null)
                query = query.Where(v => vendorIds.Contains(v.Id));

            return query.ToList();
        }

        /// <summary>
        /// Inserts a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void InsertVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            _vendorRepository.Insert(vendor);

            //event notification
            _eventPublisher.EntityInserted(vendor);
        }

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void UpdateVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            _vendorRepository.Update(vendor);

            //event notification
            _eventPublisher.EntityUpdated(vendor);
        }

        /// <summary>
        /// Gets a vendor note
        /// </summary>
        /// <param name="vendorNoteId">The vendor note identifier</param>
        /// <returns>Vendor note</returns>
        public virtual VendorNote GetVendorNoteById(int vendorNoteId)
        {
            if (vendorNoteId == 0)
                return null;

            return _vendorNoteRepository.ToCachedGetById(vendorNoteId);
        }

        /// <summary>
        /// Gets all vendor notes
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Vendor notes</returns>
        public virtual IPagedList<VendorNote> GetVendorNotesByVendor(int vendorId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _vendorNoteRepository.Table.Where(vn => vn.VendorId == vendorId);

            query = query.OrderBy(v => v.CreatedOnUtc).ThenBy(v => v.Id);

            return new PagedList<VendorNote>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Deletes a vendor note
        /// </summary>
        /// <param name="vendorNote">The vendor note</param>
        public virtual void DeleteVendorNote(VendorNote vendorNote)
        {
            if (vendorNote == null)
                throw new ArgumentNullException(nameof(vendorNote));

            _vendorNoteRepository.Delete(vendorNote);

            //event notification
            _eventPublisher.EntityDeleted(vendorNote);
        }

        /// <summary>
        /// Inserts a vendor note
        /// </summary>
        /// <param name="vendorNote">Vendor note</param>
        public virtual void InsertVendorNote(VendorNote vendorNote)
        {
            if (vendorNote == null)
                throw new ArgumentNullException(nameof(vendorNote));

            _vendorNoteRepository.Insert(vendorNote);

            //event notification
            _eventPublisher.EntityInserted(vendorNote);
        }

        /// <summary>
        /// Formats the vendor note text
        /// </summary>
        /// <param name="vendorNote">Vendor note</param>
        /// <returns>Formatted text</returns>
        public virtual string FormatVendorNoteText(VendorNote vendorNote)
        {
            if (vendorNote == null)
                throw new ArgumentNullException(nameof(vendorNote));

            var text = vendorNote.Note;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);

            return text;
        }

        #endregion
    }
}
