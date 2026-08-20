using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 操作日志
    /// </summary>
    public class LogController : BaseController
    {
        private readonly LogBiz _biz = new LogBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();

        public ActionResult Search(LogVModel vModel)
        {
            var SalesTeams = new List<SelectListItem>();
            if (GlobalContext.Current.UserInfo.AccountType == 2) // 管理员
            {
                vModel.IsLeader = 1;
                SalesTeams = _teamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                vModel.IsLeader = 1;
                SalesTeams = _teamBiz.GetSalesTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                vModel.IsLeader = 1;
                SalesTeams = _teamBiz.GetOpTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else
            {
                SalesTeams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(vModel.BizLog.ModifiedBy))
                    vModel.BizLog.TeamID = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;

                if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长")
                    || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "签证总监")
                    || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长"))
                {
                    vModel.IsLeader = 1;
                }
                else
                {
                    vModel.IsLeader = 0;
                    vModel.BizLog.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                }
            }
            ViewBag.AccountTeamBeans = SalesTeams;
           
            if (string.IsNullOrEmpty(vModel.BizLog.TeamID) && GlobalContext.Current.UserInfo.AccountType != 2) // 普通人必须选择部门
                vModel.BizLog.TeamID = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;

            ViewBag.SalesOfTeam = _accountBiz.GetAccountByTeam(GlobalContext.Current.OwnerCode, vModel.BizLog.TeamID).Where(a => a.Code == GlobalContext.Current.UserInfo.Code).ToSelectListFor(k => k.Code, v => v.Name);
            ViewBag.NoticeTypes = DictionaryTools.GetEnumsBy(Enums.NoticeTypeEnum).ToSelectListFor();

            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.LogList = _biz.GetPageList(vModel);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        public ActionResult Details(int id)
        {
            var model = _biz.GetById(id);
            return View(model);
        }
    }
}