using Lvy.Models.TicketDB;

namespace Lvy.VModels.Ticket
{
    public enum TicketOperation
    {
        Add = 1,
        Edit = 2,
        Copy = 3
    }

    /// <summary>
    /// 提交返回结果
    /// </summary>
    public class EidtTktResultModel
    {
        public EidtTktResultModel(string state, TicketOperation operation, string productId)
        {
            State = state;
            Operation = operation;
            ProductId = productId;
        }

        public string State { get; set; }
        public TicketOperation Operation { get; set; }
        public string ProductId { get; set; }
    }

    public class EditTicketVModel : BaseVModel
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public TicketOperation Operation { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        public string ArriveDestName { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        public string[] Themes { get; set; }
        public string OutDateRange { get; set; }
        public string BookingRange { get; set; }

        /// <summary>
        /// 门票产品
        /// </summary>
        public TktProductModel TicketProduct { get; set; }

    }
}