using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;

namespace Lvy.Trip.Weixin.Models
{
    public class OrderData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem OrderId { get; set; }
        public TemplateDataItem ProductId { get; set; }
        public TemplateDataItem ProductName { get; set; }
        public TemplateDataItem remark { get; set; }
    }
}
