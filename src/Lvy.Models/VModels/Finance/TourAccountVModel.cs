using Lvy.Models;
using Lvy.Models.TourDB;
using Lvy.VModels.Tour;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.VModels.Finance
{
    /// <summary>
    /// 单团
    /// </summary>
    public class TourAccountVModel : BaseVModel
    {
        public TourAccountVModel()
        {
            if (Condition == null)
                Condition = new ConditionModel();
            if (TourAccountList == null)
                TourAccountList = new PagedList<TourAccountInfoVModel>();
            if (AccountInfoVModel == null)
                AccountInfoVModel = new TourAccountInfoVModel();
            if (AccountDetailVModel == null)
                AccountDetailVModel = new FinanceTotalModel();
            this.IsCopy = false;
        }

        /// <summary>
        /// 查询条件信息
        /// </summary>
        public ConditionModel Condition { get; set; }

        /// <summary>
        /// 团订单列信息
        /// </summary>
        public TourAccountInfoVModel AccountInfoVModel { get; set; }

        /// <summary>
        /// 团单信息列表
        /// </summary>
        public PagedList<TourAccountInfoVModel> TourAccountList { get; set; }

        /// <summary>
        /// 结算明细
        /// </summary>
        public FinanceTotalModel AccountDetailVModel { get; set; }

        /// <summary>
        /// 营业收入
        /// </summary>
        public List<BusinessIncomeVModel> BusinessIncomeVModels { get; set; }

        /// <summary>
        /// 成本模板列表
        /// </summary>
        public List<TpTourCostModel> Costs { get; set; }

        /// <summary>
        /// 成本
        /// </summary>
        public List<TpTourCostModel> CostModels
        {
            get { return Costs.ToList(); }
            set { Costs.AddRange(value); }
        }

        public bool IsCopy { get; set; }
    }

    /// <summary>
    /// 团订单计划信息
    ///  DESC: 捡取多张表的字段，解决petapoco的pager不支持多表问题
    /// </summary>
    public class TourAccountInfoVModel
    {
        /// <summary>
        /// 团编号
        /// </summary>
        public int TourId { get; set; }

        public string MasterOrderCode { get; set; }

        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 线路名+标注
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 出团日期
        /// </summary>
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 计划名额
        /// </summary>
        public int PlanQuota { get; set; }

        /// <summary>
        /// 已用名额
        /// </summary>
        public int UsedQuota { get; set; }

        /// <summary>
        /// 剩余名额
        /// </summary>
        public int UseQuota { get; set; }

        /// <summary>
        /// 团队性质 1 散拼 2 整团 3 专线 4 商务团 5 其他
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 审核状态 0:未成团, 1:已成团 2:团单制作中 3:提交财务 4:财务审核
        /// </summary>
        public int AuditState { get; set; }

        /// <summary>
        /// 团渠道来源 OTA|旅行社
        /// </summary>
        public int TourSource { get; set; }

        /// <summary>
        /// 推荐方式
        /// </summary>
        public int TuiJianType { get; set; }

        /// <summary>
        /// 线路类型
        /// </summary>
        public string LineType { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string ModifiedBy { get; set; }
    }

    /// <summary>
    /// 营业收入
    /// </summary>
    public class BusinessIncomeVModel
    {
        /// <summary>
        /// 商户编号
        /// </summary>
        public string BookingCustomer { get; set; }

        /// <summary>
        /// 对应销售
        /// </summary>
        public string BookingAccount { get; set; }

        /// <summary>
        /// 预定人数
        /// </summary>
        public int TravellerCount { get; set; }

        /// <summary>
        /// 应收款
        /// </summary>
        public decimal TolYsPrice { get; set; }

        /// <summary>
        /// 已收款
        /// </summary>
        public decimal TolPaid { get; set; }

        /// <summary>
        /// 未收款
        /// </summary>
        public decimal NoPaid
        {
            get { return TolYsPrice - TolPaid; }
        }
    }
}