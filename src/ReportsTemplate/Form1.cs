using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Arch.Common;
using Lvy.Trip.Biz.Product;
using Lvy.VModels.Order;
using Lvy.Trip.Biz.Crm;
using Microsoft.Reporting.WinForms;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Site;
using Lvy.VModels.Tour;
using Lvy.Trip.Biz.Finance;
using Lvy.Visa.Biz;
using Lvy.Trip.Biz.Ticket;

namespace ReportsTemplate
{
    public partial class Form1 : Form
    {

        private OrderBiz _biz = new OrderBiz();
        private TravellerBiz _travellerBiz = new TravellerBiz();
        private TpLineRouteBiz lineRouteBiz = new TpLineRouteBiz();
        private TpLineTourPlanBiz tourPlanBiz = new TpLineTourPlanBiz();
        private TpLineBiz _lineBiz = new TpLineBiz();
        private TpTourPlanBiz _planBiz = new TpTourPlanBiz();
        private TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private CustomerBiz _customerBiz = new CustomerBiz();
        private TpOrderPayInBiz _payinBiz = new TpOrderPayInBiz();
        private readonly TourBalanceBiz _balanceBiz = new TourBalanceBiz();
        private readonly VisaOrderBiz _visaOrderBiz = new VisaOrderBiz();
        private readonly TktOrderBiz tktOrderBiz = new TktOrderBiz();
        private readonly OrderBiz _orderBiz = new OrderBiz();
        private TpChildOrderBiz _childBiz = new TpChildOrderBiz();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PrintBalance();
        }
        private void PrintBalance()
        {
            TourBalanceVModel model = UpdateTourBalance("2009000001", false);

            // 团号
            ReportParameter para1 = new ReportParameter("TourNo", model.Balance.TourNo);
            // 线路名称
            ReportParameter para2 = new ReportParameter("LineName", model.Balance.ProductName);
            // 毛利率
            ReportParameter para3 = new ReportParameter("OutDate", model.Balance.OutDate.Value.ToString("yyyy-MM-dd"));
            // 天数
            ReportParameter para4 = new ReportParameter("DayNum", model.Balance.TravelDays.ToString());
            // 游客人数
            ReportParameter para5 = new ReportParameter("TouristCount", model.Balance.Num.ToString());
            // 导游蚺属
            ReportParameter para6 = new ReportParameter("LeaderCount", "1");
            // 导游名称
            ReportParameter para7 = new ReportParameter("LeaderName", "导游");
            // 成人人数
            ReportParameter para8 = new ReportParameter("AuditPax", model.Balance.AuditPax.ToString());
            // 儿童人数
            ReportParameter para9 = new ReportParameter("ChildPax", model.Balance.ChildPax.ToString());
            // 老人人数
            ReportParameter para10 = new ReportParameter("OldPax", model.Balance.OldPax.ToString());

            // 收入
            ReportParameter para11 = new ReportParameter("TotalPrice", model.Balance.YingShou.ToString());
            // 已收
            ReportParameter para12 = new ReportParameter("YiShou", model.Balance.YiShou.ToString());
            // 成本
            ReportParameter para13 = new ReportParameter("TotalCost", model.Balance.TotalCost.ToString());
            // 毛利
            ReportParameter para14 = new ReportParameter("MaoLi", model.Balance.MaoLi.ToString());
            //
            ReportParameter para15 = new ReportParameter("GrossProfit", model.Balance.MaoLi.ToString());

            //// 审核人
            //ReportParameter para15 = new ReportParameter("CWAuditBy", model.Balance.CWAuditBy);
            //// 审核时间
            //ReportParameter para16 = new ReportParameter("CWAuditTime", model.Balance.CWAuditTime.ToString());
            //// 制单人
            //ReportParameter para17 = new ReportParameter("CreatedBy", model.Balance.CreatedBy);



            this.reportViewer1.LocalReport.SetParameters(new ReportParameter[] {para1, para2, para3, para4, para5, para6, para7,
                    para8, para9, para10, para11, para12, para13, para14, para15 });
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CostDataSet", model.CostList));
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("OrderDataSet", model.Orders));
            this.reportViewer1.RefreshReport();
        }

        private TourBalanceVModel UpdateTourBalance(string orderCode, bool IsCopy)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            vModel.Balance = _balanceBiz.GetBalanceByOrderCode(orderCode, IsCopy);  //获取单团
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);

            if (vModel.Balance.Type == 1)
            {
                int tourId = vModel.Balance.TourId.Value;
                vModel.Tour = _planBiz.GetTourById(tourId);
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
        public void PrintBill()
        {

            OrderPayInVModel model = new OrderPayInVModel();
            model.PayInModel = _payinBiz.GetById(4);
            model.OrderModel = _biz.GetOrderByOrderCode(model.PayInModel.OrderCode);
            model.OrderFiles = _biz.GetOrderFileList(model.PayInModel.OrderCode);

            // 缴款客户名称
            ReportParameter para1 = new ReportParameter("CustomerName", model.OrderModel.CustomerName);
            // 汇款人
            ReportParameter para2 = new ReportParameter("Remitter", model.PayInModel.Remitter);
            // 税号
            ReportParameter para3 = new ReportParameter("TaxNumber", model.PayInModel.TaxNumber);
            // 线路名称
            ReportParameter para4 = new ReportParameter("LineName", model.OrderModel.LineName);
            // 团队人数
            ReportParameter para5 = new ReportParameter("Pax", model.OrderModel.TravellerCount.ToString());
            // 金额
            ReportParameter para6 = new ReportParameter("Amount", model.PayInModel.Amount.ToString());
            // 支票号
            ReportParameter para7 = new ReportParameter("JoinNo", model.PayInModel.JoinNo);
            // 支票金额
            ReportParameter para8 = new ReportParameter("PaymentType", model.PayInModel.PaymentType.ToString());
            // 备注
            ReportParameter para9 = new ReportParameter("Remark", model.PayInModel.Remark);
            // 加款
            ReportParameter para10 = new ReportParameter("AddAmount", model.PayInModel.AddAmount.ToString());

            string picroot = AppSetting.Get("UploadFileRoot");
            var pingzheng = model.OrderFiles.Where(m => m.SourceType == "2" && m.KeyId == model.PayInModel.Id).FirstOrDefault();
            var zhangdan = model.OrderFiles.Where(m => m.SourceType == "4" && m.KeyId == model.PayInModel.Id).FirstOrDefault();

            // 付款凭证
            ReportParameter para11 = new ReportParameter("BankFilePath", picroot + pingzheng.FilePath);
            // 回传账单
            ReportParameter para12 = new ReportParameter("BillFilePath", picroot + zhangdan.FilePath);
            // 团号
            ReportParameter para13 = new ReportParameter("TourNo", model.PayInModel.TourNo);
            // 订单编号
            ReportParameter para14 = new ReportParameter("OrderCode", model.OrderModel.OrderCode);

            // 总计金额大写
            //ReportParameter para17 = new ReportParameter("InvoiceAmountChina", rmb);
            // 已付金额
            //ReportParameter para18 = new ReportParameter("TolPaid", model.OrderModel.TolPaid.ToString());
            // 尚欠金额
            //ReportParameter para19 = new ReportParameter("TolDebt", (amount - model.OrderModel.TolPaid).ToString());



            this.reportViewer1.LocalReport.SetParameters(new ReportParameter[] {para1, para2, para3, para4, para5, para6, para7,
                    para8, para9, para10, para11, para12, para13, para14});
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", model.OrderFiles));
            this.reportViewer1.RefreshReport();
        }

        private void PrintBill2()
        {
            OrderConfirmPrintVModel model = GetBill("1902000001");

            // 团队编号
            ReportParameter para1 = new ReportParameter("TourName", model.OrderModel.TourId.ToString());
            // 出团日期
            ReportParameter para2 = new ReportParameter("OutDate", model.OrderModel.OutDate.ToString());
            // 产品名称
            ReportParameter para3 = new ReportParameter("RouteName", model.LineModel.LineName);
            // 预订时间
            ReportParameter para4 = new ReportParameter("LastDate", model.OrderModel.CreatedTime.ToShortDateString());
            // 行程天数
            ReportParameter para5 = new ReportParameter("RouteDays", model.LineModel.TravelDays.ToString());
            // 地接社
            ReportParameter para6 = new ReportParameter("ReceivingAgency", "dijieshe");
            // 本期付款金额
            ReportParameter para7 = new ReportParameter("Payment", "54154");
            ReportParameter para15 = new ReportParameter("PaymentChina", "54154");
            // 联系人信息
            ReportParameter para8 = new ReportParameter("ContactName", model.OrderModel.LinkMan);
            // 客户名称
            ReportParameter para9 = new ReportParameter("CustomerName", "kehu");
            // 参考航班
            ReportParameter para10 = new ReportParameter("RefFlights", "参考航班");
            // 费用不包含
            ReportParameter para11 = new ReportParameter("NoSellingContain", model.LineModel.PriceNoContain);
            // 费用包含
            ReportParameter para12 = new ReportParameter("SellingContain", model.LineModel.PriceContain);
            // 付款要约
            ReportParameter para13 = new ReportParameter("PriceNotes", model.LineModel.PriceContain);
            //
            ReportParameter para14 = new ReportParameter("RefFlights", "cankao hanban");

            List<PersonSetModel> travellerList = new List<PersonSetModel>();
            //foreach (var m in model.TravellerVModels)
            //{
            PersonSetModel travellerModel = new PersonSetModel();
            travellerModel.PersonType = "成人";
            travellerModel.Count = 2;
            travellerModel.Price = 260;
            travellerModel.Discount = 50;
            travellerModel.Total = 420;
            //travellerModel.Note = "";

            travellerList.Add(travellerModel);
            //}


            this.reportViewer1.LocalReport.SetParameters(new ReportParameter[] {para1, para2, para3, para4, para5, para6, para7,
                    para8, para9, para10, para11, para12, para13, para14, para15 });
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("RouteDataSet", model.LineRoutes));
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("PersonDataSet", travellerList));


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
            //vModel.LineRoutes = _lineRouteBiz.GetRouteListByLineId(vModel.OrderModel.LineId);
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
            var ChildOrderList = _childBiz.GetTpChildOrderList(orderCode);
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


            //根据 【OwnerCode==>SysPlatform】获取平台信息
            vModel.PlatformModel = new PlatformBiz().GetByCustomerCode(vModel.OrderModel.OwnerCode);

            var businessCard = new SiteBiz().GetBusinessCard(vModel.OrderModel.LineId);
            vModel.LocalTravelAgency = businessCard.CustomerAccount;
            vModel.OrganizingTravelAgency = businessCard.PlatAccount;

            return vModel;
        }
    }
}