using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Excel
{
    public class DuiZhangDanExcelVModel
    {

        /// <summary>
        /// 团号
        /// </summary>
        public int TourId { get; set; }
        /// <summary>
        /// 团名
        /// </summary>
        public string TourName { get; set; }
        /// <summary>
        /// 出发日期
        /// </summary>
        public string OutDate { get; set; }
        public List<DuiZhangDanContentVModel> Contents { get; set; }
      



    }

    public class DuiZhangDanContentVModel
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
        /// 团号
        /// </summary>	
        //[DisplayName("订单编号")]
        //public int TourId { get; set; }


        /// <summary>
        /// 游客联系人
        /// </summary>	
        [Description("游客联系人")]
        public string LinkMan { get; set; }

        /// <summary>
        /// 分销商联系人
        /// </summary>	
        [Description("分销商联系人")]
        public string Managers { get; set; }

        /// <summary>
        /// 分销商
        /// </summary>
        [Description("分销商")]
        public string BookingCustomer { get; set; }

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
        [Description("报价（单价*人数）")]
        public string PriceContents { get; set; }

        /// <summary>
        /// 备注
        /// </summary>	
        [Description("备注")]
        public string Remark { get; set; }

    }
   

}
