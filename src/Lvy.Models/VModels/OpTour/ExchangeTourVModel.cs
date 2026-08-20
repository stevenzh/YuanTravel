using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.OpTour
{
    public class ExchangeTourVModel : BaseVModel
    {
        /*
         * 约定：
         * 由于在分入到新团视图，引用了CopyTourVModel视图模型，
         * 为了不导致提交表单的时候表单元素重复，在此约定：该视图模型中的属性名与CopyTourVModel中不能重复。
         */

        /// <summary>
        /// 操作状态
        /// 1：换团，选择待换团；2：并入他团：选择目标团
        /// </summary>
        public int OperationState { get; set; }

        /// <summary>
        /// 需要被调整的团（被换团）Id
        /// </summary>
        public int ExchangeFromTourId { get; set; }

        /// <summary>
        /// 需要被调整的团（被换团）
        /// </summary>
        public TpTourPlanModel ExchangeFromTour { get; set; }

        /// <summary>
        /// 并入的目标团
        /// </summary>
        public int ExchangeToTourId { get; set; }

        /// <summary>
        /// 所选订单号（逗号分隔）
        /// </summary>
        public string ExchangeOrders { get; set; }

        #region Query Condition

        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 产品部门
        /// </summary>
        public string CrmTeamId { get; set; }

        /// <summary>
        /// 线路名
        /// </summary>
        public string LineName { get; set; }

        public string TourOk { get; set; }
        public string MinOutDate { get; set; }
        public string MaxOutDate { get; set; }

        #endregion Query Condition

        /// <summary>
        /// 团期、库存Map列表
        /// </summary>
        public List<TourQuotaMapModel> TourList { get; set; }

        /// <summary>
        /// 订单列表
        /// </summary>
        public List<TpOrderModel> OrderList { get; set; }

        public int ExchangeTourTrafficType { get; set; }
    }
}