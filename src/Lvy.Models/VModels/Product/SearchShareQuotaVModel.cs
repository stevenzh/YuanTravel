using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class SearchShareQuotaVModel : BaseVModel
    {
        /// <summary>
        /// 共享编号
        /// </summary>
        public string ShareId { get; set; }
        /// <summary>
        /// 共享名称
        /// </summary>
        public string ShareName { get; set; }


        public PagedList<QuotaModel> PagedQuota { get; set; }

    }
}
