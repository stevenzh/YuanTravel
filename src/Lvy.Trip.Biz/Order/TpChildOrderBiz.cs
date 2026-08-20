using Arch.Common.Utils;
using log4net;
using Lvy.Models.OrderDB;
using Lvy.Trip.Dao.Order;
using PetaPoco;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Order
{
    public class TpChildOrderBiz : BaseBiz
    {
        private TpChildOrderDao _dao = new TpChildOrderDao();

        private static readonly ILog logger = LogManager.GetLogger(typeof(TpChildOrderBiz));

        /// <summary>
        /// 添加子订单信息-重新计算订单总金额
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long SaveTpChildOrder(TpChildOrderModel model)
        {
            model.ChildOrderCode = "Z" + DBTools.GetSeqNo("ChildOrder");
            model.IsCancel = 0;
            model.Amount = model.UnitPrice * model.Quantity;

            _dao.Insert(model);

            return model.Id;
        }

        /// <summary>
        /// 更新子订单信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateTpChildOrder(TpChildOrderModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TpChildOrderModel> GetTpChildOrderList(string orderCode)
        {
            Sql sql = new Sql();
            sql.Append(@" select t.*,c.Name as SupplierName ,b.`Value` as ProductTypeName from TpChildOrders t  
              left join CrmCustomer c on t.suppliercode=c.code 
              left join BaseDictionaryDetail b on b.`Key`=t.ProductType and  b.name='SupplierCostItemsEnum' and b.IsValid=1 
            where ordercode=@0 ", orderCode);

            return _dao.Query(sql.SQL, sql.Arguments).ToList();
        }

        public TpChildOrderModel GetTpChildOrderById(long id)
        {
            return _dao.GetById(id);
        }

        /// <summary>
        /// 取消子订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int CancelChidOrder(int id)
        {
            return _dao.Update("SET IsCancel=1 WHERE ID=@0", id);
        }

        public int RecoverChidOrder(int id)
        {
            return _dao.Update("SET IsCancel=0 WHERE ID=@0", id);
        }
    }
}