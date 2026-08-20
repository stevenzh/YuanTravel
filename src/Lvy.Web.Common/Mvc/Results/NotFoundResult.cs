using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.Results
{
    public class NotFoundResult : ViewResult
    {

        public override void ExecuteResult(ControllerContext context)
        {
            ViewData = context.Controller.ViewData;
            TempData = context.Controller.TempData;

            ViewName = "404";

            base.ExecuteResult(context);
 
            context.HttpContext.Response.StatusDescription = "File Not Found";
            context.HttpContext.Response.StatusCode = 404;
        }
    }
}
