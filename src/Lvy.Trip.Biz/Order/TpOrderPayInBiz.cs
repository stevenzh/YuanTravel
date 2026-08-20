using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Dao.Tour;
using Lvy.VModels.OpTour;
using Lvy.VModels.Tour;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Order
{
    public class TpOrderPayInBiz : BaseBiz
    {
        private TpOrderPayInDao _dao = new TpOrderPayInDao();
        private ViewPayInDao _viewDao = new ViewPayInDao();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<ViewPayInModel> GetPagedList(GatheringVModel vModel)
        {
            var sql = GreateSql(vModel, 1);
            return _viewDao.Pager(vModel.TourPayInList.PageIndex, vModel.TourPayInList.PageSize, sql.SQL, sql.Arguments);
        }

        public FinanceTotalModel GetFinanceSummary(GatheringVModel vModel)
        {
            var sql = GreateSql(vModel, 2);
            return _viewDao.Query<FinanceTotalModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="queryType">查询种类：1.分页列表  2.合计 </param>
        /// <returns></returns>
        public Sql GreateSql(GatheringVModel vModel, int queryType)
        {
            Sql sql = new Sql();
            if (queryType == 1) 
            {
                sql.Append(@" SELECT p.* FROM vw_payin p WHERE p.IsValid=1 AND p.OwnerCode=@0 ", vModel.OwnerCode);
            }
            else
            {
                sql.Append(@" SELECT SUM(p.amount) SumTolYsPrice, SUM(if(p.state=20, p.amount, 0 )) SumTolPaid
FROM vw_payin p WHERE p.OwnerCode=@0 AND p.IsValid=1 ", vModel.OwnerCode);
            }

            if (!vModel.Condition.LineTeam.IsNullOrEmpty())
            {
                sql.Append(" and p.TeamID=@0 ", vModel.Condition.LineTeam);
            }
            //线路名称
            if (!vModel.Condition.ProductName.IsNullOrEmpty())
                sql.Append(@" AND p.ProductName LIKE @0 ", AnsiLike(vModel.Condition.ProductName));
            //出发日期
            if (!vModel.Condition.OutDateRange.IsNullOrEmpty())
            {
                var t = vModel.Condition.OutDateRange.Split('-');
                sql.Append(@" AND p.OutDate >= @0 AND p.OutDate <= @0 ", t[0].ToDateTime(), t[1].ToDateTime());
            }
            //缴款日期
            if (!vModel.Condition.PayInTimeRange.IsNullOrEmpty())
            {
                var t = vModel.Condition.PayInTimeRange.Split('-');
                sql.Append(@" AND p.CreateTime >= @0 AND p.CreateTime<= @0 ", t[0].ToDateTime(), t[1].ToDateTime());
            }
            //分销商
            if (!vModel.Condition.BookingCustomer.IsNullOrEmpty())
                sql.Append(@" AND p.CustomerCode =@0", vModel.Condition.BookingCustomer);

            if (!vModel.Condition.SaleTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and p.SalesTeamID=@0 ", vModel.Condition.SaleTeamId);
            }
            if (!vModel.Condition.FrTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and p.FinanceCode=@0 ", vModel.Condition.FrTeamId);
            }
            if (!vModel.Condition.SalerCode.IsNullOrEmpty())
            {
                sql.Append(@" and p.SalerCode=@0 ", vModel.Condition.SalerCode);
            }
            if (!vModel.Condition.TourNo.IsNullOrEmpty())
            {
                sql.Append(@" and p.TourNo like @0 ", AnsiLike(vModel.Condition.TourNo));
            }
            if (!vModel.Condition.OrderCode.IsNullOrEmpty())
            {
                sql.Append(@" and p.OrderCode=@0 ", vModel.Condition.OrderCode);
            }
            if (!vModel.Condition.PayInId.IsNullOrEmpty())
            {
                sql.Append(@" and p.Id like @0 ", AnsiLike(vModel.Condition.PayInId));
            }
            if (!vModel.Condition.JieSuanState.IsNullOrEmpty())
            {
                sql.Append(@" and p.State=@0 ", vModel.Condition.JieSuanState);
            }
            return sql;
        }

        public int Update(TpOrderPayInModel model)
        {
            return (int)_dao.Update(model);
        }

        public void AddPayIn(TpOrderPayInModel model)
        {
            model.PayInCode = DBTools.GetSeqNo("PayIn");
            _dao.Insert(model);
        }

        public void DeletePayIn(int id, int isValid)
        {
            _dao.Update("SET IsValid=@0 WHERE ID=@0", id, isValid);
        }

        /// <summary>
        /// 获取缴款单列表
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TpOrderPayInModel> GetPayInList(string orderCode)
        {
            return _dao.Fetch(@"SELECT tpp.*, dds.`Value` AS PaymentTypeValue FROM tporderpayin tpp
INNER JOIN BaseDictionaryDetail dds ON tpp.PaymentType = dds.`Key` AND dds.Name = 'PaymentTypeEnum'
WHERE tpp.IsValid = 1 AND tpp.OrderCode=@0", orderCode);
        }

        public TpOrderPayInModel GetById(int id)
        {
            return _dao.GetById(id);
        }


        public TpOrderPayInModel GetOrderPayInModelById(int Id)
        {
            Sql sql = new Sql();
            sql.Append(@" select p.* ,o.TolYsPrice,o.TolPaid
from TpOrderPayIn p left join TpOrder o on p.OrderCode=o.OrderCode
where p.id=@0 ", Id);

            return _dao.Query(sql.SQL, sql.Arguments).FirstOrDefault();
        }


    }
}