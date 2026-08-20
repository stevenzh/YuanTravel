using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 后台主界面
    /// </summary>
    public class AdminController : BaseController
    {
        private FinanceBiz financeBiz = new FinanceBiz();
        private ArticleBiz articleBiz = new ArticleBiz();
        private LogBiz _logBiz = new LogBiz();
        private TpTourPlanBiz _planBiz = new TpTourPlanBiz();
        private OrderBiz _orderBiz = new OrderBiz();
        private TaskBiz _taskBiz = new TaskBiz();

        // GET: Admin
        public ActionResult Index()
        {
            int IsSaler = 0;
            int IsSaleLeader = 0;
            int IsOp = 0;
            int IsCaiWu = 0;
            var auditCustomer = financeBiz.GetAuditCustomer(GlobalContext.Current.OwnerCode);

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                IsSaler = 1;
            }

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                IsSaleLeader = 1;
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                IsSaleLeader = 1;
                // 所有部门未审核客户
                auditCustomer = auditCustomer.Where(t => GlobalContext.Current.LoginUserTeams.Select(m => m.TeamID).Contains(t.TeamID)).ToList();
            }
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                IsOp = 1;
            }
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "财务总监"))
            {
                IsCaiWu = 1;
            }
            ViewBag.IsSaler = IsSaler;
            ViewBag.IsSaleLeader = IsSaleLeader;
            ViewBag.IsOp = IsOp;
            ViewBag.IsCaiWu = IsCaiWu;

            StatItemVModel model = financeBiz.GetWaiDaiBanInfo(UserInfo, GlobalContext.Current.LoginUserRoles, GlobalContext.Current.LoginUserTeams);
            model.ToAuditCustomer = auditCustomer;
            model.LineSalesStat = financeBiz.GetOrderStat(GlobalContext.Current.OwnerCode);
            model.PlanStoreStat = financeBiz.GetPlanStat(GlobalContext.Current.OwnerCode);

            var userinfo = GlobalContext.Current.UserInfo;
            ViewData["MsgList"] = _logBiz.GetLogByUserId(userinfo.Code);
            ViewData["TaskList"] = _taskBiz.GetTaskList(5, userinfo.Code);
            ViewData["NoticeList"] = articleBiz.GetArticleList(GlobalContext.Current.OwnerCode, 5, 1);

            return View(model);
        }

        /// <summary>
        /// 桌面
        /// </summary>
        /// <returns></returns>

        public ActionResult Desktop()
        {
            return View();
        }

        #region ajax 加载

        /// <summary>
        /// 当前用户近期开班收客情况
        /// </summary>
        /// <returns></returns>
        public ActionResult RecentPlan()
        {
            return View(_planBiz.RecentPlan(UserInfo.Code));
        }

        /// <summary>
        /// 销售员最近的订单
        /// </summary>
        /// <returns></returns>
        public ActionResult RecentOrder()
        {
            return View(_orderBiz.RecentOrder(UserInfo.Code));
        }

        #endregion ajax 加载
    }
}