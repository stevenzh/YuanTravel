using Arch.Common;
using Arch.Common.Utils;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.WebSite.Mvc.Attributes;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    public class UserController : BaseController
    {
        private readonly CustomerBiz customerBiz = new CustomerBiz();
        private readonly AccountBiz _biz = new AccountBiz();
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

                var tempModel = _biz.AgentLogin(ownerCode, loginName, loginPwd);
                if (tempModel != null)
                {
                    GlobalContext.Current.UserInfo = tempModel;
                    //GlobalContext.Current.FunctionList = new FunctionBiz().GetFunctionByAccountCode(tempModel.Code);

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
            var _functionBiz = new FunctionBiz();
            if (vModel.ValidateCode == GlobalContext.Current.ValidateCode)
            {
                string Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
                // 用户验证
                var tempModel = _biz.AgentLogin(vModel.Account.OwnerCode, vModel.Account.LoginName, Pwd);
                if (tempModel == null)
                {
                    return Json(new { Code = 500, Message = "用户名或密码错误！" });
                }
                else if (tempModel.CustomerCode == vModel.Account.OwnerCode)
                {
                    return Json(new { Code = 501, Message = "非员工登录入口！" });
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
            if (vModel.ValidateCode == GlobalContext.Current.ValidateCode)
            {
                string Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
                // 用户验证
                var tempModel = _biz.AgentLogin(vModel.Account.OwnerCode, vModel.Account.LoginName, Pwd);
                if (tempModel == null)
                {
                    var url = TempData["ReturnUrl"];
                    return Content("<script>alert('用户名或密码错误！');location.href='/User/Login" + (url == null ? "" : "?url=" + (string)url) + "';</script>");
                }
                else if (tempModel.CustomerCode == vModel.Account.OwnerCode)
                {
                    return Content("<script>alert('非员工登录入口！');location.href='/User/Login';</script>");
                }
                else
                {
                    GlobalContext.Current.UserInfo = tempModel;
                    GlobalContext.Current.OwnerCode = tempModel.OwnerCode;
                    GlobalContext.Current.CustomerBy = customerBiz.GetById(tempModel.CustomerCode);

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
                        string loginName = vModel.Account.LoginName;
                        string pwd = vModel.Account.Pwd;
                        string oid = vModel.Account.OwnerCode;
                        string cookieValue = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(loginName + ":" + pwd + ":" + oid));
                        HttpCookie uidCookie = new HttpCookie("uid_cookie", cookieValue);
                        uidCookie.Expires = DateTime.Now.AddDays(14);
                        Response.Cookies.Add(uidCookie);
                    }
                }
            }
            else
            {
                return Content("<script>alert('验证码错误！location.href='/User/Login';');</script>");//
            }
            if (TempData["ReturnUrl"] != null)
                return Redirect(TempData["ReturnUrl"].ToString());

            return RedirectToAction("Index", "Seller");
        }

        #region 注册

        public ActionResult RemoteLoginName(string userName)
        {
            var accountModel = _biz.GetByLoginName(userName);
            var result = accountModel == null ? true : false;
            return Content(result.ToJsonSerialize());
        }

        [HttpGet]
        public ActionResult CustomerReg()
        {
            CrmAccountModel model = new CrmAccountModel();
            model.CustomerCode = DBTools.GetSeqNo("CrmCustomer");
            return View(model);
        }

        /// <summary>
        /// 客户注册信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DoReg(CrmAccountModel model)
        {
            CustomerRegistrationModel registration = new CustomerRegistrationModel();
            registration.CustomerCode = model.CustomerCode;
            if (!Request.Form["IdCardPath"].IsNullOrEmpty())
                registration.IdCardPath = Request.Form["IdCardPath"];
            else
                registration.IdCardPath = "";
            if (!Request.Form["BusinessLicencePath"].IsNullOrEmpty())
                registration.BusinessLicencePath = Request.Form["BusinessLicencePath"];
            else
                registration.BusinessLicencePath = "";
            string customerCode = _biz.Register(model, Request.Url.Host);

            _biz.AddCustomerRegistration(registration);

            string msg = "请等待我们的工作人员审核！我们会尽快联系您！";
            return Content("<script type=\"text/javascript\">alert('{0}');window.location.href='{1}';</script>".With(msg, "/User/Login"));
        }

        /// <summary>
        /// 本地图片上传
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadPhoto(string customerCode, string imgFile)
        {
            string fileTypes = "gif,jpg,jpeg,png,bmp";
            int maxSize = 3000000;

            Hashtable hash = new Hashtable();

            HttpPostedFileBase file = Request.Files[imgFile];
            if (file == null)
            {
                hash = new Hashtable();
                hash["error"] = 1;
                hash["message"] = "请选择文件";
                return Json(hash, "text/html;charset=UTF-8");
            }

            string fileName = file.FileName;
            string fileExt = Path.GetExtension(fileName).ToLower();

            ArrayList fileTypeList = ArrayList.Adapter(fileTypes.Split(','));

            if (file.InputStream == null || file.InputStream.Length > maxSize)
            {
                hash = new Hashtable();
                hash["error"] = 1;
                hash["message"] = "上传文件大小超过限制";
                return Json(hash, "text/html;charset=UTF-8");
            }

            if (string.IsNullOrEmpty(fileExt) || Array.IndexOf(fileTypes.Split(','), fileExt.Substring(1).ToLower()) == -1)
            {
                hash = new Hashtable();
                hash["error"] = 1;
                hash["message"] = "上传文件扩展名是不允许的扩展名";
                return Json(hash, "text/html;charset=UTF-8");
            }

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = @"customer\{0}".With(customerCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            hash = new Hashtable();
            hash["error"] = 0;
            hash["url"] = response.FilePath.Replace("\\", "/") + response.FileName;

            return Json(hash, "text/html;charset=UTF-8"); ;
        }

        #endregion 注册

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

        /// <summary>
        /// 愿望清单
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult WishList()
        {
            return View();
        }
    }
}