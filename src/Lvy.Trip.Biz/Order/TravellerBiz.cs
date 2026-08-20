using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Dao.Order;
using Lvy.VModels.Order;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Order
{
    /// <summary>
    /// 游客
    /// </summary>
    public class TravellerBiz : BaseBiz
    {
        private readonly TpTravellerDao _travellerDao = new TpTravellerDao();

        private readonly GuideBiz _guideBiz = new GuideBiz();
        private readonly OrderBiz _orderBiz = new OrderBiz();
        private readonly TpOrderFileDao _fileDao = new TpOrderFileDao();

        /// <summary>
        /// 获取一个游客的对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TpTravellerModel GetById(int Id)
        {
            return _travellerDao.GetById(Id);
        }

        /// <summary>
        /// 获取游客资料信息
        /// </summary>
        /// <returns></returns>
        public List<TpOrderFileModel> GetTouristsFileList(int keyId)
        {
            var sql = new Sql();
            sql.Append("select * from TpOrderFiles where KeyId=@0 and IsDel=0", keyId);
            return _fileDao.Query(sql.SQL, sql.Arguments).ToList<TpOrderFileModel>();
        }

        /// <summary>
        /// 根据订单编号获取 所有游客对象
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TpTravellerModel> GetByOrderCode(string orderCode)
        {
            return _travellerDao.Fetch(@"SELECT * FROM TpTraveller WHERE OrderCode=@0 ", orderCode);
        }

        /// <summary>
        /// 根据团号获取有效游客列表
        /// </summary>
        /// <param name="tourId">团号</param>
        /// <returns></returns>
        public List<TpTravellerModel> GetByTourId(int tourId, bool containLock = true, bool containCancel = false)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT tt.* FROM tptraveller tt
INNER JOIN TpOrder ON TpOrder.OrderCode=tt.OrderCode ");
            if (!containCancel)
                sql.Append(" AND TpOrder.IsCancel=0");
            if (!containLock)
                sql.Append(" AND TpOrder.OrderState=2 ");
            sql.Append(@" WHERE tt.TourId=@0 AND tt.State=2 
UNION ALL
SELECT tt.* FROM tptraveller tt
WHERE tt.TourId=@0 AND tt.State=2 AND tt.IsLeader=1", tourId);

            return _travellerDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// @根据编号获取上一个游客信息
        /// </summary>
        /// <param name="id">游客ID</param>
        /// <param name="tourId">团号</param>
        /// <returns></returns>
        public TpTravellerModel GetPreByTourId(int id, int tourId)
        {
            // 缺少领队
            return _travellerDao.FirstOrDefault(@"SELECT TpTraveller.* FROM TpTraveller
INNER JOIN TpOrder ON TpOrder.OrderCode=TpTraveller.OrderCode AND TpOrder.IsCancel=0
WHERE TpTraveller.ID<@0 AND TpTraveller.State=2 AND TpTraveller.TourId=@1 ORDER BY TpTraveller.Id DESC ", id, tourId);
        }

        /// <summary>
        /// 根据编号获取下一个游客信息
        /// </summary>
        /// <param name="id">游客ID</param>
        /// <param name="tourId">团号</param>
        /// <returns></returns>
        public TpTravellerModel GetNextByTourId(int id, int tourId)
        {
            // 缺少领队
            return _travellerDao.FirstOrDefault(@"SELECT TpTraveller.* FROM TpTraveller
INNER JOIN TpOrder ON TpOrder.OrderCode=TpTraveller.OrderCode AND TpOrder.IsCancel=0
WHERE TpTraveller.ID>@0 AND TpTraveller.State=2 AND TpTraveller.TourId=@1 ORDER BY TpTraveller.Id ", id, tourId);
        }

        /// <summary>
        /// @根据编号获取上一个游客信息
        /// </summary>
        /// <param name="id">游客ID</param>
        /// <param name="orderCode">团号</param>
        /// <returns></returns>
        public TpTravellerModel GetPreByOrderCode(int id, string orderCode)
        {
            return _travellerDao.FirstOrDefault(@"SELECT TpTraveller.* FROM TpTraveller
inner join TpOrder ON TpOrder.OrderCode=TpTraveller.OrderCode
WHERE TpTraveller.ID<@0 AND TpTraveller.State=2 AND TpOrder.OrderCode=@1 ORDER BY TpTraveller.Id DESC ", id, orderCode);
        }

        /// <summary>
        /// 根据编号获取下一个游客信息
        /// </summary>
        /// <param name="id">游客ID</param>
        /// <param name="orderCode">团号</param>
        /// <returns></returns>
        public TpTravellerModel GetNextByOrderCode(int id, string orderCode)
        {
            return _travellerDao.FirstOrDefault(@"SELECT TpTraveller.* FROM TpTraveller
inner join TpOrder ON TpOrder.OrderCode=TpTraveller.OrderCode
WHERE TpTraveller.ID>@0 AND TpTraveller.State=2 AND TpOrder.OrderCode=@1 ORDER BY TpTraveller.Id ", id, orderCode);
        }

        /// <summary>
        /// 更新游客信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(TpTravellerModel model)
        {
            return _travellerDao.Update(model);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<PrintTravellerVModel> GetByTourId(string tourIds)
        {
            Sql sql = new Sql();
            sql.Append("SELECT 1 as IsChecked , * FROM TpTraveller WHERE TourId in (" + tourIds + ") ORDER BY Cast(SeatNum as int) ASC ");

            return _travellerDao.Query<PrintTravellerVModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="guideId"></param>
        /// <returns></returns>
        public object AddTravellerByGuideId(int tourId, int guideId, CrmAccountModel userInfo)
        {
            //先删除原来的领队记录。
            Sql sql = new Sql();
            sql.Append(" delete from TpTraveller where TourId=@0 and IsLeader=1 ", tourId);
            _travellerDao.Execute(sql.SQL, sql.Arguments);

            //重新计算团人数。
            var guideModel = _guideBiz.GetById(guideId);
            TpTravellerModel tpModel = new TpTravellerModel();
            tpModel.TourId = tourId;
            tpModel.Name = guideModel.Name;
            tpModel.Sex = guideModel.Sex.ToString();
            tpModel.PinYin = guideModel.PinYin;
            tpModel.DateOfBirth = guideModel.BirthDate;
            tpModel.PlaceOfBirth = guideModel.BirthPlace;
            tpModel.Phone = guideModel.Mobile;
            tpModel.IsLeader = true;
            tpModel.LeaderNo = guideModel.Id;
            tpModel.PassType = 2;
            tpModel.PassNo = guideModel.Hzno;//护照号码
            tpModel.PlaceOfIssue = guideModel.HzAddress; //签发地
            tpModel.CreatedTime = DateTime.Now;
            tpModel.ModifiedTime = DateTime.Now;
            tpModel.ModifiedBy = userInfo.Code;
            tpModel.CreatedBy = userInfo.Code;
            tpModel.State = 2;

            object i = _travellerDao.Insert(tpModel);

            // 更新库存
            _orderBiz.FreeQuota(tourId, "", userInfo.Code);

            return i;
        }
    }
}