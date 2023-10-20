using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IWebHelper _webHelper;
        protected readonly ILogger _logger;

        public BaseController()
        {
            _storeContext = EngineContext.Current.Resolve<IStoreContext>();
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _webHelper = EngineContext.Current.Resolve<IWebHelper>();
            _logger = EngineContext.Current.Resolve<ILogger>();
        }

        protected ApiResultResponse Prepare(object obj, ApiValidationResultResponse validationResult = null)
        {
            ApiResultResponse result = new ApiResultResponse();

            try
            {
                var data = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                result.result = JsonConvert.DeserializeObject(data);
                result.status = HttpStatusCode.OK;
                if (validationResult != null && validationResult.fieldValidationResult.Any())
                {
                    result.status = HttpStatusCode.BadRequest;
                    result.validationResult = validationResult;
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message, ex, _workContext.CurrentCustomer);
                _logger.LogError("This can never happen: " + ex);
            }

            return result;
        }


        protected ApiResult PrepareStringResponse(object obj, ApiValidationResult validationResult = null)
        {
            ApiResult result = new ApiResult();

            try
            {
                var data = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                result.result = data;
                result.status = HttpStatusCode.OK;
                if (validationResult != null && validationResult.FieldValidationResult.Any())
                {
                    result.status = HttpStatusCode.BadRequest;
                    result.validationResult = validationResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("This can never happen: " + ex);
            }

            return result;
        }
    }
}
