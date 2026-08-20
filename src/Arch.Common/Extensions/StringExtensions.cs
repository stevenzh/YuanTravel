using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace System
{
    /// <summary>
    ///  字符串类的扩展
    /// </summary>
    public static class StringExtensions
    {
        #region 文字处理
        /// <summary>
        ///  将数组转换成字符串，并以joinText为分隔符
        /// </summary>
        /// <param name="values">要转换的数组</param>
        /// <param name="joinText">分隔符</param>
        /// <returns></returns>
        public static string Join(this string[] values, string joinText)
        {
            var result = new StringBuilder();

            if (values.Length == 0)
                return string.Empty;

            result.Append(values[0]);

            for (int i = 1; i < values.Length; i++)
            {
                result.Append(joinText);
                result.Append(values[i]);
            }

            return result.ToString();
        }

        /// <summary>
        ///  将string字符串转换为集合
        /// </summary>
        /// <param name="text">要转换的字符串</param>
        /// <param name="sepeater">分割字符串的字符</param>
        /// <returns>字符串集合</returns>
        public static List<string> GetStringToList(this string text, char sepeater)
        {
            string[] ss = text.Split(sepeater);
            return ss.Where(s => !string.IsNullOrEmpty(s) && s != sepeater.ToString()).ToList();
        }

        /// <summary>
        ///  字符串是否为空
        /// </summary>
        /// <returns>为空返回false.否则返回true</returns>
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }


        /// <summary>
        ///  字符串格式化 
        ///  exmple:
        ///  "{0},{1}".With("aaa","bbb")
        /// </summary>
        /// <param name="target"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string With(this string target, params object[] args)
        {
            return string.Format(target, args);
        }

        /// <summary>
        ///  省略文字，以省略号代替
        /// </summary>
        /// <returns></returns>
        public static string TrimWithElipsis(this string text, int length = 50)
        {
            if (text.IsNullOrEmpty())
                return string.Empty;

            if (text.Length <= length) return text;
            return text.Substring(0, length) + "...";
        }
        /// <summary>
        ///  去掉字符串前后空格
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToTrim(this string text)
        {
            if (text == null)
                return null;
            else
                return text.Trim();
        }

        /// <summary>
        ///  转换成中文拼音
        /// </summary>
        /// <param name="text"></param>
        /// <returns>中文拼音</returns>
        public static string GetChineseSpell(this string input)
        {
            return null; // 未实现
        }
        /// <summary>
        /// 转全角(SBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>全角字符串</returns>
        public static string ToSBC(this string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }
        /// <summary>
        /// 转半角(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        public static string ToDBC(this string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }


        #endregion

        /// <summary>
        /// 删除最后结尾的指定字符后的字符
        /// </summary>
        public static string DelLastChar(this string str, string strchar = ",")
        {
            return str.Substring(0, str.LastIndexOf(strchar));
        }

        #region stringBuilder

        public static void NewLine(this StringBuilder stringBuilder)
        {
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(Environment.NewLine);
        }

        public static byte[] AsBytes(this StringBuilder stringBuilder)
        {

            //有汉字的场合，需要转换成utf-8
            UTF8Encoding utf8 = new UTF8Encoding();

            Byte[] encodedBytes = utf8.GetBytes(stringBuilder.ToString());

            return encodedBytes;
        }




        #endregion

    }
}
