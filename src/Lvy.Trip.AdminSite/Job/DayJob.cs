using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Quartz;
using log4net;
using Lvy.Trip.Dao.Crm;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Order;
using PetaPoco;

namespace Lvy.Trip.Job
{
    /// <summary>
    /// 每天执行一次
    /// </summary>
    public class DayJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DayJob));
        private static readonly string AdminUserCode = "1610000002";
        private static readonly string AdminTeamId = "1812000015";
        private static readonly CustomerDao _dao = new CustomerDao();
        private AccountBiz _accountBiz = new AccountBiz();
        private OrderBiz _orderBiz = new OrderBiz();

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("DayJob running...");

#if DEBUG
            logger.Info("DayJob Debug ");
#else

            // 找到三个月有订单客户  状态变更的提醒
            Sql sql = new Sql();
            sql.Append(@"select * from CrmCustomer where IsDistributors=1 and IsValid=1 and Code in(
select BookingCustomer from TpOrder where BookingCustomer is not null and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -3 MONTH))");
            var list = _dao.Query(sql.SQL, sql.Arguments).ToList();
            foreach (var cust in list)
            {
                // 取得近三个月的开单数量
                sql = new Sql();
                sql.Append(@"select cc.CreatedTime, a.num as OneMonth, b.num as TwoMonth, c.num as ThreeMonth from CrmCustomer cc,
(select COUNT(Id) as num from TpOrder where BookingCustomer=@0 and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -1 MONTH)) a ,
(select COUNT(Id) as num from TpOrder where BookingCustomer=@0 and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -2 MONTH) and CreatedTime < date_add(now(), INTERVAL -1 MONTH)) b,
(select COUNT(Id) as num from TpOrder where BookingCustomer=@0 and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -3 MONTH) and CreatedTime < date_add(now(), INTERVAL -2 MONTH)) c 
where Code = @0", cust.Code);

                var num = _dao.Query<NumModel>(sql.SQL, sql.Arguments).FirstOrDefault();

                // 0 高度活跃 1活跃 2 普通客户 3 沉睡客户

                // 创建时间超过
                if (cust.CreatedTime.AddMonths(3) < DateTime.Today)
                {
                    // 近三个月的都有开单
                    if (num.OneMonth > 0 && num.TwoMonth > 0 && num.ThreeMonth > 0)
                    {
                        //活跃
                        if (num.OneMonth + num.TwoMonth + num.ThreeMonth > 5 && cust.ActiveState != 0)
                        {
                            UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 0);
                        }
                        else if (cust.ActiveState != 1)
                        {
                            UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 1);
                        }
                    }
                    else if (cust.ActiveState != 2)
                    {
                        //普通
                        UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 2);
                    }
                }
                else if (cust.CreatedTime.AddMonths(2) < DateTime.Today && cust.CreatedTime.AddMonths(3) > DateTime.Today)
                {
                    // 创建满两个月
                    if (num.OneMonth > 0 && num.TwoMonth > 0)
                    {
                        // 活跃
                        if (num.OneMonth + num.TwoMonth > 3 && cust.ActiveState != 0)
                        {
                            UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 0);
                        }
                        else if (cust.ActiveState != 1)
                        {
                            UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 1);
                        }
                    }
                    else if (cust.ActiveState != 2)
                    {
                        // 某一个月没单
                        // 普通
                        UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 2);
                    }
                }
                else if (cust.CreatedTime.AddMonths(1) < DateTime.Today && cust.CreatedTime.AddMonths(2) > DateTime.Today)
                {
                    // 创建时间满一个月
                    if (num.OneMonth > 1 && cust.ActiveState != 0)
                    {
                        // 非常活跃
                        UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 0);
                    }
                    else if (cust.ActiveState != 1)
                    {
                        // 只有一单 设置 活跃
                        UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 1);
                    }
                }
            }


            // 满一个月未开单只通知一次
            sql = new Sql();
            sql.Append(@"select * from CrmCustomer where IsDistributors=1 and IsValid=1 and EmptyInMonth=0 and CreatedTime< date_add(now(), INTERVAL -1 MONTH) and Code not in(
select BookingCustomer from TpOrder where BookingCustomer is not null and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -1 MONTH))");
            var list1 = _dao.Query(sql.SQL, sql.Arguments).ToList();
            foreach (var cust in list1)
            {
                // 状态改变 通知销售
                // _dao.Execute("update CrmCustomer set EmptyInMonth=1 where Code=@0 ", cust.Code);
                // 记录日志
                // LogBiz.WriteCustomerLog(cust.Code, cust.TeamID, cust.SalerCode, AdminUserCode, "一个月未开单提醒", 1);
            }


            // 近一个月有单 变更状态
            sql = new Sql();
            sql.Append(@"select * from CrmCustomer where IsDistributors=1 and IsValid=1 and EmptyInMonth=1 and Code in(
select BookingCustomer from TpOrder where BookingCustomer is not null and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -1 MONTH))");
            var list2 = _dao.Query(sql.SQL, sql.Arguments).ToList();
            foreach (var cust in list2)
            {
                // 状态改变 通知销售
                // _dao.Execute("update CrmCustomer set EmptyInMonth=0 where Code=@0 ", cust.Code);
            }

            // 三个月没订单的活跃客户
            sql = new Sql();
            sql.Append(@"select * from CrmCustomer where IsDistributors=1 and IsValid=1 and ActiveState<3 and Code not in(
select BookingCustomer from TpOrder where BookingCustomer is not null and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -3 MONTH))");
            var list3 = _dao.Query(sql.SQL, sql.Arguments).ToList();
            foreach (var cust in list3)
            {
                UpdateStatus(cust.SalerCode, cust.Code, cust.Name, 3);

                // 认领期多余一个月 转入公共区域
                if (cust.ReceiveDate > DateTime.Now.AddMonths(-1) && cust.InPublic == false)
                {
                    // UpdatePublic(cust.SalerCode, cust.Code, cust.Name, 1);
                    // LogBiz.WriteCustomerLog(cust.Code, cust.TeamID, cust.SalerCode, "", "三个月未开单，转入公共区域。");
                }
            }

            // 认领期一个月仍没开单的客户  转入公共区
            sql = new Sql();
            sql.Append(@"select * from CrmCustomer where IsDistributors=1 and IsValid=1 and InPublic=0 
and ReceiveDate < DATE_ADD(now(), INTERVAL -1 MONTH)
and Code not in(select BookingCustomer from TpOrder where BookingCustomer is not null and OrderState=2 and isCancel=0 and CreatedTime > date_add(now(), INTERVAL -1 MONTH))");
            var list4 = _dao.Query(sql.SQL, sql.Arguments).ToList();
            foreach (var cust in list4)
            {
                // UpdatePublic(cust.SalerCode, cust.Code, cust.Name, 1);
                // LogBiz.WriteCustomerLog(cust.Code, cust.TeamID, cust.SalerCode, "", "领用后一个月仍未开单，转入公共区域。");
            }



            // 订单催款  销售三天内付款订单合计
            var debt = _orderBiz.GetDebtBySales();
            foreach (var cust in debt)
            {
                // 记录日志  1812000015:网络部
                // LogBiz.WriteLog(cust.SalerCode, AdminTeamId, AdminUserCode, "当前应付款订单：" + cust.OrderNum + ",欠款金额：" + cust.Amount, 1);
            }
#endif

            logger.Info("DayJob run finished.");
        }

        public void UpdateStatus(string salerCode, string customerCode, string customerName, int newStatus)
        {
            logger.Info("更新状态 UpdateStatus， CustomerCode:" + customerCode + ", NewStatus:" + newStatus);

            // 状态改变 通知销售
            _dao.Execute("update CrmCustomer set ActiveState=@0 where Code=@1 ", newStatus, customerCode);

            //发送消息
            //var sales = _accountBiz.GetAllAccount(salerCode).FirstOrDefault();
            //SendMessagClient.SendTemplateMessage(sales.OpenID, "", customerName, newStatus.ToString(), "", "", "", "");
        }

        /// <summary>
        /// 转入公共区域
        /// </summary>
        /// <param name="salerCode"></param>
        /// <param name="customerCode"></param>
        /// <param name="customerName"></param>
        /// <param name="state"></param>
        public void UpdatePublic(string salerCode, string customerCode, string customerName, int state)
        {
            logger.Info("更新状态 UpdateStatus， CustomerCode:" + customerCode + ", NewStatus:" + state);

            // 状态改变 通知销售
            _dao.Execute("update CrmCustomer set InPublic=@0, SalerCode=null where Code=@1 ", state, customerCode);

            //发送消息
            //var sales = _accountBiz.GetAllAccount(salerCode).FirstOrDefault();
            //SendMessagClient.SendTemplateMessage(sales.OpenID, "", customerName, newStatus.ToString(), "", "", "", "");
        }

    }



    public class NumModel
    {
        public DateTime CreatedTime { get; set; }
        public int OneMonth { get; set; }
        public int TwoMonth { get; set; }
        public int ThreeMonth { get; set; }

    }
}