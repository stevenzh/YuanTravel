using System.Data;
using System.Text.RegularExpressions;
using System.Web;

namespace System
{
    /// <summary>
    /// 类型转换工具类
    /// </summary>
    public static class Convertor1
    {

        #region 转换成拼音

        private static string XML_FILE = HttpContext.Current.Server.MapPath("~/") + "WebRes/docs/PinYin.XML";

        /// <summary>
        /// 汉字转换PinYin
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ConvertPinYin(this string word)
        {
            return ConvertPinYin(word, word.Length);
        }

        public static string ConvertJPinYin(this string word)
        {
            var jpy = "";
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] >= 'A' && word[i] <= 'Z')
                {
                    jpy += word[i].ToString();
                }
            }
            return jpy;
        }

        /// <summary>
        /// 汉字转换成拼音
        /// </summary>
        /// <param name="word"></param>
        /// <param name="Cnt">几个字母</param>
        /// <returns></returns>
        public static string ConvertPinYin(this string word, int Cnt)
        {
            string pinYin = string.Empty;
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            for (int i = 0; i < Cnt; i++)
            {
                pinYin += GetPinYin(word[i].ToString());
            }
            return pinYin;
        }

        private static string GetPinYin(string word)
        {
            DataSet ds = new DataSet();
            string pinYin = string.Empty;
            ds.ReadXml(XML_FILE);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (ds.Tables[0].Rows[i]["chinese"].Equals(word))
                {
                    pinYin = ds.Tables[0].Rows[i]["english"].ToString();
                    break;
                }
            }
            System.Globalization.TextInfo tInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            return tInfo.ToTitleCase(pinYin.ToLower());
        }

        #endregion 转换成拼音
    }
}