using Lvy.Models.TicketDB;
using System;

namespace Lvy.VModels.Ticket
{
    public class TktProductVModel : TktProductModel
    {

        #region 常规价

        /// <summary>
        /// 价格类型  标准价
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// 签单价|返利
        /// </summary>
        public decimal SysPrice { get; set; }

        /// <summary>
        /// 是否常规报价
        ///     0:否 1：是
        /// </summary>
        public int IsGeneral { get; set; }

        /// <summary>
        /// 合同有效年份
        /// </summary>
        public int Year { get; set; }

        #endregion 常规价

        /// <summary>
        /// 数量说明
        /// </summary>
        public string NumberDesc
        {
            get
            {
                if (PlanQuota > 0)
                {
                    return (PlanQuota - HoldQuota - UsedQuota).ToString();
                }
                else
                {
                    return "充足";
                }
            }
        }

        /// <summary>
        /// 系统价显示
        /// </summary>
        public string SysPriceDesc
        {
            get
            {
                if (TktType == 1 || TktType == 2)
                {
                    return "签单价：" + SysPrice;
                }
                else
                {
                    return "返利：" + SysPrice;
                }
            }
        }
    }
}