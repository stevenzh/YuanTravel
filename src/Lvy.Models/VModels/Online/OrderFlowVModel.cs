using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.VModels.Online
{
    public class OrderFlowVModel : BaseVModel
    {

        public int OrderId { get; set; }

        public int TourId { get; set; }


        public string LineName { get; set; }

        public DateTime OutDate { get; set; }

        public string OutDateFormat
        {
            get
            {
                var dt = OutDate.ToDateFormat().Substring(5);
                return dt + "出发";
            }
        }

        public DateTime CreatedTime { get; set; }


        public string NearTime
        {
            get
            {
                string strRet = String.Empty;
                TimeSpan ts = DateTime.Now - CreatedTime;
                if (ts.Days <= 0)
                {
                    if (ts.Hours <= 0)
                        strRet = string.Format("{0}之前", ts.Minutes <= 0 ? "1分钟" : ts.Minutes.ToString() + "分钟");
                    else
                        strRet = string.Format("{0}之前", ts.Hours.ToString() + "小时");
                }
                else
                    strRet = string.Format("{0}之前", ts.Days <= 6 ? ts.Days.ToString() + "天" : "1周");

                return strRet + "产生了一张订单";
            }
        }


    }




}
