using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Excel
{

    /// <summary>
    /// 订单统计
    /// </summary>
    public class OrderExcelVModel
    {
        /// <summary>
        /// 订单号
        /// </summary>
        [Description("订单编号")]
        public string OrderCode { get; set; }
        /// <summary>
        /// OTA关联订单号
        /// </summary>
        [Description("关联订单号")]
        public string JoinOrderCode { get; set; }


        /// <summary>
        /// 团名称
        /// 【编号】-【团名称】
        /// </summary>
        [Description("团名称")]
        public string TourName { get; set; }

        [Description("出发日期")]
        public string OutDate { get; set; }

        /// <summary>
        /// 【快捷码】-【客户名称】
        /// </summary>
        [Description("分销商")]
        public string BookingCustomer { get; set; }
        /// <summary>
        /// 游客联系人
        /// </summary>	
        [Description("游客联系人")]
        public string LinkMan { get; set; }
        [Description("游客联系电话")]
        public string LinkPhone { get; set; }
        /// <summary>
        /// 分销商联系人
        /// </summary>	
        [Description("分销商联系人")]
        public string Managers { get; set; }
        [Description("分销商联系人电话")]
        public string ManagerPhone { get; set; }

        /// <summary>
        /// 出行人数
        /// </summary>	
        [Description("人数")]
        public int TravellerCount { get; set; }

        /// <summary>
        /// 总应收
        /// </summary>	
        [Description("应收")]
        public decimal TolYsPrice { get; set; }
        /// <summary>
        /// 自费说明
        /// </summary>
        [Description("自费")]
        public string ZiFei { get; set; }
        [Description("单房差")]
        public string SingleRoom { get; set; }
        [Description("接送费")]
        public decimal JsPrice { get; set; }
        [Description("折让")]
        public decimal FanLi { get; set; }
        [Description("报价（单价*人数）")]
        public string PriceContents { get; set; }


        [Description("订单状态")]
        public string OrderState { get; set; }
        [Description("备注")]
        public string Remark { get; set; }
    }

}
