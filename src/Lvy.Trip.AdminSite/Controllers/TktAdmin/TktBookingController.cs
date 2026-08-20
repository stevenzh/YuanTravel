using Common.Logging;
using Lvy.Models;
using Lvy.Models.TicketDB;
using Lvy.Models.TourDB;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Ticket;
using Lvy.VModels;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    /// <summary>
    /// 预订签证产品
    /// </summary>
    public class TktBookingController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(BookingController));
        private readonly TktProductBiz _biz = new TktProductBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly TktOrderBiz _orderBiz = new TktOrderBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();

        public ActionResult Search(SearchTicketVModel vModel)
        {
            try
            {
                vModel.ProductState = 3;   // 只能预订上线产品
                //产品部门
                ViewData["TeamList"] = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName, "", "", "-选择部门-");

                vModel.PagedTickets = _biz.GetPagedTicket(vModel, UserInfo.OwnerCode);
                if (Request.IsAjaxRequest())
                    return PartialView("Ticket/UCTicketList", vModel);
                return View(vModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult Booking(string id)
        {
            ViewData["Teams"] = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, t => t.TeamName, "", "", "--请选择部门--");
            ViewData["Salers"] = _customerBiz.GetTeamSales(UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            BookingVModel vModel = new BookingVModel();
            vModel.Product = _orderBiz.GetProductById(id);
            vModel.OrderedProducts.Add(vModel.Product);
            return View(vModel);
        }

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AddOrder(BookingVModel vModel)
        {
            vModel.Order.OrderSource = 2;
            CommonJsonResult result = _orderBiz.AddOrder(vModel, UserInfo);

            if (result.State == "0")
                return Json(new { State = 0, Msg = result.Message });
            return Json(new { State = 1, OrderCode = result.Code });
        }

        public ActionResult GetCurrentPrices(string selectDate, string productId)
        {
            var vModel = new TktPriceListVModel
            {
                Product = _biz.GetById(productId)
            };
            if (vModel.Product.PriceMode == 1)
                vModel.PriceList = _orderBiz.GetCurrentPrices(productId);
            else
                vModel.PriceList = _orderBiz.GetCurrentPrices(selectDate, productId);

            if (vModel.PriceList.Count > 0)
                return PartialView("UCPriceList", vModel);
            else
                return Content("<div style=\"color: red;\" align=\"center\">没有该天的报价！</div>");
        }

        public ActionResult BookingOk(string ordercode)
        {
            try
            {
                ViewData["ordercode"] = ordercode;
                return View("~/Views/Visa/Booking/BookingOk.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }


        /// <summary>
        /// 打开查询层
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenSearchDialog(string destId)
        {
            ViewBag.SameProducts = _orderBiz.GetSameProductsByDest(destId, OwnerCode);
            ViewBag.SameShoppings = _orderBiz.GetSameShoppingsByDest(destId, OwnerCode);

            return PartialView("UCSearchDialog");
        }

    }
}