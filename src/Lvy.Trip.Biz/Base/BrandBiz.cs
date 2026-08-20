using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using PetaPoco;
using System;

namespace Lvy.Trip.Biz.Base
{
    public class BrandBiz : BaseBiz
    {
        private readonly BrandDao _dao = new BrandDao();

        public PagedList<BrandModel> GetBrandList(BrandVModel vModel)
        {
            var sql = new Sql();
            sql.Append(" SELECT * FROM BaseBrands WHERE OwnerCode=@0 AND IsValid=1 ", vModel.OwnerCode);
            if (!vModel.BrandModel.TeamID.IsNullOrEmpty())
            {
                sql.Append(" and TeamID = @0", vModel.BrandModel.TeamID);
            }
            if (!vModel.BrandModel.Name.IsNullOrEmpty())
            {
                sql.Append(" and Name like @0", AnsiLike(vModel.BrandModel.Name));
            }

            return _dao.Pager(vModel.BrandList.PageIndex, vModel.BrandList.PageSize, sql.SQL, sql.Arguments);
        }

        public object Add(BrandModel model)
        {
            return _dao.Insert(model);
        }

        public int Update(BrandModel model)
        {
            return _dao.Update(model);
        }

        public int Delete(int id)
        {
            return _dao.Update("SET IsValid=0 WHERE Id=@0 ", id);
        }

        public BrandModel GetBrandById(int id)
        {
            return _dao.GetById(id);
        }

        public bool CheckName(string name, string brandId)
        {
            var sql = new Sql();
            sql.Append("select count(1) from BaseBrands where Name=@0", Ansi(name));
            if (!brandId.IsNullOrEmpty())
                sql.Append(" and ID <>@0", Convert.ToInt32(brandId));

            long cnt = _dao.ExecuteScalar<long>(sql.ToString(), sql.Arguments);
            return cnt > 0;
        }
    }
}