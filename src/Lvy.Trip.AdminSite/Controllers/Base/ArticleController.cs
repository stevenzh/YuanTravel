using Lvy.Models.BaseDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Site;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 商户功能 - 公告栏功能控制器
    /// notifications
    /// </summary>
    public class ArticleController : BaseController
    {
        private readonly ArticleBiz _biz = new ArticleBiz();
        private readonly BaseTagBiz baseTagBiz = new BaseTagBiz();
        private readonly SearchProductBiz _searchBiz = new SearchProductBiz();

        /// <summary>
        /// 查询公告-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Search(ArticleVModel vModel)
        {
            if (vModel == null)
                vModel = new ArticleVModel();

            vModel.Article.OwnerCode = OwnerCode;
            vModel.ArticlePageList = _biz.GetPageList(vModel);
            InitPage();
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 添加公告-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            InitPage();
            return View("Create");
        }

        /// <summary>
        /// 添加公告-保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult Add(BaseArticleModel model)
        {
            model.IsValid = 1;
            model.CreatedTime = DateTime.Now;
            model.CreatedBy = UserInfo.Code;
            model.Url = model.Url != null ? model.Url.Replace("http://", "") : null;
            model.OwnerCode = OwnerCode;
            var newid = _biz.AddArticle(model);

            CheckLineFolder(newid);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 编辑公告-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            InitPage();
            var model = _biz.GetById(id);
            return View(model);
        }

        /// <summary>
        /// 编辑公告-保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        [ValidateInput(false)]
        public ActionResult Update(BaseArticleModel model)
        {
            var obj = _biz.GetById(model.Id);
            obj.ModifiedBy = UserInfo.Code;
            obj.ModifiedTime = DateTime.Now;
            obj.NoticeType = model.NoticeType;
            obj.Title = model.Title;
            obj.Contents = model.Contents;
            obj.Url = model.Url != null ? model.Url.Replace("http://", "") : null;
            obj.ImgUrl = model.ImgUrl;
            obj.IsTop = model.IsTop;
            obj.Tags = string.Join("|", model.SelectedMutliTags);
            _biz.UpdateArticle(obj);
            return RedirectToAction("Search");
        }

        public ActionResult Details(int id)
        {
            InitPage();
            var model = _biz.GetById(id);
            return View(model);
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetValidState(int id)
        {
            var model = _biz.GetById(id);
            model.IsValid = model.IsValid == 1 ? 0 : 1;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _biz.UpdateArticle(model);
            return RedirectToAction("Search");
        }

        public ActionResult LineSearch(string term, string _type, string q, string group, string outcity)
        {
            var CacheKey = "CacheKey=Tools|Search|List:1001";
            var _getModel = CacheContext.Current.Get(CacheKey);
            var products = new List<TpLineModel>();
            if (_getModel == null)
            {
                products = _searchBiz.GetAllLine(OwnerCode);
                CacheContext.Current.Add(CacheKey, products);
            }
            else
            {
                products = ((List<TpLineModel>)_getModel);
            }
            var query = products.AsQueryable();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(t => t.LineName.Contains(q));
            if (!string.IsNullOrEmpty(outcity))
                query = query.Where(t => t.DepartDest == outcity);
            if (!string.IsNullOrEmpty(group))
                query = query.Where(t => t.TeamID == group);

            var ll = (from cc in query
                      select new
                      {
                          id = cc.LineId,
                          text = cc.LineName
                      }).Take(20).ToList();

            var result = new
            {
                total_count = ll.Count,
                items = ll
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 页面初始化

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected override void InitPage()
        {
            ViewBag.NoticeTypes = DictionaryTools.GetEnumsBy(Enums.NoticeTypeEnum).ToSelectListFor();
            ViewBag.TagList = baseTagBiz.GetTags(UserInfo.OwnerCode, 3);
            ViewBag.DestList = new SelectList(DictionaryBiz.GetLineDestsCached(OwnerCode), "ArriveDestName", "ArriveDestName").ToList();
        }

        private string CheckLineFolder(int lineId)
        {
            var request = new UploadFileRequest();
            request.VirtualPath = @"article\" + lineId;
            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.CreateFolder(request);
            return response.FilePath + response.FileName;
        }

        #endregion 页面初始化
    }
}