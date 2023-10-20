using Nop.Core;
using Nop.Core.Domain.Foundations;
using Nop.Services.Foundations;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Foundation;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class FoundationModelFactory: IFoundationModelFactory
    {
        #region flds
 private IPermissionService _permissionService;
        private IPictureService _pictureService;
        private IWorkContext _workContext;
        private IFoundationService _foundationsService;
        private ILocalizationService _localizationService;
        private ILocalizedEntityService _localizedEntityService;
        #endregion
        #region ctr
        public FoundationModelFactory(IWorkContext workContext, 
            IFoundationService foundationsService,
            IPictureService pictureService, 
            ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService,
            IPermissionService permissionService)
        {
            _workContext = workContext;
            _foundationsService = foundationsService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _permissionService = permissionService;
        }


        #endregion

        #region methods
       

        public FoundationSearchModel PrepareFoundationSearchModel(FoundationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public FoundationModel PrepareFoundationModel(FoundationModel model, Foundation foundation, bool excludeProperties = false)
        {
            if (foundation != null)
            {
                //fill in model values from the entity
                model ??= foundation.ToModel<FoundationModel>();
            }
            return model;
        }

        public FoundationListModel PrepareFoundationListModel(FoundationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

           var  foundations = _foundationsService.GetAllFoundations(searchModel.SearchName).ToPagedList(searchModel);
           var model = new FoundationListModel().PrepareToGrid(searchModel, foundations, () =>
            {
                return foundations.Select(point =>
                {
                    //fill in model values from the entity
                    var foundationMoldel = point.ToModel<FoundationModel>();
                    foundationMoldel.Name = _localizationService.GetLocalized(point, c => c.Name, _workContext.WorkingLanguage.Id);
                    return foundationMoldel;
                });
            });
            return model;
        }
        #endregion

        #region utls
        //void UpdateLocales(Foundation vendor, FoundationModel model)
        //{
          
        //}

        void IFoundationModelFactory.UpdateLocales(Foundation vendor, FoundationModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(vendor,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);
            }
        }




        #endregion

    }
}
