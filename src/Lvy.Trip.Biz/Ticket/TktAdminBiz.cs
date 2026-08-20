using Lvy.Models.CrmDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Dao.Ticket;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Ticket
{
    public class TktAdminBiz : BaseBiz
    {
        private readonly TktAdminDao _dao = new TktAdminDao();
        private TktProductDao productDao = new TktProductDao();
        private AccountBiz accountBiz = new AccountBiz();

        /// <summary>
        /// 根据门票Id获取专管员列表
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public List<TktAdminModel> GetByTicketId(string productID)
        {
            return _dao.Fetch(@"SELECT * FROM TktAdmin WHERE ProductId = @0", productID);
        }

        /// <summary>
        /// 根据门票订单号获取专管员列表
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TktAdminModel> GetByTicketOrderCode(string orderCode)
        {
            return _dao.Fetch(@"SELECT * FROM TktAdmin WHERE ProductID IN (SELECT ProductId FROM TktOrders WHERE MasterOrderCode = @0)", orderCode);
        }

        /// <summary>
        /// 获取专管员编辑对象
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public EditTicketAdminVModel GetEditLineAdminVModel(string productId, CrmAccountModel userInfo)
        {
            var vModel = new EditTicketAdminVModel
            {
                TicketId = productId,
                Admins = new List<TicketAdminVModel>()
            };
            var ticket = productDao.GetByProductId(productId);
            var ticketAdmins = GetByTicketId(productId);

            List<CrmAccountModel> accountList = accountBiz.GetAccountByTeam(userInfo.CustomerCode, ticket.TeamID);

            foreach (var account in accountList)
            {
                var ticketAdmin = ticketAdmins.Find(p => p.AccountCode == account.Code);
                vModel.Admins.Add(new TicketAdminVModel
                {
                    Checked = ticketAdmin == null ? 0 : 1,
                    AccountCode = account.Code,
                    Name = account.Name,
                    TktAdminId = ticketAdmin == null ? 0 : ticketAdmin.Id
                });
            }

            return vModel;
        }

        /// <summary>
        /// 保存线路专管员
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public string SaveTicketAdmin(EditTicketAdminVModel vModel)
        {
            if (vModel.Admins.FindAll(p => p.Checked == 1).Count == 0)
                return "error";
            var ticketId = vModel.TicketId;
            var admins = GetByTicketId(ticketId);
            var add = new List<TktAdminModel>();
            var update = new List<TktAdminModel>();
            var delete = new List<TktAdminModel>();
            foreach (var admin in vModel.Admins)
            {
                if (admin.Checked == 1 && admin.TktAdminId == 0)
                {
                    add.Add(new TktAdminModel
                    {
                        AccountCode = admin.AccountCode,
                        ProductId = ticketId
                    });
                }
                //else if (admin.Checked == 1 && admin.TktAdminId > 0)
                //{
                //    var model = admins.SingleOrDefault(p => p.Id == admin.TktAdminId);
                //    if (null != model)
                //    {
                //        update.Add(model);
                //    }
                //}
                else if (admin.Checked == 0 && admin.TktAdminId > 0)
                {
                    var model = admins.SingleOrDefault(p => p.Id == admin.TktAdminId);
                    if (null != model)
                        delete.Add(model);
                }
            }

            using (var scope = new TransactionScope())
            {
                foreach (var a in add)
                {
                    _dao.Insert(a);
                }
                //foreach (var u in update)
                //{
                //    _dao.Update(u);
                //}
                foreach (var d in delete)
                {
                    _dao.Delete(d);
                }
                scope.Complete();
            }
            return "success";
        }
    }
}