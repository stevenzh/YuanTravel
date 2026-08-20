using Lvy.Models;
using Lvy.Models.TicketDB;

namespace Lvy.VModels.Ticket
{
    public class SearchTicketVModel : BaseVModel
    {
        public SearchTicketVModel()
        {
            this.PagedTickets = new PagedList<TktProductModel>();
        }

        public string ProductID { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 商品分类
        /// </summary>
        public string ProductType { get; set; }
        /// <summary>
        /// 上线状态
        /// </summary>
        public int ProductState { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        public string ArriveDestName { get; set; }

        /// <summary>
        /// 购票方式
        /// </summary>
        public string ProductCategory { get; set; }
        /// <summary>
        /// 产品组
        /// </summary>
        public string TeamID { get; set; }

        public string IsImport { get; set; }


        public PagedList<TktProductModel> PagedTickets { get; set; }
    }
}