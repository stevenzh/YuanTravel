using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Models.TourDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.VModels.Finance;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Finance
{
    /// <summary>
    /// 财务付款
    /// </summary>
    public partial class PaymentController : BaseController
    {
        private FinanceBiz _financeBiz = new FinanceBiz();
        private PlatformBiz _platformBiz = new PlatformBiz();
        private TpLineBiz _lineBiz = new TpLineBiz();
        private CustomerBiz _customerBiz = new CustomerBiz();
        private PaymentBiz _paymentBiz = new PaymentBiz();
        private TeamBiz _teamBiz = new TeamBiz();

        #region 财务付款

        /// <summary>
        /// 查询付款列表
        /// </summary>
        /// <param name="financeVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchPayment(PaymentVModel financeVModel)
        {
            InitPage();

            //供应商
            if (!financeVModel.Condition.CostSupplierName.IsNullOrEmpty())
                financeVModel.Condition.CostSupplierName = CreateBookingCustomerQueryString(financeVModel.Condition.CostSupplierName);
        
            financeVModel.OwnerCode = UserInfo.OwnerCode;
            //获取订单列表信息
            financeVModel.CostModels = _paymentBiz.GetCostList(financeVModel);
            financeVModel.TotalModel = _paymentBiz.GetFinanceSummary(financeVModel);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", financeVModel);
            return View(financeVModel);
        }

        /// <summary>
        /// 编辑成本跳转-视图
        /// </summary>
        /// <param name="TourId"></param>
        /// <returns></returns>
        public ActionResult EditCost(string orderCode)
        {
            return RedirectToAction("EditTourBalance", "Finance", new { orderCode = orderCode, isCaiWu = 1 });
        }

        /// <summary>
        /// 批量付款
        /// </summary>
        /// <param name="chkOpOrderCode"></param>
        /// <returns></returns>
        public ActionResult BatchOrderFinance(string chkOpOrderCode)
        {
            var cost = _paymentBiz.GetCost(chkOpOrderCode.Split('|'), UserInfo.OwnerCode);
            var paymentInfoList = new List<TpPaymentModel>();
            for (int i = 0; i < cost.Count; i++)
            {
                _paymentBiz.FuKuan(cost[i].Id, cost[i].ItemCost - cost[i].PaidCost, UserInfo);
            }

            return Content("1");
        }

        /// <summary>
        /// 付款确认
        /// </summary>
        /// <param name="chkOpOrderCode"></param>
        /// <returns></returns>
        public ActionResult MultiOrderPaymentCheck(string chkOpOrderCode)
        {
            var orderCodes = chkOpOrderCode.Split('|').ToList();
            if (orderCodes.Count > 0)
            {
                orderCodes = orderCodes.Distinct().ToList();
                var orders = new OrderBiz().GetOrder(orderCodes, UserInfo.OwnerCode);
                var totalYs = orders.Sum(p => p.TolYsPrice);
                var totalPaid = orders.Sum(p => p.TolPaid);
                var totalUnPaid = totalYs - totalPaid;
                var vModel = new PayInConfirmVModel
                {
                    OrderId = string.Join(",", orders.Select(p => p.Id)),
                    TotalYs = totalYs,
                    TotalPaid = totalPaid,
                    TotalUnPaid = totalUnPaid
                };
                return PartialView("UCMultiPaymentConfirm", vModel);
            }
            return PartialView("UCMultiPaymentConfirm", null);
        }

        /// <summary>
        /// 付款确认
        /// </summary>
        /// <param name="costId"></param>
        /// <returns></returns>
        public ActionResult SingleOrderPaymentCheck(int costId)
        {
            var costModel = _paymentBiz.GetCostById(costId);
            return PartialView("UCSinglePaymentConfirm", costModel);
        }

        /// <summary>
        /// 单项付款
        /// </summary>
        /// <returns></returns>
        public ActionResult OrdersTolPaid(int costId, decimal currentPayment)
        {
            int row = _paymentBiz.FuKuan(costId, currentPayment, UserInfo);

            return Content(row.ToString());
        }

        #endregion 财务付款

        #region 导出

        /// <summary>
        ///导出财务对账单
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportExcel(FinanceVModel financeVModel)
        {
            var workBook = new HSSFWorkbook();
            var ms = new MemoryStream();
            try
            {
                //创建工作簿
                var sheet1 = workBook.CreateSheet("财务对账单");
                var rowHeight = 20 * 20;

                #region 单元格格式设置

                #region 格式一:字体加粗+宋体+字号10+垂直居中+水平居中

                var cellStyle = workBook.CreateCellStyle();
                var cFont = workBook.CreateFont();
                cFont.IsBold = true;//字体加粗
                cFont.FontName = "宋体";//字体名称
                cFont.FontHeightInPoints = 18;//字号
                cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle.SetFont(cFont);

                #endregion 格式一:字体加粗+宋体+字号10+垂直居中+水平居中

                #region 格式二:字体不加粗+宋体+字号10+垂直居中+水平居中+上边框加粗

                var cFont2 = workBook.CreateFont();
                var cellStyle2 = workBook.CreateCellStyle();
                var hssfDataFormat = workBook.CreateDataFormat();
                cFont2.IsBold = false;//字体加粗
                cFont2.FontName = "宋体";//字体名称
                cFont2.FontHeightInPoints = 10;//字号
                cellStyle2.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle2.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle2.BorderTop = BorderStyle.Medium;//上边框
                cellStyle2.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle2.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle2.BorderRight = BorderStyle.Thin;//右边框
                cellStyle2.SetFont(cFont2);

                #endregion 格式二:字体不加粗+宋体+字号10+垂直居中+水平居中+上边框加粗

                #region 格式三:字体不加粗+宋体+字号10+垂直居中+水平居中+右边框加粗

                var cFont3 = workBook.CreateFont();
                var cellStyle3 = workBook.CreateCellStyle();
                cFont3.IsBold = false;//字体加粗
                cFont3.FontName = "宋体";//字体名称
                cFont3.FontHeightInPoints = 10;//字号
                cellStyle3.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle3.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle3.BorderTop = BorderStyle.Thin;//上边框
                cellStyle3.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle3.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle3.BorderRight = BorderStyle.Medium;//右边框
                cellStyle3.SetFont(cFont3);

                #endregion 格式三:字体不加粗+宋体+字号10+垂直居中+水平居中+右边框加粗

                #region 格式四:字体不加粗+宋体+字号10+垂直居中+水平居中+上下左右不加粗

                var cFont4 = workBook.CreateFont();
                var cellStyle4 = workBook.CreateCellStyle();
                cFont4.IsBold = false;//字体加粗
                cFont4.FontName = "宋体";//字体名称
                cFont4.FontHeightInPoints = 10;//字号
                cellStyle4.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle4.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle4.BorderTop = BorderStyle.Thin;//上边框
                cellStyle4.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle4.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle4.BorderRight = BorderStyle.Thin;//右边框
                cellStyle4.SetFont(cFont4);

                #endregion 格式四:字体不加粗+宋体+字号10+垂直居中+水平居中+上下左右不加粗

                #region 格式五:字体不加粗+宋体+字号10+垂直居中+水平居中+上右加粗

                var cFont5 = workBook.CreateFont();
                var cellStyle5 = workBook.CreateCellStyle();
                cFont5.IsBold = false;//字体加粗
                cFont5.FontName = "宋体";//字体名称
                cFont5.FontHeightInPoints = 10;//字号
                cellStyle5.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle5.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle5.BorderTop = BorderStyle.Medium;//上边框
                cellStyle5.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle5.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle5.BorderRight = BorderStyle.Medium;//右边框
                cellStyle5.SetFont(cFont5);

                #endregion 格式五:字体不加粗+宋体+字号10+垂直居中+水平居中+上右加粗

                #region 格式六:字体不加粗+宋体+字号10+垂直居中+水平居中+上加粗

                var cFont6 = workBook.CreateFont();
                var cellStyle6 = workBook.CreateCellStyle();
                cFont6.IsBold = false;//字体加粗
                cFont6.FontName = "宋体";//字体名称
                cFont6.FontHeightInPoints = 12;//字号
                cellStyle6.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle6.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle6.BorderTop = BorderStyle.Medium;//上边框
                cellStyle6.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle6.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle6.BorderRight = BorderStyle.Thin;//右边框
                cellStyle6.SetFont(cFont6);

                #endregion 格式六:字体不加粗+宋体+字号10+垂直居中+水平居中+上加粗

                #region 格式七:字体不加粗+宋体+字号12+垂直居下+水平居左+上加粗

                var cFont7 = workBook.CreateFont();
                var cellStyle7 = workBook.CreateCellStyle();
                cFont7.IsBold = false;//字体加粗
                cFont7.FontName = "宋体";//字体名称
                cFont7.FontHeightInPoints = 12;//字号
                cellStyle7.VerticalAlignment = VerticalAlignment.Bottom;//垂直居下
                cellStyle7.Alignment = HorizontalAlignment.Left;//水平居左
                cellStyle7.BorderTop = BorderStyle.Medium;//上边框
                cellStyle7.SetFont(cFont7);

                #endregion 格式七:字体不加粗+宋体+字号12+垂直居下+水平居左+上加粗

                #endregion 单元格格式设置

                #region 第一行

                var row1 = sheet1.CreateRow(0);
                row1.Height = (short)rowHeight;
                //合并单元格
                sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, 10));
                row1.CreateCell(0).SetCellValue("对账单");
                row1.Height = 20 * 30;
                row1.GetCell(0).CellStyle = cellStyle;

                #endregion 第一行

                #region 第二行

                var row2 = sheet1.CreateRow(1);
                row2.Height = (short)rowHeight;
                row2.CreateCell(0).SetCellValue("订单号");
                row2.CreateCell(1).SetCellValue("关联订单号");
                row2.CreateCell(2).SetCellValue("团号");
                row2.CreateCell(3).SetCellValue("分销商");
                row2.CreateCell(4).SetCellValue("线路名称");
                row2.CreateCell(5).SetCellValue("发团日期");
                row2.CreateCell(6).SetCellValue("人数");
                row2.CreateCell(7).SetCellValue("应收");
                row2.CreateCell(8).SetCellValue("客人姓名");
                row2.CreateCell(9).SetCellValue("分销商联系人");
                row2.CreateCell(10).SetCellValue("备注");

                row2.GetCell(0).CellStyle = cellStyle2;
                row2.GetCell(1).CellStyle = cellStyle2;
                row2.GetCell(2).CellStyle = cellStyle2;
                row2.GetCell(3).CellStyle = cellStyle2;
                row2.GetCell(4).CellStyle = cellStyle2;
                row2.GetCell(5).CellStyle = cellStyle2;
                row2.GetCell(6).CellStyle = cellStyle2;
                row2.GetCell(7).CellStyle = cellStyle2;
                row2.GetCell(8).CellStyle = cellStyle2;
                row2.GetCell(9).CellStyle = cellStyle2;
                row2.GetCell(10).CellStyle = cellStyle5;

                #endregion 第二行

                #region 第三行......循环加载数据

                var orderModels = new List<TpOrderModel>();

                ////根据 分销商 模糊查找对应的 CustomerCode
                //if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
                //    financeVModel.Condition.BookingCustomer = GetCustomerCodes(financeVModel.Condition.BookingCustomer);
                //报名客户查询条件
                if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
                    financeVModel.Condition.BookingCustomer = CreateBookingCustomerQueryString(financeVModel.Condition.BookingCustomer);

                //根据 产品类型 查找对应的 LineId
                if (!financeVModel.Condition.LineType.IsNullOrEmpty())
                    financeVModel.Condition.LineType = GetLineIdsByLineLineType(financeVModel.Condition.LineType);
                //根据指定的条件获取订单列表信息
                orderModels = _financeBiz.ExportDuiZhangDan(financeVModel, UserInfo); //_orderBiz.GetOrderList(financeVModel, UserInfo.OwnerCode);
                var cnt = orderModels.Count;
                var rowIndex = 2;
                if (cnt > 0)
                {
                    for (int i = 0; i < cnt; i++)
                    {
                        var row3 = sheet1.CreateRow(rowIndex);
                        row3.Height = (short)rowHeight;
                        //根据报名账户编码获取报名账户名称
                        var customer = DictionaryTools.GetCachedCustomer(orderModels[i].BookingCustomer);

                        row3.CreateCell(0).SetCellValue(orderModels[i].Id);
                        row3.CreateCell(1).SetCellValue(orderModels[i].JoinOrderCode);
                        row3.CreateCell(2).SetCellValue(orderModels[i].TourId);
                        row3.CreateCell(3).SetCellValue(customer.FastCode + "-" + customer.Name);
                        row3.CreateCell(4).SetCellValue(orderModels[i].LineName);
                        row3.CreateCell(5).SetCellValue(orderModels[i].OutDate.ToDateFormat());
                        row3.CreateCell(6).SetCellValue(orderModels[i].TravellerCount.ToString());
                        row3.CreateCell(7).SetCellValue(orderModels[i].TolYsPrice.ToString());
                        row3.CreateCell(8).SetCellValue(orderModels[i].LinkMan);
                        row3.CreateCell(9).SetCellValue(orderModels[i].Managers);
                        row3.CreateCell(10).SetCellValue(orderModels[i].Remark);

                        row3.GetCell(0).CellStyle = cellStyle4;
                        row3.GetCell(1).CellStyle = cellStyle4;
                        row3.GetCell(2).CellStyle = cellStyle4;
                        row3.GetCell(3).CellStyle = cellStyle4;
                        row3.GetCell(4).CellStyle = cellStyle4;
                        row3.GetCell(5).CellStyle = cellStyle4;
                        row3.GetCell(6).CellStyle = cellStyle4;
                        row3.GetCell(7).CellStyle = cellStyle4;
                        row3.GetCell(8).CellStyle = cellStyle4;
                        row3.GetCell(9).CellStyle = cellStyle4;
                        row3.GetCell(10).CellStyle = cellStyle3;

                        rowIndex++;
                    }
                }

                #endregion 第三行......循环加载数据

                #region 第cnt+2行

                var host = GlobalContext.Current.OwnerInfo;
                rowIndex = cnt + 2;
                var rowN = sheet1.CreateRow(rowIndex);
                rowN.Height = 20 * 140;
                sheet1.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 1));
                rowN.CreateCell(0).SetCellValue(host.ProfileModels.Where(m => m.Key == "host.AccountInfo").FirstOrDefault().Value.ToNoHTML());

                cellStyle7.WrapText = true;
                rowN.GetCell(0).CellStyle = cellStyle7;
                rowN.CreateCell(1).CellStyle = cellStyle7;
                rowN.CreateCell(2).CellStyle = cellStyle7;
                rowN.CreateCell(3).CellStyle = cellStyle7;
                rowN.CreateCell(4).CellStyle = cellStyle7;
                rowN.CreateCell(5).CellStyle = cellStyle7;
                rowN.CreateCell(6).CellStyle = cellStyle7;
                rowN.CreateCell(7).CellStyle = cellStyle7;
                rowN.CreateCell(8).CellStyle = cellStyle7;
                rowN.CreateCell(9).CellStyle = cellStyle7;
                rowN.CreateCell(10).CellStyle = cellStyle7;

                #endregion 第cnt+2行

                #region 列宽设置

                sheet1.SetColumnWidth(0, 18 * 256);
                sheet1.SetColumnWidth(1, 18 * 256);
                sheet1.SetColumnWidth(2, 18 * 256);
                sheet1.SetColumnWidth(3, 23 * 256);
                sheet1.SetColumnWidth(4, 28 * 256);
                sheet1.SetColumnWidth(5, 11 * 256);
                sheet1.SetColumnWidth(6, 8 * 256);
                sheet1.SetColumnWidth(7, 8 * 256);
                sheet1.SetColumnWidth(8, 18 * 256);
                sheet1.SetColumnWidth(9, 8 * 256);
                sheet1.SetColumnWidth(10, 27 * 256);

                #endregion 列宽设置

                workBook.Write(ms);
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode("财务对账单.xls"));
                Response.BinaryWrite(ms.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                workBook = null;
                ms.Close();
                ms.Dispose();
            }
            return Content("");
        }

        #endregion 导出

        #region 导入销账单

        /// <summary>
        /// 导入销账单
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 销账单格式为单列多行，内容为ordercode
        /// </remarks>
        public ActionResult ImportDuiZhangDan()
        {
            var file = Request.Files["file"];
            var vModel = new FinanceVModel();
            if (null != file && file.ContentLength > 0)
            {
                var workBook = new HSSFWorkbook(file.InputStream);
                ISheet sheet = workBook.GetSheet("Sheet1");

                //取行Excel的最大行数
                int rowsCount = sheet.PhysicalNumberOfRows;
                //int colsCount = sheet.GetRow(0).PhysicalNumberOfCells;

                var orderIds = new List<string>();
                for (int rowIndex = 1; rowIndex < rowsCount; rowIndex++)
                {
                    string sOrderId = sheet.GetRow(rowIndex).GetCell(0).ToString();
                    if (sOrderId.IsNullOrEmpty())
                        continue;
                    orderIds.Add(sOrderId);
                }
                orderIds = orderIds.Distinct().ToList();
                var orders = new OrderBiz().GetOrderById(orderIds, UserInfo);
                vModel.OrderModels.Items = orders;
                if (rowsCount - 1 != orders.Count)
                    ViewBag.TipMsg = "导入的部分订单可能由于以下原因被忽略：<br/>1.订单号重复；<br/>2.未找到对应订单";
            }

            return View("PayInBatch", vModel);
        }

        /// <summary>
        /// 验证收款总额
        /// </summary>
        /// <param name="orderCodes"></param>
        /// <returns></returns>
        public ActionResult PaymentBatchCheck(List<string> orderCodes)
        {
            if (orderCodes.Count > 0)
            {
                orderCodes = orderCodes.Distinct().ToList();
                var orders = new OrderBiz().GetOrder(orderCodes, UserInfo.OwnerCode);
                var totalYs = orders.Sum(p => p.TolYsPrice);
                var totalPaid = orders.Sum(p => p.TolPaid);
                var totalUnPaid = totalYs - totalPaid;
                var vModel = new PayInConfirmVModel
                {
                    OrderCount = orders.Count,
                    TotalYs = totalYs,
                    TotalPaid = totalPaid,
                    TotalUnPaid = totalUnPaid
                };
                return PartialView("UCImportPayInConfirm", vModel);
            }
            return PartialView("UCImportPayInConfirm", null);
        }

        /// <summary>
        /// 执行收款
        /// </summary>
        /// <param name="orderCodes"></param>
        //public string PaymentBatch(List<string> orderCodes)
        //{
        //    if (orderCodes.Count > 0)
        //    {
        //        //int result = new OrderBiz().PayInBath(orderCodes);
        //        orderCodes = orderCodes.Distinct().ToList();
        //        var orderBiz = new OrderBiz();
        //        var orders = orderBiz.GetOrder(orderCodes);
        //        var payInList = new List<TourPayInModel>();
        //        for (int i = 0; i < orders.Count; i++)
        //        {
        //            var payInInfo = new TourPayInModel();
        //            if (orders[i] != null)
        //                ChangeCostState(orders[i], payInInfo, orders[i].TolYsPrice - orders[i].TolPaid);
        //            payInList.Add(payInInfo);
        //        }
        //        orderBiz.PayIn(orders, payInList);
        //        return "success";
        //    }
        //    return "null";
        //}

        #endregion 导入销账单

        #region 页面初始化

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected override void InitPage()
        {
            // 产品部门
            ViewBag.ProductTeams = _teamBiz.GetOpTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            // 成团状态
            ViewBag.IsTourOks = new List<KeyValueBean>
                                    {
                                        new KeyValueBean{Key ="1",Value = "已成团"},
                                        new KeyValueBean{Key = "0",Value = "未成团"}
                                    }.ToSelectListFor();
            //团状态
            ViewBag.AuditStates = DictionaryTools.GetEnumsBy(Enums.AuditStateEnum).ToSelectListFor();
            //状态
            ViewBag.CostStatus = DictionaryTools.GetEnumsBy(Enums.CostStatusEnum).ToSelectListFor();
            // 平台列表
            if (UserInfo.AccountType == 1)
            {
                ViewBag.Platforms = new PlatformBiz().GetPlatforms();
            }
        }

        #endregion 页面初始化

        #region 自定义函数

        /// <summary>
        /// 根据商户名称 模糊查询 对应的商户Code
        /// </summary>
        /// <param name="customerName"></param>
        /// <returns></returns>
        private string GetCustomerCodes(string customerName)
        {
            var strTemp = "0";
            var customerModels = new List<CrmCustomerModel>();
            customerModels = _customerBiz.GetCustomerCodeByName(customerName, UserInfo.OwnerCode);
            if (customerModels.Count > 0)
            {
                strTemp = "";
                foreach (var crmCustomerModel in customerModels)
                {
                    strTemp += crmCustomerModel.Code + ",";
                }
                strTemp = strTemp.Substring(0, strTemp.Length - 1);
            }
            return strTemp;
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
        /// 分销商 查询字串
        /// </summary>
        /// <param name="bookingCustomer"></param>
        /// <returns></returns>
        private string CreateBookingCustomerQueryString(string bookingCustomer)
        {
            string strTemp = string.Empty;
            if (!bookingCustomer.IsNullOrEmpty())
            {
                var customerModels = new List<CrmCustomerModel>();
                customerModels = new CustomerBiz().GetCustomers(bookingCustomer);
                if (customerModels.Count > 0)
                {
                    foreach (var crmCustomerModel in customerModels)
                    {
                        strTemp += crmCustomerModel.Code + ",";
                    }
                    strTemp = strTemp.Substring(0, strTemp.Length - 1);
                }
            }
            return strTemp;
        }

        #endregion 自定义函数
    }
}