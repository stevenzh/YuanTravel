using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.VModels.Tour;

namespace Lvy.VModels.OpTour
{
    public class GatheringVModel : BaseVModel
    {
        public GatheringVModel()
        {
            if (TourPayInList == null)
                TourPayInList = new PagedList<ViewPayInModel>();
            if (Condition == null)
                Condition = new ConditionModel();
        }

        /// <summary>
        /// 订单信息列表
        /// </summary>
        public PagedList<ViewPayInModel> TourPayInList { get; set; }

        /// <summary>
        /// 查询条件信息
        /// </summary>
        public ConditionModel Condition { get; set; }

        public string JieSuanState { get; set; }

        public FinanceTotalModel TotalModel { get; set; }
    }

    /// <summary>
    /// 条件查询类
    /// </summary>
    public class ConditionModel
    {
        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 产品部门
        /// </summary>
        public string LineTeam { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 分销商 （编号） -》关联 crm_customer-》(Name)
        /// </summary>
        public string BookingCustomer { get; set; }

        public string SaleTeamId { get; set; }

        /// <summary>
        /// 销售
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// 原：订单状态  1，新订单；2，已确认；8，已取消（未产生费用）；9，已退团（产生费用）
        /// 10，已结算
        /// 现：订单状态：10，已结算；11，未结算( 2.已确认   和  9.已退团 )
        /// </summary>
        public string OrderState { get; set; }

        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrderSource { get; set; }

        /// <summary>
        /// 出发日期-起
        /// </summary>
        public string OutDateRange { get; set; }

        /// <summary>
        /// 预定日期-起
        /// </summary>
        public string CreatedTimeRange { get; set; }

        /// <summary>
        /// 收款日期-起
        /// </summary>
        public string PayInTimeRange { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 结算状态
        /// 0:未结算；1：已结算
        /// </summary>
        public string SettlementState { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortKey { get; set; }

        /// <summary>
        /// 团队性质
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 团期审核状态 0 未审核 1 已审核
        /// </summary>
        public string TourAuditState { get; set; }

        public string PayInId { get; set; }

        public string JieSuanState { get; set; }

        /// <summary>
        /// 财务部门ID
        /// </summary>
        public string FrTeamId { get; set; }
    }
}