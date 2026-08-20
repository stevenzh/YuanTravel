using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Lvy.Model.Other;
using Lvy.Model.Member;
using Lvy.Core.Tools;

using System.Configuration;
using Lvy.Biz.System;
using Lvy.Core.Web;
using Microsoft.Practices.ServiceLocation;
using GlobalContext = Lvy.Core.Web.GlobalContext;

namespace Lvy.Web.Common.Mvc.Attributes
{
    public class OperateCookies : ActionFilterAttribute
    {
        public static string cookieName = ConfigurationManager.AppSettings["PrefixKey"] + "Lvyonline";

        protected virtual string CookieName
        {
            get { return cookieName; }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string strAuth = "http://member.gogotrips.com/Account/Login";

            //string strAuth = "http://localhost:17633/Account/Login";

            //System.Web.HttpContext.Current.Server.UrlDecode
            string strURL = HttpContext.Current.Request.Url.AbsoluteUri;

            //GlobalContext.Current.UrlReferrerSession = filterContext.HttpContext.Request.Url.AbsoluteUri;

            // HttpCookie cookie = new HttpCookie("returnUrl");
            //// cookie.Path = "/";
            // cookie.Domain = ".test.com";      
            // cookie.Values.Add("memberUrl", returnlUrl);            
            // cookie.Expires = DateTime.Now.AddDays(1);           

            if (HttpContext.Current.Session["memberOnlineInfo"] == null)
            {
                if (HttpContext.Current.Request.Cookies[OperateCookies.cookieName] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies[OperateCookies.cookieName].Values["AccountId"].ToString()))
                {
                    HttpCookie mem = filterContext.HttpContext.Request.Cookies[OperateCookies.cookieName];
                    string strAccountId = mem.Values["AccountId"].ToString().ToDecrypt();
                    long strMemberId = long.Parse(mem.Values["memberId"].ToString().ToDecrypt());

                    IUserAccountService _userService = ServiceLocator.Current.GetInstance<IUserAccountService>();

                    //验证是否存在AccountId和memberId
                    if (_userService.CheckCookies(strAccountId, strMemberId))
                    {
                        //直接执行action

                        var userInfo = new MemberInfo()
                        {
                            AccountId = strAccountId,
                            memberId = strMemberId,
                            LoginName = mem.Values["LoginName"].ToString(),
                            CnName = mem.Values["CnName"].ToString()
                        };
                        GlobalContext.Current.memberOnlineInfo = userInfo;
                    }
                    else
                    {
                        //cookies被改。跳到登录页；登录成功，返回到之前页面                        
                        filterContext.Result = new RedirectResult(strAuth + "?returnUrl=" + strURL);
                    }
                }
                else
                {
                    //cookies没有。跳到登录页；登录成功，返回到之前页面                   
                    filterContext.Result = new RedirectResult(strAuth + "?returnUrl=" + strURL);
                }
            }

            //有Seesion,直接执行下面方法
        }

    }

}
