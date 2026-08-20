using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.Product
{
    public class AddTourVModel : BaseVModel
    {
        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 线路
        /// </summary>
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 座位列表
        /// </summary>
        public List<BusSeatModel> SeatList { get; set; }

        /// <summary>
        /// 所选择的出发日期 逗号分隔
        /// </summary>
        public string SelectedDays { get; set; }

        /// <summary>
        /// 团计划
        /// </summary>
        public TpTourPlanModel TourPlan { get; set; }

        /// <summary>
        /// 资源
        /// </summary>
        public QuotaModel Quota { get; set; }

        /// <summary>
        /// 单房差
        /// </summary>
        public decimal SingleRoom { get; set; }

        /// <summary>
        /// 小费
        /// </summary>
        public decimal Tips { get; set; }

        /// <summary>
        /// 特价让利
        /// </summary>
        public decimal TeJiaFanLi { get; set; }

        /// <summary>
        /// 自费
        /// </summary>
        public decimal ZeiFei { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public List<TpPriceModel> PriceList { get; set; }

        /// <summary>
        /// 航空公司
        /// </summary>
        public List<BaseAirlineModel> AirlineList { get; set; }

        /// <summary>
        /// 航班信息
        /// </summary>
        public List<TpTourFlightModel> TourFlightList { get; set; }

        /// <summary>
        /// 套餐列表
        /// </summary>
        public List<TpLineSuiteModel> SuiteList { get; set; }
    }
}