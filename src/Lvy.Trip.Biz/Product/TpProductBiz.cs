using Lvy.Models;
using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Product;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Product
{
    /// <summary>
    /// 子产品
    /// </summary>
    public class TpProductBiz : BaseBiz
    {
        private readonly TpProductDao dao = new TpProductDao();

        public List<TpProductModel> GetProductByTeam(string teamCode)
        {
            var sql = new Sql();
            sql.Append(@"SELECT tp.*, cc.Name AS SupplierName, bdd.Value AS ProductTypeName
FROM TpProducts tp
LEFT JOIN CrmCustomer cc ON tp.SupplierCode = cc.Code
LEFT JOIN BaseDictionaryDetail bdd ON tp.ProductType=bdd.`Key` AND bdd.Name = 'SupplierCostItemsEnum'
WHERE tp.IsValid = 1 AND tp.TeamCode=@0 ", teamCode);

            return dao.Query(sql.SQL, sql.Arguments).ToList();
        }

        public PagedList<TpProductModel> GetPagedProduct(ProductVModel vModel)
        {
            var sql = new Sql();
            sql.Append(@"SELECT tp.*, ct.TeamName, cc.Name AS SupplierName, bdd.Value AS ProductTypeName
FROM TpProducts tp
LEFT JOIN CrmTeam ct ON ct.TeamID = tp.TeamCode
LEFT JOIN CrmCustomer cc ON tp.SupplierCode = cc.Code
LEFT JOIN BaseDictionaryDetail bdd ON tp.ProductType=bdd.`Key` AND bdd.Name = 'SupplierCostItemsEnum'
WHERE tp.OwnerCode=@0 AND tp.IsValid = 1 ", vModel.OwnerCode);

            if (!vModel.ProductModel.ProductName.IsNullOrEmpty())
            {
                sql.Append(" and ProductName like @0", AnsiLike(vModel.ProductModel.ProductName));
            }
            if (!vModel.ProductModel.TeamCode.IsNullOrEmpty())
            {
                sql.Append(" and TeamCode = @0", Ansi(vModel.ProductModel.TeamCode));
            }

            return dao.Pager(vModel.ProductPageList.PageIndex, vModel.ProductPageList.PageSize, sql.SQL, sql.Arguments);
        }

        public object Add(TpProductModel model)
        {
            return dao.Insert(model);
        }

        public int Update(TpProductModel model)
        {
            return dao.Update(model);
        }

        public int Delete(int id)
        {
            return dao.Delete(id);
        }

        public TpProductModel GetProductById(int id)
        {
            return dao.GetById(id);
        }

        public bool CheckProductName(string name, string Id)
        {
            var sql = new Sql();
            sql.Append(" SELECT COUNT(*) FROM TpProducts WHERE ProductName=@0", name);
            if (!Id.IsNullOrEmpty())
            {
                sql.Append(" AND ProductID<>@0 ", Id);
            }
            long cnt = dao.ExecuteScalar<long>(sql.ToString(), sql.Arguments);
            return cnt > 0;
        }
    }
}