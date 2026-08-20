using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 平台功能-模块、菜单和功能控制器
    /// </summary>
    public class ModuleController : BaseController
    {

        private FunctionBiz _functionBiz = new FunctionBiz();

        /// <summary>
        /// 查询模块-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Search(FunctionVModel vModel)
        {
            vModel.Functions.PageSize = 10;
            vModel.Functions = _functionBiz.GetPageList(vModel);

            InitPage();
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetValidState(int id)
        {
            var model = _functionBiz.GetByModuleId(id);
            model.IsValid = model.IsValid ? false : true;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _functionBiz.UpdateModule(model);
            return RedirectToAction("Search");
        }

        #region 模块操作

        /// <summary>
        /// 添加模块-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 添加模块-保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult Add(FunctionVModel vModel)
        {
            vModel.Function.IsValid = true;
            vModel.Function.ModifiedBy = UserInfo.Code;
            vModel.Function.ModifiedTime = DateTime.Now;
            vModel.Function.CreatedBy = UserInfo.Code;
            vModel.Function.CreatedTime = DateTime.Now;
            vModel.Function.ParentId = 0;
            vModel.Function.FuncType = 1;

            _functionBiz.AddModule(vModel);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 编辑模块-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            //InitPage();
            var vModel = new FunctionVModel();
            vModel.Function = _functionBiz.GetByModuleId(id);
            return View(vModel);
        }

        /// <summary>
        /// 编辑模块-保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult Update(FunctionVModel vModel)
        {
            var obj = _functionBiz.GetByModuleId(vModel.Function.Id);
            //obj.ParentId = vModel.Function.ParentId;
            //obj.FuncType = vModel.Function.FuncType;
            obj.Name = vModel.Function.Name;
            //obj.URL = vModel.Function.URL;
            obj.Sort = vModel.Function.Sort;
            obj.IsSuper = vModel.Function.IsSuper;
            obj.Description = vModel.Function.Description;
            obj.IconClass = vModel.Function.IconClass;
            obj.ModifiedBy = UserInfo.Code;
            obj.ModifiedTime = DateTime.Now;

            _functionBiz.UpdateModule(obj);

            return RedirectToAction("Search");
        }

        #endregion 模块操作

        #region 菜单操作

        /// <summary>
        /// 添加菜单-视图
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateMenu(int id)
        {
            var vModel = new FunctionVModel();
            vModel.Function = _functionBiz.GetByModuleId(id);
            vModel.Function.Description = "";
            vModel.Function.Name = "";
            vModel.Function.URL = "";
            vModel.Function.Sort = 0;
            vModel.Function.IsSuper = 0;
            InitPage();
            return View(vModel);
        }

        /// <summary>
        /// 添加菜单-保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AddMenu(FunctionVModel vModel)
        {
            vModel.Function.IsValid = true;
            vModel.Function.ModifiedBy = UserInfo.Code;
            vModel.Function.ModifiedTime = DateTime.Now;
            vModel.Function.CreatedBy = UserInfo.Code;
            vModel.Function.CreatedTime = DateTime.Now;
            vModel.Function.ParentId = vModel.Function.Id;
            vModel.Function.FuncType = 2;

            _functionBiz.AddModule(vModel);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 编辑菜单-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditMenu(int id)
        {
            InitPage();
            var vModel = new FunctionVModel();
            vModel.Function = _functionBiz.GetByModuleId(id);
            return View(vModel);
        }

        /// <summary>
        /// 编辑菜单-保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult UpdateMenu(FunctionVModel vModel)
        {
            var obj = _functionBiz.GetByModuleId(vModel.Function.Id);
            obj.ParentId = vModel.Function.ParentId;
            obj.Name = vModel.Function.Name;
            obj.URL = vModel.Function.URL;
            obj.Sort = vModel.Function.Sort;
            obj.IsSuper = vModel.Function.IsSuper;
            obj.Description = vModel.Function.Description;
            obj.IconClass = vModel.Function.IconClass;

            obj.ModifiedBy = UserInfo.Code;
            obj.ModifiedTime = DateTime.Now;

            _functionBiz.UpdateModule(obj);

            return RedirectToAction("Search");
        }

        #endregion 菜单操作

        #region 功能操作

        /// <summary>
        /// 添加功能-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CreateFunction(int id)
        {
            var vModel = new FunctionVModel();
            vModel.Function = _functionBiz.GetByModuleId(id);
            vModel.ModuleName = _functionBiz.GetByModuleParentId(vModel.Function.ParentId).Name;
            vModel.MenuName = vModel.Function.Name;
            vModel.Function.Description = "";
            vModel.Function.Name = "";
            vModel.Function.URL = "";
            vModel.Function.Sort = 0;
            vModel.Function.IsSuper = 0;
            InitPage();
            return View(vModel);
        }

        /// <summary>
        /// 添加功能-保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AddFunction(FunctionVModel vModel)
        {
            vModel.Function.IsValid = true;
            vModel.Function.ModifiedBy = UserInfo.Code;
            vModel.Function.ModifiedTime = DateTime.Now;
            vModel.Function.CreatedBy = UserInfo.Code;
            vModel.Function.CreatedTime = DateTime.Now;
            vModel.Function.ParentId = vModel.Function.Id;
            vModel.Function.FuncType = 5;

            _functionBiz.AddModule(vModel);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 编辑功能-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditFunction(int id)
        {
            InitPage();
            var vModel = new FunctionVModel();
            vModel.Function = _functionBiz.GetByModuleId(id);
            //根据功能的id获取相应的 菜单名称 和 模块名称
            var tempVModel = _functionBiz.GetByModuleParentId(vModel.Function.ParentId);
            vModel.MenuName = tempVModel.Name;
            vModel.ModuleName = _functionBiz.GetByModuleParentId(tempVModel.ParentId).Name;
            return View(vModel);
        }

        /// <summary>
        /// 编辑功能-保存
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult UpdateFunction(FunctionVModel vModel)
        {
            var obj = _functionBiz.GetByModuleId(vModel.Function.Id);

            obj.Name = vModel.Function.Name;
            obj.URL = vModel.Function.URL;
            obj.Sort = vModel.Function.Sort;
            obj.Description = vModel.Function.Description;
            obj.IsValid = vModel.Function.IsValid;
            obj.ModifiedBy = UserInfo.Code;
            obj.ModifiedTime = DateTime.Now;

            _functionBiz.UpdateModule(obj);

            return RedirectToAction("Search");
        }

        #endregion 功能操作

        #region 页面初始化

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected override void InitPage()
        {
            //所属模块数据加载
            ViewBag.ModuleBeans = _functionBiz.GetModuleNames().ToSelectListFor(k => k.Key, v => v.Value);
            ViewBag.FuncType = DictionaryTools.GetEnumsBy(Enums.FuncTypeEnum).ToSelectListFor();
        }

        #endregion 页面初始化
    }
}