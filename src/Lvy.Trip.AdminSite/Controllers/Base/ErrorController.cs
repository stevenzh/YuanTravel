using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// 无权访问
        /// </summary>
        /// <returns></returns>
        public ActionResult NoAuthorityAccess()
        {
            return View();
        }

        /// <summary>
        /// 错误页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NotFound()
        {
            return View("404");
        }
        public ActionResult ErrorPage()
        {
            return View("500");
        }
    }
}