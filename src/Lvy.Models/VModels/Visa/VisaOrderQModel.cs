using Lvy.Models;
using Lvy.Models.TourDB;
using Lvy.Visa.Models;
using Lvy.VModels;

namespace Lvy.Visa.VModels
{
    /// <summary>
    /// Visa_Order:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    public class VisaOrderQModel : BaseVModel
    {
        public VisaOrderQModel()
        {
            this.visaOrderModelsList = new PagedList<TpTourBalanceModel>();
        }

        public TpTourBalanceModel visaOrderModel { get; set; }

        /// <summary>
        /// 订单分页列表
        /// </summary>
        public PagedList<TpTourBalanceModel> visaOrderModelsList { get; set; }

        public OrderQueryModel orderQueryModel { get; set; }

        public string SortProperty { get; set; }

        public bool IsAscending { get; set; }

        //public List<SelectListItem> RepayType { get; set; }

        //public List<SelectListItem> OrderStatus { get; set; }

        //public List<SelectListItem> RepayStatus { get; set; }

        //public List<SelectListItem> OrderSource { get; set; }

        //public List<SelectListItem> OperStatus { get; set; }

        /// <summary>
        /// 待处理/全部
        /// </summary>
        public int IsSearch { get; set; }

        /// <summary>
        /// 度假 出行人信息
        /// </summary>
        //public IList<VtTravellerModel> vttravellers { get; set; }
        /// <summary>
        /// 订单编码
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 文件名称(面试通知书)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 是否详细页面
        /// </summary>
        public int IsDetail { get; set; }

        /// <summary>
        /// 取消订单数
        /// </summary>
        public int CancelCount { get; set; }

        public VisaInformationModel ProductModel { get; set; }
    }
}