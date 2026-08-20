using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Excel
{
    public class BusPointCountExcelVModel
    {
        [Description("序号")]
        public int RowIndex { get; set; }
        [Description("上车点名称")]
        public string BusPointName { get; set; }
        [Description("接送类型")]
        public string JieSongType { get; set; }
        [Description("订单数量")]
        public int OrderCount { get; set; }
        [Description("人数")]
        public int PeopleCount { get; set; }
        [Description("接价")]
        public decimal JiePrice { get; set; }
        [Description("送价")]
        public decimal SongPrice { get; set; }
        [Description("接送时间")]
        public string JieSongTime { get; set; }
    }
}
