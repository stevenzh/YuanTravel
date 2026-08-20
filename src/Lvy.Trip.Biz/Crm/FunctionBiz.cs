using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 系统功能
    /// </summary>
    public class FunctionBiz : BaseBiz
    {
        private FunctionDao _dao = new FunctionDao();

        /// <summary>
        /// 获取模块集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<SysFunctionModel> GetPageList(FunctionVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(" select c.* ,a.Name as ModuleValue from SysFunction c left join SysFunction a   ")
                .Append(" on c.ParentId=a.Id  where 1=1 ");
            //   .Append(" where c.IsValid =1 ");
            if (!vModel.Function.ParentId.ToString().IsNullOrEmpty() && vModel.Function.ParentId != 0)
                sql.Append(" and c.ParentId=@0 ", vModel.Function.ParentId);
            if (!vModel.Function.FuncType.ToString().IsNullOrEmpty() && vModel.Type != 0)
                sql.Append(" and c.FuncType=@0 ", vModel.Type);
            sql.Append(" order by c.ModifiedTime desc,c.ParentId asc, c.Sort asc");
            var list = _dao.Pager(vModel.Functions.PageIndex, vModel.Functions.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        /// <summary>
        /// 根据账户编号获取该账号对应的功能权限
        /// </summary>
        /// <param name="accountCode">账户编号</param>
        /// <returns></returns>
        public IList<SysFunctionModel> GetFunctionByAccountCode(CrmAccountModel model)
        {
            var funAll = _dao.GetAll().Where(t => t.IsSuper == 0 && t.IsValid).OrderBy(a => a.Sort).ToList();

            if (model.AccountType == 1)  //平台管理员
            {
                return _dao.GetAll().Where(t => t.IsValid).OrderBy(a => a.Sort).ToList();
            }
            if (model.AccountType == 2)  //系统管理员
            {
                return funAll;
            }
            var sql = new Sql();

            sql.Append(" select distinct( moduleFun.Name) as ModuleValue,fun.* from CrmAccount account ")
                .Append(" inner join SysUserRoleMap roleMap on account.Code=roleMap.AccountCode ")
                .Append(" inner join SysPermissionMap PerMap on roleMap.RoleId=PerMap.RoleId  ")
                .Append(" inner join SysFunction fun on PerMap.FunctionId=fun.Id ")
                .Append(" inner join SysFunction moduleFun ")
                .Append(" on fun.ParentId=moduleFun.Id ")
                .Append(" where  account.Code=@0 and fun.IsValid=1 ", model.Code)
                .Append(" order by ParentId asc ");
            var modules = funAll.Where(a => a.FuncType == 1);
            var funParts = _dao.Query<SysFunctionModel>(sql.SQL, sql.Arguments).ToList();
            funParts.AddRange(modules);
            return funParts;
        }

        /// <summary>
        /// 获取所有所属模块名称
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetModuleNames()
        {
            return (from kv in _dao.GetModuleNames()
                    select new KeyValueBean() { Key = kv.Id.ToString(), Value = kv.Name }).ToList();
        }

        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public int AddModule(FunctionVModel vModel)
        {
            return _dao.Insert(vModel.Function).ToInt();
        }

        /// <summary>
        /// 获取功能模块的对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SysFunctionModel GetByModuleId(int id)
        {
            return _dao.GetById(id);
        }

        /// <summary>
        /// 根据parentId获取模块或菜单对象
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public SysFunctionModel GetByModuleParentId(int parentId)
        {
            return _dao.FirstOrDefault(@"SELECT * FROM SysFunction WHERE Id=@0", parentId);
        }

        /// <summary>
        /// 更新功能模块
        /// </summary>
        /// <param name="model"></param>
        /// <returns>返回1：true 0:false</returns>
        public int UpdateModule(SysFunctionModel model)
        {
            return _dao.Update(model);
        }
    }
}