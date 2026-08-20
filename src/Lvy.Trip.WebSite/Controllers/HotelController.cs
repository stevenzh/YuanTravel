using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Site;
using Lvy.VModels.Online;
using Lvy.Web.Common;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    public class HotelController : BaseController
    {
        private readonly SiteNavBiz _navBiz = new SiteNavBiz();
        private readonly HotelBiz _hotelBiz = new HotelBiz();
        private SearchProductBiz _biz = new SearchProductBiz();
        /// <summary>
        /// 酒店首页
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = Consts.OutputCacheDuration1)]
        public ActionResult Index(HotelVModel vModel)
        {
            Response.Cache.SetOmitVaryStar(true);

            // 推荐酒店
            vModel.TuiJianList = _biz.GetHotHotels("W001H1", GlobalContext.Current.OwnerCode);

            return View(vModel);
        }

        /// <summary>
        /// 线路详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            var vModel = _hotelBiz.GetByCode(id);
            vModel.RoomList = _hotelBiz.GetRooms(id);
            vModel.FileList = _hotelBiz.GetFileList(id);

            return View(vModel);
        }
    }
}