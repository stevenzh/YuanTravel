using Lvy.Models.TicketDB;

namespace Lvy.VModels.Ticket
{
    public class EditTaskOrderVModel : BaseVModel
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string ConnectInfo { get; set; }

        public TktTaskOrderModel TaskOrder { get; set; }
    }
}