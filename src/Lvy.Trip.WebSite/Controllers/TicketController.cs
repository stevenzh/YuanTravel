using Lvy.Models;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Ticket;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 通用商品
    /// </summary>
    public class TicketController : BaseController
    {
        private readonly TktOnlineBiz _biz = new TktOnlineBiz();
        private readonly TktProductBiz _productBiz = new TktProductBiz();
        private readonly BasePlaceBiz _placeBiz = new BasePlaceBiz();

        /// <summary>
        /// 门票首页
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = Consts.OutputCacheDuration1)]
        public ActionResult Index(IndexVModel vModel)
        {
            Response.Cache.SetOmitVaryStar(true);

            vModel.Notices = new ArticleBiz().GetArticleList(GlobalContext.Current.OwnerCode, 3, 5);

            // 加载特惠专区
            //vModel.TeHuiList = _biz.GetTeHui(5);
            // 推荐专区
            vModel.TuiJianList = _biz.GetHotTickets("2001", GlobalContext.Current.OwnerCode);

            return View(vModel);
        }

        /// <summary>
        /// 查询门票产品
        /// </summary>
        /// <returns></returns>
        public ActionResult Search(SearchVModel vModel)
        {
            if (vModel == null)
                vModel = new SearchVModel();
            if (vModel.Products == null)
            {
                vModel.Products = new PagedList<TktProductVModel>();
            }
            vModel.OwnerCode = OwnerCode;
            vModel.Products = _biz.GetProducts(vModel);

            return PartialView("UCSearch", vModel);
        }

        /// <summary>
        /// 线路详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            var vModel = _productBiz.GetById(id);
            vModel.FileList = _productBiz.GetFileList(vModel.ProductId);

            if (vModel.ProductType == 5 && !string.IsNullOrEmpty(vModel.PlaceCode))  // 景点门票
            {
                vModel.Place = _placeBiz.GetPlaceByCode(vModel.PlaceCode);
            }
            return View(vModel);
        }

        /// <summary>
        /// 下载专区
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = Consts.OutputCacheDuration2)]
        public ActionResult LoadDownload()
        {
            Response.Cache.SetOmitVaryStar(true);

            var model = _biz.GetFiles(OwnerCode);
            return PartialView("Common/UCDownload", model);
        }

        /// <summary>
        /// 加载热门目的地and 景区
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = Consts.OutputCacheDuration2)]
        public ActionResult LoadHotDest()
        {
            Response.Cache.SetOmitVaryStar(true);
            ViewBag.HotDests = _biz.GetHotDestBeans(OwnerCode);
            //ViewBag.HotProducts = _biz.GetHotProductBeans();

            return PartialView("Common/UCHot");
        }

        /// <summary>
        /// 显示报价
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ActionResult ShowPrice(string productId)
        {
            var models = _biz.GetPrices(productId);

            return PartialView("Common/UCFloatPrices", models);
        }

        /// <summary>
        /// 所有有效的门票（Select2使用）查询页面使用 废弃
        /// </summary>
        /// <param name="fromCustomer"></param>
        /// <param name="keyword"></param>
        /// <param name="hasChild"></param>
        /// <returns></returns>
        public ActionResult GetTicketSelect2(string keyword, int page = 0, int size = 10)
        {
            var customers = DictionaryTools.GetCachedTicketDict().Values.Where(a => a.OwnerCode == GlobalContext.Current.OwnerCode && a.ProductState == 3);
            if (!keyword.IsNullOrEmpty())
            {
                customers = customers.Where(a => (a.ProductName != null && a.ProductName.Contains(keyword))).Take(15);
            }
            int total = customers.Count();

            var list = (from vv in customers.OrderByDescending(a => a.ProductName)
                        select new
                        {
                            id = vv.Id,
                            text = vv.ProductName
                        }).Skip(page * size).Take(size).ToList();

            var model = new
            {
                rows = list,
                total = total
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}