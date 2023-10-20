using AutoMapper;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopWarehouse.VendorReview.Models;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.Infrastrcture.Mapper
{
    public static class AutoMapperConfiguration
    {
        private static MapperConfiguration _mapperConfiguration;
        private static IMapper _mapper;

        /// <summary>
        /// Initialize mapper
        /// </summary>
        public static void Init()
        {
            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                //follower
                cfg.CreateMap<VendorFollowerModel, VendorFollower>();
                //.ForMember(dest => dest.LimitedToStores, mo => mo.Ignore())

                cfg.CreateMap<VendorFollower, VendorFollowerModel>()
                    .ForMember(dest => dest.VendorName, mo => mo.MapFrom(v => v.Vendor.Name))
                    .ForMember(dest => dest.CustomerName, mo => mo.MapFrom(c => c.Customer.Username)) //get fullname
                    .ForMember(dest => dest.FollowOnUtcStr, mo => mo.MapFrom(c => c.FollowOnUtc.ToShortDateString()))
                    .ForMember(dest => dest.UnFollowOnUtcStr, mo => mo.MapFrom(c => c.UnFollowOnUtc.HasValue ? c.UnFollowOnUtc.Value.ToShortDateString() : ""))
                    .ForMember(dest => dest.Status, mo => mo.MapFrom(c => c.Unfollowed
                        ? EngineContext.Current.Resolve<ILocalizationService>().GetResource("common.yes")
                        : EngineContext.Current.Resolve<ILocalizationService>().GetResource("common.no")))
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            });
            _mapper = _mapperConfiguration.CreateMapper();
        }

        /// <summary>
        /// Mapper
        /// </summary>
        public static IMapper Mapper
        {
            get
            {
                return _mapper;
            }
        }
        /// <summary>
        /// Mapper configuration
        /// </summary>
        public static MapperConfiguration MapperConfiguration
        {
            get
            {
                return _mapperConfiguration;
            }
        }
    }
}
