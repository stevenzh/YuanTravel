using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Online
{
    public class RecommendVModel : BaseVModel
    {
        /// <summary>
        /// 线路Id
        /// </summary>
        public int LineId { get; set; }
        /// <summary>
        /// 线路名称
        /// </summary>
        public string LineName { get; set; }
        /// <summary>
        /// 行程天数
        /// </summary>	
        public int TravelDays { get; set; }
        /// <summary>
        /// 主题标签
        /// </summary>	
        public string Themes { get; set; }
        /// <summary>
        /// 线路特色
        /// </summary>	
        public string LineSpecial { get; set; }
        /// <summary>
        /// 供应商code
        /// </summary>
        public string CustomerCode { get; set; }
 
        /// <summary>
        /// 最低价
        ///  各团期的最低标准价
        /// </summary>
        public decimal MinPrice { get; set; }

        public List<RecommendTourVModel> RecommendTours { get; set; }
    }

    public class RecommendTourVModel
    {
        /// <summary>
        /// 线路Id
        /// </summary>
        public int LineId { get; set; }
        /// <summary>
        /// 线路名称+标注
        /// </summary>
        public string LineNameSign { get; set; }
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
