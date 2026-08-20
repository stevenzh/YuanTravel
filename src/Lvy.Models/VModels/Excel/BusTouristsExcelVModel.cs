using System.ComponentModel;

namespace Lvy.VModels.Excel
{
    /// <summary>
    /// 汽车班游客名单vmodel
    /// </summary>
    public class BusTouristsExcelVModel
    {
        [Description("订单编号")]
        public string OrderCode { get; set; }

        [Description("座位号")]
        public string Seats { get; set; }

        [Description("联系人")]
        public string LinkMan { get; set; }

        [Description("联系电话")]
        public string LinkPhone { get; set; }

        [Description("人数")]
        public int TravellerCount { get; set; }

        [Description("报价说明")]
        public string PriceContents { get; set; }

        [Description("自费")]
        public string ZiFei { get; set; }

        [Description("单房差")]
        public string SingleRoom { get; set; }

        [Description("上车点")]
        public string BusPoint { get; set; }

        [Description("分销商")]
        public string BookingCustomer { get; set; }

        [Description("备注")]
        public string Remark { get; set; }
    }
}