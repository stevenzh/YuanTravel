using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Online
{
    /// <summary>
    /// 含有线路的目的地
    /// </summary>
    public class DestNavVModel
    {

        /// <summary>
        /// 所在省 区  编号
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// 所在省名称
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// 线路类型
        /// </summary>
        public int LineType { get; set; }

        /// <summary>
        /// 目的地编号
        /// </summary>
        public string ArriveDest { get; set; }
        /// <summary>
        /// 目的地名称
        /// </summary>
        public string ArriveDestName { get; set; }

        /// <summary>
        /// 该目的地下是否有今天往后的团
        /// </summary>
        public bool IsHasTour { get; set; }

        /// <summary>
        /// 点击次数
        /// </summary>
        public int ClickCnt { get; set; }
    }


}
