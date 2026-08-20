using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Dao.Crm;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Op;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Text.Json;

namespace Lvy.Trip.Biz.Product
{
    /// <summary>
    ///
    /// </summary>
    public class TpLineTourPlanBiz : BaseBiz
    {
        private readonly DictBiz dictionBiz = new DictBiz();
        private readonly DestinationBiz desttionBiz = new DestinationBiz();
        private readonly TpTourFlightBiz _tourFlghtBiz = new TpTourFlightBiz();
        private readonly TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private readonly TpLineBiz _lineBiz = new TpLineBiz();
        private readonly TpTourPlanDao _dao = new TpTourPlanDao();

        #region Basic

        /// <summary>
        /// 根据团计划Id获取团计划信息
        /// </summary>
        /// <param name="tourId">团计划Id</param>
        /// <returns></returns>
        public TpTourPlanModel GetTourById(int tourId)
        {
            return _dao.FirstOrDefault(@"SELECT * FROM TpTourPlan WHERE Id=@0", tourId);
        }

        /// <summary>
        /// 根据LineId获取对象列表
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetByLineId(string lineId, bool afterTody = false)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM TpTourPlan WHERE LineId=@0 ", lineId);
            if (afterTody)
                sql.Append(" AND OutDate> NOW() ");
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 排除已经删除的团计划信息
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetByLineId3(int lineId)
        {
            return _dao.Fetch(@"SELECT * FROM TpTourPlan WHERE LineId=@0 and TourState!=0  ", lineId);
        }

        /// <summary>
        /// 每天只取得一个开班 ？
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetByLineId2(int lineId)
        {
            var sql = new Sql();
            sql.Append(" SELECT * FROM TpTourPlan WHERE TourState!=0 and LineId=@0 and id in (", lineId);
            sql.Append(" select min(id) from TpTourPlan p where p.LineId=@0 and TourState!=0  group by p.OutDate ,p.PackageId  ", lineId);
            sql.Append(" ) ");

            return _dao.Query<TpTourPlanModel>(sql.SQL, sql.Arguments).ToList();
        }

        #endregion Basic

        #region 查询

        /// <summary>
        /// 获取团计划列表
        /// </summary>
        /// <param name="searchTourVModel">查询视图对象</param>
        /// <returns></returns>
        public PagedList<TourInfoVModel> GetTourList(SearchTourVModel searchTourVModel,CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@"SELECT tp.Id AS TourId, tp.TourNo, tp.TourState, tp.OutDate, tp.TuiJianType, tp.TourType,tp.BookingLastDays
,Quota.PlanQuota, Quota.HoldQuota, Quota.UseQuota, Quota.UnlockQuota, Quota.UsedQuota
,Line.DepartDest, Line.ArriveDest, Line.LineName
FROM TpTourPlan tp
INNER JOIN TpTourQuotaMap Map ON Map.TourId=tp.Id
INNER JOIN TpQuota Quota ON Quota.Id=Map.QuotaId
INNER JOIN TpLine Line ON Line.LineId=tp.LineId");

            #region 组织查询条件

            sql.Append(@" WHERE Line.IsValid=1 AND tp.OwnerCode=@0", Ansi(userInfo.OwnerCode));
            if (searchTourVModel.PlanStatus == "valid")
            {
                sql.Append(@" AND tp.TourState>0 ");
            }

            if (userInfo.OwnerCode != userInfo.CustomerCode)
                sql.Append(@" AND Line.CustomerCode=@0", Ansi(userInfo.CustomerCode));
            if (!string.IsNullOrEmpty(searchTourVModel.LineId))
                sql.Append(@" AND tp.LineId=@0", searchTourVModel.LineId);
            if (!searchTourVModel.Condition.LineId.IsNullOrEmpty())
                sql.Append(@" AND tp.LineId Like @0", AnsiLike(searchTourVModel.Condition.LineId));
            if (!searchTourVModel.Condition.TourId.IsNullOrEmpty())
                sql.Append(@" AND tp.Id Like @0", AnsiLike(searchTourVModel.Condition.TourId));
            if (!searchTourVModel.Condition.LineName.IsNullOrEmpty())
                sql.Append(@" AND line.LineName LIKE @0", AnsiLike(searchTourVModel.Condition.LineName));
            if (!searchTourVModel.Condition.SupplierCode.IsNullOrEmpty())
                sql.Append(@" AND Line.CustomerCode = @0", Ansi(searchTourVModel.Condition.SupplierCode));

            if (!string.IsNullOrEmpty(searchTourVModel.Condition.OutDateRange))
            {
                var t = searchTourVModel.Condition.OutDateRange.Split('-');
                sql.Append(@" AND tp.OutDate>=@0 AND tp.OutDate<=@1 ", t[0].ToDateTime(), t[1].ToDateTime());
            }

            if (!searchTourVModel.Condition.ArriveDest.IsNullOrEmpty())
                sql.Append(@" AND Line.ArriveDest = @0", Ansi(searchTourVModel.Condition.ArriveDest));
            if (searchTourVModel.Condition.RecommendType >= 0)
                sql.Append(@" AND tp.TuiJianType = @0", searchTourVModel.Condition.RecommendType);
            if (searchTourVModel.Condition.TourType > 0)
                sql.Append(@" AND tp.TourType = @0", searchTourVModel.Condition.TourType);

            if (!searchTourVModel.Condition.CrmTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and Line.TeamID=@0 ", searchTourVModel.Condition.CrmTeamId);
            }

            #endregion 组织查询条件

            sql.Append(@" ORDER BY tp.OutDate ");
            //sql.Append(@" ORDER BY tp.ModifiedTime DESC, tp.OutDate");
            var result = _dao.Pager<TourInfoVModel>(searchTourVModel.TourList.PageIndex, searchTourVModel.TourList.PageSize, sql.SQL, sql.Arguments);

            //查询出发-目的地
            GetArriveAndDepart(result.Items);

            return result;
        }

        /// <summary>
        /// 同上，取得所有的不分页
        /// </summary>
        /// <param name="searchTourVModel"></param>
        /// <returns></returns>
        public List<TourInfoVModel> GetTourStoreList(SearchTourVModel searchTourVModel, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@"SELECT Tour.Id AS TourId, Tour.TourState, Tour.OutDate, Tour.TuiJianType, Tour.TourType,
Tour.BookingLastDays,Quota.PlanQuota,Quota.HoldQuota,Quota.UseQuota,Line.DepartDest,Line.ArriveDest,Line.LineName
FROM TpTourPlan Tour
INNER JOIN TpTourQuotaMap Map ON Map.TourId=Tour.Id
INNER JOIN TpQuota Quota ON Quota.Id=Map.QuotaId
INNER JOIN TpLine Line ON Line.LineId=Tour.LineId");
            sql.Append(@" WHERE Line.IsValid=1 and TourState!=0 ");
            if (!string.IsNullOrEmpty(searchTourVModel.LineId))
                sql.Append(@" AND Tour.LineId=@0", searchTourVModel.LineId);
            if (!searchTourVModel.Condition.OutDateRange.IsNullOrEmpty())
            {
                var t = searchTourVModel.Condition.OutDateRange.Split('-');
                sql.Append(@" AND Tour.OutDate>=@0 AND Tour.OutDate<=@1", t[0].ToDateTime(), t[1].ToDateTime());
            }

            sql.Append(@" ORDER BY Tour.OutDate DESC");

            var result = _dao.Query<TourInfoVModel>(sql.SQL, sql.Arguments).ToList<TourInfoVModel>();
            //查询出发-目的地
            GetArriveAndDepart(result);

            return result;
        }

        /// <summary>
        /// 获取团计划列表 【微网站使用】
        ///
        /// 只查大于等于今天上线开班
        /// </summary>
        /// <param name="searchTourVModel">查询视图对象</param>
        /// <returns></returns>
        public SearchTourVModel SearchLinq(SearchTourVModel searchTourVModel)
        {
            var sql = new Sql();
            sql.Append(@"SELECT L.* FROM TpLine L
WHERE L.LineId in(select b.lineid from TpTourPlan b where b.OutDate >=@0 and b.TourState = 3)
AND L.IsValid=1 AND L.LineState=3 AND L.OwnerCode=@1", DateTime.Today, Ansi(searchTourVModel.OwnerCode));

            //if (userInfo.OwnerCode != userInfo.CustomerCode)
            //    sql.Append(@" AND L.CustomerCode=@0", Ansi(userInfo.CustomerCode));
            if (!searchTourVModel.NavCondition.Region.IsNullOrEmpty())
            {
                string q = searchTourVModel.NavCondition.Region.Replace(",", "','");
                sql.Append(@" AND L.ArriveDest in ('" + q + "') ");
            }
            if (!string.IsNullOrEmpty(searchTourVModel.NavCondition.OutCity))
                sql.Append(@" AND (L.DepartDest IS NULL OR L.DepartDest = @0) ", searchTourVModel.NavCondition.OutCity);

            var result = _dao.Query<TpLineModel>(sql.SQL, sql.Arguments).ToList();
            foreach (var dd in result)
            {
                dd.Tours = GetToursByLine(dd.LineId);
            }

            searchTourVModel.LineList = result;
            //查询出发-目的地
            //GetArriveAndDepart(result.Items);

            return searchTourVModel;
        }

        /// <summary>
        /// 取得线路所有开班计划
        /// </summary>
        /// <param name="lineId">产品ID</param>
        /// <returns></returns>
        public List<TourInfoVModel> GetToursByLine(string lineId, bool afterToday = false)
        {
            var sql = new Sql();
            sql.Append(@"SELECT T.Id AS TourId, L.LineName, T.TourState, T.OutDate, T.TuiJianType,T.TourType, T.BookingLastDays, T.Price,
Q.PlanQuota,Q.HoldQuota,Q.UseQuota
FROM TpTourPlan T
INNER JOIN TpLine L ON T.LineId=L.LineId
INNER JOIN TpTourQuotaMap Map ON Map.TourId=T.Id
INNER JOIN TpQuota Q ON Q.Id=Map.QuotaId
WHERE T.LineId=@0 ", lineId);
            if (afterToday)
                sql.Append(" AND T.OutDate>NOW() ");

            sql.Append(" ORDER BY T.OutDate");

            var list = _dao.Query<TourInfoVModel>(sql.SQL, sql.Arguments).ToList();

            return list;
        }

        /// <summary>
        /// 查询出发-目的地
        /// </summary>
        /// <param name="items"></param>
        private void GetArriveAndDepart(IEnumerable<TourInfoVModel> items)
        {
            var destDao = new DestinationDao();
            var dests = destDao.GetDests();
            foreach (var tour in items)
            {
                if (!string.IsNullOrEmpty(tour.DepartDest))
                {
                    var tempDepart = DictionaryBiz.GetEnumValue(Enums.OutCityEnum, tour.DepartDest);
                    if (!tempDepart.IsNullOrEmpty())
                        tour.DepartDestName = tempDepart;
                }
                var tempArrive = dests.FirstOrDefault(i => i.ParentStr == tour.ArriveDest);
                if (tempArrive != null)
                    tour.ArriveDestName = tempArrive.Name;
            }
        }

        /// <summary>
        /// 更改团计划的上线、下线状态
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modifiedBy"></param>
        /// <returns></returns>
        public int SaleOrClose(TpTourPlanModel model, string modifiedBy)
        {
            switch (model.TourState)
            {
                case 2:
                    model.TourState = 3;
                    break;

                case 3:
                    model.TourState = 2;
                    break;
            }
            model.ModifiedBy = modifiedBy;
            model.ModifiedTime = DateTime.Now;
            _dao.Update(model);
            return model.TourState;
        }

        /// <summary>
        /// 删除团计划
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="modifiedBy"></param>
        /// <returns></returns>
        public int DeleteTour(int tourId, string modifiedBy)
        {
            return _dao.Update("SET TourState=0, ModifiedBy=@1, ModifiedTime=now() WHERE Id=@0 ", tourId, modifiedBy);
        }

        public int RestoreTour(int tourId, string modifiedBy)
        {
            return _dao.Update("SET TourState=2, ModifiedBy=@1, ModifiedTime=now() WHERE Id=@0 ", tourId, modifiedBy);
        }

        #endregion 查询

        #region 添加团计划

        /// <summary>
        /// 添加团计划
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modifyBy"></param>
        /// <param name="ownerCode"></param>
        public void AddTour(AddTourVModel model, string modifyBy, string ownerCode)
        {
            var days = GetTourDays(model.SelectedDays);

            if (days.Count > 0)
            {
                //插入数据
                var lineModel = _lineBiz.GetLineById(model.LineId);
                DateTime modifyTime = DateTime.Now;
                //团号：CCT20180715PJ-A
                //获取出发地代码
                string DepartDestCode = "";
                var dictionModel = dictionBiz.GetOutCityEnum(lineModel.DepartDest);
                if (dictionModel != null)
                {
                    DepartDestCode = dictionModel.JPinYin;
                }
                //获取目的地代码
                string ArriveDestCode = "";

                var desttionModel = desttionBiz.GetByStr(lineModel.ArriveDest);
                if (desttionModel != null)
                {
                    ArriveDestCode = desttionModel.JPinYin;
                }
                //获取团的记录数 设置后缀 -A,-B,-C等等
                //定义一个后缀数组.
                string[] h = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                List<TpTourPlanModel> ListAllTourPlan = GetByLineId(model.LineId);

                //团计划表
                var standardPrice = model.PriceList.FirstOrDefault(p => p.IsStandard == 1);
                var newTour = new TpTourPlanModel
                {
                    LineId = model.LineId,
                    Price = standardPrice == null ? 0 : standardPrice.Price, //若标准价取值为null，默认赋值0
                    // FanLi = standardPrice == null ? 0 : standardPrice.FanLi,   //若标准价取值为null，默认赋值0
                    SettlePrice = standardPrice.SettlePrice,
                    TuiJianType = model.TourPlan.TuiJianType,
                    TourType = model.TourPlan.TourType,
                    Source = 1,//Default  同业
                    TourState = model.TourPlan.TourState,
                    AuditState = 0,
                    MixedNum = model.TourPlan.MixedNum,
                    Remarks = model.TourPlan.Remarks,
                    AdditionInfo = model.TourPlan.AdditionInfo,
                    ModifiedBy = modifyBy,
                    ModifiedTime = modifyTime,
                    OwnerCode = ownerCode,
                    PackageId = model.TourPlan.PackageId,
                    LastKaiPiaoDate = model.TourPlan.LastKaiPiaoDate,
                    VisaPrice = model.TourPlan.VisaPrice,
                    Tax = model.TourPlan.Tax,
                    SingleRoom = model.SingleRoom
                };

                //资源表
                var newQuota = new QuotaModel
                {
                    PlanQuota = model.Quota.PlanQuota,
                    UseQuota = model.Quota.PlanQuota - model.Quota.HoldQuota,
                    UsedQuota = model.Quota.UsedQuota,
                    UnLockQuota = 0,
                    HoldQuota = model.Quota.HoldQuota,
                    ShareDesc = model.Quota.ShareDesc,
                    ModifiedBy = modifyBy,
                    ModifiedTime = modifyTime,
                    OwnerCode = ownerCode,
                    Source = 1,
                    TrafficType = lineModel.TrafficType
                };

                var newMap = new TourQuotaMapModel
                {
                    Max = 0,//Default
                    Source = 1//Default
                };

                //座位表
                TpBusSeatModel newBusSeat = null;
                if (lineModel.TrafficType == BusInTrafficType)
                {
                    //交通类型为汽车
                    var seatDetail = JsonSerializer.Serialize(model.SeatList);
                    newBusSeat = new TpBusSeatModel { SeatNum = model.Quota.PlanQuota, SeatDetail = seatDetail };
                }

                //价格列表
                var priceTypeBeans = DictionaryBiz.GetEnumsBy(Enums.TpPriceTypeEnum);
                var priceList = (from price in model.PriceList
                                 where price.Id >= 0 && price.PriceType > 0
                                 let priceTypeBean = priceTypeBeans.FirstOrDefault(p => p.Key == price.PriceType.ToString(CultureInfo.InvariantCulture))
                                 select new TpPriceModel
                                 {
                                     PriceType = price.PriceType,
                                     PriceTypeName = priceTypeBean == null ? string.Empty : priceTypeBean.Value,
                                     PriceRemark = string.IsNullOrEmpty(price.PriceRemark) ? priceTypeBean.Value : price.PriceRemark,
                                     Price = price.Price,
                                     SettlePrice = price.SettlePrice,
                                     Cost = price.Cost,
                                     //Tips = model.Tips,
                                     //ZiFei = price.ZiFei,
                                     //SingleRoom = model.SingleRoom,
                                     TeJiaFanLi = price.TeJiaFanLi,
                                     IsStandard = price.IsStandard,
                                     IsValid = price.IsValid,
                                     SuitNum = price.SuitNum,
                                     ModifiedBy = modifyBy,
                                     ModifiedTime = modifyTime
                                 }).ToList();

                var mapDao = new TourQuotaMapDao();
                var buSeatDao = new TpBusSeatDao();
                var priceDao = new TpPriceDao();

                //获取当前线路套餐下的所有的TourPlan
                List<TpTourPlanModel> tourPlanList = GetTourPlanList(model.LineId, days);
                using (var scope = new TransactionScope())
                {
                    foreach (DateTime day in days)
                    {
                        ////检查当前日期是否已经存在出团计划  保证同一套餐同一日期下仅有一个出团计划
                        //var list = tourPlanList.Where(a => a.OutDate == day).ToList();
                        //if (list.Count>0)
                        //{
                        //    continue;
                        //}

                        //设置团号
                        var list = ListAllTourPlan.Where(a => a.OutDate == day && a.PackageId == newTour.PackageId).ToList();
                        newTour.TourNo = string.Format("{0}{1}{2}-{3}", DepartDestCode, day.ToString("yyyyMMdd"), ArriveDestCode, h[list.Count]);
                        newTour.OutDate = day;
                        newTour.BookingLastDays = day.AddDays(-(lineModel.MoveUpDays));
                        //计算最后开票日期。
                        //newTour.LastKaiPiaoDate = newTour.OutDate.AddDays(- newTour.KaiPiaoJieZhiDay);
                        newTour.LastKaiPiaoDate = newTour.OutDate.AddDays(-lineModel.MoveUpDays);

                        newQuota.OutDate = day;

                        int newTourId = Convert.ToInt32(_dao.Insert(newTour));
                        int newQuotaId = Convert.ToInt32(_quotaBiz.Insert(newQuota));
                        newMap.TourId = newTourId;
                        newMap.QuotaId = newQuotaId;
                        mapDao.Insert(newMap);
                        if (null != newBusSeat)
                        {
                            newBusSeat.QuotaId = newQuotaId;
#pragma warning disable 612,618
                            newBusSeat.TourId = newTourId;
#pragma warning restore 612,618
                            buSeatDao.Insert(newBusSeat);
                        }
                        foreach (var price in priceList)
                        {
                            price.TourId = newTourId;
                            priceDao.Insert(price);
                        }
                        if (model.TourFlightList != null)
                        {
                            //插入航班信息数据
                            foreach (var item in model.TourFlightList)
                            {
                                item.TourId = newTourId;
                                if (!string.IsNullOrEmpty(item.AirlineCode))
                                    _tourFlghtBiz.Insert(item);
                            }
                        }
                    }
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// 获取团期
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        /// string beginDate, string endDate,
        private List<DateTime> GetTourDays(string days)
        {
            var dateList = new List<DateTime>();

            foreach (var item in days.Split(','))
            {
                if (!item.IsNullOrEmpty())
                {
                    dateList.Add(Convert.ToDateTime(item));
                }
            }

            return dateList;
        }

        #endregion 添加团计划

        #region 编辑页

        #region 编辑页 初始化

        /// <summary>
        /// 获取编辑团计划所需数据
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public EditTourVModel GetEditTour(int tourId, string ownerCode)
        {
            if (tourId <= 0)
                throw new Exception("团计划Id不能小于等于0");
            #region 组装视图模型

            var vModel = new EditTourVModel();
            var mapBiz = new TpTourQuotaMapBiz();
            vModel.Map = mapBiz.GetMapWithAll(tourId);              //Map
            vModel.Tour = vModel.Map.Tour;                          //团
            vModel.Quota = vModel.Map.Quota;                        //库存
            vModel.Line = _lineBiz.GetLineById(vModel.Tour.LineId);  //线路

            if (vModel.Map.Source == 2)                             //若当前为共享，获取共享下拉列表数据
            {
                vModel.ShareQuotaDic = _quotaBiz.GetShareQuotaDic(vModel.Tour.OutDate, vModel.Line.TrafficType, ownerCode);
            }

            if (vModel.Line.TrafficType == BusInTrafficType)        //若交通类型为汽车，则需要获取当前座位信息
            {
                var busSeatBiz = new TpBusSeatBiz();
                vModel.BusSeat = vModel.Quota.Source == 1 ? busSeatBiz.GetBusSeat(vModel.Tour.Id, vModel.Quota.Id) : busSeatBiz.GetByShareQuota(vModel.Quota.Id);
            }

            var priceBiz = new TpPriceBiz();
            vModel.PriceList = priceBiz.GetPrices(tourId);          //价格
            //if (vModel.PriceList != null && vModel.PriceList.Count > 0)
            //{
            //    var firstPrice = vModel.PriceList.FirstOrDefault();
            //    vModel.Tips = (firstPrice == null) ? 0 : firstPrice.Tips;
            //    vModel.SingleRoom = (firstPrice == null) ? 0 : firstPrice.SingleRoom;
            //    vModel.TeJiaFanLi = (firstPrice == null) ? 0 : firstPrice.TeJiaFanLi;
            //}

            //获取航空公司信息
            AirlineBiz airlineBiz = new AirlineBiz();
            vModel.AirlineList = airlineBiz.GetAirlineList();
            //获取团计划航班信息

            vModel.TourFlightList = airlineBiz.GetTpTourFlightList(tourId);
            #endregion 组装视图模型

            return vModel;
        }

        #endregion 编辑页 初始化

        #region 编辑页 提交

        /// <summary>
        /// 保存团计划（入口）
        /// </summary>
        /// <param name="vModel"></param>
        public void SaveTour(EditTourVModel vModel, CrmAccountModel currentUser)
        {
            if (vModel.IsCopy == 0)
            {
                SaveEdit(vModel, DateTime.Now, currentUser);
            }
            else
            {
                SaveCopy(vModel, DateTime.Now, currentUser);
            }
        }

        #region 编辑团计划 保存

        /// <summary>
        /// 填充更新的团计划对象
        ///
        /// 缺少团号生成 //TODO
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="modifiedTime"></param>
        private void FillUpdateTour(EditTourVModel vModel, DateTime modifiedTime, CrmAccountModel currentUser)
        {
            //团计划
            TpTourPlanModel updateTour = GetTourById(vModel.Tour.Id);
            updateTour.MixedNum = vModel.Tour.MixedNum;
            updateTour.BookingLastDays = vModel.Tour.BookingLastDays;
            updateTour.TuiJianType = vModel.Tour.TuiJianType;
            updateTour.TourType = vModel.Tour.TourType;
            updateTour.TourState = vModel.Tour.TourState;
            updateTour.Remarks = vModel.Tour.Remarks;
            updateTour.AdditionInfo = vModel.Tour.AdditionInfo;
            updateTour.ModifiedBy = currentUser.Code;
            updateTour.ModifiedTime = modifiedTime;
            updateTour.OwnerCode = currentUser.OwnerCode;
            var standardPrice = vModel.PriceList.FirstOrDefault(p => p.IsStandard == 1);
            updateTour.Price = standardPrice != null ? standardPrice.Price : 0;
            updateTour.SettlePrice = standardPrice != null ? standardPrice.SettlePrice : 0;
            updateTour.LastKaiPiaoDate = vModel.Tour.LastKaiPiaoDate;

            // 费用共通
            updateTour.VisaPrice = vModel.Tour.VisaPrice;
            updateTour.Tax = vModel.Tour.Tax;
            updateTour.ZiFei = vModel.Tour.ZiFei;
            updateTour.SingleRoom = vModel.Tour.SingleRoom;
            updateTour.Tips = vModel.Tour.Tips;

            vModel.Tour = updateTour;
        }

        /// <summary>
        /// 将提交的价格新增与更新分离
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="modifiedTime"></param>
        /// <returns></returns>
        private Dictionary<string, List<TpPriceModel>> ReturnUpOrAddPrice(EditTourVModel vModel, DateTime modifiedTime, CrmAccountModel userInfo)
        {
            var addPrice = new List<TpPriceModel>();
            var upPrice = new List<TpPriceModel>();
            var priceBiz = new TpPriceBiz();
            var deletePrice = priceBiz.GetPrices(vModel.Tour.Id);

            //if (vModel.Tour.TuiJianType == 0)
            //    vModel.Tour.TeJiaFanLi = 0.00m;//若为常规团，则视特价让利为0
            var priceTypeBeans = DictionaryBiz.GetEnumsBy(Enums.TpPriceTypeEnum);
            foreach (var priceItem in vModel.PriceList)
            {
                var priceTypeBean = priceTypeBeans.FirstOrDefault(p => p.Key == priceItem.PriceType.ToString(CultureInfo.InvariantCulture));
                if (priceItem.Id > 0)
                {
                    var priceModel = priceBiz.GetById(priceItem.Id);
                    priceModel.PriceType = priceItem.PriceType;
                    priceModel.PriceTypeName = priceTypeBean == null ? string.Empty : priceTypeBean.Value;
                    priceModel.PriceRemark = priceItem.PriceRemark.IsNullOrEmpty() ? (priceTypeBean == null ? string.Empty : priceTypeBean.Value) : priceItem.PriceRemark;
                    priceModel.Price = priceItem.Price;
                    priceModel.SettlePrice = priceItem.SettlePrice;
                    priceModel.Cost = priceItem.Cost;
                    priceModel.SuitNum = priceItem.SuitNum;
                    priceModel.IsValid = priceItem.IsValid;
                    priceModel.IsStandard = priceItem.IsStandard;

                    //// 开班 公共数据
                    //priceModel.Tips = vModel.Tips;
                    //priceModel.SingleRoom = vModel.SingleRoom;
                    priceModel.TeJiaFanLi = priceItem.TeJiaFanLi;
                    //priceModel.ZiFei = priceItem.ZiFei;

                    priceModel.ModifiedBy = userInfo.Code;
                    priceModel.ModifiedTime = modifiedTime;
                    upPrice.Add(priceModel);

                    deletePrice.Remove(deletePrice.Find(p => p.Id == priceItem.Id));
                }
                else if (priceItem.Id == 0)
                {
                    var newPrice = new TpPriceModel
                    {
                        TourId = vModel.Tour.Id,
                        PriceType = priceItem.PriceType,
                        PriceTypeName = priceTypeBean == null ? string.Empty : priceTypeBean.Value,
                        PriceRemark = priceItem.PriceRemark,
                        Price = priceItem.Price,
                        SettlePrice = priceItem.SettlePrice,
                        Cost = priceItem.Cost,
                        //Tips = vModel.Tour.Tips,
                        //ZiFei = vModel.Tour.ZiFei,
                        //SingleRoom = vModel.Tour.SingleRoom,
                        TeJiaFanLi = priceItem.TeJiaFanLi,
                        IsStandard = priceItem.IsStandard,
                        IsValid = priceItem.IsValid,
                        SuitNum = priceItem.SuitNum,
                        ModifiedBy = userInfo.Code,
                        ModifiedTime = modifiedTime
                    };
                    addPrice.Add(newPrice);
                }
            }
            var dic = new Dictionary<string, List<TpPriceModel>> { { "Add", addPrice }, { "Update", upPrice }, { "Delete", deletePrice } };
            return dic;
        }

        ///// <summary>
        ///// 返回待更新的Map和Quota
        ///// </summary>
        ///// <param name="vModel"></param>
        ///// <param name="modifiedTime"></param>
        ///// <returns></returns>
        //private EditTourVModel ReturnMapAndQuota(EditTourVModel vModel, DateTime modifiedTime)
        //{
        //    var quotaDao = new QuotaDao();
        //    if (vModel.Quota.Id == 0)
        //    {
        //        var quota = new QuotaModel
        //        {
        //            PlanQuota = vModel.Quota.PlanQuota,
        //            UseQuota = vModel.Quota.PlanQuota - vModel.Quota.HoldQuota,
        //            UsedQuota = vModel.Quota.UsedQuota,
        //            HoldQuota = vModel.Quota.HoldQuota,
        //            ShareDesc = vModel.Quota.ShareDesc,
        //            ModifiedBy = GlobalContext.Current.UserInfo.Code,
        //            ModifiedTime = modifiedTime,
        //            OwnerCode = GlobalContext.Current.UserInfo.OwnerCode,
        //            Source = 1,
        //            TrafficType = vModel.Line.TrafficType
        //        };
        //        vModel.Quota.Id = Convert.ToInt32(quotaDao.Insert(quota));
        //    }
        //    QuotaModel vQuota = quotaDao.GetById(vModel.Quota.Id); //视图选择的库存

        //    TpTourQuotaMapBiz mapBiz = new TpTourQuotaMapBiz();
        //    TourQuotaMapModel oldMap = mapBiz.GetMapWithQuota(vModel.Tour.Id); //数据库当前的Map
        //    QuotaModel oldQuota = oldMap.Quota; //数据库当前的与库存

        //    TourQuotaMapModel updateMap = null; //待更新的Map
        //    QuotaModel updateQuota = null; //待更新的库存
        //    TpBusSeatModel newBusSeat = null;
        //    if (vQuota.Id == oldQuota.Id)
        //    {
        //        //未更改对应关系，故Map无需更新
        //        updateQuota = oldQuota;
        //        if (vModel.BusSeat != null)
        //        {
        //            newBusSeat = vModel.BusSeat;
        //            newBusSeat.QuotaId = oldQuota.Id;
        //        }
        //        if (oldQuota.Source == 1)//若Source==2，则表明为共享库存，无需修改库存信息
        //        {
        //            if (vModel.BusSeat != null)
        //                newBusSeat.TourId = vModel.Tour.Id;
        //            updateQuota.PlanQuota = vModel.Quota.PlanQuota;
        //            updateQuota.HoldQuota = vModel.Quota.HoldQuota;
        //            updateQuota.UseQuota = updateQuota.PlanQuota - updateQuota.HoldQuota - updateQuota.UsedQuota;
        //            updateQuota.ModifiedBy = GlobalContext.Current.UserInfo.Code;
        //            updateQuota.ModifiedTime = modifiedTime;
        //        }
        //    }
        //    else
        //    {
        //        //更改对应关系，需更新Map
        //        updateMap = oldMap;
        //        updateMap.QuotaId = vQuota.Id;
        //        updateMap.Source = vQuota.Source;

        //        updateQuota = vQuota;
        //        if (vModel.BusSeat != null)
        //        {
        //            newBusSeat = vModel.BusSeat;
        //            newBusSeat.QuotaId = vQuota.Id;
        //        }
        //        if (vQuota.Source == 1)
        //        {//从共享变为非共享
        //            //座位表需重建
        //            if (vModel.BusSeat != null)
        //            {
        //                newBusSeat.Id = 0;
        //                newBusSeat.TourId = vModel.Tour.Id;
        //            }
        //        }
        //        else
        //        { //从非共享变为共享
        //            //需删除现有座位
        //            if (vModel.BusSeat != null)
        //            {
        //                newBusSeat.Id = -1;
        //            }
        //        }

        //    }
        //    return new EditTourVModel { Map = updateMap, Quota = updateQuota, BusSeat = newBusSeat };
        //}

        private void FilterQuotaSeat(EditTourVModel vModel, out TourQuotaMapModel map, out Dictionary<string, QuotaModel> quotaDic,
            out Dictionary<string, TpBusSeatModel> seatDic, DateTime modifiedTime, CrmAccountModel currentUser)
        {
            var mapBiz = new TpTourQuotaMapBiz();
            var seatBiz = new TpBusSeatBiz();
            quotaDic = new Dictionary<string, QuotaModel>();
            seatDic = new Dictionary<string, TpBusSeatModel>();
            if (vModel.Quota.Id == 0)
            {
                /*若提交的库存Id为零，则表明该团计划库存由共享转为标准，需要重新创建编辑的库存*/
                #region
                quotaDic.Add("Add", new QuotaModel
                {
                    PlanQuota = vModel.Quota.PlanQuota,
                    UseQuota = vModel.Quota.PlanQuota - vModel.Quota.HoldQuota,
                    UsedQuota = vModel.Quota.UsedQuota,
                    HoldQuota = vModel.Quota.HoldQuota,
                    OutDate = vModel.Tour.OutDate,
                    ShareDesc = vModel.Quota.ShareDesc,
                    Source = 1,  //标准团
                    ModifiedBy = currentUser.Code,
                    ModifiedTime = modifiedTime,
                    OwnerCode = currentUser.OwnerCode,
                    TrafficType = vModel.Line.TrafficType
                });
                map = mapBiz.GetMap(vModel.Tour.Id);
                map.Source = 1;                 //标准团
                map.QuotaId = 0;                //在事务中创建库存后赋值
                if (vModel.Line.TrafficType == BusInTrafficType && vModel.BusSeat != null)
                {
                    /*在交通类型为汽车的状况下，需重新创建座位表*/
                    vModel.BusSeat.Id = 0;
                    vModel.BusSeat.TourId = vModel.Tour.Id;
                    vModel.BusSeat.QuotaId = 0; //在事务中创建库存后赋值
                    seatDic.Add("Add", vModel.BusSeat);
                }
                #endregion 编辑团计划 保存
            }
            else
            {
                /*
                 * 若提交的库存Id大于零，则分两种情况处理：
                 * 1. 变更关系（共享-->标准已在前面捕获，此处为标准-->共享或共享变更）
                 * 2. 不变更关系（标准下，需修改库存，共享下库存无需修改）
                 */
                #region
                map = mapBiz.GetMap(vModel.Tour.Id);
                var selectQuota = _quotaBiz.GetQuota(vModel.Quota.Id);   //当前选择的库存
                if (map.QuotaId == vModel.Quota.Id)
                {
                    /*关系不变*/
                    #region
                    if (selectQuota.Source == 1)
                    {
                        /*标准团*/
                        selectQuota.PlanQuota = vModel.Quota.PlanQuota;
                        selectQuota.HoldQuota = vModel.Quota.HoldQuota;
                        selectQuota.UseQuota = selectQuota.PlanQuota - selectQuota.HoldQuota - selectQuota.UsedQuota;
                        selectQuota.ModifiedBy = currentUser.Code;
                        selectQuota.ModifiedTime = modifiedTime;
                        quotaDic.Add("Update", selectQuota);
                        if (vModel.Line.TrafficType == BusInTrafficType && vModel.BusSeat != null)
                        {
                            var unShareSeat = seatBiz.GetBusSeat(vModel.Tour.Id, selectQuota.Id);
                            unShareSeat.SeatNum = vModel.BusSeat.SeatNum;
                            unShareSeat.SeatDetail = vModel.BusSeat.SeatDetail;
                            seatDic.Add("Update", unShareSeat);
                        }
                    }
                    else if (selectQuota.Source == 2)
                    {
                        /*共享团,无需更新库存（页面锁定），座位表若存在则更新*/
                        if (vModel.Line.TrafficType == BusInTrafficType && vModel.BusSeat != null)
                        {
                            var shareSeat = seatBiz.GetByShareQuota(selectQuota.Id);
                            shareSeat.SeatDetail = vModel.BusSeat.SeatDetail;
                            seatDic.Add("Update", shareSeat);
                        }
                    }
                    map = null;
                    #endregion 编辑页 提交
                }
                else
                {
                    /*关系改变*/
                    #region
                    var dbQuota = _quotaBiz.GetQuota(map.QuotaId);       //当前关联的库存
                    if (dbQuota.Source == 1 && selectQuota.Source == 2)
                    {
                        /*标准-->共享,删除目前库存与座位表（若存在）*/
                        quotaDic.Add("Delete", dbQuota);
                        var deleteSeat = seatBiz.GetBusSeat(vModel.Tour.Id, dbQuota.Id);    //当前关联的座位
                        if (deleteSeat != null)
                            seatDic.Add("Delete", deleteSeat);
                    }
                    else if (dbQuota.Source == 2 && selectQuota.Source == 2)
                    {
                        /*共享-->共享,库存部分无需改变，仅修改座位表（若提交存在）*/
                        if (vModel.Line.TrafficType == BusInTrafficType && vModel.BusSeat != null)
                        {
                            var shareSeat = seatBiz.GetByShareQuota(selectQuota.Id);
                            shareSeat.SeatDetail = vModel.BusSeat.SeatDetail;
                            seatDic.Add("Update", shareSeat);
                        }
                    }
                    map.QuotaId = selectQuota.Id;
                    map.Source = 2;                 //共享
                    #endregion 编辑页
                }
                #endregion
            }
        }

        /// <summary>
        /// 保存更新
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="modifiedTime">统一的更新时间</param>
        private void SaveEdit(EditTourVModel vModel, DateTime modifiedTime, CrmAccountModel currentUser)
        {
            FillUpdateTour(vModel, modifiedTime, currentUser);

            TourQuotaMapModel map;
            Dictionary<string, QuotaModel> quotaDic;
            Dictionary<string, TpBusSeatModel> seatDic;
            FilterQuotaSeat(vModel, out map, out quotaDic, out seatDic, modifiedTime, currentUser);

            var dic = ReturnUpOrAddPrice(vModel, modifiedTime, currentUser);                     //待更新的价格

            var quotaDao = new QuotaDao();
            var mapDao = new TourQuotaMapDao();
            var busSeatDao = new TpBusSeatDao();
            var priceDao = new TpPriceDao();
            using (var scope = new TransactionScope())
            {
                _dao.Update(vModel.Tour);
                var addQuota = quotaDic.ContainsKey("Add") ? quotaDic["Add"] : null;
                if (addQuota != null)
                {
                    int newQuotaId = Convert.ToInt32(quotaDao.Insert(addQuota));
                    map.QuotaId = newQuotaId;
                    mapDao.Update(map);
                    var addSeat = seatDic.ContainsKey("Add") ? seatDic["Add"] : null;
                    if (addSeat != null)
                    {
                        addSeat.QuotaId = newQuotaId;
                        busSeatDao.Insert(addSeat);
                    }
                }
                else
                {
                    var updateQuota = quotaDic.ContainsKey("Update") ? quotaDic["Update"] : null; //更新库存
                    if (updateQuota != null)
                        quotaDao.Update(updateQuota);
                    var updateSeat = seatDic.ContainsKey("Update") ? seatDic["Update"] : null; //更新座位
                    if (updateSeat != null)
                        busSeatDao.Update(updateSeat);

                    var deleteQuota = quotaDic.ContainsKey("Delete") ? quotaDic["Delete"] : null; //删除库存
                    if (deleteQuota != null)
                    {
                        quotaDao.Delete(deleteQuota);
                        if (map != null)          // 关联外键删除 所以重新添加
                            mapDao.Insert(map);
                    }
                    else if (map != null)
                        mapDao.Update(map);

                    var deleteSeat = seatDic.ContainsKey("Delete") ? seatDic["Delete"] : null; //删除座位
                    if (deleteSeat != null)
                        busSeatDao.Delete(deleteSeat);
                }

                foreach (var price in dic["Add"])
                {
                    priceDao.Insert(price);
                }
                foreach (var price in dic["Update"])
                {
                    priceDao.Update(price);
                }
                foreach (var price in dic["Delete"])
                {
                    price.IsValid = 0;
                    price.IsStandard = 0;
                    priceDao.Update(price);
                }
                _tourFlghtBiz.DeleteTourFlight(vModel.Tour.Id);

                if (vModel.TourFlightList != null)
                {
                    foreach (var item in vModel.TourFlightList)
                    {
                        item.TourId = vModel.Tour.Id;
                        _tourFlghtBiz.Insert(item);
                    }
                }

                scope.Complete();
            }
        }

        #endregion

        #region 复制

        /// <summary>
        /// 保存 复制
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="modifiedTime"> </param>
        private void SaveCopy(EditTourVModel vModel, DateTime modifiedTime, CrmAccountModel currentUser)
        {
            var mapBiz = new TpTourQuotaMapBiz();
            var busSeatBiz = new TpBusSeatBiz();

            //团计划
            FillUpdateTour(vModel, modifiedTime, currentUser);

            //if (vModel.Tour.TuiJianType == 0)
            //    vModel.Tour.TeJiaFanLi = 0.00m;//若为常规团，则视特价让利为0

            //资源
            #region

            var addQuota = new QuotaModel
            {
                PlanQuota = vModel.Quota.PlanQuota,
                UseQuota = vModel.Quota.PlanQuota - vModel.Quota.HoldQuota,
                UsedQuota = vModel.Quota.UsedQuota,
                HoldQuota = vModel.Quota.HoldQuota,
                ShareDesc = vModel.Quota.ShareDesc,
                ModifiedBy = currentUser.Code,
                ModifiedTime = modifiedTime,
                OwnerCode = currentUser.OwnerCode,
                Source = 1,
                TrafficType = vModel.Line.TrafficType
            };
            #endregion

            //Map
            var addMap = mapBiz.GetMap(vModel.Tour.Id);
            addMap.Source = addQuota.Source;
            addMap.Id = 0;

            //座位表
            #region
            TpBusSeatModel addBusSeat = null;
            if (vModel.Line.TrafficType == BusInTrafficType)
            {
                /*若为标准，则将Id置零，在事物中判断Id的值，来进行新增或更新*/
                addBusSeat = busSeatBiz.GetBusSeatByTour(addMap.TourId) ?? new TpBusSeatModel();
                addBusSeat.Id = 0;
                if (vModel.BusSeat != null)
                {
                    addBusSeat.SeatNum = vModel.BusSeat.SeatNum;
                    addBusSeat.SeatDetail = vModel.BusSeat.SeatDetail;
                }
                else
                {
                    var temp = addBusSeat.SeatModels.Select(item => item.State == 2 ? new BusSeatModel { No = item.No, State = 1 } : new BusSeatModel { No = item.No, State = item.State }).ToList();
                    addBusSeat.SeatDetail = JsonSerializer.Serialize(temp);
                }
            }
            #endregion

            #region 事务
            var mapDao = new TourQuotaMapDao();
            var busSeatDao = new TpBusSeatDao();
            var priceDao = new TpPriceDao();
            var priceTypeBeans = DictionaryBiz.GetEnumsBy(Enums.TpPriceTypeEnum);
            using (var scope = new TransactionScope())
            {
                int newTourId = Convert.ToInt32(_dao.Insert(vModel.Tour));
                int newQuotaId = addQuota.Id;
                if (addQuota.Id == 0)
                    newQuotaId = Convert.ToInt32(_quotaBiz.Insert(addQuota));

                addMap.TourId = newTourId;
                addMap.QuotaId = newQuotaId;
                mapDao.Insert(addMap);

                if (addBusSeat != null)
                {
                    if (addBusSeat.Id > 0)
                    {
                        busSeatDao.Update(addBusSeat);
                    }
                    else
                    {
#pragma warning disable 612,618
                        addBusSeat.TourId = newTourId;
#pragma warning restore 612,618
                        addBusSeat.QuotaId = newQuotaId;
                        busSeatDao.Insert(addBusSeat);
                    }
                }

                foreach (var priceItem in vModel.PriceList)
                {
                    if (priceItem.Id < 0)
                        continue;
                    var priceTypeBean = priceTypeBeans.FirstOrDefault(p => p.Key == priceItem.PriceType.ToString(CultureInfo.InvariantCulture));
                    priceItem.Id = 0;
                    priceItem.PriceTypeName = priceTypeBean == null ? string.Empty : priceTypeBean.Value;
                    priceItem.PriceRemark = (priceTypeBean == null ? string.Empty : priceTypeBean.Value) + (priceItem.PriceRemark.IsNullOrEmpty() ? string.Empty : "(" + priceItem.PriceRemark + ")");
                    priceItem.TourId = newTourId;
                    //priceItem.SingleRoom = vModel.Tour.SingleRoom;
                    priceItem.TeJiaFanLi = 0;
                    //priceItem.Tips = vModel.Tour.Tips;
                    //priceItem.ZiFei = vModel.Tour.ZiFei;
                    priceItem.Cost = 0; //Default
                    priceItem.ModifiedBy = currentUser.Code;
                    priceItem.ModifiedTime = modifiedTime;
                    priceDao.Insert(priceItem);
                }
                scope.Complete();
            }

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region 批量编辑

        /// <summary>
        /// 获取不重复的团（根据团名）
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetDistinctNameTour(string lineId)
        {
            var sql = new Sql();
            sql.Append(
                @"SELECT * FROM (SELECT *,ROW_NUMBER() OVER(PARTITION BY LineNameSign ORDER BY id) rn FROM TpTourPlan WHERE lineId=@0) T WHERE T.rn=1", lineId);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 保存批量编辑团期
        /// </summary>
        /// <param name="vModel"></param>
        public void SaveBatchEidtTour(BatchEditTourVModel vModel, CrmAccountModel user)
        {
            var modTime = DateTime.Now;
            var tourList = GetTourList(vModel.Line.LineId, vModel.Tour.LineName);
            var existTour = new List<TpTourPlanModel>();    //批量修改所涉及的团计划
            var existPrice = new List<TpPriceModel>();      //批量修改所涉及的价格
            var replacePrice = new List<TpPriceModel>();    //用于替换当前已存价格（在删除当前价格后重新插入的数据）
            var begin = vModel.BeginDate.ToDateTime();
            var end = vModel.EndDate.ToDateTime();
            var priceTypeBeans = DictionaryBiz.GetEnumsBy(Enums.TpPriceTypeEnum);
            while (begin <= end)
            {
                //若选择了星期，且星期中不包含当天，则跳过
                if (vModel.SelectedDays.Count > 0 && !vModel.SelectedDays.Contains(begin.DayOfWeek.ToString()))
                {
                    begin = begin.AddDays(1);
                    continue;
                }
                var tours = tourList.Where(p => p.OutDate == begin).ToList();//同一天出发的同名团期可能有多个。
                if (tours.Count > 0)
                {
                    foreach (var tour in tours)
                    {
                        tour.TuiJianType = vModel.Tour.TuiJianType; //更新推荐类型
                        var prices = new TpPriceBiz().GetPrices(tour.Id);
                        existPrice.AddRange(prices);
                        foreach (var price in vModel.PriceList)
                        {
                            if (price.Id < 0)
                                continue;
                            var priceTypeBean = priceTypeBeans.FirstOrDefault(p => p.Key == price.PriceType.ToString(CultureInfo.InvariantCulture));
                            if (price.IsStandard == 1)
                                tour.Price = price.Price;           //更新标准价
                            var iPirce = new TpPriceModel
                            {
                                PriceType = price.PriceType,
                                PriceTypeName = priceTypeBean == null ? string.Empty : priceTypeBean.Value,
                                PriceRemark = price.PriceRemark,
                                Price = price.Price,
                                SettlePrice = price.SettlePrice,
                                IsStandard = price.IsStandard,
                                IsValid = price.IsValid,
                                SuitNum = price.SuitNum,
                                TourId = tour.Id,
                                Cost = prices.Count > 0 ? prices[0].Cost : 0,
                                TeJiaFanLi = price.TeJiaFanLi,
                                ModifiedBy = user.Code,
                                ModifiedTime = modTime
                            };

                            replacePrice.Add(iPirce);
                        }
                    }
                    existTour.AddRange(tours);
                }
                else
                {
                    //TODO:对于不存在的团期，做新增处理，因页面未涉及库存等信息输入，故暂不处理
                }
                begin = begin.AddDays(1);
            }
            var priceDao = new TpPriceDao();
            using (var scope = new TransactionScope())
            {
                foreach (var tour in existTour)
                {
                    _dao.Update(tour);
                }
                foreach (var dPrice in existPrice)
                {
                    dPrice.IsValid = 0;
                    dPrice.IsStandard = 0;
                    priceDao.Update(dPrice);
                }
                foreach (var iPrice in replacePrice)
                {
                    priceDao.Insert(iPrice);
                }
                scope.Complete();
            }
        }

        /// <summary>
        /// 根据线路编号、线路名+标注查找团计划
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="lineName"></param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetTourList(string lineId, string lineName)
        {
            return _dao.Fetch(@"SELECT tp.*, tl.LineName FROM TpTourPlan tp
INNER JOIN TpLine tl ON tp.LineId=tl.LineId
WHERE tl.LineId=@0 AND tl.LineName=@1", lineId, Ansi(lineName));
        }

        #endregion

        #region 复制团期
        /*
         * 此处将复制团期功能单独处理，
         * 待有时间再将团计划管理部分的复制团期拆解
         */

        /// <summary>
        /// 返回复制的团期
        ///
        /// 团号说明缺少 //TODO
        /// </summary>
        /// <param name="tourInfo"></param>
        /// <param name="standardPrice"></param>
        /// <param name="modifyTime"></param>
        /// <returns></returns>
        public TpTourPlanModel CreateCopiedTour(TpTourPlanModel tourInfo, TpPriceModel standardPrice,
            DateTime modifyTime, CrmAccountModel userInfo)
        {
            TpTourPlanModel tour = GetTourById(tourInfo.Id);
            TpLineModel line = _lineBiz.GetLineById(tour.LineId);

            //拷贝复制团期页面输入信息
            tour.MixedNum = tourInfo.MixedNum;                              //最小成团人数
            tour.TuiJianType = tourInfo.TuiJianType;                        //推荐类型
            tour.AdditionInfo = tourInfo.AdditionInfo;                      //附加信息
            tour.Remarks = tourInfo.Remarks;                                //备注
            tour.Price = standardPrice != null ? standardPrice.Price : 0;   //标准价报价
                                                                            // tour.FanLi = standardPrice != null ? standardPrice.FanLi : 0;   //标准价返利
            tour.SettlePrice = standardPrice != null ? standardPrice.SettlePrice : 0;   //结算价
            tour.OutDate = tourInfo.OutDate;                                //出发日期
            tour.BookingLastDays = tour.OutDate.AddDays(-(line.MoveUpDays));   //最后预定日期
            tour.ModifiedBy = userInfo.Code;
            tour.ModifiedTime = modifyTime;
            tour.OwnerCode = userInfo.OwnerCode;
            return tour;
        }

        /// <summary>
        /// 保存复制团期
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="seatList"></param>
        /// <returns></returns>
        public int SaveCopyTour(CopyTourVModel vModel, List<BusSeatModel> seatList, CrmAccountModel currentUser)
        {
            DateTime modifiedTime = DateTime.Now;
            var standardPrice = vModel.PriceList.FirstOrDefault(p => p.IsStandard == 1);
            vModel.Tour.OutDate = vModel.OutDate.ToDateTime();//传递出发日期
            var copiedTour = CreateCopiedTour(vModel.Tour, standardPrice, modifiedTime, currentUser);    //复制的团期
            var line = _lineBiz.GetLineByTour(vModel.Tour.Id);

            #region 库存
            var copiedQuota = new QuotaModel
            {
                PlanQuota = vModel.Quota.PlanQuota,                             //计划库存
                UseQuota = vModel.Quota.PlanQuota - vModel.Quota.HoldQuota,     //可用库存
                UsedQuota = vModel.Quota.UsedQuota,                             //已用库存
                HoldQuota = vModel.Quota.HoldQuota,                             //预留库存
                Source = 1,                                                     //来源：标准团
                TrafficType = line.TrafficType,                                 //线路类型
                ShareDesc = null,                                               //共享说明
                ModifiedBy = currentUser.Code,
                ModifiedTime = modifiedTime,
                OwnerCode = currentUser.OwnerCode
            };
            #endregion
            #region 座位表
            TpBusSeatModel copiedBusSeat = null;
            if (line.TrafficType == BusInTrafficType && seatList.Count > 0)
            {
                copiedBusSeat = new TpBusSeatModel
                {
                    SeatNum = vModel.Quota.PlanQuota,
                    SeatDetail = JsonSerializer.Serialize(seatList)
                };
            }
            #endregion
            #region 团期库存关联
            var copiedMap = new TourQuotaMapModel
            {
                Max = 0,
                Source = 1
            };
            #endregion
            #region 价格列表

            var priceTypeBeans = DictionaryBiz.GetEnumsBy(Enums.TpPriceTypeEnum);
            var copiedPriceList = (from price in vModel.PriceList
                                   where price.Id >= 0
                                   let priceTypeBean = priceTypeBeans.FirstOrDefault(p => p.Key == price.PriceType.ToString(CultureInfo.InvariantCulture))
                                   select new TpPriceModel
                                   {
                                       PriceType = price.PriceType,
                                       PriceTypeName = priceTypeBean == null ? string.Empty : priceTypeBean.Value,
                                       PriceRemark = price.PriceRemark,
                                       Price = price.Price, //报价
                                       SettlePrice = price.SettlePrice,
                                       Cost = 0, //成本价（未涉及，置0）
                                       //Tips = vModel.Tips, //小费
                                       //ZiFei = price.ZiFei, //自费
                                       //SingleRoom = vModel.SingleRoom, //单房差
                                       TeJiaFanLi = price.TeJiaFanLi, //特价让利
                                       IsStandard = price.IsStandard, //是否标准价
                                       IsValid = price.IsValid, //是否有效
                                       SuitNum = price.SuitNum, //套餐
                                       ModifiedBy = currentUser.Code,
                                       ModifiedTime = modifiedTime
                                   }).ToList();

            #endregion

            int copiedTourId;
            using (var scope = new TransactionScope())
            {
                copiedTourId = Convert.ToInt32(_dao.Insert(copiedTour));
                int copiedQuotaId = Convert.ToInt32(new QuotaDao().Insert(copiedQuota));

                copiedMap.TourId = copiedTourId;
                copiedMap.QuotaId = copiedQuotaId;
                new TourQuotaMapDao().Insert(copiedMap);

                if (null != copiedBusSeat)
                {
                    copiedBusSeat.QuotaId = copiedQuotaId;
                    new TpBusSeatDao().Insert(copiedBusSeat);
                }

                foreach (var price in copiedPriceList)
                {
                    price.TourId = copiedTourId;
                    new TpPriceDao().Insert(price);
                }

                scope.Complete();
            }
            return copiedTourId;
        }

        #endregion

        #region 加载库存团期相关数据信息.

        /// <summary>
        /// 取得线路开班
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="outDate">日期列表</param>
        /// <returns></returns>
        public List<TpTourPlanModel> GetTourPlanList(string lineId, List<DateTime> outDate)
        {
            var sql = new Sql();
            sql.Append("select * from TpTourPlan where LineId=@0 ", lineId);
            sql.Append("and OutDate in (@0) ", outDate);

            var list = _dao.Query<TpTourPlanModel>(sql.SQL, sql.Arguments).ToList();

            return list;
        }

        #endregion

        #region 查询出境游客名单 团信息

        /// <summary>
        /// 需要修改  //TODO
        /// </summary>
        /// <param name="tourIds"></param>
        /// <returns></returns>
        public List<OutBandHeadModel> GetTourOutBandHeadInfo(string tourIds)
        {
            var sql = new Sql();
            sql.Append(" select t.LineId, t.OutDate, line.LineName, t.Id as TourId, t.EntryDate, ");
            sql.Append("  (select Name from  BaseDestination where Id=t.PortOfEntry) as PortOfEntry ,");
            sql.Append("  (select Name from  BaseDestination where Id=t.PortOfExit) as PortOfExit ,");
            sql.Append(" (select count(*) from TpTraveller tp where  tp.TourId=t.Id) as TravellerCount, ");
            sql.Append(" (select count(*) from TpTraveller tp where tp.Sex = '2' and tp.TourId = t.Id) as WomenCount,");
            sql.Append("(select count(*) from TpTraveller tp where tp.Sex = '1' and tp.TourId = t.Id ) as ManCount , ");
            sql.Append("(select count(*) from TpTraveller tp where tp.IsLeader =1 and tp.TourId = t.Id ) as LeaderCount ");
            sql.Append(" from TpTourPlan t left join TpLine line on t.LineId = line.LineId where t.Id in (" + tourIds + ")");

            return _dao.Query<OutBandHeadModel>(sql.SQL, sql.Arguments).ToList<OutBandHeadModel>();
        }

        #endregion
    }
}