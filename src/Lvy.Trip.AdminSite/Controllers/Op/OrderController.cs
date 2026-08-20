using Arch.Common;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Booking;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Common;
using Lvy.VModels.Op;
using Lvy.VModels.Order;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Lvy.Trip.AdminSite.Controllers.Op
{
    /// <summary>
    /// 订单功能控制器
    /// </summary>
    public partial class OrderController : BaseController
    {
        #region 变量

        private readonly OrderBiz _biz = new OrderBiz();
        private readonly TravellerBiz _travellerBiz = new TravellerBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TpLineRouteBiz _lineRouteBiz = new TpLineRouteBiz();
        private readonly TpLineTourPlanBiz _tourPlanBiz = new TpLineTourPlanBiz();
        private readonly TpTourPlanBiz _planBiz = new TpTourPlanBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly TpChildOrderBiz _childOrderBiz = new TpChildOrderBiz();
        private readonly TpLineAdminBiz _adminBiz = new TpLineAdminBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly BookingBiz _bookingBiz = new BookingBiz();
        private readonly InvoiceBiz _invoiceBiz = new InvoiceBiz();
        private readonly TpOrderPayInBiz _payinBiz = new TpOrderPayInBiz();
        private readonly TpProductBiz _productBiz = new TpProductBiz();

        private ILog logger = LogManager.GetLogger("OrderController");

        #endregion 变量

        #region 订单列表

        /// <summary>
        /// 查询订单-视图
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchOrder(TpOrderVModel vModel)
        {
            // 取得查询分页条件
            var q = (TpOrderVModel)CacheContext.Current.Get(Consts.PageOrderController + GlobalContext.Current.UserInfo.Code);
            if (q != null && vModel.FirstTime)
                vModel = q;

            InitPage();
            //根据 分销商 模糊查找对应的 CustomerCode
            if (!vModel.CustomerName.IsNullOrEmpty())
                vModel.CustomerName = _customerBiz.GetCustomerCodesSql(vModel.CustomerName, OwnerCode);
            //if (!Request.IsAjaxRequest() && vModel.StartOutDate.IsNullOrEmpty())
            //    vModel.StartOutDate = DateTime.Now.ToDateFormat();

            int IsSaler = 0;
            int IsOp = 0;
            var SalesTeams = new List<SelectListItem>();
            var OpTeams = new List<SelectListItem>();

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                IsSaler = 1;
                SalesTeams = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                IsSaler = 1;
                SalesTeams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(vModel.SaleTeamId))
                {
                    vModel.SaleTeamId = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
                else
                {
                    ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(vModel.SaleTeamId, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                IsSaler = 1;
                SalesTeams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                vModel.SaleTeamId = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                vModel.SalerCode = GlobalContext.Current.UserInfo.Code;
                ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(vModel.SaleTeamId, OwnerCode).Where(a => a.Code == GlobalContext.Current.UserInfo.Code).ToSelectListFor(k => k.Code, v => v.Name);
            }
            else
            {
                // 不是销售
                SalesTeams = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }

            if (GlobalContext.Current.IsSysAdmin || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                IsOp = 1;
                OpTeams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                IsOp = 1;
                OpTeams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(vModel.CrmTeamId) && OpTeams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值 ！不是总监不能为空
                {
                    vModel.CrmTeamId = OpTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "门店管理"))
            {
                IsOp = 1;
                OpTeams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(vModel.CrmTeamId) && OpTeams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值 ！不是总监不能为空
                {
                    vModel.CrmTeamId = OpTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }
            else
            {
                // 不是OP
                OpTeams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }

            ViewBag.IsOp = IsOp;
            ViewBag.IsSaler = IsSaler;

            // 保存查询分页条件
            CacheContext.Current.Add(Consts.PageOrderController + GlobalContext.Current.UserInfo.Code, vModel, Consts.OutputCacheDuration2);

            //根据条件获取对应的订单列表信息
            _biz.GetPageList(vModel, UserInfo);
            vModel.FirstTime = false;

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            ViewBag.AccountTeamBeans = OpTeams;
            ViewBag.SalesOfTeam = SalesTeams;

            if (Request.IsAjaxRequest())
                return PartialView("UCSearchOrder", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 修改订单-视图
        /// </summary>
        /// <param name="orderCode">订单编号</param>
        /// <returns></returns>
        public ActionResult EditOrder(string orderCode, int isCaiWu = 0)
        {
            ViewBag.Title = isCaiWu == 1 ? "财务-编辑订单" : "编辑订单";

            var vModel = new OrderEditVModel();
            // 订单信息
            vModel.Order = _biz.GetOrderLineTourist(orderCode);

            if (GlobalContext.Current.IsSysAdmin
                || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长")
                || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                vModel.IsOP = 1;
            }
            else if (vModel.Order.OrderState == 1)
            {
                // 未确认前 销售可以修改客户和联系人
                ViewBag.CustomerList = _customerBiz.GetCustomerBySales(GlobalContext.Current.UserInfo.Code).ToSelectListFor(t => t.Code, t => t.Name);
            }
            vModel.IsEditPric = GlobalContext.Current.FunctionList.Where(a => a.FuncType == 5 && a.Name == "修改金额").FirstOrDefault() == null ? false : true;

            // 线路信息
            vModel.LineModel = vModel.Order.Line;
            // 联系人列表
            ViewBag.LinkMan = _customerBiz.GetContactListByCustomerCode(vModel.Order.BookingCustomer);
            // 开班价格表
            vModel.Prices = new TpPriceBiz().GetValidPrices(vModel.Order.TourId);
            // 开班信息
            vModel.TourPlan = _bookingBiz.GetTourById(vModel.Order.TourId);
            // 子订单
            vModel.ChildOrderList = _childOrderBiz.GetTpChildOrderList(orderCode);
         
            // 上下车点
            if (vModel.LineModel.TrafficType == 1)
                vModel.LineBusPoints = InitBusPoints(vModel.Order);

            InitPage();
            return View("Edit/EditOrder", vModel);
        }

        /// <summary>
        /// 修改订单-保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SaveOrder(OrderEditVModel vModel)
        {
            OrderResultState state = _biz.SaveOrder(vModel, UserInfo);
            return Json(new { StateCode = ((int)state).ToString(), OrderCode = vModel.Order.OrderCode });
        }

        /// <summary>
        /// 订单 跟单状态变更
        /// </summary>
        /// <param name="orderVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult OrdersSure(string OrderCode, int TraceState)
        {
            TpOrderVModel orderVModel = new TpOrderVModel();
            //检查订单游客信息是否完整。不完整不允许确认订单。
            orderVModel.OrderModel = _biz.GetOrderLineTourist(OrderCode);

            orderVModel.OrderModel.TraceState = TraceState;
            orderVModel.OrderModel.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            orderVModel.OrderModel.ModifiedTime = DateTime.Now;
            _biz.Update(orderVModel.OrderModel);

            // 更新库存
            _biz.FreeQuota(orderVModel.OrderModel.TourId, OrderCode, GlobalContext.Current.UserInfo.Code);

            var stateLabel = DictionaryTools.GetEnumValue(Enums.OrderTraceStateEnum, TraceState.ToString(), false);

            // 变更通知OP
            var op = _adminBiz.GetPrimaryAdmin(orderVModel.OrderModel.LineId);

            // 记录日志
            LogBiz.WriteOrderLog(UserInfo.OwnerCode, OrderCode, "", GlobalContext.Current.UserInfo.Code, "状态变更:" + stateLabel, 0);

            if (op != null && !string.IsNullOrEmpty(op.OpenID))
            {
                var first = string.Format("{0}您好,跟单状态变更。", op.Name);
                var remark = string.Format(@"客户名称：{0}
出团日期：{1}", orderVModel.OrderModel.CustomerName, orderVModel.OrderModel.OutDate.ToDateFormat());

                SendMessagClient.SendTemplateMessage(op.OpenID, "8i7VY_GnnYnvTfmDRmntS079TzfJK2KmXV3LUOeOHM0", first, OrderCode,
                    orderVModel.OrderModel.Line.LineName, "价格", stateLabel, "", remark);
            }

            return SearchOrder(orderVModel);
        }

        /// <summary>
        /// 订单 确认订位操作
        /// </summary>
        /// <param name="orderVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult OrdersMakeSure(string OrderCode)
        {
            TpOrderVModel orderVModel = new TpOrderVModel();
            //检查订单游客信息是否完整。不完整不允许确认订单。
            orderVModel.OrderModel = _biz.GetOrderLineTourist(OrderCode);

            orderVModel.OrderModel.OrderState = 2;
            orderVModel.OrderModel.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            orderVModel.OrderModel.ModifiedTime = DateTime.Now;

            // 该团是否上传出团通知 同步
            var file = _biz.GetTourNoticeFile(orderVModel.OrderModel.TourId);
            if (file != null)
            {
                var model = new TpOrderFileModel
                {
                    OrderCode = orderVModel.OrderModel.OrderCode,
                    FileName = file.FileName,
                    FilePath = file.FilePath,
                    CreatedTime = DateTime.Now,
                    IsDel = 0,
                    CreatedBy = GlobalContext.Current.UserInfo.Code,
                    MediaType = MediaType.document.ToString(),
                    SourceType = "22",
                    Revision = file.Revision
                };

                var fid = _biz.AddOrderFile(model);
                orderVModel.OrderModel.TraceState = 50;
            }

            _biz.Update(orderVModel.OrderModel);

            // 更新库存
            _biz.FreeQuota(orderVModel.OrderModel.TourId, OrderCode, GlobalContext.Current.UserInfo.Code);

            // 记录日志
            LogBiz.WriteOrderLog(UserInfo.OwnerCode, OrderCode, "", GlobalContext.Current.UserInfo.Code, "确认占位.", 0);

            // 通知销售
            var sales = _accountBiz.GetById(orderVModel.OrderModel.SalerCode);
            if (sales != null && !String.IsNullOrEmpty(sales.OpenID))
            {
                var first = string.Format("{0}您好,订单状态变更。", sales.Name);
                var remark = string.Format(@"客户名称：{0}
出团日期：{1}", orderVModel.OrderModel.CustomerName, orderVModel.OrderModel.OutDate.ToDateFormat());

                SendMessagClient.SendTemplateMessage(sales.OpenID, "8i7VY_GnnYnvTfmDRmntS079TzfJK2KmXV3LUOeOHM0", first, OrderCode,
                    orderVModel.OrderModel.Line.LineName, "价格", "已确认占位", "", remark);
            }

            return SearchOrder(orderVModel);
        }

        /// <summary>
        /// 参数传递， 显示跟单状态变更页面
        /// </summary>
        /// <param name="traceState"></param>
        /// <param name="orderCode"></param>
        /// <param name="productname"></param>
        /// <param name="orderprice"></param>
        /// <param name="bookingname"></param>
        /// <param name="TolPaid"></param>
        /// <returns></returns>
        public ActionResult GetUCOrderConfirm(int traceState, string orderCode, string productname, string orderprice, string bookingname, string TolPaid)
        {
            ViewBag.traceState = traceState;
            ViewBag.orderCode = orderCode;
            ViewBag.productname = productname;
            ViewBag.orderprice = orderprice;
            ViewBag.bookingname = bookingname;
            ViewBag.TolPaid = TolPaid;
            TpOrderVModel model = new TpOrderVModel();
            return PartialView("UCOrderConfirm", model);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult CancelOrder(TpOrderVModel orderVModel)
        {
            _biz.CancelOrder(orderVModel.OrderModel, UserInfo);

            // 记录日志
            LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderVModel.OrderModel.OrderCode, "", GlobalContext.Current.UserInfo.Code, "订单取消.", 0);

            // 重新初始页面
            orderVModel.OrderModel = _biz.GetOrderLineTourist(orderVModel.OrderModel.OrderCode);
            return SearchOrder(orderVModel);
        }

        /// <summary>
        /// 恢复订单
        /// </summary>
        /// <param name="orderVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult RestoreOrder(TpOrderVModel orderVModel)
        {
            int result = _biz.RestoreOrder(orderVModel.OrderModel, UserInfo);

            if (result == 1)
            {
                // 记录日志
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderVModel.OrderModel.OrderCode, "", GlobalContext.Current.UserInfo.Code, "订单恢复.", 0);

                // TODO 恢复成功通知销售
                return Json(new { Code = "yes", Message = "" });
            }
            else
            {
                return Json(new { Code = "no", Message = "恢复失败，余位不足" });
            }
        }

        /// <summary>
        /// 保存上车点
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult UpdateLineBusPoint(OrderEditVModel vModel)
        {
            //此处需要用事务做处理

            //1.订单对应的游客表中的接价、送价和应收款
            var orderCode = vModel.Order.OrderCode;
            var orderModel = _biz.GetOrderLineTourist(orderCode);
            if (!string.IsNullOrEmpty(orderModel.LineBusPoint))  // 和原来的比较
            {
                var currentBs = orderModel.LineBusPoint.ToJsonDeserialize<TpLineBusPointModel>();
                if (vModel.LineBusPointView.Split('|')[0].ToInt() == currentBs.Id)
                    return Content("1");
            }

            //decimal orderTolYsPrice = orderModel.TolYsPrice;
            //var busPoint = new TpLineBusPointBiz().GetBusPointById(vModel.LineBusPointView.Split('|')[0].ToInt());

            //var travellerModels = new List<TpTravellerModel>();
            //根据订单编号获取对应的游客对象列表
            //travellerModels = _travellerBiz.GetByOrderCode(orderCode).Where(a => a.State != 0).ToList();

            //if (travellerModels.Count > 0)
            //{
            //    orderTolYsPrice = 0;
            //    foreach (var travellerModel in travellerModels)
            //    {
            //        decimal oldjsprice = travellerModel.JiePrice + travellerModel.SongPrice;
            //        decimal jsprice = 0;
            //        if (busPoint.JsType == 1)
            //        {
            //            jsprice = busPoint.JiePrice;
            //            travellerModel.JiePrice = busPoint.JiePrice;
            //            travellerModel.SongPrice = 0;
            //        }
            //        else if (busPoint.JsType == 2)
            //        {
            //            jsprice = busPoint.SongPrice;
            //            travellerModel.JiePrice = 0;
            //            travellerModel.SongPrice = busPoint.SongPrice;
            //        }
            //        else if (busPoint.JsType == 3)
            //        {
            //            jsprice = busPoint.JiePrice + busPoint.SongPrice;
            //            travellerModel.JiePrice = busPoint.JiePrice;
            //            travellerModel.SongPrice = busPoint.SongPrice;
            //        }

            //        //订单总应收计算
            //        travellerModel.YsPrice = travellerModel.YsPrice - oldjsprice + jsprice;

            //        orderTolYsPrice += travellerModel.YsPrice;
            //        _travellerBiz.Update(travellerModel);
            //    }
            //}

            //2.订单的总应收改变和上车点改变
            //orderModel.TolYsPrice = orderTolYsPrice;
            int bsId = vModel.LineBusPointView.Split('|')[0].ToInt(); ;
            orderModel.LineBusPointId = bsId;
            orderModel.LineBusPoint = new TpLineBusPointBiz().GetBusPointById(bsId).ToJsonSerialize();
            int result = _biz.UpdateLineBusPoint(orderModel);

            return Content(result.ToString());
        }

        /// <summary>
        /// 取消游客
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult CancelTraveller(OrderEditVModel vModel)
        {
            // 记录日志
            LogBiz.WriteOrderLog(UserInfo.OwnerCode, vModel.Order.OrderCode, "", GlobalContext.Current.UserInfo.Code, "取消游客 ID=" + vModel.TravellerId, 0);

            var code = _biz.CancelTraveller(vModel, UserInfo);
            return Content(code.ToString());
        }

        /// <summary>
        /// 恢复游客
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult RestoreTraveller(OrderEditVModel vModel)
        {
            var result = _biz.RestoreTraveller(vModel, UserInfo);
            if (result == 1)
            {
                // 记录日志
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, vModel.Order.OrderCode, "", GlobalContext.Current.UserInfo.Code, "取消游客 ID=" + vModel.TravellerId, 0);

                return Json(new { Code = "yes", Message = "" });
            }
            else
            {
                return Json(new { Code = "no", Message = "恢复失败，余位不足" });
            }
        }

        /// <summary>
        /// 加载游客信息
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult LoadTravellers(string orderCode, int tourId)
        {
            var vModel = new OrderEditVModel();
            vModel.Order = _biz.GetOrderLineTourist(orderCode);
            vModel.LineModel = vModel.Order.Line;
            vModel.Prices = _biz.GetPricesByTourId(tourId);
            vModel.TourPlan = _tourPlanBiz.GetTourById(vModel.Order.TourId);

            // 补充游客数量
            if (vModel.Order.TravellerModels.Count < vModel.Order.TravellerCount)
            {
                for (var ro = vModel.Order.TravellerModels.Count; ro < vModel.Order.TravellerCount; ro++)
                {
                    vModel.Order.TravellerModels.Add(new TpTravellerModel { TourId = tourId, OrderCode = orderCode, State = 2 });
                }
            }
            vModel.Travellers2 = vModel.Order.TravellerModels.Where(a => a.State == 2).ToList(); // 有效
            vModel.Travellers10 = vModel.Order.TravellerModels.Where(a => a.State != 2).ToList(); //已退团

            ViewBag.PricesList = _biz.GetPricesByTourId(tourId);
            ViewBag.PassTypes = DictionaryTools.GetEnumsBy(Enums.PassTypeEnum).ToSelectListFor();
            ViewBag.Sex = DictionaryTools.GetEnumsBy(Enums.SexEnum).ToSelectListFor();

            // 用户功能中包含“修改金额”的
            var IsEditPric = GlobalContext.Current.FunctionList.Where(a => a.FuncType == 5 && a.Name == "修改金额").FirstOrDefault() == null ? false : true;

            // 审核只是占位 价格还是可以改的
            //if (IsEditPric)
            //    IsEditPric = new TpTourPlanBiz().GetTourById(tourId).AuditState < 3; // 未审核 可编辑

            //有权限修改金额
            if (IsEditPric)
            {
                if (vModel.LineModel.LineScope < 3)
                    return PartialView("Edit/UCBusTravellers", vModel);
                else
                    return PartialView("Edit/UCNoBusTravellers", vModel);
            }
            else
            {
                if (vModel.LineModel.LineScope < 3)
                    return PartialView("Edit/UCBusNoPrice", vModel);
                else
                    return PartialView("Edit/UCNoBusNoPrice", vModel);
            }
        }

        #endregion 订单列表

        #region 账单

        /// <summary>
        /// 查看账单-视图
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult OrderConfirmPrint(string orderCode)
        {
            return View("print/OrderConfirmPrint", GetBill(orderCode));
        }

        /// <summary>
        /// 更新账单部分信息
        /// </summary>
        public ActionResult UpdateBillInfo(TpOrderModel model)
        {
            int row = _biz.UpdateBillInfo(model);
            return Json(new { Code = "100", Message = "update row:" + row });
        }

        /// <summary>
        /// 打印账单-视图 (废弃)
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult PrintBill(string orderCode)
        {
            return View("print/PrintBill", GetBill(orderCode));
        }

        /// <summary>
        /// 打印账单
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult PrintBillPDF(string orderCode)
        {
            OrderConfirmPrintVModel qModel = GetBill(orderCode);
            TempData["OrderConfirmPrintVModel"] = qModel;
            return View("/Views/Order/Print/PrintBillPDF.aspx", qModel);
        }

        /// <summary>
        ///获取打印账单VModel
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        private OrderConfirmPrintVModel GetBill(string orderCode)
        {
            TpLineBiz lineBiz = new TpLineBiz();
            var vModel = new OrderConfirmPrintVModel();
            //根据 【订单编号】 订单信息
            vModel.OrderModel = _biz.GetOrderLineTourist(orderCode);
            //根据 【lineId==>TpLine】获取线路信息
            vModel.LineModel = vModel.OrderModel.Line;  //lineBiz.GetLineById(vModel.OrderModel.LineId);
            //根据 【线路编号】获取行程列表
            vModel.LineRoutes = _lineRouteBiz.GetRouteListByLineId(vModel.OrderModel.LineId);
            //根据 【OrderCode==>TpTraveller】 获取游客信息
            vModel.TravellerModels = vModel.OrderModel.TravellerModels; //_travellerBiz.GetByOrderCode(orderCode);
            //根据 OrderCode 获取巴士账单明细
            vModel.BusTravellerVModels = _biz.GetBusTrallsersByOrderCode(orderCode);
            // 开班计划
            vModel.TourPlan = _planBiz.GetTourById(vModel.OrderModel.TourId);
            //分销商信息
            //根据 【BookingCustomer==>[CrmCustomer]】获取商户信息
            vModel.CustomerModel = _customerBiz.GetById(vModel.OrderModel.BookingCustomer);

            //组装 座位编号
            if (vModel.TravellerModels.Count > 0)
            {
                var strSeatNums = "";
                var travellerlist = vModel.TravellerModels; // 根据座位号排序
                foreach (var travellerModel in travellerlist)
                {
                    strSeatNums += travellerModel.SeatNum + "，";
                }
                var strLength = strSeatNums.Length;
                vModel.SeatNums = strSeatNums.Substring(0, strLength - 1);

                vModel.PriceList = _biz.GetPricesByTourId(vModel.OrderModel.TourId);
                // 有效人数
                var gg = (from p in vModel.TravellerModels.Where(t => t.State == 2)
                          group p by p.PriceId into d
                          select new PersonSetModel
                          {
                              PersonType = vModel.PriceList.Where(t => t.Id == d.Key).FirstOrDefault().PriceRemark,
                              Total = d.Sum(t => t.Price - t.TeJiaFanLi + t.FanLi),
                              Discount = d.Sum(t => t.TeJiaFanLi - t.FanLi),
                              Count = d.Count(),
                              Price = vModel.PriceList.Where(t => t.Id == d.Key).FirstOrDefault().SettlePrice
                          }).ToList();
                vModel.PersonModels = gg;

                // 单房差
                var room = vModel.TravellerModels.Where(t => t.SingleRoom > 0).ToList();
                if (room.Count() > 0)
                {
                    vModel.PersonModels.Add(new PersonSetModel
                    {
                        PersonType = "单房差",
                        Total = room.Sum(t => t.SingleRoom),
                        Discount = 0,
                        Count = room.Count(),
                        Price = vModel.TourPlan.SingleRoom,
                        Note = ""
                    });
                }

                // 退团费用
                var loser = vModel.TravellerModels.Where(t => t.State != 2 && t.YsPrice > 0).ToList();
                if (loser.Count() > 0)
                {
                    vModel.PersonModels.Add(new PersonSetModel
                    {
                        PersonType = "退团游客费用",
                        Total = loser.Sum(t => t.YsPrice),
                        Discount = 0,
                        Count = loser.Count(),
                        Price = 0,
                        Note = ""
                    });
                }
            }

            // 添加子订单
            var ChildOrderList = _childOrderBiz.GetTpChildOrderList(orderCode);
            if (ChildOrderList != null)
            {
                foreach (var item in ChildOrderList)
                {
                    vModel.PersonModels.Add(new PersonSetModel
                    {
                        Count = item.Quantity,
                        PersonType = item.ProductName,
                        Price = item.UnitPrice,
                        Total = item.Amount,
                        Note = item.Remark
                    });
                }
            }

            // 客户账单是否体现折扣
            if (vModel.OrderModel.RebateInBill)
            {
                vModel.PersonModels.Add(new PersonSetModel
                {
                    PersonType = "客户协议折让",
                    Total = vModel.OrderModel.InvoiceAmount - vModel.OrderModel.TolYsPrice,
                    Discount = 0,
                    Count = 0,
                    Price = 0,
                    Note = ""
                });
            }

            #region 获取上车点信息

            /*
             * 将上车点信息序列化到订单表的【LineBusPoint】字段（目的是解决线路上车点删除之后的Bug）
             * 为了与之前的订单不冲突，特做如下处理
            */
            if (vModel.OrderModel.LineBusPoint.IsNullOrEmpty())
            {
                //之前的订单，需要通过LineBusPointId去关联
                vModel.LineBusPointModel = vModel.OrderModel.LineBusPointId != 0 ? (_biz.GetLineBusPointModelById(vModel.OrderModel.LineBusPointId) ?? new TpLineBusPointModel()) : new TpLineBusPointModel();
            }
            else
            {
                //将上车点序列化到订单之后的处理方式
                var serialize = new JavaScriptSerializer();
                vModel.LineBusPointModel = serialize.Deserialize<TpLineBusPointModel>(vModel.OrderModel.LineBusPoint) ??
                                           new TpLineBusPointModel();
            }

            ////根据 【LineBusPointId==>TpLineBusPoint】获取上车信息
            //if (vModel.OrderModel.LineBusPointId == 0)//无上车点
            //    vModel.LineBusPointModel = new TpLineBusPointModel();
            //else//有上车点
            //    vModel.LineBusPointModel = _biz.GetLineBusPointModelById(vModel.OrderModel.LineBusPointId);

            #endregion 获取上车点信息

            //根据 【OwnerCode==>SysPlatform】获取平台信息
            vModel.PlatformModel = new PlatformBiz().GetByCustomerCode(vModel.OrderModel.OwnerCode);

            var businessCard = new SiteBiz().GetBusinessCard(vModel.OrderModel.LineId);
            vModel.LocalTravelAgency = businessCard.CustomerAccount;
            vModel.OrganizingTravelAgency = businessCard.PlatAccount;

            return vModel;
        }

        #endregion 账单

        #region 页面初始化

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected void InitPage()
        {
            ViewBag.OrderStateItem = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor();
            // ViewBag.OrderSource = DictionaryTools.GetEnumsBy(Enums.TourSourceEnum).ToSelectListFor();
            ViewBag.LineTypeRadioItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum);
            ViewBag.LineScopeItems = DictionaryTools.GetEnumsBy(Enums.LineScopeEnum).ToSelectListForNoDefualt();
            ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
        }

        #endregion 页面初始化

        #region 自定义函数

        private List<TpLineBusPointModel> InitBusPoints(TpOrderModel order)
        {
            var buspoints = _biz.GetLineBusPointByLineId(order.LineId);
            //var currentBs = order.LineBusPoint.ToJsonDeserialize<TpLineBusPointModel>();
            //if (buspoints.Where(a => a.Id == currentBs.Id).Count() <= 0)
            //    buspoints.Add(currentBs);
            return buspoints;
        }

        /// <summary>
        /// 根据线路类型 查找产品表对应的LineId
        /// </summary>
        /// <param name="lineType"></param>
        /// <returns></returns>
        private string GetLineIdsByLineLineType(string lineType)
        {
            var strTemp = "0";
            var lineModels = new TpLineBiz().GetIdsByLineType(lineType, UserInfo);
            if (lineModels.Count > 0)
            {
                strTemp = "";
                foreach (var lineModel in lineModels)
                {
                    strTemp += lineModel.LineId + ",";
                }
                strTemp = strTemp.Substring(0, strTemp.Length - 1);
            }
            return strTemp;
        }

        #endregion 自定义函数

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="fromId">页面来源</param>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult OrderDetail(string orderCode, int fromId = 0, int tourId = 0)
        {
            var vModel = new OrderEditVModel();
            vModel.Order = _biz.GetOrderLineTourist(orderCode);
            vModel.LineModel = vModel.Order.Line;

            // 结算客户
            if (string.IsNullOrEmpty(vModel.Order.SettleCustomer))
            {
                vModel.Order.SettleCustomerName = "";
            }
            else
            {
                vModel.Order.SettleCustomerName = new CustomerBiz().GetById(vModel.Order.SettleCustomer).Name;
            }
            // 销售姓名
            if (string.IsNullOrEmpty(vModel.Order.SalerCode))
            {
                vModel.Order.SalerName = "";
            }
            else
            {
                vModel.Order.SalerName = new AccountBiz().GetById(vModel.Order.SalerCode).Name;
            }

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长")
                || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                vModel.IsOP = 1;
            }

            //上车点数据加载
            // vModel.LineBusPoints = InitBusPoints(vModel.Order);
            vModel.Prices = _biz.GetPricesByTourId(vModel.Order.TourId);
            //获取游客信息
            vModel.Travellers = vModel.Order.TravellerModels;
            vModel.Travellers2 = vModel.Travellers.Where(a => a.State == 2).ToList(); // 有效
            vModel.Travellers10 = vModel.Travellers.Where(a => a.State != 2).ToList(); //已退团

            vModel.ListTourPayInModel = _payinBiz.GetPayInList(orderCode);
            vModel.ListTpInvoiceInfo = _invoiceBiz.GetInvoiceList(orderCode);
            vModel.FileList = _biz.GetOrderFileList(orderCode);

            vModel.TourPlan = _tourPlanBiz.GetTourById(vModel.Order.TourId);

            //加载子订单信息
            vModel.ChildOrderList = _childOrderBiz.GetTpChildOrderList(vModel.Order.OrderCode);
            // 取得日志
            vModel.LogList = new LogBiz().GetOrderLog(orderCode);

            ViewBag.fromId = fromId;
            ViewBag.tourId = tourId;
            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();

            InitPage();
            return View("Detail/OrderDetail", vModel);
        }

        #region 缴款单

        /// <summary>
        /// 新增缴款单页面
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <param name="Id">缴款单Id</param>
        /// <returns></returns>
        public ActionResult CreatePayInfo(int id, string OrderCode)
        {
            OrderPayInVModel vModel = new OrderPayInVModel();
            vModel.OrderModel = _biz.GetOrderByOrderCode(OrderCode);
            if (id > 0)
            {
                vModel.PayInModel = _payinBiz.GetById(id);
            }
            else
            {
                vModel.PayInModel = new TpOrderPayInModel();
                vModel.PayInModel.OrderCode = OrderCode;
                vModel.PayInModel.CustomerCode = vModel.OrderModel.SettleCustomer;
                vModel.PayInModel.PayInBy = vModel.OrderModel.SalerCode;
            }

            vModel.OrderFiles = _biz.GetOrderFileList(OrderCode);
            // 获得税号
            var custModel = _customerBiz.GetById(vModel.OrderModel.SettleCustomer);
            if (custModel != null)
            {
                vModel.PayInModel.TaxNumber = custModel.TaxNumber;
                vModel.PayInModel.CustomerName = custModel.Name;
            }

            ViewBag.CollectionType = DictionaryTools.GetEnumsBy(Enums.PayTypeEnum).ToSelectListFor();

            return View("Detail/CreatePayInfo", vModel);
        }

        /// <summary>
        /// 缴款保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SavePayInfo(OrderPayInVModel vModel)
        {
            vModel.PayInModel.State = 0;//未确认的状态
            vModel.PayInModel.CreatedTime = DateTime.Now;
            vModel.PayInModel.IsValid = 1;
            vModel.PayInModel.Type = 1;
            _payinBiz.AddPayIn(vModel.PayInModel);   // 保存
            var payInId = vModel.PayInModel.Id;

            if (payInId > 0)
            {
                if (!string.IsNullOrEmpty(vModel.PayInModel.TaxNumber))
                {
                    //更新掉客户的税号信息.
                    _customerBiz.UpdateTaxNumber(vModel.PayInModel.CustomerCode, vModel.PayInModel.TaxNumber);
                }

                #region 上传凭证信息

                HttpPostedFileBase file = Request.Files["orderFileName"];
                if (file != null && file.ContentLength > 0)
                {
                    string filename = "";
                    string filenameExt = "";
                    string FilePath = UploadOrderFile(vModel.PayInModel.OrderCode, "orderFileName", ref filename, ref filenameExt);

                    TpOrderFileModel model = new TpOrderFileModel();
                    model.KeyId = payInId;
                    model.OrderCode = vModel.PayInModel.OrderCode;
                    model.FileName = filename;
                    model.FilePath = FilePath;
                    model.CreatedTime = DateTime.Now;
                    model.Remark = vModel.PayInModel.Remark;
                    model.IsDel = 0;
                    model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                    model.MediaType = WebToolKit.GetFileMedia(filenameExt);
                    model.SourceType = "2";

                    var fid = _biz.AddOrderFile(model);
                    vModel.PayInModel.BankFileId = fid;
                }
                else if (!string.IsNullOrEmpty(vModel.selectBank))
                {
                    var f = _biz.GetOrderFileModel(Convert.ToInt32(vModel.selectBank));
                    if (f != null)
                    {
                        _biz.UpdateFileInPanIn(f.Id, payInId);
                        vModel.PayInModel.BankFileId = f.Id;
                    }
                }

                #endregion 上传凭证信息

                #region 上传账单

                HttpPostedFileBase file1 = Request.Files["billFile"];
                if (file1 != null && file1.ContentLength > 0)
                {
                    string filename = "";
                    string filenameExt = "";
                    string FilePath = UploadOrderFile(vModel.PayInModel.OrderCode, "billFile", ref filename, ref filenameExt);

                    TpOrderFileModel model = new TpOrderFileModel();
                    model.KeyId = payInId;
                    model.OrderCode = vModel.PayInModel.OrderCode;
                    model.FileName = filename;
                    model.FilePath = FilePath;
                    model.CreatedTime = DateTime.Now;
                    model.Remark = vModel.PayInModel.Remark;
                    model.IsDel = 0;
                    model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                    model.MediaType = WebToolKit.GetFileMedia(filenameExt);
                    model.SourceType = "4";

                    var fid = _biz.AddOrderFile(model);
                    vModel.PayInModel.BillFileId = fid;
                }
                else if (!string.IsNullOrEmpty(vModel.selectBill))
                {
                    var f = _biz.GetOrderFileModel(Convert.ToInt32(vModel.selectBill));
                    if (f != null)
                        vModel.PayInModel.BillFileId = f.Id;
                }

                #endregion 上传账单
            }

            _payinBiz.Update(vModel.PayInModel);

            return Json(new { Code = "1", Message = "添加成功" });
        }

        public ActionResult PayInDetail(int id, string OrderCode)
        {
            OrderPayInVModel vModel = new OrderPayInVModel();
            vModel.OrderModel = _biz.GetOrderByOrderCode(OrderCode);
            vModel.OrderFiles = _biz.GetOrderFileList(OrderCode);
            vModel.PayInModel = _payinBiz.GetOrderPayInModelById(id);

            return View("Detail/PayInDetail", vModel);
        }

        public ActionResult PayinPrint(int id)
        {
            OrderPayInVModel vModel = new OrderPayInVModel();
            vModel.PayInModel = _payinBiz.GetOrderPayInModelById(id);
            vModel.OrderModel = _biz.GetOrderByOrderCode(vModel.PayInModel.OrderCode);
            vModel.OrderFiles = _biz.GetOrderFileList(vModel.PayInModel.OrderCode);

            TempData["PayInPrintVModel"] = vModel;
            return View("/Views/Order/Print/PrintPayIn.aspx", vModel);
        }

        /// <summary>
        /// 上传缴款单凭证
        /// </summary>
        /// <returns></returns>
        public ActionResult AddUpLoadOrderFile(OrderEditVModel vModel)
        {
            string filename = "";
            string fileExt = "";
            TpOrderModel orderModel = _biz.GetOrderByOrderCode(vModel.FileModel.OrderCode);
            if (orderModel != null)
            {
                string FilePath = UploadOrderFile(vModel.FileModel.OrderCode, "orderFileName", ref filename, ref fileExt);

                TpOrderFileModel model = new TpOrderFileModel();
                model.SourceType = vModel.FileModel.SourceType;
                model.KeyId = vModel.FileModel.KeyId;
                model.OrderCode = vModel.FileModel.OrderCode;
                model.FileName = filename;
                model.FilePath = FilePath;
                model.CreatedTime = DateTime.Now;
                model.Remark = vModel.FileModel.Remark;
                model.IsDel = 0;
                model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.MediaType = WebToolKit.GetFileMedia(fileExt);
                _biz.AddOrderFile(model);
            }

            vModel.FileList = _biz.GetOrderFileList(vModel.FileModel.OrderCode);
            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();

            return PartialView("Detail/UCAddOrderFile", vModel);
        }

        public ActionResult ReLoadPayIn(string orderCode)
        {
            var vModel = new OrderEditVModel();
            vModel.ListTourPayInModel = _payinBiz.GetPayInList(orderCode);

            return PartialView("Detail/TourPayInfo", vModel);
        }

        public ActionResult DeletePayIn(int id, string orderCode)
        {
            _payinBiz.DeletePayIn(id, 0);
            return Json(new { Code = "1", Message = "Success" });
        }

        #endregion 缴款单

        #region 订单附件

        private string UploadOrderFile(string OrderCode, string requestFileName, ref string file_name, ref string file_extension)
        {
            HttpPostedFileBase file = Request.Files[requestFileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            file_name = file.FileName;
            file_extension = Path.GetExtension(file.FileName);
            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = string.Format(@"order\{0}", OrderCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        /// <summary>
        /// 需要修改  //TODO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteOrderFile(int id)
        {
            TpOrderFileModel model = _biz.GetOrderFileModel(id);
            _biz.DeleteOrderFile(id);

            // 重新查询
            OrderEditVModel md = new OrderEditVModel();
            md.FileList = _biz.GetOrderFileList(model.OrderCode);
            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).ToSelectListFor();
            return PartialView("Detail/UCAddOrderFile", md);
        }

        /// <summary>
        /// 需要修改  //TODO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DownLoadFile(int id)
        {
            TpOrderFileModel model = _biz.GetOrderFileModel(id);
            if (model == null)
                return null;

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
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, model.OrderCode, "", GlobalContext.Current.UserInfo.Code, "账单下载，修订号：" + model.Revision, 0);
            }
            else if (model.SourceType == "5")
            {
                // 记录下载日志
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, model.OrderCode, "", GlobalContext.Current.UserInfo.Code, "成团通知下载，修订号：" + model.Revision, 0);
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

        #endregion 订单附件

        #region 发票列表

        public ActionResult ReLoadInvoice(string orderCode)
        {
            var vModel = new OrderEditVModel();
            vModel.ListTpInvoiceInfo = _invoiceBiz.GetInvoiceList(orderCode);

            return PartialView("Detail/InvoiceList", vModel);
        }

        /// <summary>
        ///  新增发票申请
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="orderCode">订单编号</param>
        /// <param name="lineType">线路类型 出境/国内</param>
        /// <returns></returns>
        public ActionResult CreateInvoice(int Id, string orderCode, string lineType)
        {
            TpInvoiceModel vModel = new TpInvoiceModel();
            if (Id > 0)
            {
                vModel = _invoiceBiz.GetInvoiceById(Id);
            }
            else
            {
                // 根据订单的结算客户 获取开户行信息
                var orderModel = _biz.GetOrderByOrderCode(orderCode);
                if (!string.IsNullOrEmpty(orderModel.SettleCustomer))
                {
                    vModel.SettleCustomer = orderModel.SettleCustomer;
                    var customer = _customerBiz.GetById(orderModel.SettleCustomer);
                    vModel.CustomerName = customer.Name;
                    vModel.Address = customer.Address;
                    vModel.TaxNumber = customer.TaxNumber;

                    if (!customer.BankInfo.IsNullOrEmpty())
                    {
                        BankInfoModel mm = JsonConvert.DeserializeObject<BankInfoModel>(customer.BankInfo);

                        vModel.BankName = mm.BankName;
                        vModel.BankAccount = mm.BankAccount;
                        vModel.Phone = mm.Phone;
                        vModel.Address = mm.Address;
                    }
                }
                vModel.OrderCode = orderCode;
            }
            if (lineType == "3")
            {
                // 出境
                ViewBag.InvoiceTitleEnum = DictionaryTools.GetEnumsBy(Enums.OutboundInvoiceTitleEnum).ToSelectListFor(k => k.Value, v => v.Value);
            }
            else
            {
                ViewBag.InvoiceTitleEnum = DictionaryTools.GetEnumsBy(Enums.InboundInvoiceTitleEnum).ToSelectListFor(k => k.Value, v => v.Value);
            }

            return View("Detail/CreateInvoice", vModel);
        }

        /// <summary>
        /// 保存发票申请
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SaveInvoce(TpInvoiceModel vModel)
        {
            vModel.CreatedBy = UserInfo.Code;
            vModel.IsValid = 1;
            vModel.CreatedTime = DateTime.Now;
            _invoiceBiz.AddInvoice(vModel);

            if (!vModel.SettleCustomer.IsNullOrEmpty())
            {
                var customer = _customerBiz.GetById(vModel.SettleCustomer);
                if (customer.Name == vModel.CustomerName)
                {
                    BankInfoModel mm = new BankInfoModel
                    {
                        CustomerName = vModel.CustomerName,
                        BankName = vModel.BankName,
                        BankAccount = vModel.BankAccount,
                        Address = vModel.Address,
                        Phone = vModel.Phone
                    };
                    string userData = JsonConvert.SerializeObject(mm);
                    _customerBiz.UpdateBankInfo(vModel.SettleCustomer, userData);
                }
            }

            return Json(new { Code = "1", Message = "添加成功" });
        }

        public ActionResult DeleteInvoice(int id)
        {
            _invoiceBiz.SetValid(id, 0);
            return Json(new { Code = "1", Message = "Success" });
        }

        #endregion 发票列表

        #region 子订单的相关操作方法

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult UcChildOrer(string orderCode)
        {
            var vModel = new OrderEditVModel();

            if (GlobalContext.Current.IsSysAdmin
                || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长")
                || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                vModel.IsOP = 1;
            }
            //加载子订单信息
            vModel.ChildOrderList = _childOrderBiz.GetTpChildOrderList(orderCode);

            return PartialView("Edit/UCChildOrders", vModel);
        }

        public ActionResult AddChildOrder(int id, string lineId, string OrderCode, string teamCode)
        {
            var model = new TpChildOrderModel();
            if (id > 0)
            {
                //编辑
                model = _childOrderBiz.GetTpChildOrderById(id);
            }
            else
            {
                model.OrderCode = OrderCode;
            }

            ViewData["lineType"] = DictionaryTools.GetEnumsBy(Enums.SupplierCostItemsEnum).ToSelectListFor();
            ViewData["supplierList"] = _customerBiz.GetAllSupplier().ToSelectListFor(t => t.Code, t => t.Name);
            ViewData["itemList"] = _productBiz.GetProductByTeam(teamCode).ToSelectListFor(t => t.ProductID.ToString(), t => t.ProductName);

            return View("Edit/AddChildOrder", model);
        }

        /// <summary>
        /// 保存/编辑子订单0
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveChildOrder(TpChildOrderModel model)
        {
            if (model != null)
            {
                model.Amount = model.UnitPrice * model.Quantity;
                if (model.Id == 0)
                {
                    long i = _childOrderBiz.SaveTpChildOrder(model);
                    if (i > 0)
                    {
                        _biz.CalcAmount(model.OrderCode);
                        return Json(new { Code = 0, Message = "保存成功！" });
                    }
                }
                else
                {
                    //编辑
                    int i = _childOrderBiz.UpdateTpChildOrder(model);
                    if (i > 0)
                    {
                        _biz.CalcAmount(model.OrderCode);
                        return Json(new { Code = 0, Message = "保存成功！" });
                    }
                }
            }

            return Json(new { Code = 1, Message = "保存失败！" });
        }

        /// <summary>
        /// 取消子订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CancelChidOrder(int id, string orderCode)
        {
            //取消子订单 重新计算订单的总金额，修改子订单的状态为1取消
            try
            {
                int i = _childOrderBiz.CancelChidOrder(id);
                if (i > 0)
                {
                    _biz.CalcAmount(orderCode);
                    return Json(new { Code = 0, Message = "操作成功！" });
                }

                return Json(new { Code = 1, Message = "操作失败！" });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { Code = 1, Message = "操作失败！" });
            }
        }

        /// <summary>
        /// 恢复子订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RecoverChidOrder(int id, string orderCode)
        {
            //回复子订单 重新计算订单的总金额，修改子订单的状态为0 正常
            try
            {
                int i = _childOrderBiz.RecoverChidOrder(id);
                if (i > 0)
                {
                    _biz.CalcAmount(orderCode);
                    return Json(new { Code = 0, Message = "操作成功！" });
                }

                return Json(new { Code = 1, Message = "操作失败！" });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { Code = 1, Message = "操作失败！" });
            }
        }

        #endregion 子订单的相关操作方法

        /// <summary>
        /// 取得客户信息和折让规则
        ///
        /// </summary>
        /// <param name="custcode">客户ID</param>
        /// <param name="lineDest">产品目的地ParentStr</param>
        /// <returns></returns>
        public ActionResult GetCustomerInfo(string custcode, string lineDest)
        {
            // lineDest示例 /8/9/12/
            var list = _customerBiz.GetPolicyList(custcode);
            // 获得目的地规则
            var entity = _biz.GetBestPolicy(list, lineDest);

            var model = new
            {
                contacts = _customerBiz.GetContactListByCustomerCode(custcode),
                policy = entity
            };

            return Json(model);
        }

        /// <summary>
        /// 页面获取订单信息
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult JsonOrderInfo(string orderCode)
        {
            var order = _biz.GetOrderLineTourist(orderCode);
            order.PayInList = _payinBiz.GetPayInList(orderCode);

            return Json(order, JsonRequestBehavior.AllowGet);
        }

        #region 核对游客信息

        public ActionResult CheckTourists(string orderCode, int? tralId)
        {
            var vModel = new CheckTouristsVModel();
            var order = _biz.GetOrderByOrderCode(orderCode);
            vModel.PassportExpiry = order.OutDate.AddDays(order.TravelDays + 180);  // 签证有效期
            var list = _travellerBiz.GetByOrderCode(orderCode).OrderBy(t => t.Id);
            if (tralId != null)
            {
                vModel.TouristsInfo = list.FirstOrDefault(s => s.Id == tralId);  // 指定游客信息
            }
            else
            {
                vModel.TouristsInfo = list.FirstOrDefault();  // 获得订单第一个游客
            }
            int id = vModel.TouristsInfo != null ? (int)vModel.TouristsInfo.Id : 0;
            vModel.TouristsId = id;
            vModel.TouristsFile = _travellerBiz.GetTouristsFileList(id);
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长")
                        || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                vModel.IsOP = 1;
            }

            return View(vModel);
        }

        /// <summary>
        /// 保存游客资料
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckTourists(CheckTouristsVModel vModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var model = _travellerBiz.GetById(vModel.TouristsId);
                    model.Name = vModel.TouristsInfo.Name;
                    model.PinYin = vModel.TouristsInfo.PinYin;
                    model.Sex = vModel.TouristsInfo.Sex;
                    model.DateOfBirth = vModel.TouristsInfo.DateOfBirth;
                    model.PlaceOfBirth = vModel.TouristsInfo.PlaceOfBirth;
                    model.Phone = vModel.TouristsInfo.Phone;
                    model.PassType = vModel.TouristsInfo.PassType;
                    model.PassNo = vModel.TouristsInfo.PassNo;
                    model.DateOfExpiry = vModel.TouristsInfo.DateOfExpiry;
                    model.IsChecked = vModel.TouristsInfo.IsChecked;
                    if (vModel.TouristsInfo.PassType != 1)
                    {
                        model.DateOfIssue = vModel.TouristsInfo.DateOfIssue;
                        model.PlaceOfIssue = vModel.TouristsInfo.PlaceOfIssue;
                    }
                    bool result = _travellerBiz.Update(model) > 0;
                    if (result)
                    {
                        return Json(new { code = 200, message = "保存成功" });
                    }
                }
                return Json(new { code = 200, message = "保存失败" });
            }
            catch (Exception)
            {
                return Json(new { code = 200, message = "操作异常" });
            }
        }

        [HttpGet]
        public ActionResult GetPreTourists(string orderCode, int tralId)
        {
            try
            {
                var vModel = new CheckTouristsVModel();
                vModel.TouristsInfo = _travellerBiz.GetPreByOrderCode(tralId, orderCode);
                if (vModel.TouristsInfo != null)
                {
                    vModel.TouristsFile = _travellerBiz.GetTouristsFileList((int)vModel.TouristsInfo.Id);
                    vModel.TouristsId = (int)vModel.TouristsInfo.Id;
                }
                return Json(new { code = 200, data = vModel, message = "获取数据成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, message = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetNextTourists(string orderCode, int tralId)
        {
            try
            {
                var vModel = new CheckTouristsVModel();
                vModel.TouristsInfo = _travellerBiz.GetNextByOrderCode(tralId, orderCode);
                if (vModel.TouristsInfo != null)
                {
                    vModel.TouristsFile = _travellerBiz.GetTouristsFileList((int)vModel.TouristsInfo.Id);
                    vModel.TouristsId = (int)vModel.TouristsInfo.Id;
                }
                return Json(new { code = 200, data = vModel, message = "获取数据成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, message = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion 核对游客信息
    }
}