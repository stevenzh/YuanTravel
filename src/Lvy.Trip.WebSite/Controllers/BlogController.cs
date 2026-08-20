using Arch.Common;
using Common.Logging;
using Lvy.Trip.Biz.Base;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 博客
    /// </summary>
    public class BlogController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(BlogController));

        public ArticleBiz _biz = new ArticleBiz();

        // GET: Blog
        [OutputCache(Duration = Consts.OutputCacheDuration1)]
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
    }
}