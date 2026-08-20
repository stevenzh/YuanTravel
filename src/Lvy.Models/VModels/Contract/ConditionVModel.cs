using Lvy.Models;

namespace Lvy.VModels.Contract
{
    public class ConditionVModel : BaseVModel
    {
        public ConditionVModel()
        {
            if (PagedList == null)
                PagedList = new PagedList<ContractInfo>();
        }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string orderCode { get; set; }

        /// <summary>
        /// 合同编号
        /// </summary>
        public string contractNumber { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string routeName { get; set; }

        /// <summary>
        /// 合同信息列表
        /// </summary>
        public PagedList<ContractInfo> PagedList { get; set; }
    }
}