using Lvy.Models;
using Lvy.Models.OrderDB;
using System.Collections.Generic;

namespace Lvy.VModels.Finance
{
    public class InvoiceVModel : BaseVModel
    {
        public InvoiceVModel()
        {
            this.InvoicePageList = new PagedList<ViewInvoiceModel>();
        }

        /// <summary>
        /// 部门
        /// </summary>
        public string TeamCode { get; set; }

        /// <summary>
        /// 发票状态
        /// </summary>
        public int State { get; set; }

        public List<TpInvoiceModel> JieSongTypes { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// 销售
        /// </summary>
        public string SalerCode { get; set; }

        public string OrderCode { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerName { get; set; }

        public PagedList<ViewInvoiceModel> InvoicePageList { get; set; }
    }
}