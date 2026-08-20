using Arch.Common;
using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Weixin;
using Lvy.Trip.Weixin.Mvc.Attributes;
using Lvy.VModels.Base;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 微信后台登录
    /// </summary>
    public class AccountController : Controller
    {
        private ILog logger = LogManager.GetLogger("AccountController");
        private MemberBiz service = new MemberBiz();
        private AccountBiz _accountBiz = new AccountBiz();

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            LoginVModel vModel = new LoginVModel();
            vModel.Account = new CrmAccountModel();
            TempData["ReturnUrl"] = Request.QueryString["url"];

            // 根据发布地址取得商户号
            var platFormBiz = new PlatformBiz();
            string host = Request.Url.Host;
            if (host.Equals("localhost")) host = "yuanwx.sh-cct.cn";
            var platFrom = platFormBiz.GetByHostUrl(host);
            if (null != platFrom)
            {
                var customerBiz = new CustomerBiz();
                var customer = customerBiz.GetById(platFrom.CustomerCode);
                platFrom.CrmCustomer = customer;
                GlobalContext.Current.OwnerInfo = platFrom;    // 初始化的时候吧owner放入cached
                ViewBag.LogoPath = AppSetting.Get("UploadFileRoot") + platFrom.SiteLogoPath;
                ViewBag.CustomerName = customer.ShortName;
                vModel.Account.OwnerCode = platFrom.CustomerCode;
            }

            return View(vModel);
        }

        //
        // POST: /Account/Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVModel vModel)
        {
            var _functionBiz = new FunctionBiz();

            if (vModel.ValidateCode == GlobalContext.Current.ValidateCode)
            {
                CrmAccountModel accountModel = vModel.Account;
                accountModel.Pwd = Toolkit.Security.ToEncrypt(accountModel.Pwd);
                // 用户验证
                var tempModel = _accountBiz.GetByLogin(accountModel.OwnerCode, accountModel.LoginName, accountModel.Pwd);
                if (tempModel != null)
                {
                    FormsAuthentication.SetAuthCookie(accountModel.LoginName, false);
                    // 当前用户
                    GlobalContext.Current.UserInfo = tempModel;
                    // 用户角色
                    GlobalContext.Current.LoginUserRoles = _accountBiz.GetRoleByAccountCode(tempModel.Code);
                    // 用户部门
                    GlobalContext.Current.LoginUserTeams = _accountBiz.GetTeamByAccountCode(tempModel.Code);
                    //
                    GlobalContext.Current.OwnerCode = GlobalContext.Current.UserInfo.OwnerCode;
                }
                else
                {
                    return Content("<script>alert('用户名或密码错误！');location.href='/Account/Login';</script>");
                }
            }
            else
            {
                return Content("<script>alert('验证码错误！');location.href='/Account/Login';</script>");
            }

            if (TempData["ReturnUrl"] != null)
                return Redirect(TempData["ReturnUrl"].ToString());

            return RedirectToAction("Index", "Admin");
        }

        //
        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Login");
        }

        public ActionResult MyAccount()
        {
            string code = GlobalContext.Current.UserInfo.Code;
            var vModel = new AccountEditVModel();
            vModel.Account = _accountBiz.GetById(code);
            vModel.Account.Pwd = Toolkit.Security.ToDecrypt(vModel.Account.Pwd);
            vModel.SexBeans = DictionaryTools.GetEnumsBy(Enums.SexEnum);

            string teamName = string.Join(",", _accountBiz.GetTeamByAccountCode(code).Select(t => t.TeamName).ToArray());

            ViewBag.TeamName = teamName;
            return View(vModel);
        }

        /// <summary>
        /// 更新我的账户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateMyAccount(AccountEditVModel vModel)
        {
            var model = _accountBiz.GetById(vModel.Account.Code);

            //model.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
            model.Name = vModel.Account.Name;
            model.Sex = vModel.Account.Sex;
            model.Mobile = vModel.Account.Mobile;
            model.Phone = vModel.Account.Phone;
            model.Email = vModel.Account.Email;
            model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            model.ModifiedTime = DateTime.Now;

            _accountBiz.Update(model);

            return Json(new { code = "1" });
        }

        [AllowAnonymous]
        public ActionResult ForbidAuth()
        {
            return View();
        }
    }
}