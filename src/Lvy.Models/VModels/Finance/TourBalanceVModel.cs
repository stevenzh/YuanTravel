using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Models.TourDB;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.VModels.Tour
{
    /// <summary>
    /// 单团核算详细
    /// </summary>
    public class TourBalanceVModel : BaseVModel
    {
        public TourBalanceVModel()
        {
            this.Condition = new TourSearchCondition();
            this.CostList = new List<TpTourCostModel>();
            this.Balances = new PagedList<TpTourBalanceModel>();
            this.Balance = new TpTourBalanceModel();
            this.FileList = new List<TourFileModel>();
            this.Invoices = new List<TpInvoiceModel>();
            this.PayInList = new List<TpOrderPayInModel>();
            this.InvoiceModel = new TpInvoiceModel();
            this.IsCaiWu = 0;
            this.IsCopy = false;
        }

        /// <summary>
        /// 当前打开入口 是否财务用户
        /// </summary>
        public int IsCaiWu { get; set; }

        public bool IsCopy { get; set; }
        public string MasterOrderCode { get; set; }

        /// <summary>
        /// 查询条件
        /// </summary>
        public TourSearchCondition Condition { get; set; }

        /// <summary>
        ///
        /// </summary>
        public PagedList<TpTourBalanceModel> Balances { get; set; }

        public TpLineModel Line { get; set; }
        public TpTourPlanModel Tour { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TpTourBalanceModel Balance { get; set; }

        public TpTourCostModel TourCost { get; set; }

        /// <summary>
        /// 付款列表
        /// </summary>
        public List<TpTourCostModel> CostList { get; set; }

        /// <summary>
        ///  有效订单
        /// </summary>
        public List<CommonOrderModel> Orders { get; set; }

        public TpOrderPayInModel PayInModel { get; set; }

        /// <summary>
        /// 收款列表
        /// </summary>
        public List<TpOrderPayInModel> PayInList { get; set; }

        /// <summary>
        ///发票列表
        /// </summary>
        public List<TpInvoiceModel> Invoices { get; set; }
        public TpInvoiceModel InvoiceModel { get; set; }

        public TourFileModel FileModel { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<TourFileModel> FileList { get; set; }

        public FinanceTotalModel SumCost { get; set; }

        /// <summary>
        /// 缴款单 选择的文件
        /// </summary>
        public string selectBank;

        public string selectBill;

        /// <summary>
        /// 成本
        /// </summary>
        public List<TpTourCostModel> CostModels
        {
            get { return CostList.ToList(); }
            set { CostList.AddRange(value); }
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        public class TourSearchCondition
        {
            /// <summary>
            /// 团号
            /// </summary>
            public string TourNo { get; set; }

            /// <summary>
            /// 出发日期段
            /// </summary>
            public string OutDateRange { get; set; }

            /// <summary>
            /// 分组id
            /// </summary>
            public string TeamId { get; set; }
            public string BranchID { get; set; }
            /// <summary>
            ///
            /// </summary>
            public string ProductName { get; set; }

            public int IsPackage { get; set; }

            public int Type { get; set; }

            /// <summary>
            /// 旅游产品类型
            /// </summary>
            public int ProductType { get; set; }

            /// <summary>
            /// 团期状态 0 未审核 1 已审核
            /// </summary>
            public string TourAuditState { get; set; }
        }
    }

    /// <summary>
    /// 结算明细
    /// </summary>
    public class FinanceTotalModel
    {
        /// <summary>
        /// 合计应收
        /// </summary>
        public decimal SumTolYsPrice { get; set; }

        /// <summary>
        /// 合计已收
        /// </summary>
        public decimal SumTolPaid { get; set; }

        /// <summary>
        /// 合计未收
        /// </summary>
        public decimal SumNoPaid
        {
            get { return SumTolYsPrice - SumTolPaid; }
        }

        /// <summary>
        /// 合计成本
        /// </summary>
        public decimal SumTolCost { get; set; }
        /// <summary>
        /// 已付成本
        /// </summary>
        public decimal SumPaidCost { get; set; }
        /// <summary>
        /// 未付成本
        /// </summary>
        public decimal SumNoPaidCost
        {
            get { return SumTolCost - SumPaidCost; }
        }

        /// <summary>
        /// 合计毛利
        /// </summary>
        public decimal SumTolMaoLi { get; set; }


        /// <summary>
        /// 现售(现付)
        /// </summary>
        public decimal XianShou { get; set; }
        /// <summary>
        /// 签单(周付，月付，季付)
        /// </summary>
        public decimal Qiandan { get; set; }

        /// <summary>
        /// 结算人数
        /// </summary>
        public int SumTravellerCount { get; set; }
    }

    public class CommonOrderModel
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 线路名
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactPhone { get; set; }


        /// <summary>
        /// 总应收(实际应收，结算客户折让过的)
        /// </summary>
        public decimal TolYsPrice { get; set; }

        /// <summary>
        /// 总实收
        /// </summary>
        public decimal TolPaid { get; set; }


        /// <summary>
        /// 出行人数
        /// </summary>
        public int TravellerCount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 分销商编号
        /// </summary>
        public string AgentCode { get; set; }

        /// <summary>
        /// 分销商名称
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// 结算状态
        /// 1：未支付 2：定金部分支付 3：定金已付 4：部分支付 5:已付清
        /// </summary>
        public int JieSuanState { get; set; }

        /// <summary>
        /// 销售所在组
        /// </summary>
        public string SalesTeamId { get; set; }

        /// <summary>
        /// 销售code
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// 销售名称
        /// </summary>
        public string SalerName { get; set; }

    }
}