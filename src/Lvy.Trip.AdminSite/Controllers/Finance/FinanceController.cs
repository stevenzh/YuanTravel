using Arch.Common.Utils;
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
using Lvy.Trip.Biz.Ticket;
using Lvy.Visa.Biz;
using Lvy.VModels.Finance;
using Lvy.VModels.Tour;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Finance
{
    /// <summary>
    /// 财务收款
    /// </summary>
    public partial class FinanceController : BaseController
    {
        private readonly FinanceBiz _financeBiz = new FinanceBiz();
        private readonly PlatformBiz _platformBiz = new PlatformBiz();
        private readonly TpLineBiz _lineBiz = new TpLineBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly OrderBiz _orderBiz = new OrderBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly TpTourPlanBiz _tourPlanBiz = new TpTourPlanBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly TourBalanceBiz _balanceBiz = new TourBalanceBiz();
        private readonly TpOrderPayInBiz _payinBiz = new TpOrderPayInBiz();
        private readonly VisaOrderBiz _visaOrderBiz = new VisaOrderBiz();
        private readonly TktOrderBiz tktOrderBiz = new TktOrderBiz();

        #region 财务收款 无需缴款单

        /// <summary>
        /// 账务收款-视图
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchFinance(FinanceVModel financeVModel)
        {
            InitPage();

            ////根据 分销商 模糊查找对应的 CustomerCode
            //if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
            //    financeVModel.Condition.BookingCustomer = GetCustomerCodes(financeVModel.Condition.BookingCustomer);

            //报名客户查询条件
            if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
                financeVModel.Condition.BookingCustomer = CreateBookingCustomerQueryString(financeVModel.Condition.BookingCustomer);

            //根据 产品类型 查找对应的 LineId
            if (!financeVModel.Condition.LineType.IsNullOrEmpty())
            {
                financeVModel.Condition.LineType = GetLineIdsByLineLineType(financeVModel.Condition.LineType);
            }
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                financeVModel.IsSalerBoss = true;
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                financeVModel.IsSalerLeader = true;
                financeVModel.Condition.SaleTeamId = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).FirstOrDefault().TeamID;
                ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(financeVModel.Condition.SaleTeamId, UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                financeVModel.IsSaler = true;
                financeVModel.Condition.SaleTeamId = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).FirstOrDefault().TeamID;
                financeVModel.Condition.SalerCode = GlobalContext.Current.UserInfo.Code;
                //销售
                ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(financeVModel.Condition.SaleTeamId, UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }
            //获取订单列表信息
            financeVModel.OrderModels = _financeBiz.GetPageList(financeVModel, UserInfo);

            #region 获取列表汇总信息

            var summary = _financeBiz.GetFinanceSummary(financeVModel, UserInfo);
            financeVModel.SumPriceCount = summary.SumPriceCount;
            financeVModel.SumTolPaid = summary.SumTolPaid;
            financeVModel.ShengYuCount = summary.ShengYuCount;
            financeVModel.SumTravellerCount = summary.SumTravellerCount;

            #endregion 获取列表汇总信息

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", financeVModel);
            return View(financeVModel);
        }

        /// <summary>
        /// 账务收款编辑-视图
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult EdtiFinance(string orderCode)
        {
            return RedirectToAction("EditOrder", "Order", new { orderCode = orderCode, isCaiWu = 1 });
        }

        /// <summary>
        /// 批量收款
        /// </summary>
        /// <param name="chkOpOrderCode"></param>
        /// <returns></returns>
        public ActionResult BatchOrderFinance(string chkOpOrderCode)
        {
            var orders = _orderBiz.GetOrder(chkOpOrderCode.Split('|'), OwnerCode);
            for (int i = 0; i < orders.Count; i++)
            {
                var payInInfo = new TpOrderPayInModel();
                if (orders[i] != null)
                    ChangeOrderState(orders[i], payInInfo, orders[i].TolYsPrice - orders[i].TolPaid);

                _orderBiz.ShowKuan(orders[i], payInInfo);
            }

            //var orderModel = new TpOrderModel();
            //string[] chks = chkOpOrderCode.Split('|');
            //foreach (var str in chks)
            //{
            //    OrderTolPaid(orderModel, str);
            //}

            return Content("1");
        }

        /// <summary>
        /// 更新Order对象的状态、收款等
        /// </summary>
        /// <param name="order"></param>
        /// <param name="payInInfo"></param>
        /// <param name="payment"></param>
        public void ChangeOrderState(TpOrderModel order, TpOrderPayInModel payInInfo, decimal payment)
        {
            //if (payment <= 0)
            //    throw new ArgumentNullException("payment");
            decimal unPaid = order.TolYsPrice - order.TolPaid;
            if (Math.Abs(payment) > Math.Abs(unPaid))
            {
                throw new Exception("本次收款大于未收款项");
            }

            var currentDateTime = DateTime.Now;
            var currentUserCode = GlobalContext.Current.UserInfo.Code;

            payInInfo.OrderCode = order.OrderCode;
            payInInfo.CustomerCode = order.BookingCustomer;
            payInInfo.Amount = payment;
            payInInfo.PayInBy = currentUserCode;
            payInInfo.PayInTime = currentDateTime;
            payInInfo.AuditBy = currentUserCode;
            payInInfo.AuditTime = currentDateTime;

            if (payment == unPaid)
            {
                //完成收款
                order.TolPaid = order.TolYsPrice;
                order.JieSuanState = 5;//已结算
            }
            else
            {
                //部分完成
                order.TolPaid = order.TolPaid + payment;
                order.JieSuanState = 4;//部分结算
            }
        }

        /// <summary>
        /// 收款确认
        /// </summary>
        /// <param name="chkOpOrderCode"></param>
        /// <returns></returns>
        public ActionResult MultiOrderShowKuanCheck(string chkOpOrderCode)
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
                return PartialView("UCMultiPayInConfirm", vModel);
            }
            return PartialView("UCMultiPayInConfirm", null);
        }

        /// <summary>
        /// 单项收款
        /// </summary>
        /// <returns></returns>
        public ActionResult OrdersTolPaid(string orderCode, decimal currentPayment)
        {
            var orderBiz = new OrderBiz();
            var order = orderBiz.GetOrderLineTourist(orderCode);
            var payInInfo = new TpOrderPayInModel();
            ChangeOrderState(order, payInInfo, currentPayment);
            new TpOrderPayInBiz().AddPayIn(payInInfo);
            return Content(orderBiz.Update(order).ToString());
        }

        /// <summary>
        /// 收款确认
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult SingleOrderPayInCheck(string orderCode)
        {
            var order = new OrderBiz().GetOrderLineTourist(orderCode);
            var vModel = new PayInConfirmVModel
            {
                OrderId = order.Id.ToString(),
                TotalYs = order.TolYsPrice,
                TotalPaid = order.TolPaid,
                TotalUnPaid = order.TolYsPrice - order.TolPaid,
                CurrentPayment = order.TolYsPrice - order.TolPaid
            };
            return PartialView("UCSinglePayInConfirm", vModel);
        }

        #endregion 财务收款

        #region 导出对账单

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
                rowN.CreateCell(0).SetCellValue(host.ProfileModels.Where(m =>m.Key== "host.AccountInfo").First().Value.ToNoHTML());

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
        public ActionResult PayInBatchCheck(List<string> orderCodes)
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
        //public string PayInBatch(List<string> orderCodes)
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
        //                ChangeOrderState(orders[i], payInInfo, orders[i].TolYsPrice - orders[i].TolPaid);
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
            ViewBag.ProductTeams = _teamBiz.GetOpTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            // 产品类型
            ViewBag.LineTypes = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum).ToSelectListFor();
            //订单状态分类
            ViewBag.OrderStates = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor();
            //结算状态
            ViewBag.SettlementStateBean = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key = "1",Value = "已结算"},
                                         new KeyValueBean{Key="0",Value="未结算"}
                                     }.ToSelectListFor();
            ViewBag.OrderSourceItems = DictionaryTools.GetEnumsBy(Enums.TourSourceEnum).ToSelectListFor();
            //所有订单状态
            ViewBag.AllOrderStates = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor();

            ViewBag.Salers = _customerBiz.GetTeamSales(UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
        }

        #endregion 页面初始化

        #region 自定义函数

        /// <summary>
        /// 收款
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        //private int OrderTolPaid(TpOrderModel orderModel, string orderCode)
        //{
        //    orderModel = _orderBiz.GetOrderLineTourist(orderCode);
        //    orderModel.OrderState = 10;
        //    orderModel.TolPaid = orderModel.TolYsPrice;
        //    int result = _orderBiz.Update(orderModel);
        //    return result;
        //}

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
        /// 分销商查询字串
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

        #region 单团核算

        [LvyAuth]
        public ActionResult SearchBalance(TourBalanceVModel vModel)
        {
            InitPage2();
            //页面第一次加载时设置条件初始值
            vModel.Balances = _balanceBiz.GetPageList(vModel, UserInfo.OwnerCode);
            vModel.SumCost = _balanceBiz.Summey(vModel, UserInfo.OwnerCode);
            if (Request.IsAjaxRequest())
                return PartialView("TourBalance/UCSearchBalance", vModel);

            return View("TourBalance/SearchBalance", vModel);
        }

        protected void InitPage2()
        {
            // 线路类型
            ViewBag.ProductTypes = DictionaryTools.GetEnumsBy(Enums.ProductTypeEnum).ToSelectListFor("", "", "-选择团单来源-");
            ViewBag.ProductAllTypes = DictionaryTools.GetEnumsBy(Enums.ProductAllTypeEnum).ToSelectListFor();
            //结算状态
            ViewBag.SettlementStateBean = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key = "1",Value = "已结算"},
                                         new KeyValueBean{Key="0",Value="未结算"}
                                     }.ToSelectListFor();
            ViewBag.Teams = _teamBiz.GetBalanceTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, t => t.TeamName, "", "", "-选择部门-");
            //取得门店列表
            ViewBag.Branchs = _customerBiz.GetAllBranch().ToSelectListFor(t => t.Code, t => t.Name);
            ViewData["AuditState"] = DictionaryTools.GetEnumsBy(Enums.AuditStateEnum).ToSelectListFor("", "", "-团单状态-");
            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();
        }

        /// <summary>
        /// 核算明细-视图
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="IsCopy">false-OP true-CW</param>
        /// <returns></returns>
        public ActionResult EditTourBalance(string orderCode, bool IsCopy = false)
        {
            //if (IsCopy == true) // 财务版
            //{
            string ownerCode = GlobalContext.Current.OwnerCode;
            ViewBag.Suppliers = new CustomerBiz().GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name,
                Help1 = DictionaryTools.GetEnumValue(Enums.PaymentTypeEnum, a.PaymentType.ToString()),
                Help2 = a.PaymentType.ToString()
            });
            var model = UpdateTourBalance(orderCode, IsCopy);
            model.IsCopy = IsCopy;
            if (model.Balance == null)
                return new Lvy.Web.Common.Mvc.Results.NotFoundResult();
            return View(model);
            //}
            //else   // OP 版
            //{
            //    return RedirectToAction("ShowTourBalance", new { orderCode = orderCode });
            //}
        }

        /// <summary>
        /// 单团 只读模式
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult ShowTourBalance(string orderCode)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            UpdateTourBalance(orderCode, vModel);
            string ownerCode = GlobalContext.Current.OwnerCode;
            ViewBag.Suppliers = new CustomerBiz().GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name,
                Help1 = DictionaryTools.GetEnumValue(Enums.PaymentTypeEnum, a.PaymentType.ToString())
            });

            return View(vModel);
        }

        public ActionResult PrintTourBalance(string orderCode)
        {
            var qModel = UpdateTourBalance(orderCode, false);
            TempData["OrderConfirmPrintVModel"] = qModel;
            if (qModel.Balance.IsPackage == 2)
                return View("/Views/OpTour/TourBalance/PrintPackageBalance.aspx", qModel);
            else if (qModel.Balance.Type == 1)
                return View("/Views/OpTour/TourBalance/PrintTourBalance.aspx", qModel);
            else
                return View("/Views/OpTour/TourBalance/PrintCommonBalance.aspx", qModel);
        }

        /// <summary>
        /// 财务审核 复制记录
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        [LvyAuth]
        public string FinanceAuditTour(string orderCode)
        {
            var tourBalance = _balanceBiz.GetBalanceByOrderCode(orderCode);  //获取单团
            if (tourBalance.Type == 1)
            {
                var tourId = tourBalance.TourId.Value;
                var tour = _tourPlanBiz.GetTourById(tourId);
                tour.AuditState = 4;
                _tourPlanBiz.UpdateTourPlan(tour);
            }

            tourBalance.CWAuditBy = GlobalContext.Current.UserInfo.Code;
            tourBalance.CWAuditTime = DateTime.Now;
            _balanceBiz.UpdateBalance(tourBalance);
            _balanceBiz.CopyTourBalance(tourBalance.MasterOrderCode);

            return "1";
        }

        /// <summary>
        /// 添加一条成本
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult AddRowCost(int rowIndex, string orderCode)
        {
            ViewBag.RowIndex = rowIndex;
            TpTourCostModel vModel = new TpTourCostModel();
            vModel.MasterOrderCode = orderCode;

            //var plan = _tourPlanBiz.GetTourAndLine(tourId);
            string ownerCode = GlobalContext.Current.OwnerCode;

            ViewBag.Suppliers = _customerBiz.GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name,
                Help1 = DictionaryTools.GetEnumValue(Enums.PaymentTypeEnum, a.PaymentType.ToString()),
                Help2 = a.PaymentType.ToString()
            }).ToList();
            return PartialView("TourBalance/UCRowCost", vModel);
        }

        /// <summary>
        /// 保存单团核算
        /// </summary>
        public ActionResult SaveTourBalance(TourBalanceVModel vModel)
        {
            var masterOrderCode = vModel.Balance.MasterOrderCode;
            var tourBalance = _balanceBiz.GetBalanceByOrderCode(masterOrderCode);  //获取单团

            if (tourBalance.Type == 1)
            {
                var tourId = tourBalance.TourId.Value;
                //var plan = _tourPlanBiz.GetTourById(tourId);
                if (tourBalance.AuditState > 3) // OP提交财务后，不允许修改单团
                    return Json(new { Code = "2", Message = "OP提交财务，不允许修改单团!" });

                var orders = _orderBiz.GetValidOrderByTourId(tourId); // 获取有效订单
                tourBalance.YiShou = orders.Sum(a => a.TolPaid);
            }

            tourBalance.TotalCost = vModel.CostList.Sum(t => t.ItemCost);
            tourBalance.MaoLi = tourBalance.YingShou - tourBalance.TotalCost;
            _balanceBiz.UpdateBalance(tourBalance);

            // 更新成本
            var Costs = _balanceBiz.GetCostsByOrderCode(tourBalance.MasterOrderCode);
            foreach (var costRule in Costs)
            {
                // 更新数据和添加
                var d = vModel.CostList.Where(t => t.Id == costRule.Id).FirstOrDefault();
                if (d != null)
                {
                    costRule.Item = d.Item;
                    costRule.Cost = d.Cost;
                    costRule.Currency = d.Currency;
                    costRule.ROE = d.ROE;
                    costRule.Num = d.Num;
                    costRule.ItemCost = d.ItemCost;
                    costRule.Remark = d.Remark;
                    costRule.PaymentType = d.PaymentType;
                    costRule.SupplierId = d.SupplierId;
                    costRule.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                    costRule.ModifiedTime = DateTime.Now;
                }
                else
                {
                    costRule.IsValid = 0;
                }
                _balanceBiz.UpdateCost(costRule);
            }

            // 添加新的
            foreach (var d in vModel.CostList.Where(t => t.Id == default(int)))
            {
                TpTourCostModel tourCost = new TpTourCostModel();
                tourCost.MasterOrderCode = masterOrderCode;
                tourCost.Code = DBTools.GetSeqNo("TourCost");
                tourCost.SupplierId = d.SupplierId;
                tourCost.Item = d.Item;
                tourCost.Cost = d.Cost;
                tourCost.Currency = d.Currency;
                tourCost.ROE = d.ROE;
                tourCost.Num = d.Num;
                tourCost.ItemCost = d.ItemCost;
                tourCost.Remark = d.Remark;
                tourCost.PaymentType = d.PaymentType;
                tourCost.IsValid = 1;
                tourCost.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                tourCost.ModifiedTime = DateTime.Now;
                tourCost.IsCopy = false;
                _balanceBiz.SaveCost(tourCost);
            }
            return Json(new { Code = "1", Message = "成功" });
        }

        /// <summary>
        /// 取得单团核算信息 填充MODEL
        /// 数据库为空，更新数据库
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="vModel"></param>
        private void UpdateTourBalance(string orderCode, TourBalanceVModel vModel)
        {
            var model = _balanceBiz.GetBalanceByOrderCode(orderCode);  //获取单团
            vModel.CostList = _balanceBiz.GetCostsByOrderCode(model.MasterOrderCode);
            model.TotalCost = vModel.CostList.Sum(t => t.ItemCost);

            if (model.Type == 1)
            {
                var tourId = model.TourId.Value;
                //var quota = new TpTourQuotaMapBiz().GetMapWithQuota(tourId);

                // vmodel对象复制
                vModel.Tour = _tourPlanBiz.GetTourById(tourId);
                //vModel.Line = _lineBiz.GetLineById(vModel.Tour.LineId);
                vModel.Orders = _orderBiz.GetValidCommonOrderByTourId(tourId); // 获取有效订单

                //model.GuideName = "";
                model.Num = vModel.Tour.TravellerCount;
                model.YingShou = vModel.Orders.Sum(a => a.TolYsPrice);
                model.YiShou = vModel.Orders.Sum(a => a.TolPaid);
                model.MaoLi = model.YingShou - model.TotalCost;
            }
            else if (model.Type == 3)
            {
                // 签证订单
                vModel.Orders = _visaOrderBiz.GetCommonOrderByCode(orderCode);

                // 子订单

            }
            else if (model.Type == 9)
            {
                // 门票订单
                vModel.Orders = tktOrderBiz.GetCommonOrderByCode(orderCode);

                // 子订单
            }

            model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            model.ModifiedTime = DateTime.Now;

            //update balance
            _balanceBiz.UpdateBalance(model);
            // get view model
            vModel.Balance = model;

            // sum
            vModel.SumCost = new FinanceTotalModel();
            vModel.SumCost.XianShou = vModel.CostModels.Where(a => a.PaymentType == 1).Sum(a => a.ItemCost);
            vModel.SumCost.Qiandan = vModel.CostModels.Where(a => a.PaymentType != 1).Sum(a => a.ItemCost);
            vModel.SumCost.SumTolCost = vModel.SumCost.XianShou + vModel.SumCost.Qiandan;
        }

        /// <summary>
        /// 打印单团核算-视图
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        //public ActionResult PrintTourBalance(string orderCode, bool isCopy = false)
        //{
        //    return View("TourBalance/PrintTourBalance", UpdateTourBalance(orderCode, isCopy));
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="IsCopy">True-CW  false-OP</param>
        /// <returns></returns>
        private TourBalanceVModel UpdateTourBalance(string orderCode, bool IsCopy)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            vModel.Balance = _balanceBiz.GetBalanceByOrderCode(orderCode, IsCopy);  //获取单团
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);

            if (vModel.Balance.Type == 1)
            {
                int tourId = vModel.Balance.TourId.Value;
                vModel.Tour = _tourPlanBiz.GetTourById(tourId);
                vModel.Line = _lineBiz.GetLineById(vModel.Tour.LineId);
                vModel.Orders = _orderBiz.GetValidCommonOrderByTourId(tourId);  // 获取有效订单
            }
            else if (vModel.Balance.Type == 3)
            {
                // 签证订单
                vModel.Orders = _visaOrderBiz.GetCommonOrderByCode(orderCode);

                // 子订单
            }
            else if (vModel.Balance.Type == 9)
            {
                // 门票订单
                vModel.Orders = tktOrderBiz.GetCommonOrderByCode(orderCode);

                // 子订单
            }
            vModel.CostList = _balanceBiz.GetCostsByOrderCode(vModel.Balance.MasterOrderCode, IsCopy);

            // sum
            vModel.SumCost = new FinanceTotalModel();
            vModel.SumCost.XianShou = vModel.CostModels.Where(a => a.PaymentType == 1).Sum(a => a.ItemCost);
            vModel.SumCost.Qiandan = vModel.CostModels.Where(a => a.PaymentType != 1).Sum(a => a.ItemCost);
            vModel.SumCost.SumTolCost = vModel.SumCost.XianShou + vModel.SumCost.Qiandan;

            return vModel;
        }


        public ActionResult ReLoadPayIn(string orderCode)
        {
            var vModel = new TourBalanceVModel();
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);
            vModel.Balance.MasterOrderCode = orderCode;
            return PartialView("TourBalance/UCPayIn", vModel);
        }

        #endregion 单团核算
    }
}