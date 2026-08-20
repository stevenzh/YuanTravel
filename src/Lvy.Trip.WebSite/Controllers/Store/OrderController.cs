using Arch.Common;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Booking;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.WebSite.Mvc.Attributes;
using Lvy.VModels.Booking;
using Lvy.VModels.Order;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Lvy.Web.Common.Mvc.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 线路订单管理
    /// 包含 预订、查询、编辑订单
    /// </summary>
    [LvyAuth]
    public partial class OrderController : BaseController
    {
        private ILog logger = LogManager.GetLogger("OrderController");

        private readonly OrderBiz _biz = new OrderBiz();
        private readonly BookingBiz _bookingBiz = new BookingBiz();
        private readonly TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TpLineBiz _lineBiz = new TpLineBiz();

        // <summary>
        /// 有巴士信息的预定页 （短途）
        /// </summary>
        /// <returns></returns>
        public ActionResult Booking(string lineId, int tourId = 0)
        {
            BookingVModel vModel = new BookingVModel();
            if (tourId == 0)
            {
                // 找到最近有余位的团
                vModel.Tour = _bookingBiz.GetFirstPlan(lineId);
                if (vModel.Tour != null)
                    tourId = vModel.Tour.Id;
            }
            else
            {
                vModel.Tour = _bookingBiz.GetTourById(tourId);
            }
            vModel.LineModel = _lineBiz.GetLineById(lineId);
            if (vModel.Tour == null)
            {
                return new NotFoundResult();
            }

            vModel.Quota = _quotaBiz.GetQuotaByTour(tourId);
            //vModel.OutDateBeans = _bookingBiz.GetOutDateBeansByLineId(vModel.Tour.LineId);
            vModel.PriceModels = _bookingBiz.GetPricesByTourId(tourId);
            vModel.BusPoints = _bookingBiz.GetBusPoints(lineId);
            vModel.OrderSourceBean = DictionaryTools.GetEnumsBy(Enums.TourSourceEnum);
            //ViewBag.PassTypes = DictionaryTools.GetEnumsBy(Enums.PassTypeEnum).ToSelectListFor();

            vModel.BookingCustomer = GlobalContext.Current.CustomerBy.Code;
            vModel.BookingCustomerName = GlobalContext.Current.CustomerBy.Name;
            //vModel.SalerCode = GlobalContext.Current.OwnerInfo.SalerCode;

            // 取得当前用户
            var sales = _customerBiz.GetSalesByCustomerCode(GlobalContext.Current.CustomerBy.Code);
            ViewBag.Salers = sales.ToSelectListFor(k => k.Code, v => v.Name);

            //  具体加载哪个视图
            switch (vModel.LineModel.LineScope)
            {
                case 1:
                case 2:
                    return View("BusBooking", vModel);
                default:
                    return View("LBooking", vModel);
            }
        }

        /// <summary>
        /// 预定报名
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveOrder(BookingVModel vModel)
        {
            string orderCode = "";
            vModel.OrderState = 1;
            vModel.TraceState = 10;
            vModel.BookingCustomer = GlobalContext.Current.UserInfo.CustomerCode;
            vModel.SettlePlatForm = 1;
            vModel.SettleCustomer = GlobalContext.Current.UserInfo.CustomerCode;
            OrderResultState code = _bookingBiz.BookingTrans(ref orderCode, vModel, UserInfo);
            return Json(new { StateCode = ((int)code).ToString(), OrderCode = orderCode });
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult CancelOrder(string orderCode)
        {
            TpOrderModel model = new TpOrderModel();
            model.OrderCode = orderCode;
            model.TolYsPrice = 0;
            model.IsCancel = 1;
            model.Remark = "";
            int cnt = _biz.CancelOrder(model, UserInfo);

            //TODO  订单取消不明 ，要通知OP或销售

            return Content(cnt.ToString());
        }

        /// <summary>
        /// 修改订单-视图
        /// </summary>
        /// <param name="orderCode">订单编号</param>
        /// <returns></returns>
        public ActionResult EditTimerOrder(string orderCode, int dayFlag = 1)
        {
            TempData["PageSource"] = "SearchTimerOrder";
            ViewData.Add("dayFlag", dayFlag);
            var vModel = new OrderEditVModel();
            vModel.Order = _biz.GetOrderLineTourist(orderCode);
            vModel.LineModel = vModel.Order.Line;
            //上车点数据加载
            vModel.LineBusPoints = InitBusPoints(vModel.Order);
            InitPage();
            return View("Edit/EditTimerOrder", vModel);
        }

        /// <summary>
        /// 修改订单-视图
        /// </summary>
        /// <param name="orderCode">订单编号</param>
        /// <returns></returns>
        public ActionResult EditOrder(string orderCode)
        {
            TempData["PageSource"] = "OrderStatistic";
            ViewData.Add("dayFlag", 1);
            var vModel = new OrderEditVModel();
            vModel.Order = _biz.GetOrderLineTourist(orderCode);
            vModel.LineModel = vModel.Order.Line;
            //上车点数据加载
            vModel.LineBusPoints = InitBusPoints(vModel.Order);
            InitPage();
            return View("Edit/EditTimerOrder", vModel);
        }

        public ActionResult SaveOrder1(OrderEditVModel vModel)
        {
            OrderResultState state = _biz.SaveOrder(vModel, UserInfo);
            return Json(new { StateCode = ((int)state).ToString(), OrderCode = vModel.Order.OrderCode });
        }

        /// <summary>
        /// 查看线路订单详情
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult ReviewOrder(string orderCode)
        {
            var order = _biz.GetOrderLineTourist(orderCode);
            if (order == null)
                return View("Review/ReviewOrder", new ReviewOrderVModel { Order = new TpOrderModel(), Line = new TpLineModel() });
            var vModel = new ReviewOrderVModel()
            {
                Order = order,
                Line = order.Line,
                LineBusPoints = _biz.GetLineBusPointByLineId(order.LineId),
                Travellers = order.TravellerModels
            };
            return View("Review/ReviewOrder", vModel);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DownLoadFile(int id)
        {
            TpOrderFileModel model = _biz.GetOrderFileModel(id);
            if (model == null)
                return null;
            // TODO  权限检查
            // 文件检查
            try
            {
                WebRequest.Create(AppSetting.Get("UploadFileRoot") + model.FilePath);
            }
            catch (Exception ex)
            {
                logger.Error("File not Found.", ex);
                return null;
            }
            if (model.SourceType == "3")
            {
                // 记录下载日志
                LogBiz.WriteOrderLog(OwnerCode, model.OrderCode, "", GlobalContext.Current.UserInfo.Code, "账单下载，修订号：" + model.Revision, 0);
            }
            else if (model.SourceType == "5")
            {
                // 记录下载日志
                LogBiz.WriteOrderLog(OwnerCode, model.OrderCode, "", GlobalContext.Current.UserInfo.Code, "成团通知下载，修订号：" + model.Revision, 0);
            }

            // 获取文件
            byte[] fileData;
            try
            {
                using (WebClient client = new WebClient())
                {
                    fileData = client.DownloadData(AppSetting.Get("UploadFileRoot") + model.FilePath);

                    return File(fileData, "application/octet-stream", Server.UrlEncode(model.FileName));
                }
            }
            catch (Exception ex)
            {
                logger.Error("File download failure..", ex);
                return null;
            }
        }

        private List<TpLineBusPointModel> InitBusPoints(TpOrderModel order)
        {
            var buspoints = _biz.GetLineBusPointByLineId(order.LineId);
            var currentBs = order.LineBusPoint.ToJsonDeserialize<TpLineBusPointModel>();
            if (buspoints.Where(a => a.Id == currentBs.Id).Count() <= 0)
                buspoints.Add(currentBs);
            return buspoints;
        }

        /// <summary>
        /// 加载游客信息
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult LoadTimerTravellers(string orderCode, int tourId, int trafficType, int dayFlag = 1)
        {
            var vModel = new OrderEditVModel();
            vModel.Order = _biz.GetOrderLineTourist(orderCode);
            vModel.Prices = _biz.GetPricesByTourId(tourId);
            //获取游客信息
            vModel.Travellers = vModel.Order.TravellerModels;
            vModel.Travellers2 = vModel.Travellers.Where(a => a.State == 2).ToList();
            vModel.Travellers10 = vModel.Travellers.Where(a => a.State != 2).ToList();

            ViewBag.PassTypes = DictionaryTools.GetEnumsBy(Enums.PassTypeEnum).ToSelectListFor();

            //有权限修改金额
            if (vModel.Order.OrderState == 1)
            {
                if (trafficType == 1)
                    return PartialView("Edit/UCBusTravellers", vModel);
                else
                    return PartialView("Edit/UCNoBusTravellers", vModel);
            }
            else
            {
                if (trafficType == 1)
                    return PartialView("Edit/UCBusNoPrice", vModel);
                else
                    return PartialView("Edit/UCNoBusNoPrice", vModel);
            }
        }

        /// <summary>
        /// 加载座位表
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadSeats(int tourId)
        {
            var busSeat = _bookingBiz.GetSeatDetails(tourId);

            return PartialView("UCSeats", busSeat);
        }

        /// <summary>
        ///  检查关联订单号是否存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckJoinOrderCode(string joinOrderCode)
        {
            bool flag = _biz.CheckJoinOrderCode(joinOrderCode);

            return Content(flag ? "1" : "0");
        }

        public ActionResult GetParentCompany(string custCode)
        {
            var model = _customerBiz.GetParentCrmCustomerModel(custCode, OwnerCode);
            return Json(model);
        }

        #region 出团通知 废弃

        /// <summary>
        /// 保存附加信息
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult SaveAdditionInfo(TpOrderModel order)
        {
            var orderModel = _biz.GetOrderLineTourist(order.OrderCode);
            orderModel.AdditionInfo = order.AdditionInfo;
            orderModel.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            orderModel.ModifiedTime = DateTime.Now;
            _biz.Update(orderModel);
            return Json(new { State = "success", Msg = orderModel.AdditionInfo });
        }

        /// <summary>
        /// 打印出团通知-视图
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult PrintTourNotice(string orderCode)
        {
            var order = _biz.GetOrderLineTourist(orderCode);
            if (order != null)
            {
                var line = _lineBiz.GetLineById(order.LineId);
                if (line.TrafficType == 1)
                {
                    return View("print/PrintTourNotice", GetTourNotice(orderCode));
                }
                else
                {
                    //交通类型非汽车
                    var vModel = GetTourNotice(orderCode);
                    if (vModel.OrderModel.AdditionInfo.IsNullOrEmpty())
                        vModel.OrderModel.AdditionInfo = vModel.TourPlanModel.AdditionInfo;
                    return View("print/PrintLongTripNotification", vModel);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取出团通知VModel
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        private GoNoticePrintVModel GetTourNotice(string orderCode)
        {
            TpTourPlanBiz tourPlanBiz = new TpTourPlanBiz();
            TpLineRouteBiz lineRouteBiz = new TpLineRouteBiz();
            var vmodel = new GoNoticePrintVModel();

            //根据 【订单编号】 订单信息
            vmodel.OrderModel = _biz.GetOrderLineTourist(orderCode);
            var tourId = vmodel.OrderModel.TourId;
            //根据 【团号】 获取出团计划信息
            vmodel.TourPlanModel = tourPlanBiz.GetTourById(tourId);
            var lineId = vmodel.TourPlanModel.LineId;

            //根据 OrderCode 获取巴士报价种类
            vmodel.BusTravellerVModels = _biz.GetBusTrallsersByOrderCode(orderCode);

            //根据 【团号】 获取出行人信息
            //vmodel.TravellerModels = _travellerBiz.GetByTourId(tourId);

            //根据订单获取出行人数信息
            vmodel.TravellerModels = vmodel.OrderModel.TravellerModels;
            //组装出行人数
            //vmodel.TrallerCount = vmodel.TravellerModels.Count.ToString();

            var strSeatNums = "";
            //组装座位编号
            foreach (var traveller in vmodel.TravellerModels)
            {
                strSeatNums += traveller.SeatNum + "，";
            }
            vmodel.SeatNums = strSeatNums.Substring(0, strSeatNums.Length - 1);

            #region 获取上车点信息

            /*
             * 20130327 将上车点信息序列化到订单表的【LineBusPoint】字段（目的是解决线路上车点删除之后的Bug）
             * 为了与之前的订单不冲突，特做如下处理
            */
            if (vmodel.OrderModel.LineBusPoint.IsNullOrEmpty())
            {
                //之前的订单，需要通过LineBusPointId去关联
                vmodel.LineBusPointModel = vmodel.OrderModel.LineBusPointId != 0 ? (_biz.GetLineBusPointModelById(vmodel.OrderModel.LineBusPointId) ?? new TpLineBusPointModel()) : new TpLineBusPointModel();
            }
            else
            {
                //将上车点序列化到订单之后的处理方式
                var serialize = new JavaScriptSerializer();
                vmodel.LineBusPointModel = serialize.Deserialize<TpLineBusPointModel>(vmodel.OrderModel.LineBusPoint) ??
                                           new TpLineBusPointModel();
            }

            ////根据 【LineBusPointId==>TpLineBusPoint】获取上车信息

            //if (vmodel.OrderModel.LineBusPointId == 0)//无上车点
            //    vmodel.LineBusPointModel = new TpLineBusPointModel();
            //else//有上车点
            //    vmodel.LineBusPointModel = _biz.GetLineBusPointModelById(vmodel.OrderModel.LineBusPointId);

            #endregion 获取上车点信息

            //根据 【线路编号】 获取线路信息
            vmodel.LineModel = vmodel.OrderModel.Line;
            //根据 【线路编号】获取行程列表
            vmodel.LineRoutes = lineRouteBiz.GetRouteListByLineId(lineId);
            //根据 【CustomerCode==>[CrmCustomer]】获取商户信息
            vmodel.CustomerModel = _customerBiz.GetById(vmodel.TourPlanModel.OwnerCode);

            return vmodel;
        }

        #endregion 出团通知 废弃
    }
}