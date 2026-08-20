using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Base;
using Lvy.VModels;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 平台功能-词典管理
    /// </summary>
    public class DictionaryController : Controller
    {
        private DictBiz _dicBiz = new DictBiz();

        #region 查询

        public ActionResult Search()
        {
            var list = _dicBiz.GetList();
            return View(list);
        }

        #endregion 查询

        #region 添加

        /// <summary>
        /// 添加字典-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return View("Create");
        }

        /// <summary>
        /// 添加字典-保存
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public ActionResult AddDictionary(BaseDictionaryModel collection)
        {
            _dicBiz.AddDictionary(collection);
            return RedirectToAction("Search");
        }

        #endregion 添加

        #region 编辑

        /// <summary>
        /// 修改字典-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var obj = _dicBiz.GetById(id);
            return View(obj);
        }

        /// <summary>
        /// 修改字典-保存
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public ActionResult UpdateDictionary(BaseDictionaryModel collection)
        {
            _dicBiz.UpdateDictionary(collection);
            return RedirectToAction("Search");
        }

        #endregion 编辑

        #region 设置字典是否有效

        public ActionResult SetValidStateByDictionary(int id)
        {
            _dicBiz.SetValidStateByDictionary(id);
            return RedirectToAction("Search");
        }

        #endregion 设置字典是否有效

        #region 设置字典属性

        #region 查询

        public ActionResult SearchDictionaryDetail(int dicId)
        {
            var vModel = new DictionaryVModel();
            TempData["dicId"] = dicId;
            vModel.DetailModels = _dicBiz.GetDetailList(dicId);
            vModel.DetailModel = new BaseDictionaryDetailModel();
            return View(vModel);
        }

        #endregion 查询

        #region 修改和添加

        /// <summary>
        /// 修改字典属性-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditDictionaryDetail(int id)
        {
            var vModel = new DictionaryVModel();
            vModel.DetailModel = _dicBiz.GetByDetailId(id);
            vModel.DetailModels = _dicBiz.GetDetailList(vModel.DetailModel.DicId);
            return View("SearchDictionaryDetail", vModel);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveDictionaryDetail(DictionaryVModel model)
        {
            model.DetailModel.DicId = TempData["dicId"].ToInt();
            var obj = _dicBiz.GetById(model.DetailModel.DicId);
            model.DetailModel.Name = obj.Name;
            if (model.DetailModel.Id <= 0)
            {
                model.DetailModel.IsValid = 1;
                // add
                _dicBiz.AddDictionaryDetail(model.DetailModel);
                return RedirectToAction("SearchDictionaryDetail", new { dicId = model.DetailModel.DicId });
            }
            else
            {
                // update
                _dicBiz.UpdateDictionaryDetail(model.DetailModel);
                return RedirectToAction("SearchDictionaryDetail", new { dicId = model.DetailModel.DicId });
            }
        }

        #endregion 修改和添加

        #region 设置字典属性是否有效

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetValidStateByDictionaryDetail(int id, int dicId)
        {
            _dicBiz.SetValidStateByDictionaryDetail(id);
            return RedirectToAction("SearchDictionaryDetail", new { dicId = dicId });
        }

        #endregion 设置字典属性是否有效

        #endregion 设置字典属性
    }
}