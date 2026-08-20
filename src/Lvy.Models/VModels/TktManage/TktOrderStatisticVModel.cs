//using Lvy.Models;
//using Lvy.Models.TourDB;

//namespace Lvy.VModels.Saler
//{
//    public class TktOrderStatisticVModel : BaseVModel
//    {
//        #region 表单查询条件

//        /// <summary>
//        /// 订单编号
//        /// </summary>
//        public string OrderCode { get; set; }

//        /// <summary>
//        /// 开始日期
//        /// </summary>
//        public string OutDateRange { get; set; }

//        /// <summary>
//        /// 产品名称
//        /// </summary>
//        public string ProductName { get; set; }

//        /// <summary>
//        /// 订单状态
//        /// </summary>
//        public string OrderState { get; set; }

//        /// <summary>
//        /// 预定客户（编号）
//        /// </summary>
//        public string BookingCustomer { get; set; }

//        /// <summary>
//        /// 产品编号
//        /// </summary>
//        public string ProductId { get; set; }

//        ///// <summary>
//        ///// 结算状态
//        ///// 0:未结算；1：已结算
//        ///// </summary>
//        //public string SettlementState { get; set; }

//        #endregion 表单查询条件

//        #region 统计

//        /// <summary>
//        /// 销售量
//        /// </summary>
//        public int TotalSaledNum { get; set; }

//        /// <summary>
//        /// 销售额
//        /// </summary>
//        public decimal TotalSaledVolume { get; set; }

//        /// <summary>
//        /// 已收款
//        /// </summary>
//        public decimal TotalPaid { get; set; }

//        /// <summary>
//        /// 欠款
//        /// </summary>
//        public decimal TotalDebt { get { return TotalSaledVolume - TotalPaid; } }

//        #endregion 统计

//        public PagedList<TpTourBalanceModel> Orders { get; set; }
//    }
//}