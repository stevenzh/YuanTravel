using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;

namespace Lvy.Trip.Biz.Product
{
    /// <summary>
    /// 座位
    /// </summary>
    public class TpBusSeatBiz : BaseBiz
    {
        private readonly TpBusSeatDao _dao = new TpBusSeatDao();

        /// <summary>
        /// 获取车座信息
        /// </summary>
        /// <param name="busSeatId">座位表Id</param>
        /// <returns></returns>
        public TpBusSeatModel GetBusSeat(int busSeatId)
        {
            return _dao.GetById(busSeatId);
        }

        ///// <summary>
        ///// 获取车座信息
        ///// </summary>
        ///// <param name="quotaId">库存Id</param>
        ///// <returns></returns>
        //public TpBusSeatModel GetByQuota(int quotaId)
        //{
        //    return _dao.SingleOrDefault("SELECT * FROM TpBusSeat WHERE QuotaId=@0", quotaId);
        //}

        /// <summary>
        /// 获取车座信息
        /// </summary>
        /// <param name="quotaId">库存Id</param>
        /// <returns></returns>
        public TpBusSeatModel GetByShareQuota(int quotaId)
        {
            //return _dao.SingleOrDefault("SELECT * FROM TpBusSeat WHERE QuotaId=@0 AND TourId=0", quotaId);
            return _dao.FirstOrDefault("SELECT * FROM TpBusSeat WHERE QuotaId=@0", quotaId);
        }

        /// <summary>
        /// 获取车座信息
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <param name="quotaId">资源Id</param>
        /// <returns></returns>
        public TpBusSeatModel GetBusSeat(int tourId, int quotaId)
        {
            // return _dao.FirstOrDefault(@"SELECT Id,TourId,SeatNum,SeatDetail,QuotaId FROM TpBusSeat WHERE TourId=@0 AND QuotaId=@1", tourId, quotaId);
            return _dao.FirstOrDefault(@"SELECT Id,TourId,SeatNum,SeatDetail,QuotaId FROM TpBusSeat WHERE QuotaId=@0", quotaId);
        }

        /// <summary>
        /// 根据团计划Id获取座位表
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public TpBusSeatModel GetBusSeatByTour(int tourId)
        {
            return _dao.FirstOrDefault(@"SELECT TpBusSeat.Id,TpBusSeat.TourId,TpBusSeat.SeatNum,TpBusSeat.SeatDetail,TpBusSeat.QuotaId FROM TpBusSeat INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.QuotaId=TpBusSeat.QuotaId WHERE TpTourQuotaMap.TourId=@0", tourId);
        }
    }
}