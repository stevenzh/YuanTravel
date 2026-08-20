using System.Web.Mvc;

namespace Lvy.Web.Common.Exceptions
{
    /// <summary>
    /// 处理当前Action中未自定义的异常
    /// </summary>
    public class ExceptionFilterEx : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
           // filterContext.Exception.WriteLog(LogMessageType.Error);
            filterContext.HttpContext.Response.Redirect("~/Error.html");
            filterContext.ExceptionHandled = true;
        }
    }
}
