using Common.Logging;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Weixin.Services;
using Lvy.Visa.Biz;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 签证显示
    /// </summary>
    public class VisaController : BaseController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(VisaController));

        private ProductBiz visaProductService = new ProductBiz();
        private SearchProductBiz homeService = new SearchProductBiz();
        private WeixinService service = new WeixinService();

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string code, string state)
        {

            // 团签
            var CacheKey = "CacheKey=Visa|Index|GroupVisaModule:1002";
            var _getModel = CacheContext.Current.Get(CacheKey);
            if (_getModel == null)
            {
                ViewData["GroupVisaList"] = homeService.GetB2bVisaList("2");
                CacheContext.Current.Add(CacheKey, ViewData["GroupVisaList"], Consts.OutputCacheDuration1);
            }
            else
            {
                ViewData["GroupVisaList"] = ((List<VisaProductQModel>)_getModel);
            }


            // 个签
            var CacheKey1 = "CacheKey=Visa|Index|PersonVisaModule:1001";
            var _getModel2 = CacheContext.Current.Get(CacheKey1);
            if (_getModel2 == null)
            {
                ViewData["PersonVisaList"] = homeService.GetB2bVisaList("1");
                CacheContext.Current.Add(CacheKey1, ViewData["PersonVisaList"], Consts.OutputCacheDuration1);
            }
            else
            {
                ViewData["PersonVisaList"] = ((List<VisaProductQModel>)_getModel2);
            }

            InWeixin(code, state);

            return View();
        }

        public ActionResult Details(string id)
        {
            try
            {
                var productModel = homeService.GetVisaProductInfo(id);
                if (productModel != null) // && productModel.VType == 1)
                {
                    var model = new OnLineProductQModel
                    {
                        VisaModel = productModel,
                        VisaCategoryModels = visaProductService.GetCategroyList(id),
                        VisaDataModels = visaProductService.GetVisaDataList(id),
                        VisaDataFileModels = visaProductService.GetVisaMaterialFileList(id)
                    };

                    return View(model);
                }
                else
                {
                    logger.Error("Visa->ProductDetails:错误，产品不存在。");
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return View("Error");
            }
        }
    }
}