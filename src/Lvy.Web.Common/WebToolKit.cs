using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Linq;
using Lvy.Models;

namespace Lvy.Web.Common
{
    public static class WebToolKit
    {
        /// <summary>
        ///  获取客户端IP
        /// </summary>
        /// <returns></returns>
        public static string GetClientIp()
        {
            string result = String.Empty;
            result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            if (null == result || result == String.Empty)
            {
                return "0.0.0.0";
            }
            return result;
        }

        public static string GetFileMedia(string fileExtend)
        {
            string[] images = { ".png", ".jpg", ".bmp", ".gif", "jpeg" };
            string[] documents = { ".doc", ".docx", ".xls", ".xlsx", ".pdf", ".wps" };
            string[] voices = { ".mp3", ".wma", ".wav", ".ogg" };
            string[] videos = { ".mp4", ".avi", ".mpg", ".mpeg", ".mov", ".mkv", ".wmv", ".asf" };

            if (images.Contains(fileExtend))
            {
                return MediaType.image.ToString();
            }
            else if (voices.Contains(fileExtend))
            {
                return MediaType.voice.ToString();
            }
            else if (videos.Contains(fileExtend))
            {
                return MediaType.voice.ToString();
            }

            return MediaType.document.ToString();
        }

        #region JSON序列化反序列化

        public static string ToJsonSerialize<T>(this T obj)
        {
            var json = new JavaScriptSerializer();
            return json.Serialize(obj);
        }

        public static T ToJsonDeserialize<T>(this string str)
        {
            var json = new JavaScriptSerializer();
            return json.Deserialize<T>(str);
        }

        #endregion

    }
}
