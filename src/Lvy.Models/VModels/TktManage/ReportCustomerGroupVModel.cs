using System;

namespace Lvy.VModels.Ticket
{
    public class ReportCustomerGroupVModel : BaseVModel
    {
        /// <summary>
        /// 快捷码 提供快速查询功能
        /// </summary>
        public string FastCode { get; set; }

        /// <summary>
        /// 预定账号，如果是代定的场合  null
        /// </summary>
        public string BookingAccount { get; set; }

        /// <summary>
        /// 分销商(code)
        /// </summary>
        public string BookingCustomer { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 价格类型
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// 签单价|返利
        /// </summary>
        public decimal SysPrice { get; set; }

        /// <summary>
        /// 购票方式
        ///     1:固定签单，2:特殊签单，3:任务单，4:特殊任务单
        /// </summary>
        public int TktType { get; set; }

        /// <summary>
        /// 人数
        /// </summary>
        public int PeopleNum { get; set; }

        /// <summary>
        /// 第几天
        /// </summary>
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 导游姓名
        /// </summary>
        public string GuideName { get; set; }

        /// <summary>
        /// 导游电话
        /// </summary>
        public string GuidePhone { get; set; }

        /// <summary>
        /// 分销商联系人姓名
        /// </summary>
        public string Managers { get; set; }

        /// <summary>
        /// 分销商联系人电话
        /// </summary>
        public string ManagerPhone { get; set; }

        /// <summary>
        /// 分销商(名称)
        /// </summary>
        public string BookingCustomerName { get; set; }
    }
}