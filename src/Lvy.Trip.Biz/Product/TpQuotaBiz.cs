using Lvy.Models;
using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lvy.Trip.Biz.Product
{
    public class TpQuotaBiz : BaseBiz
    {
        private readonly QuotaDao _dao = new QuotaDao();

        #region Basic

        /// <summary>
        /// 获取库存对象
        /// </summary>
        /// <param name="quotaId">库存Id</param>
        /// <returns></returns>
        public QuotaModel GetQuota(int quotaId)
        {
            return _dao.GetById(quotaId);
        }

        /// <summary>
        /// 新增库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Insert(QuotaModel model)
        {
            return Convert.ToInt32(_dao.Insert(model));
        }

        /// <summary>
        /// 更新库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(QuotaModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        /// 删除库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Delete(QuotaModel model)
        {
            return _dao.Delete(model);
        }

        #endregion Basic

        #region 共享库存管理

        /// <summary>
        /// 获取共享库存分页列表
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<QuotaModel> GetPagedShareQuota(SearchShareQuotaVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT Id,QuotaName,PlanQuota,UseQuota,UsedQuota,HoldQuota,OutDate,ShareDesc,Source,ModifiedBy,ModifiedTime,OwnerCode FROM TpQuota WHERE Source=2 AND OwnerCode=@0", vModel.OwnerCode);
            if (!vModel.ShareId.IsNullOrEmpty())
                sql.Append(@" AND Id=@0", Convert.ToInt16(vModel.ShareId));
            if (!vModel.ShareName.IsNullOrEmpty())
                sql.Append(@" AND QuotaName LIKE @0", AnsiLike(vModel.ShareName));
            sql.Append(@" ORDER BY ModifiedTime DESC,OutDate");
            return _dao.Pager(vModel.PagedQuota.PageIndex, vModel.PagedQuota.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 新增共享库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void AddShareQuota(ShareQuotaVModel model)
        {
            using (var tran = _dao.GetTransaction())
            {
                model.Quota.Source = 2;//共享团
                model.Quota.ModifiedTime = DateTime.Now;
                model.Quota.UsedQuota = 0;
                model.Quota.UseQuota = model.Quota.PlanQuota - model.Quota.HoldQuota;
                int quotaId = Convert.ToInt32(_dao.Insert(model.Quota));
                if (model.BusSeat != null)
                {
                    model.BusSeat.QuotaId = quotaId;
                    _dao.Execute(@"INSERT INTO TpBusSeat (TourId,SeatNum,SeatDetail,QuotaId) Values (@0,@1,@2,@3)",
                                 model.BusSeat.TourId, model.BusSeat.SeatNum, model.BusSeat.SeatDetail,
                                 model.BusSeat.QuotaId);
                }
                tran.Complete();
            }
        }

        /// <summary>
        /// 保存共享库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void EditShareQuota(ShareQuotaVModel vModel)
        {
            QuotaModel quota = GetQuota(vModel.Quota.Id);
            quota.QuotaName = vModel.Quota.QuotaName;
            quota.PlanQuota = vModel.Quota.PlanQuota;
            quota.HoldQuota = vModel.Quota.HoldQuota;
            quota.UseQuota = quota.PlanQuota - quota.HoldQuota - quota.UsedQuota;
            quota.OutDate = vModel.Quota.OutDate;
            quota.ShareDesc = vModel.Quota.ShareDesc;
            quota.ModifiedTime = DateTime.Now;

            Sql sql = new Sql();
            if (quota.TrafficType == 1 && vModel.Quota.TrafficType != 1)
            {
                //删除座位
                sql.Append(@"DELETE FROM TpBusSeat WHERE TourId=0 AND QuotaId=@0", quota.Id);
            }
            else if (quota.TrafficType != 1 && vModel.Quota.TrafficType == 1)
            {
                //添加座位
                if (vModel.BusSeat != null)
                {
                    vModel.BusSeat.QuotaId = quota.Id;
                    sql.Append(@"INSERT INTO TpBusSeat (TourId,SeatNum,SeatDetail,QuotaId) Values (@0,@1,@2,@3)", vModel.BusSeat.TourId, vModel.BusSeat.SeatNum, vModel.BusSeat.SeatDetail, vModel.BusSeat.QuotaId);
                }
            }
            else if (quota.TrafficType == 1 && vModel.Quota.TrafficType == 1)
            {
                //更新座位
                if (vModel.BusSeat != null)
                {
                    sql.Append(@"UPDATE TpBusSeat SET SeatNum=@0,SeatDetail=@1 WHERE TourId=0 AND QuotaId=@2", vModel.BusSeat.SeatNum, vModel.BusSeat.SeatDetail, quota.Id);
                }
            }
            else
            {
                sql = null;
            }

            using (var tran = _dao.GetTransaction())
            {
                quota.TrafficType = vModel.Quota.TrafficType;
                _dao.Update(quota);

                if (null != sql)
                    _dao.Execute(sql.SQL, sql.Arguments);

                tran.Complete();
            }
        }

        /// <summary>
        /// 获取共享库存团计划
        /// </summary>
        /// <param name="shareQuotaId"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetTourWithShare(int shareQuotaId)
        {
            TpTourPlanDao tourDao = new TpTourPlanDao();
            return tourDao.Fetch(@"SELECT TpTourPlan.* FROM TpTourPlan INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=TpTourPlan.Id WHERE TpTourQuotaMap.QuotaId=@0", shareQuotaId);
        }

        #endregion 共享库存管理

        /// <summary>
        /// 获取共享库存列表
        /// </summary>
        /// <param name="outDate">出发日期</param>
        /// <returns></returns>
        public List<QuotaModel> GetShareQuotasByDay(DateTime outDate, int trafficType, string ownerCode)
        {
            return _dao.Fetch(@"SELECT * FROM TpQuota WHERE Source=2 AND TpQuota.OutDate=@0 AND TrafficType=@1 AND TpQuota.OwnerCode=@2 ORDER BY Id", outDate, trafficType, ownerCode);
        }

        /// <summary>
        /// 根据团计划Id与来源获取对应的库存
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <param name="source">来源(1:非共享，2:共享)</param>
        /// <returns></returns>
        public QuotaModel GetQuotaByTourSource(int tourId, int source)
        {
            return _dao.SingleOrDefault(@"SELECT TpQuota.* FROM TpTourPlan INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=TpTourPlan.Id INNER JOIN TpQuota ON TpTourQuotaMap.QuotaId=TpQuota.Id WHERE TpTourPlan.Id=@0 AND TpTourQuotaMap.Source=@1", tourId, source);
        }

        /// <summary>
        /// 根据团计划Id获取对应的库存
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public QuotaModel GetQuotaByTour(int tourId)
        {
            return _dao.SingleOrDefault(@"SELECT TpQuota.* FROM TpQuota INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.QuotaId=TpQuota.Id WHERE TpTourQuotaMap.TourId=@0", tourId);
        }

        /// <summary>
        /// 获取共享库存字典
        /// </summary>
        /// <param name="outDate"></param>
        /// <param name="trafficType"> </param>
        /// <returns></returns>
        public List<KeyValueBean> GetShareQuotaDic(DateTime outDate, int trafficType, string ownerCode)
        {
            var quotaBiz = new TpQuotaBiz();
            var shareQuotas = quotaBiz.GetShareQuotasByDay(outDate, trafficType, ownerCode);
            return shareQuotas.Select(item => new KeyValueBean { Key = item.Id.ToString(CultureInfo.InvariantCulture), Value = item.QuotaName.ToString(CultureInfo.InvariantCulture) }).ToList();
        }
    }
}