using Lvy.VModels.Stat;
using System.Collections.Generic;

namespace Lvy.VModels.Base
{
    /// <summary>
    /// 统计项目
    /// </summary>
    public class StatItemVModel
    {
        /// <summary>
        /// 近期订单量
        /// </summary>
        public int OrdersInRecentDays { get; set; }
        /// <summary>
        /// 近期预定游客量
        /// </summary>
        public int TouristsInRecentDays { get; set; }
        /// <summary>
        /// 近期订单应收款
        /// </summary>
        public decimal AmountInRecentDays { get; set; }

        public int NewCutomerRecentDays { get; set; }

        #region 销售

        /// <summary>
        /// 待确认订单数量
        /// </summary>
        public int XiaDanCount { get; set; }

        /// <summary>
        /// 未结清订单数量
        /// </summary>
        public int JiaoKuanCount { get; set; }

        /// <summary>
        /// 客户总数
        /// </summary>
        public int CustomerCount { get; set; }

        /// <summary>
        /// 联系人总数
        /// </summary>
        public int ContactCount { get; set; }

        /// <summary>
        /// 近一个月开单客户
        /// </summary>
        public int OrderCustomerCount { get; set; }

        /// <summary>
        /// 待审核客户数量（销售）
        /// </summary>
        public int WaitAuditCustomerCount { get; set; }

        #endregion 销售

        #region 计调

        /// <summary>
        /// 待确认的订单数量
        /// </summary>
        public int QueRenDingWeiCount { get; set; }

        /// <summary>
        /// 未结清订单
        /// </summary>
        public int UnbalancedOrderCount { get; set; }

        /// <summary>
        /// 未输入订单  游客和客户资料未全
        /// </summary>
        public int WaitInputOrderCount { get; set; }

        #endregion 计调

        #region 财务

        /// <summary>
        /// 待确认收款的数量
        /// </summary>
        public int AuditPayInCount { get; set; }

        #endregion 财务

        public List<TeamStatModel> ToAuditCustomer { get; set; }

        public List<TimeStatModel> LineSalesStat { get; set; }
        public List<TimeStatModel> PlanStoreStat { get; set; }
    }
}