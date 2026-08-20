using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;
using Lvy.Models.JModels;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Common;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Crm
{
    /// <summary>
    /// 联系人管理（客户和供应商）
    ///
    /// 权限设置  销售总监产所有客户  销售组长 销售组所以客户  销售  只能查看编辑自己的客户
    /// </summary>
    public class ContactController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(ContactController));
        private readonly AccountBiz _biz = new AccountBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly OrderBiz _orderBiz = new OrderBiz();
        private readonly TaskBiz _taskBiz = new TaskBiz();

        /// <summary>
        /// 查询账户
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Search(AccountVModel vModel)
        {
            // 取得查询分页条件
            var q = (AccountVModel)CacheContext.Current.Get(Consts.PageContactController + GlobalContext.Current.UserInfo.Code);
            if (q != null && vModel.FirstTime)
                vModel = q;

            vModel.IsEmployee = 0; // 外部联系人
            vModel.Account.OwnerCode = UserInfo.OwnerCode;
            vModel.OwnerCode = UserInfo.OwnerCode;
            // ---------------------------------------------------------------------------------------------------
            var teams = new List<SelectListItem>();
            var sales = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            // 根据用户角色 锁定过滤
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                vModel.IsLeader = 1;
                var temp = _teamBiz.GetSalesTeams(OwnerCode);
                teams = temp.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(vModel.CrmTeamId))
                {
                    vModel.CrmTeamId = temp.FirstOrDefault().TeamID;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                vModel.IsLeader = 1;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(vModel.CrmTeamId))
                {
                    vModel.CrmTeamId = GlobalContext.Current.LoginUserTeams.FirstOrDefault().TeamID;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                vModel.IsLeader = 0;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(vModel.CrmTeamId))
                {
                    vModel.CrmTeamId = GlobalContext.Current.LoginUserTeams.FirstOrDefault().TeamID;
                }
                vModel.SalesCode = GlobalContext.Current.UserInfo.Code;
            }

            // 过滤销售
            if (!String.IsNullOrEmpty(vModel.CrmTeamId))
                sales = _customerBiz.GetTeamUsersByTeamId(vModel.CrmTeamId, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            ViewBag.Teams = teams;
            ViewBag.Salers = sales;
            // ---------------------------------------------------------------------------------------------------------

            // 保存查询分页条件
            CacheContext.Current.Add(Consts.PageContactController + GlobalContext.Current.UserInfo.Code, vModel, Consts.OutputCacheDuration2);

            vModel.Accounts = _biz.GetPagedList(vModel);
            vModel.FirstTime = false;
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        [HttpGet]
        public ActionResult Create(string customerCode, string CustomerName)
        {
            var vModel = InitEditVModel();
            vModel.Account = new CrmAccountModel();

            if (!string.IsNullOrEmpty(customerCode))
                vModel.Account.CustomerName = DictionaryTools.GetCachedCustomer(customerCode).Name;

            if (!string.IsNullOrEmpty(CustomerName))
                vModel.Account.CustomerName = CustomerName;

            var team = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).FirstOrDefault();
            if (team != null)
            {
                vModel.Account.TeamID = team.TeamID;
                vModel.Account.SalerCode = GlobalContext.Current.UserInfo.Code;
                vModel.Account.Pwd = "888888";
                ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(vModel.Account.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }
            else
            {
                ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }

            return View(vModel);
        }

        public ActionResult Edit(string code)
        {
            var vModel = InitEditVModel();
            vModel.Account = _biz.GetById(code);

            // 初始化下拉列表
            if (string.IsNullOrEmpty(vModel.Account.TeamID))
                ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            else
                ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(vModel.Account.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            return View(vModel);
        }

        public ActionResult Details(string code)
        {
            var vModel = InitEditVModel();
            vModel.Account = _biz.GetById(code);
            vModel.Customer = _customerBiz.GetById(vModel.Account.CustomerCode);

            // 初始化下拉列表
            if (string.IsNullOrEmpty(vModel.Account.TeamID))
                ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            else
                ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(vModel.Account.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            return View(vModel);
        }

        /// <summary>
        /// 提出审核到组长微信
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult SendAuditMsg(string code)
        {
            try
            {
                // 联系人所属组 审核
                var model = _biz.GetById(code);
                var customer = _customerBiz.GetById(model.CustomerCode);
                if (customer.TeamID != model.TeamID)
                {
                    // 添加任务
                    BaseTaskModel task = new BaseTaskModel();
                    var leader = _teamBiz.GetTeamLeader(GlobalContext.Current.OwnerCode, model.TeamID);
                    var leader1 = _teamBiz.GetTeamLeader(GlobalContext.Current.OwnerCode, customer.TeamID);
                    task.CreatedTime = model.ModifiedTime;
                    task.Status = 0;
                    task.TeamID = model.TeamID;
                    task.Originator = GlobalContext.Current.UserInfo.Code;
                    task.OwnerCode = GlobalContext.Current.OwnerCode;
                    task.WorkFlowID = 6002;
                    task.WorkmanTeam = model.TeamID;
                    task.Workman = leader.Code;
                    task.Contents = "添加联系人";
                    TaskJModel jm = new TaskJModel
                    {
                        ContactCode = model.Code,
                        ContactName = model.Name,
                        CustomerCode = customer.Code,
                        CustomerName = customer.Name,
                        WorkmanTeam = customer.TeamID,
                        Workman = leader1.Code
                    };
                    task.JsonData = jm.ToJsonSerialize();
                    var newid = _taskBiz.AddTask(task);

                    // 子任务
                    BaseTaskModel sub = new BaseTaskModel();
                    sub.ParentID = newid;
                    sub.CreatedTime = task.CreatedTime;
                    sub.Status = 0;
                    sub.TeamID = task.TeamID;
                    sub.Originator = task.Originator;
                    sub.OwnerCode = task.OwnerCode;
                    sub.WorkFlowID = task.WorkFlowID;
                    sub.WorkmanTeam = jm.WorkmanTeam;
                    sub.Workman = jm.Workman;
                    sub.Contents = task.Contents;
                    sub.JsonData = task.JsonData;
                    _taskBiz.AddTask(sub);
                }
                else
                {
                    var leader = _teamBiz.GetTeamLeader(UserInfo.OwnerCode, customer.TeamID); //
                    if (!string.IsNullOrEmpty(leader.OpenID))
                    {
                        var first = string.Format("客户名称：{0}", model.Name);
                        var param1 = GlobalContext.Current.UserInfo.Name;
                        var param2 = DateTime.Now.ToString();
                        var param3 = "联系人审核";
                        var param4 = code;
                        //var remark1 = string.Format("remark");
                        SendMessagClient.SendTemplateMessage(leader.OpenID, "zx_5OoMRcAEr5YkHoOooKbRwqaueXMpXjlQaQuNHGmc", first, param1, param2, param3, param4, "", "");
                    }
                }

                return Content("yes");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
                //throw;
            }
        }

        /// <summary>
        /// 审核结果 并发送关联销售
        /// </summary>
        /// <param name="code">联系人编码</param>
        /// <param name="remark">备注</param>
        /// <param name="state">审核结果</param>
        /// <returns></returns>
        public ActionResult AuditContact(string code, string remark, int state = 0)
        {
            var model = _biz.GetById(code);
            var customer = _customerBiz.GetById(model.CustomerCode);
            if (model.TeamID == customer.TeamID)
            {
                model.SalerState = state;//设置为审核状态
            }
            else if (state == 1)   // 选择通过
            {
                model.SalerState = 1;
            }

            int i = _biz.Update(model);

            if (i > 0)
            {
                // 记录日志
                LogBiz.WriteConttactLog(UserInfo.OwnerCode, model.Code, "", model.TeamID, GlobalContext.Current.UserInfo.Code, (state == 1 ? "客户审核通过" : "客户审核不通过"));

                // 微信通知销售
                var sales = _biz.GetAccountCustomer(model.SalerCode);
                if (!string.IsNullOrEmpty(sales.OpenID))
                {
                    var first = string.Format("联系人姓名：{0}", model.Name);
                    var param1 = (state == 1 ? "联系人审核通过" : "联系人审核不通过");
                    var param3 = code;
                    var remark1 = "操作人:" + GlobalContext.Current.UserInfo.Name + "\r" + remark;
                    SendMessagClient.SendTemplateMessage(sales.OpenID, "ILdChJL_b9gREEWboGh7u3WtVsgat9kvfNznLRp79no", first, param1, DateTime.Now.ToString(), param3, "", "", remark);
                }
            }

            return Json(new { Code = i }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加账户
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(AccountEditVModel vModel)
        {
            var customer = _customerBiz.GetById(vModel.Account.CustomerCode);
            vModel.Account.Code = DBTools.GetSeqNo("CrmAccount");

            vModel.Account.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
            vModel.Account.ModifiedBy = UserInfo.Code;
            vModel.Account.ModifiedTime = DateTime.Now;
            vModel.Account.AccountType = 9; // 普通员工
            vModel.Account.IsValid = 1;
            vModel.Account.OwnerCode = OwnerCode;
            if (customer.TeamID == vModel.Account.TeamID && customer.CustomerState == 1)
            {
                vModel.Account.SalerState = 1;
            }

            // 设置角色
            var role = new List<String>();
            if (customer.IsDistributors)
            {
                role.Add("5");
            }
            if (customer.IsSupplier)
            {
                role.Add("4");
            }

            _biz.AddContact(vModel.Account, role.ToArray());

            if (customer.TeamID != vModel.Account.TeamID)    // 组外添加任务
            {
                BaseTaskModel task = new BaseTaskModel();
                var leader = _teamBiz.GetTeamLeader(GlobalContext.Current.OwnerCode, vModel.Account.TeamID);
                var leader1 = _teamBiz.GetTeamLeader(GlobalContext.Current.OwnerCode, customer.TeamID);
                task.CreatedTime = vModel.Account.ModifiedTime;
                task.Status = 0;
                task.TeamID = vModel.Account.TeamID;
                task.Originator = GlobalContext.Current.UserInfo.Code;
                task.OwnerCode = GlobalContext.Current.OwnerCode;
                task.WorkFlowID = 6002;
                task.WorkmanTeam = vModel.Account.TeamID;
                task.Workman = leader.Code;
                task.Contents = "添加联系人";
                TaskJModel jm = new TaskJModel
                {
                    ContactCode = vModel.Account.Code,
                    ContactName = vModel.Account.Name,
                    CustomerCode = customer.Code,
                    CustomerName = customer.Name,
                    WorkmanTeam = customer.TeamID,
                    Workman = leader1.Code
                };
                task.JsonData = jm.ToJsonSerialize();
                var newid = _taskBiz.AddTask(task);

                // 子任务
                BaseTaskModel sub = new BaseTaskModel();
                sub.ParentID = newid;
                sub.CreatedTime = task.CreatedTime;
                sub.Status = 0;
                sub.TeamID = task.TeamID;
                sub.Originator = task.Originator;
                sub.OwnerCode = task.OwnerCode;
                sub.WorkFlowID = task.WorkFlowID;
                sub.WorkmanTeam = jm.WorkmanTeam;
                sub.Workman = jm.Workman;
                sub.Contents = task.Contents;
                sub.JsonData = task.JsonData;
                _taskBiz.AddTask(sub);
            }

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
            var customer = _customerBiz.GetById(vModel.Account.CustomerCode);
            var account = _biz.GetById(vModel.Account.Code);
            if (vModel.Account.SalerState != 1)  // 审核后不能修改
            {
                account.Name = vModel.Account.Name;
                account.LoginName = vModel.Account.LoginName;
                account.Sex = vModel.Account.Sex;
                account.Mobile = vModel.Account.Mobile;
                account.Email = vModel.Account.Email;
                account.Phone = vModel.Account.Phone;
                account.QQ = vModel.Account.QQ;
                //account.CustomerCode = vModel.Account.CustomerCode;
                account.SalerCode = vModel.Account.SalerCode;
                account.TeamID = vModel.Account.TeamID;
            }

            account.Remarks = vModel.Account.Remarks;
            account.ModifiedBy = UserInfo.Code;
            account.ModifiedTime = DateTime.Now;


            // 设置角色
            var role = new List<String>();
            if (customer.IsDistributors)
            {
                role.Add("5");
            }
            if (customer.IsSupplier)
            {
                role.Add("4");
            }

            _biz.UpdateContact(account, role.ToArray());

            // clear cache
            CacheContext.Current.Remove(Consts.AccountStrDic);
            return RedirectToAction("Search");
        }

        public ActionResult ResetPwd(string accountCode, string newPwd = "888888")
        {
            // 如果密码重置
            int row = _biz.ResetPwd(accountCode, newPwd);

            return Content(row.ToString());
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <returns></returns>
        public ActionResult SetValidState(string code)
        {
            var obj = _biz.GetById(code);
            if (obj.IsValid == 0 && !string.IsNullOrEmpty(obj.LoginName))
            {
                if (_biz.GetByLoginName(obj.LoginName) != null)
                    return AlertResult("该用户名已存在！");
            }
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
            var teams = new List<SelectListItem>();

            // 根据用户角色 锁定过滤
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                vModel.IsLeader = 1;
                teams = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                vModel.IsLeader = 1;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                vModel.IsLeader = 0;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }

            ViewBag.Teams = teams;
            vModel.SexBeans = DictionaryTools.GetEnumsBy(Enums.SexEnum);
            return vModel;
        }

        /// <summary>
        /// 根据信箱验证重复
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public ActionResult CheckContactEmail(string email, string code)
        {
            var accountModel = _biz.CheckContactEmail(GlobalContext.Current.OwnerCode, email, code);
            var result = 0;
            result = accountModel == null ? 0 : 1;

            return Content(result.ToString());
        }

        public ActionResult CheckContactMobile(string mobile, string code)
        {
            var accountModel = _biz.CheckContactMobile(GlobalContext.Current.OwnerCode, mobile, code);
            var result = 0;
            result = accountModel == null ? 0 : 1;

            return Content(result.ToString());
        }

        /// <summary>
        /// 同一个客户不能有重复姓名的联系人
        /// </summary>
        /// <param name="customerName">客户名称</param>
        /// <param name="name">联系人姓名</param>
        /// <param name="code">联系人编号</param>
        /// <returns></returns>
        public ActionResult CheckContactName(string customerName, string name, string code)
        {
            var rt = new { Code = "0", Message = "", ContactCode = "", WorkFlowId = "" };

            var teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToList();  //当前用户的所有组
            var customer = _customerBiz.GetByCustomerName(GlobalContext.Current.OwnerCode, customerName, "");
            var contact = _biz.CheckContactName(customerName, name, code);
            bool inteam = teams.Any(t => t.TeamID == customer.TeamID);  // 客户是否在我的组

            if (contact != null)  // 已存在重名联系人
            {
                if (contact.SalerCode == GlobalContext.Current.UserInfo.Code)    // 已存在我的联系人中
                {
                    rt = new { Code = "2", Message = "已存在我的联系人中", ContactCode = contact.Code, WorkFlowId = "" };
                    return Json(rt);
                }

                // 检查是否有订单
                var order = _orderBiz.LastOrderByContact(contact.Code);
                string wk = "6001";
                string mmm = " 联系人已存在";
                if (teams.Any(t => t.TeamID == contact.TeamID) == false)  // 当前用户不在 联系人所在组
                {
                    // 不同组？
                    mmm += "属" + DictionaryTools.GetCachedTeam(contact.TeamID).TeamName;
                    wk = "6004";
                }
                if (order != null)
                    mmm += "，最晚订单是" + order.CreatedTime.ToString("yyyy-MM-dd");

                rt = new { Code = "1", Message = (order != null ? mmm : "联系人已存在，无订单"), ContactCode = contact.Code, WorkFlowId = wk };
            }
            else if (inteam == false)   // 没有重名
            {
                rt = new { Code = "3", Message = "所在客户不在我的组，需要所在组长审核", ContactCode = "", WorkFlowId = "6002" };
            }

            return Json(rt);
        }
    }
}