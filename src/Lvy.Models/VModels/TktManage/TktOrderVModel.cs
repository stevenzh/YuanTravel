using Lvy.Models;
using Lvy.Models.TourDB;

namespace Lvy.VModels.Ticket
{
    /// <summary>
    /// 门票订单视图模型
    /// </summary>
    public class TktOrderVModel : BaseVModel
    {
        public TktOrderVModel()
        {
            this.Orders = new PagedList<TpTourBalanceModel>();
        }

        #region 表单

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public string DateRange { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int OrderState { get; set; }

        public int AuditState { get; set; }

        /// <summary>
        /// 预定客户（编号）
        /// </summary>
        public string BookingCustomer { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 结算状态
        /// 0:未结算；1：已结算
        /// </summary>
        public string SettlementState { get; set; }

        #endregion 表单

        #region 统计

        /// <summary>
        /// 销售量
        /// </summary>
        public int TotalSaledNum { get; set; }

        /// <summary>
        /// 销售额
        /// </summary>
        public decimal TotalSaledVolume { get; set; }

        /// <summary>
        /// 已收款
        /// </summary>
        public decimal TotalPaid { get; set; }

        /// <summary>
        /// 欠款
        /// </summary>
        public decimal TotalDebt { get { return TotalSaledVolume - TotalPaid; } }

        #endregion 统计

        public PagedList<TpTourBalanceModel> Orders { get; set; }
    }
}