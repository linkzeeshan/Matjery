using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.Models
{
    public class VendorFollowerListModel : BaseSearchModel
    {
        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorFollowers.List.FollowOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? FollowOnFrom { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorFollowers.List.FollowOnTo")]
        [UIHint("DateNullable")]
        public DateTime? FollowOnTo { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorFollowers.List.SearchVendor")]
        public int SearchVendorId { get; set; }

        public VendorFollowerListModel()
        {
        }
    }
}
