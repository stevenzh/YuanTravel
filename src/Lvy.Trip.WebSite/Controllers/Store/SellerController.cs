using Common.Logging;
using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Models.TourDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Ticket;
using Lvy.Trip.WebSite.Mvc.Attributes;
using Lvy.VModels.Excel;
using Lvy.VModels.Order;
using Lvy.VModels.Saler;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 分销商后台管理
    /// </summary>
    [LvyAuth]
    public class SellerController : BaseController
    {
        private ILog logger = LogManager.GetLogger("SellerController");

        private readonly OrderBiz _biz = new OrderBiz();
        private readonly TktOrderBiz _tktOrderBiz = new TktOrderBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TpLineBiz _lineBiz = new TpLineBiz();
        private readonly TravellerBiz trvBiz = new TravellerBiz();
        private readonly TpPriceBiz priceBiz = new TpPriceBiz();

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        #region 团队游

        /// <summary>
        /// 查询订单倒计时
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SearchTimerOrder(TpOrderVModel vModel)
        {
            var userInfo = GlobalContext.Current.UserInfo;

            vModel.OwnerCode = GlobalContext.Current.OwnerCode;
            vModel.CustomerCode = userInfo.CustomerCode;
            vModel.OrderModels = _biz.GetListBySaler(vModel, UserInfo);
            foreach (var item in vModel.OrderModels)
            {
                item.BillFile = _biz.GetOrderFileList(item.OrderCode).Where(m => m.SourceType == "3").OrderByDescending(m => m.Revision).FirstOrDefault();
            }

            if (Request.IsAjaxRequest())
                return PartialView("OrderTimer/UCTimerOrderSearch", vModel);
            return View("OrderTimer/SearchTimerOrder", vModel);
        }

        /// <summary>
        /// 团队游订单预定统计
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// 我的订单模块->团队游预定统计
        /// 用户：有分销功能的批发商、分销商
        /// </remarks>
        public ActionResult OrderSearch(OrderStatisticVModel vModel)
        {
            InitOrderStatistic();
            if (null == vModel.Condition) vModel.Condition = new OrderStatisticCondition();
            if (null == vModel.OrderModels) vModel.OrderModels = new PagedList<TpOrderModel>();

            //根据 当前分销商 过滤
            // vModel.Condition.BookingCustomer = GlobalContext.Current.UserInfo.CustomerCode;

            vModel.OrderModels = _biz.GetMyOrderStatistic(vModel, true, UserInfo);
            foreach (var item in vModel.OrderModels.Items)
            {
                item.BillFile = _biz.GetOrderFileList(item.OrderCode).Where(m => m.SourceType == "3").OrderByDescending(m => m.Revision).FirstOrDefault();
            }
            var summary = _biz.GetStatisticSummary(vModel, UserInfo);
            vModel.SumPriceCount = summary.SumPriceCount;
            vModel.SumTolPaid = summary.SumTolPaid;
            vModel.ShengYuCount = summary.ShengYuCount;
            vModel.SumTravellerCount = summary.SumTravellerCount;
            vModel.SumFanLiCount = summary.SumFanLiCount;

            if (Request.IsAjaxRequest())
                return PartialView("Order/UCOrderList", vModel);
            return View("Order/OrderSearch", vModel);
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected override void InitPage()
        {
            ViewBag.OrderState = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor();
            ViewBag.LineTypeRadioItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum);
        }

        /// <summary>
        /// 根据线路类型 查找产品表对应的LineId
        /// </summary>
        /// <param name="lineType"></param>
        /// <returns></returns>
        private string GetLineIdsByLineLineType(string lineType)
        {
            var strTemp = "0";
            var lineModels = new List<TpLineModel>();
            lineModels = _lineBiz.GetIdsByLineType(lineType, UserInfo);
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

        /// <summary>
        /// 初始化预定统计相关数据
        /// </summary>
        public void InitOrderStatistic()
        {
            ViewBag.SettlementStateBean = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key = "1",Value = "已结算"},
                                         new KeyValueBean{Key="0",Value="未结算"}
                                     }.ToSelectListFor();

            ViewBag.CustomerList = _customerBiz.GetCustomers(GlobalContext.Current.UserInfo.CustomerCode).Select(p => new KeyValueBean() { Key = p.Code, Value = p.Name }).ToSelectListFor();
        }

        /// <summary>
        /// 导出预定统计
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public FileContentResult DownloadBookingOrder(OrderStatisticVModel vModel)
        {
            ////根据 分销商 模糊查找对应的 CustomerCode
            //if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
            //    vModel.Condition.BookingCustomer = GetCustomerCodes(vModel.Condition.BookingCustomer);
            //根据 产品类型 查找对应的 LineId
            if (!vModel.Condition.LineType.IsNullOrEmpty())
                vModel.Condition.LineType = GetLineIdsByLineLineType(vModel.Condition.LineType);
            var orders = _biz.GetMyOrderStatistic(vModel, false, UserInfo).Items;
            if (null == orders || orders.Count == 0)
                return null;

            orders = orders.Where(a => a.IsCancel != 1)
                         .OrderBy(a => a.OutDate)
                         .ThenBy(a => a.TourId)
                         .ToList();

            var tourists = new List<TpTravellerModel>();
            var tourPirces = new List<TpPriceModel>();
            foreach (var order in orders)
            {
                var temp = trvBiz.GetByOrderCode(order.OrderCode);// 包含已取消游客

                tourists.AddRange(temp);
                var temp2 = priceBiz.GetPrices(order.TourId);
                tourPirces.AddRange(temp2); // 该团所有报价
            }

            var datas = (from o in orders
                         select new OrderExcelVModel()
                         {
                             OrderCode = o.Id.ToString(),
                             JoinOrderCode = o.JoinOrderCode,
                             TourName = o.TourId.ToString() + "-" + o.LineName,
                             OutDate = o.OutDate.ToDateFormat(),
                             BookingCustomer = ExportUtils.GetBookingCustomer(o.BookingCustomer, OwnerCode),
                             LinkMan = o.LinkMan,
                             LinkPhone = o.LinkPhone,
                             Managers = o.Managers,
                             ManagerPhone = o.ManagerPhone,
                             TravellerCount = o.TravellerCount,
                             TolYsPrice = o.TolYsPrice,
                             ZiFei = ExportUtils.GetZifei(o.OrderCode, tourists),
                             SingleRoom = ExportUtils.GetSingleRoom(o.OrderCode, tourists),
                             JsPrice = tourists.Where(b => b.OrderCode == o.OrderCode && b.State == 2).Sum(b => b.JiePrice + b.SongPrice),
                             PriceContents = ExportUtils.GetPriceContents(o, tourists, tourPirces),
                             OrderState = DictionaryTools.GetEnumValue(Enums.OrderStateEnum, o.OrderState.ToString()),
                             Remark = o.Remark
                         }).ToList();

            using (var ms = new MemoryStream())
            {
                var workBook = new HSSFWorkbook();
                // 新增試算表。
                var sheet1 = workBook.CreateSheet("预定统计");

                Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 0, string.Empty);
                Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1);

                Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);
                Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                row.CreateCell(10).SetCellValue((double)datas.Sum(a => a.TolYsPrice));
                row.CreateCell(9).SetCellValue(datas.Sum(a => a.TravellerCount));

                // 自适应宽度
                Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1);

                workBook.Write(ms);
                return File(ms.GetBuffer(), "application/vnd.ms-excel", HttpUtility.UrlEncode("预定统计.xls"));
            }
        }

        #endregion 团队游

        #region 团队门票及统计

        /// <summary>
        /// 我的订单->团队门票
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult TktOrderSearch(TktOrderVModel vModel)
        {
            if (vModel.Orders == null)
                vModel.Orders = new PagedList<TpTourBalanceModel>();
            vModel.Orders = _tktOrderBiz.GetTktOrderStatistic(vModel, true, UserInfo);
            _tktOrderBiz.StatisticTktOrder(vModel, UserInfo);
            InitOrderStatistic();
            if (Request.IsAjaxRequest())
                return PartialView("Ticket/UCTktOrderList", vModel);
            return View("Ticket/TktOrderSearch", vModel);
        }

        /// <summary>
        /// 打印确认单
        /// </summary>
        /// <returns></returns>
        public ActionResult PrintConfirmation(string orderCode)
        {
            var vModel = _tktOrderBiz.GetConfirmOrder(orderCode);
            return View("Ticket/PrintConfirmOrder", vModel);
        }

        /// <summary>
        /// 假删除订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult Delete(string orderId)
        {
            var flag = _tktOrderBiz.DeleteOrderInfo(orderId, UserInfo);
            return Content(flag.ToString());
        }

        #endregion 团队门票及统计
    }
}