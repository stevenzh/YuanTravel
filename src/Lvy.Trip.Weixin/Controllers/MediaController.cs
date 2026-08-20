using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Containers;

namespace Lvy.Trip.Weixin.Controllers
{
    [AllowAnonymous]
    public class MediaController : Controller
    {
        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];

        public FileResult GetVoice(string mediaId)
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);

            MemoryStream ms = new MemoryStream();
            MediaApi.Get(accessToken, mediaId, ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "audio/amr", "voice.amr");
        }

        /// <summary>
        /// 这个方法专为Senparc.Weixin.MP.Test/HttpUtility/RequestUtilityTest.cs/HttpPostTest() 提供上传测试目标
        /// </summary>
        /// <param name="token"></param>
        /// <param name="type"></param>
        /// <param name="contentLength"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TestUploadMediaFile(string token, UploadMediaFileType type, int contentLength /*, HttpPostedFileBase postedFile*/)
        {
            var inputStream = Request.InputStream;
            if (contentLength != inputStream.Length)
            {
                return Content("ContentLength不正确，可能接收错误！");
            }

            if (token != "TOKEN")
            {
                return Content("TOKEN不正确！");
            }

            if (type != UploadMediaFileType.image)
            {
                return Content("UploadMediaFileType不正确！");
            }

            //储存文件，对比是否上传成功
            using (FileStream ms = new FileStream(Server.MapPath("~/TestUploadMediaFile.jpg"), FileMode.OpenOrCreate))
            {
                inputStream.CopyTo(ms, 256);
            }

            return Content("{\"type\":\"image\",\"media_id\":\"MEDIA_ID\",\"created_at\":123456789}");
        }
    }
}
