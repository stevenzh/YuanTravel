using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Online
{
    public class HotTourVModel : BaseVModel
    {
        /// <summary>
        /// 线路类型
        /// 1，周边游；2，国内游；3，出境游；5，自驾游 6，自由行  7,签证  8，邮轮
        /// </summary>
        public int LineType { get; set; }
        /// <summary>
        /// 线路Id
        /// </summary>
        public string LineId { get; set; }
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

        public string LogoPath { get; set; }

        /// <summary>
        /// 价格列表
        /// </summary>
        public List<TpPriceModel> PriceList { get; set; }
    }
}
