using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Product
{
    public class TpLineVisaBiz : BaseBiz
    {
        private TpLineVisaDao _dao = new TpLineVisaDao();

        /// <summary>
        /// 获取visa列表信息
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpLineVisaModel> GetTpLineVisaList(string lineId)
        {
            var sql = new Sql();
            sql.Append("  select * from TpLineVisa where lineId=@0 ", lineId);
            return _dao.Query<TpLineVisaModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 保存添加的签证信息数据.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int SaveLineVisa(TpLineVisaModel model)
        {
            var row = 0;
            if (model.Id == 0)
            {
                row = Convert.ToInt32(_dao.Insert(model));
            }
            else
            {
                row = _dao.Update(model);
            }
            return row;
        }

        public int deleteLineVisa(int id)
        {
            return _dao.Delete(id);
        }

        public TpLineVisaModel GetById(int id)
        {
            return _dao.GetById(id);
        }
    }
}