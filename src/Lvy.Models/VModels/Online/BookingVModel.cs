using Lvy.Models;
using Lvy.Models.ProductDB;
using System;
using System.Collections.Generic;

namespace Lvy.VModels.Booking
{
    public class BookingVModel : BaseVModel
    {
        /// <summary>
        /// 团信息
        /// </summary>
        public TpTourPlanModel Tour { get; set; }

        /// <summary>
        /// 该团库存
        /// </summary>
        public QuotaModel Quota { get; set; }

        /// <summary>
        /// 上车点
        /// </summary>
        public List<TpLineBusPointModel> BusPoints { get; set; }

        /// <summary>
        /// 该团期对应的价格类型
        /// </summary>
        public List<TpPriceModel> PriceModels { get; set; }

        /// <summary>
        ///  订单来源键值对
        /// </summary>
        public List<KeyValueBean> OrderSourceBean { get; set; }

        public TpLineModel LineModel { get; set; }

        #region Form提交

        /// <summary>
        /// 团编号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 分销商联系姓名
        /// </summary>
        public string Managers { get; set; }

        /// <summary>
        /// 分销商联系人电话
        /// </summary>
        public string ManagerPhone { get; set; }

        /// <summary>
        /// 分销商
        /// </summary>
        public string BookingCustomer { get; set; }

        /// <summary>
        /// 分销商名称
        /// </summary>
        public string BookingCustomerName { get; set; }

        /// <summary>
        /// 分销商联系人
        /// </summary>
        public string ContactCode { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int OrderState { get; set; }

        /// <summary>
        /// 跟单状态
        /// </summary>
        public int TraceState { get; set; }

        public string SalesTeamId { get; set; }
        public string SalerCode { get; set; }

        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrderSource { get; set; }

        /// <summary>
        /// OTA关联订单号
        /// </summary>
        public string JoinOrderCode { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 订单备注2 系统备注
        /// </summary>
        public string Remark2 { get; set; }

        /// <summary>
        /// 接送点编号
        /// </summary>
        public int BusPointId { get; set; }

        public string SettleCustomer { get; set; }

        public int SettlePlatForm { get; set; }

        /// <summary>
        /// 开单人数
        /// </summary>
        public int TravellerCount { get; set; }

        /// <summary>
        /// 开单预留时间
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

        public string OpenPriceStr { get; set; }

        /// <summary>
        /// 游客信息
        /// </summary>
        public List<BookingPostVModel> Travellers { get; set; }

        #endregion Form提交
    }

    /// <summary>
    /// 预定页面提交model
    ///
    /// </summary>
    public class BookingPostVModel
    {
        /// <summary>
        /// 客人姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 证件类型
        /// </summary>
        public int PassType { get; set; }

        /// <summary>
        /// 证件号码
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
        /// 报价编号
        /// </summary>
        public string PriceId { get; set; }

        /// <summary>
        /// 折让
        /// </summary>
        public string FanLi { get; set; }

        /// <summary>
        /// 结算客户协议折让类型 每个游客固定折让，还是百分比折让
        /// </summary>
        public int DiscountType { get; set; }

        public decimal DiscountPerCent { get; set; }
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// 是否自费
        /// </summary>
        public bool IsZiFei
        {
            get
            {
                if (ZiFei == "on")
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 是否自费
        /// </summary>
        public bool IsSingleRoom
        {
            get
            {
                if (SingleRoom == "on")
                    return true;
                else
                    return false;
            }
        }

        public bool IsTax
        {
            get
            {
                if (Tax == "on")
                    return true;
                else
                    return false;
            }
        }

        public bool IsVisaPrice
        {
            get
            {
                if (VisaPrice == "on")
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 签证费
        /// on：true
        /// off:false
        /// </summary>
        public string VisaPrice { get; set; }

        /// <summary>
        /// 税费
        /// on：true
        /// off:false
        /// </summary>
        public string Tax { get; set; }

        /// <summary>
        /// 自费开关
        /// on：true
        /// off:false
        /// </summary>
        public string ZiFei { get; set; }

        /// <summary>
        /// 单房差
        /// on：true
        /// off:false
        /// </summary>
        public string SingleRoom { get; set; }

        /// <summary>
        /// 是否免票 买一送X的场合
        /// </summary>
        public int IsMianPiao { get; set; }

        /// <summary>
        /// 客人备注
        /// </summary>
        public string Remark { get; set; }
    }
}