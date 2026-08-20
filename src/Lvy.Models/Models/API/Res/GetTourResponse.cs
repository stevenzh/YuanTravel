using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.APIVModels.Res
{
    public class GetTourResponse : BaseResponse
    {
        public TpTourStateModel Tour { get; set; }
    }


    /// <summary>
    /// 团状态Model
    /// </summary>
    public class TpTourStateModel
    {
        /// <summary>
        /// 编号
        /// </summary>	
        public int TourId { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>	
        public int LineId { get; set; }


        /// <summary>
        /// 状态 0：无效 1：有效 2：下线 3:上线
        /// </summary>	
        public int TourState { get; set; }
        /// <summary>
        /// 团状态
        /// </summary>

        public int Source { get; set; }

        /// <summary>
        /// 线路状态
        /// </summary>

        public int LineState { get; set; }

        #region TpQuota

        /// <summary>
        /// 计划名额
        /// </summary>	
        public int PlanQuota { get; set; }

        /// <summary>
        /// 可用名额
        /// </summary>	
        public int UseQuota { get; set; }

        #endregion


    }

    /// <summary>
    /// 价格详情
    /// </summary>
    public class PriceInfo
    {
        /// <summary>
        /// 标准价
        /// </summary>
        public decimal StandPrice { get; set; }

        /// <summary>
        /// 标准结算价
        /// </summary>
        public decimal StandClearinPrice { get; set; }

        /// <summary>
        /// 儿童价
        /// </summary>
        public decimal KidsPrice { get; set; }

        /// <summary>
        /// 儿童结算价
        /// </summary>
        public decimal KidsClearinPrice { get; set; }

        /// <summary>
        /// 老人价
        /// </summary>
        public decimal AgedPrice { get; set; }

        /// <summary>
        /// 老人结算价
        /// </summary>
        public decimal AgedClearingPrice { get; set; }

    }

    public class TpPrice
    {
        public int Id { get; set; }

        public int PriceType { get; set; }

        public String PriceTypeName { get; set; }

        public decimal Price { get; set; }

        public decimal SettlePrice { get; set; }

        public int IsStandard { get; set; }
    }
}
