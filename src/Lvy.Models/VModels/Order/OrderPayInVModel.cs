using Lvy.Models.OrderDB;
using System.Collections.Generic;

namespace Lvy.VModels.Order
{
    /// <summary>
    /// 缴款单
    /// </summary>
    public class OrderPayInVModel
    {
        public TpOrderModel OrderModel { get; set; }

        public TpOrderPayInModel PayInModel { get; set; }

        public List<TpOrderFileModel> OrderFiles { get; set; }

        /// <summary>
        /// 账单选择
        /// </summary>
        public string selectBill { get; set; }

        /// <summary>
        /// 付款凭证选择
        /// </summary>
        public string selectBank { get; set; }
    }
}