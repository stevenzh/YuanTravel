using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Dao.Tour;
using Lvy.VModels.Finance;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Finance
{
    public class InvoiceBiz : BaseBiz
    {
        private TpInvoiceDao _dao = new TpInvoiceDao();
        private ViewInvoiceDao _viewDao = new ViewInvoiceDao();

        /// <summary>
        /// 发票纪录
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TpInvoiceModel> GetInvoiceList(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append("SELECT * FROM TpInvoices WHERE IsValid=1 AND orderCode=@0", orderCode);

            return _dao.Query(sql.SQL, sql.Arguments).ToList();
        }

        public object AddInvoice(TpInvoiceModel model)
        {
            return _dao.Insert(model);
        }

        public TpInvoiceModel GetInvoiceById(int id)
        {
            return _dao.GetById(id);
        }

        public PagedList<ViewInvoiceModel> GetPageList(InvoiceVModel vModel)
        {
            var sql = new Sql();
            sql.Append(@" select tt.* from vw_invoice tt WHERE tt.OwnerCode=@0 AND tt.IsValid = 1 ", vModel.OwnerCode);

            if (!vModel.TeamCode.IsNullOrEmpty())
                sql.Append(@" AND tt.SalesTeamId = @0 ", Ansi(vModel.TeamCode));
            if (!vModel.SalerCode.IsNullOrEmpty())
                sql.Append(@" AND tt.SalerCode = @0 ", Ansi(vModel.SalerCode));
            if (!vModel.StartDate.IsNullOrEmpty())
                sql.Append(@" AND tt.OutDate >= @0 ", vModel.StartDate.ToDateTime());
            if (!vModel.EndDate.IsNullOrEmpty())
                sql.Append(@" AND tt.OutDate <= @0 ", vModel.EndDate.ToDateTime());
            if (!vModel.OrderCode.IsNullOrEmpty())
                sql.Append(@" AND tt.OrderCode = @0 ", vModel.OrderCode);
            if (!vModel.InvoiceNo.IsNullOrEmpty())
                sql.Append(@" AND tt.InvoiceNo = @0 ", vModel.InvoiceNo);

            sql.Append(" order by tt.Id ");

            var list = _viewDao.Pager(vModel.InvoicePageList.PageIndex, vModel.InvoicePageList.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        public int Update(TpInvoiceModel model)
        {
            var entity = _dao.GetById(model.Id);
            if (entity != null)
            {
                entity.Phone = model.Phone;
                entity.Remark = model.Remark;
                entity.Address = model.Address;
                entity.Amount = model.Amount;
                entity.BankAccount = model.BankAccount;
                entity.BankName = model.BankName;
                entity.CustomerName = model.CustomerName;
                entity.ServiceItems = model.ServiceItems;
                entity.TaxNumber = model.TaxNumber;
                entity.TourNo = model.TourNo;

                _dao.Update(entity);
            }
            return 1;
        }

        public int SetValid(int id, int isValid)
        {
            return _dao.Update("SET IsValid=@1 WHERE Id=@0", id, isValid);
        }

        public int CheckInvoice(TpInvoiceModel model)
        {
            var entity = _dao.GetById(model.Id);
            if (entity != null)
            {
                entity.InvoiceNo = model.InvoiceNo;
                entity.CheckedBy = model.CheckedBy;
                entity.CheckedTime = DateTime.Now;
                entity.State = 1;
                _dao.Update(entity);
            }
            return 1;
        }

    }
}