using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    public class PermissionController : BaseController
    {
        private PremissionBiz _biz = new PremissionBiz();

        #region 角色

        /// <summary>
        /// 查询角色
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchRole()
        {
            return View("Role/Search", GetRoleVModel());
        }

        /// <summary>
        ///  添加角色
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        public ActionResult AddRole(SysRoleModel roleModel)
        {
            roleModel.IsValid = 1;
            roleModel.CreatedBy = UserInfo.Code;
            roleModel.CreatedTime = DateTime.Now;
            roleModel.ModifiedBy = UserInfo.Code;
            roleModel.ModifiedTime = DateTime.Now;
            roleModel.OwnerCode = OwnerCode;
            _biz.AddRole(roleModel);
            return PartialView("Role/UCRoles", GetRoleVModel());
        }

        /// <summary>
        ///  更新角色
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        public ActionResult UpdateRole(SysRoleModel roleModel)
        {
            var model = _biz.GetByRoleId(roleModel.Id);
            model.Name = roleModel.Name;
            model.Description = roleModel.Description;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;

            _biz.UpdateRole(model);
            return PartialView("Role/UCRoles", GetRoleVModel());
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditRole(int id)
        {
            var role = _biz.GetByRoleId(id);

            return PartialView("Role/UCEditRole", role);
        }

        /// <summary>
        ///  有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetValidStateByRole(int id)
        {
            _biz.SetValidStateByRole(id, UserInfo);
            return RedirectToAction("SearchRole");
        }

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckRoleName(string roleName)
        {
            bool flag = _biz.CheckRoleName(roleName, UserInfo.OwnerCode);
            return Json(flag);
        }

        private RoleVModel GetRoleVModel()
        {
            RoleVModel vModel = new RoleVModel();
            vModel.OwnerCode = GlobalContext.Current.OwnerCode;
            vModel.Roles = _biz.SearchRole(vModel);
            return vModel;
        }

        #endregion 角色

        #region 角色功能

        /// <summary>
        /// 设置权限
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SetPermission()
        {
            RoleVModel vModel1 = new RoleVModel();
            vModel1.OwnerCode = GlobalContext.Current.OwnerCode;
            vModel1.IsValid = 1;
            PermissionVModel vModel = new PermissionVModel();
            vModel.RoleBeans = (from role in _biz.SearchRole(vModel1)
                                select new KeyValueBean()
                                {
                                    Key = role.Id.ToString(),
                                    Value = role.Name
                                }).ToList();

            vModel.Functions = _biz.GetAllFunctions();

            var aa = vModel.Functions.Where(a => a.FuncType == 1);
            var bb = vModel.Functions.Where(a => a.FuncType == 2);
            var cc = vModel.Functions.Where(a => a.FuncType == 5);

            return View("RoleFunc/SetPermission", vModel);
        }

        /// <summary>
        ///  勾选已关联的功能
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public ActionResult SelectedFunctionsByRoleId(int roleId)
        {
            var obj = _biz.GetPermissionByRoleId(roleId);
            return Json(obj);
        }

        /// <summary>
        /// 保存权限
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SavePermission(PermissionVModel vModel)
        {
            var flag = _biz.SavePermission(vModel);
            return Content(flag.ToString());
        }

        #endregion 角色功能
    }
}