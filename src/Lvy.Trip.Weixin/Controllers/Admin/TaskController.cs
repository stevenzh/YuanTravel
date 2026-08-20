using Lvy.Models.BaseDB;
using Lvy.Models.JModels;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Weixin.Models;
using Lvy.VModels.Base;
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
    ///
    /// </summary>
    public class TaskController : AdminBaseController
    {
        private TaskBiz _biz = new TaskBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();

        /// <summary>
        /// 查询公告-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Search(TaskVModel vModel)
        {
            if (vModel == null)
                vModel = new TaskVModel();

            // -------------------------------------------------------------------------------------------------
            var SalesTeams = new List<SelectListItem>();
            if (GlobalContext.Current.UserInfo.AccountType == 2) // 管理员
            {
                vModel.IsLeader = 1;
                SalesTeams = _teamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                vModel.IsLeader = 1;
                SalesTeams = _teamBiz.GetTeams("5", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                vModel.IsLeader = 1;
                SalesTeams = _teamBiz.GetTeams("2", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else
            {
                SalesTeams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(vModel.Task.Originator))
                    vModel.Task.TeamID = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;

                if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长")
                    || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "签证总监")
                    || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长"))
                {
                    vModel.IsLeader = 1;
                }
                else
                {
                    vModel.IsLeader = 0;
                    vModel.Task.Originator = GlobalContext.Current.UserInfo.Code;
                }
            }
            ViewBag.AccountTeamBeans = SalesTeams;

            if (string.IsNullOrEmpty(vModel.Task.TeamID) && GlobalContext.Current.UserInfo.AccountType != 2) // 普通人必须选择部门
                vModel.Task.TeamID = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;

            ViewBag.SalesOfTeam = _accountBiz.GetAccountByTeam(GlobalContext.Current.OwnerCode, vModel.Task.TeamID).Where(a => a.Code == GlobalContext.Current.UserInfo.Code).ToSelectListFor(k => k.Code, v => v.Name);
            // ------------------------------------------------------------------------------------------------------------------

            vModel.Task.OwnerCode = OwnerCode;
            vModel.TaskPageList = _biz.GetPageList(vModel);
            ViewBag.WorkFlowTypes = DictionaryTools.GetEnumsBy(Enums.WorkFlowEnum).ToSelectListFor();

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        public ActionResult Create()
        {
            ViewBag.WorkFlowTypes = DictionaryTools.GetEnumsBy(Enums.WorkFlowEnum).ToSelectListFor();
            ViewBag.TeamBeans = _teamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            ViewBag.SalesOfTeam = _accountBiz.GetAccountByTeam(GlobalContext.Current.OwnerCode, "").ToSelectListFor(k => k.Code, v => v.Name);

            return View();
        }

        /// <summary>
        /// 添加任务  重复联系人
        /// </summary>
        /// <param name="contactCode"></param>
        /// <returns></returns>
        public ActionResult CreateContactTask(string contactCode, int workflowId)
        {
            TaskVModel model = new TaskVModel();
            var contact = _accountBiz.GetAccountCustomer(contactCode);
            model.JsonModel.ContactCode = contactCode;  // 需要的联系人
            model.JsonModel.ContactName = contact.Name;
            model.JsonModel.CustomerCode = contact.CustomerCode;
            model.JsonModel.CustomerName = contact.Customer.Name;

            model.Task.WorkFlowID = workflowId;
            var myteam = GlobalContext.Current.LoginUserTeams.Where(m => m.DepartCode == 5).FirstOrDefault();
            var leader = _teamBiz.GetTeamLeader(GlobalContext.Current.OwnerCode, myteam.TeamID);
            model.Task.WorkmanTeam = myteam.TeamID;
            model.Task.TeamID = myteam.TeamID;
            if (leader != null)
                model.Task.Workman = leader.Code;

            if (workflowId == 6001)  // 同部门
            {
            }
            else if (workflowId == 6002 || workflowId == 6004)  // 跨部门申请联系人
            {
                model.JsonModel.WorkmanTeam = contact.TeamID;    // 联系人所在销售组
                var leader1 = _teamBiz.GetTeamLeader(GlobalContext.Current.OwnerCode, contact.TeamID);
                if (leader1 != null)
                    model.JsonModel.Workman = leader1.Code;
            }
            model.Note = "认领客户" + contact.Customer.Name + "的联系人" + contact.Name;

            return View(model);
        }

        [ValidateInput(false)]
        public ActionResult Add(TaskVModel model)
        {
            model.Task.Status = 0;
            model.Task.CreatedTime = DateTime.Now;
            model.Task.Originator = GlobalContext.Current.UserInfo.Code;
            model.OwnerCode = OwnerCode;
            model.Task.JsonData = model.JsonModel.ToJsonSerialize();
            model.Task.OwnerCode = GlobalContext.Current.OwnerCode;
            var newid = _biz.AddTask(model.Task);

            // 添加子任务
            if (model.Task.WorkFlowID == 6001)   // 同部门申请联系人
            {
            }
            else if (model.Task.WorkFlowID == 6002 || model.Task.WorkFlowID == 6004)   // 跨部门申请联系人
            {
                BaseTaskModel sub = new BaseTaskModel();
                sub.ParentID = newid;
                sub.CreatedTime = model.Task.CreatedTime;
                sub.Status = 0;
                sub.TeamID = model.Task.TeamID;
                sub.Originator = model.Task.Originator;
                sub.OwnerCode = model.Task.OwnerCode;
                sub.WorkFlowID = model.Task.WorkFlowID;
                sub.WorkmanTeam = model.JsonModel.WorkmanTeam;
                sub.Workman = model.JsonModel.Workman;
                sub.Contents = model.Task.Contents;
                sub.JsonData = model.Task.JsonData;
                _biz.AddTask(sub);
            }

            return Json(new { Code = "1" });
        }

        public ActionResult Details(int id, string code, string state)
        {
            InWeixin(code, state);

            var model = _biz.GetByTaskId(id);

            model.SubTasks = _biz.GetSubTasks(id);

            return View(model);
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetStatus(long id, int status)
        {
            var ret = new { Code = "0", Message = "" };
            var model = _biz.GetByTaskId(id);
            var list = _biz.GetSubTasks(id);

            bool subStatus = true;
            foreach (var item in list)
            {
                if (item.Status != status)
                {
                    subStatus = false;
                    break;
                }
            }
            if (subStatus == false)
            {
                ret = new { Code = "1", Message = "子任务没完成。" };
                return Json(ret);
            }

            if (status == 10)  // 通过
            {
                if (model.ParentID == null)
                {
                    if (model.WorkFlowID == 6001 || model.WorkFlowID == 6004)  // 转移联系人
                    {
                        var v = model.JsonData.ToJsonDeserialize<TaskJModel>();
                        _accountBiz.MoveContact(v.CustomerCode, v.ContactCode, model.TeamID, model.Originator, 1);
                    }
                    else if (model.WorkFlowID == 6002)   // 组外添加联系人
                    {
                        var v = model.JsonData.ToJsonDeserialize<TaskJModel>();
                        _accountBiz.UpdateContactState(v.ContactCode, 1);
                    }
                    else if (model.WorkFlowID == 6003)   // 转移客户
                    {
                        var v = model.JsonData.ToJsonDeserialize<TaskJModel>();
                        _customerBiz.MoveCustomer(v.CustomerCode, model.TeamID, model.Originator);
                    }
                }

                model.Status = status;
                model.OperateTime = DateTime.Now;
                _biz.UpdateTask(model);

                // 发送微信 通知销售
                var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                var saler = _teamBiz.GetTeamLeader(OwnerCode, model.TeamID); //
                if (!string.IsNullOrEmpty(saler.OpenID))
                {
                    var v = model.JsonData.ToJsonDeserialize<TaskJModel>();
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem("您好，提交了" + DictionaryTools.GetEnumValue(Enums.WorkFlowEnum, model.WorkFlowID.ToString(), false)),
                        keyword1 = new TemplateDataItem("联系人审核通过"),
                        keyword2 = new TemplateDataItem(v.ContactName),
                        remark = new TemplateDataItem("")
                    };
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Ftask%2FDetails%2F" + model.TaskID + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    TemplateApi.SendTemplateMessage(accessToken, saler.OpenID, "ILdChJL_b9gREEWboGh7u3WtVsgat9kvfNznLRp79no", url, testData);
                }
            }
            else if (status == 20)
            {
                // 发送微信 通知销售
                var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                var saler = _teamBiz.GetTeamLeader(OwnerCode, model.TeamID); //
                if (!string.IsNullOrEmpty(saler.OpenID))
                {
                    var v = model.JsonData.ToJsonDeserialize<TaskJModel>();
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem("您好，提交了" + DictionaryTools.GetEnumValue(Enums.WorkFlowEnum, model.WorkFlowID.ToString(), false)),
                        keyword1 = new TemplateDataItem("联系人审核不通过"),
                        keyword2 = new TemplateDataItem(v.ContactName),
                        remark = new TemplateDataItem("")
                    };
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Ftask%2FDetails%2F" + model.TaskID + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    TemplateApi.SendTemplateMessage(accessToken, saler.OpenID, "ILdChJL_b9gREEWboGh7u3WtVsgat9kvfNznLRp79no", url, testData);
                }
            }

            return Json(ret);
        }
    }
}