using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Arch.Common.Utils
{
    public class StringUtils
    {

        /// <summary>
        /// 判断字段是否为空
        /// </summary>
        /// <param name="str"></param>
        /// <returns>true:空 </returns>
        public static bool IsTrimEmpty(string str)
        {
            if (str.Trim().Length == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 取得最小的日期
        /// </summary>
        /// <returns></returns>
        public static DateTime GetMinDate()
        {
            string temp = "1900-01-01";
            return Convert.ToDateTime(temp);
        }

        public static string GetTodayLastTime(DateTime dt)
        {
            return DateFormat(dt) + " 23:59:00";
        }

        /// <summary>
        /// format style: yyyy-mm-dd
        /// 日期类型转换成字符串类型
        /// </summary>
        public static string DateFormat(DateTime date)
        {
            string strDate = String.Format("{0:d}", date);
            return strDate.Replace('/', '-');
        }

        public static string DateFormat(object date)
        {
            string strDate = String.Format("{0:d}", date);
            return strDate.Replace('/', '-');
        }

        /// <summary>
        /// 清除空格
        /// </summary>
        public static string TrimString(object str)
        {

            if (str == null)
            {
                return string.Empty;
            }
            return str.ToString().Trim();
        }

        public static string GetDateWeek(DateTime datetime, string Type)
        {
            string dt = datetime.DayOfWeek.ToString();
            string week = "";
            switch (dt)
            {
                case "Monday":
                    week = "星期一";
                    break;
                case "Tuesday":
                    week = "星期二";
                    break;
                case "Wednesday":
                    week = "星期三";
                    break;
                case "Thursday":
                    week = "星期四";
                    break;
                case "Friday":
                    week = "星期五";
                    break;
                case "Saturday":
                    week = "星期六";
                    break;
                case "Sunday":
                    week = "星期日";
                    break;
            }
            return (Type.Equals("s") ? week.Substring(2) : week);
        }

        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="maxNum"></param>
        /// <returns></returns>
        public static string RandomNum(int maxNum)
        {
            Random random = new Random();
            string value = "";
            for (int i = 0; i < maxNum; i++)
            {
                value = value + "9";
            }
            value = random.Next(1, Convert.ToInt32(value)).ToString().PadLeft(maxNum, '0');
            return value;
        }

        /// <summary>
        /// 格式化字符串 
        /// </summary>
        /// <returns></returns>
        public static string FormatString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            else
            {
                return obj.ToString().Trim();
            }
        }

        /// <summary>
        /// 通过身份证得到生日
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        public static string GetBirthdayByIDCard(string idCard)
        {

            string birthday = string.Empty;
            if (idCard.Length == 18)
            {
                birthday = idCard.Substring(6, 8);
            }
            else if (idCard.Length == 15)
            {
                birthday = "19" + idCard.Substring(6, 6);
            }

            birthday = string.Format("{0}-{1}-{2}", birthday.Substring(0, 4), birthday.Substring(4, 2), birthday.Substring(6));

            if (!TypeValidate.IsDate(birthday))
            {
                return string.Empty;
            }

            return birthday;
        }

    }
}
