using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.APIVModels.Req
{
    public class GetProductsRequest : BaseRequest
    {
        public int QueryDay { get; set; }

        /// <summary>
        /// 线路类型
        /// </summary>
        public int LineType { get; set; }
    }
}
