using System;
using System.Web.Configuration;
using Common.Logging;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Lvy.Trip.Weixin.Models;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Weixin;
using Lvy.Trip.Weixin.CommonService.Download;
using Senparc.Weixin.Exceptions;
using Lvy.Trip.Biz.Crm;

namespace Lvy.Trip.Weixin.CommonService.CustomMessageHandler
{

    public partial class CustomMessageHandler
    {
        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];

        private AccountBiz accountBiz = new AccountBiz();

        ILog logger = LogManager.GetLogger("CustomMessageHandler");

        private string GetWelcomeInfo(string openID)
        {
            string subject = WebConfigurationManager.AppSettings["WeixinSubject"];
            return string.Format("您好，谢谢关注{0}微信公众号。", subject);
        }

        public override IResponseMessageBase OnTextOrEventRequest(RequestMessageText requestMessage)
        {
            // 预处理文字或事件类型请求。
            // 这个请求是一个比较特殊的请求，通常用于统一处理来自文字或菜单按钮的同一个执行逻辑，
            // 会在执行OnTextRequest或OnEventRequest之前触发，具有以下一些特征：
            // 1、如果返回null，则继续执行OnTextRequest或OnEventRequest
            // 2、如果返回不为null，则终止执行OnTextRequest或OnEventRequest，返回最终ResponseMessage
            // 3、如果是事件，则会将RequestMessageEvent自动转为RequestMessageText类型，其中RequestMessageText.Content就是RequestMessageEvent.EventKey

            if (requestMessage.Content == "OneClick")
            {
                var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                strongResponseMessage.Content = "您点击了底部按钮。\r\n为了测试微信软件换行bug的应对措施，这里做了一个——\r\n换行";
                return strongResponseMessage;
            }
            return null;//返回null，则继续执行OnTextRequest或OnEventRequest
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {
            IResponseMessageBase reponseMessage = null;

            //菜单点击，需要跟创建菜单时的Key匹配
            switch (requestMessage.EventKey)
            {
                case "Description":
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                        strongResponseMessage.Content = GetWelcomeInfo(requestMessage.FromUserName);
                        reponseMessage = strongResponseMessage;
                    }
                    break;
                default:
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                        strongResponseMessage.Content = "您点击了按钮，EventKey：" + requestMessage.EventKey;
                        reponseMessage = strongResponseMessage;
                    }
                    break;
            }

            return reponseMessage;
        }

        /// <summary>
        /// 进入事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_EnterRequest(RequestMessageEvent_Enter requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);
            responseMessage.Content = "您刚才发送了ENTER事件请求。";
            return responseMessage;
        }

        /// <summary>
        /// 位置事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_LocationRequest(RequestMessageEvent_Location requestMessage)
        {
            //这里是微信客户端（通过微信服务器）自动发送过来的位置信息
            // 记录用户位置
            new LocationBiz().AddLocation(1, requestMessage.FromUserName, requestMessage.Longitude, requestMessage.Latitude, requestMessage.Precision);
            //var responseMessage = CreateResponseMessage<ResponseMessageText>();
            // responseMessage.Content = "这里写什么都无所谓，比如：上帝爱你！";
            // return responseMessage;//这里也可以返回null（需要注意写日志时候null的问题）
            return null;
        }

        /// <summary>
        /// 通过二维码扫描关注扫描事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ScanRequest(RequestMessageEvent_Scan requestMessage)
        {
            if (!string.IsNullOrEmpty(requestMessage.EventKey)) // 扫描二维码来源
            {
                // 保存用户
                var accessToken = AccessTokenContainer.GetAccessToken(AppID);

                if (requestMessage.EventKey.StartsWith("qln_"))
                {
                    var account = accountBiz.GetAccountByOpenID(requestMessage.FromUserName);
                    if (account.Count > 0)
                    {
                        string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fadmin%2Ferplogin%2F%3F&response_type=code&scope=snsapi_base&state=" + requestMessage.EventKey + "#wechat_redirect";
                        var testData = new MessageData()
                        {
                            first = new TemplateDataItem("登陆确认提醒"),
                            keyword1 = new TemplateDataItem(account[0].Name),
                            keyword2 = new TemplateDataItem(DateTime.Now.ToShortDateString()),
                            remark = new TemplateDataItem("请点击详情确认登陆！")
                        };

                        var result = TemplateApi.SendTemplateMessage(accessToken, requestMessage.FromUserName, "NkctJeHY111TNcPtmAAS2jBhp7MpnowX9gE_ZMv_Ouo", url, testData);
                    }
                    else {
                        var responseMessage = CreateResponseMessage<ResponseMessageText>();
                        responseMessage.Content = "当前微信没有绑定用户。";
                        return responseMessage;
                    }
                }
                else
                {
                    int qrid = Convert.ToInt32(requestMessage.EventKey);
                    if (qrid > 1610000000)
                    {
                        // 后台账号绑定
                        var account = accountBiz.GetAccountCustomer(requestMessage.EventKey);
                        //string url = "http://yuanwx.sh-cct.cn/member/BindingAccount/?openid=" + u.openid + "&sceneId=" + sceneId;
                        string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fmember%2Fbindingaccount%2F%3F&response_type=code&scope=snsapi_base&state=" + requestMessage.EventKey + "#wechat_redirect";
                        var testData = new MessageData()
                        {
                            first = new TemplateDataItem("您好，您在进行用户绑定，点击消息接下来的操作。"),
                            keyword1 = new TemplateDataItem(account.Name),
                            keyword2 = new TemplateDataItem("绑定尚未完成，请在链接的页面核实身份，核实成功后，会帮助您更好的业务合作。"),
                            keyword3 = new TemplateDataItem(DateTime.Now.ToShortDateString()),
                            remark = new TemplateDataItem("")
                        };

                        var result = TemplateApi.SendTemplateMessage(accessToken, requestMessage.FromUserName, "BskZMoA58MchWImAZfPyBKJ6qYiKL5qnP5IOaXHrWzI", url, testData);
                    }
                    else
                    {
                        var u = UserApi.Info(accessToken, requestMessage.FromUserName);
                        //发送审核到销售
                        MemberBiz _service = new MemberBiz();
                        // 销售用户扫描
                        Member s = _service.GetUserByQr(qrid);

                        string url = "http://yuanwx.sh-cct.cn/memberadmin/details/" + u.openid;
                        var testData = new MessageData()
                        {
                            first = new TemplateDataItem("您好，您的客户提交信息审核。"),
                            keyword1 = new TemplateDataItem(u.nickname),
                            keyword2 = new TemplateDataItem(u.nickname),
                            keyword3 = new TemplateDataItem(""),
                            keyword4 = new TemplateDataItem(DateTime.Now.ToShortDateString()),
                            remark = new TemplateDataItem("审核客户信息后请及时确认。")
                        };

                        var result = TemplateApi.SendTemplateMessage(accessToken, s.OpenID, "dbtBNSrwDX2qG4qXu9pAGTj172O5_ZqvVkPxQlvnpDw", url, testData);

                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 打开网页事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ViewRequest(RequestMessageEvent_View requestMessage)
        {
            //说明：这条消息只作为接收，下面的responseMessage到达不了客户端，类似OnEvent_UnsubscribeRequest
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您点击了view按钮，将打开网页：" + requestMessage.EventKey;
            return responseMessage;
        }

        /// <summary>
        /// 群发完成事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_MassSendJobFinishRequest(RequestMessageEvent_MassSendJobFinish requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "接收到了群发完成的信息。";
            return responseMessage;
        }

        /// <summary>
        /// 订阅（关注）事件
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);
            responseMessage.Content = GetWelcomeInfo(requestMessage.FromUserName);
            // 保存用户
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppID, Secret);
            var u = UserApi.Info(accessToken, requestMessage.FromUserName);
            //MemberBiz _service = new MemberBiz();

            if (!string.IsNullOrEmpty(requestMessage.EventKey))
            {
                responseMessage.Content += "\r\n============\r\n场景值：" + requestMessage.EventKey;
            }

            //推送消息
            //下载文档
            //if (requestMessage.EventKey.StartsWith("qrscene_"))
            //{
            //    //发送审核到销售
            //    int qrid = Convert.ToInt32(requestMessage.EventKey.Substring(8));
            //    Member s = _service.GetUserByQr(qrid);

            //    string url = "http://yuanwx.sh-cct.cn/memberadmin/details/" + u.openid;
            //    var testData = new MessageData()
            //    {
            //        first = new TemplateDataItem("您好，您的客户提交信息审核。"),
            //        keyword1 = new TemplateDataItem(u.nickname),
            //        keyword2 = new TemplateDataItem(u.nickname),
            //        keyword3 = new TemplateDataItem(""),
            //        keyword4 = new TemplateDataItem(DateTime.Now.ToShortDateString()),
            //        remark = new TemplateDataItem("审核客户信息后请及时确认。")
            //    };

            //    var result = TemplateApi.SendTemplateMessage(accessToken, s.OpenID, "dbtBNSrwDX2qG4qXu9pAGTj172O5_ZqvVkPxQlvnpDw", url, testData);
            //}

            service.Subscribe("1611000001", u.openid, u.nickname, u.language, u.sex, u.city, u.province, u.country,
                u.headimgurl, DateTimeHelper.GetDateTimeFromXml(u.subscribe_time), requestMessage.EventKey);
            return responseMessage;
        }

        /// <summary>
        /// 退订
        /// 实际上用户无法收到非订阅账号的消息，所以这里可以随便写。
        /// unsubscribe事件的意义在于及时删除网站应用中已经记录的OpenID绑定，消除冗余数据。并且关注用户流失的情况。
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_UnsubscribeRequest(RequestMessageEvent_Unsubscribe requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "有空再来";
            service.Unsubscribe(requestMessage.FromUserName);
            return responseMessage;
        }

        /// <summary>
        /// 事件之扫码推事件(scancode_push)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ScancodePushRequest(RequestMessageEvent_Scancode_Push requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "事件之扫码推事件";
            return responseMessage;
        }

        /// <summary>
        /// 事件之扫码推事件且弹出“消息接收中”提示框(scancode_waitmsg)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ScancodeWaitmsgRequest(RequestMessageEvent_Scancode_Waitmsg requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "事件之扫码推事件且弹出“消息接收中”提示框";
            return responseMessage;
        }

        /// <summary>
        /// 事件之弹出拍照或者相册发图（pic_photo_or_album）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_PicPhotoOrAlbumRequest(RequestMessageEvent_Pic_Photo_Or_Album requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "事件之弹出拍照或者相册发图";
            return responseMessage;
        }

        /// <summary>
        /// 事件之弹出系统拍照发图(pic_sysphoto)
        /// 实际测试时发现微信并没有推送RequestMessageEvent_Pic_Sysphoto消息，只能接收到用户在微信中发送的图片消息。
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_PicSysphotoRequest(RequestMessageEvent_Pic_Sysphoto requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "事件之弹出系统拍照发图";
            return responseMessage;
        }

        /// <summary>
        /// 事件之弹出微信相册发图器(pic_weixin)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_PicWeixinRequest(RequestMessageEvent_Pic_Weixin requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "事件之弹出微信相册发图器";
            return responseMessage;
        }

        /// <summary>
        /// 事件之弹出地理位置选择器（location_select）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_LocationSelectRequest(RequestMessageEvent_Location_Select requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "事件之弹出地理位置选择器";
            return responseMessage;
        }

        /// <summary>
        /// 事件之发送模板消息返回结果
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_TemplateSendJobFinishRequest(RequestMessageEvent_TemplateSendJobFinish requestMessage)
        {
            switch (requestMessage.Status)
            {
                case "success":
                    //发送成功

                    break;
                case "failed:user block":
                    //送达由于用户拒收（用户设置拒绝接收公众号消息）而失败
                    break;
                case "failed: system failed":
                    //送达由于其他原因失败
                    break;
                default:
                    throw new WeixinException("未知模板消息状态：" + requestMessage.Status);
            }

            //注意：此方法内不能再发送模板消息，否则会造成无限循环！

            //            try
            //            {
            //                var msg = @"已向您发送模板消息
            //状态：{0}
            //MsgId：{1}
            //（这是一条来自MessageHandler的客服消息）".FormatWith(requestMessage.Status, requestMessage.MsgID);
            //                CustomApi.SendText(appId, WeixinOpenId, msg);//发送客服消息
            //            }
            //            catch (Exception e)
            //            {
            //                Senparc.Weixin.WeixinTrace.SendCustomLog("模板消息发送失败", e.ToString());
            //            }


            //无需回复文字内容
            //return requestMessage
            //    .CreateResponseMessage<ResponseMessageNoResponse>();
            return null;
        }

        #region 微信认证事件推送

        public override IResponseMessageBase OnEvent_QualificationVerifySuccessRequest(RequestMessageEvent_QualificationVerifySuccess requestMessage)
        {
            //以下方法可以强制定义返回的字符串值
            //TextResponseMessage = "your content";
            //return null;

            return new SuccessResponseMessage();//返回"success"字符串
        }

        #endregion
    }
}