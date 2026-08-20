using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 权限管理模块
    /// </summary>
    public class PremissionBiz : BaseBiz
    {
        private RoleDao _roleDao = new RoleDao();
        private FunctionDao _functionDao = new FunctionDao();

        #region 角色

        /// <summary>
        /// 是否存在角色名称
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool CheckRoleName(string roleName, string ownerCode)
        {
            return _roleDao.CheckRoleName(roleName, ownerCode);
        }

        /// <summary>
        /// 获取一个角色对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SysRoleModel GetByRoleId(int id)
        {
            return _roleDao.GetById(id);
        }

        /// <summary>
        ///  添加角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns>返回对象id</returns>
        public int AddRole(SysRoleModel model)
        {
            return _roleDao.Insert(model).ToInt();
        }

        /// <summary>
        ///  更新角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns>返回1：true  0:false </returns>
        public int UpdateRole(SysRoleModel model)
        {
            return _roleDao.Update(model);
        }

        /// <summary>
        /// 查询出所有有效的角色
        /// </summary>
        /// <returns></returns>
        public List<SysRoleModel> SearchRole(RoleVModel vModel)
        {
            return _roleDao.SearchRole(vModel);
        }

        /// <summary>
        ///  设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SetValidStateByRole(int id, CrmAccountModel currentUser)
        {
            var obj = _roleDao.GetById(id);

            obj.IsValid = obj.IsValid == 1 ? 0 : 1;
            obj.ModifiedBy = currentUser.Code;
            obj.ModifiedTime = DateTime.Now;
            return _roleDao.Update(obj);
        }

        #endregion 角色

        #region 权限（角色管理功能）

        /// <summary>
        /// 获取所有有效的模块菜单功能数据
        /// </summary>
        /// <returns></returns>
        public List<SysFunctionModel> GetAllFunctions()
        {
            return _functionDao.GetAll();
        }

        /// <summary>
        /// 通过角色id获取权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public string[] GetPermissionByRoleId(int roleId)
        {
            string sql = " select FunctionId from SysPermissionMap where roleId=@0";
            return _functionDao.Query<string>(sql, roleId).ToArray();
        }

        /// <summary>
        /// 保存权限
        /// </summary>
        /// <returns></returns>
        public int SavePermission(PermissionVModel vModel)
        {
            using (var ts = _functionDao.GetTransaction())
            {
                _functionDao.Execute("delete from SysPermissionMap where RoleId=@0", vModel.RoleId);

                if (vModel.FuncIds != null)
                {
                    foreach (var item in vModel.FuncIds)
                    {
                        string sql = "Insert into SysPermissionMap (RoleId,FunctionId) values (@0,@1) ";

                        _functionDao.Execute(sql, vModel.RoleId, item);
                    }
                }
                ts.Complete();
            }

            return 100;
        }

        #endregion 权限（角色管理功能）
    }
}