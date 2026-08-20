using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.TourDB;
using Lvy.Trip.Dao.Tour;
using Lvy.VModels.Finance;
using Lvy.VModels.Tour;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Finance
{
    /// <summary>
    /// 付款记录
    /// </summary>
    public class PaymentBiz : BaseBiz
    {
        #region 变量

        private readonly TpTourCostDao _costsDao = new TpTourCostDao();
        private readonly TpTourPaymentDao _paymentDao = new TpTourPaymentDao();

        #endregion 变量

        #region 财务付款

        /// <summary>
        /// 取得团单付款列表
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<TpTourCostModel> GetCostList(PaymentVModel vModel)
        {
            var sql = GreateSql(vModel, 1);
            var list = _costsDao.Pager(vModel.CostModels.PageIndex, vModel.CostModels.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        public FinanceTotalModel GetFinanceSummary(PaymentVModel vModel)
        {
            var sql = GreateSql(vModel, 2);
            return _costsDao.Query<FinanceTotalModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="queryType">查询种类：1.分页列表  2.合计 </param>
        /// <returns></returns>
        public Sql GreateSql(PaymentVModel vModel, int queryType)
        {
            var sql = new Sql();

            if (queryType == 1)
            {
                sql.Append(@" SELECT tc.*, tb.TourNo, tb.ProductName, tb.OutDate, tb.AuditState
FROM TpTourCosts tc INNER JOIN TpTourBalance tb ON tc.MasterOrderCode = tb.MasterOrderCode
WHERE tc.IsCopy=0 ");
            }
            else if (queryType == 2)
            {
                sql.Append(@" SELECT SUM(tc.ItemCost) as SumTolCost, SUM(tc.PaidCost) as SumPaidCost
FROM TpTourCosts tc INNER JOIN TpTourBalance tb ON tc.MasterOrderCode = tb.MasterOrderCode
WHERE tc.IsCopy=0 ");
            }

            sql.Append(@" AND tb.OwnerCode=@0 ", Ansi(vModel.OwnerCode));

            //var userInfo = GlobalContext.Current.UserInfo;  // 当前用户
            //var userCustomer = DictionaryTools.GetCachedCustomer(userInfo.CustomerCode);
            //if (userCustomer.IsOwner) { }
            //else if (userCustomer.IsSupplier)
            //{
            //    sql.Append(@" AND tb.OwnerCode=@0 ", Ansi(userInfo.OwnerCode));
            //    //供应商，获取自己产品的订单
            //    sql.Append(@" AND tc.SupplierId=@0 ", Ansi(userInfo.CustomerCode));
            //}
            //else if (userCustomer.IsDistributors)
            //{
            //    sql.Append(@" AND tb.OwnerCode=@0 ", Ansi(userInfo.OwnerCode));
            //    //分销商，不应看到订单统计
            //    return new PagedList<TpTourCostModel> { Items = new List<TpTourCostModel>() };
            //}

            if (!vModel.Condition.CostSupplier.IsNullOrEmpty())
                sql.Append(@" AND tc.SupplierId = @0 ", Ansi(vModel.Condition.CostSupplier));

            if (!vModel.Condition.ProductName.IsNullOrEmpty())
                sql.Append(@" AND tb.ProductName LIKE @0 ", AnsiLike(vModel.Condition.ProductName));
            if (!vModel.Condition.StartOutDate.IsNullOrEmpty())
                sql.Append(@" AND tb.OutDate >= @0 ", vModel.Condition.StartOutDate.ToDateTime());
            if (!vModel.Condition.EndOutDate.IsNullOrEmpty())
                sql.Append(@" AND tb.OutDate <= @0 ", vModel.Condition.EndOutDate.ToDateTime());

            if (!vModel.Condition.StartPaymentTime.IsNullOrEmpty())
                sql.Append(@" AND tc.PayTime >= @0 ", vModel.Condition.StartPaymentTime);
            if (!vModel.Condition.EndPaymentTime.IsNullOrEmpty())
                sql.Append(@" AND tc.PayTime < @0 ", vModel.Condition.EndPaymentTime.ToDateTime().AddDays(1).ToString());

            // 团号
            if (!vModel.Condition.TourNo.IsNullOrEmpty())
                sql.Append(@" AND tb.TourNo = @0", vModel.Condition.TourNo);
            //线路类型
            if (!vModel.Condition.LineTeam.IsNullOrEmpty())
                sql.Append(@" AND tb.TeamId=@0  ", vModel.Condition.LineTeam);
            if (!vModel.Condition.TourAuditState.IsNullOrEmpty())
                sql.Append(@" AND tb.AuditState = @0 ", vModel.Condition.TourAuditState.ToInt());
            //状态
            if (!vModel.Condition.CostStatus.IsNullOrEmpty())
                sql.Append(@" AND tc.Status=@0 ", vModel.Condition.CostStatus.ToInt());

            if (queryType == 1)
            {
                sql.Append(" ORDER BY tc.Status, tb.Outdate ");
            }

            return sql;
        }

        /// <summary>
        /// 付款
        /// </summary>
        /// <param name="costId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public int FuKuan(int costId, decimal amount, CrmAccountModel userInfo)
        {
            var costModel = _costsDao.GetById(costId);
            decimal paidCost = costModel.ItemCost;   // 应付

            decimal unPaid = costModel.ItemCost - costModel.PaidCost;
            if (Math.Abs(amount) > Math.Abs(unPaid))
            {
                throw new Exception("本次付款大于未付款项");
            }

            var currentDateTime = DateTime.Now;

            var paymentModel = new TpPaymentModel();
            paymentModel.MasterOrderCode = costModel.MasterOrderCode;
            paymentModel.CostCode = costModel.Code;
            paymentModel.SupplierId = costModel.SupplierId;
            paymentModel.Amount = amount;
            paymentModel.PaymentBy = userInfo.Code;
            paymentModel.PayTime = currentDateTime;
            paymentModel.AuditBy = userInfo.Code;
            paymentModel.AuditTime = currentDateTime;


            costModel.PaidCost = costModel.PaidCost + amount;
            if (amount < unPaid)
            {
                costModel.Status = 3; //已付款
            }
            else
            {
                costModel.Status = 4;   // 已付清
            }

            _costsDao.Execute("UPDATE TpTourCosts set PaidCost=@1, Status=@2, PayTime=now() WHERE Code =@0", costModel.Code, costModel.PaidCost, costModel.Status);
            _paymentDao.Insert(paymentModel);

            return 1;
        }

        /// <summary>
        /// 获得成本
        /// </summary>
        /// <param name="costId"></param>
        /// <returns></returns>
        public TpTourCostModel GetCostById(int costId)
        {
            return _costsDao.GetById(costId);
        }

        /// <summary>
        /// 获取成本集合
        /// </summary>
        /// <param name="costIds"></param>
        /// <returns></returns>
        public List<TpTourCostModel> GetCost(IEnumerable<string> costIds, string ownerCode)
        {
            if (costIds == null) throw new ArgumentNullException("orderCodes");
            var sql = new Sql();

            sql.Append(" SELECT * FROM TpTourCosts WHERE OwnerCode=@0 ", ownerCode);
            sql.Append(@" AND Id IN (  " + string.Join(",", costIds) + " )");

            return _costsDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 添加付款记录
        /// </summary>
        /// <param name="paymentModel"></param>
        public void AddTourPayment(TpPaymentModel paymentModel)
        {
            _paymentDao.Insert(paymentModel);
        }

        /// <summary>
        /// 更新成本
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public int UpdateCost(TpTourCostModel cost)
        {
            return (int)_costsDao.Update(cost);
        }

        #endregion 财务付款
    }
}