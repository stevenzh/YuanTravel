using Lvy.Models;
using Lvy.Models.TourDB;
using Lvy.VModels.Tour;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.VModels.Finance
{
    public class PaymentVModel : BaseVModel
    {
        public PaymentVModel()
        {
            if (CostModels == null)
                CostModels = new PagedList<TpTourCostModel>();
            if (Condition == null)
                Condition = new PaymentConditionModel();
            this.TotalModel = new FinanceTotalModel();
        }

        /// <summary>
        /// 订单信息列表
        /// </summary>
        public PagedList<TpTourCostModel> CostModels { get; set; }

        /// <summary>
        /// 查询条件信息
        /// </summary>
        public PaymentConditionModel Condition { get; set; }

        public FinanceTotalModel TotalModel { get; set; }
    }

    /// <summary>
    /// 条件查询类
    /// </summary>
    public class PaymentConditionModel
    {

        public string TourNo { get; set; }

        /// <summary>
        /// 计调部门
        /// </summary>
        public string LineTeam { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string CostSupplier { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string CostSupplierName { get; set; }

        /// <summary>
        /// 出发日期-起
        /// </summary>
        public string StartOutDate { get; set; }

        /// <summary>
        /// 出发日期-止
        /// </summary>
        public string EndOutDate { get; set; }

        /// <summary>
        /// 付款日期-起
        /// </summary>
        public string StartPaymentTime { get; set; }

        /// <summary>
        /// 付款日期-止
        /// </summary>
        public string EndPaymentTime { get; set; }

        /// <summary>
        /// 是否成团0：未成团；1已成团
        /// </summary>
        public string IsTourOk { get; set; }

        /// <summary>
        /// 状态
        /// 0：初始 1：已审核 2：已付款
        /// </summary>
        public string CostStatus { get; set; }

        /// <summary>
        /// 团队性质
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 团标注
        /// </summary>
        //public string TourSign { get; set; }

        /// <summary>
        /// 团期审核状态 0 未审核 1 已审核
        /// </summary>
        public string TourAuditState { get; set; }

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
    }

}