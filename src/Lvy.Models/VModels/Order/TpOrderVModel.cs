using System;
using System.Collections.Generic;
using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Order
{
    /// <summary>
    /// 对应订单信息的View Model
    /// </summary>
    [Serializable]
    public class TpOrderVModel : BaseVModel
    {
        public TpOrderVModel()
        {
            if (OrderModel == null)
                OrderModel = new TpOrderModel();
            if (LineModel == null)
                LineModel = new TpLineModel();
            if (PagedList == null)
                PagedList = new PagedList<TpOrderModel>();
            if (OrderModels == null)
                OrderModels = new List<TpOrderModel>();
            this.FirstTime = true;
        }

        #region 查询条件

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 订单状态 1，新订单；
        /// 2，已确认；
        /// 3，已结算；
        /// 4，已取消；
        /// 5，退团单 
        /// </summary>
        public string OrderState { get; set; }
        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrderSource { get; set; }
        /// <summary>
        /// 团名称
        /// </summary>
        public string TourName { get; set; }

        /// <summary>
        /// 线路名称
        /// </summary>
        public string LineName { get; set; }

        public string LineScope { get; set; }

        /// <summary>
        /// 线路类型
        /// </summary>
        public string LineType { get; set; }

        /// <summary>
        /// 预定日期-开始
        /// </summary>
        public string CreatedRange { get; set; }
        /// <summary>
        /// 发出日期-开始
        /// </summary>
        public string OutDateRange { get; set; }
        /// <summary>
        /// 分销商编码
        /// </summary>
        public string CustomerCode { get; set; }
        /// <summary>
        /// 分销商名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// OTA关联订单号
        /// </summary>
        public string JoinOrderCode { get; set; }
        /// <summary>
        /// 团编号
        /// </summary>
        public string TourId { get; set; }
        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }
        /// <summary>
        /// 分销商联系人
        /// </summary>
        public string Manager { get; set; }

        public bool FirstTime { get; set; }

        #endregion

        /// <summary>
        /// 订单信息
        /// </summary>
        public TpOrderModel OrderModel { get; set; }

        /// <summary>
        /// 线路信息
        /// </summary>
        public TpLineModel LineModel { get; set; }

        /// <summary>
        /// 订单信息列表
        /// </summary>
        public PagedList<TpOrderModel> PagedList { get; set; }

        public OrderTotalModel TotalModel { get; set; }

        /// <summary>
        /// 订单列表
        /// </summary>
        public List<TpOrderModel> OrderModels { get; set; }

        /// <summary>
        /// 产品部门
        /// </summary>
        public string CrmTeamId { get; set; }
        /// <summary>
        /// 销售部门
        /// </summary>

        public string SaleTeamId { get; set; }
        /// <summary>
        /// 销售
        /// </summary>
        public string SalerCode { get; set; }

        ///// <summary>
        ///// 等待下单状态
        ///// </summary>
        //public bool IsWaitXiadan { get; set; }
        ///// <summary>
        ///// 等待交款状态
        ///// </summary>
        //public bool IsWaitJiaoKuan { get; set; }
        ///// <summary>
        ///// 等待确认定位
        ///// </summary>
        //public bool IsWaitQueRenDingWei { get; set; }

    }

    public class OrderTotalModel
    {
        /// <summary>
        /// 人数合计
        /// </summary>
        public int PaxSum { get; set; }
        public int ConfirmPax { get; set; }
        public int HoldPax { get; set; }
        /// <summary>
        /// 团款合计
        /// </summary>
        public decimal AmountSum { get; set; }
        /// <summary>
        /// 未收款合计
        /// </summary>
        public decimal PaidSum { get; set; }
    }
}
