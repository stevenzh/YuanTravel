using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Product
{
    public class TpPriceBiz : BaseBiz
    {
        private readonly TpPriceDao _priceDao = new TpPriceDao();

        /// <summary>
        /// 获取一个报价对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TpPriceModel GetById(int id)
        {
            return _priceDao.GetById(id);
        }

        /// <summary>
        /// 获取报价对象列表
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public List<TpPriceModel> GetPrices(int tourId)
        {
            return _priceDao.Fetch(@"SELECT tp.Id, tp.TourId, tp.PriceType, tp.PriceTypeName, tp.PriceRemark, tp.Price, tp.SettlePrice, tp.Cost,
ttp.Tips, ttp.ZiFei, ttp.SingleRoom, tp.TeJiaFanLi, tp.IsStandard, tp.IsValid, tp.SuitNum, tp.ModifiedBy, tp.ModifiedTime
FROM TpPrice tp inner join TpTourPlan ttp on tp.TourId=ttp.Id
WHERE tp.TourId=@0", tourId);
        }

        /// <summary>
        /// 获取有效报价对象列表
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public List<TpPriceModel> GetValidPrices(int tourId)
        {
            return _priceDao.Fetch(@"SELECT tp.Id, tp.TourId, tp.PriceType, tp.PriceTypeName, tp.PriceRemark, tp.Price, tp.SettlePrice, tp.Cost,
ttp.Tips, ttp.ZiFei, ttp.SingleRoom, tp.TeJiaFanLi, tp.IsStandard, tp.IsValid, tp.SuitNum, tp.ModifiedBy, tp.ModifiedTime
FROM TpPrice tp inner join TpTourPlan ttp on tp.TourId=ttp.Id
WHERE tp.IsValid=1 and tp.TourId=@0", tourId);
        }

        ///// <summary>
        ///// 获取标准价
        ///// </summary>
        ///// <param name="tourId">团计划Id</param>
        ///// <returns></returns>
        //public TpPriceModel GetStandard(int tourId)
        //{
        //    return _priceDao.FirstOrDefault(@"SELECT Id,TourId,PriceType,PriceTypeName,PriceRemark,Price,Cost,Tips,ZiFei,SingleRoom,TeJiaFanLi,IsStandard,IsValid,SuitNum,ModifiedBy,ModifiedTime FROM TpPrice WHERE IsStandard=1 AND TourId=@0", tourId);
        //}
    }
}