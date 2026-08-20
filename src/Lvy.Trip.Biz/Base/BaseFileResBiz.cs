using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using PetaPoco;
using System;

namespace Lvy.Trip.Biz.Base
{
    /// <summary>
    /// 文件资源业务层
    /// </summary>
    public class BaseFileResBiz : BaseBiz
    {
        private readonly BaseFileResDao _dao = new BaseFileResDao();

        /// <summary>
        /// 获取查询记录
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<BaseFileResModel> GetPager(FileResVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append("select * from BaseFileRes ")
                .Append(" where ownercode=@0", vModel.OwnerCode)
                .Append(" and IsValid=1");
            if (!vModel.QueryModel.FileName.IsNullOrEmpty())
            {
                sql.Append(" and FileName like @0", AnsiLike(vModel.QueryModel.FileName));
            }

            if (vModel.QueryModel.ResType > 0)
            {
                sql.Append(" and ResType=@0", vModel.QueryModel.ResType);
            }

            return _dao.Pager(vModel.FileResModels.PageIndex, vModel.FileResModels.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseFileResModel GetById(int id)
        {
            return _dao.GetById(id);
        }

        public int Add(BaseFileResModel model)
        {
            return _dao.Insert(model).ToInt();
        }

        public int Update(BaseFileResModel model)
        {
            return _dao.Update(model);
        }
    }
}