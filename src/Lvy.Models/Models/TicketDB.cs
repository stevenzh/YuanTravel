using Lvy.Models.BaseDB;
using Lvy.Models.TourDB;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Models.TicketDB
{
    /// <summary>
    /// 通用旅游商品
    /// </summary>
    [TableName("TktProduct")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TktProductModel
    {
        /// <summary>
        /// 自增列
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 产品负责部门
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 全拼
        /// </summary>
        public string PinYin { get; set; }

        /// <summary>
        /// 简拼
        /// </summary>
        public string JPinYin { get; set; }

        /// <summary>
        /// 关联景区编码
        /// </summary>
        public string PlaceCode { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 购票方式
        ///     1:固定签单，2:特殊签单，3:任务单，4:特殊任务单
        ///     签单： 就是旅行社和景区通过协议约定 旅行社来支付 所以门票订单需要付款
        ///     任务单： 应该是景区到付， 通过返利返还预定客户 佣金
        /// </summary>
        public int TktType { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 库存方式
        /// 1:无限库存，2:限制库存，3:日期库存
        /// </summary>
        public int TuiJianType { get; set; }

        /// <summary>
        /// 价格模式 1-固定价格  2-日期价格
        /// </summary>
        public int PriceMode { get; set; }

        /// <summary>
        /// 商品大类    ProductAllTypeEnum
        /// 1-旅游线路 2-机票 3-签证服务 4-酒店住宿 5-景区门票 6-套餐 7-火车票/汽车票 8-WIFI/上网卡 9-旅游特产 10-租车
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// 商品小类
        /// </summary>
        public string ProductCategory { get; set; }

        /// <summary>
        /// 状态  0:无效, 2:下线, 3:上线
        /// </summary>
        public int ProductState { get; set; }

        /// <summary>
        /// 预定须知
        /// </summary>
        public string BookingDesc { get; set; }

        /// <summary>
        /// 商品说明
        /// </summary>
        public string ProductDesc { get; set; }

        /// <summary>
        /// CreatedBy
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// CreatedTime
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 修改人
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
        /// 主题标签
        /// </summary>
        public string Themes { get; set; }

        /// <summary>
        /// 首图地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 总库存
        /// </summary>
        public int PlanQuota { get; set; }

        /// <summary>
        /// 预留库存
        /// </summary>
        public int HoldQuota { get; set; }

        /// <summary>
        /// 已用库存
        /// </summary>
        public int UsedQuota { get; set; }

        /// <summary>
        /// 【使用期限】开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 【使用期限】结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 预订限额（最多购买份数）
        /// </summary>
        public int LimitQuota { get; set; }

        /// <summary>
        /// 【购买期限】开始日期
        /// </summary>
        public DateTime? BeginBuyTime { get; set; }

        /// <summary>
        /// 【购买期限】截止日期
        /// </summary>
        public DateTime? LastDate { get; set; }

        /// <summary>
        /// 预订提前天数， 门票一般提前一天  国内短线跟团提前一天  出境根据签证办理时间而定
        /// </summary>
        public int PreDays { get; set; }
        /// <summary>
        /// 预订时间限制  例如 提前一天 16:00 预约
        /// </summary>
        public string PreTime { get; set; }

        /// <summary>
        /// 是否外部录入
        /// </summary>
        public bool IsImport { get; set; }

        /// <summary>
        /// 审核标记   0 初始 1 提交申请  2 挂起  4 通过
        /// </summary>
        public int ImportState { get; set; }

        /// <summary>
        /// 关联景区名称
        /// </summary>
        [ResultColumn]
        public string PlaceName { get; set; }

        [ResultColumn]
        public string TeamName { get; set; }

        /// <summary>
        /// 市场价  标准价
        /// </summary>
        [ResultColumn]
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 结算价  标准价
        /// </summary>
        [ResultColumn]
        public decimal SettlePrice { get; set; }

        /// <summary>
        /// 专管员
        /// </summary>
        [ResultColumn]
        public List<TktAdminModel> Admins { get; set; }

        [ResultColumn]
        public List<TktPriceRuleModel> PriceRules { get; set; }

        [ResultColumn]
        public BasePlaceModel Place { get; set; }
        [ResultColumn]
        public List<TktFileModel> FileList { get; set; }
    }

    /// <summary>
    /// 商品专管员
    /// </summary>
    [TableName("TktAdmin")]
    [PrimaryKey("Id")]
    public class TktAdminModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 门票产品Id
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 专管员账号
        /// </summary>
        public string AccountCode { get; set; }
    }

    /// <summary>
    /// 价格规则表
    /// </summary>
    [TableName("TktPriceRule")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TktPriceRuleModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// 价格类型  标准价
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// 市场价  标准价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 结算价  标准价
        /// </summary>
        public decimal SettlePrice { get; set; }

        /// <summary>
        /// 购票方式  1:固定签单，2:特殊签单，3:任务单，4:特殊任务单
        /// </summary>
        public int TktType { get; set; }

        /// <summary>
        /// 签单价|返利
        /// </summary>
        public decimal SysPrice { get; set; }

        /// <summary>
        /// 是否常规报价
        ///     0:否 1：是
        /// </summary>
        public int IsGeneral { get; set; }

        public int IsValid { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        ///  one pricerule to many price
        /// </summary>
        [ResultColumn]
        public List<TktPriceModel> Prices { get; set; }
    }

    /// <summary>
    /// one-to-many关系对象
    /// </summary>
    public class TktProductToPriceRuleRelator
    {
        private TktProductModel current = null;

        public TktProductModel MapIt(TktProductModel product, TktPriceRuleModel priceRule)
        {
            if (product == null)
                return current;
            if (current != null && current.Id == product.Id)
            {
                current.PriceRules.Add(priceRule);
                return null;
            }

            // Save the current author
            var prev = current;
            current = product;
            current.PriceRules = new List<TktPriceRuleModel>();
            current.PriceRules.Add(priceRule);
            return prev;
        }
    }

    /// <summary>
    /// 商品价格表
    /// </summary>
    [TableName("TktPrice")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TktPriceModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 价格规则编号
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// 价格类型
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 同业结算价
        /// </summary>
        public decimal SettlePrice { get; set; }

        /// <summary>
        /// 购票方式  1:固定签单，2:特殊签单，3:任务单，4:特殊任务单
        /// </summary>
        public int TktType { get; set; }

        /// <summary>
        /// 签单价（成本）|返利（同行结算价-成本 ）
        /// </summary>
        public decimal SysPrice { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 标准价
        /// </summary>
        public int IsStandard { get; set; }

        /// <summary>
        /// 价格政策说明
        /// </summary>
        public string PriceDesc { get; set; }

        /// <summary>
        /// 修改人
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
    }

    /// <summary>
    /// 规则价格中间表
    /// </summary>
    [TableName("TktRulePriceMap")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TktRulePriceMapModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 价格规则编号
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime CurrentDate { get; set; }

        /// <summary>
        /// 总库存
        /// </summary>
        public int PlanQuota { get; set; }

        /// <summary>
        /// 已用库存
        /// </summary>
        public int UsedQuota { get; set; }
    }

    /// <summary>
    /// one-to-many关系对象
    /// </summary>
    public class TktPriceRuleToPriceRelator
    {
        private TktPriceRuleModel current = null;

        public TktPriceRuleModel MapIt(TktPriceRuleModel priceRule, TktPriceModel price)
        {
            if (priceRule == null)
                return current;
            if (current != null && current.Id == priceRule.Id)
            {
                current.Prices.Add(price);
                return null;
            }

            // Save the current author
            var prev = current;
            current = priceRule;
            current.Prices = new List<TktPriceModel>();
            current.Prices.Add(price);
            return prev;
        }
    }

    /// <summary>
    /// 订单明细表
    /// 主订单： TpTourBalanceModel
    /// </summary>
    [TableName("TktOrders")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TktOrdersModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 团单ID
        /// </summary>
        public string MasterOrderCode { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string DestId { get; set; }

        /// <summary>
        /// 门票产品编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品价格编号  成人票|儿童票
        /// </summary>
        public int PriceId { get; set; }

        /// <summary>
        /// 价格类型
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 结算价
        /// </summary>
        public decimal SettlePrice { get; set; }

        /// <summary>
        /// 签单价|返利
        /// </summary>
        public decimal SysPrice { get; set; }

        /// <summary>
        /// 购票方式
        ///     1:固定签单，2:特殊签单，3:任务单，4:特殊任务单
        /// </summary>
        public int TktType { get; set; }

        /// <summary>
        /// 人数
        /// </summary>
        public int PeopleNum { get; set; }

        /// <summary>
        /// 应收
        /// </summary>
        public decimal YsPrice { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 入园时间
        /// </summary>
        public DateTime OutDate { get; set; }

    }


    /// <summary>
    /// one-to-many关系对象
    /// </summary>
    public class TktOrderToDetailRelator
    {
        private TpTourBalanceModel current;

        public TpTourBalanceModel MapIt(TpTourBalanceModel a, TktOrdersModel p)
        {
            if (a == null)
                return current;
            if (current != null && current.MasterOrderCode == a.MasterOrderCode)
            {
                current.OrderDetails.Add(p);
                return null;
            }

            // Save the current author
            var prev = current;
            current = a;
            current.OrderDetails = new List<TktOrdersModel>();
            current.OrderDetails.Add(p);
            return prev;
        }
    }

    /// <summary>
    /// 任务单
    /// </summary>
    [TableName("TktTaskOrder")]
    [PrimaryKey("Id")]
    [Serializable]
    public class TktTaskOrderModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string MasterOrderCode { get; set; }

        /// <summary>
        /// 团号
        /// </summary>
        public string TourCode { get; set; }

        /// <summary>
        /// 人数
        /// </summary>
        public int TouristNumber { get; set; }

        /// <summary>
        /// 出团 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 出团 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 导游名称
        /// </summary>
        public string GuideName { get; set; }

        /// <summary>
        /// 游程安排
        /// </summary>
        public string RouteDetail { get; set; }

        /// <summary>
        /// 交通
        /// </summary>
        public string Traffic { get; set; }

        /// <summary>
        /// 门票
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// 住宿
        /// </summary>
        public string Hotel { get; set; }

        /// <summary>
        /// 餐费
        /// </summary>
        public string Catering { get; set; }

        /// <summary>
        /// 其他
        /// </summary>
        public string Other { get; set; }

        /// <summary>
        /// 景区备注
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 预支团款
        /// </summary>
        public decimal PreMoney { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }
    }

    /// <summary>
    /// 商品小分类（树形）
    /// </summary>
    [TableName("tkt_category")]
    [PrimaryKey("ID")]
    public partial class TktCategoryModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        /// <summary>
        /// 大类 1:线路 3:签证 4:酒店 9:其他
        /// </summary>
        public string ProductType { get; set; }
        public int ParentID { get; set; }
        public bool IsLeaf { get; set; }
        public bool IsValid { get; set; }
        public int SortOrder { get; set; }
        public string Remarks { get; set; }
        [ResultColumn]
        public string ParentName { get; set; }
        [ResultColumn]
        public TktCategoryModel ParentNode { get; set; }
    }


    /// <summary>
    /// 商品附件
    /// </summary>
    [TableName("tkt_files")]
    [PrimaryKey("FileID")]
    public partial class TktFileModel
    {
        /// <summary>
        ///
        /// </summary>
        public int FileID { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ProductID { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        ///  1:酒店图片  2:房型图片
        /// </summary>
        public string Type { get; set; }

        public object ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public int IsValid { get; set; }
        public int FileSize { get; set; }
    }

}