using Arch.Common;
using Arch.Common.Utils;
using log4net;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 平台功能 - 账户模块控制器
    /// </summary>
    public class UserAdminController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(UserAdminController));
        private readonly AccountBiz _biz = new AccountBiz();

        /// <summary>
        /// 查询账户
        /// </summary>
        /// <returns></returns>
        public ActionResult Search(AccountVModel vModel)
        {
            if (vModel == null)
                vModel = new AccountVModel();
            if (vModel.Account == null)
                vModel.Account = new CrmAccountModel();
            if (vModel.Accounts == null)
                vModel.Accounts = new PagedList<CrmAccountModel>();

            vModel.Accounts = _biz.AdminGetPagedList(vModel);

            ViewBag.DepartCodes = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum).ToSelectListFor();
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var vModel = InitEditVModel();
            vModel.Account = new CrmAccountModel();
            return View(vModel);
        }

        public ActionResult Edit(string code)
        {
            var vModel = InitEditVModel();
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

            vModel.Account.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
            vModel.Account.ModifiedBy = UserInfo.Code;
            vModel.Account.ModifiedTime = DateTime.Now;
            vModel.Account.AccountType = 2;
            vModel.Account.IsValid = 1;
            vModel.Account.OwnerCode = vModel.Account.CustomerCode;
            _biz.AdminAddTrans(vModel);
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
            // 如果密码重置
            if (vModel.Account.Pwd.Equals("123456"))
                vModel.Account.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);

            vModel.Account.ModifiedBy = UserInfo.Code;
            vModel.Account.ModifiedTime = DateTime.Now;
            vModel.Account.OwnerCode = vModel.Account.CustomerCode;
            _biz.AdminUpdateTrans(vModel);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <returns></returns>
        public ActionResult SetValidState(string code)
        {
            var obj = _biz.GetById(code);

            obj.IsValid = obj.IsValid == 1 ? 0 : 1;
            obj.ModifiedBy = UserInfo.Code;
            obj.ModifiedTime = DateTime.Now;
            _biz.Update(obj);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 初始化编辑Vmodel
        /// </summary>
        /// <returns></returns>
        private AccountEditVModel InitEditVModel()
        {
            var vModel = new AccountEditVModel();
            //vModel.RoleBeans = _biz.GetAllRoles();
            //vModel.DestinationBeans = _biz.GetAllDestBeans();
            vModel.CustomerBeans = _biz.AdminGetAllCustomerBeans();
            vModel.DepartBeans = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum);
            vModel.SexBeans = DictionaryTools.GetEnumsBy(Enums.SexEnum);
            vModel.AccountTypeBeans = DictionaryTools.GetEnumsBy(Enums.AccountTypeEnum);
            return vModel;
        }
    }
}