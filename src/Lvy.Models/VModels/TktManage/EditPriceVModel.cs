using Lvy.Models.TicketDB;
using System;
using System.Collections.Generic;

namespace Lvy.VModels.Ticket
{

    public class EditPriceVModel : BaseVModel
    {
        /// <summary>
        /// 操作类型    1：新增，2：编辑
        /// </summary>
        public int Operation { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PriceMode { get; set; }

        public TktProductModel TkcketProduct { get; set; }
        public TktPriceRuleModel PriceRule { get; set; }
        public List<TktPriceModel> PriceList { get; set; }
    }

    /// <summary>
    /// 编辑其他价格规则视图模型
    /// </summary>
    public class OtherPriceVModel : BaseVModel
    {
        /// <summary>
        /// 操作类型    1：新增，2：编辑
        /// </summary>
        public int Operation { get; set; }

        public TktProductModel TkcketProduct { get; set; }
        public TktPriceRuleModel PriceRule { get; set; }
        public List<TktPriceModel> PriceList { get; set; }
    }

    /// <summary>
    /// 其他价格规则列表视图模型
    /// </summary>
    public class OtherPriceListVModel : BaseVModel
    {
        /// <summary>
        /// 门票类型
        /// </summary>
        public int TktType { get; set; }

        public TktPriceRuleModel PriceRule { get; set; }
        public List<TktPriceModel> PriceList { get; set; }
    }

    public class BatchPriceVModel : BaseVModel
    {
        public BatchPriceVModel()
        {
            this.TkcketProduct = new TktProductModel();
        }
        /// <summary>
        /// 操作类型    1：更新，2：删除
        /// </summary>
        public int Operation { get; set; }
        public string ProductID { get; set; }
        public string DateRange { get; set; }

        /// <summary>
        /// 所选择的星期
        /// </summary>
        public string SelectedDays { get; set; }

        public int RuleID { get; set; }

        public int PlanQuota { get; set; }

        public TktProductModel TkcketProduct { get; set; }
    }
}