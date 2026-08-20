using Lvy.Models;
using Lvy.Models.ProductDB;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lvy.VModels.Finance
{
    public class BusPointCountVModel : BaseVModel
    {
        #region 查询条件

        /// <summary>
        /// 上车点名称
        /// </summary>
        public string BusPointName { get; set; }

        /// <summary>
        /// 接送类型
        /// 0：全部 1：接  2：送 3：接送
        /// </summary>
        public int JieSongType { get; set; }

        public List<KeyValueBean> JieSongTypes
        {
            get
            {
                return new List<KeyValueBean>
                             {
                                 new KeyValueBean{Key="1",Value="只接不送"},
                                 new KeyValueBean{Key="2",Value="只送不接"},
                                 new KeyValueBean{Key="3",Value="接/送"},
                             };
            }
        }

        /// <summary>
        /// 出发日期（起）
        /// </summary>
        public string BeginOutDate { get; set; }

        /// <summary>
        /// 出发日期（止）
        /// </summary>
        public string EndOutDate { get; set; }

        #endregion 查询条件

        public List<BusPointCountRow> BusPointCountList { get; set; }
    }

    /// <summary>
    /// 上车点统计列
    /// </summary>
    public class BusPointCountRow
    {
        public string BusPointJson { get; set; }

        public TpLineBusPointModel BusPointModel
        {
            get { return BusPointJson.IsNullOrEmpty() ? null : JsonSerializer.Deserialize<TpLineBusPointModel>(BusPointJson); }
        }

        ///// <summary>
        ///// 上车点名称
        ///// </summary>
        //public string BusPointName { get; set; }
        ///// <summary>
        ///// 接送类型
        ///// </summary>
        //public string JieSongType { get; set; }
        /// <summary>
        /// 订单数量
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// 人数
        /// </summary>
        public int PeopleCount { get; set; }

        ///// <summary>
        ///// 接送价
        ///// </summary>
        //public decimal JieSongPrice { get; set; }
        ///// <summary>
        ///// 接送时间
        ///// </summary>
        //public string JieSongTime { get; set; }
    }
}