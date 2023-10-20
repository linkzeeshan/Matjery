using Nop.Core.Domain.Vendors;
using Nop.Plugin.NopWarehouse.VendorReview.Infrastrcture.Mapper;
using Nop.Plugin.NopWarehouse.VendorReview.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.Extensions
{
    public static class MappingExtensions
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }


        #region Follower

        public static VendorFollowerModel ToModel(this VendorFollower entity)
        {
            return entity.MapTo<VendorFollower, VendorFollowerModel>();
        }

        public static VendorFollower ToEntity(this VendorFollowerModel model)
        {
            return model.MapTo<VendorFollowerModel, VendorFollower>();
        }

        public static VendorFollower ToEntity(this VendorFollowerModel model, VendorFollower destination)
        {
            return model.MapTo(destination);
        }

        #endregion

    }
}
