using Lvy.Models;
using Lvy.VModels.Product;
using System.Collections.Generic;

namespace Lvy.VModels.Online
{
    public class SearchProductVModel : BaseVModel
    {
        #region 查询条件

        /// <summary>
        /// 目的地导航
        /// </summary>
        public List<DestNavVModel> DestsNav { get; set; }

        public string LineId { get; set; }

        /// <summary>
        /// 线路类型
        /// </summary>
        public int LineType { get; set; }

        public string OutCity { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        public string ArriveDestName { get; set; }

        /// <summary>
        ///  出发日期 起
        /// </summary>
        public string MinOutDate { get; set; }

        /// <summary>
        /// 出发日期 止
        /// </summary>
        public string MaxOutDate { get; set; }

        /// <summary>
        /// 线路名称 + 标注
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Themes { get; set; }

        /// <summary>
        /// 出游天数
        /// </summary>
        public int TravelDays { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// 排序种类
        /// </summary>
        public Dictionary<string, KeyValueBean> OrderOption
        {
            get
            {
                Dictionary<string, KeyValueBean> dic = new Dictionary<string, KeyValueBean>();
                dic.Add("1", new KeyValueBean { Key = "tp.OutDate", Value = "出发日期" });
                dic.Add("2", new KeyValueBean { Key = "tp.Price ASC", Value = "价格从低到高" });
                dic.Add("3", new KeyValueBean { Key = "tp.Price DESC", Value = "价格从高到低" });
                //dic.Add("4", new KeyValueBean { Key = "tp.FanLi ASC", Value = "定金从低到高" });
                //dic.Add("5", new KeyValueBean { Key = "tp.FanLi DESC", Value = "定金从高到低" });
                dic.Add("6", new KeyValueBean { Key = "tq.UsedQuota ASC", Value = "销量从低到高" });
                dic.Add("7", new KeyValueBean { Key = "tq.UsedQuota DESC", Value = "销量从高到低" });
                return dic;
            }
        }

        #endregion 查询条件

        /// <summary>
        /// 产品分页对象
        /// </summary>
        public PagedList<TourInfoVModel> ProductPagedList { get; set; }
    }
}