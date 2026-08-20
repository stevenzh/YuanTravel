using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Excel
{


    public class ZhangLingVModel  // : BaseVModel
    {
        [Description("分销商")]
        public string BookCustomer { get; set; }
        [Description("1个月")]
        public decimal Month1 { get; set; }
        [Description("2个月")]
        public decimal Month2 { get; set; }
        [Description("3个月")]
        public decimal Month3 { get; set; }
        [Description("4个月")]
        public decimal Month4 { get; set; }
        [Description("5个月")]
        public decimal Month5 { get; set; }
        [Description("6个月")]
        public decimal Month6 { get; set; }
        [Description("半年~一年")]
        public decimal HalfYear { get; set; }
        [Description("一年以上")]
        public decimal YearUp { get; set; }
        [Description("合计")]
        public string Sum1 { get { return "SUM(B{0}:I{0})"; }
        }
    }
}
