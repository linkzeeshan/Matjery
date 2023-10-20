using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;


namespace Nop.Core.Domain.Vendors
{
    public class VendorReview : BaseEntity
    {
        private ICollection<VendorReviewHelpfulness> _vendorReviewHelpfulnessEntries;

        public DateTime CreatedOnUtc { get; set; }
        public virtual Customer Customer { get; set; }
        //{

        //    get
        //    {

        //        Customer customer = EngineContext.Current.Resolve<Customer>();
        //        return customer;
        //    }
        //}
        public int CustomerId { get; set; }
        public int HelpfulNoTotal { get; set; }
        public int HelpfulYesTotal { get; set; }
        public bool IsApproved { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
       
        public virtual Store Store { get; set; }
        //{

        //    //get
        //    //{

        //    //    Store Store = EngineContext.Current.Resolve<Store>();
        //    //    return Store;
        //    //}
        //}
    public int StoreId { get; set; }
        public string Title { get; set; }
        public  Vendor Vendor { get; set; }
        //{
        //    get
        //    {
        //        Vendor vendor = EngineContext.Current.Resolve<Vendor>();
        //        return vendor;
        //    }

        //}
        public int VendorId { get; set; }
        public virtual ICollection<VendorReviewHelpfulness> VendorReviewHelpfulness
        {
            get { return _vendorReviewHelpfulnessEntries ?? (_vendorReviewHelpfulnessEntries = new List<VendorReviewHelpfulness>()); }
            protected set { _vendorReviewHelpfulnessEntries = value; }
        }
    }
}
