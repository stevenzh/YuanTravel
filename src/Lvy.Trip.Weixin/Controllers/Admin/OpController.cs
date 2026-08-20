using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Logging;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.VModels.Product;
using Lvy.Web.Common;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 计调首页
    /// </summary>
    public class OpController : AdminBaseController
    {
        ILog logger = LogManager.GetLogger("OpController");

        private OrderBiz _orderBiz = new OrderBiz();
        private TpTourPlanBiz _planBiz = new TpTourPlanBiz();
        private TpLineTourPlanBiz _lineTourBiz = new TpLineTourPlanBiz();

        // GET: Op
        public ActionResult Index()
        {
            // 计调近期开班售卖情况
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {

            }

            // 急需处理订单


            // 销售请求


            return View();
        }

        public ActionResult PlanList(SearchTourVModel qmodel)
        {
            if (qmodel.Condition.CrmTeamId.IsNullOrEmpty())
                qmodel.Condition.CrmTeamId = GlobalContext.Current.LoginUserTeams.FirstOrDefault().TeamID;
            qmodel.OwnerCode = OwnerCode;
            qmodel.TourList = _planBiz.GetPageTours(qmodel);

            return View(qmodel);
        }

        public ActionResult PageList(SearchTourVModel qmodel)
        {
            qmodel.OwnerCode = OwnerCode;
            qmodel.TourList = _planBiz.GetPageTours(qmodel);

            return View("PageList", qmodel);
        }

        public ActionResult PlanDetails(int id)
        {
            var vModel = _lineTourBiz.GetEditTour(id, OwnerCode);
            vModel.OrderList = _orderBiz.GetOrderByTourId(vModel.Tour.Id);

            return View(vModel);
        }
    }
}