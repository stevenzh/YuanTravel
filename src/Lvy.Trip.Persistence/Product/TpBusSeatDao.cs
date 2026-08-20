using Lvy.Models.ProductDB;

namespace Lvy.Trip.Dao.Product
{
    public class TpBusSeatDao : YuanDbRepository<TpBusSeatModel>
    {
        /// <summary>
        /// 保存座位详细
        /// </summary>
        /// <param name="quotaId"></param>
        /// <param name="jsonSeats"></param>
        /// <returns></returns>
        public int UpdateSeatDetail(int quotaId, string jsonSeats)
        {
            return _repo.Update<TpBusSeatModel>(" SET SeatDetail=@0 WHERE QuotaId=@1", Ansi(jsonSeats), quotaId);
        }
    }
}