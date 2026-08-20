using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// 类型验证工具类
    /// </summary>
    public static class TypeValidate
    {
        /// <summary>
        /// 判断字符串是否是decimal类型
        /// </summary>
        /// <param name="input">要转化的值</param>
        /// <returns></returns>
        public static bool IsDecimal(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, "^[0-9]+[.]?[0-9]+$");
        }
        /// <summary>
        /// 判断字符串是否是小数
        /// </summary>
        /// <param name="input">要转化的字符串</param>
        /// <returns>true or false</returns>
        public static bool IsDecimalSign(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, "^[+-]?[0-9]+[.]?[0-9]+$");
        }


        /// <summary>
        /// 检测是否符合email格式
        /// </summary>
        /// <param name="strEmail">要判断的email字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsEmail(this string strEmail)
        {
            //if (string.IsNullOrEmpty(strEmail))
            //{
            //    return false;
            //}
            return Regex.IsMatch(strEmail, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        /// <summary>
        /// 检测是否符合IP格式
        /// </summary>
        /// <param name="strIp">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsIP(this string strIp)
        {
            return (!string.IsNullOrEmpty(strIp) &&
                    Regex.IsMatch(strIp.Trim(),
                                  @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$"));
        }

        /// <summary>
        /// 判断给定的字符串(strNumber)是否是数值型
        /// </summary>
        /// <param name="strNumber">要确认的字符串</param>
        /// <returns>是则返加true 不是则返回 false</returns>
        public static bool IsNumber(this string strNumber)
        {
            return Regex.IsMatch(strNumber, "^[0-9]+$");
        }
        /// <summary>
        /// 匹配字符串是否是数值型。正负
        /// </summary>
        /// <param name="input">要匹配的字符串</param>
        /// <returns>true or false</returns>
        public static bool IsNumberSign(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, "^[+-]?[0-9]+$");
        }

        /// <summary>
        /// 检测是否符合邮编格式
        /// </summary>
        /// <param name="postCode"></param>
        /// <returns></returns>
        public static bool IsPostCode(this string postCode)
        {
            return Regex.IsMatch(postCode, @"^\d{6}$");
        }


        /// <summary>
        /// 检测是否符合身份证号码格式
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsIdentityNumber(this string num)
        {
            return Regex.IsMatch(num, @"^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$");
        }

        /// <summary>
        /// 检测是否符合时间格式
        /// </summary>
        /// <returns></returns>
        public static bool IsTime(this string timeval)
        {
            return Regex.IsMatch(timeval,
                                 @"20\d{2}\-[0-1]{1,2}\-[0-3]?[0-9]?(\s*((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?))?");
        }

        /// <summary>
        /// 检测是否符合url格式,前面必需含有http://
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsURL(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            return Regex.IsMatch(url, @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$");
        }

        /// <summary>
        /// 检测是否符合电话格式
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static bool IsPhoneNumber(this string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^(\(\d{3}\)|\d{3}-)?\d{7,8}$");
        }
        /// <summary>
        ///  是否是手机
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool IsMobile(this string mobile)
        {
            //return Regex.IsMatch(mobile, @"^((\(\d{3}\))|(\d{3}\-))?13\d{9}$");
            if (mobile.Length < 11)
                return false;
            if (!IsNumber(mobile))
                return false;

            mobile = mobile.Substring(mobile.Length - 11, 11);
            string temp = mobile.Substring(0, 2);
            if (temp == "13" || temp == "15" || temp == "18")
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否是日期
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsDate(this string date)
        {
            if (string.IsNullOrEmpty(date))
                return false;

            return Regex.IsMatch(date,
                                 @"((^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(10|12|0?[13578])([-\/\._])(3[01]|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(11|0?[469])([-\/\._])(30|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(0?2)([-\/\._])(2[0-8]|1[0-9]|0?[1-9])$)|(^([2468][048]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([3579][26]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][13579][26])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][13579][26])([-\/\._])(0?2)([-\/\._])(29)$))");
        }
    }
}