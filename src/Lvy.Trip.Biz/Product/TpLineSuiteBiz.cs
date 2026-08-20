using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using System;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Product
{
    public class TpLineSuiteBiz : BaseBiz
    {
        private TpLineSuiteDao dao = new TpLineSuiteDao();

        public void AddTourPackage(TpLineSuiteModel model)
        {
            dao.Insert(model);
        }

        public int EditTourPackage(TpLineSuiteModel model)
        {
            return dao.Update(model);
        }

        public int DeleteTourPackage(string id)
        {
            return dao.Delete(Convert.ToInt32(id));
        }

        public List<TpLineSuiteModel> GetLineSuites(string lineId)
        {
            return dao.Fetch("SELECT * FROM TpLineSuites WHERE LineId=@0 ", lineId);
        }
    }
}