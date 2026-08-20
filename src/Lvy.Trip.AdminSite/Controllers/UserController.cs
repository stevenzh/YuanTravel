using Arch.Common;
using Arch.Common.Utils;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Common;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using System;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    public class UserController : Controller
    {
        public ActionResult ForbidAuth()
        {
            return View();
        }

        /// <summary>
        /// 用户登录-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            if (Request.Cookies["uid_cookie"] != null && !Request.Cookies["uid_cookie"].Value.IsNullOrEmpty())
            {
                string cookieValue = System.Text.Encoding.Default.GetString(Convert.FromBase64String(Request.Cookies["uid_cookie"].Value));
                var arr = cookieValue.Split(':');
                var loginName = arr[0];
                var loginPwd = arr[1];
                var ownerCode = arr[2];

                var tempModel = new AccountBiz().GetByLogin(ownerCode, loginName, loginPwd);
                if (tempModel != null)
                {
                    GlobalContext.Current.UserInfo = tempModel;
                    GlobalContext.Current.FunctionList = new FunctionBiz().GetFunctionByAccountCode(tempModel);

                    if (TempData["ReturnUrl"] != null)
                        return Redirect(TempData["ReturnUrl"].ToString());

                    return RedirectToAction("Index", "Home");
                }
            }

            LoginVModel vModel = new LoginVModel();
            vModel.Account = new CrmAccountModel();
            TempData["ReturnUrl"] = Request.QueryString["url"];

            // 根据发布地址取得商户号
            var platFrom = (SysPlatformModel)this.RouteData.Values["tenant"];
            if (null != platFrom)
            {
                var customerBiz = new CustomerBiz();
                var customer = customerBiz.GetById(platFrom.CustomerCode);
                platFrom.CrmCustomer = customer;
                GlobalContext.Current.OwnerInfo = platFrom;    // 初始化的时候吧owner放入cached
                ViewBag.LogoPath = AppSetting.Get("UploadFileRoot") + customer.LogoPath;
                ViewBag.CustomerName = customer.ShortName;
                vModel.Account.OwnerCode = platFrom.CustomerCode;
            }

            return View(vModel);
        }

        [HttpPost]
        public ActionResult CheckLogin(LoginVModel vModel)
        {
            var _accountBiz = new AccountBiz();
            var _functionBiz = new FunctionBiz();
            if (vModel.ValidateCode == GlobalContext.Current.ValidateCode)
            {
                CrmAccountModel accountModel = vModel.Account;
                accountModel.Pwd = Toolkit.Security.ToEncrypt(accountModel.Pwd);
                // 用户验证
                var tempModel = _accountBiz.GetByLogin(accountModel.OwnerCode, accountModel.LoginName, accountModel.Pwd);
                if (tempModel == null)
                {
                    return Json(new { Code = 500, Message = "用户名或密码错误！" });
                }
            }
            else
            {
                return Json(new { Code = 501, Message = "验证码错误！" });
            }

            return Json(new { Code = 200, Message = "登录成功！" });
        }

        /// <summary>
        /// 用户登录-操作
        /// </summary>
        /// <returns></returns>
        public ActionResult LoginIn(LoginVModel vModel)
        {
            var _accountBiz = new AccountBiz();
            var _functionBiz = new FunctionBiz();

            if (vModel.ValidateCode == GlobalContext.Current.ValidateCode)
            {
                CrmAccountModel accountModel = vModel.Account;
                accountModel.Pwd = Toolkit.Security.ToEncrypt(accountModel.Pwd);
                // 用户验证
                var tempModel = _accountBiz.GetByLogin(accountModel.OwnerCode, accountModel.LoginName, accountModel.Pwd);
                if (tempModel != null)
                {
                    GlobalContext.Current.UserInfo = tempModel;
                    GlobalContext.Current.FunctionList = _functionBiz.GetFunctionByAccountCode(tempModel);
                    GlobalContext.Current.LoginUserRoles = _accountBiz.GetRoleByAccountCode(tempModel.Code);
                    GlobalContext.Current.LoginUserTeams = _accountBiz.GetTeamByAccountCode(tempModel.Code);
                    GlobalContext.Current.OwnerCode = tempModel.OwnerCode;

                    // 暂时关闭
                    //if (!string.IsNullOrEmpty(tempModel.OpenID))
                    //{
                    //    // 发送登录通知
                    //    var first = "您好，您的账号于电脑端登录";
                    //    var param1 = tempModel.LoginName;
                    //    var param2 = "未知";
                    //    var param3 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //    var remark = "感谢您的登录！";
                    //    SendMessagClient.SendTemplateMessage(tempModel.OpenID, "gunCr4m5dY0ftvk7Nfi9izhz6YldziliEUcE6LPmMJQ", first, param1, param2, param3, "", "", remark);
                    //}

                    if (vModel.AutoLogin > 0)
                    {
                        string loginName = accountModel.LoginName;
                        string pwd = accountModel.Pwd;
                        string oid = accountModel.OwnerCode;
                        string cookieValue = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(loginName + ":" + pwd + ":" + oid));
                        HttpCookie uidCookie = new HttpCookie("uid_cookie", cookieValue);
                        uidCookie.Expires = DateTime.Now.AddDays(14);
                        Response.Cookies.Add(uidCookie);
                    }
                }
                else
                {
                    return Content("<script>alert('用户名或密码错误！location.href='/User/Login';');</script>");//
                }
            }
            else
            {
                return Content("<script>alert('验证码错误！location.href='/User/Login';');</script>");//
            }
            if (TempData["ReturnUrl"] != null)
                return Redirect(TempData["ReturnUrl"].ToString());

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            Response.Cookies["uid_cookie"].Expires = DateTime.Now.AddDays(-1);
            return RedirectToAction("Login", "User");
        }

        public ActionResult InitMailDialog()
        {
            return PartialView("MailSendDialog");
        }

        #region 微信登录

        public ActionResult CreateWeixinQr()
        {
            string code = "qln_" + DateTime.Now.ToString("yyyyMMddHHmm") + StringUtils.RandomNum(6);
            GlobalContext.Current.WeixinQrCode = code;
            string ticket = SendMessagClient.CreateQrCode(code, "60");
            return Content(ticket);
        }

        public ActionResult CheckWeixinQr()
        {
            if (GlobalContext.Current.WeixinQrCode == null)
                return Content("None");
            var code = CacheContext.Current.Get(GlobalContext.Current.WeixinQrCode);
            if (code == null)
            {
                return Content("Fail");
            }
            return Content("OK");
        }

        public ActionResult LoginWeixinQr()
        {
            var accountCode = CacheContext.Current.Get(GlobalContext.Current.WeixinQrCode).ToString();
            if (accountCode == null)
                return RedirectToAction("Login", "User");

            var _accountBiz = new AccountBiz();
            var _functionBiz = new FunctionBiz();

            var tempModel = _accountBiz.GetAccountCustomer(accountCode);
            if (tempModel != null)
            {
                GlobalContext.Current.UserInfo = tempModel;
                GlobalContext.Current.FunctionList = _functionBiz.GetFunctionByAccountCode(tempModel);
                GlobalContext.Current.LoginUserRoles = _accountBiz.GetRoleByAccountCode(tempModel.Code);
                GlobalContext.Current.LoginUserTeams = _accountBiz.GetTeamByAccountCode(tempModel.Code);
                GlobalContext.Current.OwnerCode = tempModel.OwnerCode;

                return RedirectToAction("Desktop", "Admin");
            }

            return RedirectToAction("Login", "User");
        }

        #endregion 微信登录
    }
}