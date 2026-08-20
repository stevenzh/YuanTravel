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
    public class BaseFileBiz : BaseBiz
    {
        private readonly BaseFileDao _dao = new BaseFileDao();

        /// <summary>
        /// 获取查询记录
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<BaseFileModel> GetPager(FileVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append("select * from BaseFiles ")
                .Append(" where ownercode=@0", vModel.QueryModel.OwnerCode)
                .Append(" and IsValid=1");
            if (!vModel.QueryModel.FileName.IsNullOrEmpty())
            {
                sql.Append(" and FileName like @0", AnsiLike(vModel.QueryModel.FileName));
            }

            return _dao.Pager(vModel.FileModels.PageIndex, vModel.FileModels.PageSize, sql.SQL, sql.Arguments);
        }

        public BaseFileModel GetById(int id)
        {
            return _dao.GetById(id);
        }

        public int Add(BaseFileModel model)
        {
            return _dao.Insert(model).ToInt();
        }

        public int Update(BaseFileModel model)
        {
            return _dao.Update(model);
        }

        public int ExistPath(string path)
        {
            return _dao.Fetch("select * from BaseFiles where Path=@0 ", path).Count;
        }
    }
}