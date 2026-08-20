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
using Lvy.Trip.Weixin.Models;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{

    /// <summary>
    /// 联系人管理 CRM
    /// </summary>
    [Authorize]
    public class ContactController : AdminBaseController
    {
        private ILog logger = LogManager.GetLogger("ContactController");

        private readonly AccountBiz _biz = new AccountBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly OrderBiz _orderBiz = new OrderBiz();
        //private readonly MemberBiz _memberBiz = new MemberBiz();

        private readonly TaskBiz _taskBiz = new TaskBiz();

        public ActionResult Search(AccountVModel vModel)
        {
            if (vModel == null)
                vModel = new AccountVModel();

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
                var temp = _teamBiz.GetTeams("5", OwnerCode);
                teams = temp.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(vModel.CrmTeamId))
                {
                    vModel.CrmTeamId = temp.FirstOrDefault().TeamID;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                vModel.IsLeader = 1;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(vModel.CrmTeamId))
                {
                    vModel.CrmTeamId = GlobalContext.Current.LoginUserTeams.FirstOrDefault().TeamID;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                vModel.IsLeader = 0;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).ToSelectListFor(t => t.TeamID, v => v.TeamName);
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

            vModel.Accounts = _biz.GetPagedList(vModel);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        public ActionResult Create(string customerCode, string CustomerName)
        {
            var vModel = InitEditVModel();
            vModel.Account = new CrmAccountModel();

            if (!string.IsNullOrEmpty(customerCode))
                vModel.Account.CustomerName = DictionaryTools.GetCachedCustomer(customerCode).Name;

            if (!string.IsNullOrEmpty(CustomerName))
                vModel.Account.CustomerName = CustomerName;

            var team = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).FirstOrDefault();
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

            return View("Edit", vModel);
        }

        public ActionResult Edit(string id)
        {
            var vModel = InitEditVModel();
            if (!string.IsNullOrEmpty(id))
                vModel.Account = _biz.GetById(id);

            // 初始化下拉列表
            if (string.IsNullOrEmpty(vModel.Account.TeamID))
                ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            else
                ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(vModel.Account.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            return View(vModel);
        }

        public ActionResult Details(string code, string state, string id)
        {
            try
            {
                InWeixin(code, state);

                if (GlobalContext.Current.UserInfo == null)
                {
                    return Content("微信绑定信息获取失败.");
                }
                var vModel = InitEditVModel();
                vModel.Account = _biz.GetById(id);
                vModel.Customer = _customerBiz.GetById(vModel.Account.CustomerCode);

                // 初始化下拉列表
                if (string.IsNullOrEmpty(vModel.Account.TeamID))
                    ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                else
                    ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(vModel.Account.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

                return View(vModel);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                throw;
            }
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
                // 客户所属组 审核
                var custoemr = _customerBiz.GetById(model.CustomerCode);
                if (custoemr.TeamID != model.TeamID)
                {
                    // 做任务
                }
                else
                {

                    var leader = _teamBiz.GetTeamLeader(OwnerCode, model.TeamID); //
                    if (!string.IsNullOrEmpty(leader.OpenID))
                    {
                        var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                        string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fsaler%2Fcustomerdetails%2F" + code + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                        var testData = new MessageData()
                        {
                            first = new TemplateDataItem(string.Format("联系人姓名：{0}", model.Name)),
                            keyword1 = new TemplateDataItem(GlobalContext.Current.UserInfo.Name),
                            keyword2 = new TemplateDataItem(DateTime.Now.ToString()),
                            keyword3 = new TemplateDataItem("联系人审核"),
                            remark = new TemplateDataItem("审核客户信息后请及时确认。")
                        };

                        var result = TemplateApi.SendTemplateMessage(accessToken, leader.OpenID, "zx_5OoMRcAEr5YkHoOooKbRwqaueXMpXjlQaQuNHGmc", url, testData);
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
            try
            {
                var teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).Select(t => t.TeamID).ToArray();
                var model = _biz.GetById(code);
                var customer = _customerBiz.GetById(model.CustomerCode);
                if (model.TeamID == customer.TeamID)
                {
                    model.SalerState = state;//设置为审核状态
                }
                else if (state == 1)   // 选择通过
                {
                    // OK
                    model.SalerState = 1;
                }

                int i = _biz.Update(model);

                if (i > 0)
                {
                    // 记录日志
                    LogBiz.WriteConttactLog(UserInfo.OwnerCode, model.Code, "", model.TeamID, GlobalContext.Current.UserInfo.Code, (state == 1 ? "客户审核通过" : "客户审核不通过"));

                    // 发消息销售
                    var sales = _biz.GetAccountCustomer(model.SalerCode);
                    if (!string.IsNullOrEmpty(sales.OpenID))
                    {
                        var first = string.Format("联系人名称：{0}", model.Name);
                        var param1 = (state == 1 ? "联系人审核通过" : "联系人审核不通过");

                        var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                        var testData = new SendMessageData()
                        {
                            first = new TemplateDataItem(first),
                            keyword1 = new TemplateDataItem(param1),
                            keyword2 = new TemplateDataItem(DateTime.Now.ToString()),
                            remark = new TemplateDataItem(remark)
                        };
                        string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fsaler%2Fcontactdetails%2F" + code + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                        SendTemplateMessageResult result = TemplateApi.SendTemplateMessage(accessToken, sales.OpenID, "ILdChJL_b9gREEWboGh7u3WtVsgat9kvfNznLRp79no", url, testData);

                        //if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
                        //    return "1";
                        //else
                        //    return "0";
                    }
                }

                return Json(new { Code = i }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                throw;
            }
        }

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

            return RedirectToAction("ContactIndex");
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <returns></returns>
        //public ActionResult SetValidState(string code)
        //{
        //    var obj = _biz.GetById(code);
        //    if (obj.IsValid == 0 && !string.IsNullOrEmpty(obj.LoginName))
        //    {
        //        if (_biz.GetByLoginName(obj.LoginName) != null)
        //            return AlertResult("该用户名已存在！");
        //    }
        //    obj.IsValid = obj.IsValid == 1 ? 0 : 1;
        //    obj.ModifiedBy = UserInfo.Code;
        //    obj.ModifiedTime = DateTime.Now;
        //    _biz.Update(obj);
        //    return RedirectToAction("Search");
        //}

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
                teams = _teamBiz.GetTeams("5", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                vModel.IsLeader = 1;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                vModel.IsLeader = 0;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).ToSelectListFor(t => t.TeamID, v => v.TeamName);
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

            var teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).ToList();  //当前用户的所有组
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

        public ActionResult GetTeamUserByTeamId(string teamId)
        {
            var sales = _customerBiz.GetTeamUsersByTeamId(teamId, OwnerCode);

            #region 根据用户角色 锁定过滤

            int IsBoss = 0;
            int GroupLeader = 0;

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
                IsBoss = 1;
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))//
            {
                GroupLeader = 1;
            }
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                if (IsBoss == 0 && GroupLeader == 0)
                {
                    sales = _customerBiz.GetTeamSales(OwnerCode).Where(a => a.Code == GlobalContext.Current.UserInfo.Code).ToList();
                }
            }

            #endregion 根据用户角色 锁定过滤

            return Json(sales, JsonRequestBehavior.AllowGet);
        }
    }
}