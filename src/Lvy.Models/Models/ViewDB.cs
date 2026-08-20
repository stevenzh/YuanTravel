using PetaPoco;
using System;

namespace Lvy.Models.OrderDB
{
    /// <summary>
    /// 收款记录
    /// </summary>
    [TableName("vw_payin")]
    [PrimaryKey("Id")]
    public class ViewPayInModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

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

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 缴款客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 销售组
        /// </summary>
        public string SalesTeamID { get; set; }

        /// <summary>
        /// 出行日期
        /// </summary>
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 产品组
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// 销售员编码
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// 总应收
        /// </summary>
        public decimal YingShou { get; set; }

        /// <summary>
        /// 分管财务部
        /// </summary>
        public string FinanceCode { get; set; }

        /// <summary>
        /// 缴款人姓名
        /// </summary>
        public string JiaoKuanRen { get; set; }

        public string OwnerCode { get; set; }
    }

    /// <summary>
    /// 发票申请
    /// </summary>
    [TableName("vw_invoices")]
    [PrimaryKey("Id")]
    public class ViewInvoiceModel
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

        [ResultColumn]
        public DateTime OutDate { get; set; }

        [ResultColumn]
        public string ProductName { get; set; }

        [ResultColumn]
        public string TeamID { get; set; }

        [ResultColumn]
        public string SalesTeamID { set; get; }

        [ResultColumn]
        public string SalerCode { get; set; }
        public string OwnerCode { get; set; }
    }
}