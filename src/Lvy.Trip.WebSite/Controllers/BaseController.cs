using Arch.Common;
using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Site;
using Lvy.Web.Common;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 前台基础Controller
    /// </summary>
    public class BaseController : Controller
    {
        private readonly SiteNavBiz _navBiz = new SiteNavBiz();
        private readonly DictionaryBiz commonBiz = new DictionaryBiz();
        private ILog _logger = null;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //base.OnActionExecuting(filterContext);
            var ownerInfo = GlobalContext.Current.OwnerInfo;
            if (ownerInfo == null)
            {
                var host = (SysPlatformModel)this.RouteData.Values["tenant"];
                var customer = new CustomerBiz().GetById(host.CustomerCode);
                if (customer != null)
                {
                    host.CrmCustomer = customer;
                    GlobalContext.Current.OwnerCode = host.CustomerCode;
                    GlobalContext.Current.OwnerInfo = host;
                }
            }
            else
            {
                GlobalContext.Current.OwnerCode = ownerInfo.CustomerCode;
                GlobalContext.Current.OwnerInfo = ownerInfo;
            }

            // 设置出发城市
            if (GlobalContext.Current.CurrentCity.IsNullOrEmpty())
                GlobalContext.Current.CurrentCity = GlobalContext.Current.OutCity;
            if (!string.IsNullOrEmpty(Request.Params["out_city"]))
                GlobalContext.Current.CurrentCity = Request.Params["out_city"];
            GlobalContext.Current.CurrentCityName = DictionaryTools.GetEnumValue("OutCityEnum", GlobalContext.Current.CurrentCity);


            // 根据当前城市获得 导航栏
            if (GlobalContext.Current.CurrentCity == "31")
            {
                ViewBag.N1 = commonBiz.GetLineDestsCached("S001", GlobalContext.Current.OwnerCode);
            }
            else if (GlobalContext.Current.CurrentCity == "3201")  //南京
            {
                ViewBag.N1 = commonBiz.GetLineDestsCached("S101", GlobalContext.Current.OwnerCode);
            }
            else if (GlobalContext.Current.CurrentCity == "3401")  //合肥
            {
                ViewBag.N1 = commonBiz.GetLineDestsCached("S201", GlobalContext.Current.OwnerCode);
            }
            // 横向导肮 使用同一个
            ViewBag.N2 = commonBiz.GetLineDestsCached("S002", GlobalContext.Current.OwnerCode);

        }
        protected override void OnException(ExceptionContext filterContext)
        {
            // 执行基类中的OnException
            base.OnException(filterContext);
            if (filterContext.Exception != null)
            {
                _logger = LogManager.GetLogger(filterContext.Controller.GetType());

                string error = string.Format("类名：{0}  \r\n  错误信息：{1} \r\n  {2}",
                           filterContext.Controller.GetType(),
                           filterContext.Exception.Message,
                           filterContext.Exception.StackTrace);
                _logger.Error(error);

                // Response.Redirect("~/Error");
            }
        }

        /// <summary>
        /// 网站Logo路径
        /// </summary>
        public string LogoPath
        {
            get
            {
                return AppSetting.Get("UploadFileRoot") + GlobalContext.Current.CustomerLogoPath;
            }
        }

        /// <summary>
        /// 初始化页面数据
        /// </summary>
        protected virtual void InitPage()
        {
        }

        /// <summary>
        /// 当前账户
        /// </summary>
        public CrmAccountModel UserInfo
        {
            get
            {
                return GlobalContext.Current.UserInfo;
            }
        }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode
        {
            get { return GlobalContext.Current.OwnerCode; }
        }
    }
}