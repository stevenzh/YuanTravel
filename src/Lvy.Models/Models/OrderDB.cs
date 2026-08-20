using Lvy.Models.ProductDB;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Models.OrderDB
{
    /// <summary>
    /// 订单表
    /// </summary>
    [TableName("TpOrder")]
    [PrimaryKey("Id")]
    public class TpOrderModel
    {
        /// <summary>
        /// 系统序号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 团编号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 上车地点编码
        /// </summary>
        public int LineBusPointId { get; set; }

        /// <summary>
        /// 上车点 json对象 TpLineBusPointModel
        /// </summary>
        public string LineBusPoint { get; set; }

        /// <summary>
        /// 游客联系人姓名
        /// </summary>
        public string LinkMan { get; set; }

        /// <summary>
        /// 游客联系电话
        /// </summary>
        public string LinkPhone { get; set; }

        /// <summary>
        /// 发团日期
        /// </summary>
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 分销商联系人
        /// </summary>
        public string Managers { get; set; }

        /// <summary>
        /// 分销商联系人
        /// </summary>
        public string ManagerPhone { get; set; }

        /// <summary>
        /// 折扣方式（来自客户协议）
        /// </summary>
        public int DiscountType { get; set; }

        /// <summary>
        /// 折扣比（来自客户协议）
        /// </summary>
        public decimal DiscountPerCent { get; set; }

        /// <summary>
        /// 折扣金额（来自客户协议）
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// 账单应收金额（给预定客户，非真正结算金额）
        /// </summary>
        public decimal InvoiceAmount { get; set; }

        /// <summary>
        /// 总应收(实际应收，结算客户折让过的)
        /// </summary>
        public decimal TolYsPrice { get; set; }

        /// <summary>
        /// 总实收
        /// </summary>
        public decimal TolPaid { get; set; }

        /// <summary>
        /// 占位状态  1，已占位；2，已确认；
        /// </summary>
        public int OrderState { get; set; }

        /// <summary>
        /// 跟单状态
        /// 10 跟单初始；20 游客材料输入；30 账单已制作；40 账单已确认（通知OP占位）；50 出团通知已上传；90 跟单完成
        /// </summary>
        public int TraceState { get; set; }

        /// <summary>
        /// 是否取消或者退团   0 未取消; 1 取消（未产生成本）; 2 取消（产生成本）
        /// </summary>
        public int IsCancel { get; set; }

        /// <summary>
        /// 取消状态   10 销售发起请求; 20 OP审核清位; 30 财务审核; 90 处理完成
        /// </summary>
        public int CancelState { get; set; }

        /// <summary>
        /// 定金状态： 10： 未收取   20： 以收取
        /// </summary>
        public int DepositState { get; set; }

        /// <summary>
        /// 是否接送 是否接送 0，接送；1，不接送
        /// </summary>
        public int IsJieSong { get; set; }

        /// <summary>
        /// 已开发票
        /// </summary>
        public bool IssuedInvoice { get; set; }

        /// <summary>
        /// 已签合同
        /// </summary>
        public bool SignedContract { get; set; }

        /// <summary>
        /// 出行人数
        /// </summary>
        public int TravellerCount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 备注2
        /// </summary>
        public string Remark2 { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// CreatedBy（CrmAccountModel：Code）
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 最后修改用户
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 供应商编号 组团社
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 预定账号，如果是代定的场合  null
        /// </summary>
        public string BookingAccount { get; set; }

        /// <summary>
        /// 分销商编号|客户（CrmCustomerModel：Code）
        /// </summary>
        public string BookingCustomer { get; set; }

        /// <summary>
        /// 客户联系人编码（CrmAccountModel：Code）
        /// </summary>
        public string ContactCode { get; set; }

        /// <summary>
        /// 结算方式（1：自己,2：平台，3:父公司。）
        /// </summary>
        public int SettlePlatForm { get; set; }

        /// <summary>
        /// 结算客户（父客户 或者 平台）
        /// </summary>
        public string SettleCustomer { get; set; }

        /// <summary>
        /// 附加信息 （非汽车班 出团通知书里添加 动态航班等信息）
        /// </summary>
        public string AdditionInfo { get; set; }

        /// <summary>
        /// 订单来源 （1：同业 2：平台 3：电商）
        /// </summary>
        public int OrderSource { get; set; }

        /// <summary>
        /// OTA关联订单号
        /// </summary>
        public string JoinOrderCode { get; set; }

        /// <summary>
        /// 结算状态
        /// 1：未支付 2：定金部分支付 3：定金已付 4：部分支付 5:已付清
        /// </summary>
        public int JieSuanState { get; set; }

        /// <summary>
        /// 销售所在组
        /// </summary>
        public string SalesTeamId { get; set; }

        /// <summary>
        /// 销售code
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// 控位时长（小时）
        /// </summary>
        public int EffectiveHour { get; set; }

        /// <summary>
        /// 需缴纳定金
        /// </summary>
        public decimal Deposit { get; set; }

        /// <summary>
        /// 定金最晚缴纳日期
        /// </summary>
        public DateTime? DepositDate { get; set; }

        /// <summary>
        /// 项目说明与特别约定（账单）
        /// </summary>
        public string BillOffers { get; set; }

        /// <summary>
        /// 账单本期金额
        /// </summary>
        public decimal BillAmount { get; set; }

        /// <summary>
        /// 账单付款时限
        /// </summary>
        public DateTime? BillDeadline { get; set; }

        /// <summary>
        /// 账单是否显示折扣 false 不显示 true 显示 (不显示在账单 为后返， 显示账单为即使折让)
        /// </summary>
        public bool RebateInBill { get; set; }

        /// <summary>
        /// 线路对象
        /// </summary>
        [ResultColumn]
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 上车点对象
        /// </summary>
        //[ResultColumn]
        //public TpLineBusPointModel LineBusPoint { get; set; }
        /// <summary>
        ///订单对应的游客对象集合
        /// </summary>
        [ResultColumn]
        public List<TpTravellerModel> TravellerModels { get; set; }

        /// <summary>
        /// 收款列表
        /// </summary>
        [ResultColumn]
        public List<TpOrderPayInModel> PayInList { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [ResultColumn]
        public string CustomerName { get; set; }

        /// <summary>
        /// 结算客户名称
        /// </summary>
        [ResultColumn]
        public string SettleCustomerName { get; set; }

        /// <summary>
        /// 线路名
        /// </summary>
        [ResultColumn]
        public string LineName { get; set; }

        /// <summary>
        /// 线路出行天数
        /// </summary>
        [ResultColumn]
        public int TravelDays { get; set; }

        /// <summary>
        /// 销售名称
        /// </summary>
        [ResultColumn]
        public string SalerName { get; set; }

        /// <summary>
        /// 账单文件
        /// </summary>
        [ResultColumn]
        public TpOrderFileModel BillFile { get; set; }

        [ResultColumn]
        public string TourNo { get; set; }
    }

    /// <summary>
    /// one-to-many关系对象
    /// </summary>
    public class OrderToTravellerRelator
    {
        private TpOrderModel OrderModel;

        public TpOrderModel MapIt(TpOrderModel order, TpTravellerModel traveller)
        {
            if (order == null)
                return OrderModel;
            if (OrderModel != null && OrderModel.Id == order.Id)
            {
                OrderModel.TravellerModels.Add(traveller);
                return null;
            }
            var prev = OrderModel;
            OrderModel = order;
            OrderModel.TravellerModels = new List<TpTravellerModel>();
            OrderModel.TravellerModels.Add(traveller);
            return prev;
        }
    }

    /// <summary>
    /// 订单游客信息
    /// </summary>
    [TableName("TpTraveller")]
    [PrimaryKey("Id")]
    public class TpTravellerModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 订单编码
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 团号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 出行人姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 证件类型  0:无
        /// </summary>
        public int PassType { get; set; }

        /// <summary>
        /// 证件号
        /// </summary>
        public string PassNo { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string PinYin { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// 出生地
        /// </summary>
        public string PlaceOfBirth { get; set; }

        /// <summary>
        /// 证件签发时间
        /// </summary>
        public DateTime? DateOfIssue { get; set; }

        /// <summary>
        /// 证件签发地
        /// </summary>
        public string PlaceOfIssue { get; set; }

        /// <summary>
        /// 证件有效期
        /// </summary>
        public DateTime? DateOfExpiry { get; set; }

        /// <summary>
        /// 座位号
        /// </summary>
        public string SeatNum { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 报价编号
        /// </summary>
        public int PriceId { get; set; }

        /// <summary>
        /// 报价说明 成人   小孩
        /// </summary>
        public string PriceContent { get; set; }

        /// <summary>
        /// 原始价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 价格调整
        /// </summary>
        public decimal FanLi { get; set; }

        /// <summary>
        /// 单房差
        /// </summary>
        public decimal SingleRoom { get; set; }

        /// <summary>
        /// 特价让利
        /// </summary>
        public decimal TeJiaFanLi { get; set; }

        /// <summary>
        /// 0 不占位  1： 占位  （应该是国内产品设定）
        /// </summary>
        public int IsOccupiedQuota { get; set; }

        /// <summary>
        /// 接价
        /// </summary>
        public decimal JiePrice { get; set; }

        /// <summary>
        /// 送价
        /// </summary>
        public decimal SongPrice { get; set; }

        /// <summary>
        /// 自费
        /// </summary>
        public decimal ZiFei { get; set; }

        /// <summary>
        /// 签证费
        /// </summary>
        public decimal VisaPrice { get; set; }

        /// <summary>
        /// 税
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// 小费金额
        /// </summary>
        public decimal XiaoFei { get; set; }

        /// <summary>
        /// 应收 （所有费用累加）
        /// </summary>
        public decimal YsPrice { get; set; }

        /// <summary>
        /// 状态： 0：已取消（未产生费用）  1：已退团（产生费用）  2：有效
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 是否免票 买一送X的场合
        /// 默认：0  不免
        /// </summary>
        public int IsMianPiao { get; set; }

        /// <summary>
        /// CreatedTime
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// CreatedBy
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// ModifiedTime
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// ModifiedBy
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 是否是领队
        /// </summary>
        public bool IsLeader { get; set; }

        /// <summary>
        /// 领队Id （所有团都有单独领队吗？）
        /// </summary>
        public int LeaderNo { get; set; }

        /// <summary>
        /// 签证申请状态（0 未申请付款  1 已申请付款）， 多签证用分隔符 |
        /// </summary>
        public int VisaApply { get; set; }

        /// <summary>
        /// 是否儿童（合同使用）
        /// </summary>
        public bool IsChild { get; set; }

        /// <summary>
        /// 是否已检查游客护照信息
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// 是否自费
        /// </summary>
        [ResultColumn]
        public string IsZiFei { get; set; }

        /// <summary>
        /// 是否单房
        /// </summary>
        [ResultColumn]
        public string IsSingleRoom { get; set; }

        /// <summary>
        ///
        /// </summary>
        [ResultColumn]
        public string IsTax { get; set; }

        /// <summary>
        /// 是否付签证费（对于产品价格不含签证费的适用）
        /// </summary>
        [ResultColumn]
        public string IsVisaPrice { get; set; }

        [ResultColumn]
        public List<TpOrderFileModel> FileList { get; set; }
    }

    /// <summary>
    /// 子订单
    /// </summary>
    [TableName("TpChildOrders")]
    [PrimaryKey("Id")]
    public class TpChildOrderModel
    {
        /// <summary>
        /// 系统自增编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 子订单编码
        /// </summary>
        public string ChildOrderCode { get; set; }

        /// <summary>
        /// 订单编码
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int? ProductID { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 商品类型 签证，接送机，代购门票等
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 份数
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 总成本
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 产品供应商
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 子订单状态    0:正常 1：取消
        /// </summary>
        public int IsCancel { get; set; }
        /// <summary>
        /// 服务费/手续费
        /// </summary>
        public decimal? ServiceCharge { get; set; }

        public DateTime CreatedTime { get; set; }


        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        [ResultColumn]
        public string SupplierName { get; set; }

        [ResultColumn]
        public string ProductTypeName { get; set; }
    }

    /// <summary>
    /// 收款记录
    /// </summary>
    [TableName("TpOrderPayIn")]
    [PrimaryKey("Id")]
    public class TpOrderPayInModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        public string PayInCode { get; set; }
        /// <summary>
        /// 1-旅游线路, 2-机票，3-签证，4-酒店，9-通用
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 款项说明
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// 缴款客户
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 汇款人姓名
        /// </summary>
        public string Remitter { get; set; }

        /// <summary>
        /// 外部关联序号
        /// </summary>
        public string JoinNo { get; set; }

        /// <summary>
        /// 缴款方式
        /// </summary>
        public int PaymentType { get; set; }

        /// <summary>
        /// 加款金额
        /// </summary>
        public decimal AddAmount { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 有效无效 0：无效 1：有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 收款人  销售
        /// </summary>
        public string PayInBy { get; set; }

        /// <summary>
        /// 收款时间
        /// </summary>
        public DateTime? PayInTime { get; set; }

        /// <summary>
        /// 款项用途  定金|团款
        /// </summary>
        public int PayInUse { get; set; }

        /// <summary>
        /// 0 初始 10 提交财务  20财务收款
        /// </summary>
        public int State { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string AuditBy { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditTime { get; set; }

        /// <summary>
        /// 企业税号
        /// </summary>
        public string TaxNumber { get; set; }

        /// <summary>
        /// 账单文件
        /// </summary>
        public int BillFileId { get; set; }

        /// <summary>
        /// 付款凭证文件
        /// </summary>
        public int BankFileId { get; set; }

        [ResultColumn]
        public string BillFileUrl { get; set; }

        [ResultColumn]
        public string BankFileUrl { get; set; }

        [ResultColumn]
        public string ProductName { get; set; }

        [ResultColumn]
        public string TourNo { get; set; }

        [ResultColumn]
        public string CustomerName { get; set; }

        [ResultColumn]
        public decimal TolYsPrice { get; set; }

        [ResultColumn]
        public decimal TolPaid { get; set; }

        [ResultColumn]
        public string LineTeamId { get; set; }
        [ResultColumn]
        public string PaymentTypeValue { get; set; }
    }

    /// <summary>
    /// 发票申请
    /// </summary>
    [TableName("TpInvoices")]
    [PrimaryKey("Id")]
    public class TpInvoiceModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 订单OrderCode
        /// </summary>
        public string OrderCode { get; set; }

        public int Type { get; set; }

        #region 发票抬头

        /// <summary>
        /// 企业名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 税号
        /// </summary>
        public string TaxNumber { get; set; }

        /// <summary>
        /// 企业地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 企业电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 开户银行
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 银行账户
        /// </summary>
        public string BankAccount { get; set; }

        #endregion 发票抬头

        /// <summary>
        /// 开票金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 服务项目
        /// </summary>
        public string ServiceItems { get; set; }

        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 操作状态  0申请  1 已开具
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 财务开票人
        /// </summary>
        public string CheckedBy { get; set; }

        /// <summary>
        /// 开票时间
        /// </summary>
        public DateTime? CheckedTime { get; set; }

        /// <summary>
        /// 发票号
        /// </summary>
        public string InvoiceNo { get; set; }

        public string SettleCustomer { get; set; }

        /// <summary>
        /// 有效无效 0：无效 1：有效
        /// </summary>
        public int IsValid { get; set; }

        [ResultColumn]
        public string TourNo { get; set; }
    }

    /// <summary>
    /// 订单附件
    /// </summary>
    [TableName("TpOrderFiles")]
    [PrimaryKey("Id")]
    public class TpOrderFileModel
    {
        /// <summary>
        /// 自增编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联编号
        /// </summary>
        public int KeyId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 存放路径URL
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 文件类型    image document(doc or pdf) voice video
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// 资源类型   游客资料   账单  付款凭证
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// 是否删除  0：正常  1：删除
        /// </summary>
        public int IsDel { get; set; }

        /// <summary>
        /// 修订
        /// </summary>
        public int Revision { get; set; }
    }
}