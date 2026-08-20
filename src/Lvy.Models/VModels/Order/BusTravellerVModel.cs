using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Order
{
    public class BusTravellerVModel
    {

        /// <summary>
        /// 报价说明 成人   小孩
        /// </summary>	
        public string PriceContent { get; set; }
        /// <summary>
        /// 价格
        /// </summary>	
        public decimal Price { get; set; }
        /// <summary>
        /// 小费
        /// </summary>	
        public decimal Tips { get; set; }
        /// <summary>
        /// 单房差
        /// </summary>	
        public decimal SingleRoom { get; set; }
        /// <summary>
        /// 特价让利
        /// </summary>	
        public decimal TeJiaFanLi { get; set; }
        /// <summary>
        /// 接价
        /// </summary>	
        public decimal JiePrice { get; set; }
        /// <summary>
        /// 送价
        /// </summary>	
        public decimal SongPrice { get; set; }
        /// <summary>
        /// 自费
        /// </summary>	
        public decimal ZiFei { get; set; }
        /// <summary>
        /// 折让
        /// </summary>	
        public decimal FanLi { get; set; }
        /// <summary>
        /// 人数
        /// </summary>
        public string PeopleCount { get; set; }
        /// <summary>
        /// 游客状态
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 售价
        /// </summary>
        public decimal ShouJia
        {
            get { return Price - TeJiaFanLi; }
        }
        /// <summary>
        /// 应收款项
        /// </summary>
        public decimal GroupYsPrice { get; set; }

        ///// <summary>
        ///// 单个报价的总计
        ///// </summary>
        //public decimal TotalPriceType
        //{
        //    get
        //    {
        //        return ((Price + SingleRoom + JiePrice + SongPrice + ZiFei) - TeJiaFanLi - Fanli) * PeopleCount.ToInt();
        //    }
        //}

        ///// <summary>
        ///// 出团通知书去掉返利的个人总计
        ///// </summary>
        //public decimal TolPrice
        //{
        //    get
        //    {
        //        return (Price + SingleRoom + JiePrice + SongPrice + ZiFei - TeJiaFanLi - Fanli) * PeopleCount.ToInt();
        //    }
        //}

    }
}
