using System.Data;
using System.Text.RegularExpressions;
using System.Web;

namespace System
{
    /// <summary>
    /// 类型转换工具类
    /// </summary>
    public static class Convertor
    {
        #region ToInt

        /// <summary>
        /// 字符串转换数字为空的情况下返回-1
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue"></param>
        /// <returns>返回转换后的值</returns>
        public static int ToInt(this string value, int defaultValue = -1)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            if (!value.IsNumber())
                throw new Exception("无法转换成数字类型！");
            return Convert.ToInt32(value.Trim());
        }

        /// <summary>
        /// 字符串转换数字
        /// </summary>
        public static int ToInt(this object value)
        {
            return ToInt(value.ToString());
        }

        /// <summary>
        /// 字符串转换数字
        /// </summary>
        public static int ToInt(this decimal value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 去除decimal数值类型的小数位
        /// </summary>
        /// <param name="value"> </param>
        /// <returns>已经去除后的文字</returns>
        public static int ToInteger(this decimal value)
        {
            return Convert.ToInt32(value);
        }

        #endregion ToInt

        #region ToDecimal

        /// <summary>
        /// 字符串转换金额
        /// </summary>
        public static Decimal ToDecimal(this object value)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                return 0;
            }
            return Convert.ToDecimal(value);
        }

        /// <summary>
        /// 字符串转换金额为空返回0
        /// </summary>
        /// <param name="value">需要转换的值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>

        public static Decimal ToDecimal(this string value, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return decimal.Round(Convert.ToDecimal(value), 2);
        }

        #endregion ToDecimal

        #region ToBool

        /// <summary>
        /// 字符串转换为Bool
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>false</returns>
        public static bool ToBool(this string value)
        {
            return (value == "Y" || value == "0") && false;
        }

        /// <summary>
        /// 对象转换Bool
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns></returns>
        public static bool ToBool(this object value)
        {
            if (value == null)
                return false;

            return ToBool(Convert.ToInt16(value));
        }

        /// <summary>
        /// int转换为bool
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns></returns>
        public static bool ToBool(this int value)
        {
            return value != 0;
        }

        #endregion ToBool

        #region ToGuid

        /// <summary>
        /// 字符串转化为guid类型
        /// </summary>
        /// <param name="value">字符串的值</param>
        /// <returns></returns>
        public static Guid ToGuid(this string value)
        {
            return new Guid(value);
        }

        #endregion ToGuid

        #region 日期时间

        /// <summary>
        /// format style: 24hh:mm:ss
        /// 时间类型转换成字符串类型
        /// </summary>
        /// <param name="time">所要转换的时间</param>
        /// <returns>HH:mm:ss形式的时间</returns>
        public static string ToTimeFormat(this object time)
        {
            string strTime = string.Format("{0:HH:mm:ss}", TimeSpan.Parse(time.ToString()));
            return strTime;
        }

        /// <summary>
        /// 日期类型转换
        /// </summary>
        /// <param name="date">要转换的日期</param>
        /// <returns>日期类型的日期</returns>
        public static DateTime ToDateTime(this object date)
        {
            DateTime dateTime;
            if (DateTime.TryParse(date.ToString(), out dateTime))
            {
                return DateTime.Parse(date.ToString());
            }
            return default(DateTime);
        }

        /// <summary>
        /// 日期类型转换(NULL可)
        /// </summary>
        /// <param name="date">要转换的日期，可以为空！！</param>
        public static DateTime? ToDateTimeOrNull(this object date)
        {
            if (date == null)
                return null;
            DateTime dateTime;
            if (DateTime.TryParse(date.ToString(), out dateTime))
                return DateTime.Parse(date.ToString());
            return null;
        }

        /// <summary>
        /// 日期类型转换(NULL可)
        /// </summary>
        /// <param name="date">字符串类型的参数</param>
        /// <returns>日期类型</returns>
        public static DateTime? ToDateTimeOrNull(this string date)
        {
            if (string.IsNullOrEmpty(date))
                return null;
            return Convert.ToDateTime(date);
        }

        /// <summary>
        /// 把秒转换成分钟
        /// </summary>
        /// <param name="second">秒</param>
        /// <returns>分</returns>
        public static int ToMinute(this int second)
        {
            decimal mm = second / (decimal)60;
            return Convert.ToInt32(Math.Ceiling(mm));
        }

        /// <summary>
        /// format style: yyyy-mm-dd
        /// 日期类型转换成字符串类型
        /// </summary>
        public static string ToDateFormat(this object date)
        {
            string strDate = string.Format("{0:yyyy-MM-dd}", date);

            return strDate;
        }

        /// <summary>
        /// format style: yyyy-mm-dd
        /// 日期类型转换成字符串类型
        /// 如果=null，返回""
        /// </summary>
        public static string ToDateFormat(this DateTime? date)
        {
            if (date == null)
            {
                return "";
            }
            string strDate = string.Format("{0:yyyy-MM-dd}", date.Value);
            return strDate;
        }

        /// <summary>
        ///  转换周期
        /// </summary>
        /// <param name="dayOfWeek">转换的参数dayofweek</param>
        /// <returns>数字形式的一周中的某天</returns>
        public static int ToWeek(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;

                case DayOfWeek.Tuesday:
                    return 2;

                case DayOfWeek.Wednesday:
                    return 3;

                case DayOfWeek.Thursday:
                    return 4;

                case DayOfWeek.Friday:
                    return 5;

                case DayOfWeek.Saturday:
                    return 6;

                case DayOfWeek.Sunday:
                    return 7;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// 星期转换
        /// </summary>
        /// <param name="dayOfWeek">要转换的dayofweek形式的参数</param>
        /// <returns>文字形式的一周中的某天</returns>
        public static string ToWeekCn(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "周一";

                case DayOfWeek.Tuesday:
                    return "周二";

                case DayOfWeek.Wednesday:
                    return "周三";

                case DayOfWeek.Thursday:
                    return "周四";

                case DayOfWeek.Friday:
                    return "周五";

                case DayOfWeek.Saturday:
                    return "周六";

                case DayOfWeek.Sunday:
                    return "周日";

                default:
                    return "周*";
            }
        }

        /// <summary>
        /// 获取两个时间的时间差
        /// </summary>
        /// <param name="dateTime1">第一个时间</param>
        /// <param name="dateTime2">第二个时间</param>
        /// <returns>时间差</returns>
        public static int DateTimeDiff(this DateTime dateTime1, DateTime dateTime2)
        {
            var ts1 = new TimeSpan(dateTime1.Ticks);
            var ts2 = new TimeSpan(dateTime2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            return ts.Days;
        }

        /// <summary>
        /// 计算2个日期相差的月份
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static int CalcMonthDiff(this DateTime d1, DateTime d2)
        {
            DateTime max = d1 > d2 ? d1 : d2;
            DateTime min = d1 > d2 ? d2 : d1;

            int yeardiff = max.Year - min.Year;
            int monthdiff = max.Month - min.Month;

            return yeardiff * 12 + monthdiff + 1;
        }

        #endregion 日期时间

        /// <summary>
        /// 去除HTML标记
        /// </summary>
        /// <param name="html"> </param>
        /// <param name="Htmlstring"> </param>
        /// <returns>已经去除后的文字</returns>
        public static string ToNoHTML(this string html, bool keepLinebreak = false)
        {
            if (html.IsNullOrEmpty())
                return "";

            //删除脚本

            html = Regex.Replace(html, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);

            //删除HTML
            if (keepLinebreak)
            {
                html = Regex.Replace(html, @"<p.*>", "", RegexOptions.IgnoreCase);
                html = Regex.Replace(html, @"</p>", "<br/>", RegexOptions.IgnoreCase);
                html = Regex.Replace(html, @"<[^p][^[br]][^>]+>", "", RegexOptions.IgnoreCase);
            }
            else
            {
                html = Regex.Replace(html, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            }

            html = Regex.Replace(html, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"-->", "", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"<!--.*", "", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            html.Replace("<", "");

            html.Replace(">", "");

            html.Replace("\r\n", "");
            return html;
        }

        #region 转换成拼音

        //private static string XML_FILE = HttpContext.Current.Server.MapPath("~/") + "WebRes/docs/PinYin.XML";

        /// <summary>
        /// 汉字转换PinYin
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        //public static string ConvertPinYin(this string word, string XML_FILE)
        //{
        //    return ConvertPinYin(word, word.Length, XML_FILE);
        //}

        //public static string ConvertJPinYin(this string word)
        //{
        //    var jpy = "";
        //    for (int i = 0; i < word.Length; i++)
        //    {
        //        if (word[i] >= 'A' && word[i] <= 'Z')
        //        {
        //            jpy += word[i].ToString();
        //        }
        //    }
        //    return jpy;
        //}

        /// <summary>
        /// 汉字转换成拼音
        /// </summary>
        /// <param name="word"></param>
        /// <param name="Cnt">几个字母</param>
        /// <returns></returns>
        //public static string ConvertPinYin(this string word, int Cnt, string XML_FILE)
        //{
        //    string pinYin = string.Empty;
        //    if (string.IsNullOrEmpty(word))
        //        return string.Empty;

        //    for (int i = 0; i < Cnt; i++)
        //    {
        //        pinYin += GetPinYin(word[i].ToString(), XML_FILE);
        //    }
        //    return pinYin;
        //}

        //private static string GetPinYin(string word, string XML_FILE)
        //{
        //    DataSet ds = new DataSet();
        //    string pinYin = string.Empty;
        //    ds.ReadXml(XML_FILE);
        //    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //    {
        //        if (ds.Tables[0].Rows[i]["chinese"].Equals(word))
        //        {
        //            pinYin = ds.Tables[0].Rows[i]["english"].ToString();
        //            break;
        //        }
        //    }
        //    System.Globalization.TextInfo tInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
        //    return tInfo.ToTitleCase(pinYin.ToLower());
        //}

        #endregion 转换成拼音
    }
}