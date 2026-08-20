using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class BatchEditTourVModel : BaseVModel
    {
        /// <summary>
        /// 线路
        /// </summary>
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 团期
        /// </summary>
        public TpTourPlanModel Tour { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public List<TpPriceModel> PriceList { get; set; }

        /// <summary>
        /// 不重复的团期（按线路+标注分组）
        /// </summary>
        public List<TpTourPlanModel> LineNameSignList { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public string BeginDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public string EndDate { get; set; }


        /// <summary>
        /// 所选择的星期
        /// </summary>
        public List<string> SelectedDays { get; set; }

        /// <summary>
        /// 星期列表
        /// </summary>
        public IEnumerable<KeyValueBean> DaysOfWeek
        {
            get
            {
                return new List<KeyValueBean>
                           {
                               new KeyValueBean {Key=DayOfWeek.Monday.ToString(),Value="周一"},
                               new KeyValueBean {Key=DayOfWeek.Tuesday.ToString(),Value="周二"},
                               new KeyValueBean {Key=DayOfWeek.Wednesday.ToString(),Value="周三"},
                               new KeyValueBean {Key=DayOfWeek.Thursday.ToString(),Value="周四"},
                               new KeyValueBean {Key=DayOfWeek.Friday.ToString(),Value="周五"},
                               new KeyValueBean {Key=DayOfWeek.Saturday.ToString(),Value="周六"},
                               new KeyValueBean {Key=DayOfWeek.Sunday.ToString(),Value="周日"}
                           };
            }
        }

        /// <summary>
        /// 单房差
        /// </summary>
        public decimal SingleRoom { get; set; }

        /// <summary>
        /// 单房差
        /// </summary>
        public decimal TeJiaFanLi { get; set; }
    }
}
