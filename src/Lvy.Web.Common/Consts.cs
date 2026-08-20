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
}
