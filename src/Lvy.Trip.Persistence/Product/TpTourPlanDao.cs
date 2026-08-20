using Lvy.Models.ProductDB;
using PetaPoco;
using System.Collections.Generic;

namespace Lvy.Trip.Dao.Product
{
    public class TpTourPlanDao : YuanDbRepository<TpTourPlanModel>
    {
        /// <summary>
        /// 取得线路有效 开班
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> TourPlanId(string lineId)
        {
            Sql sql = new Sql();
            sql.Append(@"select * from TpTourPlan where LineId=@0", lineId);

            return _repo.Fetch<TpTourPlanModel>(sql.SQL, sql.Arguments);
        }
    }

    public class QuotaDao : YuanDbRepository<QuotaModel> { }

    public class TourQuotaMapDao : YuanDbRepository<TourQuotaMapModel> { }

    public class TpPriceDao : YuanDbRepository<TpPriceModel>
    {
        /// <summary>
        /// 通过tourid获取价格数据
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<TpPriceModel> GetValidByTourId(int tourId)
        {
            Sql sql = new Sql();
            sql.Append("select * from TpPrice where IsValid=1");
            sql.Append(" and TourId=@0", tourId);

            return _repo.Fetch<TpPriceModel>(sql.SQL, sql.Arguments);
        }
    }

    public class TpTourFlightDao : YuanDbRepository<TpTourFlightModel> { }

    public class TpTourFileDao : YuanDbRepository<TpTourFileModel> { }
}