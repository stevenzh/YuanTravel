using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using System;

namespace Lvy.Trip.Biz.Product
{
    public class TpLineTagMapBiz : BaseBiz
    {
        private readonly TpLineTagMapDao _dao = new TpLineTagMapDao();

        public int Insert(TpLineTagMapModel model)
        {
            return Convert.ToInt32(_dao.Insert(model));
        }
    }
}