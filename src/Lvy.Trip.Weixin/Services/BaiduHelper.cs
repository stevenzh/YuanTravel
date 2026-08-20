using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Wap.Site.Helpers
{
    public static class BaiduHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <returns></returns>
        public static LocationResultJson GeoConv(string lng, string lat)
        {
            string url = string.Format("http://api.map.baidu.com/geoconv/v1/?coords={0},{1}&from=3&to=5&ak={2}", lng, lat, "S0yxie593jr9DpgLwdSs7Mq3");
            string returnText = Senparc.Weixin.HttpUtility.RequestUtility.HttpGet(url, null);
            JavaScriptSerializer js = new JavaScriptSerializer();
            LocationResultJson result = js.Deserialize<LocationResultJson>(returnText);

            return result;
        }
    }

    public class LocationResultJson
    {
        public int status { get; set; }
        public List<LocationInfoJson> result { get; set; }
    }
    public class LocationInfoJson
    {
        /// <summary>
        /// 经度
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double y { get; set; }
    }
}