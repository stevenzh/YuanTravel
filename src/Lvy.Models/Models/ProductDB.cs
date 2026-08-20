using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;
using Lvy.VModels.Booking;
using Lvy.VModels.Product;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace Lvy.Models.ProductDB
{
    /// <summary>
    /// 旅游线路
    /// </summary>
    [TableName("TpLine")]
    [PrimaryKey("Id")]
    public class TpLineModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 线路名称
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 线路名字后缀
        /// </summary>
        public string LineNamePostfix { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 航空公司
        /// </summary>
        public string AirlineCode { get; set; }

        /// <summary>
        /// 行程天数
        /// </summary>
        public int TravelDays { get; set; }

        /// <summary>
        /// 行程住宿夜数（晚）
        /// </summary>
        public int Night { get; set; }

        /// <summary>
        /// 提前预定天数
        /// </summary>
        public int MoveUpDays { get; set; }

        /// <summary>
        /// 出发地
        /// </summary>
        public string DepartDest { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 多个目的地 逗号分隔
        /// </summary>
        public string MutliDest { get; set; }

        /// <summary>
        /// 主题标签
        /// </summary>
        public string Themes { get; set; }

        /// <summary>
        /// 线路类型
        /// 1，周边游；2，国内游；3，出境游；4, 当地参团 5，自驾游  6，自由行  7,赴台游  8，游轮
        /// 1. 跟团游 2. 自由行  3.当地参团 4 自驾游  5 游轮
        /// </summary>
        public int LineType { get; set; }
        /// <summary>
        /// 1，周边游 2。国内游 3.港澳台 4。出境游
        /// </summary>
        public int LineScope { get; set; }
        /// <summary>
        /// 交通类型 1:汽车 2：火车 3：飞机 4：轮船 5：自驾  9：其他
        /// </summary>
        public int TrafficType { get; set; }

        /// <summary>
        /// 线路特色
        /// </summary>
        public string LineSpecial { get; set; }

        /// <summary>
        /// 签证说明
        /// </summary>
        public string VisaNote { get; set; }

        /// <summary>
        /// 费用包含
        /// </summary>
        public string PriceContain { get; set; }

        /// <summary>
        /// 费用不包含
        /// </summary>
        public string PriceNoContain { get; set; }

        /// <summary>
        /// 预定须知
        /// </summary>
        public string BookingNodes { get; set; }

        /// <summary>
        /// 退改规则
        /// </summary>
        public string OpDesc { get; set; }

        /// <summary>
        /// 出行提示
        /// </summary>
        public string FootNotes { get; set; }

        /// <summary>
        /// 购物描述
        /// </summary>
        public string Shopping { get; set; }

        /// <summary>
        /// 线路状态 2：下线 3:上线
        /// </summary>
        public int LineState { get; set; }

        /// <summary>
        /// 有效无效 0：无效 1：有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 应急电话 【平台的产品提供商的电话】
        /// </summary>
        public string YingJiPhone { get; set; }

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
        /// 图片
        /// </summary>
        public string LogoPath { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 产品品牌
        /// </summary>
        public string BrandCode { get; set; }

        /// <summary>
        /// 是否自组团
        /// </summary>
        public bool IsSelfGroup { get; set; }

        /// <summary>
        /// 是否外部录入
        /// </summary>
        public bool IsImport { get; set; }

        /// <summary>
        /// 审核标记   0 初始 1 提交申请  2 挂起  4 通过
        /// </summary>
        public int ImportState { get; set; }

        public bool IsNew { get; set; }

        /// <summary>
        /// 线路富文本说明
        /// </summary>
        public string LineDesc { get; set; }

        /// <summary>
        /// 航空公司名称
        /// </summary>
        [ResultColumn]
        public string AirlineName { get; set; }

        /// <summary>
        /// 供应商对象
        /// </summary>
        [ResultColumn]
        public CrmCustomerModel Supplier { get; set; }

        /// <summary>
        ///
        /// </summary>
        [ResultColumn]
        public List<TpLineAdminModel> Admins { get; set; }

        /// <summary>
        /// 开班计划
        /// </summary>
        [ResultColumn]
        public List<TourInfoVModel> Tours { get; set; }

        /// <summary>
        /// 线路信息 行程
        /// </summary>
        [ResultColumn]
        public RouteVModel RouteInfo { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        [ResultColumn]
        public string TeamName { get; set; }

        [ResultColumn]
        public List<TpLineFileModel> PicList { get; set; }
    }

    /// <summary>
    /// 价格表
    /// </summary>
    [TableName("TpPrice")]
    [PrimaryKey("Id")]
    public class TpPriceModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 团编号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 价格类型
        /// </summary>
        public int PriceType { get; set; }

        /// <summary>
        /// 价格类型名称
        /// </summary>
        /// <remarks>冗余字段</remarks>
        public string PriceTypeName { get; set; }

        /// <summary>
        /// 价格类型说明
        /// </summary>
        public string PriceRemark { get; set; }

        /// <summary>
        /// 市场价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 同业结算价
        /// </summary>
        public decimal SettlePrice { get; set; }

        /// <summary>
        /// 折扣金额
        /// </summary>
        public decimal TeJiaFanLi { get; set; }

        /// <summary>
        /// 成本价（估值）
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 是否标准价 0:非标准(defualt) 1：标准
        /// </summary>
        public int IsStandard { get; set; }

        /// <summary>
        /// 是否有效  0：无效 1：有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 套餐 该价格占位数量 0：不占位 1：1人占位 2：买一送一
        /// </summary>
        public int SuitNum { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        #region 以下部分迁移到计划表

        /// <summary>
        /// 小费
        /// </summary>
        [PetaPoco.ResultColumn]
        public decimal Tips { get; set; }

        /// <summary>
        /// 自费
        /// </summary>
        [PetaPoco.ResultColumn]
        public decimal ZiFei { get; set; }

        /// <summary>
        /// 单房差
        /// </summary>
        [PetaPoco.ResultColumn]
        public decimal SingleRoom { get; set; }

        ///// <summary>
        ///// 签证费
        ///// </summary>
        //[PetaPoco.ResultColumn]
        //public decimal VisaPrice { get; set; }

        #endregion 以下部分迁移到计划表
    }

    /// <summary>
    /// 出团计划表
    /// </summary>
    [TableName("TpTourPlan")]
    [PrimaryKey("Id")]
    public class TpTourPlanModel
    {
        /// <summary>
        /// 自增编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 团号 { CCT20180721CTD-A | 无中文字符 | 唯一不重复字段 }
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 出团日期
        /// </summary>
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 市场价 和价格表同步
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 结算价  即同业价
        /// </summary>
        public decimal SettlePrice { get; set; }

        /// <summary>
        /// 报名截止日期
        /// </summary>
        public DateTime BookingLastDays { get; set; }

        /// <summary>
        /// 推荐方式 0 常规团  1 特价团
        /// </summary>
        public int TuiJianType { get; set; }

        /// <summary>
        /// 团队性质 1 散拼 2 整团 3 专线 4 商务团 5 其他
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 渠道 1:OTA   2:同业分销(默认值)
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// 状态  2：下线 3:上线
        /// </summary>
        public int TourState { get; set; }

        /// <summary>
        /// 团单状态  0:未成团, 1:已成团  3:提交财务 4:财务审核 5:收付款完成
        /// </summary>
        public int AuditState { get; set; }

        /// <summary>
        /// 最小成团人数
        /// </summary>
        public int MixedNum { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public string AdditionInfo { get; set; }

        /// <summary>
        /// 暂时无用（套餐应用所有计划，只要有套餐价格，没有限制只能是那个套餐）
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// 入境日期
        /// </summary>
        public DateTime? EntryDate { get; set; }

        /// <summary>
        /// 入境口岸
        /// </summary>
        public int PortOfEntry { get; set; }

        /// <summary>
        /// 出境口岸
        /// </summary>
        public int PortOfExit { get; set; }

        /// <summary>
        /// 最晚开票时间
        /// </summary>
        public DateTime? LastKaiPiaoDate { get; set; }

        /// <summary>
        /// 开票截止日期天数
        /// </summary>
        [ResultColumn]
        public int KaiPiaoJieZhiDay { get; set; }

        /// <summary>
        /// 签证费（0 价格里含签证费或线路无需签证 ; >0 签证办理费用另付 价格里不含）
        /// </summary>
        public decimal VisaPrice { get; set; }

        /// <summary>
        /// 税
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// 小费
        /// </summary>
        public decimal Tips { get; set; }

        /// <summary>
        /// 自费
        /// </summary>
        public decimal ZiFei { get; set; }

        /// <summary>
        /// 单房差
        /// </summary>
        public decimal SingleRoom { get; set; }

        /// <summary>
        /// 人均成本
        /// </summary>
        public decimal PerCapitaCost { get; set; }

        /// <summary>
        /// 确认人数
        /// </summary>
        public int TravellerCount { get; set; }

        /// <summary>
        /// 线路名
        /// </summary>
        [ResultColumn]
        public string LineName { get; set; }

        /// <summary>
        /// 线路对象
        /// </summary>
        [ResultColumn]
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 应收
        /// </summary>
        [ResultColumn]
        public decimal TolYsPrice { get; set; }

        /// <summary>
        /// 应付
        /// </summary>
        [ResultColumn]
        public decimal TolPaid { get; set; }
    }

    /// <summary>
    /// 线路行程表
    /// </summary>
    [TableName("TpLineRoute")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TpLineRouteModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 第几天
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 住宿
        /// </summary>
        public string Hotel { get; set; }

        /// <summary>
        /// 餐饮
        /// </summary>
        public string Catering { get; set; }

        /// <summary>
        /// 早餐
        /// </summary>
        public string Breakfast { get; set; }

        /// <summary>
        /// 午餐
        /// </summary>
        public string Lunch { get; set; }

        /// <summary>
        /// 晚餐
        /// </summary>
        public string Supper { get; set; }

        /// <summary>
        /// 行程内容
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 线路行程 交通列表
        /// </summary>
        [PetaPoco.ResultColumn]
        public List<TpLineTrafficModel> LineTrafficList { get; set; }
    }

    /// <summary>
    /// 线路关联标签
    /// </summary>
    [TableName("TpLineTagMap")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TpLineTagMapModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 标签编号
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName { get; set; }
    }

    public class Line2LineAdmin
    {
        public TpLineModel current;

        public TpLineModel MapIt(TpLineModel line, TpLineAdminModel admin)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (line == null)
                return current;

            // Is this the same author as the current one we're processing
            if (current != null && current.Id == line.Id)
            {
                // Yes, just add this post to the current author's collection of posts
                current.Admins.Add(admin);

                // Return null to indicate we're not done with this author yet
                return null;
            }

            // This is line different author to the current one, or this is the
            // first time through and we don't have an author yet

            // Save the current author
            var prev = current;

            // Setup the new current author
            current = line;
            current.Admins = new List<TpLineAdminModel>();
            current.Admins.Add(admin);

            // Return the now populated previous author (or null if first time through)
            return prev;
        }
    }

    /// <summary>
    /// 线路专管
    /// </summary>
    [TableName("TpLineAdmin")]
    [PrimaryKey("Id")]
    public class TpLineAdminModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 线路Id
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 专管员账号
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 所属单位 0：专线批发商 1：平台供应商
        /// </summary>
        public int Department { get; set; }

        /// <summary>
        /// 是否主要负责 0：否，1：是
        /// </summary>
        public int IsPrimary { get; set; }
    }

    /// <summary>
    /// 上车地点表
    /// </summary>
    [TableName("TpLineBusPoint")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TpLineBusPointModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 基础数据 上车点ID
        /// </summary>
        public int BusPointId { get; set; }

        /// <summary>
        /// 上车点
        /// </summary>
        public string BusPoint { get; set; }

        /// <summary>
        /// 返回地点
        /// </summary>
        public string PlaceOfReturn { get; set; }

        /// <summary>
        /// 接价
        /// </summary>
        public decimal JiePrice { get; set; }

        /// <summary>
        /// 送价
        /// </summary>
        public decimal SongPrice { get; set; }

        /// <summary>
        /// 接送类型 0:不接不送 1：接  2：送 3：接送
        /// </summary>
        /// <remarks>
        /// 添加[0 不接不送]选择上车点处有此情况。
        /// </remarks>
        public int JsType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 发车时间
        /// </summary>
        public string JsTime { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 上车点对象
        /// </summary>
        [PetaPoco.Ignore]
        public BaseBusPointModel BusPointModel { get; set; }
    }

    /// <summary>
    /// 线路签证表
    /// </summary>
    [TableName("TpLineVisa")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TpLineVisaModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 国家或地区
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 签证产品ID
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 成本
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 对应游客字段（自动分配）
        /// </summary>
        public string TravelColumn { get; set; }

        public string CountryName { get; set; }
        public string ProductName { get; set; }

        [ResultColumn]
        public int VType { get; set; }
    }

    /// <summary>
    /// 线路成本规则模板
    /// </summary>
    [TableName("TpLineCostRules")]
    [PrimaryKey("Id")]
    public class TpLineCostRuleModel : BaseModel
    {
        public int Id { get; set; }
        public string LineId { get; set; }

        [Description("供应商编号")]
        public string SupplierId { get; set; }

        [Description("项目")]
        public string Item { get; set; }

        [Description("单项成本")]
        public decimal Cost { get; set; }

        [Description("备注")]
        public string Remark { get; set; }

        [PetaPoco.Ignore]
        public int IsValid { get; set; }
    }

    /// <summary>
    /// 巴士座位分布表
    /// </summary>
    [TableName("TpBusSeat")]
    [PrimaryKey("Id")]
    public class TpBusSeatModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 团计划Id
        /// </summary>
        [Obsolete("不要通过该字段关联该表。使用quotaId关联")]
        public int TourId { get; set; }

        /// <summary>
        /// 座位数量
        /// </summary>
        public int SeatNum { get; set; }

        /// <summary>
        /// 座位分布 采用json数据
        /// </summary>
        public string SeatDetail { get; set; }

        /// <summary>
        /// 团资源编号
        /// </summary>
        public int QuotaId { get; set; }

        /// <summary>
        /// SeatDetail序列化后的对象集合
        /// </summary>
        [Ignore]
        public List<BusSeatModel> SeatModels
        {
            get
            {
                return  JsonSerializer.Deserialize<List<BusSeatModel>>(SeatDetail);
            }
        }
    }

    /// <summary>
    /// 资源表
    /// </summary>
    [TableName("TpQuota")]
    [PrimaryKey("Id")]
    [Serializable]
    public class QuotaModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string QuotaName { get; set; }

        /// <summary>
        /// 计划名额
        /// </summary>
        public int PlanQuota { get; set; }

        /// <summary>
        /// 可用名额
        /// </summary>
        public int UseQuota { get; set; }

        /// <summary>
        /// 已用名额
        /// </summary>
        public int UsedQuota { get; set; }

        /// <summary>
        /// 虚占位
        /// </summary>
        public int UnLockQuota { get; set; }

        /// <summary>
        /// 预留名额
        /// </summary>
        public int HoldQuota { get; set; }

        /// <summary>
        /// 出发日期
        /// </summary>
        public DateTime OutDate { get; set; }

        /// <summary>
        /// 共享说明
        /// </summary>
        public string ShareDesc { get; set; }

        /// <summary>
        /// 来源 1：标准团  2：共享团   默认1
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 交通类型 1:汽车 2：火车 3：飞机 4：轮船 5：自驾  9：其他
        /// </summary>
        public int TrafficType { get; set; }
    }

    /// <summary>
    /// 团库存中间表
    /// </summary>
    [TableName("TpTourQuotaMap")]
    [PrimaryKey("Id")]
    public class TourQuotaMapModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 团编号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 库存资源编号
        /// </summary>
        public int QuotaId { get; set; }

        /// <summary>
        /// 最大可用名额 共享位置的场合可用
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// 来源 1：标准团  2：共享团   默认1
        /// </summary>
        public int Source { get; set; }

        #region Realation

        /// <summary>
        /// 资源对象
        /// </summary>
        [PetaPoco.ResultColumn]
        public QuotaModel Quota { get; set; }

        /// <summary>
        /// 团计划对象
        /// </summary>
        [PetaPoco.ResultColumn]
        public TpTourPlanModel Tour { get; set; }

        #endregion Realation
    }

    /// <summary>
    /// 开班计划航班信息表
    /// </summary>
    [TableName("TpTourFlight")]
    [PrimaryKey("Id")]
    public class TpTourFlightModel
    {
        /// <summary>
        ///自增ID
        /// </summary>
        public int Id { get; set; }

        public int TourId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string FlightType { get; set; }

        /// <summary>
        /// 航空公司
        /// </summary>
        public string AirlineCode { get; set; }

        /// <summary>
        /// 航班号
        /// </summary>
        public string FlightNum { get; set; }

        /// <summary>
        /// 起飞时间
        /// </summary>
        public string StartingTime { get; set; }

        /// <summary>
        /// 抵达时间
        /// </summary>
        public string ArrivaTime { get; set; }

        /// <summary>
        /// 飞行时间
        /// </summary>
        public int TotalTravelTime { get; set; }
    }

    /// <summary>
    /// 线路价格套餐
    /// </summary>
    [TableName("TpLineSuites")]
    [PrimaryKey("Id")]
    public class TpLineSuiteModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 线路ID
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 套餐名称
        /// </summary>
        public string PackageDescr { get; set; }
    }

    /// <summary>
    /// 线路行程交通城市From-To
    /// </summary>
    [TableName("TpLineTraffics")]
    [PrimaryKey("Id")]
    public class TpLineTrafficModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 行程ID
        /// </summary>
        public int LineRouteId { get; set; }

        /// <summary>
        /// 离开城市
        /// </summary>
        public string FromCity { get; set; }

        /// <summary>
        /// 抵达城市
        /// </summary>
        public string ToCity { get; set; }

        /// <summary>
        /// 交通方式
        /// </summary>
        public int TrafficId { get; set; }

        /// <summary>
        /// 用时
        /// </summary>
        public string SpendTime { get; set; }
    }

    /// <summary>
    /// 线路图片行程文件表
    /// </summary>
    [TableName("TpLineFiles")]
    [PrimaryKey("Id")]
    public class TpLineFileModel
    {
        public int Id { get; set; }

        public string LineId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 文件类型    [image, document, voice, video]
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// 资源类型   成本附件
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public int IsDel { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 关联图片库
        /// </summary>
        public long PhotoId { get; set; }
    }

    /// <summary>
    /// 团核算成本附件
    /// </summary>
    [TableName("TpTourFiles")]
    [PrimaryKey("Id")]
    public class TpTourFileModel
    {
        /// <summary>
        /// 自增编号
        /// </summary>
        public int Id { get; set; }

        public int TourID { get; set; }

        /// <summary>
        /// 关联单团成本ID
        /// </summary>
        public int? KeyId { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 文件类型    image  document voice video
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// 出团通知   成本附件
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public int IsDel { get; set; }

        /// <summary>
        /// 修订
        /// </summary>
        public int Revision { get; set; }
    }

    [TableName("TpProducts")]
    [PrimaryKey("ProductID")]
    public class TpProductModel
    {
        /// <summary>
        /// 系统自增编号
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string TeamCode { get; set; }

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
        /// 总成本
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 产品供应商
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        ///  0:正常 1：取消
        /// </summary>
        public int IsValid { get; set; }

        public string CreatedBy { get; set; }
        public string OwnerCode { get; set; }

        public DateTime CreatedTime { get; set; }

        [ResultColumn]
        public string TeamName { get; set; }

        [ResultColumn]
        public string SupplierName { get; set; }

        [ResultColumn]
        public string ProductTypeName { get; set; }
    }
}