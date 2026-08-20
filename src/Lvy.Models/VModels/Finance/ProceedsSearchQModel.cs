using Lvy.Models;
using Lvy.Models.BaseDB;
using System;
using System.Collections.Generic;

namespace Lvy.VModels.Finance
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class ProceedsSearchQModel : BaseVModel
    {
        public ProceedsSearchQModel()
        {
            if (ProceedsList == null)
            {
                ProceedsList = new List<VTProceedsModel>();
            }
            if (ProceedsPageList == null)
            {
                ProceedsPageList = new PagedList<VTProceedsModel>();
            }
        }

        public List<VTProceedsModel> ProceedsList { set; get; }

        public PagedList<VTProceedsModel> ProceedsPageList { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ProceedsCode { get; set; }

        /// <summary>
        /// 交款人
        /// </summary>
        public string ChargerName { get; set; }

        /// <summary>
        /// 交款部门
        /// </summary>
        public string ChargerDept { set; get; }

        /// <summary>
        /// 交款客户
        /// </summary>
        public string ChargerHost { set; get; }

        /// <summary>
        /// 收款时间
        /// </summary>
        public string CollectedDateFrom { get; set; }

        /// <summary>
        /// 收款时间
        /// </summary>
        public string CollectedDateTo { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public string PayStatus { get; set; }

        /// <summary>
        /// pos,cash,
        /// </summary>
        public string CollectionType { get; set; }

        /// <summary>
        /// 到账状态
        /// </summary>
        public string CollectedStatus { get; set; }

        /// <summary>
        /// 收款人
        /// </summary>
        public string CollectedMan { get; set; }

        /// <summary>
        /// 收款状态
        /// </summary>
        public string CollectionStatus { get; set; }

        /// <summary>
        /// 到账确认人
        /// </summary>
        public string CollectedConfirmMan { get; set; }
    }
}