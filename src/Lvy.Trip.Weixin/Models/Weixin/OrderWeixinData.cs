using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;

namespace Lvy.Trip.Weixin.Models
{
    public class OrderWeixinData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem orderId { get; set; }
        public TemplateDataItem orderPrice { get; set; }
        public TemplateDataItem orderStatus { get; set; }
        public TemplateDataItem productName { get; set; }
        public TemplateDataItem remark { get; set; }
    }
}
