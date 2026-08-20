using Lvy.Models.WeixinDB;
using Lvy.Trip.Dao.Weixin;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Weixin
{
    public class LocationBiz : BaseBiz
    {
        private readonly MemberLocationDao _dao = new MemberLocationDao();

        public void AddLocation(int hostId, string openID, double lon, double lat, double prec)
        {
            _dao.Insert(new MemberLocation
            {
                HostID = hostId,
                OpenID = openID,
                Latitude = Convert.ToString(lat),
                Longitude = Convert.ToString(lon),
                Precision = Convert.ToString(prec),
                CreatedDate = DateTime.Now
            });
        }

        public MemberLocation GetLastLocation(int hostID, string OpenID)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMemberLocations where OpenID=@0 ORDER BY CreatedDate DESC ", Ansi(OpenID));

            var entity = _dao.FirstOrDefault(sql.SQL, sql.Arguments);
            return entity;
        }

        public List<MemberLocation> getLocations(string id)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMemberLocations where OpenID=@0 ORDER BY CreatedDate DESC ", Ansi(id));

            var list = _dao.Query(sql.SQL, sql.Arguments).ToList();
            return list;
        }
    }
}