using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Online
{
    public class SpecialPriceTourVModel : BaseVModel
    {
        /// <summary>
        /// 线路类型
        /// </summary>
        public int LineType { get; set; }
        /// <summary>
        /// 线路Id
        /// </summary>
        public int LineId { get; set; }
        /// <summary>
        /// 线路名称+标注
        /// </summary>
        public string LineName { get; set; }
        /// <summary>
        /// 出团日期
        /// </summary>	
        public DateTime OutDate { get; set; }
        /// <summary>
        /// 标准价 和价格表同步
        /// </summary>	
        public decimal Price { get; set; }
        /// <summary>
        /// 团计划Id
        /// </summary>
        public int TourId { get; set; }
        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }
        /// <summary>
        /// 资源Id
        /// </summary>
        public int QuotaId { get; set; }
        /// <summary>
        /// 计划名额
        /// </summary>	
        public int PlanQuota { get; set; }
        /// <summary>
        /// 可用名额
        /// </summary>	
        public int UseQuota { get; set; }
        /// <summary>
        /// 已用名额
        /// </summary>	
        public int UsedQuota { get; set; }
        /// <summary>
        /// 预留名额
        /// </summary>	
        public int HoldQuota { get; set; }
        /// <summary>
        /// 单房差
        /// </summary>	
        public decimal SingleRoom { get; set; }
        /// <summary>
        /// 特价让利
        /// </summary>	
        public decimal TeJiaFanLi { get; set; }
    }
}
