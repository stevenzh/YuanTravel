using Lvy.Trip.Weixin.Models;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.QrCode;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using System;
using System.Web.Configuration;
using System.Web.Services;

namespace Lvy.Trip.Weixin
{
    /// <summary>
    /// Summary description for SendWeixinMsessage
    /// </summary>
    [WebService(Namespace = "http://www.sh-cct.cn/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    // [System.Web.Script.Services.ScriptService]
    public class SendWeixinMsessage : System.Web.Services.WebService
    {
        private static readonly string AppID = WebConfigurationManager.AppSettings["WeixinAppId"];
        private static readonly string Secret = WebConfigurationManager.AppSettings["WeixinAppSecret"];

        /// <summary>
        /// 回复客户消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [WebMethod]
        public string SendMessage(string openId, string content, string timeStamp, string signed)
        {
            // 验证
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + timeStamp);
            var time = DateTime.Parse(timeStamp);
            string sign = Convert.ToBase64String(plainTextBytes);

            if (DateTime.Now.Year == time.Year && DateTime.Now.Month == time.Month
                && DateTime.Now.Day == time.Day && sign == signed)
            {
                var accessToken = AccessTokenContainer.TryGetAccessToken(AppID, Secret);
                var result = CustomApi.SendText(accessToken, openId, content);
                if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
                    return "1";
                else
                    return "0";
            }

            return "0";
        }

        /// <summary>
        /// 发送图片给客户
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="filePath"></param>
        /// <param name="timeStamp"></param>
        /// <param name="signed"></param>
        /// <returns></returns>
        [WebMethod]
        public string SendImage(string openId, string filePath, string timeStamp, string signed)
        {
            // 验证
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + timeStamp);
            var time = DateTime.Parse(timeStamp);
            string sign = Convert.ToBase64String(plainTextBytes);

            if (DateTime.Now.Year == time.Year && DateTime.Now.Month == time.Month
                && DateTime.Now.Day == time.Day && sign == signed)
            {
                var accessToken = AccessTokenContainer.TryGetAccessToken(AppID, Secret);
                var type = UploadMediaFileType.image;
                var result1 = MediaApi.UploadTemporaryMedia(accessToken, type, filePath);
                // 发送消息
                var result = CustomApi.SendImage(accessToken, openId, result1.media_id);
                if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
                    return "1";
                else
                    return "0";
            }

            return "0";
        }

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="template"></param>
        /// <param name="first">昵称</param>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <param name="param3">参数3</param>
        /// <param name="param4">参数4</param>
        /// <param name="param5">参数5</param>
        /// <param name="signed"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        [WebMethod]
        public string SendTemplateMessage(string openId, string template, string first, string param1, string param2, string param3,
            string param4, string param5, string remark, string timeStamp, string signed)
        {
            // 验证
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + timeStamp);
            var time = DateTime.Parse(timeStamp);
            string sign = Convert.ToBase64String(plainTextBytes);

            if (DateTime.Now.Year == time.Year && DateTime.Now.Month == time.Month && DateTime.Now.Day == time.Day && sign == signed)
            {
                var accessToken = AccessTokenContainer.TryGetAccessToken(AppID, Secret);
                SendTemplateMessageResult result = null;
                if (template.Equals("gunCr4m5dY0ftvk7Nfi9izhz6YldziliEUcE6LPmMJQ"))  
                {
                    // 账号登录提醒
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem(first),
                        keyword1 = new TemplateDataItem(param1),
                        keyword2 = new TemplateDataItem(param2),
                        keyword3 = new TemplateDataItem(param3),
                        remark = new TemplateDataItem(remark)
                    };
                    //string url = "http://yuanwx.sh-cct.cn/booking/details/" + param1;
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";

                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                else if (template.Equals("8i7VY_GnnYnvTfmDRmntS079TzfJK2KmXV3LUOeOHM0"))
                {
                    // 订单状态变更通知
                    var testData = new OrderWeixinData()
                    {
                        first = new TemplateDataItem(first),
                        orderId = new TemplateDataItem(param1),
                        productName = new TemplateDataItem(param2),
                        orderPrice = new TemplateDataItem(param3),
                        orderStatus = new TemplateDataItem(param4),
                        remark = new TemplateDataItem(remark)
                    };
                    //string url = "http://yuanwx.sh-cct.cn/booking/details/" + param1;
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fbooking%2Fdetails%2F" + param1 + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";

                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                else if (template.Equals("jFkZkkv74K27HcZ6xnyaNV5elqSX7IdcYQHI4Nus170"))
                {
                    // 新订单生成通知  开单
                    var testData = new OrderData()
                    {
                        first = new TemplateDataItem(first),
                        OrderId = new TemplateDataItem(param1),
                        ProductName = new TemplateDataItem(param2),
                        ProductId = new TemplateDataItem(param3),
                        remark = new TemplateDataItem(remark)
                    };
                    //   string url = "http://yuan.sh-cct.cn/booking/details/" + param1;
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fbooking%2Fdetails%2F" + param1 + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";

                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                else if (template.Equals("dbtBNSrwDX2qG4qXu9pAGTj172O5_ZqvVkPxQlvnpDw"))
                {
                    ///新客户审核提醒
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem(first),
                        keyword1 = new TemplateDataItem(param1),
                        keyword2 = new TemplateDataItem(param2),
                        keyword3 = new TemplateDataItem(param3),
                        keyword4 = new TemplateDataItem(param4),
                        remark = new TemplateDataItem(remark)
                    };
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fcustomer%2Fdetails%2F" + param5 + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                else if (template.Equals("H4wr3tCcSDvlVOR9J9cbomJjgajRyYzcrVJX_x3YLVA"))
                {
                    ///客户状态通知
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem(first),
                        keyword1 = new TemplateDataItem(param1),
                        keyword2 = new TemplateDataItem(param2),
                        remark = new TemplateDataItem(remark)
                    };
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fcustomer%2Fdetails%2F" + param3 + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                else if (template.Equals("zx_5OoMRcAEr5YkHoOooKbRwqaueXMpXjlQaQuNHGmc"))
                {
                    // 申请审核提醒
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem(first),
                        keyword1 = new TemplateDataItem(param1),
                        keyword2 = new TemplateDataItem(param2),
                        keyword3 = new TemplateDataItem(param3),
                        remark = new TemplateDataItem(remark)
                    };
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fcontact%2Fdetails%2F" + param4 + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                else if (template.Equals("ILdChJL_b9gREEWboGh7u3WtVsgat9kvfNznLRp79no"))
                {
                    // 审核提醒
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem(first),
                        keyword1 = new TemplateDataItem(param1),
                        keyword2 = new TemplateDataItem(param2),
                        remark = new TemplateDataItem(remark)
                    };
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fcontact%2Fdetails%2F" + param3 + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                else if (template.Equals("DTf1U8_1_btUmajk9-ynVh0OCVAb4iM5_xuXxdFm59g"))
                {
                    // 审批任务通知
                    var testData = new SendMessageData()
                    {
                        first = new TemplateDataItem(first),
                        keyword1 = new TemplateDataItem(param1),
                        keyword2 = new TemplateDataItem(param2),
                        keyword3 = new TemplateDataItem(param3),
                        keyword4 = new TemplateDataItem(param4),
                        keyword5 = new TemplateDataItem(param5),
                        remark = new TemplateDataItem(remark)
                    };
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Ftask%2Fdetails%2F" + param1 + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                }
                if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
                    return "1";
                else
                    return "0";
            }

            return "0";
        }

        /// <summary>
        /// 客户消息提醒
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="template"></param>
        /// <param name="nick"></param>
        /// <param name="content"></param>
        /// <param name="timeStamp"></param>
        /// <param name="signed"></param>
        /// <returns></returns>
        [WebMethod]
        public string SendChatMessage(string openId, string template, string nick, string content, string timeStamp, string signed)
        {
            // 验证
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(openId + timeStamp);
            var time = DateTime.Parse(timeStamp);
            string sign = Convert.ToBase64String(plainTextBytes);

            if (DateTime.Now.Year == time.Year && DateTime.Now.Month == time.Month && DateTime.Now.Day == time.Day && sign == signed)
            {
                var accessToken = AccessTokenContainer.TryGetAccessToken(AppID, Secret);
                var testData = new SendMessageData()
                {
                    first = new TemplateDataItem("您好，您的客户有新的咨询消息。"),
                    keyword1 = new TemplateDataItem(nick),
                    keyword2 = new TemplateDataItem(time.ToString("yyyy-MM-dd HH:mm:ss")),
                    remark = new TemplateDataItem("客户咨询内容：" + content)
                };
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fchat%2FMemberIndex%2F&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                var result = TemplateApi.SendTemplateMessage(accessToken, openId, template, url, testData);
                if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
                    return "1";
                else
                    return "0";
            }

            return "0";
        }

        /// <summary>
        /// 创建二维码
        /// </summary>
        /// <param name="sceneId"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        [WebMethod]
        public string CreateQrCode(string sceneId, string expireSeconds)
        {
            int _expireSeconds = Convert.ToInt32(expireSeconds);
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppID, Secret);
            CreateQrCodeResult result = QrCodeApi.Create(accessToken, _expireSeconds, 0, QrCode_ActionName.QR_STR_SCENE, sceneId);  // 临时QRCode

            return result.ticket;
        }
    }
}