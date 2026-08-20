using Lvy.Models.OrderDB;
using Lvy.Models.TicketDB;
using Lvy.Visa.Models;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lvy.Models.TourDB
{
    /// <summary>
    /// 单团核算&主订单
    /// </summary>
    [TableName("TpTourBalance")]
    [PrimaryKey("Id")]
    public class TpTourBalanceModel : BaseModel
    {
        public TpTourBalanceModel()
        {
            this.VisaOrder = new VisaOrderModel();
        }

        public int Id { get; set; }

        /// <summary>
        /// 主订单编号
        /// </summary>
        public string MasterOrderCode { get; set; }

        /// <summary>
        /// 产品组
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// 销售组
        /// </summary>
        public string SalesTeamId { get; set; }

        /// <summary>
        /// 销售编码
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// 销售名称
        /// </summary>
        public string Saler { get; set; }

        /// <summary>
        /// 门店编码(取自客户编码)  门店报团使用
        /// </summary>
        public string BranchCode { get; set; }

        [Description("团号")]
        public string TourNo { get; set; }

        /// <summary>
        /// 关联 TpTourPlan主键
        /// </summary>
        public int? TourId { get; set; }

        /// <summary>
        /// 产品类型 1-旅游线路, 2-机票，3-签证，4-酒店，9-通用产品
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否部门报团  1非报团  2 报团
        /// </summary>
        public int IsPackage { get; set; }

        /// <summary>
        /// 旅游产品类型
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 团单状态  0:初始, 3:提交财务 4:财务审核 5:收付款完成
        /// </summary>
        public int AuditState { get; set; }

        /// <summary>
        /// 预计出行(签证)或入园时间(门票)
        /// </summary>
        public DateTime? OutDate { get; set; }

        /// <summary>
        /// 旅游天数
        /// </summary>
        public int TravelDays { get; set; }

        /// <summary>
        /// 总应收
        /// 非抱团：订单累计+子订单累计
        /// 抱团： 缴款累计
        /// </summary>
        [Description("总应收")]
        public decimal YingShou { get; set; }

        /// <summary>
        /// 实收   缴款已收部分累计
        /// </summary>
        [Description("已收")]
        public decimal YiShou { get; set; }

        /// <summary>
        /// 结算人数
        /// </summary>
        [Description("结算人数")]
        public int Num { get; set; }

        /// <summary>
        /// 成人数
        /// </summary>
        public int AuditPax { get; set; }

        /// <summary>
        /// 老人数
        /// </summary>
        public int OldPax { get; set; }

        /// <summary>
        /// 儿童数
        /// </summary>
        public int ChildPax { get; set; }

        /// <summary>
        /// 总成本  成本表累计
        /// </summary>
        [Description("总成本")]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// 毛利
        /// </summary>
        [Description("毛利")]
        public decimal MaoLi { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 供应商账户   创建人
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// CreatedDate
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        ///  提交人
        /// </summary>
        public string OPAuditBy { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime? OPAuditTime { get; set; }

        /// <summary>
        ///  审核人
        /// </summary>
        public string CWAuditBy { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? CWAuditTime { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 是否复制
        /// </summary>
        public bool IsCopy { get; set; }

        #region 订单属性

        /// <summary>
        /// 订单取消状态
        /// </summary>
        public int IsCancel { get; set; }

        /// <summary>
        /// 通用订单状态  1，已占位；2，已确认；10，已完成（自动关闭）
        /// </summary>
        public int OrderState { get; set; }

        /// <summary>
        /// 订单来源 1.官网 2.同业 3.门店 4. 微信 5. 淘宝店 6.大客户
        /// </summary>
        public int? OrderSource { get; set; }

        /// <summary>
        /// 游客姓名
        /// </summary>
        public string TouristName { get; set; }

        /// <summary>
        /// 游客电话
        /// </summary>
        public string TouristPhone { get; set; }

        /// <summary>
        /// 分销商编码
        /// </summary>
        public string AgentCode { get; set; }

        /// <summary>
        /// 分销商名称
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// 联系人邮箱
        /// </summary>
        public string ContactEmail { get; set; }

        /// <summary>
        /// 邮编
        /// </summary>
        public string PostCode { get; set; }

        /// <summary>
        /// 配送地址
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// 是否需要发票
        /// </summary>
        public int IsneedInvoice { get; set; }

        /// <summary>
        /// 签约方式  门市签约|网上签约
        /// </summary>
        public int ContractType { get; set; }

        /// <summary>
        /// 订单付款状态  1.未支付  2 定金部分支付 3：定金已付 4：全款部分已付  5 已支付
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 最晚支付时间
        /// </summary>
        public DateTime? LaterPayDate { get; set; }

        /// <summary>
        /// 导游姓名
        /// </summary>
        [Description("导游/领队")]
        public string GuideName { get; set; }

        /// <summary>
        /// 导游电话
        /// </summary>
        public string GuidePhone { get; set; }

        #endregion 订单属性

        [ResultColumn]
        public string PaymentStatusName { get; set; }

        [ResultColumn]
        public string OrderStatusName { get; set; }

        [ResultColumn]
        public string StatusName { get; set; }

        [ResultColumn]
        public string OrderSourceValue { get; set; }

        [ResultColumn]
        public string LineId { get; set; }

        [ResultColumn]
        public string TeamName { get; set; }

        [ResultColumn]
        public string BranchName { get; set; }

        [ResultColumn]
        public int MalePax { get; set; }

        [ResultColumn]
        public int FemalePax { get; set; }

        [ResultColumn]
        public decimal TotalRealpay { get; set; }

        /// <summary>
        /// 成本列表
        /// </summary>
        [ResultColumn]
        public List<TpTourCostModel> Costs { get; set; }

        /// <summary>
        /// 门票订单
        /// </summary>
        [ResultColumn]
        public List<TktOrdersModel> OrderDetails { get; set; }

        /// <summary>
        /// 签证订单
        /// </summary>
        [ResultColumn]
        public VisaOrderModel VisaOrder { get; set; }

        /// <summary>
        /// 子订单列表
        /// </summary>
        [ResultColumn]
        public List<TpChildOrderModel> ChildOrders { get; set; }
    }

    public class TpTourBalanceCosts
    {
        public TpTourBalanceModel current;

        public TpTourBalanceModel MapIt(TpTourBalanceModel model1, TpTourCostModel model2)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (model1 == null)
                return current;

            // Is this the same author as the current one we're processing
            if (current != null && current.Id == model1.Id)
            {
                // Yes, just add this post to the current author's collection of posts
                current.Costs.Add(model2);

                // Return null to indicate we're not done with this author yet
                return null;
            }

            // This is line different author to the current one, or this is the
            // first time through and we don't have an author yet

            // Save the current author
            var prev = current;

            // Setup the new current author
            current = model1;
            current.Costs = new List<TpTourCostModel>();
            current.Costs.Add(model2);

            // Return the now populated previous author (or null if first time through)
            return prev;
        }
    }

    /// <summary>
    /// 营收成本表和团其他收入
    /// </summary>
    [TableName("TpTourCosts")]
    [PrimaryKey("Id")]
    public class TpTourCostModel : BaseModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 记录复制是用于对应付款记录
        /// </summary>
        public string Code { get; set; }

        public string MasterOrderCode { get; set; }

        /// <summary>
        /// 客户ID
        /// </summary>
        [Description("供应商编号")]
        public string SupplierId { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        [Description("项目")]
        public string Item { get; set; }

        /// <summary>
        /// 单项成本
        /// </summary>
        [Description("单项成本")]
        public decimal Cost { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Description("数量")]
        public int Num { get; set; }

        /// <summary>
        /// 单项总成本
        /// </summary>
        [Description("单项总成本")]
        public decimal ItemCost { get; set; }

        /// <summary>
        /// 已付金额
        /// </summary>
        [Description("已付金额")]
        public decimal PaidCost { get; set; }

        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        [Description("付款方式")]
        public int PaymentType { get; set; }

        /// <summary>
        /// 付款时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        [Description("是否有效")]
        public int IsValid { get; set; }

        /// <summary>
        /// 状态 0：初始 1：提交审核 2：审核 3：部分付款  4：已付清
        /// </summary>
        [Description("是否有效")]
        public int Status { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public decimal ROE { get; set; }

        /// <summary>
        /// 成本游客细化数组【签证申请】
        /// </summary>
        public string TravelerArray { get; set; }

        /// <summary>
        /// 是否复制  False:OP ， True:CW
        /// </summary>
        public bool IsCopy { get; set; }

        [ResultColumn]
        public string TourNo { get; set; }

        /// <summary>
        /// 线路名称
        /// </summary>
        [ResultColumn]
        public string ProductName { get; set; }

        /// <summary>
        /// 出团时间
        /// </summary>
        [ResultColumn]
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 团单状态
        /// </summary>
        [ResultColumn]
        public int AuditState { get; set; }
        [ResultColumn]
        public string SupplierName { get; set; }
        [ResultColumn]
        public string ItemValue { get; set; }
    }

    /// <summary>
    /// 附件列表
    /// </summary>
    [TableName("TourFiles")]
    [PrimaryKey("Id")]
    public class TourFileModel
    {
        /// <summary>
        /// 自增编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 团编号
        /// </summary>
        public string MasterOrderCode { get; set; }

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
        /// 资源类型   账单  付款凭证
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

        [ResultColumn]
        public int KeyId { get; set; }
    }

    /// <summary>
    /// 付款记录表
    /// </summary>
    [TableName("TpPayments")]
    [PrimaryKey("Id")]
    public class TpPaymentModel : BaseModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }

        public string MasterOrderCode { get; set; }

        /// <summary>
        /// 团成本ID
        /// </summary>
        public string CostCode { get; set; }

        /// <summary>
        /// 付款供应商
        /// </summary>
        public string SupplierId { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 收款人
        /// </summary>
        public string PaymentBy { get; set; }

        /// <summary>
        /// 收款时间
        /// </summary>
        public DateTime PayTime { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string AuditBy { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime AuditTime { get; set; }
    }
}