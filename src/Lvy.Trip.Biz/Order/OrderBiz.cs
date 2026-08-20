using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Booking;
using Lvy.VModels.Order;
using Lvy.VModels.Saler;
using Lvy.VModels.Tour;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Text.Json;
using Lvy.Web.Common;

namespace Lvy.Trip.Biz.Order
{
    /// <summary>
    /// 订单
    /// </summary>
    public class OrderBiz : BaseBiz
    {
        private TpOrderDao _dao = new TpOrderDao();
        private TpLineDao _lineDao = new TpLineDao();
        private TpLineBusPointDao _lineBusPointDao = new TpLineBusPointDao();
        private TpTravellerDao _travellerDao = new TpTravellerDao();
        private TpOrderFileDao _fileDao = new TpOrderFileDao();
        private TpInvoiceDao _invoiceDao = new TpInvoiceDao();
        private TpOrderPayInDao _payInDao = new TpOrderPayInDao();
        private TpChildOrderDao _childDao = new TpChildOrderDao();

        private TpOrderPayInBiz _payInBiz = new TpOrderPayInBiz();
        private CustomerBiz _customerBiz = new CustomerBiz();
        private TpTourPlanBiz _planBiz = new TpTourPlanBiz();
        private TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private TourBalanceBiz tourBalanceBiz = new TourBalanceBiz();
        private TpChildOrderBiz _childBiz = new TpChildOrderBiz();

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(OrderBiz));

        #region 订单

        /// <summary>
        /// 根据LineId获取有效订单
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpOrderModel> GetValidOrderByLineId(int lineId)
        {
            var sql = new Sql();
            sql.Append(@"SELECT t.* FROM TpOrder t INNER JOIN TpTourPlan tp ON t.tourId=tp.Id WHERE tp.LineId=@0 AND t.IsCancel=0", lineId);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据TourId获取订单
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<TpOrderModel> GetOrderByTourId(int tourId)
        {
            var sql = new Sql();
            sql.Append(@"SELECT t.*, c.Name as CustomerName, tl.LineName
FROM TpOrder t
inner join TpLine tl on t.LineId = tl.LineId
left join CrmCustomer c on t.BookingCustomer=c.Code
WHERE t.tourId=@0 order by t.BookingCustomer", tourId);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public List<TpOrderModel> GetOrderByQuotaId(int quotaId)
        {
            var sql = new Sql();
            sql.Append(@"SELECT t.*, c.Name as CustomerName, tl.LineName
FROM TpOrder t
inner join TpLine tl on t.LineId = tl.LineId
inner join TpTourQuotaMap ttq on t.TourId = ttq.TourId
left join CrmCustomer c on t.BookingCustomer=c.Code
WHERE ttq.QuotaId=@0 order by t.BookingCustomer", quotaId);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据TourId获取有效订单
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<TpOrderModel> GetValidOrderByTourId(int tourId)
        {
            var sql = new Sql();
            sql.Append(@"SELECT t.*, c.Name as CustomerName
FROM TpOrder t
left join CrmCustomer c on t.BookingCustomer=c.Code
WHERE t.tourId=@0 AND ((t.OrderState=2 AND t.IsCancel=0) OR t.IsCancel = 2) ", tourId);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 已确认订单 和 取消但有费用
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<CommonOrderModel> GetValidCommonOrderByTourId(int tourId)
        {
            var sql = new Sql();
            sql.Append(@"SELECT t.OrderCode, t.LineId AS ProductId, l.LineName AS ProductName,
 t.Managers ContactName, t.ManagerPhone ContactPhone, t.TolYsPrice,
 t.TourId, t.TravellerCount, t.BookingCustomer AgentCode, c.Name AgentName,
 t.JieSuanState, t.SalesTeamID, t.SalerCode, ca.Name AS SalerName
FROM TpOrder t
INNER JOIN tpline l ON t.lineId = l.LineId
LEFT JOIN CrmCustomer c ON t.BookingCustomer=c.Code
LEFT JOIN crmaccount ca ON t.SalerCode=ca.Code
WHERE ((t.OrderState=2 AND t.IsCancel=0) OR t.IsCancel = 2) AND t.tourId=@0 ", tourId);
            return _dao.Query<CommonOrderModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///  获取一个订单的对象 线路信息和游客列表
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public TpOrderModel GetOrderLineTourist(string orderCode)
        {
            var order = _dao.GetOrder(orderCode);
            order.Line = _lineDao.GetByLineId(order.LineId);
            order.TravellerModels = GetTravellersByOrderCode(order.OrderCode);
            return order;
        }

        /// <summary>
        /// 根据上车点基础数据编号获取上车点列表集合
        /// </summary>
        /// <param name="lineId">线路Id</param>
        /// <returns></returns>
        public List<TpLineBusPointModel> GetLineBusPointByLineId(string lineId)
        {
            string sql = @" SELECT * FROM TpLineBusPoint WHERE LineId=@0 ORDER BY ModifiedTime DESC ";
            return _lineBusPointDao.Fetch(sql, lineId);
        }

        /// <summary>
        /// 根据Id获取上车信息
        /// </summary>
        /// <param name="id">上车点编号</param>
        /// <returns></returns>
        public TpLineBusPointModel GetLineBusPointModelById(int id)
        {
            return _lineBusPointDao.GetById(id);
        }


        /// <summary>
        /// 根据线路编号获取游客信息
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TpTravellerModel> GetTravellersByOrderCode(string orderCode)
        {
            return _travellerDao.GetAllTravellers(orderCode);
        }

        /// <summary>
        /// 根据出发日期和线路类型获取订单列表 【商户可用】
        /// </summary>
        /// <param name="outDate"></param>
        /// <param name="lineIds"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用者：下载接送单 /OpTour/ExportJSExcel
        /// </remarks>
        public List<TpOrderModel> GetByOutDate(string outDate, string lineIds, CrmAccountModel userInfo)
        {
            Sql sql = new Sql();

            sql.Append(" select * from TpOrder where IsCancel=0 and OwnerCode=@0 ", Ansi(userInfo.OwnerCode));

            //if (userInfo.CustomerCode != userInfo.OwnerCode)
            //    sql.Append(@" and (BookingCustomer=@0 ", Ansi(userInfo.CustomerCode))
            //        .Append(@" OR SupplierCode=@0) ", Ansi(userInfo.CustomerCode));
            var customer = DictionaryBiz.GetCachedCustomer(userInfo.CustomerCode, userInfo.OwnerCode);
            if (customer.IsOwner) { }
            if (customer.IsSupplier)
            {
                //供应商仅能获取自己产品订单
                sql.Append(@" and SupplierCode=@0 ", Ansi(userInfo.CustomerCode));
            }
            else if (customer.IsDistributors)
            {
                //若为分销商，理论上无法查看该数据
                return new List<TpOrderModel>();
            }

            if (!outDate.IsNullOrEmpty())
                sql.Append(@" and OutDate=@0 ", Ansi(outDate.ToDateTime().ToString("yyyyMMdd")));
            if (!lineIds.IsNullOrEmpty())
                sql.Append(@" and LineId in ( " + lineIds + " ) ");
            sql.Append(" order by LineBusPointId asc ");

            return _dao.Query<TpOrderModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        ///根据订单Code获取巴士游客信息 且订单状态为已退团（2）或有效（1）
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<BusTravellerVModel> GetBusTrallsersByOrderCode(string orderCode)
        {
            string sql = @" Select PriceContent,Price,FanLi,SingleRoom,JiePrice,SongPrice,ZiFei,TeJiaFanLi,State
,count(*) AS PeopleCount,sum(YsPrice) as GroupYsPrice
from TpTraveller
where ordercode=@0 and State in (1,2)
group by PriceContent,Price,FanLi,SingleRoom,JiePrice,SongPrice,ZiFei,TeJiaFanLi,State";

            return _travellerDao.Query<BusTravellerVModel>(sql, orderCode).ToList();
        }

        /// <summary>
        /// 根据TourId获取订单信息列表
        /// </summary>
        /// <param name="tourId">团编号</param>
        /// <returns></returns>
        public List<TpOrderModel> GetOrdersByTourId(int tourId)
        {
            return _dao.GetOrders(tourId);
        }

        ///// <summary>
        ///// 获取订单列表信息
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// 调用者：1. 财务导出对账单
        ///// </remarks>
        //public List<TpOrderModel> GetOrderList(FinanceVModel financeVModel, string ownerCode)
        //{
        //    //var sql = new Sql();
        //    //var userInfo = GlobalContext.Current.UserInfo;
        //    //sql.Append(@" SELECT * FROM TpOrder ")
        //    //     .Append(" where IsCancel=0 and OwnerCode=@0 ", Ansi(GlobalContext.Current.OwnerCode));
        //    //var customer = DictionaryTools.GetCachedCustomer(userInfo.CustomerCode);
        //    //if (customer == 1)
        //    //{
        //    //    //若为供应商，仅能看到自己的产品订单
        //    //    sql.Append(@" and SupplierCode=@0 ", Ansi(userInfo.CustomerCode));
        //    //}
        //    //else if (customer == 2)
        //    //{
        //    //    //理论上分销商无权使用财务模块的到处对账单功能
        //    //    return new List<TpOrderModel>();
        //    //}

        //    //if (!financeVModel.Condition.LineName.IsNullOrEmpty())
        //    //    sql.Append(@" AND LineName LIKE @0 ", AnsiLike(financeVModel.Condition.LineName));
        //    ////分销商
        //    //if (!financeVModel.Condition.BookingCustomer.IsNullOrEmpty())
        //    //    sql.Append(@" AND BookingCustomer in (  " + financeVModel.Condition.BookingCustomer + " )");
        //    ////线路类型
        //    //if (!financeVModel.Condition.LineType.IsNullOrEmpty())
        //    //    sql.Append(@" AND LineId in (  " + financeVModel.Condition.LineType + " )");

        //    //if (!financeVModel.Condition.StartOutDate.IsNullOrEmpty())
        //    //    sql.Append(@" AND OutDate >= @0 ", financeVModel.Condition.StartOutDate.ToDateTime());
        //    //if (!financeVModel.Condition.EndOutDate.IsNullOrEmpty())
        //    //    sql.Append(@" AND OutDate <= @0 ", financeVModel.Condition.EndOutDate.ToDateTime());
        //    //if (!financeVModel.Condition.StartCreatedTime.IsNullOrEmpty())
        //    //    sql.Append(@" AND CreatedTime >= @0 ", financeVModel.Condition.StartCreatedTime.ToDateTime());
        //    //if (!financeVModel.Condition.EndCreatedTime.IsNullOrEmpty())
        //    //    sql.Append(@" AND CreatedTime <= @0 ", financeVModel.Condition.EndCreatedTime.ToDateTime());
        //    //if (!financeVModel.Condition.OrderId.IsNullOrEmpty())
        //    //    sql.Append(@" AND Id=@0", financeVModel.Condition.OrderId.ToInt());
        //    ////团号
        //    //int tourId;
        //    //if (!financeVModel.Condition.TourId.IsNullOrEmpty() && int.TryParse(financeVModel.Condition.TourId, out tourId))
        //    //{
        //    //    sql.Append(@" AND TourId = @0", tourId);
        //    //}
        //    ////未结算(已确认和已退团)
        //    //if (financeVModel.Condition.OrderState == "11")
        //    //    sql.Append(@" AND OrderState in( 2 ,9 ) ");
        //    //else if (financeVModel.Condition.OrderState == "10")
        //    //    sql.Append(@" AND OrderState = @0 ", financeVModel.Condition.OrderState);
        //    //else
        //    //    sql.Append(@" AND OrderState in( 2,9,10 )  ");

        //    //sql.Append(" order by OutDate DESC  ");
        //    var sql = new FinanceBiz().CreateFinanceSql(financeVModel);
        //    return _ordersDao.Query<TpOrderModel>(sql.SQL, sql.Arguments).ToList();

        //}

        public int UpdateLineBusPoint(TpOrderModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        ///  查询分销商订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// 我的订单->团队游订单
        /// 数据项内容为：当前登录客户及其附属客户所预定的订单。
        /// 即便是平台客户，对于我的订单，也仅仅可以查询到自己所预定的订单
        /// </remarks>
        public PagedList<TpOrderModel> GetPageListBySaler(TpOrderVModel vModel, CrmAccountModel userInfo)
        {
            if (userInfo.CustomerCode.IsNullOrEmpty())
                return new PagedList<TpOrderModel> { Items = new List<TpOrderModel>() };
            var customerList = new CustomerBiz().GetCustomers(userInfo.CustomerCode);   //查询当前客户及其附属客户列表
            if (customerList == null || customerList.Count == 0)
                return new PagedList<TpOrderModel> { Items = new List<TpOrderModel>() };

            var sql = new Sql();
            sql.Append(@" SELECT a.* FROM TpOrder a 
                 inner join TpLine b on a.LineId=b.LineId
                WHERE a.OwnerCode =@0 ", userInfo.OwnerCode);

            sql.Append(" AND a.BookingCustomer IN (@0)", customerList.Select(t => t.Code).ToArray());

            if (!vModel.LineName.IsNullOrEmpty())
                sql.Append(@" AND b.LineName like @0 ", AnsiLike(vModel.LineName));
            if (!vModel.OrderId.IsNullOrEmpty())
                sql.Append(@" AND a.OrderCode=@0 ", vModel.OrderId);
            //分销商
            if (!vModel.CustomerName.IsNullOrEmpty())
                sql.Append(@" AND a.BookingCustomer in (  " + vModel.CustomerName + " )");
            //线路类型
            if (!vModel.LineType.IsNullOrEmpty())
                sql.Append(@" AND b.LineType =@0", vModel.LineType.ToInt());
            if (!vModel.OrderState.IsNullOrEmpty())
                sql.Append(@" AND a.OrderState=@0 ", vModel.OrderState.ToInt());
            if (!vModel.OrderSource.IsNullOrEmpty())
                sql.Append(@" AND a.OrderSource=@0 ", vModel.OrderSource.ToInt());
            if (!vModel.JoinOrderCode.IsNullOrEmpty())
                sql.Append(" AND a.JoinOrderCode like @0", AnsiLike(vModel.JoinOrderCode));
            if (!vModel.OutDateRange.IsNullOrEmpty())
            {
                var t = vModel.OutDateRange.Split('-');
                sql.Append(@" AND a.OutDate>=@0 AND a.OutDate<@1", t[0].ToDateTime(), t[1].ToDateTime().AddDays(1));
            }

            if (!vModel.CreatedRange.IsNullOrEmpty())
            {
                var t = vModel.CreatedRange.Split('-');
                sql.Append(@" AND a.CreatedTime>=@0 AND a.CreatedTime<@1 ", t[0].Trim(), t[1].ToDateTime().AddDays(1));
            }


            sql.Append(" order by a.OrderState , a.OutDate ");

            var list = _dao.Pager(vModel.PagedList.PageIndex, vModel.PagedList.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        public List<TpOrderModel> GetListBySaler(TpOrderVModel vModel, CrmAccountModel userInfo)
        {
            if (userInfo.CustomerCode.IsNullOrEmpty())
                return new List<TpOrderModel>();
            var customerList = new CustomerBiz().GetCustomers(userInfo.CustomerCode);   //查询当前客户及其附属客户列表
            if (customerList == null || customerList.Count == 0)
                return new List<TpOrderModel>();

            var sql = new Sql();
            sql.Append(@" SELECT a.* FROM TpOrder a 
                 inner join TpLine b on a.LineId=b.LineId
                WHERE a.OwnerCode=@0 and a.OrderState=2 and a.IsCancel=0 ", userInfo.OwnerCode);

            sql.Append(" AND a.BookingCustomer IN (@0)", customerList.Select(t => t.Code).ToArray());

            if (!vModel.LineName.IsNullOrEmpty())
                sql.Append(@" AND b.LineName like @0 ", AnsiLike(vModel.LineName));
            if (!vModel.OrderId.IsNullOrEmpty())
                sql.Append(@" AND a.OrderCode=@0 ", vModel.OrderId);
            //分销商
            if (!vModel.CustomerName.IsNullOrEmpty())
                sql.Append(@" AND a.BookingCustomer in (  " + vModel.CustomerName + " )");
            //线路类型
            if (!vModel.LineType.IsNullOrEmpty())
                sql.Append(@" AND b.LineType =@0", vModel.LineType.ToInt());
            if (!vModel.OrderSource.IsNullOrEmpty())
                sql.Append(@" AND a.OrderSource=@0 ", vModel.OrderSource.ToInt());
            if (!vModel.JoinOrderCode.IsNullOrEmpty())
                sql.Append(" AND a.JoinOrderCode like @0", AnsiLike(vModel.JoinOrderCode));
            if (!vModel.OutDateRange.IsNullOrEmpty())
            {
                var t = vModel.OutDateRange.Split('-');
                sql.Append(@" AND a.OutDate>=@0 AND a.OutDate<@1", t[0].ToDateTime(), t[1].ToDateTime().AddDays(1));
            }

            if (!vModel.CreatedRange.IsNullOrEmpty())
            {
                var t = vModel.CreatedRange.Split('-');
                sql.Append(@" AND a.CreatedTime>=@0 AND a.CreatedTime<@1 ", t[0].Trim(), t[1].ToDateTime().AddDays(1));
            }


            sql.Append(" order by a.OrderState , a.OutDate ");

            var list = _dao.Fetch(sql.SQL, sql.Arguments);

            return list;
        }

        /// <summary>
        /// 获取订单集合  【商户可用】
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用者：团队游 订单管理->订单查询
        /// </remarks>
        public TpOrderVModel GetPageList(TpOrderVModel vModel, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            var sql1 = new Sql();

            sql.Append(@" SELECT a.*, b.LineName, c.Name AS Manager, cc.Name AS CustomerName
FROM TpOrder a 
INNER JOIN TpLine b ON a.LineId=b.LineId
LEFT JOIN CrmAccount c ON a.ContactCode=c.Code 
LEFT JOIN CrmCustomer cc ON a.BookingCustomer=cc.Code
WHERE a.OwnerCode=@0 ", userInfo.OwnerCode);

            sql1.Append(@" SELECT SUM(CASE WHEN a.IsCancel=0 THEN a.TravellerCount ELSE 0 END) AS PaxSum,
SUM(CASE WHEN a.OrderState=1 THEN a.TravellerCount ELSE 0 END) AS HoldPax, 
SUM(CASE WHEN a.OrderState=2 and a.IsCancel=0 THEN a.TravellerCount ELSE 0 END) as ConfirmPax, 
SUM(a.TolYsPrice) AS AmountSum, 
SUM(a.TolYsPrice - a.TolPaid) AS PaidSum FROM TpOrder a 
INNER JOIN TpLine b ON a.LineId=b.LineId
WHERE a.OwnerCode=@0 ", userInfo.OwnerCode);

            // 根据当前用户设置范围
            var customer = DictionaryBiz.GetCachedCustomer(userInfo.CustomerCode, userInfo.OwnerCode);
            if (customer.IsOwner) { }
            else if (customer.IsSupplier)
            {
                //若为供应商，仅能看到自己的产品订单
                sql.Append(@" and a.SupplierCode=@0 ", Ansi(userInfo.CustomerCode));
            }
            else if (customer.IsDistributors)
            {
                //分销商理论上无法查看订单管理，若分配了该权限，则不显示数据
                //return new PagedList<TpOrderModel> { Items = new List<TpOrderModel>() };
                vModel.CustomerCode = customer.Code;
            }

            if (!vModel.LineName.IsNullOrEmpty())
            {
                sql.Append(@" AND b.LineName like @0 ", AnsiLike(vModel.LineName));
                sql1.Append(@" AND b.LineName like @0 ", AnsiLike(vModel.LineName));
            }
            if (!vModel.OrderId.IsNullOrEmpty())
            {
                sql.Append(@" AND a.OrderCode=@0 ", vModel.OrderId);
                sql1.Append(@" AND a.OrderCode=@0 ", vModel.OrderId);
            }
            //分销商
            if (!vModel.CustomerCode.IsNullOrEmpty())
            {
                sql.Append(@" AND a.BookingCustomer=@0", vModel.CustomerCode);
                sql1.Append(@" AND a.BookingCustomer=@0", vModel.CustomerCode);
            }
            // 销售
            if (!vModel.SalerCode.IsNullOrEmpty())
            {
                sql.Append(@" AND a.SalerCode=@0  ", vModel.SalerCode);
                sql1.Append(@" AND a.SalerCode=@0  ", vModel.SalerCode);
            }

            //线路类型
            if (!vModel.LineScope.IsNullOrEmpty())
            {
                sql.Append(@" AND b.LineScope=@0 ", vModel.LineScope);
                sql1.Append(@" AND b.LineScope=@0 ", vModel.LineScope);
            }
            if (!vModel.LineType.IsNullOrEmpty())
            {
                sql.Append(@" AND b.LineType=@0 ", vModel.LineType);
                sql1.Append(@" AND b.LineType=@0 ", vModel.LineType);
            }
            if (!vModel.OrderState.IsNullOrEmpty())
            {
                sql.Append(@" AND a.OrderState=@0 ", vModel.OrderState.ToInt());
                sql1.Append(@" AND a.OrderState=@0 ", vModel.OrderState.ToInt());
            }
            if (!vModel.OrderSource.IsNullOrEmpty())
            {
                sql.Append(@" AND a.OrderSource=@0 ", vModel.OrderSource.ToInt());
                sql1.Append(@" AND a.OrderSource=@0 ", vModel.OrderSource.ToInt());
            }
            if (!vModel.JoinOrderCode.IsNullOrEmpty())
            {
                sql.Append(" AND a.JoinOrderCode like @0", AnsiLike(vModel.JoinOrderCode));
                sql1.Append(" AND a.JoinOrderCode like @0", AnsiLike(vModel.JoinOrderCode));
            }

            if (!vModel.OutDateRange.IsNullOrEmpty())
            {
                var t = vModel.OutDateRange.Split('-');
                sql.Append(@" AND a.OutDate>=@0 AND a.OutDate<@1 ", t[0].ToDateTime(), t[1].ToDateTime().AddDays(1));
                sql1.Append(@" AND a.OutDate>=@0 AND a.OutDate<@1 ", t[0].ToDateTime(), t[1].ToDateTime().AddDays(1));
            }


            if (!vModel.CreatedRange.IsNullOrEmpty())
            {
                var t = vModel.CreatedRange.Split('-');
                sql.Append(@" AND a.CreatedTime>=@0 AND a.CreatedTime<@1", t[0].Trim(), t[1].ToDateTime().AddDays(1));
                sql1.Append(@" AND a.CreatedTime>=@0  AND a.CreatedTime<@1", t[0].Trim(), t[1].ToDateTime().AddDays(1));
            }

            int tourId;
            if (!vModel.TourId.IsNullOrEmpty() && int.TryParse(vModel.TourId, out tourId))
            {
                sql.Append(@" AND a.TourId = @0", tourId);
                sql1.Append(@" AND a.TourId = @0", tourId);
            }

            if (!vModel.CrmTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and  b.TeamID=@0  ", vModel.CrmTeamId);
                sql1.Append(@" and  b.TeamID=@0  ", vModel.CrmTeamId);
            }

            if (!vModel.SaleTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and  a.SalesTeamID=@0  ", vModel.SaleTeamId);
                sql1.Append(@" and  a.SalesTeamID=@0  ", vModel.SaleTeamId);
            }

            sql.Append(" order by a.OrderState , a.OutDate ");

            vModel.PagedList = _dao.Pager(vModel.PagedList.PageIndex, vModel.PagedList.PageSize, sql.SQL, sql.Arguments);
            vModel.TotalModel = _dao.Query<OrderTotalModel>(sql1.SQL, sql1.Arguments).FirstOrDefault();

            return vModel;
        }

        public List<TpOrderModel> GetOrderList(TpOrderVModel vModel)
        {
            var sql = new Sql();
            sql.Append(@" SELECT t.* FROM TpOrder t
INNER JOIN TpLine tl ON tl.LineId = t.LineId
WHERE t.OwnerCode=@0 ", vModel.OwnerCode);

            //.Append(@" AND t.OrderState=1 ");
            // 产品部门
            if (!vModel.CrmTeamId.IsNullOrEmpty())
                sql.Append(@" AND tl.TeamID=@0 ", Ansi(vModel.CrmTeamId));
            // 分销商
            if (!vModel.CustomerCode.IsNullOrEmpty())
                sql.Append(@" AND t.BookingCustomer=@0 ", Ansi(vModel.CustomerCode));
            // 销售
            if (!vModel.SalerCode.IsNullOrEmpty())
                sql.Append(@" AND t.SalerCode=@0 ", Ansi(vModel.SalerCode));
            //线路类型
            if (!vModel.LineType.IsNullOrEmpty())
                sql.Append(@" AND t.LineType=@0 ", vModel.LineType);

            sql.Append(" order by t.Outdate DESC ");

            var list = _dao.Query<TpOrderModel>(sql.SQL, sql.Arguments).ToList();
            return list;
        }

        /// <summary>
        /// 获取订单集合
        /// </summary>
        /// <param name="orderCodes"></param>
        /// <returns></returns>
        public List<TpOrderModel> GetOrder(IEnumerable<string> orderCodes, string ownerCode)
        {
            if (orderCodes == null) throw new ArgumentNullException("orderCodes");
            var sql = new Sql();

            sql.Append(" SELECT * FROM TpOrder WHERE OwnerCode=@0 ", ownerCode);
            sql.Append(@" AND OrderCode IN (  " + string.Join(",", orderCodes) + " )");
            sql.Append(" ORDER BY OrderState,Outdate ");

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 获取订单集合
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public List<TpOrderModel> GetOrderById(IEnumerable<string> orderIds, CrmAccountModel userInfo)
        {
            if (orderIds == null) throw new ArgumentNullException("orderIds");
            var sql = new Sql();

            sql.Append(" SELECT * FROM TpOrder WHERE OwnerCode=@0 ", userInfo.OwnerCode);
            sql.Append(@" AND Id IN (  " + string.Join(",", orderIds) + " )");
            sql.Append(" ORDER BY OrderState,Outdate ");

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }


        /// <summary>
        /// 收款（批量）
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="shouKuanInfos"></param>
        public void ShowKuan(TpOrderModel orders, TpOrderPayInModel shouKuanInfos)
        {
            using (var scope = new TransactionScope())
            {
                _dao.Update(orders);   // 更新订单状态 和收款金额
                _payInBiz.AddPayIn(shouKuanInfos);   // 添加缴款记录
                // 更新单团 金额  TODO

                scope.Complete();
            }
        }


        public void ConfirmPay(TpOrderPayInModel payin, string userCode)
        {
            using (var ts = _dao.GetTransaction())
            {
                //获取订单信息 修改订单的付款金额等信息.
                var order = _dao.GetOrder(payin.OrderCode);
                decimal unPaid = order.TolYsPrice - order.TolPaid;  //还剩的待付款的金额.
                if (payin.Amount == unPaid)
                {
                    //完成收款
                    order.TolPaid = order.TolYsPrice;
                    order.JieSuanState = 5;//已结算
                }
                else
                {
                    //部分完成
                    order.TolPaid = order.TolPaid + payin.Amount;
                    order.JieSuanState = 4;//部分结算
                }
                _dao.Update(order);

                _dao.Execute("UPDATE TpOrderPayIn set AuditBy=@1, AuditTime=now(), State=@2 WHERE ID=@0 ", payin.Id, userCode, 20);
                ts.Complete();
            }
        }


        /// <summary>
        /// 更新订单表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(TpOrderModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        /// 通过tourId获取该团的价格类型
        /// </summary>
        /// <returns></returns>
        public List<TpPriceModel> GetPricesByTourId(int tourId)
        {
            return new TpPriceDao().GetValidByTourId(tourId);
        }

        /// <summary>
        /// 获取新订单总数
        /// </summary>
        /// <returns></returns>
        public int GetNewOrderCount(CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@"SELECT COUNT(1) FROM TpOrder WHERE OrderState=1 AND OwnerCode=@0 ", userInfo.OwnerCode);
            if (userInfo.CustomerCode != userInfo.OwnerCode)
                sql.Append(@" AND (BookingCustomer=@0 OR SupplierCode=@1)", Ansi(userInfo.CustomerCode), Ansi(userInfo.CustomerCode));
            return _dao.ExecuteScalar<int>(sql.SQL, sql.Arguments);
        }

        #endregion 订单

        #region 取消订单

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderVModel"></param>
        /// <returns></returns>
        public int CancelOrder(TpOrderModel orderModel, CrmAccountModel userInfo)
        {
            var travellerModel = new TpTravellerModel();
            travellerModel.OrderCode = orderModel.OrderCode;

            var order = _dao.GetOrder(orderModel.OrderCode);
            //修改 修改订单里面的库存和总应收
            order.TolYsPrice = orderModel.TolYsPrice;
            order.InvoiceAmount = orderModel.TolYsPrice;

            //取消无损失费 已取消
            if (orderModel.TolYsPrice.Equals(0m))// !=0的场合、已退团
            {
                order.IsCancel = 1;
                order.OrderState = 1;
                order.CancelState = 90;
            }
            else//取消有损失费 已退团
            {
                order.IsCancel = 2;
                order.OrderState = 1;
                order.CancelState = 20;
            }

            order.Remark = orderModel.Remark;
            order.ModifiedBy = userInfo.Code;
            order.ModifiedTime = DateTime.Now;

            using (var ts = new TransactionScope())
            {
                //1.修改订单里面的总应收、总人数、订单状态和是否取消
                _dao.Update(order);

                // 财务没做的缴款单、发票设置无效，子订单设置无效 //TODO
                _payInDao.Update(" SET IsValid=0 WHERE OrderCode=@0", orderModel.OrderCode);
                _invoiceDao.Update(" SET IsValid=0 WHERE OrderCode=@0", orderModel.OrderCode);
                _childDao.Update(" SET IsValid=0 WHERE OrderCode=@0", orderModel.OrderCode);

                if (order.IsCancel == 2)
                {
                    // 添加账单
                    _payInBiz.AddPayIn(new TpOrderPayInModel
                    {
                        OrderCode = order.OrderCode,
                        Amount = order.TolYsPrice,
                        State = 0,
                        CreatedTime = DateTime.Now,
                        IsValid = 1,

                        Remark = "取消订单产生费用"
                    });
                }

                //根据ordercode获取所有游客信息
                //var travellers = _travellerDao.GetTravellers(travellerModel);
                //foreach (var traveller in travellers)
                //{
                //    //3.如果是汽车班 释放座位
                //    if (!traveller.SeatNum.IsNullOrEmpty())
                //    {
                //        //释放库存
                //        FreeSeat(traveller.SeatNum, traveller.TourId);
                //        //将游客座位号清空
                //        traveller.SeatNum = "";
                //        traveller.YsPrice = 0;
                //        traveller.State = order.OrderState == 5 ? 0 : 1; // 取消订单的场合。  已取消0 或者 已退团1
                //        _travellerDao.Update(traveller);
                //    }
                //}

                //4.更新库存
                FreeQuota(order.TourId, orderModel.OrderCode, userInfo.Code);

                ts.Complete();
            }

            return 1;
        }

        #endregion 取消订单

        #region 恢复订单

        /// <summary>
        /// 恢复订单
        /// </summary>
        /// <param name="orderVModel"></param>
        /// <returns></returns>
        public int RestoreOrder(TpOrderModel orderModel, CrmAccountModel userInfo)
        {
            //根据ordercode获取所有游客信息
            var travellers = _travellerDao.GetAllTravellers(orderModel.OrderCode);
            var order = _dao.GetOrder(orderModel.OrderCode);

            // 余位审查
            var quota = _quotaBiz.GetQuotaByTour(order.TourId);
            if (quota.UseQuota < order.TravellerCount)
                return 0;

            //order.OrderState = 2;
            order.CancelState = 0;
            order.IsCancel = 0;
            order.Remark = orderModel.Remark;
            order.ModifiedBy = userInfo.Code;
            order.ModifiedTime = DateTime.Now;
            order.CreatedTime = DateTime.Now;
            order.EffectiveHour = orderModel.EffectiveHour;

            using (var ts = new TransactionScope())
            {
                decimal amount = 0;

                //重新计算应收
                foreach (var traveller in travellers)
                {
                    //3.如果是汽车班 释放座位
                    //if (!traveller.SeatNum.IsNullOrEmpty())
                    //{
                    //    //释放库存
                    //    FreeSeat(traveller.SeatNum, traveller.TourId);
                    //    //将游客座位号清空
                    //    traveller.SeatNum = "";
                    //    traveller.YsPrice = 0;
                    //    traveller.State = order.OrderState == 5 ? 0 : 1; // 取消订单的场合。  已取消0 或者 已退团1
                    //    _travellerDao.Update(traveller);
                    //}
                    amount += traveller.YsPrice;
                }
                var childAmount = _childBiz.GetTpChildOrderList(orderModel.OrderCode).Sum(t => t.Amount);
                order.InvoiceAmount = amount + childAmount;
                order.TolYsPrice = CalcDiscount(order.LineId, amount + childAmount, order.SettleCustomer, order.TravellerCount);

                //1.修改订单里面的总应收、总人数、订单状态和是否取消
                _dao.Update(order);

                //4.更新库存
                FreeQuota(order.TourId, orderModel.OrderCode, userInfo.Code);

                ts.Complete();
            }

            return 1;
        }

        #endregion 恢复订单

        #region 取消游客

        /// <summary>
        /// 取消游客
        /// </summary>
        /// <returns></returns>
        public int CancelTraveller(OrderEditVModel vModel, CrmAccountModel userInfo)
        {
            var traveller = _travellerDao.GetById(vModel.TravellerId);
            if (traveller.State == 0)  // 重复取消
                return 0;

            var order = _dao.GetOrder(vModel.Order.OrderCode);

            // 调整应收  原始 - 游客应收 + <取消产生费用>
            decimal tolYsPrice = order.InvoiceAmount - traveller.YsPrice + vModel.CancelMoney;
            order.TravellerCount = order.TravellerCount - 1; // 删除一个人 ，库存-1
            order.InvoiceAmount = tolYsPrice;
            order.TolYsPrice = CalcDiscount(order.LineId, tolYsPrice, order.SettleCustomer, order.TravellerCount);
            order.ModifiedBy = userInfo.Code;
            order.ModifiedTime = DateTime.Now;

            traveller.YsPrice = vModel.CancelMoney;
            traveller.ModifiedBy = userInfo.Code;
            traveller.ModifiedTime = DateTime.Now;
            using (var ts = new TransactionScope())
            {
                //1.修改订单里面的总应收和总人数
                _dao.Update(order);

                //取消无损失费 已取消
                if (vModel.CancelMoney.Equals(0m))// !=0的场合、已退团
                    traveller.State = 0;
                else//取消有损失费 已退团
                    traveller.State = 1;

                ////2.如果是汽车班 释放座位
                //if (!traveller.SeatNum.IsNullOrEmpty())
                //    FreeSeat(traveller.SeatNum, traveller.TourId);

                //3.更新库存
                FreeQuota(traveller.TourId, vModel.OwnerCode, userInfo.Code);

                //4.修改游客表里的应收款和座位编号
                //traveller.YsPrice = vModel.CancelMoney;
                traveller.SeatNum = "";

                _travellerDao.Update(traveller);

                ts.Complete();
            }

            return 1;
        }

        /// <summary>
        /// 更新当前库存
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public OrderResultState FreeQuota(int tourId, string orderCode, string userCode)
        {
            var quota = _quotaBiz.GetQuotaByTour(tourId);
            var plan = _planBiz.GetTourByIds(tourId);
            if (quota == null)
                throw new Exception("团号：{0}库存对象为空！".With(tourId));

            List<TpOrderModel> orders = new List<TpOrderModel>();
            // 取得所有订单
            if (quota.Source == 1)
                orders = GetOrderByTourId(tourId);         // 独享机位
            else
                orders = GetOrderByQuotaId(quota.Id);      // 共享机位

            int usedquota = orders.Where(m => m.OrderState == 2 && m.IsCancel == 0).Sum(m => m.TravellerCount);
            int lockquota = orders.Where(m => m.OrderState == 1 && m.IsCancel == 0).Sum(m => m.TravellerCount);
            int count = orders.Where(m => m.OrderState == 2 && m.IsCancel == 0 && m.TourId == tourId).Sum(m => m.TravellerCount);
            int leader = GetLeaderOfTour(tourId).Where(m => m.OrderCode.IsNullOrEmpty()).Count();
            if (count + leader != plan.TravellerCount)
            {
                // 更新团人数
                plan.TravellerCount = count + leader;
                _planBiz.UpdateTourPlan(plan);
            }
            // 领队

            if (quota.Source != 1)
                leader = GetLeaderOfQuota(quota.Id).Where(m => m.OrderCode.IsNullOrEmpty()).Count();

            // 修改
            quota.UseQuota = quota.PlanQuota - quota.HoldQuota - usedquota - lockquota - leader;
            quota.UsedQuota = usedquota + leader;
            quota.UnLockQuota = lockquota;
            quota.ModifiedBy = userCode;
            quota.ModifiedTime = DateTime.Now;
            _quotaBiz.Update(quota);

            if (plan.AuditState > 0 && string.IsNullOrEmpty(orderCode) == false)
                tourBalanceBiz.UpdateBalanceAmount(1, orderCode);

            return OrderResultState.Code100;
        }

        public void CalcAmount(string orderCode)
        {
            //获取订单信息
            var orderModel = GetOrderLineTourist(orderCode);
            // 游客总应收
            var YsPrice = orderModel.TravellerModels.Sum(a => a.YsPrice);
            // 子订单合计
            var childAmount = _childBiz.GetTpChildOrderList(orderCode).Where(t => t.IsCancel == 0).Sum(a => a.Amount);


            //重新计算订单总金额
            orderModel.InvoiceAmount = YsPrice + childAmount;
            orderModel.TolYsPrice = CalcDiscount(orderModel.LineId, orderModel.InvoiceAmount, orderModel.SettleCustomer, orderModel.TravellerCount);

            Update(orderModel);

        }

        /// <summary>
        /// 计算结算客户折扣
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="YsPrice"></param>
        /// <param name="customerCode"></param>
        /// <param name="pax"></param>
        /// <returns></returns>
        private decimal CalcDiscount(string lineId, decimal YsPrice, string customerCode, int pax)
        {
            var res = YsPrice;
            var list = _customerBiz.GetPolicyList(customerCode);
            var line = _lineDao.GetByLineId(lineId);
            // lineDest示例 /8/9/12/
            var policy = GetBestPolicy(list, line.DepartDest);
            if (policy != null)
            {
                if (policy.RebateType == 1)
                {
                    res = YsPrice - policy.Amount * pax;
                }
                else
                {
                    res = res * (100 - policy.Percent) / 100;
                }
            }

            return res;
        }

        /// <summary>
        /// 释放座位
        /// </summary>
        private void FreeSeat(string seatNum, int tourId)
        {
            var model = GetSeatDetails(tourId);
            var seats = model.SeatModels;
            var seat = seats.FirstOrDefault(a => a.No == seatNum);
            if (seat == null)
            {
                //todo: 如果找不到座位号，可能改过汽车型号
                return;
            }
            seat.State = 1;// 未占  释放。
            TpBusSeatDao dao = new TpBusSeatDao();
            var jsonSeats = JsonSerializer.Serialize(seats);
            dao.UpdateSeatDetail(model.QuotaId, jsonSeats);
        }

        #endregion 取消游客

        #region 恢复游客

        /// <summary>
        /// 恢复游客
        /// </summary>
        /// <returns></returns>
        public int RestoreTraveller(OrderEditVModel vModel, CrmAccountModel userInfo)
        {
            var traveller = _travellerDao.GetById(vModel.TravellerId);
            if (traveller.State == 2)  // 重复恢复
                return 0;

            // 余位审查
            var quota = _quotaBiz.GetQuotaByTour(vModel.Order.TourId);
            if (quota.UseQuota < 1)
                return 0;

            var order = _dao.GetOrder(vModel.Order.OrderCode);
            var price = GetPricesByTourId(order.TourId).Where(m => m.Id == traveller.PriceId).FirstOrDefault();

            if (price != null)
                traveller.YsPrice = price.SettlePrice;

            //修改 订单里面的库存和总应收
            decimal tolYsPrice = order.InvoiceAmount + traveller.YsPrice;
            order.TravellerCount = order.TravellerCount + 1; // 删除一个人 ，库存-1
            order.InvoiceAmount = tolYsPrice;
            order.TolYsPrice = CalcDiscount(order.LineId, tolYsPrice, order.SettleCustomer, order.TravellerCount);
            order.ModifiedBy = userInfo.Code;
            order.ModifiedTime = DateTime.Now;

            traveller.ModifiedBy = userInfo.Code;
            traveller.ModifiedTime = DateTime.Now;
            using (var ts = new TransactionScope())
            {
                //1.修改订单里面的总应收和总人数
                _dao.Update(order);

                //取消无损失费 已取消
                if (vModel.CancelMoney.Equals(0m))// !=0的场合、已退团
                    traveller.State = 2;
                else//取消有损失费 已退团
                    traveller.State = 2;

                ////2.如果是汽车班 释放座位
                //if (!traveller.SeatNum.IsNullOrEmpty())
                //    FreeSeat(traveller.SeatNum, traveller.TourId);

                //3.更新库存
                FreeQuota(traveller.TourId, vModel.Order.OrderCode, userInfo.Code);

                //4.修改游客表里的应收款和座位编号
                //traveller.YsPrice = vModel.CancelMoney;
                traveller.SeatNum = "";

                _travellerDao.Update(traveller);

                ts.Complete();
            }

            return 1;
        }

        #endregion 恢复游客

        #region 保存订单

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public OrderResultState SaveOrder(OrderEditVModel vModel, CrmAccountModel userInfo)
        {
            OrderResultState ERROR_STATE;

            // 获取订单游客信息
            var travellers = GetTravellersByOrderCode(vModel.Order.OrderCode);
            vModel.TourPlan = _planBiz.GetTourById(vModel.Order.TourId);
            vModel.Prices = GetPricesByTourId(vModel.Order.TourId);
            var quota = _quotaBiz.GetQuotaByTour(vModel.Order.TourId);
            var order = GetOrderLineTourist(vModel.Order.OrderCode);

            // 是否有新客人添加  更新库存
            var nwes = vModel.Travellers2.Where(t => t.Id == 0).ToList();
            if (nwes.Count() > 0)
            {
                if (quota.UseQuota > nwes.Count())
                {
                    // 订单人数  累计
                    order.TravellerCount = order.TravellerCount + nwes.Count();
                }
                else
                {
                    return OrderResultState.Code110;
                }
            }

            //更新订单信息
            order.JoinOrderCode = vModel.Order.JoinOrderCode;
            order.Remark = vModel.Order.Remark;
            order.TravellerCount = vModel.Travellers2.Count();
            order.InvoiceAmount = vModel.Order.InvoiceAmount;
            order.TolYsPrice = vModel.Order.TolYsPrice;

            //销售和代理商
            order.SalerCode = vModel.Order.SalerCode;
            order.BookingCustomer = vModel.Order.BookingCustomer;
            order.SettleCustomer = vModel.Order.SettleCustomer;
            order.SettlePlatForm = vModel.Order.SettlePlatForm;
            order.ContactCode = vModel.Order.ContactCode;         // 代理商联系人
            order.Managers = vModel.Order.Managers;
            order.ManagerPhone = vModel.Order.ManagerPhone;       // 代理商联系电话
            order.LinkMan = vModel.Order.LinkMan;                 // 游客联系人
            order.LinkPhone = vModel.Order.LinkPhone;             // 游客联系电话
                                                                  // 账单是否显示折扣

            if (order.SettleCustomer != vModel.Order.SettleCustomer)      // 结算客户变更
            {
                order.RebateInBill = _customerBiz.GetById(vModel.Order.SettleCustomer).RebateInBill;
                if (order.InvoiceAmount == order.TolYsPrice) order.RebateInBill = false;   // 如果没有折扣设置不要显示
            }

            using (var ts = new TransactionScope())
            {
                _dao.Update(order);

                #region 更新游客信息

                if (vModel.Travellers2 != null && vModel.Travellers2.Count > 0)
                {
                    foreach (var newTraveller in vModel.Travellers2)
                    {
                        if (newTraveller.Id == default(int))
                        {
                            // 新游客
                            var tt = UpdateTraveller(vModel, newTraveller, userInfo);
                            _travellerDao.Insert(tt);
                        }
                        else
                        {
                            var traveller = travellers.FirstOrDefault(a => a.Id == newTraveller.Id);

                            //// 如果是汽车班的场合，更新座位表
                            //if (!traveller.SeatNum.IsNullOrEmpty())
                            //{
                            //    //TODO: 待修改，变更团计划为共享后，已定座位的处理方式
                            //    ERROR_STATE = SaveSeatDetails(traveller.SeatNum, newTraveller.SeatNum, traveller.TourId);

                            //    //return之后就跳出循环了,后面的代码无法执行
                            //    if (ERROR_STATE != OrderResultState.Code100)
                            //        return ERROR_STATE;
                            //}

                            traveller.Name = newTraveller.Name;
                            traveller.SeatNum = newTraveller.SeatNum;
                            traveller.PassNo = newTraveller.PassNo;
                            traveller.Sex = newTraveller.Sex;
                            traveller.DateOfBirth = newTraveller.DateOfBirth;
                            traveller.PlaceOfBirth = newTraveller.PlaceOfBirth;
                            traveller.DateOfIssue = newTraveller.DateOfIssue;
                            traveller.PlaceOfIssue = newTraveller.PlaceOfIssue;
                            traveller.PinYin = newTraveller.PinYin;
                            traveller.DateOfExpiry = newTraveller.DateOfExpiry;
                            traveller.Phone = newTraveller.Phone;
                            traveller.PassType = newTraveller.PassType;

                            traveller.PriceId = newTraveller.PriceId;
                            traveller.Price = newTraveller.Price;
                            traveller.YsPrice = newTraveller.YsPrice;
                            traveller.FanLi = newTraveller.FanLi;
                            traveller.ZiFei = newTraveller.ZiFei;
                            traveller.SingleRoom = newTraveller.SingleRoom;
                            traveller.Tax = newTraveller.Tax;
                            traveller.VisaPrice = newTraveller.VisaPrice;
                            traveller.TeJiaFanLi = newTraveller.TeJiaFanLi;

                            _travellerDao.Update(traveller);
                        }
                    }
                }

                #endregion 更新游客信息

                // 更新巴士座位信息
                ts.Complete();
            }

            // 重新计算
            FreeQuota(vModel.Order.TourId, vModel.Order.OrderCode, userInfo.Code);

            return OrderResultState.Code100;
        }

        public int UpdateBillInfo(TpOrderModel model)
        {
            var sql = new Sql();
            sql.Append(" update TpOrder set BillOffers=@0, BillAmount=@1, BillDeadline=@2, Deposit=@3, RebateInBill=@4",
                model.BillOffers, model.BillAmount, model.BillDeadline, model.Deposit, model.RebateInBill);
            if (model.DepositDate != null)
                sql.Append(" ,DepositDate=@0 ", model.DepositDate);
            sql.Append(" where OrderCode=@0 ", model.OrderCode);

            return _fileDao.Execute(sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///  保存座位信息
        /// </summary>
        public OrderResultState SaveSeatDetails(string oldSeatNum, string newSeatNum, int tourId)
        {
            // 如果座位未改变 则不更新座位表
            if (oldSeatNum == newSeatNum)
                return OrderResultState.Code100;

            var model = GetSeatDetails(tourId);
            var seats = model.SeatModels;

            var newSeats = seats.FirstOrDefault(a => a.No == newSeatNum.ToString());
            if (newSeats != null)
            {
                switch (newSeats.State)
                {
                    case 1:  // 未占
                        newSeats.State = 2;
                        break;

                    case 2:  //已占
                        return OrderResultState.Code101;

                    case 3:  //锁定
                        return OrderResultState.Code102;

                    default:
                        throw new Exception("无此座位状态！");
                }
            }

            var oldSeats = seats.FirstOrDefault(a => a.No == oldSeatNum.ToString());
            if (oldSeats != null)
                oldSeats.State = 1; // 释放位子。
            // 如果没有该座位号 ，不作处理
            TpBusSeatDao dao = new TpBusSeatDao();
            var jsonSeats = JsonSerializer.Serialize(seats);
            dao.UpdateSeatDetail(model.QuotaId, jsonSeats);

            return OrderResultState.Code100;
        }

        /// <summary>
        /// 获取座位分布数据
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        private TpBusSeatModel GetSeatDetails(int tourId)
        {
            string sql = @"select TpBusSeat.* from TpBusSeat inner join TpTourQuotaMap on TpTourQuotaMap.QuotaId=TpBusSeat.QuotaId
where TpTourQuotaMap.TourId=@0";

            return new TpTourPlanDao().Query<TpBusSeatModel>(sql, tourId).FirstOrDefault();
        }

        #endregion 保存订单

        #region 我的订单->预定统计

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="queryType">查询种类：1.列表，2.应收与已收，3.游客数与返利</param>
        /// <returns></returns>
        public Sql GreateOrderStatisticSql(OrderStatisticVModel vModel, int queryType, CrmAccountModel userInfo)
        {
            var customerList = new CustomerBiz().GetCustomers(userInfo.CustomerCode); //查询当前客户及其附属客户列表
            if (customerList != null && customerList.Count > 0)
            {
                var sql = new Sql();
                if (queryType == 1)
                {
                    sql.Append(@"SELECT A.* FROM TpOrder A");
                }
                else if (queryType == 2)
                {
                    sql.Append(@"SELECT SUM(TolYsPrice) SumPriceCount,SUM(TolPaid) SumTolPaid FROM TpOrder A");
                }
                else//3
                {
                    sql.Append(
                        @"SELECT COUNT(B.Id) SumTravellerCount,SUM(B.FanLi) SumFanLiCount FROM TpOrder A INNER JOIN TpTraveller B on a.OrderCode=b.OrderCode and B.State=2 and A.TravellerCount !=0 ");//已确认和已结算订单(不包括已退团的结算)里的游客
                }

                #region 条件

                sql.Append(@" INNER JOIN TpLine L ON A.LineId=L.LineId WHERE A.OwnerCode=@0 ", Ansi(userInfo.OwnerCode));
                sql.Append(" AND A.BookingCustomer IN (@0)", customerList.Select(t => t.Code).ToArray());

                if (!vModel.Condition.LineName.IsNullOrEmpty())
                    sql.Append(@" AND L.LineName LIKE @0 ", AnsiLike(vModel.Condition.LineName));
                if (!vModel.Condition.OutDateRange.IsNullOrEmpty())
                {
                    var t = vModel.Condition.OutDateRange.Split('-');
                    sql.Append(@" AND A.OutDate>=@0 AND A.OutDate<=@1 ", t[0].ToDateTime(), t[1].ToDateTime());
                }

                if (!vModel.Condition.CreatedTimeRange.IsNullOrEmpty())
                {
                    var t = vModel.Condition.CreatedTimeRange.Split('-');
                    sql.Append(@" AND A.CreatedTime>=@0 AND A.CreatedTime<@0 ", t[0].ToDateTime(), t[1].ToDateTime().AddDays(1));
                }

                //线路编号
                int lineId;
                if (!vModel.Condition.OrderId.IsNullOrEmpty() && int.TryParse(vModel.Condition.OrderId, out lineId))
                    sql.Append(@" AND A.Id=@0", lineId);
                //团号
                int tourId;
                if (!vModel.Condition.TourId.IsNullOrEmpty() && int.TryParse(vModel.Condition.TourId, out tourId))
                {
                    sql.Append(@" AND A.TourId = @0", tourId);
                }
                //分销商
                //if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
                //    sql.Append(@" AND A.BookingCustomer in ( " + vModel.Condition.BookingCustomer + " ) ");
                if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
                    sql.Append(@" AND A.BookingCustomer = @0 ", Ansi(vModel.Condition.BookingCustomer));
                //线路类型
                if (!vModel.Condition.LineScope.IsNullOrEmpty())
                    sql.Append(@" AND A.LineScope=@0 ", vModel.Condition.LineScope);
                //线路类型
                if (!vModel.Condition.LineType.IsNullOrEmpty())
                    sql.Append(@" AND A.LineType=@0 ", vModel.Condition.LineType);
                //订单状态
                if (!vModel.Condition.OrderState.IsNullOrEmpty())
                    sql.Append(@" AND A.OrderState = @0 ", vModel.Condition.OrderState.ToInt());
                //结算状态
                if (!vModel.Condition.SettlementState.IsNullOrEmpty())
                    sql.Append(vModel.Condition.SettlementState == "0" ? " AND JieSuanState<5 " : " AND JieSuanState=5 ");
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
        /// 获取订单列表
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="needPager"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用位置：预定统计(/Seller/OrderStatistic),导出预订单(/Seller/DownloadBookingOrder)
        /// </remarks>
        public PagedList<TpOrderModel> GetMyOrderStatistic(OrderStatisticVModel vModel, bool needPager, CrmAccountModel userInfo)
        {
            var sql = GreateOrderStatisticSql(vModel, 1, userInfo);
            if (sql != null)
            {
                if (needPager)
                    return _dao.Pager(vModel.OrderModels.PageIndex, vModel.OrderModels.PageSize, sql.SQL, sql.Arguments);
                else
                    return new PagedList<TpOrderModel> { Items = _dao.Fetch(sql.SQL, sql.Arguments) };
            }
            return new PagedList<TpOrderModel>() { Items = new List<TpOrderModel>() };
        }

        /// <summary>
        /// 获取汇总
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用位置：预定统计(/Seller/OrderStatistic)
        /// </remarks>
        public OrderStatisticVModel GetStatisticSummary(OrderStatisticVModel vModel, CrmAccountModel userInfo)
        {
            var result = new OrderStatisticVModel();
            var sqlOrder = GreateOrderStatisticSql(vModel, 2, userInfo);
            var sqlTraveller = GreateOrderStatisticSql(vModel, 3, userInfo);
            var tempVModelOrder = sqlOrder != null
                                      ? _dao.Query<OrderStatisticVModel>(sqlOrder.SQL, sqlOrder.Arguments).FirstOrDefault()
                                      : null;
            var tempVModelTraveller = sqlTraveller != null
                                      ? _dao.Query<OrderStatisticVModel>(sqlTraveller.SQL, sqlTraveller.Arguments).FirstOrDefault()
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
                result.SumFanLiCount = tempVModelTraveller.SumFanLiCount;
            }
            return result;
        }

        #endregion 我的订单->预定统计

        #region 关联订单号处理

        /// <summary>
        /// 检查关联订单号是否存在
        /// </summary>
        /// <param name="joinOrderCode"></param>
        /// <returns></returns>
        public bool CheckJoinOrderCode(string joinOrderCode)
        {
            string sql = "select count(1) from TpOrder where JoinOrderCode=@0 and IsCancel=0";

            return _dao.ExecuteScalar<int>(sql, Ansi(joinOrderCode)) > 0;
        }

        #endregion 关联订单号处理

        #region 换团

        /// <summary>
        /// 获取游客数
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public Int32 GetTouristCount(string orderIds)
        {
            return _dao.ExecuteScalar<Int32>(@"SELECT SUM(TravellerCount) FROM TpOrder WHERE Id IN (" + orderIds + ")");
        }

        /// <summary>
        /// 获取座位号列表
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public List<Int32> GetSeatList(string orderIds)
        {
            var seatNums = _dao.Query<string>(@"SELECT A.SeatNum FROM TpTraveller A INNER JOIN TpOrder B ON B.OrderCode = A.OrderCode WHERE B.Id IN (" + orderIds + ")").ToList();
            int temp = 0;
            return (from str in seatNums where Int32.TryParse(str, out temp) select temp).ToList();
        }

        /// <summary>
        /// 并入他团
        /// </summary>
        /// <param name="orderIds">订单号（形如：1,2,3）</param>
        /// <param name="exchangeToTourId">目标团号</param>
        /// <param name="exchangeQuota">源 库存</param>
        /// <param name="targetQuota">目标 库存</param>
        /// <param name="exchangeBusSeat">源 座位表</param>
        /// <param name="targetBusSeat">目标 座位表</param>
        public void MergeToExistTour(string orderIds, int exchangeToTourId, QuotaModel exchangeQuota, QuotaModel targetQuota,
            TpBusSeatModel exchangeBusSeat, TpBusSeatModel targetBusSeat, CrmAccountModel userInfo)
        {
            List<TpOrderModel> orders = _dao.Fetch(@"SELECT * FROM TpOrder WHERE Id IN (" + orderIds + ")");
            List<TpTravellerModel> tourists =
                _travellerDao.Fetch(
                    @"SELECT A.* FROM TpTraveller A INNER JOIN TpOrder B ON B.OrderCode = A.OrderCode WHERE B.Id IN (" +
                    orderIds + ")");
            var exchangeToTour = new TpTourPlanBiz().GetTourById(exchangeToTourId);
            var quotaDao = new QuotaDao(); var busSeatDao = new TpBusSeatDao();

            using (var scope = new TransactionScope())
            {
                foreach (var order in orders)
                {
                    // 记录日志 通知销售
                    LogBiz.WriteOrderLog(exchangeToTour.OwnerCode, order.OrderCode, order.SalerCode, userInfo.Code, "换团", 1);

                    order.TourId = exchangeToTourId;
                    order.LineName = exchangeToTour.LineName;
                    order.LineId = exchangeToTour.LineId;
                    order.OutDate = exchangeToTour.OutDate;

                    _dao.Update(order);
                }
                foreach (var tourist in tourists)
                {
                    tourist.TourId = exchangeToTourId;
                    _travellerDao.Update(tourist);
                }
                quotaDao.Update(exchangeQuota);
                quotaDao.Update(targetQuota);
                if (exchangeBusSeat != null)
                    busSeatDao.Update(exchangeBusSeat);
                if (targetBusSeat != null)
                    busSeatDao.Update(targetBusSeat);
                scope.Complete();
            }
        }

        public List<TpOrderModel> GetExchangeOrders(int tourId, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@" SELECT a.* FROM TpOrder a
INNER JOIN TpLine b ON a.LineId=b.LineId
WHERE a.OwnerCode=@0 ", userInfo.OwnerCode);
            sql.Append(" and tourId = @0 AND IsCancel=0 ", tourId);
            //增加权限设置 非管理员账号登录 仅能查看自己所属的组的信息 add by kzl
            if (userInfo.AccountType != 1 && userInfo.AccountType != 2)
            {
                sql.Append(" and b.TeamID in(select TeamID from TeamAccountMap where AccountCode=@0) ", userInfo.Code);
            }

            return _dao.Query(sql.SQL, sql.Arguments).ToList();

            // return _ordersDao.Fetch(@"SELECT * FROM TpOrder WHERE tourId = @0 AND OrderState != 8 ", tourId);
        }

        #endregion 换团

        public TpOrderModel GetOrderByOrderCode(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT cc.Name as CustomerName, line.LineName, line.TravelDays, c.Name as SalerName, ttp.TourNo, tp.*
FROM TpOrder tp
INNER JOIN TpTourPlan ttp ON ttp.ID=tp.TourId
inner join CrmAccount c on c.Code = tp.SalerCode
inner join TpLine line on tp.LineId=line.LineId
left join CrmCustomer cc on tp.BookingCustomer = cc.Code
 WHERE OrderCode=@0 ", Ansi(orderCode));

            return _dao.Query(sql.SQL, sql.Arguments).FirstOrDefault();
        }


        #region 附件处理

        /// <summary>
        /// 取得订单的所有附件
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TpOrderFileModel> GetOrderFileList(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append("  select * from TpOrderFiles where OrderCode=@0 and IsDel=0", orderCode);

            return _fileDao.Query(sql.SQL, sql.Arguments).ToList();
        }

        public object AddOrderFileInfo(TpOrderFileModel model)
        {
            return _fileDao.Insert(model);
        }

        /// <summary>
        /// 获取缴款单附件清单
        /// </summary>
        /// <param name="payInId"></param>
        /// <returns></returns>
        public int UpdateFileInPanIn(int id, int payInId)
        {
            var sql = new Sql();
            sql.Append(" update TpOrderFiles set KeyId=@1 where Id=@0 ", id, payInId);

            return _fileDao.Execute(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 添加订单附件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int AddOrderFile(TpOrderFileModel model)
        {
            if (model.SourceType == "3")   //类型 3 为 账单
            {
                var entity = _dao.GetOrder(model.OrderCode);
                if (entity.TraceState < 30)   // 如果小于账单制作 就更新
                {
                    entity.TraceState = 30;
                    _dao.Update(entity);
                }
            }

            return Convert.ToInt32(_fileDao.Insert(model));
        }

        public TpOrderFileModel GetOrderFileModel(int Id)
        {
            return _fileDao.GetById(Id);
        }

        /// <summary>
        /// 取得账单当前版本，旧版删除
        /// </summary>
        /// <param name="ordercode"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public int GetOrderFileModelVersion(string ordercode, string sourceType)
        {
            Sql sql = new Sql();
            sql.Append(" select Revision from TpOrderFiles where OrderCode=@0 and SourceType=@1 ORDER BY Revision DESC ", ordercode, sourceType);
            var c = _fileDao.FirstOrDefault(sql.SQL, sql.Arguments);
            if (c == null)
                return 0;
            else
            {
                // 设置为删除
                _fileDao.Update(" set IsDel=1 where OrderCode=@0 and SourceType=@1 ", ordercode, sourceType);
                return c.Revision;
            }
        }

        /// <summary>
        /// 取得版本号 旧版设置无效
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public int UpdateTourNoticeVersion(string orderCode, string sourceType)
        {
            return _fileDao.Update(" set IsDel=1 where OrderCode=@0 and SourceType=@1 ", orderCode, sourceType);
        }

        /// <summary>
        /// 获取出团通知
        /// </summary>
        public TpTourFileModel GetTourNoticeFile(int tourId)
        {
            return _planBiz.GetTourFileByTourId(tourId).Where(t => t.SourceType == "22").FirstOrDefault();
        }

        public int DeleteOrderFile(int Id)
        {
            return _fileDao.Update(" set IsDel=1 where Id=@0", Id.ToString());
        }

        #endregion 附件处理

        #region op开单相关操作方法

        /// <summary>
        /// 开单 不占位
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public OrderResultState SaveOpOrderTrans(BookingVModel vModel, ref string OrderCode, CrmAccountModel userInfo)
        {
            try
            {
                var order = new TpOrderModel();
                decimal TolYsPrice = 0;

                #region 订单基本信息

                order.OrderCode = DBTools.GetSeqNo("TpOrder");
                order.TourId = vModel.TourId;
                order.LineName = vModel.Tour.LineName;
                order.LineId = vModel.Tour.LineId;
                order.SalesTeamId = vModel.SalesTeamId;
                order.SalerCode = vModel.SalerCode;
                order.OutDate = vModel.Tour.OutDate;
                order.SupplierCode = vModel.LineModel.CustomerCode;
                order.OrderState = 1;  // 新订单
                order.OrderSource = 1; // 默认值 同行=1
                order.IsJieSong = 0;
                order.CreatedBy = userInfo.Code;
                order.CreatedTime = DateTime.Now;
                order.ModifiedBy = userInfo.Code;
                order.ModifiedTime = DateTime.Now;
                order.OwnerCode = userInfo.OwnerCode;
                order.TraceState = 10;                        //跟单初始化
                order.EffectiveHour = vModel.EffectiveHour;   //有效时长
                order.TravellerCount = vModel.TravellerCount; //订单人数
                order.SettlePlatForm = 1;                     // 默认自行结算
                order.RebateInBill = false;
                order.Deposit = vModel.Deposit;
                if (vModel.DepositDate != null)
                    order.DepositDate = vModel.DepositDate;

                OrderCode = order.OrderCode;

                var list = new List<TpTravellerModel>();
                if (!string.IsNullOrEmpty(vModel.OpenPriceStr))
                {
                    foreach (string person in vModel.OpenPriceStr.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(person))
                        {
                            var attr = person.Split(',');
                            var num = Convert.ToInt32(attr[2]);
                            for (int i = 0; i < num; i++)
                            {
                                var price = vModel.PriceModels.Where(t => t.Id == Convert.ToInt32(attr[0])).FirstOrDefault();
                                list.Add(new TpTravellerModel
                                {
                                    OrderCode = order.OrderCode,
                                    PriceId = Convert.ToInt32(attr[0]),
                                    PriceContent = (price == null ? "" : price.PriceRemark),
                                    Name = "游客" + i,
                                    PinYin = "You Ke" + i,
                                    Price = Convert.ToDecimal(attr[1]),
                                    YsPrice = Convert.ToDecimal(attr[1]),
                                    TourId = order.TourId,
                                    PassType = 2,
                                    FanLi = 0,
                                    XiaoFei = 0,
                                    SingleRoom = 0,
                                    TeJiaFanLi = 0,
                                    IsOccupiedQuota = 1,
                                    JiePrice = 0,
                                    SongPrice = 0,
                                    ZiFei = 0,
                                    State = 2,
                                    Sex = "1",
                                    IsMianPiao = 0,
                                    CreatedTime = DateTime.Now,
                                    ModifiedTime = DateTime.Now,
                                    IsChild = false
                                });

                                TolYsPrice += Convert.ToDecimal(attr[1]);
                            }
                        }
                    }
                }
                // 应收累加赋值
                order.TolYsPrice = TolYsPrice;
                order.InvoiceAmount = TolYsPrice;

                #endregion 订单基本信息

                //  OrderResultState ERROR_STATE;
                using (var ts = new TransactionScope())
                {
                    // 再次判断是否有名额 设定库存+-
                    //  ERROR_STATE = SetQuota(vModel);
                    //  if (ERROR_STATE != OrderResultState.Code100)
                    //    return ERROR_STATE;

                    // 添加

                    _dao.Insert(order);

                    foreach (var per in list)
                    {
                        _travellerDao.Insert(per);
                    }

                    ts.Complete();
                }

                return OrderResultState.Code100;
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return OrderResultState.Code199;
            }
        }

        /// <summary>
        ///  获取游客集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="order"></param>
        /// <param name="busPoint"></param>
        /// <returns></returns>
        private TpTravellerModel UpdateTraveller(OrderEditVModel vModel, TpTravellerModel item, CrmAccountModel userInfo)
        {
            var tp = vModel.Prices.FirstOrDefault(a => a.Id == item.PriceId.ToInt());

            TpTravellerModel traveller = new TpTravellerModel();
            traveller.OrderCode = vModel.Order.OrderCode;
            traveller.TourId = vModel.Order.TourId;
            traveller.Name = item.Name;
            traveller.Phone = item.Phone;
            traveller.PassType = item.PassType;
            traveller.PassNo = item.PassNo;
            traveller.PinYin = item.PinYin;
            traveller.Sex = item.Sex;
            traveller.DateOfBirth = item.DateOfBirth;
            traveller.PlaceOfBirth = item.PlaceOfBirth;
            traveller.DateOfIssue = item.DateOfIssue;
            traveller.PlaceOfIssue = item.PlaceOfIssue;
            traveller.DateOfExpiry = item.DateOfExpiry;
            traveller.SeatNum = item.SeatNum;
            traveller.PriceId = item.PriceId;
            traveller.PriceContent = (tp == null ? "" : tp.PriceRemark);
            traveller.State = 2;   // 有效
            traveller.Remark = item.Remark;
            traveller.CreatedBy = userInfo.Code;
            traveller.CreatedTime = DateTime.Now;
            traveller.ModifiedBy = userInfo.Code;
            traveller.ModifiedTime = DateTime.Now;
            traveller.XiaoFei = 0;
            traveller.IsMianPiao = item.IsMianPiao;
            if (item.IsMianPiao == 1)//如果 买一送X的场合
            {
                traveller.Price = 0;
                traveller.TeJiaFanLi = 0;
            }
            else
            {
                traveller.Price = tp.SettlePrice;
                traveller.FanLi = item.FanLi;
                traveller.TeJiaFanLi = tp.TeJiaFanLi;
            }
            traveller.SingleRoom = item.IsSingleRoom == "on" ? vModel.TourPlan.SingleRoom : 0;
            traveller.IsOccupiedQuota = tp.SuitNum > 0 ? 1 : 0;
            traveller.JiePrice = 0;// busPoint.JiePrice;
            traveller.SongPrice = 0; // busPoint.SongPrice;
            traveller.ZiFei = item.IsZiFei == "on" ? vModel.TourPlan.ZiFei : 0;
            traveller.Tax = item.IsTax == "on" ? vModel.TourPlan.Tax : 0;
            traveller.VisaPrice = item.IsVisaPrice == "on" ? vModel.TourPlan.VisaPrice : 0;
            traveller.YsPrice = traveller.Price + traveller.SingleRoom
                                                // + traveller.JiePrice + traveller.SongPrice
                                                + traveller.ZiFei
                                                + traveller.VisaPrice + traveller.Tax
                                                + traveller.FanLi - traveller.TeJiaFanLi;

            return traveller;
        }

        #endregion op开单相关操作方法

        #region 微信使用

        /// <summary>
        /// 微信订单状态变更
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="newStatus"></param>
        public void UpdateOrderStatus(string orderCode, string newStatus)
        {
            Sql sql = new Sql();
            sql.Append(" set TraceState=@1 where OrderCode=@0", orderCode, newStatus);

            int row = _dao.Update(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得未完成订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public List<TpOrderModel> GetTaskOrderList(TpOrderVModel vModel, int topCount, CrmAccountModel userInfo)
        {
            var sql = new Sql();

            sql.Append(" SELECT a.*, b.LineName FROM TpOrder a ")
                .Append(" inner join TpLine b on a.LineId=b.LineId")
                .Append("  WHERE a.OwnerCode=@0 and TraceState < 40 ", userInfo.OwnerCode);

            if (!vModel.LineName.IsNullOrEmpty())
                sql.Append(@" AND b.LineName like @0 ", AnsiLike(vModel.LineName));
            if (!vModel.OrderId.IsNullOrEmpty())
                sql.Append(@" AND a.OrderCode=@0 ", vModel.OrderId);

            // 销售
            if (!vModel.SalerCode.IsNullOrEmpty())
                sql.Append(@" AND a.SalerCode=@0  ", vModel.SalerCode);

            if (!vModel.OrderState.IsNullOrEmpty())
                sql.Append(@" AND a.OrderState=@0 ", vModel.OrderState.ToInt());
            if (!vModel.OutDateRange.IsNullOrEmpty())
            {
                var t = vModel.OutDateRange.Split('-');
                sql.Append(@" AND a.OutDate>=@0 AND a.OutDate<@1", t[0].ToDateTime(), t[1].ToDateTime().AddDays(1));
            }


            if (!vModel.CrmTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and  b.TeamID=@0  ", vModel.CrmTeamId);
            }

            sql.Append(" order by a.OutDate LIMIT " + topCount);

            return _dao.Query(sql.SQL, sql.Arguments).ToList();
        }

        #endregion 微信使用

        public TpTravellerModel GetTravellerById(int touristId)
        {
            return _travellerDao.GetById(touristId);
        }

        public List<TpTravellerModel> GetLeaderOfTour(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM TpTraveller WHERE TourId=@0 AND IsLeader=1", tourId);
            return _travellerDao.Fetch(sql.SQL, sql.Arguments);
        }

        public List<TpTravellerModel> GetLeaderOfQuota(int tourId)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT t.*
FROM TpTraveller t
inner join TpTourQuotaMap ttq on t.TourId = ttq.TourId
WHERE ttq.QuotaId=@0 AND t.IsLeader=1 ", tourId);
            return _travellerDao.Fetch(sql.SQL, sql.Arguments);
        }

        public void UpdateTraveller(TpTravellerModel model)
        {
            var entity = _travellerDao.GetById(model.Id);
            if (entity != null)
            {
                entity.Name = model.Name;
                entity.PinYin = model.PinYin;
                entity.PlaceOfBirth = model.PlaceOfBirth;
                entity.DateOfBirth = model.DateOfBirth;
                entity.PassType = model.PassType;
                entity.PassNo = model.PassNo;
                entity.PlaceOfBirth = model.PlaceOfBirth;
                entity.DateOfIssue = model.DateOfIssue;
                entity.PlaceOfIssue = model.PlaceOfIssue;
                entity.DateOfExpiry = model.DateOfExpiry;

                _travellerDao.Update(entity);
            }
        }

        /// <summary>
        /// 递归获得最合适的折让
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public CustomerPolicyModel GetBestPolicy(List<CustomerPolicyModel> list, string dest)
        {
            var entity = list.Where(t => t.Code == dest).FirstOrDefault();
            if (entity == null)
            {
                if (dest.StartsWith("/") && dest.Length > 2 && dest.EndsWith("/"))
                {
                    string dd = dest.Substring(0, dest.Substring(0, dest.Length - 1).LastIndexOf("/") + 1);
                    return GetBestPolicy(list, dd);
                }
                else
                {
                    // 获得出境规则
                    entity = list.Where(t => t.Code == null).FirstOrDefault();
                    return entity;
                }
            }
            else
            {
                return entity;
            }
        }

        /// <summary>
        /// 销售欠款列表
        /// </summary>
        /// <returns></returns>
        public List<OrderDebtModel> GetDebtBySales()
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT tp.SalerCode, ca.OpenID ,COUNT(*) OrderNum, SUM(TolYsPrice-TolPaid) Amount
FROM TpOrder tp INNER JOIN CrmAccount ca on tp.SalerCode = ca.Code
 where TolPaid < TolYsPrice and DATE_ADD(IFNULL(BillDeadline, OutDate), interval 3 DAY) < now()
GROUP BY tp.SalerCode, ca.OpenID ");

            return _dao.Query<OrderDebtModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 销售最近订单 10条
        /// </summary>
        /// <param name="salerCode"></param>
        /// <returns></returns>
        public List<TpOrderModel> RecentOrder(string salerCode)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT t.*, c.Name as CustomerName, tl.LineName
FROM TpOrder t inner join TpLine tl on t.LineId = tl.LineId
left join CrmCustomer c on t.BookingCustomer=c.Code
where t.SalerCode=@0 ORDER BY t.Id DESC LIMIT 10 ", salerCode);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public TpOrderModel LastOrderByContact(string contactCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT t.*, c.Name as CustomerName, tl.LineName
FROM TpOrder t inner join TpLine tl on t.LineId = tl.LineId
left join CrmCustomer c on t.BookingCustomer=c.Code
where t.OrderState=2 and t.ContactCode=@0 ORDER BY t.Id DESC ", contactCode);
            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }
    }
}