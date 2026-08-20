using System;
using System.Collections.Generic;
using System.Linq;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz;

namespace Lvy.Web.Common
{
    public class ExportUtils
    {   /// <summary>
        /// 获取客户姓名
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public static string GetBookingCustomer(string customerCode, string ownerCode)
        {
            var cus = DictionaryBiz.GetCachedCustomer(customerCode, ownerCode);
            if (cus.FastCode.IsNullOrEmpty())
            {
                return cus.Name;
            }
            else
            {
                return cus.FastCode + "-" + cus.Name;
            }
        }

        /// <summary>
        /// 获取报价类型
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public static string GetPriceContents(TpOrderModel order, List<TpTravellerModel> tourists, List<TpPriceModel> tourPirces)
        {
            var objs = tourists.Where(b => b.OrderCode == order.OrderCode && b.State == 2).ToList();
            var objs2 = tourists.Where(b => b.OrderCode == order.OrderCode && b.State == 1).ToList(); // 有费用产生的退团单
            string returnValue = "";
            var groupPriceContents = from a in objs
                                     group a by new { a.PriceId, a.PriceContent, a.Price, a.TeJiaFanLi }
                                         into p
                                     select new
                                     {
                                         p.Key,
                                         Num = p.Count()
                                     };

            foreach (var content in groupPriceContents)
            {
                string priceContent = "";
                var obj = tourPirces.FirstOrDefault(a => a.Id == content.Key.PriceId);
                if (obj == null)
                    priceContent = content.Key.PriceContent;
                else
                    priceContent = obj.PriceTypeName.IsNullOrEmpty() ? obj.PriceRemark : obj.PriceTypeName;
                returnValue += "{0} {1}*{2} |".With(priceContent
                    , (content.Key.Price - content.Key.TeJiaFanLi)
                    , content.Num);
            }


            if (objs2.Count > 0)
            {
                returnValue += "已退团 {0}人 {1}".With(objs2.Count, objs2.Sum(a => a.YsPrice));
            }
            return returnValue;
        }

        /// <summary>
        /// 获取单房差
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public static string GetSingleRoom(string orderCode, List<TpTravellerModel> tourists)
        {
            //游客有效并且有单房差的
            var objs = tourists.Where(b => b.SingleRoom > 0 && b.OrderCode == orderCode && b.State == 2).ToList();
            if (objs.Count() > 0)
            {
                return objs[0].SingleRoom + "*" + objs.Count();
            }
            else
            {
                return "0";
            }
        }
        /// <summary>
        /// 获取自费
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="tourists"></param>
        /// <returns></returns>
        public static string GetZifei(string orderCode, List<TpTravellerModel> tourists)
        {
            //游客有效并且有自费的
            var objs = tourists.Where(b => b.ZiFei > 0 && b.OrderCode == orderCode && b.State == 2).ToList();

            if (objs.Count() > 0)
            {
                return objs[0].ZiFei + "*" + objs.Count();
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// 获取订单座位号 ,分隔
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="tourists"></param>
        /// <returns></returns>
        public static string GetSeats(string orderCode, List<TpTravellerModel> tourists)
        {
            string returnValue = "";
            //游客有效并且有自费的
            var objs = tourists.Where(b => b.OrderCode == orderCode && b.State == 2)
                .OrderBy(a => a.SeatNum.ToInt())
                .ToList();
            foreach (var travellerModel in objs)
            {

                returnValue += travellerModel.SeatNum + ",";
            }

            return returnValue;
        }
    }
}