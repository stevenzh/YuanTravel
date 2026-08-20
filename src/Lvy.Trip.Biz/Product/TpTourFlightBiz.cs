using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using PetaPoco;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 开班对应航班
    /// </summary>
    public class TpTourFlightBiz : BaseBiz
    {
        private TpTourFlightDao dao = new TpTourFlightDao();

        public object Insert(TpTourFlightModel model)
        {
            return dao.Insert(model);
        }

        public int DeleteTourFlight(int tourId)
        {
            var sql = new Sql();
            sql.Append(" delete from TpTourFlight where TourId=@0 ", tourId);

            return dao.Execute(sql.SQL, sql.Arguments);
        }
    }
}