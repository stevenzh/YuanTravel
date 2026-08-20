using System;

namespace Lvy.VModels.Order
{
    public class PrintTravellerVModel
    {
        public bool IsChecked { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 订单编码
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 团号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 出行人姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 证件类型  0:无
        /// </summary>
        public int PassType { get; set; }

        /// <summary>
        /// 证件号
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
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 报价编号
        /// </summary>
        public int PriceId { get; set; }

        /// <summary>
        /// 报价说明 成人   小孩
        /// </summary>
        public string PriceContent { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 小费
        /// </summary>
        public decimal Tips { get; set; }

        /// <summary>
        /// 单房差
        /// </summary>
        public decimal SingleRoom { get; set; }

        /// <summary>
        /// 特价让利
        /// </summary>
        public decimal TeJiaFanLi { get; set; }

        /// <summary>
        /// 0 不占位  1： 站位
        /// </summary>
        public int IsOccupiedQuota { get; set; }

        /// <summary>
        /// 接价
        /// </summary>
        public decimal JiePrice { get; set; }

        /// <summary>
        /// 送价
        /// </summary>
        public decimal SongPrice { get; set; }

        /// <summary>
        /// 自费
        /// </summary>
        public decimal ZiFei { get; set; }

        /// <summary>
        /// 折让
        /// </summary>
        public decimal FanLi { get; set; }

        /// <summary>
        /// 应收
        /// </summary>
        public decimal YsPrice { get; set; }

        /// <summary>
        /// 状态： 0：已取消（未产生费用）  1：已退团（产生费用）  2：有效
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 是否免票 买一送X的场合
        /// 默认：0  不免
        /// </summary>
        public int IsMianPiao { get; set; }

        /// <summary>
        /// CreatedTime
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// CreatedBy
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// ModifiedTime
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// ModifiedBy
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 领队证号
        /// </summary>
        public int LeaderNo { get; set; }

        /// <summary>
        /// 是否是领队.
        /// </summary>
        public bool IsLeader { get; set; }
    }

    public class TravellerCheckedVModel
    {
        public int Id { get; set; }

        public bool IsChecked { get; set; }
    }
}