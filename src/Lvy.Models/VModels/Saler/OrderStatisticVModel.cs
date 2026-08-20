using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models;
using Lvy.Models.OrderDB;

namespace Lvy.VModels.Saler
{
    public class OrderStatisticVModel : BaseVModel
    {
        public OrderStatisticCondition Condition { get; set; }

        /// <summary>
        /// 订单信息列表
        /// </summary>
        public PagedList<TpOrderModel> OrderModels { get; set; }

        #region 列表汇总

        /// <summary>
        /// 收客人数
        /// </summary>
        public int SumTravellerCount { get; set; }

        /// <summary>
        /// 实收总额
        /// </summary>
        public decimal SumTolPaid { get; set; }

        /// <summary>
        /// 销售总额
        /// </summary>
        public decimal SumPriceCount { get; set; }

        /// <summary>
        /// 返利总额
        /// </summary>
        public decimal SumFanLiCount { get; set; }

        /// <summary>
        /// 剩余总额
        /// </summary>
        public decimal ShengYuCount { get; set; }


        #endregion
    }

    public class OrderStatisticCondition
    {
        /// <summary>
        /// 线路类型
        /// </summary>	
        public string LineType { get; set; }
        public string LineScope { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string LineName { get; set; }
        /// <summary>
        /// 分销商
        /// </summary>
        public string BookingCustomer { get; set; }
        /// <summary>
        /// 出发日期-起
        /// </summary>	
        public string OutDateRange { get; set; }
        /// <summary>
        /// 预定日期-起
        /// </summary>	
        public string CreatedTimeRange { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 团编号
        /// </summary>
        public string TourId { get; set; }
        public string TourNo { get; set; }
        /// <summary>
        /// 订单状态  1，新订单；2，已确认；8，已取消（未产生费用）；9，已退团（产生费用）
        /// 10，已结算
        /// </summary>	
        public string OrderState { get; set; }
        /// <summary>
        /// 结算状态
        /// 0:未结算；1：已结算
        /// </summary>
        public string SettlementState { get; set; }
        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrderSource { get; set; }
    }
}
