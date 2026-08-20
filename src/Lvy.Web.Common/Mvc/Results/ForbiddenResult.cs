using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.Results
{
    /// <summary>
    ///  用户没有权限，调用该Result
    /// </summary>
    public class ForbiddenResult:ViewResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            ViewData = context.Controller.ViewData;
            TempData = context.Controller.TempData;

            ViewName = "403";

            base.ExecuteResult(context);

            context.HttpContext.Response.StatusDescription = "Forbidden";
            context.HttpContext.Response.StatusCode = 403;
        }
    }
}
