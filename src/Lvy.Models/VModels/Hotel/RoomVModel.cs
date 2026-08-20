using Lvy.Models.HotelDB;
using System.Collections.Generic;

namespace Lvy.VModels.Hotel
{
    /// <summary>
    /// 酒店房型 后台使用
    /// </summary>
    public class RoomVModel : BaseVModel
    {
        public RoomVModel()
        {
            if (HotelRoomModel == null)
                HotelRoomModel = new HotelRoomModel();
            if (RoomStock == null)
                RoomStock = new HotelStockModel();
            if (HotelStocks == null)
                HotelStocks = new List<HotelStockModel>();
        }

        /// <summary>
        /// 选择的日期
        /// </summary>
        public string SelectedDays { get; set; }

        /// <summary>
        /// 查询对象
        /// </summary>
        public HotelRoomModel HotelRoomModel { get; set; }

        public HotelStockModel RoomStock { get; set; }

        /// <summary>
        /// 查询列表
        /// </summary>
        public List<HotelStockModel> HotelStocks { get; set; }
    }
}