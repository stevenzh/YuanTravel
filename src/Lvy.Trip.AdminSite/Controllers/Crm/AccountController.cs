using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Common;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.FileUpload;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Crm
{
    /// <summary>
    /// 商户功能 - 账户模块控制器
    /// </summary>
    public class AccountController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(AccountController));
        private readonly AccountBiz _biz = new AccountBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();

        /// <summary>
        /// 查询账户
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Search(AccountVModel vModel)
        {
            // 取得查询分页条件
            var q = (AccountVModel)CacheContext.Current.Get(Consts.PageAccountController + GlobalContext.Current.UserInfo.Code);
            if (q != null && vModel.FirstTime)
                vModel = q;

            vModel.IsEmployee = 1; // 固定当前商户
            vModel.Account.OwnerCode = UserInfo.OwnerCode;
            vModel.OwnerCode = UserInfo.OwnerCode;

            ViewBag.DepartCodes = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum).ToSelectListFor();

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            ViewBag.AccountTeamBeans = _teamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);

            // 保存查询分页条件
            CacheContext.Current.Add(Consts.PageAccountController + GlobalContext.Current.UserInfo.Code, vModel, Consts.OutputCacheDuration2);

            vModel.Accounts = _biz.GetPagedList(vModel);
            vModel.FirstTime = false;
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var vModel = InitEditVModel();
            vModel.Account = new CrmAccountModel();
            vModel.Account.OwnerCode = Web.Common.GlobalContext.Current.OwnerCode;
            vModel.Account.CustomerCode = Web.Common.GlobalContext.Current.OwnerCode;
            return View(vModel);
        }

        public ActionResult Edit(string code)
        {
            var vModel = InitEditVModel();
            //编辑的时候需要获取已关联的目的地和角色
            //vModel.SelectedDestIds = _biz.GetSelectedDestIds(code);
            vModel.SelectedRoleIds = _biz.GetSelectedRoleIds(code);
            vModel.SelectedTeamIds = _biz.GetTeamByAccountCode(code).Select(t => t.TeamID).ToArray();

            #region 组、角色下拉选择的值

            string selectItem = "";
            foreach (var item in vModel.SelectedTeamIds)
            {
                selectItem += "\'" + item + "\',";
            }
            ViewBag.SelectTeamItem = selectItem.TrimEnd(',');
            selectItem = "";
            foreach (var item in vModel.SelectedRoleIds)
            {
                selectItem += "\'" + item + "\',";
            }
            ViewBag.SelectRoleItem = selectItem.TrimEnd(',');

            #endregion 组、角色下拉选择的值

            vModel.Account = _biz.GetById(code);

            return View(vModel);
        }

        /// <summary>
        /// 添加账户
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(AccountEditVModel vModel)
        {
            vModel.Account.Code = DBTools.GetSeqNo("CrmAccount");
            if (!string.IsNullOrEmpty(vModel.Account.Pwd))
                vModel.Account.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
            vModel.Account.ModifiedBy = UserInfo.Code;
            vModel.Account.ModifiedTime = DateTime.Now;
            vModel.Account.AccountType = 3; // 普通员工
            vModel.Account.IsValid = 1;
            vModel.Account.SalerState = 1;
            vModel.Account.OwnerCode = OwnerCode;
            _biz.AddTrans(vModel);

            // clear cache
            CacheContext.Current.Remove(Consts.AccountStrDic);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 更新账户
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update(AccountEditVModel vModel)
        {
            var account = _biz.GetById(vModel.Account.Code);
            account.Name = vModel.Account.Name;
            account.LoginName = vModel.Account.LoginName;
            account.Sex = vModel.Account.Sex;
            account.Mobile = vModel.Account.Mobile;
            account.Email = vModel.Account.Email;
            account.Phone = vModel.Account.Phone;
            account.QQ = vModel.Account.QQ;
            account.DepartCode = vModel.Account.DepartCode;
            //account.CustomerCode = vModel.Account.CustomerCode;
            //account.CustomerName = vModel.Account.CustomerName;
            account.ModifiedBy = UserInfo.Code;
            account.ModifiedTime = DateTime.Now;

            vModel.Account = account;
            _biz.UpdateTrans(vModel);

            // clear cache
            CacheContext.Current.Remove(Consts.AccountStrDic);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="newPwd"></param>
        /// <returns></returns>
        public ActionResult ResetPwd(string accountCode, string newPwd = "888888")
        {
            // 如果密码重置
            int flag = _biz.ResetPwd(accountCode, newPwd);

            return Content(flag.ToString());
        }

        public ActionResult CheckSalesTeam(string accountCode, string[] teamId)
        {
            var rt = new { Code = "0", TeamId = "" };

            bool bb = false;
            var teams = _biz.GetTeamByAccountCode(accountCode).Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToList(); // 获得销售部门
            foreach (var item in teams)
            {
                if (!teamId.Contains(item.TeamID))  // 消失的销售部门
                {
                    var vm = _customerBiz.GetCustomerBySales(item.TeamID, accountCode);

                    if (vm.CustomerCount + vm.ContactCount > 0)
                    {
                        bb = true;
                        break;
                    }
                }
            }

            if (bb == false)
            {
                return Json(rt);
            }

            // 现有部门是否有唯一销售部门
            var dd = _teamBiz.HasSalesTeam(teamId, OwnerCode);
            if (dd.Count == 1)
            {
                rt = new { Code = "1", TeamId = dd.FirstOrDefault().TeamID };
            }
            else
            {
                rt = new { Code = "2", TeamId = "" };
            }

            return Json(rt);
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <returns></returns>
        public ActionResult SetValidState(string code)
        {
            var obj = _biz.GetById(code);
            if (obj.IsValid == 0 && _biz.GetByLoginName(obj.LoginName) != null)
            {
                return Content("0");
            }
            obj.IsValid = obj.IsValid == 1 ? 0 : 1;
            obj.ModifiedBy = UserInfo.Code;
            obj.ModifiedTime = DateTime.Now;
            _biz.Update(obj);

            return Content("1");
        }

        /// <summary>
        /// 初始化编辑Vmodel
        /// </summary>
        /// <returns></returns>
        private AccountEditVModel InitEditVModel()
        {
            var vModel = new AccountEditVModel();
            vModel.RoleBeans = _biz.GetAllRoles(GlobalContext.Current.OwnerCode);
            //vModel.DestinationBeans = _biz.GetAllDestBeans();
            vModel.CustomerBeans = _biz.GetAllCustomerBeans(UserInfo.OwnerCode);
            vModel.AccountTeamBeans = _biz.GetAllTeamBeans(UserInfo.OwnerCode);
            vModel.DepartBeans = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum);
            vModel.SexBeans = DictionaryTools.GetEnumsBy(Enums.SexEnum);
            return vModel;
        }

        #region 我的账户

        /// <summary>
        /// 进入我的账户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult MyAccount()
        {
            string code = UserInfo.Code;
            var vModel = new AccountEditVModel();
            vModel.Account = _biz.GetById(code);
            vModel.Account.Pwd = Toolkit.Security.ToDecrypt(vModel.Account.Pwd);
            vModel.SexBeans = DictionaryTools.GetEnumsBy(Enums.SexEnum);

            string teamName = string.Join(",", _biz.GetTeamByAccountCode(code).Select(t => t.TeamName).ToArray());

            ViewBag.TeamName = teamName;
            return View(vModel);
        }

        /// <summary>
        /// 微信生产但参数二维码（加密+当前用户ID）， 手机扫描后根据 OpenID 和后台ID 绑定
        /// </summary>
        /// <returns></returns>
        public ActionResult BindingWeixin()
        {
            string secid = Web.Common.GlobalContext.Current.UserInfo.Code;
            TempData["ticket"] = SendMessagClient.CreateQrCode(secid, "60");
            return View();
        }

        /// <summary>
        /// 更新我的账户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateMyAccount(AccountEditVModel vModel)
        {
            var model = _biz.GetById(vModel.Account.Code);

            //model.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
            model.Name = vModel.Account.Name;
            model.Sex = vModel.Account.Sex;
            model.Mobile = vModel.Account.Mobile;
            model.Phone = vModel.Account.Phone;
            model.Email = vModel.Account.Email;
            model.QQ = vModel.Account.QQ;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            var path = UploadFile("UploadFile", vModel.Account.Code);
            if (!string.IsNullOrEmpty(path))
            {
                model.ProfilePath = path;
            }
            _biz.Update(model);

            // clear cache
            CacheContext.Current.Remove(Consts.AccountStrDic);
            return SaveResult("1", Url.Action("MyAccount"));
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        private string UploadFile(string fileName, string accountCode)
        {
            HttpPostedFileBase file = Request.Files[fileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            var request = new UploadFileRequest();
            request.FileName = filename;
            // request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            request.FileStream = Toolkit.Image.StreamToBytes(ImageTools.GreateMiniImage(file.InputStream, 80, 80, "Auto"));
            request.VirtualPath = @"accounts\" + accountCode;
            UploadServiceClient client = new UploadServiceClient();

            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        #endregion 我的账户

        #region 验证账户名称是否存在

        public ActionResult CheckLoginName(string loginName)
        {
            var accountModel = _biz.GetByLoginName(loginName);
            var result = 0;
            result = accountModel == null ? 0 : 1;

            return Content(result.ToString());
        }

        #endregion 验证账户名称是否存在

        #region 审核客户管理

        /// <summary>
        ///  查询未审核，审核不通过的账号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchNoAuditAccount(CrmAccountModel model)
        {
            var models = _biz.SearchNoAuditAccount(model, UserInfo.OwnerCode);
            return View(models);
        }

        /// <summary>
        /// 编辑未审核账号信息
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public ActionResult EditNoAuditAccount(string accountCode)
        {
            AuditAccountEditVModel vModel = new AuditAccountEditVModel();

            vModel.Account = _biz.GetById(accountCode);
            vModel.Customer = new CustomerBiz().GetById(vModel.Account.CustomerCode);
            vModel.Registration = _biz.GetCustomerRegistration(vModel.Account.CustomerCode);

            InitCustomerInitVModel(vModel);

            return View(vModel);
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        private void InitCustomerInitVModel(AuditAccountEditVModel vModel)
        {
            vModel.CustomerBeans = _biz.GetAllCustomerBeans(UserInfo.OwnerCode);
            vModel.DepartBeans = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum);
            vModel.SexBeans = DictionaryTools.GetEnumsBy(Enums.SexEnum);

            ViewBag.Salers = new CustomerBiz().GetTeamSales(UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            ViewBag.PaymentTypes = DictionaryTools.GetEnumsBy(Enums.PaymentTypeEnum).ToSelectListFor();

            //var kvs = new List<KeyValueBean>();
            //kvs.Add(new KeyValueBean() { Key = "1", Value = "供应商" });
            //kvs.Add(new KeyValueBean() { Key = "2", Value = "分销商" });
            //ViewBag.CustomerTypes = kvs;
        }

        /// <summary>
        /// 审核保存账号信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AuditAccount(AuditAccountEditVModel vModel)
        {
            int state = _biz.AuditAccount(vModel, UserInfo);
            if (state <= 0)
            {
                return AlertResult("账号已经存在。请重新注册！！！");
            }
            return RedirectToAction("EditNoAuditAccount", new { accountCode = vModel.Account.Code });
        }

        /// <summary>
        /// 仅保存账号信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult OnlySaveAccount(AuditAccountEditVModel vModel)
        {
            vModel.Customer.CustomerState = 0;

            return AuditAccount(vModel);
        }

        /// <summary>
        /// 保存审核通过
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AuditPass(AuditAccountEditVModel vModel)
        {
            vModel.Customer.CustomerState = 1;

            return AuditAccount(vModel);
        }

        /// <summary>
        /// 保存 审核不通过
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AuditNoPass(AuditAccountEditVModel vModel)
        {
            vModel.Customer.CustomerState = 2;

            return AuditAccount(vModel);
        }

        #endregion 审核客户管理

        [AllowAnonymous]
        public ActionResult Select2List(string term, string _type, string q)
        {
            var CacheKey = "CacheKey=Account|Search:1001";
            var _getModel = CacheContext.Current.Get(CacheKey);
            var users = new List<CrmAccountModel>();
            if (_getModel == null)
            {
                users = _biz.GetAllAccount(Web.Common.GlobalContext.Current.UserInfo.CustomerCode);
                CacheContext.Current.Add(CacheKey, users, Convert.ToInt32(AppSetting.Get("cacheDateTime")));
            }
            else
            {
                users = ((List<CrmAccountModel>)_getModel);
            }

            var ll = (from cc in users
                      select new
                      {
                          code = cc.Code,
                          name = cc.Name,
                          pinyin = cc.Name.ConvertPinYin().ToUpper()
                      }).ToList().Where(t => t.pinyin.Contains(q.ToUpper()) || t.name.Contains(q)).ToList();

            var tt = new { ReturnMsg = "0000", List = ll, TotalCount = ll.Count };

            return Json(tt, JsonRequestBehavior.AllowGet);
        }
    }
}