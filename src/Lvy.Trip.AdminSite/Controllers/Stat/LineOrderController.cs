using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.VModels.Excel;
using Lvy.VModels.Finance;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Finance
{

    /// <summary>
    /// 线路产品预订统计
    /// </summary>
    public partial class StatController : BaseController
    {
        private TeamBiz _teamBiz = new TeamBiz();
        private CustomerBiz _customerBiz = new CustomerBiz();
        private FinanceBiz _financeBiz = new FinanceBiz();
        private TpLineBiz _lineBiz = new TpLineBiz();
        private DictionaryBiz _commonBiz = new DictionaryBiz();

        /// <summary>
        /// 订单统计
        /// </summary>
        /// <param name="financeVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchBookAccount(FinanceVModel financeVModel)
        {
            InitPage();

            //报名客户查询条件
            if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
                financeVModel.Condition.BookingCustomer = CreateBookingCustomerQueryString(financeVModel.Condition.BookingCustomer);

            //根据 产品类型 查找对应的 LineId
            //if (!financeVModel.Condition.LineType.IsNullOrEmpty())
            //    financeVModel.Condition.LineType = GetLineIdsByLineLineType(financeVModel.Condition.LineType);

            var SalesTeams = new List<SelectListItem>();
            var OpTeams = new List<SelectListItem>();
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                financeVModel.IsSalerBoss = true;
                SalesTeams = _teamBiz.GetSalesTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                financeVModel.IsSalerLeader = true;

                SalesTeams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(financeVModel.Condition.SaleTeamId))
                {
                    financeVModel.Condition.SaleTeamId = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
                if (!string.IsNullOrEmpty(financeVModel.Condition.SaleTeamId))
                {
                    ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(financeVModel.Condition.SaleTeamId, UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                financeVModel.IsSaler = true;
                //销售
                SalesTeams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(financeVModel.Condition.SaleTeamId))
                {
                    financeVModel.Condition.SaleTeamId = SalesTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
                if (!string.IsNullOrEmpty(financeVModel.Condition.SaleTeamId))
                {
                    ViewBag.Salers = _customerBiz.GetTeamUsersByTeamId(financeVModel.Condition.SaleTeamId, UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                }
            }
            else
            {
                // 不是销售
                SalesTeams = _teamBiz.GetSalesTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                OpTeams = _teamBiz.GetOpTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                OpTeams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(financeVModel.Condition.CrmTeamId) && OpTeams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值 ！不是总监不能为空
                {
                    financeVModel.Condition.CrmTeamId = OpTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }
            else
            {
                // 不是OP
                OpTeams = _teamBiz.GetOpTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            ViewBag.AccountTeamBeans = OpTeams;
            ViewBag.SalesOfTeam = SalesTeams;

            //获取订单列表信息
            financeVModel.OrderModels = _financeBiz.GetOrderStatistic(financeVModel, UserInfo, true);

            #region 获取列表汇总信息

            var summary = _financeBiz.GetStatisticSummary(financeVModel, UserInfo);
            financeVModel.SumPriceCount = summary.SumPriceCount;
            financeVModel.SumTolPaid = summary.SumTolPaid;
            financeVModel.ShengYuCount = summary.ShengYuCount;
            financeVModel.SumTravellerCount = summary.SumTravellerCount;

            #endregion 获取列表汇总信息

            if (Request.IsAjaxRequest())
                return PartialView("LineOrder/UCBAccountSearch", financeVModel);
            return View("LineOrder/SearchBookAccount", financeVModel);
        }

        #region 导出

        /// <summary>
        /// 导出预定统计
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public FileContentResult DownloadBookingOrder(FinanceVModel vModel)
        {
            ////根据 分销商 模糊查找对应的 CustomerCode
            //if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
            //    vModel.Condition.BookingCustomer = GetCustomerCodes(vModel.Condition.BookingCustomer);
            //报名客户查询条件
            if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
                vModel.Condition.BookingCustomer = CreateBookingCustomerQueryString(vModel.Condition.BookingCustomer);

            //根据 产品类型 查找对应的 LineId
            if (!vModel.Condition.LineType.IsNullOrEmpty())
                vModel.Condition.LineType = GetLineIdsByLineLineType(vModel.Condition.LineType);
            var orders = _financeBiz.GetOrderStatistic(vModel, UserInfo, false).Items;
            if (null == orders || orders.Count == 0)
                return null;

            orders = orders.Where(a => a.IsCancel != 1)
                .OrderBy(a => a.OutDate)
                .ThenBy(a => a.TourId)
                .ToList();

            var trvBiz = new TravellerBiz();
            var priceBiz = new TpPriceBiz();
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

        /// <summary>
        /// 导出客户对账单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult ExportCustomerZhangdan(FinanceVModel vModel)
        {
            var customer = DictionaryTools.GetCachedCustomerDict().FirstOrDefault(a => a.Value.Code == vModel.Condition.BookingCustomer && a.Value.IsValid == 1).Value;

            if (customer == null)
            {
                throw new Exception("没有找到分销商！");
            }

            var host = GlobalContext.Current.OwnerInfo;

            ////根据 分销商 模糊查找对应的 CustomerCode
            //if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
            //    vModel.Condition.BookingCustomer = GetCustomerCodes(vModel.Condition.BookingCustomer);
            //报名客户查询条件
            if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
                vModel.Condition.BookingCustomer = CreateBookingCustomerQueryString(vModel.Condition.BookingCustomer);
            //根据 产品类型 查找对应的 LineId
            if (!vModel.Condition.LineType.IsNullOrEmpty())
                vModel.Condition.LineType = GetLineIdsByLineLineType(vModel.Condition.LineType);
            var orders = _financeBiz.GetOrderStatistic(vModel, UserInfo, false).Items;
            if (null == orders || orders.Count == 0)
                return null;

            orders = orders.Where(a => a.IsCancel != 1)
                             .OrderBy(a => a.OutDate)
                             .ThenBy(a => a.TourId)
                             .ToList();

            var trvBiz = new TravellerBiz();
            var priceBiz = new TpPriceBiz();
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
                         select new CustomerZhangdanExcelVModel()
                         {
                             OrderCode = o.Id.ToString(),
                             JoinOrderCode = o.JoinOrderCode,
                             TourName = o.TourId.ToString() + "-" + o.LineName,
                             OutDate = o.OutDate.ToDateFormat(),
                             LinkMan = o.LinkMan,
                             Managers = o.Managers,
                             TravellerCount = o.TravellerCount,
                             TolYsPrice = o.TolYsPrice,
                             TolPaid = o.TolPaid,
                             ZiFei = ExportUtils.GetZifei(o.OrderCode, tourists),
                             SingleRoom = ExportUtils.GetSingleRoom(o.OrderCode, tourists),
                             JsPrice = tourists.Where(b => b.OrderCode == o.OrderCode && b.State == 2).Sum(b => b.JiePrice + b.SongPrice),
                             PriceContents = ExportUtils.GetPriceContents(o, tourists, tourPirces),
                             Remark = o.Remark
                         }).ToList();

            using (var ms = new MemoryStream())
            {
                var workBook = new HSSFWorkbook();
                // 新增試算表。
                var sheet1 = workBook.CreateSheet("{0}对账单".With(GlobalContext.Current.OwnerInfo.Name));

                string fileName = "{0}账单".With(customer.Name);
                Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 15, fileName);
                Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1, true);

                Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);
                Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);

                row.CreateCell(4).SetCellValue(datas.Sum(a => a.TravellerCount));
                row.CreateCell(10).SetCellValue((double)datas.Sum(a => a.TolYsPrice));
                row.CreateCell(11).SetCellValue((double)datas.Sum(a => a.TolPaid));
                row.CreateCell(12).SetCellValue((double)datas.Sum(a => a.TolYsPrice - a.TolPaid));

                // 自适应宽度
                Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1);

                var cellStyle = Arch.Common.Toolkit.Npoi.TitleStyle(workBook, false);

                row = sheet1.CreateRow(sheet1.LastRowNum + 2);
                Arch.Common.Toolkit.Npoi.SetMergedRegion(sheet1, sheet1.LastRowNum, sheet1.LastRowNum, 0, 10);
                row.GetCell(0).SetCellValue("分销商 : " + customer.Name);
                sheet1.GetRow(sheet1.LastRowNum).GetCell(0).CellStyle = cellStyle;
                row.HeightInPoints = 20;

                row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                Arch.Common.Toolkit.Npoi.SetMergedRegion(sheet1, sheet1.LastRowNum, sheet1.LastRowNum, 0, 10);
                row.GetCell(0).SetCellValue("公司地址 : " + customer.Address);
                sheet1.GetRow(sheet1.LastRowNum).GetCell(0).CellStyle = cellStyle;
                row.HeightInPoints = 20;

                row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                Arch.Common.Toolkit.Npoi.SetMergedRegion(sheet1, sheet1.LastRowNum, sheet1.LastRowNum, 0, 10);
                row.GetCell(0).SetCellValue("联系电话 : " + customer.Phone);
                sheet1.GetRow(sheet1.LastRowNum).GetCell(0).CellStyle = cellStyle;
                row.HeightInPoints = 20;

                row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                Arch.Common.Toolkit.Npoi.SetMergedRegion(sheet1, sheet1.LastRowNum, sheet1.LastRowNum, 0, 10);
                row.GetCell(0).SetCellValue("银行账号 : " + host.ProfileModels.Where(m => m.Key == "host.AccountInfo").FirstOrDefault().Value.ToNoHTML());
                sheet1.GetRow(sheet1.LastRowNum).GetCell(0).CellStyle = cellStyle;
                row.HeightInPoints = 70;

                workBook.Write(ms);
                return File(ms.GetBuffer(), "application/vnd.ms-excel", HttpUtility.UrlEncode("{0}对账单.xls".With(GlobalContext.Current.OwnerInfo.Name)));
            }
        }

        #endregion 导出

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
    }
}