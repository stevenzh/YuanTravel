using Arch.Common;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Financial;
using Lvy.VModels.Finance;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Finance
{
    /// <summary>
    /// 独立模块  财务收款
    /// </summary>
    public class VTProceedsBiz : BaseBiz
    {
        private readonly ProceedsDao _costsDao = new ProceedsDao();

        /// <summary>
        /// <summary>
        /// 添加添加缴款单
        /// </summary>
        /// <returns></returns>
        public int AddCollection(VTProceedsModel model)
        {
            return _costsDao.Insert(model).ToInt();
        }

        /// <summary>
        /// 根据页面条件获取缴款单
        /// </summary>
        /// <param name="qModel"></param>
        /// <returns></returns>
        public PagedList<VTProceedsModel> GetQueryPayIns(ProceedsSearchQModel qModel)
        {
            var sql = new Sql();
            sql.Append(@" select * from VT_Proceeds where IsValid = 1");

            if (qModel != null)
            {
                if (!qModel.ProceedsCode.IsNullOrEmpty())
                {
                    sql.Append(@" AND ProceedsCode = @0 ", qModel.ProceedsCode.ToTrim());
                }
                if (!qModel.CollectedDateFrom.IsNullOrEmpty())
                {
                    sql.Append(@" AND ProceedsDate > @0 ", qModel.CollectedDateFrom);
                }
                if (!qModel.CollectedDateTo.IsNullOrEmpty())
                {
                    sql.Append(@" AND ProceedsDate <= @0 ", qModel.CollectedDateTo);
                }
                if (!qModel.ChargerName.IsNullOrEmpty())
                {
                    sql.Append(@" AND ChargerName LIKE @0 ", AnsiLike(qModel.ChargerName));
                }
                if (!qModel.ChargerDept.IsNullOrEmpty())
                {
                    sql.Append(@" AND ChargerDept == @0 ", qModel.ChargerDept);
                }
                if (!qModel.ChargerHost.IsNullOrEmpty())
                {
                    sql.Append(@" AND ChargerHost LIKE @0 ", AnsiLike(qModel.ChargerHost));
                }

                sql.Append(@" ORDER BY ProceedsDate DESC ");
            }

            return _costsDao.Pager(qModel.ProceedsPageList.PageIndex, qModel.ProceedsPageList.PageSize, sql.SQL, sql.Arguments);
        }

        public List<VTProceedsModel> GetQueryProceeds(ProceedsSearchQModel qModel)
        {
            var sql = new Sql();
            sql.Append(@" select * from VT_Proceeds where IsValid = 1");

            if (qModel != null)
            {
                if (!qModel.ProceedsCode.IsNullOrEmpty())
                {
                    sql.Append(@" AND ProceedsCode = @0 ", qModel.ProceedsCode.ToTrim());
                }
                if (!qModel.CollectedDateFrom.IsNullOrEmpty())
                {
                    sql.Append(@" AND ProceedsDate > @0 ", qModel.CollectedDateFrom);
                }
                if (!qModel.CollectedDateTo.IsNullOrEmpty())
                {
                    sql.Append(@" AND ProceedsDate <= @0 ", qModel.CollectedDateTo);
                }
                if (!qModel.ChargerName.IsNullOrEmpty())
                {
                    sql.Append(@" AND B.ChargerName LIKE @0 ", AnsiLike(qModel.ChargerName));
                }
                if (!qModel.ChargerDept.IsNullOrEmpty())
                {
                    sql.Append(@" AND ChargerDept == @0 ", qModel.ChargerDept);
                }
                if (!qModel.ChargerHost.IsNullOrEmpty())
                {
                    sql.Append(@" AND B.ChargerHost LIKE @0 ", AnsiLike(qModel.ChargerHost));
                }
            }

            return _costsDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 修改缴款状态
        /// </summary>
        /// <param name="model"></param>
        public bool UpdatePaymentStatu(long model)
        {
            _costsDao.Update(@"SET IsReceive=1 WHERE Id=@0", model);
            return true;
        }

        /// <summary>
        /// 根据编号获取缴款单信息
        /// </summary>
        /// <param name="travellerCode"></param>
        /// <returns></returns>
        public VTProceedsModel GetPayInModel(string payInCode)
        {
            Sql sql = new Sql();
            sql.Append(" select * from VT_Proceeds where ProceedsCode=@0 ", Ansi(payInCode));

            var result = _costsDao.FirstOrDefault(sql.SQL, sql.Arguments);

            if (result != null)
                result.ReceiveSumChina = Toolkit.Rmb.ToDaxie(result.ReceiveSum.Value);

            return result;
        }
    }
}