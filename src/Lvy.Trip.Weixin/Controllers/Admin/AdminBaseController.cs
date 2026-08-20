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
    public class AdminBaseController : Controller
    {
        ILog logger = LogManager.GetLogger("BaseController");
        protected string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        protected string secret = WebConfigurationManager.AppSettings["WeixinAppSecret"];
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly MemberBiz _memberBiz = new MemberBiz();
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

        public CrmAccountModel UserInfo
        {
            get
            {
                return GlobalContext.Current.UserInfo;
            }
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
                    Member user = _memberBiz.GetMemberByOpenID(result.openid);
                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(user.EmployeeID))
                        {
                            var u = _accountBiz.GetAccountCustomer(user.EmployeeID);

                            if (u != null)
                            {
                                // ViewData["Message"] = "您还没有补充公司和个人信息！";
                                FormsAuthentication.SetAuthCookie(user.EmployeeID, false);
                                //成功
                                GlobalContext.Current.UserInfo = u; // 当前用户
                                // 用户角色
                                GlobalContext.Current.LoginUserRoles = _accountBiz.GetRoleByAccountCode(u.Code);
                                // 用户部门
                                GlobalContext.Current.LoginUserTeams = _accountBiz.GetTeamByAccountCode(u.Code);

                                GlobalContext.Current.CustomerBy = _biz.GetById(GlobalContext.Current.UserInfo.CustomerCode);

                                GlobalContext.Current.OwnerCode = GlobalContext.Current.UserInfo.OwnerCode;

                                return true;
                            }
                            else
                            {
                                logger.Warn("后台账号不存在 Code:" + user.EmployeeID);
                            }
                        }
                        else
                        {
                            logger.Warn("微信未绑定后台账号 OpenID:" + result.openid);
                        }

                        //Session["WeixinUser"] = user;  // 前台Sesion
                    }
                    else
                    {
                        logger.Warn("微信用户未保存 OpenID:" + result.openid);
                    }
                }
            }
            //else
            //{
            //    Member user = _memberBiz.GetMemberByOpenID("ok6cAuKoOc85PZtNdKlSurbNiaGQ");
            //    var u = _accountBiz.GetAccountCustomer(user.EmployeeID);
            //    // ViewData["Message"] = "您还没有补充公司和个人信息！";
            //    FormsAuthentication.SetAuthCookie(user.EmployeeID, false);
            //    //成功
            //    GlobalContext.Current.UserInfo = u; // 当前用户
            //                                        // 用户角色
            //    GlobalContext.Current.LoginUserRoles = _accountBiz.GetRoleByAccountCode(u.Code);
            //    // 用户部门
            //    GlobalContext.Current.LoginUserTeams = _accountBiz.GetTeamByAccountCode(u.Code);
            //    return true;
            //}

            return false;
        }
    }
}