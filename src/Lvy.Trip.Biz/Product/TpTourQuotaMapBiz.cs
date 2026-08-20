using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.OpTour;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Product
{
    public class TpTourQuotaMapBiz : BaseBiz
    {
        private readonly TourQuotaMapDao _dao = new TourQuotaMapDao();

        /// <summary>
        /// 获取团-资源对应表实体
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public TourQuotaMapModel GetMap(int tourId)
        {
            return _dao.SingleOrDefault(@"SELECT * FROM TpTourQuotaMap WHERE TourId=@0", tourId);
        }

        /// <summary>
        /// 获取团-资源对应表实体
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <param name="quotaId">库存Id</param>
        /// <returns></returns>
        public TourQuotaMapModel GetMap(int tourId, int quotaId)
        {
            return _dao.SingleOrDefault(@"SELECT * FROM TpTourQuotaMap WHERE TourId=@0 AND QuotaId=@1", tourId, quotaId);
        }

        /// <summary>
        /// 获取团-资源对应表实体(关联资源）
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public TourQuotaMapModel GetMapWithQuota(int tourId)
        {
            return _dao.Query<TourQuotaMapModel, QuotaModel>(@"SELECT TpTourQuotaMap.*,TpQuota.* FROM TpTourQuotaMap INNER JOIN TpQuota ON TpQuota.Id=TpTourQuotaMap.QuotaId WHERE TpTourQuotaMap.TourId=@0", tourId).SingleOrDefault();
        }

        /// <summary>
        /// 获取团-资源对应表实体(关联资源）
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public TourQuotaMapModel GetMapWithTour(int tourId)
        {
            return _dao.Query<TourQuotaMapModel, TpTourPlanModel>(@"SELECT TpTourQuotaMap.*,TpTourPlan.* FROM TpTourQuotaMap INNER JOIN TpTourPlan ON TpTourPlan.Id=TpTourQuotaMap.TourId WHERE TpTourQuotaMap.TourId=@0", tourId).SingleOrDefault();
        }

        /// <summary>
        /// 获取团-Map-资源对应表实体(关联资源）
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public TourQuotaMapModel GetMapWithAll(int tourId)
        {
            return _dao.Query<TourQuotaMapModel, TpTourPlanModel, QuotaModel>(@"SELECT TpTourQuotaMap.*,TpTourPlan.*,TpQuota.* FROM TpTourQuotaMap INNER JOIN TpTourPlan ON TpTourPlan.Id=TpTourQuotaMap.TourId INNER JOIN TpQuota ON TpQuota.Id=TpTourQuotaMap.QuotaId WHERE TpTourQuotaMap.TourId=@0", tourId).SingleOrDefault();
        }

        #region 换团

        /// <summary>
        /// 获取换团列表
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public List<TourQuotaMapModel> GetExchangeTours(ExchangeTourVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT M.*,T.*,Q.*
FROM TpTourQuotaMap M INNER JOIN TpTourPlan T ON T.Id=M.TourId
INNER JOIN TpLine L ON L.Id=T.LineId
INNER JOIN TpQuota Q ON Q.Id=M.QuotaId ");

            sql.Append(@" WHERE T.OwnerCode=@0", vModel.OwnerCode);
            if (vModel.ExchangeFromTourId > 0)
            {
                var line = new TpLineBiz().GetLineByTour(vModel.ExchangeFromTourId);
                sql.Append(@" AND L.TrafficType=@0", line.TrafficType);
            }

            if (!vModel.TourNo.IsNullOrEmpty())
                sql.Append(@" AND T.TourNo = @0", vModel.TourNo);      //团号全匹配
            if (!vModel.CrmTeamId.IsNullOrEmpty())
                sql.Append(@" AND L.TeamID = @0", Ansi(vModel.CrmTeamId));
            if (!vModel.LineName.IsNullOrEmpty())
                sql.Append(@" AND L.LineName LIKE @0", AnsiLike(vModel.LineName));
            if (!vModel.MinOutDate.IsNullOrEmpty())
                sql.Append(@" AND T.OutDate >= @0", vModel.MinOutDate.ToDateTime());
            if (!vModel.MaxOutDate.IsNullOrEmpty())
                sql.Append(@" AND T.OutDate <= @0", vModel.MaxOutDate.ToDateTime());
            if (!vModel.TourOk.IsNullOrEmpty())
            {
                var b = vModel.TourOk.ToInt();
                if(b > 0)
                    sql.Append(@" AND T.AuditState>0 ");
                else
                    sql.Append(@" AND T.AuditState=0");
            }
               
            if (vModel.ExchangeFromTourId > 0)
            {
                sql.Append(@" AND M.TourId != @0", vModel.ExchangeFromTourId);
            }

            sql.Append(" LIMIT 20 ");
            return _dao.Query<TourQuotaMapModel, TpTourPlanModel, QuotaModel>(sql.SQL, sql.Arguments).ToList();
        }

        #endregion 换团
    }
}