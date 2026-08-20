using Common.Logging;
using Lvy.Models;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Site;
using Lvy.VModels.Online;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 新版前台页面
    /// </summary>
    public class HomeController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(HomeController));

        private readonly SearchProductBiz _searchProductBiz = new SearchProductBiz();
        private readonly SiteNavBiz _navBiz = new SiteNavBiz();
        private readonly SiteBiz _commonBiz = new SiteBiz();

        /// <summary>
        /// 站点首页
        /// </summary>
        /// <param name="out_city"></param>
        /// <returns></returns>
        [OutputCache(Duration = Consts.OutputCacheDuration1, VaryByParam = "out_city")]
        public ActionResult Index(string out_city = "31")
        {
            //Response.Cache.SetOmitVaryStar(true);
            HomePageRegionVModel model = new HomePageRegionVModel();
            if (GlobalContext.Current.CurrentCity == "31")
            {
                // 取得上海板块  线路推荐 分类
                var l = new DictionaryBiz().GetLineDestsCached("S003", GlobalContext.Current.OwnerCode);
                foreach (var item in l)
                {
                    HomeRegionVModel m = new HomeRegionVModel();
                    m.Name = item.Name;
                    m.PlanList = _searchProductBiz.GetHotTours(item.Code, GlobalContext.Current.OwnerCode);
                    model.RegionList.Add(m);
                }
            }

            return View(model);
        }

        /// <summary>
        /// 综合查询
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult Search(SearchProductVModel vModel)
        {
            if (vModel.MinOutDate.IsNullOrEmpty())
                vModel.MinOutDate = DateTime.Now.ToDateFormat(); //起始出发日期为今天
            if (vModel.MaxOutDate.IsNullOrEmpty())
                vModel.MaxOutDate = DateTime.Now.AddMonths(1).ToDateFormat(); //结束出发日期为一个月后的今天
            if (!vModel.ArriveDest.IsNullOrEmpty())
                vModel.ArriveDestName = DictionaryBiz.GetCacheDestNameStr(vModel.ArriveDest);
            if (vModel.OrderBy.IsNullOrEmpty())
                vModel.OrderBy = "1";
            if (vModel.ProductPagedList == null)
                vModel.ProductPagedList = new PagedList<TourInfoVModel>();

            vModel.ProductPagedList = _searchProductBiz.GetProducts(vModel, GlobalContext.Current.OwnerCode);

            if (Request.IsAjaxRequest())
                return PartialView("Search/UCProductList", vModel);

            // vModel.DestsNav = DestinationBiz.GetLineDestsCached();
            return View(vModel);
        }


        /// <summary>
        /// 显示价格
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult ShowPrice(int tourId)
        {
            TpPriceBiz priceBiz = new TpPriceBiz();
            var models = priceBiz.GetValidPrices(tourId);
            return PartialView("Index/UCShowPrice", models);
        }


        /// <summary>
        /// 新订单滚动 - 5分钟cache
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = Consts.OutputCacheDuration1)]
        public ActionResult OrderFlow()
        {
            Response.Cache.SetOmitVaryStar(true);
            var items = _commonBiz.GetOrderFlow(GlobalContext.Current.OwnerCode);
            return PartialView("Common/UCOrderFlow", items);
        }

        /// <summary>
        /// 通过tag跳转
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public ActionResult GoToByTag(int tagId, string tagName)
        {
            _commonBiz.UpdateClickCnt(tagId);
            return RedirectToAction("SearchProduct", "Online", new { themes = tagName });
        }

        /// <summary>
        /// 名片
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public ActionResult BusinessCard(string lineId)
        {
            var vModel = _commonBiz.GetBusinessCard(lineId);
            return PartialView("Common/BusinessCard", vModel);
        }

        /// <summary>
        /// 下载专区
        /// </summary>
        /// <returns></returns>
        //[OutputCache(Duration = Consts.OutputCacheDuration2)]
        //public ActionResult LoadDownload()
        //{
        //    Response.Cache.SetOmitVaryStar(true);

        //    var model = _searchProductBiz.GetFiles();
        //    return PartialView("Common/UCDownload", model);
        //}

        #region Common

        /// <summary>
        ///  热门标签
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = Consts.OutputCacheDuration3)]
        public ActionResult HotTags(int type = 1)
        {
            Response.Cache.SetOmitVaryStar(true);
            var tags = _commonBiz.GetClickCntTopTags(type, 10, GlobalContext.Current.OwnerCode);
            return PartialView("UCHotTags", tags);
        }

        #endregion Common
    }
}