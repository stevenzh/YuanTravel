using Lvy.Models.HotelDB;
using PetaPoco;
using System.Collections.Generic;

namespace Lvy.Trip.Dao
{
    public class HotelDao : YuanDbRepository<HotelModel> { }

    public class HotelFileDao : YuanDbRepository<HotelFileModel> { }

    public class HotelRoomDao : YuanDbRepository<HotelRoomModel> {

        public List<HotelRoomModel> GetRooms(string hotelCode)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT t.*, o.* 
FROM hotel_rooms t LEFT JOIN hotel_room_beds o ON o.RoomID=t.RoomID 
WHERE t.HotelCode=@0 ", hotelCode);

            return _repo.Fetch<HotelRoomModel, HotelRoomBedModel, HotelRoomModel>(new RoomToBedRelator().MapIt, sql.SQL, sql.Arguments);
        }
    
    }
    public class HotelRoomBedDao : YuanDbRepository<HotelRoomBedModel> { }

    public class HotelStockDao : YuanDbRepository<HotelStockModel> { }

}