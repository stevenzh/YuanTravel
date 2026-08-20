using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Senparc.Weixin.MP;
using Senparc.Weixin;
using Senparc.Weixin.MP.Helpers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Lvy.Trip.Biz.Weixin;
using Lvy.Models.WeixinDB;
using Lvy.VModels.Online;
using Lvy.Trip.Weixin.CommonService.Utilities;
using Lvy.VModels.Product;
using Lvy.Trip.Biz.Site;

namespace Lvy.Trip.Weixin.CommonService.CustomMessageHandler
{

    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        public MemberBiz service = new MemberBiz();

        readonly Func<string> _getRandomFileName = () => DateTime.Now.Ticks + Guid.NewGuid().ToString("n").Substring(0, 6);

        /*
         * 重要提示：v1.5起，MessageHandler提供了一个DefaultResponseMessage的抽象方法，
         * DefaultResponseMessage必须在子类中重写，用于返回没有处理过的消息类型（也可以用于默认消息，如帮助信息等）；
         * 其中所有原OnXX的抽象方法已经都改为虚方法，可以不必每个都重写。若不重写，默认返回DefaultResponseMessage方法中的结果。
         */


        //下面的Url和Token可以用其他平台的消息，或者到www.weiweihi.com注册微信用户，将自动在“微信营销工具”下得到
        private string agentUrl = WebConfigurationManager.AppSettings["WeixinAgentUrl"];//这里使用了www.weiweihi.com微信自动托管平台
        private string agentToken = WebConfigurationManager.AppSettings["WeixinAgentToken"];//Token
        private string wiweihiKey = WebConfigurationManager.AppSettings["WeixinAgentWeiweihiKey"];//WeiweihiKey专门用于对接www.Weiweihi.com平台，获取方式见：http://www.weiweihi.com/ApiDocuments/Item/25#51

        private static readonly string AppID = WebConfigurationManager.AppSettings["WeixinAppId"];
        private static readonly string Secret = WebConfigurationManager.AppSettings["WeixinAppSecret"];

        /// <summary>
        /// 模板消息集合（Key：checkCode，Value：OpenId）
        /// </summary>
        public static Dictionary<string, string> TemplateMessageCollection = new Dictionary<string, string>();

        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalWeixinContext.ExpireMinutes = 3。
            WeixinContext.ExpireMinutes = 3;

            if (!string.IsNullOrEmpty(postModel.AppId))
            {
                appId = postModel.AppId;//通过第三方开放平台发送过来的请求
            }

            //在指定条件下，不使用消息去重
            base.OmitRepeatedMessageFunc = requestMessage =>
            {
                var textRequestMessage = requestMessage as RequestMessageText;
                if (textRequestMessage != null && textRequestMessage.Content == "容错")
                {
                    return false;
                }
                return true;
            };
        }

        public CustomMessageHandler(RequestMessageBase requestMessage)
            : base(requestMessage)
        {
        }

        public override void OnExecuting()
        {
            //测试MessageContext.StorageData
            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = 0;
            }
            base.OnExecuting();
        }

        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            //TODO:这里的逻辑可以交给Service处理具体信息，参考OnLocationRequest方法或/Service/LocationSercice.cs

            #region 历史方法
            //方法一（v0.1），此方法调用太过繁琐，已过时（但仍是所有方法的核心基础），建议使用方法二到四
            //var responseMessage =
            //    ResponseMessageBase.CreateFromRequestMessage(RequestMessage, ResponseMsgType.Text) as
            //    ResponseMessageText;

            //方法二（v0.4）
            //var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(RequestMessage);

            //方法三（v0.4），扩展方法，需要using Senparc.Weixin.MP.Helpers;
            //var responseMessage = RequestMessage.CreateResponseMessage<ResponseMessageText>();

            //方法四（v0.6+），仅适合在HandlerMessage内部使用，本质上是对方法三的封装
            //注意：下面泛型ResponseMessageText即返回给客户端的类型，可以根据自己的需要填写ResponseMessageNews等不同类型。

            #endregion

            // 检查词典回复
            MessageBiz service = new MessageBiz();
            SearchProductBiz _searchProductBiz = new SearchProductBiz();

            if (requestMessage.Content.Equals("在吗"))
            {
                var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
                responseMessage.Content = "暂时无客服服务，有需要拨打销售员电话，公司总机021-61819888.";
                return responseMessage;
            }

            // 保存记录
            service.AddMessage(new MemberMessage
            {
                OpenID = requestMessage.FromUserName,
                Content = requestMessage.Content,
                CreatedDate = requestMessage.CreateTime
            });

            string content = requestMessage.Content;
            string outcity = "31";
            content = content.Replace("济州岛", "济州");
            content = content.Replace("普吉岛", "普吉");
            content = content.Replace("塞班岛", "塞班");
            content = content.Replace("马代", "马尔代夫");
            if (content.StartsWith("上海") || content.StartsWith("上海 "))
            {
                content = content.Substring(2).Trim();
            }
            else if (content.StartsWith("上海至"))
            {
                content = content.Substring(3).Trim();
            }
            else if (content.StartsWith("南京") || content.StartsWith("南京 "))
            {
                content = content.Substring(2).Trim();
                outcity = "NJ";
            }

            var pd = _searchProductBiz.GetProducts(new SearchProductVModel { OutCity = outcity, ArriveDest = content }, "1");  // 取得产品列表
            if (pd.TotalCount > 0)
            {
                var responseMessage = CreateResponseMessage<ResponseMessageNews>();
                foreach (TourInfoVModel vp in pd.Items)
                {
                    responseMessage.Articles.Add(new Article()
                    {
                        Title = vp.LineName,
                        Description = vp.LineName + vp.Price.ToString("##0"),
                        PicUrl = "http://yuan.sh-cct.com/",
                        Url = "http://yuan.sh-cct.cn/line/details/" + vp.LineId
                    });
                }
                return responseMessage;
            }
            else
            {
                //检查线路包含
                var responseMessage = CreateResponseMessage<ResponseMessageText>();
                responseMessage.Content = "亲，没找到您要的产品。";
                return responseMessage;
            }
        }

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageNews>(requestMessage);
            //BranchService branchService = new BranchService();
            //var loc = BaiduHelper.GeoConv(requestMessage.Location_Y.ToString(), requestMessage.Location_X.ToString());
            //if (loc.status == 0)
            //    responseMessage.Articles = GetNearBranch(1, loc.result.First().y, loc.result.First().x);
            //else
            //    responseMessage.Articles = GetNearBranch(1, requestMessage.Location_X, requestMessage.Location_Y);
            return responseMessage;
        }

        public override IResponseMessageBase OnShortVideoRequest(RequestMessageShortVideo requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您刚才发送的是小视频";
            return responseMessage;
        }

        //private List<Article> GetNearBranch(int hostId, double x, double y)
        //{
        //    List<Article> rlist = new List<Article>();
        //    BranchService branchService = new BranchService();
        //    var list = branchService.GetNearest(hostId, x, y);
        //    rlist.Add(new Article()
        //    {
        //        Title = "离您最近的门店"
        //    });

        //    foreach (Branch br in list)
        //    {
        //        var markersList = new List<BaiduMarkers>();
        //        markersList.Add(new BaiduMarkers()
        //        {
        //            Longitude = Convert.ToDouble(br.LocationY),
        //            Latitude = Convert.ToDouble(br.LocationX),
        //            Color = "red",
        //            Label = br.Department,
        //            Size = BaiduMarkerSize.Default,
        //        });

        //        var url = BaiduMapHelper.GetBaiduStaticMap(Convert.ToDouble(br.LocationY), Convert.ToDouble(br.LocationX), 2, 17, markersList, 100, 100);

        //        rlist.Add(new Article()
        //        {
        //            Description = "",
        //            PicUrl = url,
        //            Title = br.Department + "\r地址：" + br.Address + "\r电话：" + br.Phone,
        //            Url = @"http://api.map.baidu.com/direction?mode=driving&origin_region=上海&origin=latlng:"
        //                + x + "," + y
        //                + "|name:我的位置&destination_region=上海&destination=latlng:"
        //                + br.LocationX + "," + br.LocationY + "|name:" + br.Department
        //                + "&output=html&ak=S0yxie593jr9DpgLwdSs7Mq3"
        //        });
        //    }
        //    return rlist;
        //}

        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            var accessToken = AccessTokenContainer.GetAccessToken(appId);
            var logPath1 = string.Format("~/upload/MP/{0}/", DateTime.Now.ToString("yyyy-MM-dd"));
            var logPath = HttpContext.Current.Server.MapPath(logPath1);
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            string fileName = string.Format("{0}.jpg", _getRandomFileName());
            using (MemoryStream ms = new MemoryStream())
            {
                MediaApi.Get(accessToken, requestMessage.MediaId, ms);

                //保存到文件
                using (FileStream fs = new FileStream(logPath + fileName, FileMode.Create))
                {
                    ms.Position = 0;
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                    }
                    fs.Flush();
                }
            }

            // 消息保存数据库
            MemberMessage message = new MemberMessage
            {
                OpenID = requestMessage.FromUserName,
                MsgType = requestMessage.MsgType.ToString(),
                FileUrl = UIGlobal.WeixinSiteURL + logPath1.Replace("~", "") + fileName,
                CreatedDate = DateTime.Now,
                InOut = 0,
                IsCallBack = "0"
            };
            new MemberBiz().SendMessage(message);

            responseMessage.Content = "[自动回复]图片收到，客服会及时给您回复."; //回复内容
            return responseMessage;
        }

        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);
            var logPath1 = string.Format("~/upload/MP/{0}/", DateTime.Now.ToString("yyyy-MM-dd"));
            var logPath = HttpContext.Current.Server.MapPath(logPath1);
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            string fileName = string.Format("{0}.amr", _getRandomFileName());
            using (MemoryStream ms = new MemoryStream())
            {
                MediaApi.Get(accessToken, requestMessage.MediaId, ms);

                //保存到文件
                using (FileStream fs = new FileStream(logPath + fileName, FileMode.Create))
                {
                    ms.Position = 0;
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                    }
                    fs.Flush();
                }
            }

            // 消息保存数据库
            MemberMessage message = new MemberMessage
            {
                OpenID = requestMessage.FromUserName,
                MsgType = requestMessage.MsgType.ToString(),
                FileUrl = UIGlobal.WeixinSiteURL + logPath1.Replace("~", "") + fileName,
                CreatedDate = DateTime.Now,
                InOut = 0,
                IsCallBack = "0"
            };
            new MemberBiz().SendMessage(message);

            responseMessage.Content = "[自动回复]语音收到，客服会及时给您回复."; //回复内容
            return responseMessage;
        }

        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了一条视频信息，ID：" + requestMessage.MediaId;

            #region 上传素材并推送到客户端

            Task.Factory.StartNew(async () =>
             {
                 //上传素材
                 var dir = Server.GetMapPath("~/App_Data/TempVideo/");
                 var file = await MediaApi.GetAsync(appId, requestMessage.MediaId, dir);
                 var uploadResult = await MediaApi.UploadTemporaryMediaAsync(appId, UploadMediaFileType.video, file, 50000);
                 await CustomApi.SendVideoAsync(appId, base.WeixinOpenId, uploadResult.media_id, "这是您刚才发送的视频", "这是一条视频消息");
             }).ContinueWith(async task =>
             {
                 if (task.Exception != null)
                 {
                     WeixinTrace.Log("OnVideoRequest()储存Video过程发生错误：", task.Exception.Message);

                     var msg = string.Format("上传素材出错：{0}\r\n{1}",
                                task.Exception.Message,
                                task.Exception.InnerException != null
                                    ? task.Exception.InnerException.Message
                                    : null);
                     await CustomApi.SendTextAsync(appId, base.WeixinOpenId, msg);
                 }
             });

            #endregion

            return responseMessage;
        }


        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);
            responseMessage.Content = string.Format(@"您发送了一条连接信息：
Title：{0}
Description:{1}
Url:{2}", requestMessage.Title, requestMessage.Description, requestMessage.Url);
            return responseMessage;
        }

        public override IResponseMessageBase OnFileRequest(RequestMessageFile requestMessage)
        {
            var responseMessage = requestMessage.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = string.Format(@"您发送了一个文件：
文件名：{0}
说明:{1}
大小：{2}
MD5:{3}", requestMessage.Title, requestMessage.Description, requestMessage.FileTotalLen, requestMessage.FileMd5);
            return responseMessage;
        }

        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEventRequest(IRequestMessageEventBase requestMessage)
        {
            var eventResponseMessage = base.OnEventRequest(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
            //TODO: 对Event信息进行统一操作
            return eventResponseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            /* 所有没有被处理的消息会默认返回这里的结果，
             * 因此，如果想把整个微信请求委托出去（例如需要使用分布式或从其他服务器获取请求），
             * 只需要在这里统一发出委托请求，如：
             * var responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
             * return responseMessage;
             */

            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            // responseMessage.Content = "这条消息来自DefaultResponseMessage。";
            return responseMessage;
        }

        public override IResponseMessageBase OnUnknownTypeRequest(RequestMessageUnknownType requestMessage)
        {
            /*
             * 此方法用于应急处理SDK没有提供的消息类型，
             * 原始XML可以通过requestMessage.RequestDocument（或this.RequestDocument）获取到。
             * 如果不重写此方法，遇到未知的请求类型将会抛出异常（v14.8.3 之前的版本就是这么做的）
             */
            var msgType = MsgTypeHelper.GetRequestMsgTypeString(requestMessage.RequestDocument);
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "未知消息类型：" + msgType;

            WeixinTrace.SendCustomLog("未知请求消息类型", requestMessage.RequestDocument.ToString());//记录到日志中

            return responseMessage;
        }

        ///<summary>
        /// 下载保存多媒体文件,返回多媒体保存路径
        ///</summary>
        ///<param name="ACCESS_TOKEN"></param>
        ///<param name="MEDIA_ID"></param>
        ///<returns></returns>
        public string GetMultimedia(string ACCESS_TOKEN, string MEDIA_ID)
        {
            string file = string.Empty;
            // string content = string.Empty;
            string strpath = string.Empty;
            string savepath = string.Empty;
            string stUrl = "http://file.api.weixin.qq.com/cgi-bin/media/get?access_token=" + ACCESS_TOKEN + "&media_id=" + MEDIA_ID;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(stUrl);
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();
                strpath = myResponse.ResponseUri.ToString();
                var logPath = HttpContext.Current.Server.MapPath("~/upload/clogo/");

                WebClient mywebclient = new WebClient();
                file = DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next().ToString().Substring(0, 4) + ".jpg";
                savepath = logPath + file;
                try
                {
                    mywebclient.DownloadFile(strpath, savepath);
                }
                catch (Exception ex)
                {
                    savepath = ex.ToString();
                }
            }
            return file;
        }

    }
}
