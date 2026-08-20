using System.ComponentModel;

namespace Lvy.VModels.Excel
{
    public class PayInVModel : BaseVModel
    {
        [Description("供应商")]
        public string Supplier { get; set; }

        [Description("成人价")]
        public int PriceType1 { get; set; }

        [Description("儿童价")]
        public int PriceType2 { get; set; }

        [Description("亲子价（买2送1）")]
        public int PriceType3 { get; set; }

        [Description("亲子价（买1送1）")]
        public int PriceType4 { get; set; }

        [Description("老人价")]
        public int PriceType5 { get; set; }

        [Description("未知")]
        public int PriceType6 { get; set; }
    }

    public class PayInInfoVModel : BaseVModel
    {
        //[Description("供应商")]
        //public string Supplier { get; set; }
        [Description("分销商")]
        public string BookingCustomer { get; set; }

        [Description("成人价")]
        public int PriceType1 { get; set; }

        [Description("儿童价")]
        public int PriceType2 { get; set; }

        [Description("亲子价（买2送1）")]
        public int PriceType3 { get; set; }

        [Description("亲子价（买1送1）")]
        public int PriceType4 { get; set; }

        [Description("老人价")]
        public int PriceType5 { get; set; }

        [Description("未知")]
        public int PriceType6 { get; set; }

        [Description("联系人")]
        public string LinkMan { get; set; }

        [Description("电话")]
        public string LinkPhone { get; set; }

        [Description("公司地址")]
        public string Address { get; set; }
    }

    public class OrderReportByDateVModel : BaseVModel
    {
        /// <summary>
        /// 出发日期
        /// </summary>
        [Description("出发日期")]
        public string OutDate { get; set; }

        [Description("星期")]
        public string WeekCn { get; set; }

        [Description("其它人数")]
        public int OtherPax { get; set; }

        [Description("总人数")]
        public int AllPax { get; set; }
    }
}