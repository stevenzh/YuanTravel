using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Ticket;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 商户功能-预警页面
    /// </summary>
    public class MessageController : BaseController
    {
        /// <summary>
        /// 新订单
        /// </summary>
        /// <returns></returns>
        public ActionResult NewOrder()
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("Index", "Online");
            }
            PromptMessage msg = null;
            if (null != GlobalContext.Current.UserInfo)
            {
                var orderCount = new OrderBiz().GetNewOrderCount(UserInfo);
                var tktOrderCount = new TktOrderBiz().GetUnHandledOrderCount(UserInfo);

                msg = new PromptMessage
                {
                    State = "200",
                    OrderCount = orderCount.ToString(CultureInfo.InvariantCulture),
                    TktOrderCount = tktOrderCount.ToString(CultureInfo.InvariantCulture),
                    NoAuditCustomerCount =
                                     new AccountBiz().GetNoAuditCustomerCnt(GlobalContext.Current.OwnerCode).ToString(CultureInfo.InvariantCulture)
                };
            }
            else
            {
                msg = new PromptMessage
                {
                    State = "400"
                };
            }
            return Json(msg);
        }

        /// <summary>
        /// 查看订单
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckNewOrder()
        {
            var customer = new CustomerBiz().GetById(GlobalContext.Current.UserInfo.CustomerCode);
            if (customer.IsDistributors)
            {
                return RedirectToAction("OrderStatistic", "Saler");
            }
            return RedirectToAction("SearchOrder", "Order", new { OrderState = 1 });
        }

        /// <summary>
        /// 查看门票订单
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckNewTktOrder()
        {
            var customer = new CustomerBiz().GetById(GlobalContext.Current.UserInfo.CustomerCode);
            var dic = new RouteValueDictionary { { "Order.OrderState", 1 } };
            if (customer.IsDistributors)
            {
                return RedirectToAction("TktOrderStatistic", "Saler");
            }
            return RedirectToAction("Search", "TktOrder", dic);
        }

        /// <summary>
        ///  查看未审核客户
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckNoAduitCustomers()
        {
            return RedirectToAction("SearchNoAuditAccount", "Account", null);
        }
    }
}