using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class ShareQuotaVModel : BaseVModel
    {
        /// <summary>
        /// 共享库存
        /// </summary>
        public QuotaModel Quota { get; set; }

        /// <summary>
        /// 座位表
        /// </summary>
        public TpBusSeatModel BusSeat { get; set; }
    }
}
