using Lvy.Models.CrmDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Dao.Ticket;
using Lvy.VModels.Ticket;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Ticket
{
    public class TktTaskOrderBiz : BaseBiz
    {
        private readonly TktTaskOrderDao _dao = new TktTaskOrderDao();

        public TktTaskOrderModel GetById(int id)
        {
            return _dao.GetById(id);
        }

        public TktTaskOrderModel GetByCode(string orderCode)
        {
            return _dao.SingleOrDefault(@"SELECT * FROM TktTaskOrder WHERE MasterOrderCode = @0", orderCode);
        }

        public List<TktTaskOrderModel> GetListByCode(string orderCode)
        {
            return _dao.Fetch(@"SELECT * FROM TktTaskOrder WHERE MasterOrderCode = @0", orderCode);
        }

        public EditTaskOrderVModel GetTaskModel(string orderCode, string ownerCode)
        {
            var customer = DictionaryBiz.GetCachedCustomer(ownerCode, ownerCode);
            var vModel = new EditTaskOrderVModel
            {
                CustomerName = customer.Name,
            };
            //var plantBiz = new PlatformBiz();
            //var plantInfor = plantBiz.GetByCustomerCode(userInfo.CustomerCode);
            //if (null != plantInfor)
            //{
            //    vModel.ConnectInfo = plantInfor.TaskOrderConnect;
            //}
            var taskOrder = GetByCode(orderCode);
            if (null != taskOrder)
            {
                vModel.TaskOrder = taskOrder;
            }
            else
            {
                var orderBiz = new TktOrderBiz();
                var order = orderBiz.GetOrder(orderCode);
                if (null != order)
                {
                    int personNum = 0;
                    var ticketBiz = new TktProductBiz();
                    string ticketProduct = string.Empty;
                    string description = string.Empty;
                    foreach (var item in order.OrderDetails)
                    {
                        personNum += item.PeopleNum;
                        var ticket = ticketBiz.GetById(item.ProductId);
                        ticketProduct += item.ProductName + "：价格：" + item.SettlePrice + "[" + item.PriceType + "] 人数：" + item.PeopleNum + "<br/>";
                        description += ticket.ProductName + ":" + ticket.ProductDesc + "<br/>";
                    }
                    var start = order.OutDate.Value;
                    var end = order.OutDate.Value.AddDays(1);
                    vModel.TaskOrder = new TktTaskOrderModel
                    {
                        MasterOrderCode = orderCode,
                        TouristNumber = personNum,
                        StartDate = start,
                        EndDate = end,
                        GuideName = order.GuideName,
                        Product = ticketProduct,
                        Description = description
                    };
                }
            }

            return vModel;
        }

        public int AddTaskOrder(EditTaskOrderVModel vModel)
        {
            var taskOrder = GetListByCode(vModel.TaskOrder.MasterOrderCode);
            if (taskOrder != null && taskOrder.Count > 0)
                return 0;
            var model = vModel.TaskOrder;
            model.TourCode = model.TourCode ?? string.Empty;
            model.GuideName = model.GuideName ?? string.Empty;
            model.RouteDetail = model.RouteDetail ?? string.Empty;
            model.Traffic = model.Traffic ?? string.Empty;
            model.Product = model.Product ?? string.Empty;
            model.Hotel = model.Hotel ?? string.Empty;
            model.Catering = model.Catering ?? string.Empty;
            model.Other = model.Other ?? string.Empty;
            model.Description = model.Description ?? string.Empty;
            model.ModifiedTime = DateTime.Now;
            return Convert.ToInt32(_dao.Insert(model));
        }

        public int UpdateTaskOrder(EditTaskOrderVModel vModel)
        {
            var model = GetById(vModel.TaskOrder.ID);
            model.TourCode = vModel.TaskOrder.TourCode ?? string.Empty;
            model.TouristNumber = vModel.TaskOrder.TouristNumber;
            model.GuideName = vModel.TaskOrder.GuideName ?? string.Empty;
            model.RouteDetail = vModel.TaskOrder.RouteDetail ?? string.Empty;
            model.Traffic = vModel.TaskOrder.Traffic ?? string.Empty;
            model.Product = vModel.TaskOrder.Product ?? string.Empty;
            model.Hotel = vModel.TaskOrder.Hotel ?? string.Empty;
            model.Catering = vModel.TaskOrder.Catering ?? string.Empty;
            model.Other = vModel.TaskOrder.Other ?? string.Empty;
            model.Description = vModel.TaskOrder.Description ?? string.Empty;
            model.PreMoney = vModel.TaskOrder.PreMoney;
            model.ModifiedBy = vModel.TaskOrder.ModifiedBy;
            model.ModifiedTime = DateTime.Now;
            return _dao.Update(model);
        }

        public EditTaskOrderVModel GetPrintTaskModel(int taskOrderId, CrmAccountModel userInfo, SysPlatformModel host)
        {
            var customer = DictionaryBiz.GetCachedCustomer(userInfo.OwnerCode, userInfo.OwnerCode);
            var vModel = new EditTaskOrderVModel
            {
                CustomerName = customer.Name,
            };
            var plantBiz = new PlatformBiz();

            if (null != host)
            {
                vModel.ConnectInfo = host.ProfileModels.Where(m => m.Key == "host.TaskOrderConnect").FirstOrDefault().Value;
            }
            var taskOrder = GetById(taskOrderId);
            if (null != taskOrder)
            {
                vModel.TaskOrder = taskOrder;
            }

            return vModel;
        }
    }
}