using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using System;
using System.Text;



namespace Nop.Core.Domain.Vendors
{
    public class VendorFollower : BaseEntity
    {
        public int CustomerId { get; set; }
        public int VendorId { get; set; }
        public DateTime FollowOnUtc { get; set; }
        public bool Unfollowed { get; set; }
        public DateTime? UnFollowOnUtc { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual Vendor Vendor { get; set; }
        //{
        //    get
        //    {
        //        Vendor vendor = EngineContext.Current.Resolve<IRepository<Vendor>>().GetById(this.VendorId);
        //        return vendor;
        //    }
        //}

    }
}
