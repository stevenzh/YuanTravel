using Common.Logging;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Weixin.Models;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 酒店首页
    /// </summary>
    public class HotelController : BaseController
    {
        private ILog logger = LogManager.GetLogger("HotelController");
        private SiteBannerBiz _bannerBiz = new SiteBannerBiz();
        private SearchProductBiz _searchProductBiz = new SearchProductBiz();
        private HotelBiz _biz = new HotelBiz();

        public ActionResult Index(string code, string state)
        {
            WapModel model = new WapModel();

            // 轮播图
            ViewData["SiteBanner"] = _bannerBiz.GetBanner("W001");

            ViewData["W001H1"] = _searchProductBiz.GetHotHotels("W001H1", OwnerCode);

            InWeixin(code, state);

            return View(model);
        }

        public ActionResult Details(string id)
        {
            var model = _biz.GetByCode(id);
            model.RoomList = _biz.GetRooms(id);
            model.FileList = _biz.GetFileList(id);
            return View(model);
        }
    }
}