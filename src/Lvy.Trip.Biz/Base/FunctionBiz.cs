using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Crm;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Base
{
    public class FunctionBiz : BaseBiz
    {
        private FunctionDao _functionDao = new FunctionDao();

        /// <summary>
        /// 获取模块集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<SysFunctionModel> GetPageList(FunctionVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(" select c.* ,a.Name as ModuleValue from SysFunction c left join SysFunction a   ")
                .Append(" on c.ParentId=a.Id  ")
                .Append(" where c.IsValid =1 ");
            if (!vModel.Function.ParentId.ToString().IsNullOrEmpty() && vModel.Function.ParentId != 0)
                sql.Append(" and c.ParentId=@0 ", vModel.Function.ParentId);
            if (!vModel.Function.FuncType.ToString().IsNullOrEmpty() && vModel.Type != 0)
                sql.Append(" and c.FuncType=@0 ", vModel.Type);
            sql.Append(" order by c.ModifiedTime desc,c.ParentId asc , c.Sort asc");
            var list = _functionDao.Pager(vModel.Functions.PageIndex, vModel.Functions.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        /// <summary>
        /// 获取所有所属模块名称
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetModuleNames()
        {
            return (from kv in _functionDao.GetModuleNames()
                    select new KeyValueBean() { Key = kv.Id.ToString(), Value = kv.Name }).ToList();
        }

        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public void AddModule(FunctionVModel vModel)
        {
            _functionDao.Insert(vModel.Function);
        }

        /// <summary>
        /// 获取功能模块的对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SysFunctionModel GetByModuleId(int id)
        {
            return _functionDao.GetById(id);
        }

        /// <summary>
        /// 根据parentId获取模块或菜单对象
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public SysFunctionModel GetByModuleParentId(int parentId)
        {
            return _functionDao.FirstOrDefault(@"SELECT * FROM SysFunction WHERE Id=@0", parentId);
        }

        /// <summary>
        /// 更新功能模块
        /// </summary>
        /// <param name="model"></param>
        /// <returns>返回1：true 0:false</returns>
        public int UpdateModule(SysFunctionModel model)
        {
            return _functionDao.Update(model);
        }
    }
}