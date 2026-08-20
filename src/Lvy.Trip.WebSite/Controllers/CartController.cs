using Common.Logging;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 购物车
    /// </summary>
    public class CartController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(CartController));

        // GET: Cart
        [OutputCache(Duration = Consts.OutputCacheDuration1)]
        public ActionResult Index(ArticleVModel vModel)
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
            }
            return View();
        }
    }
}