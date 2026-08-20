using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;

namespace Lvy.Trip.Weixin.Models
{
    public class MessageData
    {
        public TemplateDataItem first { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public TemplateDataItem keyword1 { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public TemplateDataItem keyword2 { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public TemplateDataItem keyword3 { get; set; }
        /// <summary>
        /// 绑定时间
        /// </summary>
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem remark { get; set; }
    }
}
