using Lvy.Models;
using Lvy.Models.HotelDB;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.VModels.Hotel
{
    /// <summary>
    /// 酒店后台使用
    /// </summary>
    public class HotelVModel : BaseVModel
    {
        public HotelVModel()
        {
            if (HotelModel == null)
                HotelModel = new HotelModel();
            if (Hotels == null)
                Hotels = new PagedList<HotelModel>();

            this.SortKey = 2;
        }

        /// <summary>
        /// 查询对象
        /// </summary>
        public HotelModel HotelModel { get; set; }

        /// <summary>
        /// 排序集合
        /// </summary>
        public Dictionary<int, KeyValueBean> SortCollection
        {
            get
            {
                Dictionary<int, KeyValueBean> dic = new Dictionary<int, KeyValueBean>();
                dic.Add(1, new KeyValueBean { Key = "h.HotelCode ASC", Value = "编号升序" });
                dic.Add(2, new KeyValueBean { Key = "h.HotelCode DESC", Value = "编号降序" });
                dic.Add(3, new KeyValueBean { Key = "h.HotelName ASC", Value = "姓名升序" });
                dic.Add(4, new KeyValueBean { Key = "h.HotelName DESC", Value = "姓名降序" });

                return dic;
            }
        }

        /// <summary>
        /// 排序键值对
        /// </summary>
        public List<KeyValueBean> SortKeyValueBean
        {
            get
            {
                return SortCollection.Select(dic => new KeyValueBean { Key = dic.Key.ToString(), Value = dic.Value.Value }).ToList();
            }
        }

        /// <summary>
        /// 排序方式
        /// </summary>
        public int SortKey { get; set; }

        /// <summary>
        /// 查询列表
        /// </summary>
        public PagedList<HotelModel> Hotels { get; set; }
    }
}