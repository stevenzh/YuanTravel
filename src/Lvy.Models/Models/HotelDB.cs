using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Models.HotelDB
{
    /// <summary>
    /// 酒店
    /// </summary>
    [TableName("hotels")]
    [PrimaryKey("ID")]
    public partial class HotelModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int ID { get; set; }

        public string HotelCode { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string EnName { get; set; }

        /// <summary>
        /// 酒店名称
        /// </summary>
        public string HotelName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int HotelType { get; set; }

        /// <summary>
        /// 酒店星级
        /// </summary>
        public string RankCode { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string ProvinceCode { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string CityCode { get; set; }

        /// <summary>
        /// 区
        /// </summary>
        public string CityAreaCode { get; set; }

        /// <summary>
        /// 中文地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 英文地址
        /// </summary>
        public string EnAddress { get; set; }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// 联系人邮箱
        /// </summary>
        public string ContactEmail { get; set; }

        /// <summary>
        /// 酒店介绍
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// 入住时间开始
        /// </summary>
        public string InTimeBegin { get; set; }

        /// <summary>
        /// 入住时间截止
        /// </summary>
        public string InTimeEnd { get; set; }

        /// <summary>
        /// 退房时间开始
        /// </summary>
        public string OutTimeBegin { get; set; }

        /// <summary>
        /// 退房时间截止
        /// </summary>
        public string OutTimeEnd { get; set; }

        /// <summary>
        /// 服务设施
        /// </summary>
        public string Facility { get; set; }

        /// <summary>
        /// 取消政策
        /// </summary>
        public string CancelInformation { get; set; }

        /// <summary>
        /// 儿童加床
        /// </summary>
        public string ChildAndBed { get; set; }

        /// <summary>
        /// 线路状态 2：下线 3:上线
        /// </summary>
        public int HotelState { get; set; }

        /// <summary>
        /// 有效无效 0：无效 1：有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

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
        /// 供应商
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 首图
        /// </summary>
        public string LogoPath { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        [ResultColumn]
        public string SupplierName { get; set; }
        [ResultColumn]
        public string CityName { get; set; }
        /// <summary>
        /// 起价
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 酒店图片
        /// </summary>
        [ResultColumn]
        public List<HotelFileModel> FileList { get; set; }

        [ResultColumn]
        public List<HotelRoomModel> RoomList { get; set; }
    }

    /// <summary>
    /// 酒店文件
    /// </summary>
    [TableName("hotel_files")]
    [PrimaryKey("FileID")]
    public partial class HotelFileModel
    {
        /// <summary>
        ///
        /// </summary>
        public int FileID { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string HotelCode { get; set; }

        /// <summary>
        /// 关联编号  房型编号
        /// </summary>
        public int KeyId { get; set; }

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

    /// <summary>
    /// 房型
    /// </summary>
    [TableName("hotel_rooms")]
    [PrimaryKey("RoomID")]
    public partial class HotelRoomModel
    {
        /// <summary>
        ///
        /// </summary>
        public int RoomID { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string HotelCode { get; set; }

        /// <summary>
        /// 房型名称
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// 可住人数
        /// </summary>
        public int Pax { get; set; }

        /// <summary>
        /// 是否可加床
        /// </summary>
        public int AddBed { get; set; }

        /// <summary>
        /// 加床政策
        /// </summary>
        public string AddBedInfo { get; set; }

        /// <summary>
        /// 早餐
        /// </summary>
        public string Breakfast { get; set; }

        /// <summary>
        /// 房型介绍
        /// </summary>
        public string RoomInformation { get; set; }

        /// <summary>
        /// 房间设施
        /// </summary>
        public string RoomFacility { get; set; }

        public bool IsValid { get; set; }

        [ResultColumn]
        public List<HotelRoomBedModel> BedList { get; set; }
        /// <summary>
        /// 房型照片
        /// </summary>
        [ResultColumn]
        public List<HotelFileModel> FileList { get; set; }
    }

    public class RoomToBedRelator
    {
        private HotelRoomModel RoomModel;

        public HotelRoomModel MapIt(HotelRoomModel room, HotelRoomBedModel bed)
        {
            if (room == null)
                return RoomModel;
            if (RoomModel != null && RoomModel.RoomID == room.RoomID)
            {
                RoomModel.BedList.Add(bed);
                return null;
            }
            var prev = RoomModel;
            RoomModel = room;
            RoomModel.BedList = new List<HotelRoomBedModel>();
            RoomModel.BedList.Add(bed);
            return prev;
        }
    }

    /// <summary>
    /// 床型
    /// </summary>
    [TableName("hotel_room_beds")]
    [PrimaryKey("BedID")]
    public partial class HotelRoomBedModel
    {
        /// <summary>
        ///
        /// </summary>
        public int BedID { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int RoomID { get; set; }

        /// <summary>
        /// 床型
        /// </summary>
        public int BedType { get; set; }

        /// <summary>
        /// 床型名称
        /// </summary>
        public string BedName { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string BedIntroduce { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int BedNum { get; set; }
    }

    /// <summary>
    /// 床型
    /// </summary>
    [TableName("hotel_stock")]
    [PrimaryKey("StockID")]
    public partial class HotelStockModel
    {
        /// <summary>
        ///
        /// </summary>
        public int StockID { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string HotelCode { get; set; }

        /// <summary>
        /// 床型
        /// </summary>
        public int RoomID { get; set; }

        /// <summary>
        /// 结算价
        /// </summary>
        public decimal SettlePrice { get; set; }
        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 入住日期
        /// </summary>
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public int Quota { get; set; }
    }
}