using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.Product
{
    public class EditTourVModel : BaseVModel
    {
        /// <summary>
        /// 是否为复制
        ///     DESC: 0 编辑 1 复制
        /// </summary>
        public int IsCopy { get; set; }

        /// <summary>
        /// 线路
        /// </summary>
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 团计划
        /// </summary>
        public TpTourPlanModel Tour { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public QuotaModel Quota { get; set; }

        /// <summary>
        /// 共享库存字典
        /// </summary>
        public List<KeyValueBean> ShareQuotaDic { get; set; }

        /// <summary>
        /// 团计划-库存关系
        /// </summary>
        public TourQuotaMapModel Map { get; set; }

        /// <summary>
        /// 座位表
        /// </summary>
        public TpBusSeatModel BusSeat { get; set; }

        /// <summary>
        /// 报价
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

        public List<TpOrderModel> OrderList { get; set; }
    }
}