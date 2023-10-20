using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.BlackPoints;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.BlackPoints;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.BlackPoint;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class BlackPointModelFactory : IBlackPointModelFactory
    {
        #region fields
        private readonly IPermissionService _permissionService;
        private readonly IBlackPointService _blackPointService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly INotificationService _notificationService;
        #endregion

        #region ctr
        public BlackPointModelFactory(IPermissionService permissionService,
            IBlackPointService blackPointService,
            ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IVendorService vendorService,
            VendorSettings vendorSettings,
            IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper,
            INotificationService notificationService)
        {
            _permissionService = permissionService;
            _blackPointService = blackPointService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
            _localizationService = localizationService;
            _vendorService = vendorService;
            _workflowMessageService = workflowMessageService;
            _vendorSettings = vendorSettings;
            _notificationService = notificationService;
        }
        #endregion

        #region methods
        public BlackPointListModel PrepareBlackPointListModel(BlackPointSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            var points = _blackPointService.SearchBlackPoints(Name:searchModel.Name, storeId: 0,
                vendorOrCustomerId: searchModel.vendorId,
                customerId: searchModel.CustomerId,
                orderId: 0,
                blackPointType: searchModel.BlackPointType,
                blackPointStatus: searchModel.BlackPointStatus).ToPagedList(searchModel);

            //prepare grid model
            var model = new BlackPointListModel().PrepareToGrid(searchModel, points, () =>
            {
                return points.Select(point =>
                {
                    //fill in model values from the entity
                    var BlackPointModel = point.ToModel<BlackPointModel>();
                   
                        //fill in additional values (not existing in the entity)
                        var addedByCustomer = _customerService.GetCustomerById(point.AddedByCustomerId);
                    if (addedByCustomer != null)
                        BlackPointModel.AddedBy= _customerService.GetCustomerFullName(addedByCustomer);

                    if (point.AddedOnTypeId == (int)BlackPointTypeEnum.Vendor)
                    {
                        var vendor = _vendorService.GetVendorById(point.VendorOrCustomerId);
                        if (vendor != null)
                            BlackPointModel.AddedOn = _localizationService.GetLocalized(vendor, c => c.Name);
                    }
                    else
                    {
                        var customer = _customerService.GetCustomerById(point.VendorOrCustomerId);
                        if (customer != null)
                            BlackPointModel.AddedOn = _customerService.GetCustomerFullName(customer); ;
                    }
                    BlackPointModel.PointStatus= BlackPointStatus(point.BlackPointStatus);
                    BlackPointModel.BlackPointType = BlackPointType(point.AddedOnTypeId);

                    return BlackPointModel;
                });
            });
            if (!string.IsNullOrEmpty(searchModel.AddedBy))
                model.Data = model.Data.Where(x => x.AddedBy != null && x.AddedBy.Contains(searchModel.AddedBy));

                return model;
        }

        public BlackPointSearchModel PrepareBlackPointSearchModel(BlackPointSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.AvailablePointType = BlackPointTypeEnum.Customer.ToSelectListInServices(false).ToList();

            searchModel.AvailablePointType.Insert(0, new SelectListItem
            { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0", Selected = true });

            searchModel.AvailablePointStatus = BlackPointStatusEnum.Pending.ToSelectListInServices(false).ToList();

            searchModel.AvailablePointStatus.Insert(0, new SelectListItem
            { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0", Selected = true });

            searchModel.IsAdmin = _customerService.IsAdmin(_workContext.CurrentCustomer);
            return searchModel;
        }

        public BlackPointModel PrepareCustomerRoleModel(BlackPointModel model, BlackPoint blackPoint, bool excludeProperties = false)
        {
            if (blackPoint != null)
            {
                //fill in model values from the entity
                model ??= blackPoint.ToModel<BlackPointModel>();
                var addedByCustomer = _customerService.GetCustomerById(blackPoint.AddedByCustomerId);
                if (addedByCustomer != null)
                    model.AddedBy = _customerService.GetCustomerFullName(addedByCustomer);

                if (blackPoint.AddedOnTypeId == (int)BlackPointTypeEnum.Vendor)
                {
                    var vendor = _vendorService.GetVendorById(blackPoint.VendorOrCustomerId);
                    if (vendor != null)
                        model.AddedOn = _localizationService.GetLocalized(vendor, c => c.Name);
                }
                else
                {
                    var customer = _customerService.GetCustomerById(blackPoint.VendorOrCustomerId);
                    if (customer != null)
                        model.AddedOn = _customerService.GetCustomerFullName(addedByCustomer);
                }
                model.Id = blackPoint.Id;
                model.PointStatusId = blackPoint.BlackPointStatus;
                model.Comment = blackPoint.Comment;
                model.Note = blackPoint.Note;
                model.PointStatus = BlackPointStatus(blackPoint.BlackPointStatus);
                model.BlackPointCount = _blackPointService.GetVendorOrCustomerCount(blackPoint.VendorOrCustomerId);
                model.BlackPointType = BlackPointType(blackPoint.AddedOnTypeId);
                model.OrderId = blackPoint.OrderId;
            }
            return model;



        }
        #endregion

        #region utilities
        private string BlackPointType(int BlackPointTypeId)
        {
            string bType = string.Empty;
            if (BlackPointTypeId > 0)
            {
                if (BlackPointTypeId == (int)BlackPointTypeEnum.Customer)
                    bType = _localizationService.GetResource("enums.Nop.Core.Domain.BlackPoints.BlackPointTypeEnum.Customer");

                if (BlackPointTypeId == (int)BlackPointTypeEnum.Vendor)
                    bType = _localizationService.GetResource("enums.Nop.Core.Domain.BlackPoints.BlackPointTypeEnum.Vendor");
            }
            return bType;
        }
        private string BlackPointStatus(int PointStatusId)
        {
            string status = string.Empty;
            if (PointStatusId > 0)
            {
                if (PointStatusId == (int)BlackPointStatusEnum.Pending)
                    status = _localizationService.GetResource("enums.nop.core.domain.blackpoints.BlackPointStatusEnum.Pending");

                if (PointStatusId == (int)BlackPointStatusEnum.Approved)
                    status = _localizationService.GetResource("enums.nop.core.domain.blackpoints.BlackPointStatusEnum.Approved");

                if (PointStatusId == (int)BlackPointStatusEnum.Rejected)
                    status = _localizationService.GetResource("enums.nop.core.domain.blackpoints.BlackPointStatusEnum.Rejected");
            }
            return status;
        }
        #endregion

    }
}
