using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Saler
{
    public class OrderDebtModel 
    {
        /// <summary>
        /// 销售编号
        /// </summary>
        public string SalerCode { get; set; }

        public string OpenID { get; set; }
        /// <summary>
        /// 收客人数
        /// </summary>
        public int OrderNum { get; set; }

        /// <summary>
        /// 实收总额
        /// </summary>
        public decimal Amount { get; set; }

    }

}
