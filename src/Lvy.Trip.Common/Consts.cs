namespace Lvy.Web.Common
{
    public class Consts
    {
        #region cache key 缓存关键字

        public static readonly string Destination = "BaseDestinationModel"; // 目的地
        public static readonly string TktProduct = "TktProductModel";       // 门票产品
        public static readonly string BasePlace = "BasePlaceModel"; // 目的地

        public static readonly string AccountStrDic = "AccountStrDic";   // 账户字典
        public static readonly string CustomerStrDic = "CustomerStrDic"; // 客户字典
        public static readonly string SupplierStrDic = "SupplierStrDic"; // 供应商字典
        public static readonly string TeamStrDic = "TeamStrDic";         // 部门字典
        public static readonly string BrandStrDic = "BrandStrDic";       // 品牌字典
        public static readonly string AirlineStrDic = "AirlineStrDic";   // 航空公司字典
        public static readonly string TicketStrDic = "TicketStrDic";     // 门票字典
        public static readonly string PlaceStrDic = "PlaceStrDic";       // 门票字典

        public static readonly string TourProduct = "TourProduct"; //产品字典
        public static readonly string VisaProduct = "VisaProduct"; //产品字典


        public static readonly string WeixinQrScene  = "WeixinQrScene"; // 微信二维码扫描序列

        public static readonly string HostCode = "Host."; //产品字典

        // 页面缓存KEY
        public static readonly string PageCustomerController = "PageCustomerController";
        public static readonly string PageAccountController = "PageAccountController";
        public static readonly string PageContactController = "PageContactController";
        public static readonly string PageSupplierController = "PageSupplierController";
        public static readonly string PageLineController = "PageLineController";
        public static readonly string PageOrderController = "PageOrderController";

        #endregion

        #region cachetime

        /// <summary>
        /// 页面缓存时间1分钟
        /// </summary>
        public const int OutputCacheDuration0 = 60;

        /// <summary>
        /// 页面缓存时间2.5分钟
        /// </summary>
        public const int OutputCacheDuration1 = 150;

        /// <summary>
        /// 页面缓存时间10分钟
        /// </summary>
        public const int OutputCacheDuration2 = 600;

        /// <summary>
        /// 页面缓存时间1小时 
        /// </summary>
        public const int OutputCacheDuration3 = 3600;

        /// <summary>
        /// 页面缓存时间8小时 
        /// </summary>
        public const int OutputCacheDuration4 = 30000;

        /// <summary>
        /// 页面缓存时间24小时 
        /// </summary>
        public const int OutputCacheDurationDay = 30000 * 4;

        #endregion

        #region Test 
        public static string GetTestHost(string host)
        {
            var result = host;
            if (host.Equals("localhost")) result = "manage.sh-cct.cn";

            return result;
        }
        #endregion
    }


    public class Enums
    {
        public static readonly string HostProfileEnum = "HostProfileEnum";  // 商户设置

        public static readonly string SexEnum = "SexEnum";                 // 性别
        public static readonly string IsValidEnum = "IsValidEnum";         // 是否有效
        public static readonly string FuncTypeEnum = "FuncTypeEnum";       // 功能类型
        public static readonly string NoticeTypeEnum = "NoticeTypeEnum";   // 通知类型
        public static readonly string TaskStatusEnum = "TaskStatusEnum";   // 任务状态
        public static readonly string PlaceLevelEnum = "PlaceLevelEnum";   // 景区星级
        public static readonly string RebateEnum = "RebateEnum";           // 折扣方式 百分比 固定金额
        public static readonly string WorkFlowEnum = "WorkFlowEnum";       // 工作流列表

        public static readonly string PaymentTypeEnum = "PaymentTypeEnum";            // 结算方式    现结、月结
        public static readonly string PayTypeEnum = "PayTypeEnum";                    // 客户付款方式   现金、支票、支付宝
        public static readonly string CustomerChannelEnum = "CustomerChannelEnum";    // 商户类型  1：同业，2：平台，3：电商
        public static readonly string CustomerRankEnum = "CustomerRankEnum";          // 客户等级：0：新客户，1：优良，2：优质
        public static readonly string CustomerActivityEnum = "CustomerActivityEnum";  // 客户活跃度：0：高度活跃，1:活跃 1：普通，2：沉睡
        public static readonly string CustomerStateEnum = "CustomerStateEnum";        // 客户审核状态：0：未审核 1：已审核 2:审核不通过

        public static readonly string AccountTypeEnum = "AccountTypeEnum";         // 账号类型
        public static readonly string DestLevelEnum = "DestLevelEnum";             // 目的地分类

        public static readonly string ProductTypeEnum = "ProductTypeEnum";         // 产品类型 可控 1:线路 门票 签证 酒店 <其他>
        public static readonly string ProductAllTypeEnum = "ProductAllTypeEnum";   // 产品类型 所有旅游产品
        //public static readonly string SupplyProductTypeEnum = "SupplyProductTypeEnum";   // 供应商经营产品类型  1:出境线路 2：国内线路 [3：入境线路] 4：签证服务 5：门票 6：酒店 [7：机票]


        public static readonly string LineTypeEnum = "LineTypeEnum";          // 线路类型       跟团|自由行|当地参团|自驾
        public static readonly string LineScopeEnum = "LineScopeEnum";        // 线路目的地范围  周边|国内|港澳台|出境
        public static readonly string TrafficTypeEnum = "TrafficTypeEnum";    // 交通类型
        public static readonly string DietTypeEnum = "DietTypeEnum";          // 餐饮类型
        public static readonly string ImportStateEnum = "ImportStateEnum";    // 外挂产品审核状态
        public static readonly string TourSourceEnum = "TourSourceEnum";      // 团渠道来源
        public static readonly string TourStateEnum = "TourStateEnum";        // 团状态 【废弃】
        public static readonly string AuditStateEnum = "AuditStateEnum";      // 团单审核状态
        public static readonly string DepartCodeEnum = "DepartCodeEnum";      // 员工职能

        public static readonly string OrderStateEnum = "OrderStateEnum";             // 订单状态
        public static readonly string OrderTraceStateEnum = "OrderTraceStateEnum";   // 订单跟单状态
        public static readonly string OrderCancelStateEnum = "OrderCancelStateEnum"; // 订单取消状态
        public static readonly string PayInStateEnum = "PayInStateEnum";             // 缴款单状态

        public static readonly string PassTypeEnum = "PassTypeEnum";         // 证件类型
        public static readonly string OutCityEnum = "OutCityEnum";           // 出发城市 出发地

        public static readonly string TuiJianTypeEnum = "TuiJianTypeEnum";   // 出团计划  推荐方式
        public static readonly string QuotaSourceEnum = "QuotaSourceEnum";   // 库存来源
        public static readonly string ResTypeEnum = "ResTypeEnum";           // 文件资源表 资源类型
        public static readonly string TourTypeEnum = "TourTypeEnum";         // Tour.TourType 团队性质
        public static readonly string TpPriceTypeEnum = "TpPriceTypeEnum";   // 线路报价类型
        public static readonly string CurrencyEnum = "CurrencyEnum";         // 币种
        public static readonly string CostStatusEnum = "CostStatusEnum";     // 成本付款状态

        public static readonly string SupplierCostItemsEnum = "SupplierCostItemsEnum";      // crmsupplier.items 成本项目

        public static readonly string FileBusinessEnum = "FileBusinessEnum";     // 附件业务类型

        public static readonly string InboundInvoiceTitleEnum = "InboundInvoiceTitleEnum";     // 国内发票项目
        public static readonly string OutboundInvoiceTitleEnum = "OutboundInvoiceTitleEnum";   // 出境发票项目

        public static readonly string TourAuditStateEnum = "TourAuditStateEnum";     //团单审核状态

        #region ticket

        public static readonly string TktTypeEnum = "TktTypeEnum";               // 购票方式
        public static readonly string TktTuiJianTypeEnum = "TktTuiJianTypeEnum"; // 库存限制方式
        // public static readonly string TktProductTypeEnum = "TktProductTypeEnum"; // 产品类型
        // public static readonly string TktOrderStateEnum = "TktOrderStateEnum";   // 门票订单状态

        #endregion ticket

        #region wechat

        public static readonly string MemberBindingEnum = "MemberBindingEnum";      // 微信绑定
        public static readonly string WeixinSubscribeEnum = "WeixinSubscribeEnum";  // 微信关注

        #endregion wechat

        #region visa

        public static readonly string VisaAreaEnum = "VisaAreaEnum";      // 签证领区
        public static readonly string VisaTypeEnum = "VisaTypeEnum";      // 签证类型  旅游/商务/探亲
        public static readonly string VisaVTypeEnum = "VisaVTypeEnum";    // 签证类型  个人/团队
        public static readonly string ContinentEnum = "ContinentEnum";    // 洲
        public static readonly string VisaStateEnum = "VisaStateEnum";    // 签证产品状态
        public static readonly string InterviewTypeEnum = "InterviewTypeEnum";    // 签证产品状态

        public static readonly string VisaOrderStatusEnum = "VisaOrderStatusEnum"; // 签证订单状态
        public static readonly string PayStatusEnum = "PayStatusEnum";             // 支付状态
        public static readonly string OrderSourceEnum = "OrderSourceEnum";         // 订单来源

        #endregion visa

        #region site 

        public static readonly string SiteBannerEnum = "SiteBannerEnum";           // 站点滚动栏类型  官网|微信|其他

        #endregion

        #region Hotel

        public static readonly string HotelTypeEnum = "HotelTypeEnum";           // 酒店类型  酒店|客栈|民宿|公寓|旅馆|青旅|农家乐
        public static readonly string HotelLevelEnum = "HotelLevelEnum";         // 酒店星级  三星|四星|五星
        public static readonly string HotelServiceEnum = "HotelServiceEnum";     // 酒店服务设施  机场班车|吸烟区|唤醒服务|
        public static readonly string RoomFacilityEnum = "RoomFacilityEnum";     // 房间设施
        public static readonly string BedTypeEnum = "BedTypeEnum";               // 酒店床型

        #endregion


    }

}
