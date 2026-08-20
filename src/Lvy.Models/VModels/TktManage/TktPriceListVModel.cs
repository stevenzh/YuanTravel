using Lvy.Models.TicketDB;
using System.Collections.Generic;

namespace Lvy.VModels.Ticket
{
    public class TktPriceListVModel : BaseVModel
    {
        /// <summary>
        /// 价格列表
        /// </summary>
        public List<TktPriceModel> PriceList { get; set; }

        /// <summary>
        /// 门票产品
        /// </summary>
        public TktProductModel Product { get; set; }

    }
}