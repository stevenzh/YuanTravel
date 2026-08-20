using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models;
using Lvy.Models.OrderDB;

namespace Lvy.VModels.Finance
{
    public class FinanceVModel : BaseVModel
    {
        public FinanceVModel()
        {
            if (OrderModels == null)
                OrderModels = new PagedList<TpOrderModel>();
            if (Condition == null)
                Condition = new ConditionModel();
        }

        /// <summary>
        /// 订单信息列表
        /// </summary>
        public PagedList<TpOrderModel> OrderModels { get; set; }

        /// <summary>
        /// 查询条件信息
        /// </summary>
        public ConditionModel Condition { get; set; }

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
        /// 剩余总额
        /// </summary>
        public decimal ShengYuCount { get; set; }


        public bool IsSaler { get; set; }

        public bool IsSalerLeader { get; set; }

        public bool IsSalerBoss { get; set; }


        #endregion

    }

    /// <summary>
    /// 条件查询类
    /// </summary>
    public class ConditionModel
    {
        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }
        /// <summary>
        /// 线路类型
        /// </summary>	
        public string LineType { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 分销商 （编号） -》关联 crm_customer-》(Name)
        /// </summary>
        public string BookingCustomer { get; set; }
        /// <summary>
        /// 分销商（名称）
        /// </summary>
        public string BookingCustomerName { get; set; }
        /// <summary>
        /// 销售部门
        /// </summary>
        public string SaleTeamId { get; set; }
        /// <summary>
        /// 销售
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// 原：订单状态  1，新订单；2，已确认；8，已取消（未产生费用）；9，已退团（产生费用）
        /// 10，已结算
        /// 现：订单状态：10，已结算；11，未结算( 2.已确认   和  9.已退团 )
        /// </summary>	
        public string OrderState { get; set; }
        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrderSource { get; set; }
        /// <summary>
        /// 产品部门
        /// </summary>
        public string CrmTeamId { get; set; }

        /// <summary>
        /// 出发日期-起
        /// </summary>	
        public string StartOutDate { get; set; }

        /// <summary>
        /// 出发日期-止
        /// </summary>
        public string EndOutDate { get; set; }

        /// <summary>
        /// 预定日期-起
        /// </summary>	
        public string StartCreatedTime { get; set; }

        /// <summary>
        /// 预定日期-止
        /// </summary>
        public string EndCreatedTime { get; set; }

        /// <summary>
        /// 收款日期-起
        /// </summary>	
        public string StartPayInTime { get; set; }

        /// <summary>
        /// 收款日期-止
        /// </summary>
        public string EndPayInTime { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 是否成团0：未成团；1已成团
        /// </summary>
        public string IsTourOk { get; set; }
        /// <summary>
        /// 结算状态
        /// 0:未结算；1：已结算
        /// </summary>
        public string SettlementState { get; set; }

        /// <summary>
        /// 关联订单号
        /// </summary>
        public string JoinOrderCode { get; set; }

        /// <summary>
        /// 排序集合
        /// </summary>
        public Dictionary<int, KeyValueBean> SortCollection
        {
            get
            {
                Dictionary<int, KeyValueBean> dic = new Dictionary<int, KeyValueBean>();
                dic.Add(1, new KeyValueBean { Key = "OrderCode ASC", Value = "订单号升序" });
                dic.Add(2, new KeyValueBean { Key = "OrderCode DESC", Value = "订单号降序" });
                dic.Add(3, new KeyValueBean { Key = "JoinOrderCode ASC", Value = "关联订单号升序" });
                dic.Add(4, new KeyValueBean { Key = "JoinOrderCode DESC", Value = "关联订单号降序" });
                return dic;
            }
        }

        /// <summary>
        /// 排序键值对
        /// </summary>
        public List<KeyValueBean> SortKeyValueBean
        {
            get
            {
                return SortCollection.Select(dic => new KeyValueBean { Key = dic.Key.ToString(), Value = dic.Value.Value }).ToList();
            }
        }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortKey { get; set; }
        /// <summary>
        /// 团队性质
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 团单状态  0:未成团, 1:已成团 2:团单制作中 3:提交财务 4:财务审核 5:收付款完成
        /// </summary>
        public string TourAuditState { get; set; }

    }
}
