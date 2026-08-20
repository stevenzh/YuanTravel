using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Excel;
using Lvy.VModels.Finance;
using Lvy.VModels.Op;
using Lvy.VModels.Product;
using Lvy.VModels.Tour;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Text.Json;
using TourInfoVModel = Lvy.VModels.Product.TourInfoVModel;

namespace Lvy.Trip.Biz.Order
{
    /// <summary>
    /// 开班计划
    /// </summary>
    public class TpTourPlanBiz : BaseBiz
    {
        private readonly TpTourPlanDao _planDao = new TpTourPlanDao();
        private readonly TpOrderDao _ordersDao = new TpOrderDao();
        private readonly TpTourFileDao _tourFileDao = new TpTourFileDao();//团核算成本附件

        #region 团单管理

        /// <summary>
        /// 获取团订单列表信息
        /// </summary>
        /// <param name="searchTourVModel"></param>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用者：团队游 团单管理
        /// </remarks>
        public PagedList<TourInfoVModel> GetTourList(TourVModel searchTourVModel, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@" select tp.Id AS TourId, tp.TourNo, tp.OutDate,tp.BookingLastDays, tp.TuiJianType, tp.Source AS TourSource,
tp.AuditState, tp.TravellerCount,
quoTa.PlanQuota, quoTa.HoldQuota, quoTa.UsedQuota, quoTa.UseQuota,quoTa.UnLockQuota,
line.LineName, line.CustomerCode
from TpTourPlan tp
inner join TpTourQuotaMap map on tp.Id=map.TourId
inner join TpQuota quoTa on map.QuotaId=quoTa.Id
inner join TpLine line on tp.LineId=line.LineId ");

            #region 组织查询条件

            sql.Append(@" WHERE tp.TourState !=0 AND tp.OwnerCode=@0 ", Ansi(userInfo.OwnerCode));

            //if (userInfo.CustomerCode != userInfo.OwnerCode)
            //    sql.Append(@" AND line.CustomerCode=@0 ", Ansi(userInfo.CustomerCode));
            //var customer = DictionaryTools.GetCachedCustomer(userInfo.CustomerCode);
            //if (customer.IsSupplier)
            //{
            //    //若为供应商，仅能看到自己的产品订单
            //    sql.Append(@" and line.CustomerCode=@0 ", Ansi(userInfo.CustomerCode));
            //}
            //else if (customer.IsDistributors)
            //{
            //    //分销商理论上无法查看团单管理，若分配了该权限，则不显示数据
            //    return new PagedList<TourInfoVModel> { Items = new List<TourInfoVModel>() };
            //}

            if (!searchTourVModel.Condition.TourNo.IsNullOrEmpty())
                sql.Append(@" AND tp.Id=@0 ", searchTourVModel.Condition.TourNo.ToInt());
            if (!searchTourVModel.Condition.LineName.IsNullOrEmpty())
                sql.Append(@" AND line.LineName LIKE @0 ", AnsiLike(searchTourVModel.Condition.LineName));
            if (!searchTourVModel.Condition.IsTourOk.IsNullOrEmpty())
            {
                var b = searchTourVModel.Condition.IsTourOk.ToInt();
                if (b > 0)
                    sql.Append(@" AND tp.AuditState>0 ");
                else
                    sql.Append(@" AND tp.AuditState=0 ");
            }


            if (!searchTourVModel.Condition.OutDateRange.IsNullOrEmpty())
            {
                var t = searchTourVModel.Condition.OutDateRange.Split('-');
                sql.Append(@" AND tp.OutDate >= @0 AND tp.OutDate <= @1 ", t[0].ToDateTime(), t[1].ToDateTime());
            }

            if (!searchTourVModel.Condition.RecommendType.IsNullOrEmpty())
                sql.Append(@" AND tp.TuiJianType=@0 ", searchTourVModel.Condition.RecommendType);
            if (searchTourVModel.Condition.TourType > 0)
                sql.Append(@" AND tp.TourType=@0 ", searchTourVModel.Condition.TourType);
            if (!searchTourVModel.Condition.TourAuditState.IsNullOrEmpty())
                sql.Append(@" AND tp.AuditState=@0 ", searchTourVModel.Condition.TourAuditState.ToInt());

            //分组条件查询
            if (!searchTourVModel.Condition.CrmTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and line.TeamID=@0  ", searchTourVModel.Condition.CrmTeamId);
            }

            #endregion 组织查询条件

            sql.Append(@" ORDER BY tp.OutDate DESC ");

            var result = _planDao.Pager<TourInfoVModel>(searchTourVModel.TourList.PageIndex, searchTourVModel.TourList.PageSize, sql.SQL, sql.Arguments);
            return result;
        }

        /// <summary>
        /// 近期开班收客情况 10条
        /// </summary>
        /// <param name="userCode">用户ID</param>
        /// <returns></returns>
        public List<TourInfoVModel> RecentPlan(string userCode)
        {
            var sql = new Sql();
            sql.Append(@"select tp.Id AS TourId, tp.TourNo, tp.OutDate, quoTa.PlanQuota, quoTa.HoldQuota, quoTa.UsedQuota, quoTa.UseQuota,quoTa.UnLockQuota,
tp.BookingLastDays, tp.TuiJianType, tp.Source AS TourSource, tp.AuditState, tl.LineName, tl.CustomerCode
from TpTourPlan tp
inner join TpTourQuotaMap map on tp.Id=map.TourId
inner join TpQuota quoTa on map.QuotaId=quoTa.Id
inner join TpLine tl on tp.LineId=tl.LineId
inner join TpLineAdmin tla on tl.LineId= tla.LineId
where tp.OutDate>@1 and tla.AccountCode=@0 LIMIT 10 ", new AnsiString(userCode), DateTime.Today);

            return _planDao.Query<TourInfoVModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 根据团号获取团订单对象
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        public TourAccountInfoVModel GetTourAccountListByTourId(int tourId, string ownerCode)
        {
            if (ownerCode.IsNullOrEmpty())
                throw new Exception("OwnerCode不能为空！");
            var sql = new Sql();

            sql.Append(@" SELECT e.ModifiedBy, e.OwnerCode, e.TourState, e.LineId, e.TourId, f.LineName, e.OutDate, e.PlanQuota, e.UsedQuota, e.UseQuota, e.TuiJianType, e.TourSource, e.AuditState, f.LineType FROM  
                ( 
               SELECT c.ModifiedBy, c.OwnerCode, c.TourState, c.LineId, c.Id AS TourId, c.OutDate, d.PlanQuota, d.UsedQuota, d.UseQuota, c.TuiJianType, c.AuditState, c.`Source` AS TourSource FROM  
                (
                 SELECT a.*,b.QuotaId from TpTourPlan a left join TpTourQuotaMap b on a.Id=b.TourId 
                ) c left join TpQuota d on c.QuotaId=d.Id 
                ) e left join TpLine f 
             on e.LineId=f.LineId  ");

            #region 组织查询条件

            sql.Append(@" WHERE e.TourState !=0 AND e.OwnerCode=@0 ", Ansi(ownerCode));
            sql.Append(@" AND e.TourId=@0 ", tourId);

            #endregion 组织查询条件

            sql.Append(@" order by e.OutDate desc ");
            var result = _planDao.Query<TourAccountInfoVModel>(sql.SQL, sql.Arguments).ToList()[0];
            return result;
        }

        /// <summary>
        /// 根据团Id获取团对象
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public TpTourPlanModel GetTourById(int tourId)
        {
            var sql = new Sql();
            sql.Append(" SELECT t.* ");
            sql.Append(" ,(SELECT SUM(TolYsPrice) FROM TpOrder o WHERE o.tourId=t.id AND (o.OrderState=2 OR o.IsCancel=2)) AS TolYsPrice ");
            sql.Append(" ,(SELECT SUM(TolPaid) FROM TpOrder o WHERE o.tourId=t.id AND (o.OrderState=2 OR o.IsCancel=2)) AS TolPaid ");
            sql.Append(" FROM TpTourPlan t WHERE t.Id=@0  ", tourId);

            return _planDao.Query(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        public TpTourPlanModel GetTourByIds(int tourId)
        {
            return _planDao.GetById(tourId);
        }

        /// <summary>
        /// 根据ID子查询
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public TourPlanExcelVModel GetPlanById(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT a.TourNo, a.OutDate, a.Remarks, a.EntryDate, t.LineId, t.LineName, 
 '' AS Totakecommuntiy, '' AS DepartureTime, '' AS DepartBan, '' AS Returnregular, '' AS Contents, '' AS EntryTime,
 bdd.Value AS TrafficType, bd.Name AS PortOfEntry, bd2.Name AS PortOfExit,
 (SELECT Name FROM BaseGuides WHERE Id=(select LeaderNo FROM TpTraveller c WHERE c.IsLeader = 1 AND c.tourid=a.id)) AS Name,
 (SELECT LeadCard FROM BaseGuides WHERE Id=(select LeaderNo FROM TpTraveller c WHERE c.IsLeader = 1 AND c.tourid=a.id)) AS TourCard
FROM TpTourPlan a INNER JOIN TpLine t ON a.LineId = t.LineId
LEFT JOIN basedictionarydetail bdd ON bdd.Name = 'TrafficTypeEnum' AND bdd.IsValid = 1 AND bdd.`key` = t.TrafficType
LEFT JOIN basedestination bd ON bd.Id = a.PortOfEntry
LEFT JOIN basedestination bd2 ON bd2.Id = a.PortOfExit
 WHERE a.Id=@0", tourId);

            return _planDao.Query<TourPlanExcelVModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        /// <summary>
        /// 根据LineId获取团计划
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetTourByLineId(int lineId)
        {
            return _planDao.Fetch(@"SELECT * FROM TpTourPlan WHERE LineId = @0", lineId);
        }

        /// <summary>
        /// 更新团计划信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateTourPlan(TpTourPlanModel model)
        {
            return _planDao.Update(model);
        }

        #endregion 团单管理

        #region 单团核算

        /// <summary>
        /// 获取单团核算团单信息列表 
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用者 单团核算 /Finance/SearchTourAccount
        /// </remarks>
        public PagedList<TourAccountInfoVModel> GetTourAccountList(TourAccountVModel vModel, string ownerCode)
        {
            var sql = new Sql();
            //var userInfo = GlobalContext.Current.UserInfo;
            sql.Append(@" SELECT tp.TourNo, tp.ModifiedBy, tp.IsTourOk, tp.OwnerCode, tp.TourState, tp.LineId,
tp.Id AS TourId, tp.OutDate, tp.TourType, tp.AuditState, quoTa.PlanQuota, quoTa.UsedQuota,
quoTa.UseQuota, tp.TuiJianType, tp.Source AS TourSource, line.LineName, line.LineType, line.CustomerCode
FROM TpTourPlan tp
INNER JOIN TpTourQuotaMap map ON tp.Id=map.TourId
INNER JOIN TpQuota quoTa ON map.QuotaId=quoTa.Id
INNER JOIN TpLine line ON tp.LineId=line.LineId ");

            #region 组织查询条件

            sql.Append(@" WHERE tp.TourState!=0 AND tp.AuditState>0 AND tp.OwnerCode=@0 ", Ansi(ownerCode));

            if (!vModel.Condition.TourNo.IsNullOrEmpty())
                sql.Append(" AND tp.Id=@0", vModel.Condition.TourNo.ToInt());
            if (!vModel.Condition.LineName.IsNullOrEmpty())
                sql.Append(@" AND line.LineName LIKE @0 ", AnsiLike(vModel.Condition.LineName));
            if (!vModel.Condition.StartOutDate.IsNullOrEmpty())
                sql.Append(@" AND tp.OutDate >= @0 ", vModel.Condition.StartOutDate.ToDateTime());
            if (!vModel.Condition.EndOutDate.IsNullOrEmpty())
                sql.Append(@" AND tp.OutDate <= @0 ", vModel.Condition.EndOutDate.ToDateTime());
            if (!vModel.Condition.LineType.IsNullOrEmpty())
                sql.Append(@" AND line.LineType=@0 ", vModel.Condition.LineType);
            if (vModel.Condition.TourType > 0)
                sql.Append(@" AND tp.TourType=@0 ", vModel.Condition.TourType);
            if (!vModel.Condition.TourAuditState.IsNullOrEmpty())
                sql.Append(@" AND tp.AuditState=@0 ", vModel.Condition.TourAuditState.ToInt());

            #endregion 组织查询条件

            sql.Append(@" order by tp.OutDate desc ");
            var result = _planDao.Pager<TourAccountInfoVModel>(vModel.TourAccountList.PageIndex, vModel.TourAccountList.PageSize, sql.SQL, sql.Arguments);
            return result;
        }

        /// <summary>
        /// 根据团号==》TpOrder 获取结算明细
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        public FinanceTotalModel GetAccountDetailByTourId(int tourId, string ownerCode)
        {
            if (ownerCode.IsNullOrEmpty())
                throw new Exception("OwnerCode不能为空！");
            var sql = new Sql();
            sql.Append(@" SELECT IFNULL(SUM(TolYsPrice),0) AS SumTolYsPrice,IFNULL(SUM(TolPaid),0) AS SumTolPaid,IFNULL(SUM(TravellerCount),0) AS SumTravellerCount FROM TpOrder ")
                .Append(@" WHERE OwnerCode=@0 ", Ansi(ownerCode))
                .Append(@" AND TourId=@0 ", tourId);
            return _ordersDao.Query<FinanceTotalModel>(sql.SQL, sql.Arguments).ToList()[0];
        }


        #endregion 单团核算

        /// <summary>
        /// 更新游客
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public OrderResultState UpdataTourists(EditTouristsVModel vModel)
        {
            OrderResultState ERROR_STATE;
            var tourists = new TravellerBiz().GetByTourId(vModel.Tour.Id);
            var updateTourists = new List<TpTravellerModel>();
            var oldDic = new Dictionary<long, string>();
            foreach (var tourist in vModel.Tourists)
            {
                if (tourist.State != 2) continue;
                if (tourist.IsLeader)
                {
                    continue;
                }
                var updateTourist = tourists.FirstOrDefault(p => p.Id == tourist.Id);
                if (updateTourist == null) continue;
                oldDic.Add(tourist.Id, updateTourist.SeatNum);    //updateTourists添加的是引用，所以需要保存一份原始座位键值对
                updateTourist.Name = tourist.Name;
                updateTourist.Phone = tourist.Phone;
                updateTourist.SeatNum = tourist.SeatNum;
                updateTourist.PassType = tourist.PassType;
                updateTourist.PassNo = tourist.PassNo;

                updateTourist.Sex = tourist.Sex;
                updateTourist.DateOfBirth = tourist.DateOfBirth;
                updateTourist.PlaceOfBirth = tourist.PlaceOfBirth;
                updateTourist.DateOfIssue = tourist.DateOfIssue;
                updateTourist.DateOfExpiry = tourist.DateOfExpiry;

                updateTourist.PinYin = tourist.PinYin;

                updateTourists.Add(updateTourist);
            }
            //修改TpTourPlan 信息

            TpTourPlanModel TourPlanModel = GetTourById(vModel.Tour.Id);
            TourPlanModel.EntryDate = vModel.Tour.EntryDate;
            TourPlanModel.PortOfEntry = vModel.Tour.PortOfEntry;
            TourPlanModel.PortOfExit = vModel.Tour.PortOfExit;
            TourPlanModel.PerCapitaCost = vModel.Tour.PerCapitaCost;//人均成本

            var traDao = new TpTravellerDao();
            using (var scope = new TransactionScope())
            {
                foreach (var tourist in updateTourists)
                {
                    //if (!tourist.SeatNum.IsNullOrEmpty())
                    //{
                    //    //TODO: 待修改，变更团计划为共享后，已定座位的处理方式
                    //    var old = oldDic.FirstOrDefault(p => p.Key == tourist.Id);
                    //    ERROR_STATE = new OrderBiz().SaveSeatDetails(old.Value, tourist.SeatNum, tourist.TourId);

                    //    if (ERROR_STATE != OrderResultState.Code100)
                    //        return ERROR_STATE;
                    //}
                    traDao.Update(tourist);
                }
                _planDao.Update(TourPlanModel);
                scope.Complete();
            }
            return OrderResultState.Code100;
        }

        #region 重新计算团库存

        /// <summary>
        /// 重新计算团库存
        /// 如果是汽车班，重算座位号
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public void ReCalcTourQuota(int tourId)
        {
            // 如果是汽车板 ，重新计算座位分布
            if (IsBusTour(tourId))
                ReCalcSeatDetails(tourId);
        }

        /// <summary>
        /// 判断是否是汽车班
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        private bool IsBusTour(int tourId)
        {
            string sql = @"SELECT b.TrafficType FROM TpTourPlan a RIGHT JOIN tpLine b
                        ON a.LineId = b.LineId WHERE a.Id=@0";
            int trafficType = _planDao.ExecuteScalar<int>(sql, tourId);
            return trafficType == 1 ? true : false;
        }

        /// <summary>
        ///  重新计算座位分布
        /// </summary>
        /// <param name="tourId"></param>
        private void ReCalcSeatDetails(int tourId)
        {
            string sql = "SELECT SeatNum from TpTraveller where TourId=@0 and State=2";

            var seatNums = new TpTravellerDao().Query<string>(sql, tourId);

            var model = GetSeatDetails(tourId);
            var seats = model.SeatModels;

            foreach (var seat in seats)
            {
                if (seat.State == 3) // 锁定的座位不处理。
                    continue;

                seat.State = 1; //默认释放座位
                foreach (var seatNum in seatNums)
                {
                    if (seat.No.Equals(seatNum))
                    {
                        seat.State = 2; // 已占
                        break;
                    }
                }
            }

            var jsonSeats = JsonSerializer.Serialize(seats);
            new TpBusSeatDao().UpdateSeatDetail(model.QuotaId, jsonSeats);
        }

        /// <summary>
        /// 获取座位分布数据
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        private TpBusSeatModel GetSeatDetails(int tourId)
        {
            string sql = @"SELECT TpBusSeat.* from TpBusSeat inner join TpTourQuotaMap on TpTourQuotaMap.QuotaId=TpBusSeat.QuotaId
where TpTourQuotaMap.TourId=@0";

            return new TpTourPlanDao().Query<TpBusSeatModel>(sql, tourId).FirstOrDefault();
        }

        #endregion 重新计算团库存

        /// <summary>
        /// 获取团和线路信息
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public TpTourPlanModel GetTourAndLine(int tourId)
        {
            return _planDao.Query<TpTourPlanModel, TpLineModel>(
                    @"SELECT A.*,B.* FROM TpTourPlan A INNER JOIN TpLine B ON B.LineId=A.LineId WHERE A.Id = @0", tourId).
                    FirstOrDefault();
        }

        /// <summary>
        /// 查询出游客信息
        /// 取得该团所有游客绑定订单的
        ///   和领队
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<TravellerModels> GetvisitoriId(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT l.LineName, a.Name, PinYin, DateOfBirth, PlaceOfBirth, Phone, PassNo, PlaceOfIssue, DateOfIssue,
   dds.Value AS Sex, ddp.Value AS PassType
FROM TpTraveller a
INNER JOIN tporder o ON o.OrderCode = a.OrderCode AND o.IsCancel=0
LEFT JOIN BaseDictionaryDetail dds ON a.Sex = dds.`Key` and dds.IsValid = 1 and dds.Name='SexEnum'
LEFT JOIN BaseDictionaryDetail ddp ON a.PassType = ddp.`Key` and ddp.IsValid = 1 and ddp.Name='PassTypeEnum'
LEFT JOIN TpTourPlan b on a.TourId = b.Id
LEFT JOIN TpLine l ON b.LineId = l.LineId
 WHERE a.TourId=@0
 UNION ALL
SELECT l.LineName, a.Name, PinYin, DateOfBirth, PlaceOfBirth, Phone, PassNo, PlaceOfIssue, DateOfIssue,
   dds.Value AS Sex, ddp.Value AS PassType
FROM TpTraveller a
LEFT JOIN BaseDictionaryDetail dds ON a.Sex = dds.`Key` and dds.IsValid = 1 and dds.Name='SexEnum'
LEFT JOIN BaseDictionaryDetail ddp ON a.PassType = ddp.`Key` and ddp.IsValid = 1 and ddp.Name='PassTypeEnum'
LEFT JOIN TpTourPlan b on a.TourId = b.Id
LEFT JOIN TpLine l ON b.LineId = l.LineId
 WHERE a.TourId=@0 AND a.IsLeader=1 ", tourId);

            return _planDao.Query<TravellerModels>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 取得团内游客
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<GuestModels> GuestiId(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT a.Id, l.LineName, a.Name, PinYin,bd.Value as Sex,
	DateOfBirth,PlaceOfBirth, cc.Name as Booking, bdd.Value as PassType,
	PassNo,DateOfIssue,DateOfExpiry,'' as LinName, a.Phone,PlaceOfIssue,a.Remark
FROM TpTraveller a
  LEFT JOIN TpTourPlan b on a.TourId=b.Id
  LEFT JOIN TpLine l on b.LineId=l.LineId
  LEFT JOIN TpOrder c on a.OrderCode=c.OrderCode
  LEFT JOIN CrmCustomer cc on cc.Code = c.BookingCustomer
  LEFT JOIN BaseDictionaryDetail bd  on a.Sex = bd.`Key` and bd.Name = 'SexEnum' and bd.IsValid = 1
  LEFT JOIN BaseDictionaryDetail bdd on a.PassType = bdd.`Key` and bdd.Name = 'PassTypeEnum' and bdd.IsValid = 1
WHERE a.TourId=@0 AND c.IsCancel=0
UNION ALL 
SELECT a.Id, l.LineName, a.Name, PinYin,bd.Value as Sex,
	DateOfBirth,PlaceOfBirth, '' as Booking, bdd.Value as PassType,
	PassNo,DateOfIssue,DateOfExpiry, 'T/L' as LinName, a.Phone,PlaceOfIssue,a.Remark
FROM TpTraveller a
  LEFT JOIN TpTourPlan b on a.TourId=b.Id
  LEFT JOIN TpLine l on b.LineId=l.LineId
  LEFT JOIN BaseDictionaryDetail bd  on a.Sex = bd.`Key` and bd.Name = 'SexEnum' and bd.IsValid = 1
  LEFT JOIN BaseDictionaryDetail bdd on a.PassType = bdd.`Key` and bdd.Name = 'PassTypeEnum' and bdd.IsValid = 1
WHERE a.TourId=@0 and a.IsLeader=1 ", tourId);
            return _planDao.Query<GuestModels>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 查询出团队基本行程信息
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<LineRouteExcelVModel> GetTourRouteInfoTourId(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@"select l.LineName, Title, Contents, convert(b.`days`,CHAR) as days,''as City ,'' as Country ,
'' as IsGuoJin ,'' as zhandian
from TpTourPlan a
inner join TpLine l ON a.LineId=l.LineID
inner join TpLineRoute b on a.LineId=b.LineId
where a.Id=@0", tourId);
            return _planDao.Query<LineRouteExcelVModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 查询出线路行程
        /// </summary>
        /// <param name="LineId"></param>
        /// <returns></returns>
        public List<TpLineRouteModel> Getpath(string LineId)
        {
            Sql sql = new Sql();
            sql.Append(" select Title, Days, Catering, Breakfast, Lunch, Supper, Contents from TpLineRoute where LineId=@0 ", LineId);
            return _planDao.Query<TpLineRouteModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 查询游客信息
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<TouristModels> GTourisId(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT a.Name, PinYin, bd.Value as Sex, DateOfBirth,PlaceOfBirth,Phone, bdd.Value as PassType,
PassNo,PlaceOfIssue,DateOfIssue
FROM TpTraveller a
  left join TpTourPlan b on a.TourId=b.Id
  left join BaseDictionaryDetail bd  on a.Sex = bd.`Key` and bd.Name = 'SexEnum' and bd.IsValid = 1
  left join BaseDictionaryDetail bdd on a.PassType = bdd.`Key` and bdd.Name = 'PassTypeEnum' and bdd.IsValid = 1
 WHERE TourId=@0 ", tourId);

            return _planDao.Query<TouristModels>(sql.SQL, sql.Arguments).ToList();
        }

        #region 核算凭证上传方法

        /// <summary>
        /// 按条件查询团核算成本附件
        /// </summary>
        /// <param name="tourID"></param>
        /// <returns></returns>
        public List<TpTourFileModel> GetTourFile(int tourID)
        {
            Sql sql = new Sql();
            sql.Append(" SELECT * FROM TpTourFiles WHERE TourID=@0 AND IsDel=0 ", tourID);

            return _tourFileDao.Query(sql.SQL, sql.Arguments).ToList();
        }
        public List<TpTourFileModel> GetTourFileByTourId(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(" SELECT * FROM TpTourFiles WHERE TourID=@0 AND IsDel=0 ", tourId);

            return _tourFileDao.Query(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 出团通知
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public int UpdateTourNoticeVersion(int tourId, string sourceType)
        {
            Sql sql = new Sql();
            sql.Append(" select Revision from TpTourFiles where TourId=@0 and SourceType=@1 ORDER BY Revision DESC ", tourId, sourceType);
            var c = _tourFileDao.FirstOrDefault(sql.SQL, sql.Arguments);
            if (c == null)
                return 0;
            else
            {
                // 设置为删除
                _tourFileDao.Update(" set IsDel=1 where TourId=@0 and SourceType=@1 ", tourId, sourceType);
                return c.Revision;
            }
        }

        /// <summary>
        /// 新增团核算成本附件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public object AddTpTourFile(TpTourFileModel model)
        {
            return _tourFileDao.Insert(model);
        }

        /// <summary>
        /// 获取ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TpTourFileModel GetTourFileById(int Id)
        {
            return _tourFileDao.GetById(Id);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object DeleteTourFile(int id)
        {
            var sql = new Sql();
            sql.Append("  update TpTourFiles set IsDel=1 where Id=@0 ", id);

            return _tourFileDao.Execute(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TpTourFileModel GetTourFileModel(int Id)
        {
            return _tourFileDao.GetById(Id);
        }

        #endregion 核算凭证上传方法

        /// <summary>
        /// 取得未满开班列表 最好按部门分组（微信OP使用）
        /// </summary>
        /// <param name="topCount"></param>
        /// <returns></returns>
        public List<TourInfoVModel> GetOpTours(string[] teamIds, int topCount, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append("SELECT tp.LineId, tl.LineName, tl.LineType, tp.OutDate, tp.Price, tm.TourId, tm.QuotaId,");
            sql.Append(@"tq.PlanQuota, tq.UseQuota, tq.UsedQuota,tq.UnlockQuota, tq.HoldQuota, tp.SingleRoom, tpp.TeJiaFanLi, dds.Name as ProductBrand  ");
            sql.Append(@" FROM TpLine tl INNER JOIN TpTourPlan tp ON tp.lineId=tl.LineId ");
            sql.Append(@" INNER JOIN TpTourQuotaMap tm ON tm.TourId=tp.Id ");
            sql.Append(@" INNER JOIN TpQuota tq ON tq.Id=tm.QuotaId ");
            sql.Append(@" INNER JOIN TpPrice tpp ON tpp.TourId=tp.Id AND tpp.IsStandard=1 ");
            sql.Append(@" left join BaseBrands dds on tl.BrandCode=dds.Code and dds.IsValid=1 ");
            sql.Append(@" WHERE tl.OwnerCode = @0", userInfo.OwnerCode);
            sql.Append(@" AND tq.UseQuota>0 AND tp.BookingLastDays>=@0", DateTime.Today);
            sql.Append(@" AND tl.LineState=@0", 3);
            sql.Append(@" AND tl.IsValid=1 ");
            sql.Append(@" AND tp.TourState=@0", 3);

            //分组条件查询
            if (teamIds.Length > 0)
            {
                sql.Append(@" and tl.TeamID in(@0) ", teamIds);
            }
            sql.Append(@" order by tp.OutDate LIMIT " + topCount);

            List<TourInfoVModel> tourList = _planDao.Query<TourInfoVModel>(sql.SQL, sql.Arguments).ToList();

            return tourList;
        }

        public PagedList<TourInfoVModel> GetPageTours(SearchTourVModel qmodel)
        {
            var sql = new Sql();

            sql.Append("SELECT tp.LineId, tl.LineName, tl.LineType, tp.OutDate, tp.Price, tm.TourId, tm.QuotaId,");
            sql.Append(@"tq.PlanQuota, tq.UseQuota, tq.UsedQuota, tq.UnlockQuota, tq.HoldQuota, tp.SingleRoom, tpp.TeJiaFanLi, dds.Name as ProductBrand  ");
            sql.Append(@" FROM TpLine tl INNER JOIN TpTourPlan tp ON tp.lineId=tl.LineId");
            sql.Append(@" INNER JOIN TpTourQuotaMap tm ON tm.TourId=tp.LineId ");
            sql.Append(@" INNER JOIN TpQuota tq ON tq.Id=tm.QuotaId");
            sql.Append(@" INNER JOIN TpPrice tpp ON tpp.TourId = tp.Id AND tpp.IsStandard=1");
            sql.Append(@" left join BaseBrands dds on tl.BrandCode = dds.Code and dds.IsValid=1 ");
            sql.Append(@" WHERE tl.OwnerCode = @0", qmodel.OwnerCode);
            sql.Append(@" AND tl.LineState = @0", 3);
            sql.Append(@" AND tl.IsValid=1");

            //分组条件查询
            if (!qmodel.Condition.CrmTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and tl.TeamID= @0 ", qmodel.Condition.CrmTeamId);
            }
            if (qmodel.PlanStatus == "valid")
            {
                sql.Append(@" AND tp.TourState = @0", 3);
            }
            else if (qmodel.PlanStatus == "booking")
            {
                sql.Append(@" AND tp.TourState = @0 and tp.OutDate>=@1 ", 3, DateTime.Today);
            }
            sql.Append(@" order by tp.OutDate DESC ");

            PagedList<TourInfoVModel> tourList = _planDao.Pager<TourInfoVModel>(qmodel.TourList.PageIndex, qmodel.TourList.PageSize, sql.SQL, sql.Arguments);

            return tourList;
        }
    }
}