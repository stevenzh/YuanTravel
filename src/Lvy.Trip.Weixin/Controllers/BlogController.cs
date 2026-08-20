using Arch.Common;
using Common.Logging;
using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Base;
using Lvy.VModels.Base;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 内部推文
    /// </summary>
    public class BlogController : Controller
    {
        private ILog _logger = LogManager.GetLogger("BlogController");
        private readonly ArticleBiz _biz = new ArticleBiz();

        // GET: Blog
        public ActionResult Index(ArticleVModel vModel)
        {
            if (vModel == null)
                vModel = new ArticleVModel();

            vModel.Article.OwnerCode = AppSetting.Get("OwnerCode");
            vModel.Scope = 1;
            vModel.ArticlePageList = _biz.GetPageList(vModel);
            if (Request.IsAjaxRequest())
                return PartialView("List", vModel);
            return View(vModel);
        }

        public ActionResult Details(int id)
        {
            try
            {
                var model = _biz.GetById(id);
                if (model == null)
                {
                    _logger.Warn("Visa->ProductDetails:错误，文章不存在。");
                    return View("404");
                }
                else if (model.NoticeType == 1)
                {
                    _logger.Warn("Visa->ProductDetails:错误，内部文章不能浏览。");
                    return View("404");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return View("404");
            }
        }

        [AllowAnonymous]
        public ActionResult Page(int id)
        {
            var model = new BaseArticleModel();
            if (id != 0)
            {
                model = _biz.GetById(id, true);
                string ip = GetRequestIP();
                //logger.Info("新获得IP：" + ip);
                var iplist = AppSetting.Get("WeixinProxyIP").Split(',');
                if (iplist.Contains(ip))
                {
                    _biz.AddBrowse(new BaseArticleBrowseModel
                    {
                        IPAdress = ip,
                        ArticleId = id,
                        RegionCode = "xx",
                        CityName = "XX"
                    });
                }
                else
                {
                    var obj = GetIpInfo(ip);
                    if (obj != null && obj.code == "0")
                    {
                        _biz.AddBrowse(new BaseArticleBrowseModel
                        {
                            IPAdress = ip,
                            ArticleId = id,
                            RegionCode = obj.data.city_id,
                            CityName = obj.data.city,
                            CreatedTime = DateTime.Now
                        });
                    }
                    else
                    {
                        var obj1 = GetIpApi(ip);
                        if (obj1 != null)
                        {
                            _biz.AddBrowse(new BaseArticleBrowseModel
                            {
                                IPAdress = ip,
                                ArticleId = id,
                                RegionCode = obj1.region,
                                CityName = obj1.city,
                                CreatedTime = DateTime.Now
                            });
                        }
                    }
                }
            }

            return View(model);
        }

        public ActionResult Stat(int id)
        {
            var model1 = _biz.StatTime(id, DateTime.Today);
            ViewData["Sales"] = string.Format("[\"{0}\"]", string.Join("\",\"", model1.Select(t => t.UserName).ToArray()));
            ViewData["SalesCount"] = string.Format("[{0}]", string.Join(",", model1.Select(t => t.AllFans).ToArray()));

            var model2 = _biz.StatRegion(id, DateTime.Today);
            ViewData["Regions"] = string.Format("[\"{0}\"]", string.Join("\",\"", model2.Select(t => t.UserName).ToArray()));
            ViewData["RegionCount"] = string.Format("[{0}]", string.Join(",", model2.Select(t => t.AllFans).ToArray()));

            var model = new BaseArticleModel();
            if (id != 0)
            {
                model = _biz.GetById(id);
            }

            return View(model);
        }

        public string GetRequestIP()
        {
            string result = Convert.ToString(Request.ServerVariables["HTTP_X_FORWARDED_FOR"]);
            //logger.Info("HTTP_X_FORWARDED_FOR: " + result);
            string result1 = Convert.ToString(Request.ServerVariables["HTTP_VIA"]);
            //logger.Info("HTTP_VIA: " + result1);
            string result2 = Convert.ToString(Request.ServerVariables["REMOTE_ADDR"]);
            //logger.Info("REMOTE_ADDR: " + result2);

            if (!String.IsNullOrEmpty(result))
            {
                if (result.IndexOf(".") == -1) return null;
                if (result.IndexOf(",") == -1) return result;
                return result.Split(',').FirstOrDefault(i => !i.StartsWith("192.168.") && !i.StartsWith("10.") && !i.StartsWith("172.16."));
            }
            result = Request.ServerVariables["REMOTE_ADDR"];
            return !String.IsNullOrEmpty(result) ? result : Request.UserHostAddress;
        }

        public IpInfoModel GetIpInfo(string ip)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://ip.taobao.com/service/getIpInfo.php?ip=" + ip);
                webRequest.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                using (StreamReader strReader = new StreamReader(webResponse.GetResponseStream()))
                {
                    var result = strReader.ReadToEnd();
                    IpInfoModel obj = JsonConvert.DeserializeObject<IpInfoModel>(result);
                    return obj;
                }
            }
            catch (WebException ex)
            {
                _logger.Error("", ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return null;
            }
        }

        public IpApiDataModel GetIpApi(string ip)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://ip-api.com/json/" + ip + "?lang=zh-CN");
                webRequest.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                using (StreamReader strReader = new StreamReader(webResponse.GetResponseStream()))
                {
                    var result = strReader.ReadToEnd();
                    IpApiDataModel obj = JsonConvert.DeserializeObject<IpApiDataModel>(result);
                    return obj;
                }
            }
            catch (WebException ex)
            {
                _logger.Error("", ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return null;
            }
        }
    }

    public class IpInfoModel
    {
        public string code { get; set; }
        public IpInfoDataModel data { get; set; }
    }

    public class IpInfoDataModel
    {
        public string ip { get; set; }
        public string country { get; set; }
        public string area { get; set; }
        public string region { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string isp { get; set; }
        public string country_id { get; set; }
        public string area_id { get; set; }
        public string region_id { get; set; }
        public string city_id { get; set; }
        public string county_id { get; set; }
        public string isp_id { get; set; }
    }

    public class IpApiDataModel
    {

        public string status { get; set; }

        public string country { get; set; }
        public string countryCode { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public decimal lat { get; set; }
        public decimal lon { get; set; }
        public string timezone { get; set; }
        public string isp { get; set; }
        public string org { get; set; }
        public string address { get; set; }
        public string query { get; set; }

    }

}