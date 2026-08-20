using Lvy.Trip.Biz.Crm;
using Lvy.Web.Common;
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Mvc.Attributes
{
    public class LvyAuthAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (GlobalContext.Current.UserInfo == null)
            {
                if (HttpContext.Current.Request.Cookies["uid_cookie"] != null && !HttpContext.Current.Request.Cookies["uid_cookie"].Value.IsNullOrEmpty())
                {
                    string cookieValue = Encoding.Default.GetString(Convert.FromBase64String(HttpContext.Current.Request.Cookies["uid_cookie"].Value));
                    var arr = cookieValue.Split(':');
                    var loginName = arr[0];
                    var loginPwd = arr[1];
                    var ownerCode = arr[2];

                    var tempModel = new AccountBiz().GetByLogin(ownerCode, loginName, loginPwd);
                    if (tempModel != null)
                    {
                        GlobalContext.Current.UserInfo = tempModel;
                        GlobalContext.Current.FunctionList = new FunctionBiz().GetFunctionByAccountCode(tempModel);

                        return;
                    }
                }

                // 没有session的场合
                filterContext.Result = new EmptyResult();
                string urls = "/Account/Login?url=" + HttpContext.Current.Request.Url;
                filterContext.HttpContext.Response.Redirect(urls);
                return;
            }
            else
            {
                return;
            }

            //if (GlobalContext.Current.IsSysAdmin) // sys admin
            //{
            //    return;
            //}

            //string url = "/{0}/{1}".With(filterContext.RouteData.Values["controller"].ToString(), filterContext.RouteData.Values["action"].ToString());

            //// 验证
            //var obj = GlobalContext.Current.FunctionList.FirstOrDefault(a => a.URL != string.Empty && url == a.URL);
            //if (obj == null)
            //{
            //    if (filterContext.HttpContext.Request.IsAjaxRequest())
            //    {
            //        filterContext.Result = new ContentResult()
            //        {
            //            Content = "no"
            //        };
            //    }
            //    else
            //    {
            //        filterContext.Result = new EmptyResult();
            //        filterContext.HttpContext.Response.Redirect("/Account/ForbidAuth");
            //    }
            //}
        }
    }
}