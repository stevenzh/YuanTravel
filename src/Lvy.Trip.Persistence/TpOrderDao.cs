using Lvy.Models.OrderDB;
using PetaPoco;
using System.Collections.Generic;

namespace Lvy.Trip.Dao.Order
{
    /// <summary>
    ///
    /// </summary>
    public class TpOrderDao : YuanDbRepository<TpOrderModel>
    {
        /// <summary>
        ///  获取订单信息
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public TpOrderModel GetOrder(string orderCode)
        {
            string sql = @" SELECT tt.*, cc.Name AS CustomerName
FROM TpOrder tt LEFT JOIN CrmCustomer cc ON tt.BookingCustomer= cc.Code WHERE tt.OrderCode=@0";
            return _repo.FirstOrDefault<TpOrderModel>(sql, Ansi(orderCode));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<TpOrderModel> GetOrders(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT o.*,t.* 
FROM TpTraveller t INNER JOIN TpOrder o ON t.OrderCode=o.OrderCode 
WHERE o.tourId=@0 AND o.IsCancel=0 AND t.State=2 ", tourId);

            return _repo.Fetch<TpOrderModel, TpTravellerModel, TpOrderModel>(new OrderToTravellerRelator().MapIt, sql.SQL, sql.Arguments);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class TpTravellerDao : YuanDbRepository<TpTravellerModel>
    {
        /// <summary>
        ///  获取有效游客信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<TpTravellerModel> GetTravellers(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(" SELECT * FROM TpTraveller where OrderCode=@0 And State=2", Ansi(orderCode));
            sql.Append(" order by State Asc ");

            return _repo.Fetch<TpTravellerModel>(sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///  获取所有游客信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<TpTravellerModel> GetAllTravellers(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(" SELECT * FROM TpTraveller where OrderCode=@0", Ansi(orderCode));
            sql.Append(" order by State Asc ");

            return _repo.Fetch<TpTravellerModel>(sql.SQL, sql.Arguments);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class TpOrderFileDao : YuanDbRepository<TpOrderFileModel> { }

    /// <summary>
    ///
    /// </summary>
    public class TpChildOrderDao : YuanDbRepository<TpChildOrderModel> { }

    /// <summary>
    ///
    /// </summary>
    public class TpInvoiceDao : YuanDbRepository<TpInvoiceModel> { }

    /// <summary>
    ///
    /// </summary>
    public class TpOrderPayInDao : YuanDbRepository<TpOrderPayInModel> { }
}