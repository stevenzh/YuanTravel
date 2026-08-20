using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lvy.VModels
{
    public class CommonJsonResult
    {
        public string Code { get; set; }

        public string Message { get; set; }
        /// <summary>
        /// 执行结果
        ///     success,error
        /// </summary>
        public string State { get; set; }
    }

}
