using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.VModels.Stat;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Dao.Order;
using Lvy.VModels.Base;
using Lvy.VModels.Finance;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Finance
{
    /// <summary>
    ///
    /// </summary>
    public class FinanceBiz : BaseBiz
    {

        private readonly TpOrderDao _ordersDao = new TpOrderDao();


        #region 财务收款

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="financeVModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用者：财务收款 Finance/SearchFinance
        /// </remarks>
        public PagedList<TpOrderModel> GetPageList(FinanceVModel financeVModel, CrmAccountModel currentUser)
        {
            var sql = CreateFinanceSql(financeVModel, currentUser, 1);     // 支持分销商、供应商使用
            if (sql != null)
                return _ordersDao.Pager(financeVModel.OrderModels.PageIndex, financeVModel.OrderModels.PageSize, sql.SQL, sql.Arguments);
            return new PagedList<TpOrderModel> { Items = new List<TpOrderModel>() };
        }

        /// <summary>
        /// 导出对账单
        /// </summary>
        /// <param name="financeVModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用者：财务收款 Finance/ExportExcel
        /// </remarks>
        public List<TpOrderModel> ExportDuiZhangDan(FinanceVModel financeVModel, CrmAccountModel userInfo)
        {
            var sql = CreateFinanceSql(financeVModel, userInfo, 1);       // 支持分销商、供应商使用
            if (sql != null)
                return _ordersDao.Fetch(sql.SQL, sql.Arguments);
            return new List<TpOrderModel>();
        }

        /// <summary>
        /// 获取预定统计订单列表  【商户可用】
        /// </summary>
        /// <param name="financeVModel"></param>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用者：团队游 订单统计
        /// </remarks>
        public PagedList<TpOrderModel> GetBAccountPageList(FinanceVModel financeVModel, CrmAccountModel userInfo, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT TpOrder.*, tl.LineName FROM TpOrder
INNER JOIN TpLine tl ON TpOrder.LineId=tl.LineId
WHERE OwnerCode=@0 ", Ansi(ownerCode));
            //if (userInfo.CustomerCode != userInfo.OwnerCode)
            //    sql.Append(@" AND (BookingCustomer=@0 ", Ansi(userInfo.CustomerCode))
            //        .Append(@" OR SupplierCode=@0) ", Ansi(userInfo.CustomerCode));
            var customer = DictionaryBiz.GetCachedCustomer(userInfo.CustomerCode, userInfo.OwnerCode);
            if (customer.IsOwner) { }
            else if (customer.IsSupplier)
            {
                //供应商，获取自己产品的订单
                sql.Append(@" AND SupplierCode=@0 ", Ansi(userInfo.CustomerCode));
            }
            else if (customer.IsDistributors)
            {
                //分销商，不应看到订单统计
                return new PagedList<TpOrderModel> { Items = new List<TpOrderModel>() };
            }

            if (!financeVModel.Condition.LineName.IsNullOrEmpty())
                sql.Append(@" AND tl.LineName LIKE @0 ", AnsiLike(financeVModel.Condition.LineName));
            if (!financeVModel.Condition.StartOutDate.IsNullOrEmpty())
                sql.Append(@" AND OutDate >= @0 ", financeVModel.Condition.StartOutDate.ToDateTime());
            if (!financeVModel.Condition.EndOutDate.IsNullOrEmpty())
                sql.Append(@" AND OutDate <= @0 ", financeVModel.Condition.EndOutDate.ToDateTime());

            if (!financeVModel.Condition.StartCreatedTime.IsNullOrEmpty())
                sql.Append(@" AND CreatedTime >= @0 ", financeVModel.Condition.StartCreatedTime);
            if (!financeVModel.Condition.EndCreatedTime.IsNullOrEmpty())
                sql.Append(@" AND CreatedTime < @0 ", financeVModel.Condition.EndCreatedTime.ToDateTime().AddDays(1).ToString());

            if (!financeVModel.Condition.OrderId.IsNullOrEmpty())
                sql.Append(@" AND Id=@0", financeVModel.Condition.OrderId.ToInt());
            //分销商
            if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
                sql.Append(@" AND BookingCustomer in ( " + financeVModel.Condition.BookingCustomer + " ) ");
            //团号
            int tourId;
            if (!financeVModel.Condition.TourNo.IsNullOrEmpty() && int.TryParse(financeVModel.Condition.TourNo, out tourId))
            {
                sql.Append(@" AND TourId = @0", tourId);
            }
            //线路类型
            if (!financeVModel.Condition.LineType.IsNullOrEmpty())
                sql.Append(@" AND LineId in (  " + financeVModel.Condition.LineType + " )");
            if (!financeVModel.Condition.OrderState.IsNullOrEmpty())
                sql.Append(@" AND OrderState = @0 ", financeVModel.Condition.OrderState.ToInt());
            //结算状态
            if (!financeVModel.Condition.SettlementState.IsNullOrEmpty())
                sql.Append(financeVModel.Condition.SettlementState == "0" ? @" AND OrderState=2 AND JieSuanState<5 " : @" AND OrderState=2 AND JieSuanState=5 ");

            if (!financeVModel.Condition.OrderSource.IsNullOrEmpty())
                sql.Append(@" AND OrderSource=@0 ", financeVModel.Condition.OrderSource.ToInt());
            sql.Append(" order by OrderState,Outdate ");

            var list = _ordersDao.Pager(financeVModel.OrderModels.PageIndex, financeVModel.OrderModels.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        #region 财务收款统计

        /// <summary>
        /// 统计实收总额和销售总额
        /// </summary>
        /// <returns></returns>
        public FinanceVModel SumTolPaid(FinanceVModel financeVModel, CrmAccountModel userInfo, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT IFNULL(SUM(TolPaid),0) AS SumTolPaid,IFNULL(SUM(TolYsPrice),0) AS SumPriceCount
FROM TpOrder INNER JOIN TpLine t ON t.LineId = TpOrder.LineId 
WHERE OwnerCode=@0 ", ownerCode);

            if (!financeVModel.Condition.LineName.IsNullOrEmpty())
                sql.Append(@" AND t.LineName LIKE @0 ", AnsiLike(financeVModel.Condition.LineName));

            //分销商
            if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
                sql.Append(@" AND BookingCustomer in ( " + financeVModel.Condition.BookingCustomer + " ) ");
            if (userInfo.CustomerCode != userInfo.OwnerCode)
                sql.Append(@" AND (BookingCustomer=@0 ", Ansi(userInfo.CustomerCode))
                    .Append(@" OR SupplierCode=@0) ", Ansi(userInfo.CustomerCode));
            //线路类型
            if (!financeVModel.Condition.LineType.IsNullOrEmpty())
                sql.Append(@" AND LineId in (  " + financeVModel.Condition.LineType + " )");
            if (!financeVModel.Condition.StartOutDate.IsNullOrEmpty())
                sql.Append(@" AND OutDate >= @0 ", financeVModel.Condition.StartOutDate.ToDateTime());
            if (!financeVModel.Condition.EndOutDate.IsNullOrEmpty())
                sql.Append(@" AND OutDate <= @0 ", financeVModel.Condition.EndOutDate.ToDateTime());
            if (!financeVModel.Condition.StartCreatedTime.IsNullOrEmpty())
                sql.Append(@" AND CreatedTime >= @0 ", financeVModel.Condition.StartCreatedTime);
            if (!financeVModel.Condition.EndCreatedTime.IsNullOrEmpty())
                sql.Append(@" AND CreatedTime <= @0 ", financeVModel.Condition.EndCreatedTime);
            if (!financeVModel.Condition.OrderId.IsNullOrEmpty())
                sql.Append(@" AND Id=@0", financeVModel.Condition.OrderId.ToInt());
            //未结算(已确认和已退团)
            if (financeVModel.Condition.OrderState == "11")
                sql.Append(@" AND JieSuanState < 5 ");
            else if (financeVModel.Condition.OrderState == "10")
            {
                sql.Append(@" AND JieSuanState = 5");
            }

            var financeVModels = _ordersDao.Query<FinanceVModel>(sql.SQL, sql.Arguments).FirstOrDefault();
            return financeVModels;
        }

        /// <summary>
        /// 统计出行人数、销售总额和返利总额
        /// </summary>
        /// <param name="financeVModel"></param>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        public FinanceVModel GetSummary(FinanceVModel financeVModel, CrmAccountModel userInfo, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT COUNT(*) AS SumTravellerCount, IFNULL(SUM(a.YsPrice),0) AS SumPriceCount,IFNULL(SUM(a.FanLi),0) AS SumFanLiCount
FROM TpTraveller a
inner join TpOrder b on a.OrderCode = b.OrderCode
inner join TpLine t ON b.LineId=t.LineId
WHERE b.OwnerCode=@0 AND b.TravellerCount!=0 AND a.State=2 ", Ansi(ownerCode));//已确认和已结算订单(不包括已退团的结算)里的游客

            if (!financeVModel.Condition.LineName.IsNullOrEmpty())
                sql.Append(@" AND t.LineName LIKE @0 ", AnsiLike(financeVModel.Condition.LineName));
            //分销商
            if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
                sql.Append(@" AND b.BookingCustomer in ( " + financeVModel.Condition.BookingCustomer + " ) ");
            if (userInfo.CustomerCode != userInfo.OwnerCode)
                sql.Append(@" AND (b.BookingCustomer=@0 ", Ansi(userInfo.CustomerCode))
                    .Append(@" OR b.SupplierCode=@0)  ", Ansi(userInfo.CustomerCode));
            //线路类型
            if (!financeVModel.Condition.LineType.IsNullOrEmpty())
                sql.Append(@" AND b.LineId in (  " + financeVModel.Condition.LineType + " )");
            if (!financeVModel.Condition.StartOutDate.IsNullOrEmpty())
                sql.Append(@" AND b.OutDate >= @0 ", financeVModel.Condition.StartOutDate.ToDateTime());
            if (!financeVModel.Condition.EndOutDate.IsNullOrEmpty())
                sql.Append(@" AND b.OutDate <= @0 ", financeVModel.Condition.EndOutDate.ToDateTime());
            if (!financeVModel.Condition.StartCreatedTime.IsNullOrEmpty())
                sql.Append(@" AND b.CreatedTime >= @0 ", financeVModel.Condition.StartCreatedTime.ToDateFormat());
            if (!financeVModel.Condition.EndCreatedTime.IsNullOrEmpty())
                sql.Append(@" AND b.CreatedTime <= @0 ", financeVModel.Condition.EndCreatedTime.ToDateTime().AddDays(1).ToDateFormat());
            if (!financeVModel.Condition.OrderId.IsNullOrEmpty())
                sql.Append(@" AND b.Id=@0", financeVModel.Condition.OrderId.ToInt());

            //未结算(已确认和已退团)
            if (financeVModel.Condition.OrderState == "11")
                sql.Append(@" AND b.OrderState in( 2 ,9 ) ");
            else if (financeVModel.Condition.OrderState == "10")
                sql.Append(@" AND b.OrderState = @0 ", financeVModel.Condition.OrderState);
            else
                sql.Append(@" AND b.OrderState in( 2,10 )  ");

            return _ordersDao.Query<FinanceVModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        #endregion 财务收款统计

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="queryType">查询种类：1.财务收款列表，2.财务收款应收与已收，3.财务收款游客数与返利</param>
        /// <returns></returns>
        public Sql CreateFinanceSql(FinanceVModel vModel, CrmAccountModel userInfo, int queryType)
        {
            var customerList = new CustomerBiz().GetCustomers(userInfo.CustomerCode);//查询当前客户及其附属客户列表
            if (customerList != null && customerList.Count > 0)
            {
                #region Select

                var sql = new Sql();
                switch (queryType)
                {
                    case 1:
                        sql.Append(@"SELECT A.*,B.Name as SalerName FROM TpOrder A left join CrmAccount B on  A.SalerCode=B.Code ");
                        break;

                    case 2:
                        sql.Append(@"SELECT SUM(TolYsPrice) SumPriceCount,SUM(TolPaid) SumTolPaid FROM TpOrder A");
                        break;

                    case 3:
                        sql.Append(
                        @"SELECT COUNT(B.Id) SumTravellerCount,SUM(B.FanLi) SumFanLiCount FROM TpOrder A INNER JOIN TpTraveller B on a.OrderCode=b.OrderCode and B.State=2 and A.TravellerCount !=0");//已确认和已结算订单(不包括已退团的结算)里的游客
                        break;

                    default:
                        return null;//不存在1/2/3以外参数
                }

                #endregion Select

                #region 条件

                sql.Append(@" INNER JOIN TpLine t ON t.LineId=A.LineId WHERE A.OwnerCode=@0 ", Ansi(userInfo.OwnerCode));
                //若不为平台用户，需添加过滤条件
                if (!userInfo.IsOwnerUser)
                {
                    sql.Append(" AND A.SupplierCode IN (@0)", customerList.Select(t => t.Code).ToArray());
                }
                //线路类型
                if (!vModel.Condition.LineType.IsNullOrEmpty())
                    sql.Append(@" AND t.LineType in ( " + vModel.Condition.LineType + " )");
                //线路名称
                if (!vModel.Condition.LineName.IsNullOrEmpty())
                    sql.Append(@" AND t.LineName LIKE @0 ", AnsiLike(vModel.Condition.LineName));
                //出发日期-起
                if (!vModel.Condition.StartOutDate.IsNullOrEmpty())
                    sql.Append(@" AND A.OutDate >= @0 ", vModel.Condition.StartOutDate.ToDateTime());
                //出发日期-止
                if (!vModel.Condition.EndOutDate.IsNullOrEmpty())
                    sql.Append(@" AND A.OutDate <= @0 ", vModel.Condition.EndOutDate.ToDateTime());
                //收款日期-起
                if (!vModel.Condition.StartPayInTime.IsNullOrEmpty())
                    sql.Append(@" AND A.PayInTime >= @0 ", vModel.Condition.StartPayInTime.ToDateTime());
                //收款日期-止
                if (!vModel.Condition.EndPayInTime.IsNullOrEmpty())
                    sql.Append(@" AND A.PayInTime <= @0 ", vModel.Condition.EndPayInTime.ToDateTime().AddDays(1));
                //分销商
                if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
                    sql.Append(@" AND A.BookingCustomer in ( " + vModel.Condition.BookingCustomer + " ) ");
                //团号
                int tourId;
                if (!vModel.Condition.TourNo.IsNullOrEmpty() && int.TryParse(vModel.Condition.TourNo, out tourId))
                {
                    sql.Append(@" AND A.TourId = @0", tourId);
                }
                //结算状态
                if (!vModel.Condition.SettlementState.IsNullOrEmpty())
                {
                    sql.Append(vModel.Condition.SettlementState == "0" ? @" AND A.JieSuanState<5 " : @" AND A.JieSuanState=5 ");
                }
                else
                {
                    sql.Append(@" AND A.OrderState=2 ");
                }
                //订单编号
                int orderId;
                if (!vModel.Condition.OrderId.IsNullOrEmpty() && int.TryParse(vModel.Condition.OrderId, out orderId))
                    sql.Append(@" AND A.Id=@0", orderId);
                //关联订单号
                if (!vModel.Condition.JoinOrderCode.IsNullOrEmpty())
                    sql.Append(@" AND JoinOrderCode Like @0", AnsiLike(vModel.Condition.JoinOrderCode));

                if (!vModel.Condition.SalerCode.IsNullOrEmpty())
                {
                    sql.Append(" and A.SalerCode=@0 ", vModel.Condition.SalerCode);
                }

                #endregion 条件

                #region 排序

                if (queryType == 1)
                {
                    if (!vModel.Condition.SortKey.IsNullOrEmpty())
                    {
                        var sortBy = vModel.Condition.SortCollection[Convert.ToInt32(vModel.Condition.SortKey)];
                        sql.Append(" ORDER BY ");
                        if (sortBy != null)
                        {
                            sql.Append("A." + sortBy.Key + ", A.OrderState,A.Outdate");
                        }
                        else
                        {
                            sql.Append("A.OrderState,A.Outdate ");
                        }
                    }
                    else
                    {
                        sql.Append(" ORDER BY A.OrderState,A.Outdate ");
                    }
                }

                #endregion 排序

                return sql;
            }
            return null;
        }

        /// <summary>
        /// 财务收款汇总
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        /// <remarks>Ps.实质与GetStatisticSummary方法一样</remarks>
        public FinanceVModel GetFinanceSummary(FinanceVModel vModel, CrmAccountModel userInfo)
        {
            var result = new FinanceVModel();
            var sqlOrder = CreateFinanceSql(vModel,userInfo, 2);      // 支持分销商、供应商使用
            var sqlTraveller = CreateFinanceSql(vModel,userInfo, 3);  // 支持分销商、供应商使用
            var tempVModelOrder = sqlOrder != null
                                      ? _ordersDao.Query<FinanceVModel>(sqlOrder.SQL, sqlOrder.Arguments).FirstOrDefault()
                                      : null;
            var tempVModelTraveller = sqlTraveller != null
                                      ? _ordersDao.Query<FinanceVModel>(sqlTraveller.SQL, sqlTraveller.Arguments).FirstOrDefault()
                                      : null;
            if (tempVModelOrder != null)
            {
                result.SumPriceCount = tempVModelOrder.SumPriceCount;
                result.SumTolPaid = tempVModelOrder.SumTolPaid;
                result.ShengYuCount = result.SumPriceCount - result.SumTolPaid;
            }
            if (tempVModelTraveller != null)
            {
                result.SumTravellerCount = tempVModelTraveller.SumTravellerCount;
            }
            return result;
        }

        #endregion 财务收款

        #region 预定统计分析

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="queryType">查询种类：1.订单统计列表，2.订单统计应收与已收，3.订单统计游客数 4.返利</param>
        /// <returns></returns>
        public Sql GreateOrderStatisticSql(FinanceVModel vModel, CrmAccountModel userInfo, int queryType)
        {
            var customerList = new CustomerBiz().GetCustomers(userInfo.CustomerCode); //查询当前客户及其附属客户列表
            if (customerList != null && customerList.Count > 0)
            {
                var sql = new Sql();
                if (queryType == 1)
                {
                    sql.Append(@"SELECT A.*, L.LineName, c.Name AS SalerName
FROM TpOrder A INNER JOIN TpLine L ON A.LineId = L.LineId
LEFT JOIN CrmAccount c ON A.SalerCode=c.Code ");
                }
                else if (queryType == 2)
                {
                    sql.Append(@"SELECT SUM(TolYsPrice) SumPriceCount,SUM(TolPaid) SumTolPaid, SUM(TravellerCount) AS SumTravellerCount
FROM TpOrder A INNER JOIN TpLine L on A.LineId = L.LineId ");
                }
                else if (queryType == 3)
                {
                    sql.Append(@"SELECT SUM(TolYsPrice) SumPriceCount,SUM(TolPaid) SumTolPaid, SUM(TravellerCount) AS SumTravellerCount
FROM TpOrder A INNER JOIN TpLine L on A.LineId = L.LineId ");
                }

                //                else//4
                //                {
                //                    sql.Append(@"SELECT SUM(B.FanLi) SumFanLiCount
                //FROM TpOrder A INNER JOIN TpLine L on a.LineId = L.LineId
                //INNER JOIN TpTraveller B on a.OrderCode=b.OrderCode AND B.State=2 ");//已确认和已结算订单(不包括已退团的结算)里的游客
                //                }

                #region 条件

                sql.Append(@" WHERE A.OwnerCode=@0 ", Ansi(userInfo.OwnerCode));
                if (queryType == 3)   // 没有取消订单统计游客
                    sql.Append(@" AND A.IsCancel=0 ");

                if (!userInfo.IsOwnerUser)
                {
                    sql.Append(" AND A.SupplierCode IN (@0)", customerList.Select(t => t.Code).ToArray());
                }
                if (!vModel.Condition.CrmTeamId.IsNullOrEmpty())
                    sql.Append(@" AND L.TeamId = @0 ", Ansi(vModel.Condition.CrmTeamId));
                if (!vModel.Condition.LineName.IsNullOrEmpty())
                    sql.Append(@" AND L.LineName LIKE @0 ", AnsiLike(vModel.Condition.LineName));
                if (!vModel.Condition.StartOutDate.IsNullOrEmpty())
                    sql.Append(@" AND A.OutDate >= @0 ", vModel.Condition.StartOutDate.ToDateTime());
                if (!vModel.Condition.EndOutDate.IsNullOrEmpty())
                    sql.Append(@" AND A.OutDate <= @0 ", vModel.Condition.EndOutDate.ToDateTime());
                if (!vModel.Condition.StartCreatedTime.IsNullOrEmpty())
                    sql.Append(@" AND A.CreatedTime >= @0 ", vModel.Condition.StartCreatedTime);
                if (!vModel.Condition.EndCreatedTime.IsNullOrEmpty())
                    sql.Append(@" AND A.CreatedTime < @0 ", vModel.Condition.EndCreatedTime.ToDateTime().AddDays(1).ToString());
                //线路编号
                int lineId;
                if (!vModel.Condition.OrderId.IsNullOrEmpty() && int.TryParse(vModel.Condition.OrderId, out lineId))
                    sql.Append(@" AND A.Id=@0", lineId);
                //团号
                int tourId;
                if (!vModel.Condition.TourNo.IsNullOrEmpty() && int.TryParse(vModel.Condition.TourNo, out tourId))
                {
                    sql.Append(@" AND A.TourId = @0", tourId);
                }
                //分销商
                if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
                    sql.Append(@" AND A.BookingCustomer in ( " + vModel.Condition.BookingCustomer + " ) ");
                //销售组
                if (!vModel.Condition.SaleTeamId.IsNullOrEmpty())
                    sql.Append(@" AND A.SalesTeamID=@0 ", vModel.Condition.SaleTeamId);
                //销售员
                if (!vModel.Condition.SalerCode.IsNullOrEmpty())
                    sql.Append(@" AND A.SalerCode=@0 ", vModel.Condition.SalerCode);
                //线路类型
                if (!vModel.Condition.LineType.IsNullOrEmpty())
                    sql.Append(@" AND L.LineType=@0 ", vModel.Condition.LineType);
                //订单状态
                if (!vModel.Condition.OrderState.IsNullOrEmpty())
                    sql.Append(@" AND A.OrderState = @0 ", vModel.Condition.OrderState.ToInt());
                //结算状态--> 未结算：已确认、已退团；已结算：已结算
                if (!vModel.Condition.SettlementState.IsNullOrEmpty())
                    sql.Append(vModel.Condition.SettlementState == "0" ? @" AND JieSuanState<5" : @" AND JieSuanState=5");
                //订单来源
                if (!vModel.Condition.OrderSource.IsNullOrEmpty())
                    sql.Append(@" AND A.OrderSource=@0 ", vModel.Condition.OrderSource.ToInt());

                #endregion 条件

                if (queryType == 1)
                {
                    sql.Append(" ORDER BY A.OrderState,A.Outdate ");
                }
                return sql;
            }
            return null;
        }

        /// <summary>
        /// 获取订单统计列表
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="needPager"></param>
        /// <returns></returns>
        /// <remarks>调用位置：预定统计(/Finance/SearchBookAccount),导出预订单(/Finance/DownloadBookingOrder)</remarks>
        public PagedList<TpOrderModel> GetOrderStatistic(FinanceVModel vModel, CrmAccountModel userInfo, bool needPager)
        {
            var sql = GreateOrderStatisticSql(vModel, userInfo, 1);
            if (sql != null)
            {
                if (needPager)
                    return _ordersDao.Pager(vModel.OrderModels.PageIndex, vModel.OrderModels.PageSize, sql.SQL, sql.Arguments);
                else
                    return new PagedList<TpOrderModel> { Items = _ordersDao.Fetch(sql.SQL, sql.Arguments) };
            }
            return new PagedList<TpOrderModel>() { Items = new List<TpOrderModel>() };
        }

        /// <summary>
        /// 获取汇总
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用位置：预定统计(/Finance/SearchBookAccount)
        /// </remarks>
        public FinanceVModel GetStatisticSummary(FinanceVModel vModel, CrmAccountModel userInfo)
        {
            var result = new FinanceVModel();
            var sqlOrder = GreateOrderStatisticSql(vModel, userInfo, 2);
            var sqlTraveller = GreateOrderStatisticSql(vModel, userInfo, 3);
            var tempVModelOrder = sqlOrder != null
                                      ? _ordersDao.Query<FinanceVModel>(sqlOrder.SQL, sqlOrder.Arguments).FirstOrDefault()
                                      : null;
            var tempVModelTraveller = sqlTraveller != null
                                      ? _ordersDao.Query<FinanceVModel>(sqlTraveller.SQL, sqlTraveller.Arguments).FirstOrDefault()
                                      : null;
            if (tempVModelOrder != null)
            {
                result.SumPriceCount = tempVModelOrder.SumPriceCount;
                result.SumTolPaid = tempVModelOrder.SumTolPaid;
                result.ShengYuCount = result.SumPriceCount - result.SumTolPaid;
            }
            if (tempVModelTraveller != null)
            {
                result.SumTravellerCount = tempVModelTraveller.SumTravellerCount;
            }
            return result;
        }

        #endregion 预定统计分析


        /// <summary>
        /// 需要修改  //TODO
        /// </summary>
        /// <returns></returns>
        public StatItemVModel GetWaiDaiBanInfo( CrmAccountModel UserInfo, IList<SysRoleModel> userRole, IList<CrmTeamModel> userTeams)
        {
            var sql = new Sql();
            sql.Append(" select ");

            // 近七天订单量
            sql.Append(" (SELECT COUNT(*) FROM tporder WHERE OwnerCode=@0 AND IsCancel=0 AND CreatedTime > DATE_SUB(curdate(),INTERVAL 7 DAY)) AS OrdersInRecentDays, ", UserInfo.OwnerCode);
            // 近七天下单游客数量
            sql.Append(" (SELECT SUM(TravellerCount) FROM tporder WHERE OwnerCode=@0 AND IsCancel=0 AND CreatedTime > DATE_SUB(curdate(),INTERVAL 7 DAY)) AS TouristsInRecentDays, ", UserInfo.OwnerCode);
            // 近七天下单应收
            sql.Append(" (SELECT SUM(TolYsPrice) FROM tporder WHERE OwnerCode=@0 AND IsCancel=0 AND CreatedTime > DATE_SUB(curdate(),INTERVAL 7 DAY)) AS AmountInRecentDays, ", UserInfo.OwnerCode);
            // 近七日新增客户数
            sql.Append(" (SELECT COUNT(*) FROM crmcustomer WHERE OwnerCode=@0 AND CreatedTime > DATE_SUB(curdate(),INTERVAL 7 DAY)) AS NewCutomerRecentDays, ", UserInfo.OwnerCode);

            if (userRole.Any(role => role.Name == "销售"))
            {
                // 待确认订单
                sql.Append(" (SELECT COUNT(*) FROM TpOrder WHERE OrderState=1 and OwnerCode=@0 and SalerCode=@1) AS XiaDanCount, ", UserInfo.OwnerCode, UserInfo.Code);
                // 未结清订单
                sql.Append(" (SELECT COUNT(*) FROM TpOrder WHERE OrderState=2 and OwnerCode=@0 and SalerCode=@1 and TolYsPrice> TolPaid ) as JiaoKuanCount, ", UserInfo.OwnerCode, UserInfo.Code);
                // 客户总数
                sql.Append(" (SELECT COUNT(*) FROM CrmCustomer WHERE IsValid=1 and OwnerCode=@0 and SalerCode=@1) as CustomerCount, ", UserInfo.OwnerCode, UserInfo.Code);
                //待审核客户数量
                sql.Append(" (SELECT COUNT(*) FROM CrmCustomer WHERE CustomerState=0 and IsValid=1 and OwnerCode=@0 and SalerCode=@1) as WaitAuditCustomerCount ", UserInfo.OwnerCode, UserInfo.Code);
            }
            else if (userRole.Any(role => role.Name == "计调"))
            {
                string[] teams = userTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).Select(t => t.TeamID).ToArray();

                // 已占位 未结算订单
                sql.Append(" (SELECT COUNT(*) FROM TpOrder A inner join TpLine B on B.LineId=A.LineId WHERE A.OwnerCode=@0 and A.OrderState=2 and A.JieSuanState=4 and B.TeamId in (@1)) as UnbalancedOrderCount, ", UserInfo.OwnerCode, teams);
                // 待确认订单
                sql.Append(" (SELECT COUNT(*) FROM TpOrder A inner join TpLine B on B.LineId=A.LineId WHERE A.OwnerCode=@0 and A.OrderState=1 and A.TraceState>40 and B.TeamId in (@1)) as QueRenDingWeiCount, ", UserInfo.OwnerCode, teams);
                // 未输入订单
                sql.Append(" (SELECT COUNT(*) FROM TpOrder A inner join TpLine B on B.LineId=A.LineId WHERE A.OwnerCode=@0 and A.OrderState=1 and A.TraceState<40 and B.TeamId in (@1)) as WaitInputOrderCount ", UserInfo.OwnerCode, teams);
            }
            else if (userRole.Any(role => role.Name == "财务总监"))
            {
                // 未收款账单
                sql.Append(" (SELECT COUNT(*) FROM TpOrderPayIn P INNER JOIN TpOrder A ON A.OrderCode=P.OrderCode WHERE A.OwnerCode=@0 and P.state=0) as AuditPayInCount ", UserInfo.OwnerCode);
            }
            else
            {
                sql.Append(" 0 ");
            }

            return _ordersDao.Query<StatItemVModel>(sql.SQL.TrimEnd(','), sql.Arguments).FirstOrDefault();
        }

        /// <summary>
        /// 按部门统计未审核客户量
        /// </summary>
        /// <returns></returns>
        public List<TeamStatModel> GetAuditCustomer(string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" select CrmCustomer.TeamID, CrmTeam.TeamName, COUNT(*) Amount from CrmCustomer
 inner join CrmTeam on CrmCustomer.TeamID=CrmTeam.TeamID
 where CrmCustomer.OwnerCode=@0 and CustomerState=0
group by CrmCustomer.TeamID, CrmTeam.TeamName ", ownerCode);

            return _ordersDao.Query<TeamStatModel>(sql.SQL, sql.Arguments).ToList();
        }


        public List<TimeStatModel> GetOrderStat(string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT DATE_FORMAT(CreatedTime,'%Y-%m-%d') AS days, COUNT( id ) AS amount
FROM `TpOrder` WHERE OwnerCode=@0 AND CreatedTime > DATE_SUB(curdate(),INTERVAL 30 DAY) GROUP BY days ", ownerCode);

            return _ordersDao.Query<TimeStatModel>(sql.SQL, sql.Arguments).ToList();
        }


        /// <summary>
        /// 后30天库存
        /// </summary>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        public List<TimeStatModel> GetPlanStat(string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT DATE_FORMAT(ttp.outdate,'%Y-%m-%d') AS days, SUM(tq.PlanQuota) amount, SUM(tq.UsedQuota) amount2
FROM tptourplan ttp
  INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=ttp.Id
  INNER JOIN tpquota tq ON TpTourQuotaMap.QuotaId=tq.Id
WHERE ttp.OwnerCode=@0 AND ttp.OutDate> CURDATE() AND ttp.OutDate < DATE_SUB(curdate(),INTERVAL -30 DAY)
GROUP BY days ", ownerCode);

            return _ordersDao.Query<TimeStatModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 近30天销售业绩统计
        /// </summary>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        public List<TimeStatModel> GetSalerStat(string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT ca.Name, COUNT(*) FROM tporder o
INNER JOIN crmaccount ca ON o.SalerCode=ca.Code 
 WHERE o.OwnerCode=@0 AND AND o.CreatedTime > DATE_SUB(curdate(),INTERVAL 30 DAY) GROUP BY o.SalerCode", ownerCode);

            return _ordersDao.Query<TimeStatModel>(sql.SQL, sql.Arguments).ToList();

        }


    }
}