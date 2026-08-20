using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using Lvy.Trip.Weixin.Models;

namespace Lvy.Trip.Weixin.Util
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var request = actionContext.HttpContext.Request;
            if (!request.IsAjaxRequest())
            {
                actionContext.HttpContext.Response.Clear();
                actionContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                GenericErrorModel ajaxErrorModel = new GenericErrorModel();
                ajaxErrorModel.AddMessage("Invalid request");

                actionContext.Result = new JsonResult
                {
                    Data = ajaxErrorModel,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    ContentEncoding = System.Text.Encoding.UTF8,
                    ContentType = "application/json",
                };
            }
        }
    }
}
