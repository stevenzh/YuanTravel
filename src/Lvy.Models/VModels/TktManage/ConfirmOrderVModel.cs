using Lvy.Models.CrmDB;
using Lvy.Models.TourDB;
using System.Collections.Generic;

namespace Lvy.VModels.Ticket
{
    public class ConfirmOrderVModel : BaseVModel
    {
        /// <summary>
        /// 分销商
        /// </summary>
        public CrmCustomerModel ReceiveCustomer { get; set; }

        /// <summary>
        /// 平台信息
        /// </summary>
        public SysPlatformModel PlatForm { get; set; }

        /// <summary>
        /// 平台商户
        /// </summary>
        public CrmCustomerModel PlatCustomer { get; set; }

        /// <summary>
        /// 门票订单
        /// </summary>
        public TpTourBalanceModel Order { get; set; }

        /// <summary>
        /// 门票专管员
        /// </summary>
        public List<CrmAccountModel> TicketAdmins { get; set; }
    }
}