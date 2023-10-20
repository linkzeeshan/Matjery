using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    #region old
    public class ApiResult
    {
        public ApiResult()
        {
            validationResult = new ApiValidationResult();
        }

        public object result { get; set; }
        public HttpStatusCode status { get; internal set; }
        public ApiValidationResult validationResult { get; set; }
    }

    public class ApiValidationResult
    {
        public ApiValidationResult()
        {
            this.FieldValidationResult = new List<ApiValidationResultEntry>();
        }

        public IList<ApiValidationResultEntry> FieldValidationResult { get; set; }
    }

    public class ApiValidationResultEntry
    {
        public string FieldName { get; set; }
        public string ErrorType { get; set; }
        public string ErrorDescription { get; set; }
    }

    #endregion

    public class ApiResultResponse
    {
        public ApiResultResponse()
        {
            validationResult = new ApiValidationResultResponse();
        }

        public object result { get; set; }
        public HttpStatusCode status { get; internal set; }
        public ApiValidationResultResponse validationResult { get; set; }
    }

    public class ApiValidationResultResponse
    {
        public ApiValidationResultResponse()
        {
            this.fieldValidationResult = new List<ApiValidationResultEntryResponse>();
        }

        public IList<ApiValidationResultEntryResponse> fieldValidationResult { get; set; }
    }
    public class ApiValidationResultEntryResponse
    {
        public string fieldName { get; set; }
        public string errorType { get; set; }
        public string errorDescription { get; set; }
    }
}