//namespace Lvy.VModels.Ticket
//{
//    /// <summary>
//    /// 订单管理查询
//    /// </summary>
//    public class SearchOrderVModel : BaseVModel
//    {
//        #region order

//        public string Id { get; set; }

//        /// <summary>
//        /// 订单编号
//        /// </summary>
//        public string OrderCode { get; set; }

//        /// <summary>
//        /// 导游姓名
//        /// </summary>
//        public string GuideName { get; set; }

//        /// <summary>
//        /// 导游电话
//        /// </summary>
//        public string GuidePhone { get; set; }

//        /// <summary>
//        /// 分销商联系人姓名
//        /// </summary>
//        public string Managers { get; set; }

//        /// <summary>
//        /// 分销商联系人电话
//        /// </summary>
//        public string ManagerPhone { get; set; }

//        /// <summary>
//        /// 备注
//        /// </summary>
//        public string Remarks { get; set; }

//        /// <summary>
//        /// 应收
//        /// </summary>
//        public decimal TolYsPrice { get; set; }

//        /// <summary>
//        /// 已收
//        /// </summary>
//        public decimal TolPaid { get; set; }

//        /// <summary>
//        /// 订单状态  1，新订单；2，已确认；8，已取消
//        /// 10，已结算
//        /// </summary>
//        public int OrderState { get; set; }

//        /// <summary>
//        /// 预定账号，如果是代定的场合  null
//        /// </summary>
//        public string BookingAccount { get; set; }

//        /// <summary>
//        /// 分销商
//        /// </summary>
//        public string BookingCustomer { get; set; }

//        /// <summary>
//        /// 供应商编号
//        /// </summary>
//        public string SupplierCode { get; set; }

//        #endregion order

//        #region orderDetail

//        /// <summary>
//        /// 产品编号
//        /// </summary>
//        public int ProductId { get; set; }

//        /// <summary>
//        /// 产品名称
//        /// </summary>
//        public string ProductName { get; set; }

//        /// <summary>
//        /// 产品价格编号
//        /// </summary>
//        public int PriceId { get; set; }

//        /// <summary>
//        /// 价格类型
//        /// </summary>
//        public string PriceType { get; set; }

//        /// <summary>
//        /// 市场价
//        /// </summary>
//        public decimal MarketPrice { get; set; }

//        /// <summary>
//        /// 结算价
//        /// </summary>
//        public decimal SettlePrice { get; set; }

//        /// <summary>
//        /// 签单价|返利
//        /// </summary>
//        public decimal SysPrice { get; set; }

//        /// <summary>
//        /// 购票方式
//        ///     1:固定签单，2:特殊签单，3:任务单，4:特殊任务单
//        /// </summary>
//        public int TktType { get; set; }

//        /// <summary>
//        /// 人数
//        /// </summary>
//        public int PeopleNum { get; set; }

//        /// <summary>
//        /// 应收
//        /// </summary>
//        public decimal YsPrice { get; set; }

//        /// <summary>
//        /// 是否删除
//        /// </summary>
//        public int IsValid { get; set; }

//        /// <summary>
//        /// 第几天
//        /// </summary>
//        public int OutDate { get; set; }

//        #endregion orderDetail
//    }
//}