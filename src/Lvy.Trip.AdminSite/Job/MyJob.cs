using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Quartz;
using log4net;
using Lvy.Trip.Biz.Weixin;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz;
using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Common;
using System.Threading.Tasks;

namespace Lvy.Trip.Job
{
    /// <summary>
    /// 五分钟执行一次
    /// </summary>
    public class MyJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MyJob));

        private MemberBiz memberBiz = new MemberBiz();
        private LogBiz logBiz = new LogBiz();
        private OrderBiz orderBiz = new OrderBiz();

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("MyJob running...");

#if DEBUG
            logger.Info("MyJob DEBUG ");
#else
            // 需要发送微信的日志
            List<BizLogModel> list = logBiz.SendWeixinList();
            foreach (var log in list)
            {
                if (log.TableName == "TpOrder" && log.JoinCode.IsNullOrEmpty())
                {
                    var order = orderBiz.GetOrderLineTourist(log.JoinCode);
                    var member = memberBiz.GetMemberByAccount(log.SendTo);
                    if (member == null)
                        continue;

                    if (!string.IsNullOrEmpty(member.OpenID))
                    {
                        //logger.Info("sales openid:" + openid);
                        string first = "客户【" + order.BookingCustomer + "】新订单，联系人" + order.Managers + "。";
                        string remark = "人数" + order.TravellerCount + ",团款(参考)：" + order.TolYsPrice;

                        string result = SendMessagClient.SendTemplateMessage(member.OpenID, "jFkZkkv74K27HcZ6xnyaNV5elqSX7IdcYQHI4Nus170", first, order.OrderCode, order.Line.LineName, "价格", "状态", "", remark);

                        if (result == "1")
                        {
                            // 更新数据库
                            logBiz.UpdOrderSend(log.Id);
                        }
                    }
                }
            }
#endif

            logger.Info("MyJob run finished.");
        }

    }

}