using Lvy.Models.TourDB;
using Lvy.Trip.Biz.Ticket;
using Lvy.Trip.WebSite.Mvc.Attributes;
using Lvy.VModels;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 门票预订
    /// </summary>
    public class TktBookingController : BaseController
    {
        private readonly TktOrderBiz _biz = new TktOrderBiz();
        private readonly TktProductBiz _productBiz = new TktProductBiz();

        /// <summary>
        /// 预定
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Booking(BookingVModel vModel)
        {
            if (vModel == null)
                vModel = new BookingVModel();
            if (vModel.Order == null)
                vModel.Order = new TpTourBalanceModel();

            vModel.Product = _biz.GetProductById(vModel.ProductId);

            return View(vModel);
        }

        /// <summary>
        /// 检查限额
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="peopleCount"></param>
        /// <returns></returns>
        public JsonResult CheckQuota(string productId, int peopleCount)
        {
            var quota = _productBiz.GetById(productId);
            if (quota.PlanQuota - quota.HoldQuota - quota.UsedQuota < peopleCount)
            {
                return Json(new { State = 0, Msg = "可定数量不足,请修改人数。" });
            }
            else
            {
                if (quota.LimitQuota > 0)
                {
                    var orderDetails = _biz.GetDetails(productId, GlobalContext.Current.UserInfo.CustomerCode).Where(p => p.IsValid == 1);
                    var orderedNum = orderDetails.Sum(detail => detail.PeopleNum);
                    if (quota.LimitQuota < orderedNum + peopleCount)
                        return Json(new { State = 0, Msg = "预定数量超出限额,请修改人数。" });
                }
            }
            return Json(new { State = 1, Msg = "" }); ;
        }

        /// <summary>
        /// 编辑订单
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult EditOrder(string orderCode)
        {
            BookingVModel vModel = _biz.GetEditOrderModel(orderCode);
            return View("EditOrder", vModel);
        }

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AddOrder(BookingVModel vModel)
        {
            vModel.Order.AgentCode = GlobalContext.Current.UserInfo.CustomerCode;
            CommonJsonResult result = _biz.AddOrder(vModel, UserInfo);
            if (result.State == "0")
                return Json(new { State = 0, Msg = result.Message });
            return Json(new { State = 1, OrderCode = result.Code });
        }

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult UpdateOrder(BookingVModel vModel)
        {
            vModel.Order.ModifiedBy = UserInfo.Code;
            string[] result = _biz.UpdateOrder(vModel, UserInfo);
            string returnUrl = "/Seller/TktOrderStatistic";

            if (result[0] == "0")
                return Json(new { State = 0, Msg = result[1] });
            return Json(new { State = 1, OrderCode = result[1], ReturnUrl = returnUrl });
            //return Json(new { OrderCode = orderCode, ReturnUrl = returnUrl });
        }

        /// <summary>
        /// 打开查询层
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenSearchDialog(string destId)
        {
            ViewBag.SameProducts = _biz.GetSameProductsByDest(destId, OwnerCode);
            ViewBag.SameShoppings = _biz.GetSameShoppingsByDest(destId, OwnerCode);

            return PartialView("UCSearchDialog");
        }

        /// <summary>
        /// 添加单项产品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ActionResult AddProductToShopCar(string productId)
        {
            var model = _biz.GetProductById(productId);
            return PartialView("UCProduct", model);
        }

        /// <summary>
        /// 获取当天对应的价格列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCurrentPrices(string selectDate, string productId)
        {
            var vModel = new TktPriceListVModel
            {
                Product = _productBiz.GetById(productId)
            };
            if (vModel.Product.PriceMode == 1)
                vModel.PriceList = _biz.GetCurrentPrices(productId);
            else
                vModel.PriceList = _biz.GetCurrentPrices(selectDate, productId);

            if (vModel.PriceList.Count > 0)
                return PartialView("UCPriceList", vModel);
            else
                return Content("<div style=\"color: red;\" align=\"center\">没有该天的报价！</div>");
        }

        /// <summary>
        /// 确认新订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult TktConfirm(string orderId)
        {
            var model = _biz.GetOrder(orderId);
            model.OrderState = 2;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            var flag = _biz.UpdateOrderInfo(model);
            return Content(flag.ToString());
        }

        /// <summary>
        /// 假删除订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult TktDelete(string orderId)
        {
            //var model = _biz.GetById(orderId);
            //model.IsCancel = 1;
            //model.TolYsPrice = 0;
            //model.ModifiedBy = UserInfo.Code;
            //model.ModifiedTime = DateTime.Now;
            var flag = _biz.DeleteOrderInfo(orderId, UserInfo);
            return Content(flag.ToString());
        }
    }
}