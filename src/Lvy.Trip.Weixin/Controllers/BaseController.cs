using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Weixin;
using Lvy.Web.Common;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System.Diagnostics;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace Lvy.Trip.Weixin.Controllers
{
    public class BaseController : Controller
    {
        ILog logger = LogManager.GetLogger("BaseController");
        protected string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        protected string secret = WebConfigurationManager.AppSettings["WeixinAppSecret"];
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly MemberBiz _memberBize = new MemberBiz();
        private readonly CustomerBiz _biz = new CustomerBiz();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (string.IsNullOrEmpty(UIGlobal.Current.OwnerCode))
            {
                var u = WebConfigurationManager.AppSettings["OwnerCode"];
                UIGlobal.Current.OwnerCode = u;
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var mpFileVersionInfo = FileVersionInfo.GetVersionInfo(Server.MapPath("~/bin/Senparc.Weixin.MP.dll"));
            TempData["MpVersion"] = string.Format("{0}.{1}", mpFileVersionInfo.FileMajorPart, mpFileVersionInfo.FileMinorPart); //Regex.Match(fileVersionInfo.FileVersion, @"\d+\.\d+");

            base.OnResultExecuting(filterContext);
        }

        /// <summary>
        /// 所属客户
        /// </summary>
        public string OwnerCode
        {
            get { return UIGlobal.Current.OwnerCode; }
        }

        protected bool InWeixin(string code, string state)
        {
            // 微信转发
            if (!string.IsNullOrEmpty(code))
            {
                var result = OAuthApi.GetAccessToken(appId, secret, code);
                if (result.errcode == ReturnCode.请求成功)
                {
                    Member user = _memberBize.GetMemberByOpenID(result.openid);
                    if (user != null)
                    {
                        Session["WeixinUser"] = user;  // 前台Sesion
                        FormsAuthentication.SetAuthCookie(user.OpenID, false);
                    }
                    else
                    {
                        logger.Warn("微信用户未保存 OpenID:" + result.openid);
                    }
                }
            }
            //else
            //{
            //    Member user = _memberBize.GetMemberByOpenID("ok6cAuKoOc85PZtNdKlSurbNiaGQ");
            //    FormsAuthentication.SetAuthCookie(user.OpenID, false);
            //    Session["WeixinUser"] = user;
            //}

            return false;
        }
    }
}