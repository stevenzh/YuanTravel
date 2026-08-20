using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.Models
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class BusSeatModel
    {

        /// <summary>
        /// 座位号
        /// </summary>
        public string No { get; set; }
        /// <summary>
        /// 座位状态 1.未占，2.已占，3.锁定
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 座位状态的style class
        /// </summary>

        public string ChangeStateClass()
        {

            switch (State)
            {
                case 1:
                    return "gray";
                case 2:
                    return "red";
                case 3:
                    return "blue";
                default:
                    throw new Exception("座位状态异常！！！");
            }

        }
    }
}
