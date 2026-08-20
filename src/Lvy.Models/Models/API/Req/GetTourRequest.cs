using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.APIVModels.Req
{
    public class GetTourRequest : BaseRequest
    {
        /// <summary>
        /// 团编号
        /// </summary>
        public int TourId { get; set; }
    }
}
