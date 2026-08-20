using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arch.Common;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Order
{

    /// <summary>
    /// 账单详情
    /// </summary>
    public class OrderConfirmPrintVModel : BaseVModel
    {

        public OrderConfirmPrintVModel()
        {
            if (OrderModel == null)
                OrderModel = new TpOrderModel();
            if (CrmAccountModel == null)
                CrmAccountModel = new CrmAccountModel();
            if (LineModel == null)
                LineModel = new TpLineModel();
            if (TravellerModels == null)
                TravellerModels = new List<TpTravellerModel>();
            if (TravellerVModels == null)
                TravellerVModels = new List<TpTravellerVModel>();
            if (BusTravellerVModels == null)
                BusTravellerVModels = new List<BusTravellerVModel>();
            if (LineBusPointModel == null)
                LineBusPointModel = new TpLineBusPointModel();
            if (PlatformModel == null)
                PlatformModel = new SysPlatformModel();
            if (CustomerModel == null)
                CustomerModel = new CrmCustomerModel();

        }

        /// <summary>
        /// 商户电子印章
        /// </summary>
        public string ElecCertifyPath
        {
            get
            {
                return AppSetting.Get("UploadFileRoot") + PlatformModel.ElecCertifyPath;
            }
        }

        /// <summary>
        /// 商户图标
        /// </summary>
        public string LogoPath
        {
            get
            {
                return AppSetting.Get("UploadFileRoot") + PlatformModel.IconPath;
            }
        }
        /// <summary>
        /// 订单信息
        /// </summary>
        public TpOrderModel OrderModel { get; set; }

        /// <summary>
        /// 账户信息
        /// </summary>
        public CrmAccountModel CrmAccountModel { get; set; }

        /// <summary>
        /// 线路信息
        /// </summary>
        public TpLineModel LineModel { get; set; }

        /// <summary>
        /// 行程信息
        /// </summary>
        public List<TpLineRouteModel> LineRoutes { get; set; }

        /// <summary>
        ///巴士  游客信息
        /// </summary>
        public List<TpTravellerModel> TravellerModels { get; set; }

        /// <summary>
        /// 非巴士 游客信息
        /// </summary>
        public List<TpTravellerVModel> TravellerVModels { get; set; }

        /// <summary>
        /// 巴士游客信息
        /// </summary>
        public List<BusTravellerVModel> BusTravellerVModels { get; set; }
        /// <summary>
        /// 游客价格说明
        /// </summary>
        public List<PersonSetModel> PersonModels { get; set; }

        /// <summary>
        /// 上车点信息
        /// </summary>
        public TpLineBusPointModel LineBusPointModel { get; set; }

        /// <summary>
        /// 平台信息
        /// </summary>
        public SysPlatformModel PlatformModel { get; set; }

        /// <summary>
        /// 商户信息
        /// </summary>
        public CrmCustomerModel CustomerModel { get; set; }

        /// <summary>
        /// 座位编号
        /// </summary>
        public string SeatNums { get; set; }

        /// <summary>
        /// 地接社专管员
        /// </summary>
        public CrmAccountModel LocalTravelAgency { get; set; }

        /// <summary>
        /// 组团社专管员
        /// </summary>
        public CrmAccountModel OrganizingTravelAgency { get; set; }
        /// <summary>
        /// 价格列表
        /// </summary>
        public List<TpPriceModel> PriceList { get; set; }
        /// <summary>
        /// 子订单列表
        /// </summary>
        public List<TpChildOrderModel> ChildList { get; set; }
        /// <summary>
        /// 开班计划
        /// </summary>
        public TpTourPlanModel TourPlan { get; set; }
    }

    public class PersonSetModel
    {
        /// <summary>
        /// 价格类型
        /// </summary>
        public string PersonType { get; set; }
        /// <summary>
        /// 人数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 折扣
        /// </summary>
        public decimal Discount { get; set; }
        /// <summary>
        /// 合计
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Note { get; set; }

    }
}
