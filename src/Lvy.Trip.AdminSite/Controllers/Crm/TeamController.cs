using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Crm
{
    /// <summary>
    /// 组管理
    /// </summary>
    public class TeamController : BaseController
    {
        private readonly TeamBiz _biz = new TeamBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();

        #region 组维护

        #region 组列表

        /// <summary>
        /// 查询组列表
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchTeam(TeamVModel vModel)
        {
            if (vModel.PagedModel == null)
                vModel.PagedModel = new PagedList<CrmTeamModel>();
            vModel.DepartList = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum);
            vModel.OwnerCode = GlobalContext.Current.OwnerCode;
            vModel.PagedModel = _biz.GetPagedTeam(vModel);
            if (Request.IsAjaxRequest())
                return PartialView("UCTeamList", vModel);
            return View(vModel);
        }

        #endregion 组列表

        #region 新增组

        /// <summary>
        /// 新增组（初始化）
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateTeam()
        {
            var vModel = new TeamVModel();
            vModel.DepartList = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum);
            vModel.Team.IsValid = 1;
            vModel.Team.LockName = 0;
            vModel.FinanceTeams = _biz.GetTeams("9", OwnerCode);

            return View(vModel);
        }

        /// <summary>
        /// 新增组(保存）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateTeam(TeamVModel model)
        {
            model.Team.TeamID = DBTools.GetSeqNo("CrmTeam");
            model.Team.OwnerCode = UserInfo.OwnerCode;
            _biz.AddTeam(model.Team);
            return RedirectToAction("SearchTeam");
        }

        #endregion 新增组

        #region 编辑组

        /// <summary>
        /// 编辑组（初始化）
        /// </summary>
        /// <returns></returns>
        public ActionResult EditTeam(string id)
        {
            CrmTeamModel model = _biz.GetTeam(id);
            if (model == null) return null;

            var vModel = new TeamVModel
            {
                Team = model,
                DepartList = DictionaryTools.GetEnumsBy(Enums.DepartCodeEnum),
                TeamAccounts = _accountBiz.GetAccountByTeam(UserInfo.OwnerCode, id),
                FinanceTeams = _biz.GetTeams("9", OwnerCode)
            };
            return View(vModel);
        }

        /// <summary>
        /// 编辑组（保存 ）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditTeam(TeamVModel model)
        {
            var entity = _biz.GetTeam(model.Team.TeamID);
            entity.TeamName = model.Team.TeamName;
            entity.IsValid = model.Team.IsValid;
            entity.DepartCode = model.Team.DepartCode;
            entity.LeaderCode = model.Team.LeaderCode;
            entity.Remark = model.Team.Remark;
            entity.FinanceCode = model.Team.FinanceCode;
            entity.LockName = model.Team.LockName;
            _biz.UpdateTeam(entity);
            return RedirectToAction("SearchTeam");
        }

        #endregion 编辑组

        #region 删除组

        /// <summary>
        /// 删除组
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteTeam(string id)
        {
            CrmTeamModel model = _biz.GetTeam(id);
            model.IsValid = 0;
            _biz.UpdateTeam(model);
            var vModel = new TeamVModel
            {
                OwnerCode = GlobalContext.Current.OwnerCode,
                PagedModel = new PagedList<CrmTeamModel>()
            };

            vModel.PagedModel = _biz.GetPagedTeam(vModel);
            return PartialView("UCTeamList", vModel);
        }

        #endregion 删除组

        #endregion 组维护

        #region 私有方法

        ///// <summary>
        ///// 初始化【线路类型】下拉列表
        ///// </summary>
        //private void InitLineTypeItems(int lineType)
        //{
        //    List<SelectListItem> list = DictionaryTools.GetEnumsBy(Enums.LineType).ToSelectListFor(a => a.Key.ToString(CultureInfo.InvariantCulture), a => a.Value);
        //    SelectListItem selectedItem = list.FirstOrDefault(p => p.Value == lineType.ToString(CultureInfo.InvariantCulture));
        //    if (selectedItem != null)
        //    {
        //        selectedItem.Selected = true;
        //    }
        //    ViewBag.LineTypeList = list;
        //}

        #endregion 私有方法
    }
}