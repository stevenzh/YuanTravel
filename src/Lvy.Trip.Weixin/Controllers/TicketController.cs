using Common.Logging;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Biz.Ticket;
using Lvy.Trip.Weixin.Models;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 门票首页
    /// </summary>
    public class TicketController : BaseController
    {
        private ILog logger = LogManager.GetLogger("TicketController");
        private SiteBannerBiz _bannerBiz = new SiteBannerBiz();
        private readonly TktOnlineBiz _biz = new TktOnlineBiz();
        private readonly TktProductBiz _productBiz = new TktProductBiz();
        private readonly BasePlaceBiz _placeBiz = new BasePlaceBiz();

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="outCity"></param>
        /// <returns></returns>
        public ActionResult Index(string code, string state)
        {
            WapModel model = new WapModel();
            // 轮播图
            ViewData["SiteBanner"] = _bannerBiz.GetBanner("W001");
            // 
            ViewData["2001"] = _biz.GetHotTickets("2001", OwnerCode);

            InWeixin(code, state);

            return View(model);
        }

        public ActionResult Details(string id)
        {
            var vModel = _productBiz.GetById(id);
            vModel.FileList = _productBiz.GetFileList(vModel.ProductId);
            if (!string.IsNullOrEmpty(vModel.PlaceCode))
            {
                vModel.Place = _placeBiz.GetPlaceByCode(vModel.PlaceCode);
            }
            return View(vModel);
        }
    }
}