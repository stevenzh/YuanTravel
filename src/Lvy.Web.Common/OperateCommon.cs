using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arch.Common.Models;
using Arch.Common.Utils;

namespace Lvy.Web.Common
{
    public class OperateCommon
    {
        #region 汉字拼音转换
        public static string ConvertHanZiToPinYin(string word)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/XMLDocument/PinYin.XML");
            var tools = new XMLTools(path);
            return tools.ConvertPinYinTrim(word);
        }
        #endregion

        /// <summary>
        ///  读取配置文件发短信
        /// </summary>
        /// <param name="key">XML节点 例如：100 , 101a</param>
        /// <param name="strMobile">手机号码</param>
        /// <param name="strOrderNo">订单号</param>  
        public static void NewSendMsg(string key, string strMobile, string strOrderNo)
        {
            //if (Configs.isMsgOpen.ToLower() == "open")
            //{
                string path = System.Web.HttpContext.Current.Server.MapPath("~/XMLDocument/SendMsgData.xml");
                var tools = new XMLTools(path);
                string strContext = tools.GetDictionary("SendMsg", key).Value;    //XML 根节点

                if (strOrderNo.Length > 0)
                {
                    strContext = strContext.Replace("{$OrderCode}", strOrderNo);
                }
                //CCT.Message.SmsClient.SendMessage(strMobile, strContext);
            //}
        }
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="strMobile"></param>
        /// <param name="message"></param>
        public static void SendMsg(string strMobile, string message)
        {
            //if (Configs.isMsgOpen.ToLower() == "open")
            //{
            //   // CCT.Message.SmsClient.SendMessage(strMobile, message);
            //}
        }
        /// <summary>
        /// 获取xml的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static KeyValueBean GetXmlValue(string key, string value)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/XMLDocument/Dictionary.xml");
            var tools = new XMLTools(path);
            return tools.GetDictionary(key, value);
        }
    }
}
