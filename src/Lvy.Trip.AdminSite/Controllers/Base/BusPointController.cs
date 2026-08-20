using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 上车点管理
    /// </summary>
    public class BusPointController : BaseController
    {
        private readonly BusPointBiz _biz = new BusPointBiz();

        #region 上车点维护

        #region 上车点列表

        /// <summary>
        /// 查询上车点列表
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchBusPoint(BusPointVModel vModel)
        {
            if (vModel.PagedModel == null)
                vModel.PagedModel = new PagedList<BaseBusPointModel>();
            vModel.OwnerCode = GlobalContext.Current.OwnerCode;
            vModel.PagedModel = _biz.GetPagedBusPoint(vModel);
            vModel.GroupList = _biz.GetGroupItems(vModel.OutCity, GlobalContext.Current.OwnerCode);

            ViewData["CityList"] = DictionaryTools.GetEnumsBy(Enums.OutCityEnum).ToSelectListFor();

            if (Request.IsAjaxRequest())
                return PartialView("UCBusPointList", vModel);
            return View(vModel);
        }

        #endregion 上车点列表

        #region 新增上车点

        /// <summary>
        /// 新增上车点（初始化）
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateBusPoint()
        {
            InitGroupOption();
            var vModel = new EditBusPointVModel
            {
                BusPoint = new BaseBusPointModel
                {
                    JsType = 1
                }
            };
            return View(vModel);
        }

        /// <summary>
        /// 新增上车点(保存）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateBusPoint(EditBusPointVModel vModel, string[] groupIds)
        {
            var model = vModel.BusPoint;
            model.GroupId = groupIds.Any() ? groupIds.Join("|") : null;
            model.OwnerCode = UserInfo.OwnerCode;
            model.IsValid = 1;
            model.Remark = string.Empty;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _biz.AddBusPoint(model);

            return Json(new { Code = "1", Message = "" });
        }

        #endregion 新增上车点

        #region 编辑上车点

        /// <summary>
        /// 编辑上车点（初始化）
        /// </summary>
        /// <returns></returns>
        public ActionResult EditBusPoint(int id = 0)
        {
            BaseBusPointModel model = _biz.GetBusPoint(id);
            if (model == null) return null;
            InitGroupOption();
            var vModel = new EditBusPointVModel
            {
                BusPoint = model
            };
            return View(vModel);
        }

        /// <summary>
        /// 编辑上车点（保存 ）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditBusPoint(EditBusPointVModel vModel, string[] groupIds)
        {
            var entity = _biz.GetBusPoint(vModel.BusPoint.Id);
            entity.BusPoint = vModel.BusPoint.BusPoint;
            entity.JieSongTime = vModel.BusPoint.JieSongTime;
            entity.JsType = vModel.BusPoint.JsType;
            entity.IsValid = vModel.BusPoint.IsValid;
            entity.GroupId = groupIds.Any() ? groupIds.Join("|") : null;
            entity.ModifiedBy = UserInfo.Code;
            entity.ModifiedTime = DateTime.Now;
            entity.OutCity = vModel.BusPoint.OutCity;
            _biz.UpdateBusPoint(entity);

            return Json(new { Code = "1", Message = "" });
        }

        #endregion 编辑上车点

        #region 删除上车点

        /// <summary>
        /// 删除上车点
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteBusPoint(int id)
        {
            BaseBusPointModel model = _biz.GetBusPoint(id);
            model.IsValid = 0;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _biz.UpdateBusPoint(model);
            var vModel = new BusPointVModel
            {
                OwnerCode = GlobalContext.Current.OwnerCode,
                PagedModel = new PagedList<BaseBusPointModel>()
            };
            vModel.PagedModel = _biz.GetPagedBusPoint(vModel);
            return PartialView("UCBusPointList", vModel);
        }

        #endregion 删除上车点

        #endregion 上车点维护

        #region 私有方法

        private void InitGroupOption()
        {
            ViewData["CityList"] = DictionaryTools.GetEnumsBy(Enums.OutCityEnum).ToSelectListFor();
            ViewBag.GroupList = _biz.GetGroupList("", GlobalContext.Current.OwnerCode);
        }

        #endregion 私有方法

        #region 上车点分组

        public ActionResult ReloadGroup(string outCity)
        {
            var vModel = new BusPointVModel
            {
                GroupList = _biz.GetGroupItems(outCity, GlobalContext.Current.OwnerCode)
            };

            return PartialView("UCGroups", vModel);
        }

        public ActionResult EditGroup(int groupId = 0)
        {
            var model = new BusPointGroupModel();
            if (groupId != 0)
                model = _biz.GetGroupById(groupId);
            ViewData["CityList"] = DictionaryTools.GetEnumsBy(Enums.OutCityEnum).ToSelectListFor();

            return PartialView("UCEditGroup", model);
        }

        public ActionResult SaveGroup(BusPointGroupModel model)
        {
            if (model.Id > 0)
            {
                _biz.UpdateGroup(model, GlobalContext.Current.UserInfo);
            }
            else
            {
                _biz.AddGroup(model, GlobalContext.Current.UserInfo);
            }
            return Json(new { Code = "1", Message = "" });
        }

        public ActionResult DeleteGroup(int groupId = 0)
        {
            _biz.DeleteGroup(groupId, GlobalContext.Current.UserInfo);
            return Json(new { Code = "1", Message = "" });
        }

        #endregion 上车点分组
    }
}