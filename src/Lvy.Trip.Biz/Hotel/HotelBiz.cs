using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.HotelDB;
using Lvy.Trip.Dao;
using Lvy.VModels.Hotel;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    ///  账户处理模块
    ///
    /// </summary>
    public class HotelBiz : BaseBiz
    {
        private readonly HotelDao _dao = new HotelDao();
        private readonly HotelFileDao _fileDao = new HotelFileDao();
        private readonly HotelRoomDao _roomDao = new HotelRoomDao();
        private readonly HotelRoomBedDao _bedDao = new HotelRoomBedDao();
        private readonly HotelStockDao _stockDao = new HotelStockDao();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<HotelModel> GetPagedList(HotelVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT h.*, bd.Name AS CityName
FROM hotels h
LEFT JOIN basedestination bd ON h.CityCode=bd.Id
WHERE h.OwnerCode=@0 ", vModel.OwnerCode);

            if (!vModel.HotelModel.HotelName.IsNullOrEmpty())
                sql.Append(" AND h.HotelName LIKE @0", AnsiLike(vModel.HotelModel.HotelName));
            if (!string.IsNullOrEmpty(vModel.HotelModel.TeamID))
                sql.Append(" AND h.TeamID=@0", AnsiLike(vModel.HotelModel.TeamID));

            try
            {
                var sortBy = vModel.SortCollection[vModel.SortKey];
                if (sortBy != null)
                    sql.Append(" ORDER BY " + sortBy.Key);
            }
            catch { }

            return _dao.Pager(vModel.Hotels.PageIndex, vModel.Hotels.PageSize, sql.SQL, sql.Arguments);
        }

        public string InsertHotel(HotelModel model)
        {
            model.HotelCode = "H" + DBTools.GetHotelSeqNo();
            _dao.Insert(model);

            return model.HotelCode;
        }

        /// <summary>
        /// 得到一个账户对象
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public HotelModel GetByCode(string code)
        {
            return _dao.FirstOrDefault("SELECT * FROM hotels WHERE HotelCode=@0 ", code);
        }

        /// <summary>
        /// 更新账户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(HotelModel model)
        {
            return _dao.Update(model);
        }

        public int UpdateHotelState(string hotelCode, int state)
        {
            return _dao.Update("SET HotelState=@1 WHERE HotelCode=@0 ", hotelCode, state);
        }

        /// <summary>
        /// 通过ownercode取得客户信息列表
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetAllCustomerBeans(string ownerCode)
        {
            string sql = @" SELECT Code as `Key`, Name AS Value FROM CrmCustomer WHERE IsValid=1 AND OwnerCode=@0";
            return _dao.Query<KeyValueBean>(sql, ownerCode).ToList();
        }

        /// <summary>
        /// 获取账号的编码和名称集合
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public List<KeyValueBean> GetAllAccountBeans(string customerCode, string ownerCode)
        {
            string sql = @"SELECT Code AS `Key`, Name AS Value FROM CrmAccount WHERE IsValid=1 AND (CustomerCode=@0 OR CustomerCode=@1) ";

            return _dao.Query<KeyValueBean>(sql, Ansi(customerCode), ownerCode).ToList();
        }

        public List<HotelFileModel> GetFileList(string code)
        {
            return _fileDao.Fetch("SELECT * FROM hotel_files WHERE HotelCode=@0", code);
        }
        public List<HotelFileModel> GetFileList(string code, int roomid)
        {
            return _fileDao.Fetch("SELECT * FROM hotel_files WHERE HotelCode=@0 AND KeyID=@1 ", code, roomid);
        }

        public void AddPhoto(HotelFileModel model)
        {
            _fileDao.Insert(model);
        }

        public HotelFileModel GetHotelFileModel(int id)
        {
            return _fileDao.GetById(id);
        }
        public void DeleteFile(int id)
        {
            _fileDao.Update("SET IsValid=0 WHERE FileID=@0", id);
        }

        public int SetPrimaryPic(string hoetlCode, string filePath)
        {
            return _dao.Update("SET LogoPath=@1 WHERE HotelCode=@0 ", hoetlCode, filePath);
        }

        public List<HotelRoomModel> GetRooms(string code)
        {
            return _roomDao.GetRooms(code);
        }

        public void AddRoom(HotelRoomModel model)
        {
            _roomDao.Insert(model);
        }

        public HotelRoomModel GetRoomByID(int id)
        {
            return _roomDao.GetById(id);
        }

        public void UpdateRoom(HotelRoomModel item)
        {
            _roomDao.Update("SET RoomName=@1, AddBed=@2, AddBedInfo=@3, Breakfast=@4  WHERE RoomID=@0 ", item.RoomID, item.RoomName, item.AddBed, item.AddBedInfo, item.Breakfast);
        }

        public List<HotelRoomBedModel> GetBedByRoomID(int id)
        {
            return _bedDao.Fetch("SELECT * FROM hotel_room_beds WHERE RoomID=@0 ", id);
        }

        public void delBeds(int[] ids)
        {
            if (ids.Length > 0)
            {
                _bedDao.Delete("WHERE BedID IN ( @0 )", ids);
            }
        }

        public void UpdateBed(HotelRoomBedModel item)
        {
            _bedDao.Update("SET BedType=@1, BedIntroduce=@2, BedNum=@3 WHERE BedID=@0 ", item.BedID, item.BedType, item.BedIntroduce, item.BedNum);
        }

        public void AddBed(HotelRoomBedModel item)
        {
            _bedDao.Insert(item);
        }

        public List<HotelStockModel> GetRoomStock(int id)
        {
            return _stockDao.Fetch("SELECT * FROM hotel_stock WHERE RoomID=@0 ", id);
        }

        public void UpdateRoomStock(HotelStockModel model)
        {
            var entity = _stockDao.FirstOrDefault("SELECT * FROM hotel_stock WHERE RoomID=@0 AND CheckInDate=@1 ", model.RoomID, model.CheckInDate);
            if (entity != null)
            {
                _stockDao.Update("SET MarketPrice=@1,SettlePrice=@2,Quota=@3 WHERE StockID=@0", entity.StockID, model.MarketPrice, model.SettlePrice, model.Quota);
            }
            else
            {
                _stockDao.Insert(model);
            }
        }

        /// <summary>
        /// 更新酒店起价
        /// </summary>
        public void UpdateSalePrice(string code = default(string))
        {
            Sql sql = new Sql();
            sql.Append(@"UPDATE hotels SET SalePrice=
(
SELECT MIN(hs.MarketPrice) FROM hotel_stock hs 
  INNER JOIN hotel_rooms hr ON hs.RoomID=hr.RoomID
  WHERE hr.IsValid=1 AND hs.CheckInDate> NOW()
  AND hotels.HotelCode = hr.HotelCode ) ");
            if (!string.IsNullOrEmpty(code))
                sql.Append(" WHERE HotelCode=@0 ", code);

            _dao.Execute(sql.SQL, sql.Arguments);
        }


    }
}