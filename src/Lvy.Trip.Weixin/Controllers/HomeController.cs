using Common.Logging;
using Lvy.Models.SiteDB;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Weixin.Models;
using Lvy.Web.Common.Cache;
using Senparc.Weixin.MP.Containers;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 前台首页
    /// </summary>
    public class HomeController : BaseController
    {
        private ILog logger = LogManager.GetLogger("HomeController");
        private SiteNavBiz _navBiz = new SiteNavBiz();
        private SiteBannerBiz _bannerBiz = new SiteBannerBiz();
        private SearchProductBiz _searchProductBiz = new SearchProductBiz();

        public HomeController()
        {
            // 微信
            AccessTokenContainer.Register(appId, secret);
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string code, string state)
        {
            string outCity = "31";
            WapModel model = new WapModel();

            #region 取得线路导航

            var CacheNavKey = "CacheKey=Home|Index|NavList:" + outCity;
            var _NavList = CacheContext.Current.Get(CacheNavKey);
            if (_NavList == null)
            {
                model.NavList = _navBiz.SearchList("W001", outCity);
                CacheContext.Current.Add(CacheNavKey, model.NavList);
            }
            else
                model.NavList = (IList<SiteNavItemModel>)_NavList;

            #endregion 取得线路导航

            // 轮播图
            ViewData["SiteBanner"] = _bannerBiz.GetBanner("W001");

            // 推荐线路
            ViewData["W001L1"] = _searchProductBiz.GetHotTours("W001L1", OwnerCode);

            // 推荐酒店
            ViewData["W001H1"] = _searchProductBiz.GetHotHotels("W001H1", OwnerCode);

            InWeixin(code, state);

            return View(model);
        }

        public ActionResult Message(int id)
        {
            WapModel model = new WapModel();
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

        public ActionResult Categories()
        {
            return View();
        }
    }
}