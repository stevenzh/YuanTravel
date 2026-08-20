using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.TourDB;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Ticket;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Dao.Tour;
using Lvy.Visa.Biz;
using Lvy.VModels.Tour;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Finance
{
    /// <summary>
    /// 单团核算
    /// </summary>
    public class TourBalanceBiz : BaseBiz
    {
        private TpTourBalanceDao _dao = new TpTourBalanceDao();
        private TpTourCostDao _costDao = new TpTourCostDao();
        private TourFileDao _fileDao = new TourFileDao();
        //private TpTourPaymentDao _payemntDao = new TpTourPaymentDao();
        private TpOrderDao _orderDao = new TpOrderDao();

        private TpOrderPayInBiz _payInBiz = new TpOrderPayInBiz();
        private TpChildOrderBiz _childBiz = new TpChildOrderBiz();
        private VisaOrderBiz _visaBiz = new VisaOrderBiz();
        private TktOrderBiz _tktBiz = new TktOrderBiz();

        public PagedList<TpTourBalanceModel> GetPageList(TourBalanceVModel vModel, string ownerCode)
        {
            var sql = GetSql(vModel, ownerCode, 1);
            var result = _dao.Pager<TpTourBalanceModel>(vModel.Balances.PageIndex, vModel.Balances.PageSize, sql.SQL, sql.Arguments);
            return result;
        }

        public FinanceTotalModel Summey(TourBalanceVModel vModel, string ownerCode)
        {
            var sql = GetSql(vModel, ownerCode, 2);
            var result = _dao.Query<FinanceTotalModel>(sql.SQL, sql.Arguments).FirstOrDefault();
            return result;
        }

        public Sql GetSql(TourBalanceVModel vModel, string ownerCode, int type)
        {
            var sql = new Sql();

            if (type == 1)
            {
                sql.Append(@" SELECT tp.*, ct.TeamName, cc.Name AS BranchName FROM TpTourBalance tp
LEFT JOIN CrmTeam ct ON tp.TeamId=ct.TeamId
LEFT JOIN CrmCustomer cc ON tp.BranchCode=cc.Code ");
            }
            else if (type == 2)
            {
                sql.Append(@" SELECT SUM(tp.YingShou) as SumTolYsPrice, SUM(tp.YiShou) as SumTolPaid, SUM(tp.TotalCost) as SumTolCost, SUM(tp.MaoLi) as SumTolMaoLi
FROM TpTourBalance tp
LEFT JOIN CrmTeam ct ON tp.TeamId=ct.TeamId
LEFT JOIN CrmCustomer cc ON tp.BranchCode=cc.Code ");
            }

            #region 组织查询条件

            sql.Append(@" WHERE tp.OwnerCode=@0 AND IsCopy=@1 ", Ansi(ownerCode), vModel.IsCopy);

            if (!vModel.Condition.TeamId.IsNullOrEmpty())
                sql.Append(@" AND tp.TeamId = @0 ", Ansi(vModel.Condition.TeamId));
            if (!vModel.Condition.TourNo.IsNullOrEmpty())
                sql.Append(@" AND tp.TourNo LIKE @0 ", AnsiLike(vModel.Condition.TourNo));
            if (!vModel.Condition.ProductName.IsNullOrEmpty())
                sql.Append(@" AND tp.LineName LIKE @0 ", AnsiLike(vModel.Condition.ProductName));
            if (!vModel.Condition.OutDateRange.IsNullOrEmpty())
            {
                var t = vModel.Condition.OutDateRange.Split('-');
                sql.Append(@" AND tp.OutDate >= @0 AND tp.OutDate <= @1", t[0].Trim().ToDateTime(), t[1].Trim().ToDateTime());
            }
            if (vModel.Condition.Type != 0)
                sql.Append(@" AND tp.Type=@0 ", vModel.Condition.Type);
            if (vModel.Condition.IsPackage != 0)
                sql.Append(@" AND tp.IsPackage=@0 ", vModel.Condition.IsPackage);
            if (vModel.Condition.ProductType != 0)
                sql.Append(@" AND tp.ProductType=@0 ", vModel.Condition.ProductType);
            if (!vModel.Condition.TourAuditState.IsNullOrEmpty())
                sql.Append(@" AND tp.AuditState=@0 ", vModel.Condition.TourAuditState.ToInt());

            #endregion 组织查询条件

            sql.Append(@" ORDER BY tp.CreatedTime DESC ");

            return sql;
        }

        public TpTourBalanceModel GetBalanceByOrderCode(string orderCode)
        {
            return _dao.FirstOrDefault("select * from TpTourBalance where MasterOrderCode=@0", orderCode);
        }

        public void SaveBalance(TourBalanceVModel vModel)
        {
            using (var ts = new TransactionScope())
            {
                _dao.Insert(vModel.Balance);

                foreach (var cost in vModel.CostList)
                {
                    _costDao.Insert(cost);
                }

                ts.Complete();
            }
        }

        public void AddBalance(TpTourBalanceModel model)
        {
            _dao.Insert(model);
        }

        public void UpdateBalance(TpTourBalanceModel model)
        {
            _dao.Update(model);
        }

        /// <summary>
        /// 更新团单 应收 应付 已收
        /// </summary>
        /// <param name="type"></param>
        /// <param name="orderCode"></param>
        public void UpdateBalanceAmount(int type, string orderCode)
        {
            if (type == 1)   // 线路产品
            {
                var order = GetOrderByOrderCode(orderCode);
                var orders = GetValidOrderByTourId(order.TourId); // 获取有效订单
                var tourBalance = GetBalanceByTourId(order.TourId);             //获取单团
                var CostList = GetCostsByOrderCode(tourBalance.MasterOrderCode);   // 成本记录

                var yingshou = orders.Sum(t => t.TolYsPrice);  // 总应收
                var yishou = orders.Sum(t => t.TolYsPrice); // 已收
                var yingfu = CostList.Sum(t => t.ItemCost); // 应付
                var maoli = yingshou - yingfu; // 毛利
                _dao.Update("SET YingShou=@1, YiShou=@2, TotalCost=@3, MaoLi=@4 WHERE MasterOrderCode=@0 ", tourBalance.MasterOrderCode, yingshou, yishou, yingfu, maoli);
            }
            else
            {
                var tourBalance = GetBalanceByOrderCode(orderCode);   //获取单团
                var CostList = GetCostsByOrderCode(orderCode);    // 成本记录
                var payin = _payInBiz.GetPayInList(orderCode);    // 缴款记录
                var yishou = payin.Where(t => t.State == 20).Sum(t => t.Amount); // 已收
                var yingfu = CostList.Sum(t => t.ItemCost); // 应付

                if (tourBalance.IsPackage == 2)   // 部门报团
                {
                    var yingshou = payin.Sum(t => t.Amount);  // 总应收
                    var maoli = yingshou - yingfu; // 毛利

                    _dao.Update("SET YingShou=@1, YiShou=@2, TotalCost=@3, MaoLi=@4 WHERE MasterOrderCode=@0 ", orderCode, yingshou, yishou, yingfu, maoli);
                }
                else if (type == 3)
                {
                    var visaOrder = _visaBiz.GetVisaOrderByCode(orderCode);
                    var childOrder = _childBiz.GetTpChildOrderList(orderCode);
                    var yingshou = visaOrder.Price * visaOrder.TotalNum + childOrder.Where(m => m.IsCancel == 0).Sum(m => m.Amount);
                    var maoli = yingshou - yingfu; // 毛利

                    _dao.Update("SET YingShou=@1,YiShou=@2, TotalCost=@2, MaoLi=@3 WHERE MasterOrderCode=@0 ", orderCode, yingshou, yishou, yingfu, maoli);
                }
                else if (type == 9)
                {
                    var tktOrder = _tktBiz.GetOrderDetails(orderCode);
                    var childOrder = _childBiz.GetTpChildOrderList(orderCode);
                    var yingshou = tktOrder.Where(m => m.IsValid == 1).Sum(m => m.YsPrice) + childOrder.Where(m => m.IsCancel == 0).Sum(m => m.Amount);
                    var maoli = yingshou - yingfu; // 毛利

                    _dao.Update("SET YingShou=@1, YiShou=@2, TotalCost=@2, MaoLi=@3 WHERE MasterOrderCode=@0 ", orderCode, yingshou, yishou, yingfu, maoli);
                }
            }
        }

        private TpOrderModel GetOrderByOrderCode(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT cc.Name as CustomerName, line.LineName, line.TravelDays, c.Name as SalerName, ttp.TourNo, tp.*
FROM TpOrder tp
INNER JOIN TpTourPlan ttp ON ttp.ID=tp.TourId
inner join CrmAccount c on c.Code = tp.SalerCode
inner join TpLine line on tp.LineId=line.LineId
left join CrmCustomer cc on tp.BookingCustomer = cc.Code
 WHERE OrderCode=@0 ", Ansi(orderCode));

            return _orderDao.Query(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        private List<TpOrderModel> GetValidOrderByTourId(int tourId)
        {
            var sql = new Sql();
            sql.Append(@"SELECT t.*, c.Name AS CustomerName
FROM TpOrder t
LEFT JOIN CrmCustomer c ON t.BookingCustomer=c.Code
WHERE t.tourId=@0 AND ((t.OrderState=2 AND t.IsCancel=0) OR t.IsCancel = 2) ", tourId);
            return _orderDao.Fetch(sql.SQL, sql.Arguments);
        }

        public TpTourBalanceModel GetBalanceByOrderCode(string orderCode, bool isCopy = false)
        {
            var list = _dao.Fetch("SELECT * FROM TpTourBalance WHERE MasterOrderCode=@0", orderCode);
            if (isCopy)
            {
                return list.Where(t => t.IsCopy).FirstOrDefault();
            }
            else
                return list.Where(t => t.IsCopy == false).FirstOrDefault();
        }

        public TpTourBalanceModel GetBalanceByTourId(int tourId, bool isCopy = false)
        {
            var list = _dao.Fetch("select * from TpTourBalance where TourId=@0", tourId);
            if (isCopy)
            {
                var l = list.Where(t => t.IsCopy).ToList();
                //                if (l.Count == 0 && list.Count > 0)
                //                {
                //                    // 复制单团表
                //                    string sql2 = @"INSERT INTO TpTourBalance (TourId,LineId,YingShou,YiShou,Num,TotalCost,MaoLi,GuideName,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,OPAuditBy,OPAuditTime,CWAuditBy,CWAuditTime,IsCopy)
                //SELECT TourId,LineId,YingShou,YiShou,Num,TotalCost,MaoLi,GuideName,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,OPAuditBy,OPAuditTime,CWAuditBy,CWAuditTime, true FROM TpTourBalance WHERE TourId=@0 ";
                //                    _balanceDao.Execute(sql2, tourId);

                //                    l = _balanceDao.Fetch("select * from TpTourBalance where TourId=@0 and IsCopy=true", tourId);
                //                }
                return l.First();
            }
            else
                return list.Where(t => t.IsCopy == false).FirstOrDefault();
        }

        /// <summary>
        /// 复制团单
        /// </summary>
        /// <param name="orderCode"></param>
        public void CopyTourBalance(string orderCode)
        {
            using (var ts = new TransactionScope())
            {
                // 清理重复记录
                string clearBalance = "DELETE FROM TpTourBalance WHERE MasterOrderCode=@0 AND IsCopy=1 ";
                _dao.Execute(clearBalance, orderCode);
                string clearCost = "DELETE FROM TpTourCosts WHERE MasterOrderCode=@0 AND IsCopy=1 ";
                _dao.Execute(clearCost, orderCode);

                // 复制成本表
                string sql1 = @"INSERT INTO TpTourCosts (Code, MasterOrderCode, SupplierId, Item , Cost, Num, ItemCost, PaidCost, Remark, PaymentType, PayTime, IsValid, Status, ModifiedBy, ModifiedTime, Currency, ROE, TravelerArray, IsCopy)
SELECT Code,MasterOrderCode,SupplierId,Item ,Cost,Num,ItemCost,PaidCost,Remark,PaymentType,PayTime,IsValid,Status,ModifiedBy,ModifiedTime,Currency,ROE,TravelerArray, true FROM TpTourCosts WHERE MasterOrderCode=@0 ";
                _dao.Execute(sql1, orderCode);

                // 复制单团表
                string sql2 = @"INSERT INTO TpTourBalance (TourId,LineId,YingShou,YiShou,Num,TotalCost,MaoLi,GuideName,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,OPAuditBy,OPAuditTime,CWAuditBy,CWAuditTime,IsCopy)
SELECT TourId,LineId,YingShou,YiShou,Num,TotalCost,MaoLi,GuideName,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,OPAuditBy,OPAuditTime,CWAuditBy,CWAuditTime, true FROM TpTourBalance WHERE MasterOrderCode=@0 ";
                _dao.Execute(sql2, orderCode);

                ts.Complete();
            }
        }

        #region 成本

        public TpTourCostModel GetCostById(int costId)
        {
            return _costDao.GetById(costId);
        }

        public List<TpTourCostModel> GetCostsByOrderCode(string orderCode, bool isCopy = false)
        {
            var list = _costDao.Fetch(@"SELECT ttc.`*`, cc.Name AS SupplierName, bdd.`Value` AS ItemValue
FROM tptourcosts ttc
 INNER JOIN crmcustomer cc ON ttc.SupplierId = cc.`Code`
 INNER JOIN BaseDictionaryDetail bdd ON ttc.Item = bdd.`Key` AND bdd.Name = 'SupplierCostItemsEnum'
WHERE ttc.MasterOrderCode=@0 ", orderCode);
            if (isCopy)
            {
                var l = list.Where(t => t.IsCopy).ToList();
                //                if (l.Count == 0 && list.Count > 0)
                //                {
                //                    // 复制
                //                    _costsDao.Execute(@"INSERT INTO tptourcosts
                //(`TourId`, `Code`, `SupplierId`, `Item`, `Cost`, `Num`, `ItemCost`, `PaidCost`,
                // `Remark`, `PaymentType`, `PayTime`, `IsValid`, `Status`, `ModifiedBy`,
                // `ModifiedTime`, `Currency`, `ROE`, `TravelerArray`, `IsCopy`)
                //SELECT `TourId`, `Code`, `SupplierId`, `Item`, `Cost`, `Num`, `ItemCost`, `PaidCost`,
                // `Remark`, `PaymentType`, `PayTime`, `IsValid`, `Status`, `ModifiedBy`,
                // NOW(), `Currency`, `ROE`, `TravelerArray`, true FROM tptourcosts WHERE tourid = @0", tourId);

                //                    l = _costsDao.Fetch("select * from TpTourCosts where TourId=@0 and IsCopy=true", tourId);
                //                }

                return l;
            }
            else
                return list.Where(t => t.IsCopy == false).ToList();
        }

        public void SaveCost(TpTourCostModel cost)
        {
            _costDao.Insert(cost);
        }

        public void UpdateCost(TpTourCostModel cost)
        {
            _costDao.Update(cost);
        }

        #endregion 成本

        #region 附件

        public List<TourFileModel> GetFileList(string orderCode)
        {
            return _fileDao.Query("select * from TourFiles where MasterOrderCode=@0 and IsDel=0 ", orderCode).ToList();
        }

        public int AddTourFile(TourFileModel model)
        {
            return Convert.ToInt32(_fileDao.Insert(model));
        }

        public TourFileModel GetFileById(int fileid)
        {
            return _fileDao.GetById(fileid);
        }

        public int DeleteTourFile(int id)
        {
            return _fileDao.Update("set IsDel=1 where id=@0 ", id);
        }

        #endregion 附件

        public void ConfirmPay(TpOrderPayInModel payin, string code)
        {
            using (var ts = _dao.GetTransaction())
            {
                //获取订单信息 修改订单的付款金额等信息.
                var balance = GetBalanceByOrderCode(payin.OrderCode);
                decimal unPaid = balance.YingShou - balance.YiShou;  //还剩的待付款的金额.
                if (payin.Amount == unPaid)
                {
                    //完成收款
                    balance.YiShou = balance.YingShou;
                    balance.PaymentStatus = 5;//已结算
                }
                else
                {
                    //部分完成
                    balance.YiShou = balance.YiShou + payin.Amount;
                    balance.PaymentStatus = 4;//部分结算
                }
                _dao.Update(balance);

                _dao.Execute("UPDATE TpOrderPayIn set AuditBy=@1, AuditTime=now(), State=@2 WHERE ID=@0 ", payin.Id, code, 20);
                ts.Complete();
            }
        }
    }
}