using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.WebSite.Mvc.Attributes;
using Lvy.Visa.Biz;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    public class VisaController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(VisaController));

        private SiteNavBiz hotVisaService = new SiteNavBiz();
        private ProductBiz visaProductService = new ProductBiz();
        private SearchProductBiz productBiz = new SearchProductBiz();

        // GET: Visa
        [OutputCache(Duration = Consts.OutputCacheDuration1)]
        public ActionResult Index()
        {
            //Response.Cache.SetOmitVaryStar(true);

            GetHotVisaList();
            GetUrgentVisaList();
            return View();
        }

        /// <summary>
        /// 查询块
        /// </summary>
        /// <returns></returns>
        public ActionResult Search()
        {
            GetHotCountryList();
            return View();
        }

        public ActionResult Details(string id)
        {
            try
            {
                //var CacheKey = "CacheKey=Visa|ProductDetail|RecommendModule:" + id;
                //var _getModel = DataCache.GetCache(CacheKey);
                //if (_getModel == null)
                //{
                var productModel = productBiz.GetVisaProductInfo(id);
                if (productModel != null && productModel.VType == 1)
                {
                    var model = new OnLineProductQModel
                    {
                        VisaModel = productModel,
                        VisaCategoryModels = visaProductService.GetCategroyList(id),
                        VisaDataModels = visaProductService.GetVisaDataList(id),
                        VisaDataFileModels = visaProductService.GetVisaMaterialFileList(id)
                    };
                    model.VisaModel.InterviewTypeValue = DictionaryTools.GetEnumValue(Enums.InterviewTypeEnum, model.VisaModel.InterviewType.ToString());

                    //保存浏览历史记录
                    SaveHistory(productModel.InformationCode, productModel.InformationName + "|" + productModel.SellPrice);

                    //DataCache.SetCache(CacheKey, model, DateTime.Now.AddMinutes(Maidou.Core.Configs.cacheDateTime), TimeSpan.Zero);

                    return View(model);
                }
                else
                {
                    _logger.Warn("Visa->ProductDetails:错误，产品不存在。");
                    return View("404");
                }
                //}
                //else
                //{
                //    var model = ((OnLineProductQModel)_getModel);
                //    //保存浏览历史记录
                //    SaveHistory(model.VisaModel.InformationCode, model.VisaModel.InformationName + "|" + model.VisaModel.SellPrice);
                //    return View("Index", model);
                //}
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return View("404");
            }
        }

        /// <summary>
        /// 所有产品国家列表
        /// </summary>
        /// <returns></returns>
        public ActionResult HotVisaCountry()
        {
            GetHotCountryList();
            return View();
        }

        private void GetHotCountryList()
        {
            var CacheKey = "CacheKey=Visa|Index|RecommendModule:1001";
            var _getModel = CacheContext.Current.Get(CacheKey);
            if (_getModel == null)
            {
                var banKuaiList = DictionaryTools.GetEnumsBy(Enums.ContinentEnum);
                var countryList = productBiz.GetHotCountryList();
                IList<BanKuaiQModel> list = new List<BanKuaiQModel>();
                foreach (var bankuai in banKuaiList)    // 按洲分
                {
                    var bkModel = new BanKuaiQModel
                    {
                        BanKuaiKey = bankuai.Key,
                        BanKuaiValue = bankuai.Value,
                        CountryList = countryList.Where(a => a.BanKuaiKey.Equals(bankuai.Key)).ToList()
                    };
                    if (null != bkModel.CountryList && bkModel.CountryList.Count > 0)
                    {
                        list.Add(bkModel);
                    }
                }
                ViewData["HotCountryList"] = list;
                CacheContext.Current.Add(CacheKey, ViewData["HotCountryList"], Convert.ToInt32(AppSetting.Get("cacheDateTime")));
            }
            else
            {
                ViewData["HotCountryList"] = ((List<BanKuaiQModel>)_getModel);
            }
        }

        /// <summary>
        /// 热门签证
        /// </summary>
        public void GetHotVisaList()
        {
            var CacheKey = "CacheKey=Visa|Index|RecommendModule:1002";
            var _getModel = CacheContext.Current.Get(CacheKey);
            if (_getModel == null)
            {
                ViewData["HotVisaList"] = productBiz.GetHotVisaList("1002");
                CacheContext.Current.Add(CacheKey, ViewData["HotVisaList"], Convert.ToInt32(AppSetting.Get("cacheDateTime")));
            }
            else
            {
                ViewData["HotVisaList"] = ((List<VisaProductQModel>)_getModel);
            }
        }

        /// <summary>
        /// 加急签证
        /// </summary>
        public void GetUrgentVisaList()
        {
            var CacheKey = "CacheKey=Visa|Index|RecommendModule:1003";
            var _getModel = CacheContext.Current.Get(CacheKey);
            if (_getModel == null)
            {
                ViewData["UrgentVisaList"] = productBiz.GetHotVisaList("1003");
                CacheContext.Current.Add(CacheKey, ViewData["UrgentVisaList"], Convert.ToInt32(AppSetting.Get("cacheDateTime")));
            }
            else
            {
                ViewData["UrgentVisaList"] = ((List<VisaProductQModel>)_getModel);
            }
        }

        #region 清除指定缓存

        /// <summary>
        /// 清除缓存页面
        /// </summary>
        /// <returns></returns>
        public ActionResult DataCacheIndex()
        {
            return View();
        }

        /// <summary>
        /// 清除指定缓存
        /// </summary>
        /// <param name="type">缓存名称</param>
        /// <param name="deparcity">目的地</param>
        /// <returns></returns>
        public ActionResult RemoveCache(string type)
        {
            var CacheKey = "CacheKey=Visa|Index|RecommendModule:" + type;
            CacheContext.Current.Remove(CacheKey);
            return Content("true");
        }

        /// <summary>
        /// 清楚缓存列表
        /// </summary>
        /// <param name="strList">用字符串拼接的缓存名称</param>
        /// <param name="deparcity">目的地</param>
        /// <returns></returns>
        public ActionResult RemoveCacheList(string strList)
        {
            string[] strValues = strList.TrimEnd(',').Split(',');
            foreach (var type in strValues)
            {
                var CacheKey = "CacheKey=Visa|Index|RecommendModule:" + type;
                CacheContext.Current.Remove(CacheKey);
            }
            return Content("true");
        }

        #endregion 清除指定缓存

        protected void SaveHistory(string code, string name)
        {
            HttpCookie cookie = new HttpCookie(HttpUtility.UrlEncode("Visaliulan" + code + ""));
            cookie.Expires = DBTools.GetSysDate().AddDays(1);

            cookie.Values.Add(HttpUtility.UrlEncode(code.ToString()), HttpUtility.UrlEncode(name));
            Response.Cookies.Add(cookie);
        }


        [LvyAuth]
        public ActionResult Booking(BookingQModel bookModel)
        {
            try
            {
                bookModel.ProductModel = productBiz.GetVisaProductInfo(bookModel.ProductCode);
                //验证产品
                if (bookModel.ProductModel == null || bookModel.ProductModel.State != 5 || bookModel.ProductModel.VType != 1)
                {
                    _logger.Warn("BookController>Reserve:" + bookModel.ProductCode + "产品不存在或未上线或不是个签");
                    return View("400");
                }
                bookModel.ProductModel.InterviewTypeValue = DictionaryTools.GetEnumValue(Enums.InterviewTypeEnum, bookModel.ProductModel.InterviewType.ToString());
                bookModel.SalePrice = bookModel.ProductModel.SellPrice;
                bookModel.TotPeopleNum = 1;
                bookModel.TotProductAmount = bookModel.SalePrice * bookModel.TotPeopleNum;
                bookModel.TotAmount = bookModel.TotProductAmount;
                Session["BookData"] = bookModel;
                return View(bookModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return View("400");
            }
        }
    }
}