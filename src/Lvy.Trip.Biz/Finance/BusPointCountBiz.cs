using Lvy.Trip.Dao.Order;
using Lvy.VModels.Finance;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Finance
{
    /// <summary>
    /// 上下车点统计
    /// </summary>
    public class BusPointCountBiz : BaseBiz
    {
        public List<BusPointCountRow> GetBusPointCountList(BusPointCountVModel vModel)
        {
            if (vModel.BeginOutDate.IsNullOrEmpty() || vModel.EndOutDate.IsNullOrEmpty())
                return null;    //需限定起止日期

            Sql sql = new Sql();
            sql.Append(@"SELECT o.TravellerCount AS PeopleCount, o.LineBusPoint AS BusPointJson
FROM tporder o INNER JOIN tpline l ON l.LineID = o.LineId 
WHERE l.TrafficType=1 AND o.LineBusPointId<>0 AND o.OutDate>=@0 AND o.OutDate<=@1 AND o.OwnerCode=@2", vModel.BeginOutDate.ToDateTime(), vModel.EndOutDate.ToDateTime(), vModel.OwnerCode);

            if (!vModel.BusPointName.IsNullOrEmpty())
                sql.Append(" AND o.LineBusPoint LIKE @0", AnsiLike(vModel.BusPointName));
            if (vModel.JieSongType > 0)
                sql.Append(" AND o.JsType=@0", vModel.JieSongType);

            return new TpOrderDao().Query<BusPointCountRow>(sql.SQL, sql.Arguments).ToList();
        }

    }
}