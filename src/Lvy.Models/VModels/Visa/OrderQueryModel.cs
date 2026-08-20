using System;

namespace Lvy.Visa.VModels
{
    public class OrderQueryModel
    {
        public OrderQueryModel()
        {
        }
        /// <summary>
        /// 产品组
        /// </summary>
        public string TeamID { get; set; }
        public string OrderCode { get; set; }
        public string ContactName { get; set; }
        public string ContactTel { get; set; }
        public string BookMan { get; set; }
        public string ProductName { get; set; }
        public string ProduceManager { get; set; }
        public string ApplicantName { get; set; }
        public int PaymentType { get; set; }
        public int OrderStatus { get; set; }
        public int PaymentStatus { get; set; }
        public int OrderSource { get; set; }
        public int TraceState { get; set; }

        /// <summary>
        /// 预订时间
        /// </summary>
        public string BookDate { get; set; }

        /// <summary>
        /// 最后付款时限
        /// </summary>
        //public string LaterPayDate { get; set; }

        /// <summary>
        /// 取消时间
        /// </summary>
        //public string CancelDate { get; set; }

        public string TourNo { get; set; }

        //送签日期
        public string SendVisaDate { get; set; }

        /// <summary>
        /// 材料截止接受日期
        /// </summary>
        public string MaterialDeadline { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        public string OperateName { get; set; }

        /// <summary>
        /// 否已提交付款申请
        /// </summary>
        public int IsPaymentApplication { get; set; }

        /// <summary>
        /// 面试日期
        /// </summary>
        public string InterviewDate { get; set; }

        /// <summary>
        /// 跟进日期
        /// </summary>
        public string FollowupDate { get; set; }
    }
}