using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class EditRouteVModel : BaseVModel
    {
        /// <summary>
        /// 线路Id
        /// </summary>
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 交通类型 1:汽车 2：火车 3：飞机 4：轮船 5：自驾  9：其他
        /// </summary>	
        public int TrafficType { get; set; }

        /// <summary>
        /// 行程
        /// </summary>
        public List<TpLineRouteModel> LineRoutes { get; set; }
    }
}
