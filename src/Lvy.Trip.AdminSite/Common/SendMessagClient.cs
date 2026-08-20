using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Lvy.Trip.AdminSite.SendWeixinMessage;

namespace Lvy.Trip.Common
{
    /// <summary>
    /// 微信消息发送你
    /// </summary>
    public class SendMessagClient
    {

        /// <summary>
        /// 发送微信消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string SendMessage(string openId, string content)
        {
            SendWeixinMsessageSoapClient client = new SendWeixinMsessageSoapClient();
            string sdst = DateTime.Now.ToShortDateString();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + sdst);
            string sign = Convert.ToBase64String(plainTextBytes);
            return client.SendMessage(openId, content, sdst, sign);
        }

        /// <summary>
        /// 订单状态改变
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="template"></param>
        /// <param name="first"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="param3"></param>
        /// <param name="param4"></param>
        /// <param name="param5"></param>
        /// <param name="ramark"></param>
        /// <returns></returns>
        public static string SendTemplateMessage(string openId, string template, string first, string param1, string param2, string param3,
            string param4, string param5, string ramark)
        {
            ILog _logger = LogManager.GetLogger(typeof(SendMessagClient));

            try
            {
                SendWeixinMsessageSoapClient client = new SendWeixinMsessageSoapClient();
                string sdst = DateTime.Now.ToShortDateString();
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + sdst);
                string sign = Convert.ToBase64String(plainTextBytes);
                return client.SendTemplateMessage(openId, template, first, param1, param2, param3, param4, param5, ramark, sdst, sign);

            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
            }

            return "";
        }

        /// <summary>
        /// 客户消息提醒
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="template"></param>
        /// <param name="nick"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string SendChatMessage(string openId, string template, string nick, string content)
        {
            SendWeixinMsessageSoapClient client = new SendWeixinMsessageSoapClient();
            string sdst = DateTime.Now.ToShortDateString();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + sdst);
            string sign = Convert.ToBase64String(plainTextBytes);
            return client.SendChatMessage(openId, template, nick, content, sdst, sign);
        }

        /// <summary>
        /// 二维码创建
        /// </summary>
        /// <param name="sceneId"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        public static string CreateQrCode(string sceneId, string expireSeconds)
        {
            SendWeixinMsessageSoapClient client = new SendWeixinMsessageSoapClient();
            return client.CreateQrCode(sceneId, expireSeconds);
        }

        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static string SendImage(string openId, string filePath)
        {
            SendWeixinMsessageSoapClient client = new SendWeixinMsessageSoapClient();
            string sdst = DateTime.Now.ToShortDateString();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + sdst);
            string sign = Convert.ToBase64String(plainTextBytes);
            return client.SendImage(openId, filePath, sdst, sign);
        }
    }
}
