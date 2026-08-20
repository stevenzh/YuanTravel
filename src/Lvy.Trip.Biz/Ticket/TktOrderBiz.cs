using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.TicketDB;
using Lvy.Models.TourDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Dao.Ticket;
using Lvy.Trip.Dao.Tour;
using Lvy.Visa.Biz;
using Lvy.VModels;
using Lvy.VModels.Ticket;
using Lvy.VModels.Tour;
using Lvy.Web.Common;
using MySql.Data.MySqlClient;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Lvy.Trip.Biz.Ticket
{
    /// <summary>
    /// 订单相关业务模块
    /// </summary>
    public class TktOrderBiz : BaseBiz
    {
        private TpTourBalanceDao dao = new TpTourBalanceDao();
        private TktOrdersDao detailsDao = new TktOrdersDao();
        private TktProductDao productDao = new TktProductDao();
        private TktPriceDao priceDao = new TktPriceDao();

        private TouristBiz touristBiz = new TouristBiz();

        /// <summary>
        /// 根据Id获取TktProductModel
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public TktProductModel GetProductById(string productId)
        {
            return productDao.FirstOrDefault("SELECT * FROM tktproduct WHERE ProductId=@0 ", productId);
        }

        /// <summary>
        /// 获取订单及订单详细信息
        /// 产品名称
        /// </summary>
        /// <returns></returns>
        public TpTourBalanceModel GetOrder(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT a.*, b.*
FROM TpTourBalance a LEFT JOIN TktOrders b ON a.MasterOrderCode=b.MasterOrderCode
WHERE a.MasterOrderCode=@0 ", orderCode);

            return dao.Query<TpTourBalanceModel, TktOrdersModel, TpTourBalanceModel>(
                    new TktOrderToDetailRelator().MapIt, sql.SQL, sql.Arguments).FirstOrDefault();
        }

        /// <summary>
        /// 获取订单信息
        /// 产品名称
        /// </summary>
        /// <returns></returns>
        public TpTourBalanceModel GetOrderByCode(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM TpTourBalance WHERE MasterOrderCode=@0", orderCode);

            return dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据Id获取TktProductModel列表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<TpTourBalanceModel> GetById(string orderIds)
        {
            string[] ids = orderIds.Split(',').Where(t => t.IsNullOrEmpty() == false).ToArray();
            if (ids.Length <= 0) return new List<TpTourBalanceModel>();
            var sql = new Sql();
            sql.Append(@"SELECT * FROM TpTourBalance WHERE Id IN ( @0 )", ids);
            return dao.Fetch(sql.SQL, sql.Arguments);
        }

        #region 订单列表

        // Ps.此处与分销商订单统计类似，但为了保持分离，两者程序没有执行复用。

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="sql"></param>
        public void AppendOrderlistCondition(TktOrderVModel vModel, Sql sql, CrmAccountModel userInfo)
        {
            var customer = DictionaryBiz.GetCachedCustomer(userInfo.CustomerCode, userInfo.OwnerCode);
            sql.Append(@" WHERE a.OwnerCode=@0 AND B.IsValid=1", vModel.OwnerCode);
            if (customer.IsOwner) { }
            else if (customer.IsSupplier)
            {
                sql.Append(@" AND a.SupplierCode=@0 ", Ansi(userInfo.CustomerCode));    //若为供应商，仅能看到自己的产品订单
            }
            else if (customer.IsDistributors)
            {
                sql = null;                                                             //若为分销商，认为权限配置有误，不应将订单管理->订单统计暴露给分销商
            }
            if (null != sql)
            {
                if (!string.IsNullOrEmpty(vModel.OrderCode))
                    sql.Append(@" AND a.MasterOrderCode=@0 ", vModel.OrderCode);
                if (!vModel.BookingCustomer.IsNullOrEmpty())
                {
                    var customerSql = new CustomerBiz().GetCustomerCodesSql(vModel.BookingCustomer, vModel.OwnerCode);
                    sql.Append(@" AND a.BookingCustomer IN ({0})".With(customerSql));
                }
                if (vModel.OrderState > 0)
                    sql.Append(@" AND a.OrderState=@0", vModel.OrderState);
                if (vModel.AuditState > 0)
                    sql.Append(@" AND a.AuditState=@0", vModel.AuditState);
                if (!vModel.SettlementState.IsNullOrEmpty())
                    sql.Append(vModel.SettlementState == "0" ? @" AND a.PaymentStatus<5 " : @" AND a.PaymentStatus=5 ");     //未结算：已确认；已结算：已结算
                if (!string.IsNullOrEmpty(vModel.DateRange))
                {
                    var d = vModel.DateRange.Split('-');
                    sql.Append(@" AND a.OutDate >= @0 AND a.OutDate <= @1", d[0].ToDateTime(), d[1].ToDateTime());
                }
                if (!vModel.ProductName.IsNullOrEmpty())
                    sql.Append(" AND b.ProductName LIKE @0", AnsiLike(vModel.ProductName));
                int productId;
                if (!vModel.ProductId.IsNullOrEmpty() && int.TryParse(vModel.ProductId, out productId))
                    sql.Append(" AND b.ProductId = @0", productId);
            }
        }

        /// <summary>
        /// 批发商团队订单管理获取订单列表
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="needPager"></param>
        /// <returns></returns>
        /// <remarks>
        /// 调用位置：/TktOrder/Search、/TktFinance/OrderReport
        /// </remarks>
        public PagedList<TpTourBalanceModel> GetPagedOrders(TktOrderVModel vModel, bool needPager, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@" SELECT * FROM (
SELECT DISTINCT  a.Id, a.MasterOrderCode, a.ContactName, a.ContactPhone, a.YingShou, a.YiShou, a.OrderState,
  a.AgentCode, a.AgentName, a.SupplierCode, a.Outdate
FROM TpTourBalance a INNER JOIN TktOrders b ON a.MasterOrderCode=b.MasterOrderCode ");
            AppendOrderlistCondition(vModel, sql, userInfo);
            if (sql == null) return new PagedList<TpTourBalanceModel> { Items = new List<TpTourBalanceModel>() };
            sql.Append(@") TempTb ORDER BY TempTb.OrderState, TempTb.Outdate");
            //注：鉴于petapoco组织分页查询语句的处理方式，需要在包含distinct语句外包装一层子查询，并将排序放到外部
            PagedList<TpTourBalanceModel> result;
            if (needPager)
            {
                result = dao.Pager<TpTourBalanceModel>(vModel.Orders.PageIndex, vModel.Orders.PageSize, sql.SQL, sql.Arguments);
            }
            else
            {
                result = new PagedList<TpTourBalanceModel> { Items = dao.Query<TpTourBalanceModel>(sql.SQL, sql.Arguments).ToList() };
            }
            if (result.Items.Count > 0)
            {
                var orderCodeQueryStr = String.Join(",", result.Items.Select(p => p.MasterOrderCode));
                var orderDetails = detailsDao.Fetch(@"SELECT * FROM TktOrders WHERE MasterOrderCode IN (" + orderCodeQueryStr + ") And IsValid=1 ");
                result.Items.ForEach(p => p.OrderDetails = orderDetails.FindAll(m => m.MasterOrderCode == p.MasterOrderCode));
            }
            return result;
        }

        /// <summary>
        /// 统计订单
        /// </summary>
        /// <param name="vModel"></param>
        public void StatisticTktOrderSupplier(TktOrderVModel vModel, CrmAccountModel userInfo)
        {
            var saledNum = new Sql().Append(@"Select CASE COUNT(1) WHEN 0 THEN 0 ELSE SUM(b.PeopleNum) END TotalSaledNum From TpTourBalance a INNER JOIN TktOrders b ON a.MasterOrderCode=b.MasterOrderCode ");
            AppendOrderlistCondition(vModel, saledNum, userInfo);
            vModel.TotalSaledNum = dao.ExecuteScalar<int>(saledNum.SQL, saledNum.Arguments);
            var saledVolume = new Sql().Append(@"Select SUM(TempTb.YingShou) TotalSaledVolume,SUM(TempTb.YiShou) TotalPaid FROM (SELECT DISTINCT a.Id,a.YingShou,a.YiShou From TpTourBalance a INNER JOIN TktOrders b ON a.MasterOrderCode=b.MasterOrderCode ");
            AppendOrderlistCondition(vModel, saledVolume, userInfo);
            saledVolume.Append(@") TempTb");
            var result = dao.Query<TktOrderVModel>(saledVolume.SQL, saledVolume.Arguments).FirstOrDefault();
            vModel.TotalSaledVolume = result.TotalSaledVolume;
            vModel.TotalPaid = result.TotalPaid;
        }


        #endregion 订单列表

        #region 修改订单

        /// <summary>
        /// 获取编辑订单对象的数据
        /// </summary>
        /// <returns></returns>
        public BookingVModel GetEditOrderModel(string orderCode)
        {
            BookingVModel vModel = new BookingVModel();
            vModel.Order = GetOrder(orderCode);
            //编辑页只取有效明细 Modified 20130329
            vModel.Order.OrderDetails = vModel.Order.OrderDetails.Where(p => p.IsValid == 1).ToList();

            vModel.OrderedProducts = GetOrderedProductsPrices(vModel.Order);
            vModel.Product = vModel.OrderedProducts[0];
            vModel.OutDates = vModel.Order.OrderDetails.Select(a => Convertor.ToDateFormat(a.OutDate)).ToArray();
            vModel.TravellerList = touristBiz.GetTouristList(orderCode);

            vModel.ProductsCurrentDatePrices = GetProductsCurrentDatePrices(vModel);
            return vModel;
        }

        /// <summary>
        /// 获取已定产品所有的价格数据
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        private List<OrderPricesVModel> GetProductsCurrentDatePrices(BookingVModel vModel)
        {
            var results = new List<OrderPricesVModel>();

            foreach (var product in vModel.OrderedProducts)
            {
                // 判断固定价格 还是日历价格
                var detail = vModel.Order.OrderDetails.Where(a => a.ProductId == product.ProductId).FirstOrDefault();
                Sql sql = new Sql();
                sql.Append(@" SELECT a.ProductId, b.* FROM TktRulePriceMap a 
    INNER JOIN TktPrice b ON a.RuleId=b.RuleId 
    WHERE a.CurrentDate=@0 AND a.ProductId=@1", detail.OutDate, product.ProductId);

                var temps = priceDao.Query<OrderPricesVModel>(sql.SQL, sql.Arguments).ToList();

                results.AddRange(temps);
            }

            //        foreach (var product in vModel.Order.OrderDetails)
            //        {
            //            // 判断固定价格 还是日历价格
            //            var detail = vModel.Order.OrderDetails.Where(a => a.ProductId == product.ProductId).FirstOrDefault();
            //            Sql sql = new Sql();
            //            sql.Append(@" SELECT a.ProductId, b.* FROM TktPriceRule a 
            //INNER JOIN TktPrice b ON a.Id=b.RuleId 
            //WHERE b.Id=@0", product.PriceId);
            //            var temps = priceDao.Query<OrderPricesVModel>(sql.SQL, sql.Arguments).ToList();
            //            results.AddRange(temps);
            //        }

            foreach (var detail in vModel.Order.OrderDetails)
            {
                //绑定门票预定人数
                var one = results.Where(a => a.Id == detail.PriceId).FirstOrDefault();
                results.Remove(one);
                if (one != null)
                    one.PeopleNum = detail.PeopleNum;
                else
                    throw new Exception("没有对象的报价ID=" + one.Id);
                results.Add(one);
            }

            return results;
        }

        /// <summary>
        ///  获取已定的产品对象及价格类型
        /// </summary>
        /// <returns></returns>
        public List<TktProductModel> GetOrderedProductsPrices(TpTourBalanceModel model)
        {
            var productIds = model.OrderDetails.Select(a => a.ProductId).Distinct().ToArray();
            return productDao.Fetch(" SELECT * FROM TktProduct WHERE ProductID IN (@0)", productIds);
        }

        #endregion 修改订单

        #region 保存订单

        /// <summary>
        /// 预定保存订单
        /// </summary>
        /// <returns></returns>
        public CommonJsonResult AddOrder(BookingVModel vModel, CrmAccountModel currentUser)
        {
            var order = GetOrderInfo(vModel, currentUser);
            var orderDetails = GetDetailsInfo(vModel, order);
            List<TktProductModel> updateQuotas = new List<TktProductModel>();

            //验证限额
            foreach (var productId in vModel.ProductIds)
            {
                if (productId.IsNullOrEmpty()) continue;
                var product = productDao.GetByProductId(productId);
                order.ProductName = product.ProductName;
                order.ProductType = product.ProductType;
                order.TeamId = product.TeamID;

                if (product.TuiJianType == 2)  // 限制库存模式
                {
                    var detailsPerProduct = orderDetails.Where(p => p.ProductId == productId).ToList().Where(p => p.IsValid == 1);
                    var peopleCount = detailsPerProduct.Sum(a => a.PeopleNum);
                    if (product.PlanQuota - product.HoldQuota - product.UsedQuota < peopleCount)
                    {
                        return new CommonJsonResult { State = "0", Message = "[" + product.ProductName + "]可定数量不足,请修改人数。" };
                    }
                    else
                    {
                        if (product.LimitQuota > 0)
                        {
                            var details = GetDetails(productId, currentUser.CustomerCode).Where(p => p.IsValid == 1);
                            var orderedNum = details.Sum(detail => detail.PeopleNum);//当前客户对该产品预定数
                            if (product.LimitQuota < orderedNum + peopleCount)
                                return new CommonJsonResult { State = "0", Message = "[" + product.ProductName + "]预定数量超出限额,请修改人数。" };
                        }
                        updateQuotas.Add(new TktProductModel
                        {
                            ProductId = product.ProductId,
                            PlanQuota = product.PlanQuota,
                            HoldQuota = product.HoldQuota,
                            UsedQuota = product.UsedQuota + peopleCount,
                            StartTime = product.StartTime,
                            EndTime = product.EndTime,
                            LimitQuota = product.LimitQuota,
                            BeginBuyTime = product.BeginBuyTime,
                            LastDate = product.LastDate
                        });
                    }
                }
            }

            // 计算订单应收
            order.YingShou = orderDetails.Sum(a => a.YsPrice);

            using (var ts = new TransactionScope())
            {
                dao.Insert(order);

                foreach (var detail in orderDetails)
                {
                    detail.MasterOrderCode = order.MasterOrderCode;
                    detailsDao.Insert(detail);
                }

                updateQuotas.ForEach(p => productDao.UpdateQuota(p));

                ts.Complete();
            }

            return new CommonJsonResult { State = "1", Code = order.Id.ToString() };
        }

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public string[] UpdateOrder(BookingVModel vModel, CrmAccountModel userInfo)
        {
            var masterEntity = GetOrderByCode(vModel.Order.MasterOrderCode);

            masterEntity.OutDate = vModel.OutDates[0].ToDateTime();
            masterEntity.GuideName = vModel.Order.GuideName;
            masterEntity.GuidePhone = vModel.Order.GuidePhone;
            masterEntity.ContactName = vModel.Order.ContactName;
            masterEntity.ContactPhone = vModel.Order.ContactPhone;
            masterEntity.Remarks = vModel.Order.Remarks;
            masterEntity.ModifiedBy = vModel.Order.ModifiedBy;
            masterEntity.ModifiedTime = DateTime.Now;
            masterEntity.SalesTeamId = vModel.Order.SalesTeamId;
            masterEntity.SalerCode = vModel.Order.SalerCode;
            masterEntity.AgentCode = vModel.Order.AgentCode;

            // 详细表
            var orderList = GetDetailsInfo(vModel, masterEntity);
            var updateQuotas = new List<TktProductModel>();
            var needUpdateQuota = false;
            var oldOrderEntitys = GetOrderDetails(masterEntity.MasterOrderCode);

            //验证限额
            foreach (var productId in vModel.ProductIds)
            {
                needUpdateQuota = false;
                if (productId.IsNullOrEmpty()) continue;
                var product = productDao.GetByProductId(productId);
                if (product.TuiJianType == 2)   // 限制库存模式
                {
                    var orderItem = orderList.Where(p => p.ProductId == productId).ToList();
                    var peopleCount = orderItem.Sum(a => a.PeopleNum);//提交的人数
                    var oldPeopleCount = oldOrderEntitys.Sum(a => a.PeopleNum);//原始订单的人数

                    if (oldPeopleCount >= peopleCount)
                    {
                        needUpdateQuota = true;
                        peopleCount = peopleCount - oldPeopleCount;
                        //continue; //若减少，则不存在超额问题
                    }
                    else
                    {
                        peopleCount = peopleCount - oldPeopleCount;//仅计算比原始人数多出部分是否超额
                        if (product.PlanQuota - product.HoldQuota - product.UsedQuota < peopleCount)
                        {
                            return new[] { "0", "[" + product.ProductName + "]可定数量不足,请修改人数。" };
                        }
                        if (product.LimitQuota > 0)
                        {
                            var details = GetDetails(productId, userInfo.CustomerCode).Where(p => p.IsValid == 1);
                            var orderedNum = details.Sum(detail => detail.PeopleNum);
                            if (product.LimitQuota < orderedNum + peopleCount)
                                return new[] { "0", "[" + product.ProductName + "]预定数量超出限额,请修改人数。" };
                        }
                        //若可定充足，且未超出限额
                        needUpdateQuota = true;
                    }
                    if (needUpdateQuota)
                    {
                        updateQuotas.Add(new TktProductModel
                        {
                            Id = product.Id,
                            ProductId = product.ProductId,
                            PlanQuota = product.PlanQuota,
                            HoldQuota = product.HoldQuota,
                            UsedQuota = product.UsedQuota + peopleCount,
                            StartTime = product.StartTime,
                            EndTime = product.EndTime,
                            LimitQuota = product.LimitQuota,
                            BeginBuyTime = product.BeginBuyTime,
                            LastDate = product.LastDate
                        });
                    }
                }
            }

            // 计算订单应收
            masterEntity.YingShou = orderList.Sum(a => a.YsPrice);
            using (var ts = new TransactionScope())
            {
                dao.Update(masterEntity);

                // 删除历史details表数据
                detailsDao.Update(" SET IsValid=0 WHERE MasterOrderCode=@0 AND IsValid=1", masterEntity.Id);
                foreach (var item in orderList)
                {
                    var entity = oldOrderEntitys.Where(m => m.PriceId == item.PriceId).FirstOrDefault();
                    if (entity != null)
                    {
                        item.ID = entity.ID;
                        detailsDao.Update(item);
                    }
                    else
                    {
                        detailsDao.Insert(item);
                    }
                }

                updateQuotas.ForEach(p => productDao.UpdateQuota(p));

                ts.Complete();
            }

            return new string[] { "1", masterEntity.Id.ToString() };
        }

        /// <summary>
        /// 获取订单详细信息
        /// </summary>
        /// <returns></returns>
        private List<TktOrdersModel> GetDetailsInfo(BookingVModel vModel, TpTourBalanceModel order)
        {
            var details = new List<TktOrdersModel>();
            TktOrdersModel detail = null;
            for (int i = 0; i < vModel.PeopleNum.Length; i++)
            {
                if (vModel.PeopleNum[i] == 0)
                    continue;

                Sql sql = GetOrderDetailsProductSql(vModel.PriceIds[i].ToInt());
                detail = priceDao.Query<TktOrdersModel>(sql.SQL, sql.Arguments).SingleOrDefault();
                detail.MasterOrderCode = order.MasterOrderCode;
                detail.PeopleNum = vModel.PeopleNum[i].ToInt();
                detail.IsValid = 1;

                var kv = vModel.ProIdDateBeans.FirstOrDefault(a => a.Key == detail.ProductId.ToString());
                detail.OutDate = Convert.ToDateTime(kv.Value);
                if (detail.TktType == 1 || detail.TktType == 2) // 签单
                    detail.YsPrice = detail.SettlePrice * detail.PeopleNum;
                else
                    detail.YsPrice = -(detail.SysPrice * detail.PeopleNum);

                details.Add(detail);
            }

            return details;
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        private TpTourBalanceModel GetOrderInfo(BookingVModel vModel, CrmAccountModel currentUser)
        {
            TpTourBalanceModel model = new TpTourBalanceModel();
            model.MasterOrderCode = DBTools.GetSeqNo("TourBalance");
            model.Type = 9;
            model.OutDate = vModel.OutDates[0].ToDateTime();
            model.GuideName = vModel.Order.GuideName;
            model.GuidePhone = vModel.Order.GuidePhone;
            model.ContactName = vModel.Order.ContactName;
            model.ContactPhone = vModel.Order.ContactPhone;
            model.Remarks = vModel.Order.Remarks;
            model.YiShou = 0;
            model.CreatedBy = currentUser.Code;
            model.CreatedTime = DateTime.Now;
            model.ModifiedBy = currentUser.Code;
            model.ModifiedTime = DateTime.Now;
            model.OwnerCode = currentUser.OwnerCode;
            model.IsCancel = 0;
            model.AuditState = 0;
            model.OrderSource = vModel.Order.OrderSource;
            model.AgentCode = vModel.Order.AgentCode;
            model.SalesTeamId = vModel.Order.SalesTeamId;
            model.SalerCode = vModel.Order.SalerCode;
            model.IsPackage = 1;

            return model;
        }

        private Sql GetOrderDetailsProductSql(int priceId)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT a.Id as PriceId,a.PriceType,a.MarketPrice,a.SettlePrice,a.SysPrice ,
  c.ProductId, c.ProductName,c.ArriveDest as DestId, c.TktType
FROM TktPrice a INNER JOIN TktPriceRule b ON a.RuleId = b.Id
  INNER JOIN TktProduct c ON b.ProductId = c.ProductId
WHERE a.Id=@0", priceId);

            return sql;
        }

        #endregion 保存订单

        /// <summary>
        /// 删除订单信息
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public int DeleteOrderInfo(string orderCode, CrmAccountModel userInfo)
        {
            var order = GetOrder(orderCode);
            order.IsCancel = 1;
            order.YingShou = 0;
            order.ModifiedBy = userInfo.Code;
            order.ModifiedTime = DateTime.Now;

            var details = GetOrderDetails(order.MasterOrderCode);

            using (var ts = new TransactionScope())
            {
                foreach (var detail in details)
                {
                    //1. 如果特惠的场合 ，控卫
                    ResetTktQuota(detail.ProductId, detail.PeopleNum);

                    //2.清空价格
                    detail.YsPrice = 0;
                    detail.PeopleNum = 0;
                    detail.IsValid = 0;
                    detailsDao.Update(detail);
                }
                dao.Update(order);
                ts.Complete();
            }

            return 1;
        }

        /// <summary>
        /// 获取订单详细
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TktOrdersModel> GetOrderDetails(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append("SELECT * FROM TktOrders WHERE IsValid=1 AND MasterOrderCode=@0", orderCode);
            return detailsDao.Fetch(sql.SQL, sql.Arguments);
        }

        public List<CommonOrderModel> GetCommonOrderByCode(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT t.MasterOrderCode OrderCode, t.ProductId, t.PeopleNum TravelCount, 
 vi.ProductName, c.Name AS AgentName, ca.Name AS SalerName,
 t.YsPrice TolYsPrice,
 ttb.AgentCode,ttb.SalesTeamId, ttb.SalerCode
FROM TktOrders t
INNER JOIN tptourbalance ttb ON ttb.MasterOrderCode = t.MasterOrderCode
INNER JOIN TktProduct vi ON t.ProductId = vi.ProductId
LEFT JOIN CrmCustomer c ON ttb.AgentCode =c.Code
LEFT JOIN crmaccount ca ON ttb.SalerCode = ca.Code
WHERE t.MasterOrderCode = @0
union
SELECT t.ChildOrderCode AS OrderCode, t.ProductID, t.Quantity AS TravellerCount,
vi.ProductName, c.Name AS AgentName, ca.Name AS SalerName,
 t.Amount TolYsPrice, ttb.AgentCode, ttb.SalesTeamId, ttb.SalerCode
FROM  tpchildorders t
INNER JOIN tpproducts vi ON t.ProductID = vi.ProductID
INNER JOIN tptourbalance ttb ON ttb.MasterOrderCode = t.OrderCode
LEFT JOIN CrmCustomer c ON ttb.AgentCode = c.Code
LEFT JOIN crmaccount ca ON ttb.SalerCode = ca.Code
WHERE t.OrderCode = @0", orderCode);
            return dao.Query<CommonOrderModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 通过产品编号获取库存库存
        /// 如果null，则库存无限
        /// </summary>
        /// <returns></returns>
        private int ResetTktQuota(string productId, int num)
        {
            var product = productDao.GetByProductId(productId);
            if (product != null)
            {
                product.UsedQuota = product.UsedQuota - num;

                productDao.UpdateUsedQuota(product);
            }

            return 1;
        }

        /// <summary>
        /// 更新订单信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateOrderInfo(TpTourBalanceModel model)
        {
            return dao.Update(model);
        }

        /// <summary>
        /// 更新订单信息
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public int UpdateOrderInfo(IEnumerable<TpTourBalanceModel> models)
        {
            int result = 0;
            using (var scope = new TransactionScope())
            {
                foreach (var model in models)
                {
                    result += dao.Update(model);
                }
                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// 获取所选择日期的价格数据
        /// </summary>
        /// <param name="selectDate"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<TktPriceModel> GetCurrentPrices(string selectDate, string productId)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT b.* FROM TktRulePriceMap a 
INNER JOIN TktPrice b ON a.RuleId=b.RuleId 
WHERE b.IsValid=1 AND a.CurrentDate=@0 AND a.ProductId=@1 ", selectDate.ToDateTime(), productId);

            return priceDao.Fetch(sql.SQL, sql.Arguments);
        }

        public List<TktPriceModel> GetCurrentPrices(string productId)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT b.* FROM TktPriceRule a 
INNER JOIN TktPrice b ON a.Id=b.RuleId 
WHERE b.IsValid=1 AND a.ProductId=@0 ", productId);

            return priceDao.Fetch(sql.SQL, sql.Arguments);
        }

        #region 搜索相关业务

        /// <summary>
        /// 获取同目的地下所有已上线的景区产品
        /// </summary>
        /// <param name="destId"></param>
        /// <returns></returns>
        public List<KeyValueBean> GetSameProductsByDest(string destId, string ownerCode)
        {
            string sql = @"select id as `Key`,ProductName as `value` from TktProduct
                        where ArriveDest=@0 and ownerCode=@1 and ProductState=3 and ProductType=1";

            return productDao.Query<KeyValueBean>(sql, Ansi(destId), ownerCode).ToList();
        }

        /// <summary>
        /// 获取同目的地下所有已上线的shopping店
        /// </summary>
        /// <param name="destId"></param>
        /// <returns></returns>
        public List<KeyValueBean> GetSameShoppingsByDest(string destId, string ownerCode)
        {
            string sql = @"select id as `Key`,ProductName as `value` from TktProduct
                        where ArriveDest=@0 and ownerCode=@1 and ProductState=3 and ProductType=2";

            return productDao.Query<KeyValueBean>(sql, Ansi(destId), ownerCode).ToList();
        }

        #endregion 搜索相关业务

        /// <summary>
        /// 获取确认单信息
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ConfirmOrderVModel GetConfirmOrder(string orderCode)
        {
            var vModel = new ConfirmOrderVModel();
            vModel.Order = GetOrder(orderCode);

            var customer = DictionaryBiz.GetCachedCustomer(vModel.Order.AgentCode, vModel.OwnerCode);
            vModel.ReceiveCustomer = customer;  //分销商

            //var plantBiz = new PlatformBiz();
            //var userInfo = GlobalContext.Current.UserInfo;
            //vModel.PlatForm = plantBiz.GetByCustomerCode(userInfo.OwnerCode);   //平台信息
            vModel.PlatCustomer = DictionaryBiz.GetCachedCustomer(customer.OwnerCode, vModel.OwnerCode);  //平台商户

            var admins = new TktAdminBiz().GetByTicketOrderCode(vModel.Order.MasterOrderCode);
            if (admins != null && admins.Count > 0)
            {
                List<string> accountCodes = admins.Select(p => p.AccountCode).Distinct().ToList();
                vModel.TicketAdmins = new AccountBiz().GetAccountByCode(accountCodes);
            }
            return vModel;
        }

        /// <summary>
        /// 根据产品Id获取订单详细
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<TktOrdersModel> GetOrderDetailsByProductId(string productId)
        {
            return detailsDao.Fetch(@"SELECT * FROM TktOrders WHERE ProductId=@0", productId);
        }

        /// <summary>
        /// 获取订单详细
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public List<TktOrdersModel> GetDetails(string productId, string customerCode)
        {
            return detailsDao.Fetch(@"SELECT A.* FROM TktOrders A INNER JOIN TpTourBalance B ON B.MasterOrderCode=A.MasterOrderCode WHERE A.ProductId=@0 AND B.BookingCustomer=@1", productId, Ansi(customerCode));
        }


        #region 分销商->团队门票订单

        /// <summary>
        /// 分销商团队门票查询语句(条件)
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public void AppendTktOrderStatisticSql(TktOrderVModel vModel, Sql sql, CrmAccountModel userInfo)
        {
            sql.Append(" WHERE a.OwnerCode=@0 AND B.IsValid=1", userInfo.OwnerCode);
            if (!vModel.BookingCustomer.IsNullOrEmpty())
            {
                sql.Append(@" AND a.AgentName LIKE @0", AnsiLike(vModel.BookingCustomer));
            }
            else
            {
                var customerList = new CustomerBiz().GetCustomers(userInfo.CustomerCode);   //查询当前客户及其附属客户列表
                sql.Append(@" AND a.AgentCode IN (@0)", customerList.Select(t => t.Code).ToArray());
            }
            if (!vModel.OrderCode.IsNullOrEmpty())
                sql.Append(@" AND a.MasterOrderCode LIKE @0", AnsiLike(vModel.OrderCode));
            if (vModel.OrderState > 0)
                sql.Append(@" AND a.OrderState = @0", vModel.OrderState);
            if (!vModel.DateRange.IsNullOrEmpty())
            {
                var t = vModel.DateRange.Split('-');
                sql.Append(@" AND a.OutDate>=@0 AND a.OutDate<=@1", t[0].ToDateTime(), t[1].ToDateTime());
            }

            if (!vModel.ProductName.IsNullOrEmpty())
                sql.Append(" AND b.ProductName Like @0", AnsiLike(vModel.ProductName));
            int productId;
            if (!vModel.ProductId.IsNullOrEmpty() && int.TryParse(vModel.ProductId, out productId))
                sql.Append(" AND b.ProductId = @0", productId);
        }

        /// <summary>
        /// 分销商团队门票
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="needPager"></param>
        /// <returns></returns>
        public PagedList<TpTourBalanceModel> GetTktOrderStatistic(TktOrderVModel vModel, bool needPager, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@" SELECT * FROM (
SELECT DISTINCT a.Id, a.MasterOrderCode, a.ContactName, a.ContactPhone, a.YingShou, a.YiShou, a.OrderState,
  a.AgentCode, a.AgentName, a.SupplierCode,a.Outdate
 FROM TpTourBalance a INNER JOIN TktOrders b ON a.MasterOrderCode=b.MasterOrderCode ");

            AppendTktOrderStatisticSql(vModel, sql, userInfo);
            sql.Append(@") TempTb ORDER BY TempTb.OrderState, TempTb.Outdate");
            //注：鉴于petapoco组织分页查询语句的处理方式，需要在包含distinct语句外包装一层子查询，并将排序放到外部
            PagedList<TpTourBalanceModel> result;
            if (needPager)
            {
                result = dao.Pager<TpTourBalanceModel>(vModel.Orders.PageIndex, vModel.Orders.PageSize, sql.SQL, sql.Arguments);
            }
            else
            {
                result = new PagedList<TpTourBalanceModel> { Items = dao.Query<TpTourBalanceModel>(sql.SQL, sql.Arguments).ToList() };
            }
            if (result.Items.Count > 0)
            {
                var orderCodeQueryStr = String.Join(",", result.Items.Select(p => p.MasterOrderCode));
                var orderDetails = detailsDao.Fetch(@"SELECT * FROM TktOrders WHERE MasterOrderCode IN (" +
                                                   orderCodeQueryStr + ") and IsValid=1");
                result.Items.ForEach(p => p.OrderDetails = orderDetails.FindAll(m => m.MasterOrderCode == p.MasterOrderCode));
            }
            return result;
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="vModel"></param>
        public void StatisticTktOrder(TktOrderVModel vModel, CrmAccountModel userInfo)
        {
            var saledNum = new Sql().Append(@"Select CASE COUNT(1) WHEN 0 THEN 0 ELSE SUM(b.PeopleNum) END TotalSaledNum From TpTourBalance a INNER JOIN TktOrders b On a.MasterOrderCode=b.MasterOrderCode ");
            AppendTktOrderStatisticSql(vModel, saledNum, userInfo);
            vModel.TotalSaledNum = dao.ExecuteScalar<int>(saledNum.SQL, saledNum.Arguments);
            var saledVolume = new Sql().Append(@"Select SUM(TempTb.YingShou) TotalSaledVolume,SUM(TempTb.YiShou) TotalPaid FROM (SELECT DISTINCT a.Id,a.YingShou,a.YiShou From TpTourBalance a INNER JOIN TktOrders b ON a.MasterOrderCode=b.MasterOrderCode ");
            AppendTktOrderStatisticSql(vModel, saledVolume, userInfo);
            saledVolume.Append(@") TempTb");
            var result = dao.Query<TktOrderVModel>(saledVolume.SQL, saledVolume.Arguments).FirstOrDefault();
            vModel.TotalSaledVolume = result.TotalSaledVolume;
            vModel.TotalPaid = result.TotalPaid;
        }

        /// <summary>
        /// 产品分组
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public List<ReportProductGroupVModel> BillGroupByProduct(TktOrderVModel vModel, CrmAccountModel userInfo)
        {
            if (userInfo.CustomerCode.IsNullOrEmpty())
                return null;
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT ProductId, ProductName, OutDate, AgentCode, PeopleNum, TktType, rank FROM (
SELECT heyf_tmp.*, @rownum:= @rownum +1,
IF (@pdept = heyf_tmp.ProductId, @rank:= @rank +1, @rank:= 1) AS rank,
@pdept:= heyf_tmp.ProductId
FROM (SELECT b.ProductId, b.ProductName, a.OutDate, a.AgentCode, b.PeopleNum, b.TktType
 FROM TpTourBalance a INNER JOIN tktorders b ON b.MasterOrderCode=a.MasterOrderCode ");
            AppendTktOrderStatisticSql1(vModel, sql, userInfo);
            sql.Append(@" AND A.IsCancel=0 ORDER BY b.ProductId ) Heyf_tmp,
 ( SELECT @rownum:= 0, @pdept:= null, @rank:= 0) a) result");//导出账单过滤已取消

            List<ReportProductGroupVModel> list = new List<ReportProductGroupVModel>();
            using (MySqlConnection conn = new MySqlConnection(MyHelper.connectionString))
            {
                conn.Open();
                using (MySqlDataReader dr = MySqlHelper.ExecuteReader(conn, sql.ToString()))
                {
                    while (dr.Read())
                    {
                        var item = new ReportProductGroupVModel();
                        item.ProductName = MyHelper.GetString(dr, "ProductName");
                        item.OutDate = MyHelper.GetDateTime(dr, "OutDate");
                        item.BookingCustomer = MyHelper.GetString(dr, "AgentCode");
                        item.PeopleNum = MyHelper.GetInt(dr, "PeopleNum");
                        item.TktType = MyHelper.GetInt(dr, "TktType");

                        list.Add(item);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 客户分组
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public List<ReportCustomerGroupVModel> BillGroupByCustomer(TktOrderVModel vModel, CrmAccountModel userInfo)
        {
            if (userInfo.CustomerCode.IsNullOrEmpty())
                return null;
            StringBuilder sql = new StringBuilder();
            sql.Append(@"select ProductId, ProductName, OutDate, AgentCode, PeopleNum, TktType, SysPrice, PriceType,
    GuideName, GuidePhone, ContactName, ContactPhone, rank from (
SELECT heyf_tmp.*, @rownum:= @rownum +1,
if (@pdept = heyf_tmp.AgentCode, @rank:= @rank +1, @rank:= 1) as rank,
@pdept:= heyf_tmp.AgentCode
FROM ( SELECT b.ProductId, b.ProductName, a.OutDate, a.AgentCode, b.PeopleNum, b.TktType, b.SysPrice,
  a.GuideName, a.GuidePhone, a.ContactName, a.ContactPhone, b.PriceType
FROM TpTourBalance a INNER JOIN tktorders b ON b.MasterOrderCode=a.MasterOrderCode ");
            AppendTktOrderStatisticSql1(vModel, sql, userInfo);
            sql.Append(@" AND A.IsCancel=0 ORDER BY a.AgentCode ) Heyf_tmp,
 (SELECT @rownum:= 0, @pdept:= null, @rank:= 0) a) result ");//导出账单过滤已取消

            List<ReportCustomerGroupVModel> list = new List<ReportCustomerGroupVModel>();
            using (MySqlConnection conn = new MySqlConnection(MyHelper.connectionString))
            {
                conn.Open();
                using (MySqlDataReader dr = MySqlHelper.ExecuteReader(conn, sql.ToString()))
                {
                    while (dr.Read())
                    {
                        var item = new ReportCustomerGroupVModel();
                        item.ProductName = MyHelper.GetString(dr, "ProductName");
                        item.OutDate = MyHelper.GetDateTime(dr, "OutDate");
                        item.BookingCustomer = MyHelper.GetString(dr, "AgentCode");
                        item.PeopleNum = MyHelper.GetInt(dr, "PeopleNum");
                        item.TktType = MyHelper.GetInt(dr, "TktType");
                        item.GuideName = MyHelper.GetString(dr, "GuideName");
                        item.GuidePhone = MyHelper.GetString(dr, "GuidePhone");
                        item.Managers = MyHelper.GetString(dr, "ContactName");
                        item.ManagerPhone = MyHelper.GetString(dr, "ContactPhone");

                        list.Add(item);
                    }
                }
            }

            return list;
        }

        public void AppendTktOrderStatisticSql1(TktOrderVModel vModel, StringBuilder sql, CrmAccountModel userInfo)
        {
            sql.AppendFormat(" WHERE a.OwnerCode='{0}' AND B.IsValid=1 ", userInfo.OwnerCode);
            if (!vModel.BookingCustomer.IsNullOrEmpty())
            {
                sql.AppendFormat(@" AND a.BookingCustomer LIKE '{0}'", AnsiLike(vModel.BookingCustomer));
            }
            if (!vModel.OrderCode.IsNullOrEmpty())
                sql.AppendFormat(@" AND a.OrderCode LIKE {0}", AnsiLike(vModel.OrderCode));
            if (vModel.OrderState > 0)
                sql.AppendFormat(@" AND a.OrderState = {0}", vModel.OrderState);
            if (!vModel.DateRange.IsNullOrEmpty())
            {
                var t = vModel.DateRange.Split('-');
                sql.AppendFormat(@" AND a.OutDate>='{0}' AND a.OutDate<='{1}' ", t[0].Trim(), t[1].Trim());
            }

            if (!vModel.ProductName.IsNullOrEmpty())
                sql.AppendFormat(" AND b.ProductName Like '{0}'", AnsiLike(vModel.ProductName));
            int productId;
            if (!vModel.ProductId.IsNullOrEmpty() && int.TryParse(vModel.ProductId, out productId))
                sql.AppendFormat(" AND b.ProductId = {0}", productId);
        }

        #endregion 分销商->团队门票订单

        /// <summary>
        /// 获取未处理订单数  【商户可用】
        /// </summary>
        /// <returns></returns>
        public int GetUnHandledOrderCount(CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@"SELECT COUNT(1) FROM TktOrders WHERE OwnerCode=@0 AND OrderState=1", Ansi(userInfo.OwnerCode));
            var customer = DictionaryBiz.GetCachedCustomer(userInfo.CustomerCode, userInfo.OwnerCode);
            if (customer.IsOwner) { }
            else if (customer.IsDistributors)
            {
                //分销商仅能看到自己预定的订单（我的订单->团队门票）
                sql.Append(@" AND BookingCustomer = @0", Ansi(userInfo.CustomerCode));
            }
            else if (customer.IsSupplier)
            {
                //供应商在订单管理模块处理自己提供的产品
                sql.Append(@" AND SupplierCode = @0", Ansi(userInfo.CustomerCode));
            }

            return dao.ExecuteScalar<int>(sql.SQL, sql.Arguments);
        }


    }
}