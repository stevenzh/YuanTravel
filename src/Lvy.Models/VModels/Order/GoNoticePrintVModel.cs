using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arch.Common;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Order
{
    /// <summary>
    /// 出团通知
    /// </summary>
    public class GoNoticePrintVModel : BaseVModel
    {
        public GoNoticePrintVModel()
        {
            if (OrderModel == null)
                OrderModel = new TpOrderModel();
            if (TourPlanModel == null)
                TourPlanModel = new TpTourPlanModel();
            if (LineModel == null)
                LineModel = new TpLineModel();
            if (CrmAccountModel == null)
                CrmAccountModel = new CrmAccountModel();
            if (LineRoutes == null)
                LineRoutes = new List<TpLineRouteModel>();
            if (TravellerModels == null)
                TravellerModels = new List<TpTravellerModel>();
            if (LineBusPointModel == null)
                LineBusPointModel = new TpLineBusPointModel();

        }

        /// <summary>
        /// 订单信息
        /// </summary>
        public TpOrderModel OrderModel { get; set; }

        /// <summary>
        /// 账户信息
        /// </summary>
        public CrmAccountModel CrmAccountModel { get; set; }

        /// <summary>
        /// 出团计划信息
        /// </summary>
        public TpTourPlanModel TourPlanModel { get; set; }

        /// <summary>
        /// 线路信息
        /// </summary>
        public TpLineModel LineModel { get; set; }

        /// <summary>
        /// 上车点信息
        /// </summary>
        public TpLineBusPointModel LineBusPointModel { get; set; }

        /// <summary>
        /// 商户信息
        /// </summary>
        public CrmCustomerModel CustomerModel { get; set; }

        /// <summary>
        /// 线路行程安排信息
        /// </summary>
        public List<TpLineRouteModel> LineRoutes { get; set; }

        /// <summary>
        /// 游客信息
        /// </summary>
        public List<TpTravellerModel> TravellerModels { get; set; }

        /// <summary>
        /// 巴士游客信息
        /// </summary>
        public List<BusTravellerVModel> BusTravellerVModels { get; set; }

        /// <summary>
        /// 出行人数
        /// </summary>
        public string TrallerCount { get; set; }

        /// <summary>
        /// 座位编号
        /// </summary>
        public string SeatNums { get; set; }
    }
}
