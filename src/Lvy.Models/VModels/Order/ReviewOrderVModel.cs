using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.Order
{
    public class ReviewOrderVModel : BaseVModel
    {
        /// <summary>
        /// 订单
        /// </summary>
        public TpOrderModel Order { get; set; }

        /// <summary>
        /// 线路
        /// </summary>
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 上车点信息
        /// </summary>
        public List<TpLineBusPointModel> LineBusPoints { get; set; }

        /// <summary>
        /// 订单价格
        /// </summary>
        public List<TpPriceModel> Prices { get; set; }

        /// <summary>
        /// 游客信息
        /// </summary>
        public List<TpTravellerModel> Travellers { get; set; }
    }
}