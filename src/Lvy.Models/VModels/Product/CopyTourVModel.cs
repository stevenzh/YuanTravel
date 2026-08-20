using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class CopyTourVModel : BaseVModel
    {
        /// <summary>
        /// 线路信息
        /// </summary>
        public TpLineModel Line { get; set; }
        /// <summary>
        /// 团期信息
        /// </summary>
        public TpTourPlanModel Tour { get; set; }
        /// <summary>
        /// 库存信息
        /// </summary>
        public QuotaModel Quota { get; set; }
        /// <summary>
        /// 报价
        /// </summary>
        public List<TpPriceModel> PriceList { get; set; }

        public string OutDate { get; set; }

        public decimal Tips { get; set; }
        public decimal SingleRoom { get; set; }
        public decimal TeJiaFanLi { get; set; }
    }
}
