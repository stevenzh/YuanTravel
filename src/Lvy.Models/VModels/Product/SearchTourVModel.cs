using Lvy.Models;
using Lvy.Models.ProductDB;
using Lvy.VModels.Online;
using System;
using System.Collections.Generic;

namespace Lvy.VModels.Product
{
    public class SearchTourVModel : BaseVModel
    {
        public SearchTourVModel()
        {
            this.PlanStatus = "valid";
            this.NavCondition = new NavSearchVModel();
            this.Condition = new TourConditionModel
            {
                RecommendType = -1 //RecommendType赋值为-1是为了初始化时不选中推荐类型
            };
            this.TourList = new PagedList<TourInfoVModel>();
        }

        /// <summary>
        /// 线路编号
        /// Ps.此线路编号作精确查询用，由线路查询页面带入
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 开班预定情况  all 所有  booking 可预订  vaild 有效
        /// </summary>
        public string PlanStatus { get; set; }

        public TourConditionModel Condition { get; set; }

        /// <summary>
        /// 微网站使用
        /// </summary>
        public NavSearchVModel NavCondition { get; set; }

        public PagedList<TourInfoVModel> TourList { get; set; }

        public List<TourInfoVModel> TourStoreList { get; set; }
        public List<TpLineModel> LineList { get; set; }

        public List<int> TourIdsList { get; set; }
    }

    /// <summary>
    /// 查询条件
    /// </summary>
    public class TourConditionModel
    {
        /// <summary>
        /// 线路编号
        /// Ps.此线路编号作模糊查询用，在查询页面由用户输入
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 团编号
        /// </summary>
        public string TourId { get; set; }

        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 线路名称+标注
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 出发日期 止
        /// </summary>
        public string OutDateRange { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        public string ArriveDestName { get; set; }

        /// <summary>
        /// 推荐类型
        /// </summary>
        public int RecommendType { get; set; }

        /// <summary>
        /// 团队性质
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 分组查询条件
        /// </summary>
        public string CrmTeamId { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 是否外部录入产品
        /// </summary>
        public bool IsImport { get; set; }
    }

    /// <summary>
    /// 团计划信息
    ///     DESC: 捡取多张表的字段，解决petapoco的pager不支持多表问题
    /// </summary>
    public class TourInfoVModel
    {
        /// <summary>
        /// 团计划编号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 线路名称
        /// </summary>
        public string LineName { get; set; }

        public string LogoPath { get; set; }

        /// <summary>
        /// 商品品牌
        /// </summary>
        public string ProductBrand { get; set; }

        /// <summary>
        /// 状态 0：无效 1：有效 2：下线 3:上线
        /// </summary>
        public int TourState { get; set; }

        /// <summary>
        /// 出团日期
        /// </summary>
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 计划名额
        /// </summary>
        public int PlanQuota { get; set; }

        /// <summary>
        /// 预留名额
        /// </summary>
        public int HoldQuota { get; set; }

        /// <summary>
        /// 可用名额
        /// </summary>
        public int UseQuota { get; set; }

        /// <summary>
        /// 虚占位
        /// </summary>
        public int UnlockQuota { get; set; }

        /// <summary>
        /// 推荐类型
        /// </summary>
        public string RecommendType
        {
            get { return this.TuiJianType == 0 ? "普通" : "特价"; }
        }

        /// <summary>
        /// 团队性质 1 散拼 2 整团 3 专线 4 商务团 5 其他
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 是否特价
        /// </summary>
        public int TuiJianType { get; set; }

        /// <summary>
        /// 出发地
        /// </summary>
        public string DepartDest { get; set; }

        /// <summary>
        ///出发地名称
        /// </summary>
        public string DepartDestName { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        public string ArriveDestName { get; set; }

        /// <summary>
        /// 出发-目的地
        /// </summary>
        public string DepartArrive
        {
            get { return this.DepartDestName + "-" + this.ArriveDestName; }
        }

        /// <summary>
        /// 报名截止日期
        /// </summary>
        public DateTime BookingLastDays { get; set; }

        ////////////////////////////////////////////////////
        /// <summary>
        /// 团人数
        /// </summary>
        public int TravellerCount { get; set; }

        /// <summary>
        /// 已用名额（取得名额表）
        /// </summary>
        public int UsedQuota { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 团渠道来源 OTA|旅行社
        /// </summary>
        //public int TourSource { get; set; }
        /// <summary>
        /// 团单状态
        /// </summary>
        public int AuditState { get; set; }

        /////////////////////////////

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 线路特色
        /// </summary>
        public string LineSpecial { get; set; }

        /// <summary>
        /// 交通类型 1:汽车 2：火车 3：飞机 4：轮船 5：自驾  9：其他
        /// </summary>
        public int TrafficType { get; set; }

        /// <summary>
        /// 交通类型
        /// </summary>
        public int TrafficTypeName { get; set; }

        /// <summary>
        /// 主题标签
        /// </summary>
        public string Themes { get; set; }

        /// <summary>
        /// 标准价 和价格表同步
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 单房差
        /// </summary>
        public decimal SingleRoom { get; set; }

        /// <summary>
        /// 特价让利
        /// </summary>
        public decimal TeJiaFanLi { get; set; }
    }
}