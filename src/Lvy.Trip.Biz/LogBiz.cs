using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Base;
using Lvy.Trip.Dao.Crm;
using Lvy.Trip.Dao.Order;
using Lvy.VModels.Base;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Lvy.Trip.Biz
{
    public class LogBiz : BaseBiz
    {
        private readonly BizLogDao _dao = new BizLogDao();

        public static void WriteLog(string ownerCode, string sendTo, string teamId, string userCode, string events, int sendWeixin = 0)
        {
            BizLogModel log = new BizLogModel();
            log.OwnerCode = ownerCode;
            log.JoinCode = "";
            log.TableName = "";
            log.Event = events;
            log.Data = "";
            log.TeamID = teamId;
            log.ModifiedBy = userCode;
            log.ModifiedTime = DateTime.Now;
            log.SendTo = sendTo;
            log.SendWeixin = sendWeixin;
            log.Status = 0;

            Database db = new Database("YuanDB", "MySql.Data.MySqlClient");
            db.Insert(log);
        }

        public BizLogModel GetById(int id)
        {
            return _dao.GetById(id);
        }

        public PagedList<BizLogModel> GetPageList(LogVModel vModel)
        {
            var sql = new Sql();
            sql.Append("SELECT * FROM BizLog WHERE OwnerCode=@0", vModel.OwnerCode);

            if (!vModel.BizLog.ModifiedBy.IsNullOrEmpty())
                sql.Append(" AND ModifiedBy=@0 ", Ansi(vModel.BizLog.ModifiedBy));
            if (!vModel.BizLog.TeamID.IsNullOrEmpty())
                sql.Append(" AND TeamID=@0 ", Ansi(vModel.BizLog.TeamID));

            sql.Append(" ORDER BY ModifiedTime DESC ");

            var list = _dao.Pager(vModel.LogList.PageIndex, vModel.LogList.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        /// <summary>
        /// 插入订单日志
        /// 请在触发功能之前记录
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="events"></param>
        public static void WriteOrderLog(string OwnerCode, string orderCode, string sendTo, string userCode, string events, int sendWeixin = 0)
        {
            var order = new TpOrderDao().GetOrder(orderCode);
            var tourists = new TpTravellerDao().GetAllTravellers(order.OrderCode).Where(a => a.State != 0);

            BizLogModel log = new BizLogModel();
            log.OwnerCode = OwnerCode;
            log.JoinCode = order.OrderCode;
            log.TableName = "TpOrder";
            log.Event = events;
            log.Data = JsonSerializer.Serialize(order);
            log.Data2 = JsonSerializer.Serialize(tourists);
            log.ModifiedBy = userCode;
            log.ModifiedTime = DateTime.Now;
            log.SendTo = sendTo;
            log.SendWeixin = sendWeixin;
            log.Status = 0;

            Database db = new Database("YuanDB", "MySql.Data.MySqlClient");
            db.Insert(log);
        }

        /// <summary>
        /// 微信消息提醒
        /// </summary>
        /// <returns></returns>
        public List<BizLogModel> SendWeixinList()
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM BizLog WHERE SendWeixin=1 ");
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 微信提醒状态更新
        /// </summary>
        /// <param name="logId"></param>
        public void UpdOrderSend(long logId)
        {
            Sql sql = new Sql();
            sql.Append(" set SendWeixin=2 where Id=@0", logId);

            int row = _dao.Update(sql.SQL, sql.Arguments);
        }

        public List<BizLogModel> GetLogByUserId(string accountCode)
        {
            var sql = new Sql();
            sql.Append(@"select * from BizLog where SendTo=@0 or ModifiedBy=@0 order by ModifiedTime DESC LIMIT 8 ", accountCode);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="customerCode">客户编码</param>
        /// <param name="sendTo">消息发送人</param>
        /// <param name="userCode">操作人</param>
        /// <param name="events">操作内容</param>
        /// <param name="sendWeixin">是否发送微信</param>
        public static void WriteCustomerLog(string ownerCode, string customerCode, string sendTo, string teamId, string userCode, string events, int sendWeixin = 0)
        {
            var customer = new CustomerDao().GetById(customerCode);

            BizLogModel log = new BizLogModel();
            log.OwnerCode = ownerCode;
            log.JoinCode = customer.Code;
            log.TableName = "CrmCustomer";
            log.Event = events;
            log.Data = JsonSerializer.Serialize(customer);
            //log.Data2 = tourists.ToJsonSerialize();
            log.TeamID = teamId;
            log.ModifiedBy = userCode;
            log.ModifiedTime = DateTime.Now;
            log.SendTo = sendTo;
            log.SendWeixin = sendWeixin;
            log.Status = 0;

            Database db = new Database("YuanDB", "MySql.Data.MySqlClient");
            db.Insert(log);
        }

        public static void WriteConttactLog(string ownerCode, string contactCode, string sendTo, string teamId, string userCode, string events, int sendWeixin = 0)
        {
            var contact = new AccountDao().GetById(contactCode);

            BizLogModel log = new BizLogModel();
            log.OwnerCode = ownerCode;
            log.JoinCode = contact.Code;
            log.TableName = "CrmCustomer";
            log.Event = events;
            log.Data = JsonSerializer.Serialize(contact);
            //log.Data2 = tourists.ToJsonSerialize();
            log.TeamID = teamId;
            log.ModifiedBy = userCode;
            log.ModifiedTime = DateTime.Now;
            log.SendTo = sendTo;
            log.SendWeixin = sendWeixin;
            log.Status = 0;

            Database db = new Database("YuanDB", "MySql.Data.MySqlClient");
            db.Insert(log);
        }

        public List<BizLogModel> GetOrderLog(string orderCode)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM BizLog WHERE TableName='TpOrder' and JoinCode=@0 ", orderCode);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public PagedList<BizLogModel> GetPolicyList(long pageIndex, long pageSize, string code)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM BizLog WHERE TableName='CrmCustomer' and JoinCode=@0 ", code);
            return _dao.Pager(pageIndex, pageSize, sql.SQL, sql.Arguments);
        }
    }
}