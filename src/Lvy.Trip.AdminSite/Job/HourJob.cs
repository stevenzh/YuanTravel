using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Quartz;
using log4net;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Common;
using System.Threading.Tasks;
using PetaPoco;

namespace Lvy.Trip.Job
{
    /// <summary>
    /// 每小时一次
    /// </summary>
    public class HourJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(HourJob));
        private static readonly string AdminUserCode = "1610000002";
        private OrderBiz _orderBiz = new OrderBiz();
        private TpLineAdminBiz _lineAdminBiz = new TpLineAdminBiz();
        private AccountBiz _accountBiz = new AccountBiz();
        private TpOrderDao _ordersDao = new TpOrderDao();
        private HotelBiz _hotelBiz = new HotelBiz();

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("HourJob running...");
#if DEBUG
            logger.Info("HourJob Debug ");
#else

            // 查找到期的订单
            var sql = new Sql();
            sql.Append(@"SELECT Id,OrderCode,TourId,LineId,SalerCode,TravellerCount FROM TpOrder WHERE  
 DATE_ADD(CreatedTime, INTERVAL IFNULL(EffectiveHour, 0) HOUR)<now() AND OrderState=1 AND IsCancel=0 AND TraceState<40 ", "");

            var list = _ordersDao.Fetch(sql.SQL, sql.Arguments);
            foreach (var order in list)
            {
                // 更新订单
                _ordersDao.Update(" set IsCancel=1,InvoiceAmount=0,TolYsPrice=0 where Id=@0", order.Id);
                // 检查库存
                _orderBiz.FreeQuota(order.TourId, "", AdminUserCode);

                // 发消息给OP
                var op = _lineAdminBiz.GetAccountByLineId(order.LineId);


                // 发消息销售
                var sales = _accountBiz.GetAllAccount(order.SalerCode).FirstOrDefault();
                if (!string.IsNullOrEmpty(sales.OpenID))
                {
                    var first = string.Format("{0}您好,订单资料未补全，现已取消，有问题联系计调。", sales.Name);
                    var remark = string.Format(@"客户名称：{0}
出团日期：{1}", order.CustomerName, order.OutDate.ToDateFormat());
                    SendMessagClient.SendTemplateMessage(sales.OpenID, "8i7VY_GnnYnvTfmDRmntS079TzfJK2KmXV3LUOeOHM0", first, order.OrderCode, order.LineName, "价格", "状态", "", remark);

                }


                // 酒店起价更新
                _hotelBiz.UpdateSalePrice();

            }
#endif
            logger.Info("HourJob run finished.");
        }

    }

}