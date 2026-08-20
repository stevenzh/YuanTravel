using Common.Logging;
using Lvy.Web.Common.Cache;
using System.Web.Services;

namespace Lvy.Trip.AdminSite
{
    /// <summary>
    /// LinkService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。
    // [System.Web.Script.Services.ScriptService]
    public class LinkService : System.Web.Services.WebService
    {
        private ILog _logger = LogManager.GetLogger(typeof(LinkService));

        /// <summary>
        ///
        /// </summary>
        /// <param name="sceneId"></param>
        /// <param name="openId">扫描二维码的微信用户OPENID</param>
        /// <returns></returns>
        [WebMethod]
        public string WeixinQrRtn(string sceneId, string accountCode)
        {
            // 返回值保存缓存序列
            CacheContext.Current.Add(sceneId, accountCode, 360);
            _logger.Info("SceneID:" + sceneId + "; AccountCode:" + accountCode);
            return "Sucess";
        }
    }
}