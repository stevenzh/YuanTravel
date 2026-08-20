using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Order;
using Lvy.VModels.Excel;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.Trip.Biz
{
    /// <summary>
    /// 报表
    /// </summary>
    public class ReportsBiz : BaseBiz
    {
        public static List<ZhangLingVModel> GetZhangLing(string month, string ownerCode)
        {
            //string sql = @"select BookingCustomer, SUM(TolYsPrice - TolPaid) as TolYsPrice, DATE_FORMAT(OutDate, '%m%d') as outdate from TpOrder
            //        where IsCancel=0 and TolYsPrice - TolPaid > 0
            //        and OutDate <= @0  and OwnerCode = @1
            //        group by BookingCustomer, DATE_FORMAT(OutDate, '%m%d')
            //        order by BookingCustomer";

            //当前年月
            DateTime currentMonth = (month + "-01").ToDateTime();
            DateTime payInMonth = (month + "-01").ToDateTime().AddMonths(1);

            Sql sql3 = new Sql();
            sql3.Append(@"SELECT c.BookingCustomer, SUM(c.TolYsPrice - IFNULL(c.amount, 0)) AS TolYsPrice,
c.OutDate FROM (
SELECT a.*, (SELECT SUM(b.amount) FROM TpOrderPayIn b WHERE b.OrderCode = a.OrderCode AND b.PayInTime < @0 ) AS amount
 FROM TpOrder a
WHERE a.IsCancel = 0
AND a.OutDate <= @1 AND OwnerCode = @2 ) c
WHERE c.TolYsPrice - IFNULL(c.amount, 0) > 0
GROUP BY c.BookingCustomer, c.outdate
ORDER BY c.BookingCustomer ", payInMonth, currentMonth, ownerCode);

            var dao = new TpOrderDao();
            var obj = dao.Fetch(sql3.SQL, sql3.Arguments);

            string sql2 = @"SELECT BookingCustomer FROM TpOrder
WHERE IsCancel = 0 AND TolYsPrice - TolPaid > 0 AND OwnerCode = @0
GROUP BY BookingCustomer";
            //获取分销商
            var customers = dao.Query<string>(sql2, ownerCode).ToList();

            ZhangLingVModel vModel = null;

            var vModels = new List<ZhangLingVModel>();
            foreach (var cus in customers)
            {
                vModel = new ZhangLingVModel();
                vModel.BookCustomer = DictionaryBiz.GetCachedCustomer(cus, ownerCode).Name;

                var current = obj.Where(a => a.BookingCustomer == cus);

                decimal half = 0;
                decimal yearup = 0;
                foreach (var model in current)
                {
                    var date1 = string.Concat(model.OutDate, "-01").ToDateTime();
                    var date2 = string.Concat(currentMonth, "-01").ToDateTime();
                    int m = date2.CalcMonthDiff(date1);

                    if (m > 6 && m <= 12)
                    {
                        half += model.TolYsPrice;
                    }
                    if (m > 12)
                    {
                        yearup += model.TolYsPrice;
                    }

                    switch (m)
                    {
                        case 1:
                            vModel.Month1 += model.TolYsPrice;
                            break;

                        case 2:
                            vModel.Month2 += model.TolYsPrice;
                            break;

                        case 3:
                            vModel.Month3 += model.TolYsPrice;
                            break;

                        case 4:
                            vModel.Month4 += model.TolYsPrice;
                            break;

                        case 5:
                            vModel.Month5 += model.TolYsPrice;
                            break;

                        case 6:
                            vModel.Month6 += model.TolYsPrice;
                            break;
                    }
                }
                vModel.HalfYear = half;
                vModel.YearUp = yearup;

                vModels.Add(vModel);
            }
            return vModels;
        }

        public static List<PayInVModel> GetShoukeInfo1(string sDate, string eDate, string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append("select o.SupplierCode, COUNT(1) as num,b.PriceType as PriceType")
                .Append(" from TpTraveller a left join TpPrice b on a.PriceId=b.Id")
                .Append(" left join TpOrder o on a.OrderCode=o.OrderCode")
                .Append(" where a.State=2 and o.IsCancel=0 and OwnerCode=@0", ownerCode);
            if (!sDate.IsNullOrEmpty())
            {
                sql.Append(" and o.OutDate>=@0  ", sDate.ToDateTime());
            }
            if (!eDate.IsNullOrEmpty())
            {
                sql.Append(" and o.Outdate<=@0 ", eDate.ToDateTime());
            }

            sql.Append(" group by o.SupplierCode,b.PriceType");
            sql.Append("  order by num desc");
            var models = new TpOrderDao().Query<ReportTempMdel>(sql.SQL, sql.Arguments).ToList();

            string sql1 = @"select SupplierCode from TpOrder where IsCancel=0 and OwnerCode=@0 group by SupplierCode";
            var suppliers = new TpOrderDao().Query<string>(sql1, ownerCode).ToList();

            var returnModels = new List<PayInVModel>();
            PayInVModel returnModel = null;

            foreach (var supplier in suppliers)
            {
                var temps = models.Where(a => a.SupplierCode == supplier);
                returnModel = new PayInVModel();
                returnModel.Supplier = DictionaryBiz.GetCachedCustomer(supplier, ownerCode).Name;
                foreach (var model in temps)
                {
                    if (model.PriceType == 1)
                    {
                        returnModel.PriceType1 += model.num;
                    }
                    else if (model.PriceType == 2)
                    {
                        returnModel.PriceType2 += model.num;
                    }
                    else if (model.PriceType == 3)
                    {
                        returnModel.PriceType3 += model.num;
                    }
                    else if (model.PriceType == 4)
                    {
                        returnModel.PriceType4 += model.num;
                    }
                    else if (model.PriceType == 5)
                    {
                        returnModel.PriceType5 += model.num;
                    }
                    else
                    {
                        returnModel.PriceType6 += model.num;
                    }
                }
                returnModels.Add(returnModel);
            }

            return returnModels;
        }

        public static List<PayInInfoVModel> GetShoukeInfo2(string sDate, string eDate, string customerCode, string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT o.BookingCustomer, COUNT(1) AS num, b.PriceType AS PriceType, t.LineName
FROM TpTraveller a LEFT JOIN TpPrice b ON a.PriceId=b.Id
 LEFT JOIN TpOrder o ON a.OrderCode=o.OrderCode
 INNER JOIN TpLine t ON o.LineId=t.LineId
WHERE a.State=2 AND o.IsCancel=0 AND o.OwnerCode=@0 ", ownerCode);
            if (!sDate.IsNullOrEmpty())
            {
                sql.Append(" AND o.OutDate>=@0 ", sDate.ToDateTime());
            }
            if (!eDate.IsNullOrEmpty())
            {
                sql.Append(" AND o.Outdate<=@0 ", eDate.ToDateTime());
            }
            if (!customerCode.IsNullOrEmpty())
            {
                sql.Append(" AND o.SupplierCode=@0 ", customerCode);
            }

            sql.Append(" GROUP BY o.BookingCustomer, b.PriceType, t.LineName ");
            sql.Append(" ORDER BY num DESC ");

            var models = new TpOrderDao().Query<dynamic>(sql.SQL, sql.Arguments).ToList();

            string sql1 = @"select BookingCustomer from TpOrder where IsCancel=0 and OwnerCode=@0 and BookingCustomer is not null group by BookingCustomer";
            var BookingCustomers = new TpOrderDao().Query<string>(sql1, ownerCode).ToList();

            var returnModels = new List<PayInInfoVModel>();
            PayInInfoVModel returnModel = null;

            foreach (var cus in BookingCustomers)
            {
                var temps = models.Where(a => a.BookingCustomer == cus);
                returnModel = new PayInInfoVModel();
                //returnModel.Supplier = DictionaryTools.GetCachedCustomer(cus).Name;
                var obj = DictionaryBiz.GetCachedCustomer(cus, ownerCode);
                returnModel.BookingCustomer = obj.Name;
                returnModel.Address = obj.Address;
                returnModel.LinkMan = obj.Head;
                returnModel.LinkPhone = obj.Phone;

                foreach (var model in temps)
                {
                    if (model.PriceType == 1)
                    {
                        string linename = model.LineName;
                        returnModel.PriceType1 += model.num;
                    }
                    else if (model.PriceType == 2)
                    {
                        returnModel.PriceType2 += model.num;
                    }
                    else if (model.PriceType == 3)
                    {
                        returnModel.PriceType3 += model.num;
                    }
                    else if (model.PriceType == 4)
                    {
                        returnModel.PriceType4 += model.num;
                    }
                    else if (model.PriceType == 5)
                    {
                        returnModel.PriceType5 += model.num;
                    }
                    else
                    {
                        returnModel.PriceType6 += model.num;
                    }
                }
                returnModels.Add(returnModel);
            }

            return returnModels;
        }

        public static List<OrderReportByDateVModel> GetOrderReportByDate(string sDate, string eDate, CrmAccountModel userInfo)
        {
            Sql sql = new Sql();

            sql.Append("select OutDate, SUM(TravellerCount) as allPax from TpOrder  ");
            sql.Append(" where OwnerCode =@0 and IsCancel=0  ", userInfo.OwnerCode);
            if (!sDate.IsNullOrEmpty())
            {
                sql.Append(" and OutDate>=@0  ", sDate);
            }
            if (!eDate.IsNullOrEmpty())
            {
                sql.Append(" and Outdate<=@0 ", eDate);
            }

            sql.Append("    group by OutDate");

            var dao = new TpOrderDao();
            var alls = dao.Query<OrderReportByDateVModel>(sql.SQL, sql.Arguments).ToList();

            StringBuilder sql1 = new StringBuilder();

            sql1.Append(@"select t.OutDate, SUM(t.TravellerCount) as allPax
from TpOrder t INNER JOIN TpLine tl 
 where t.OwnerCode =@0 and t.IsCancel=0 and tl.LineName like @1 ");
            if (!sDate.IsNullOrEmpty())
            {
                sql1.Append(" and OutDate>={0}  ".With(sDate));
            }
            if (!eDate.IsNullOrEmpty())
            {
                sql1.Append(" and Outdate<={0} ".With(eDate));
            }

            sql1.Append("    group by OutDate");

            foreach (var vModel in alls)
            {
                vModel.OtherPax = vModel.AllPax;
                vModel.WeekCn = vModel.OutDate.ToDateTime().DayOfWeek.ToWeekCn();
            }
            return alls;
        }
    }

    public class ReportTempMdel
    {

        public string SupplierCode { get; set; }

        public int num { get; set; }

        public int PriceType { get; set; }

    }
}