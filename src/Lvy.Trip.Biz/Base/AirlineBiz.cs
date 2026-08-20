using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Base
{
    public class AirlineBiz : BaseBiz
    {
        private readonly AirlineDao dao = new AirlineDao();

        #region 航空公司信息操作相关方法

        public PagedList<BaseAirlineModel> GetPagedAirline(AirlineVModel vModel)
        {
            var sql = new Sql();
            sql.Append(" select * from BaseAirlines where 1=1 ");
            if (!vModel.AirlineInfo.Code.IsNullOrEmpty())
            {
                sql.Append(" and Code like @0", AnsiLike(vModel.AirlineInfo.Code));
            }
            if (!vModel.AirlineInfo.ShortName.IsNullOrEmpty())
            {
                sql.Append(" and ShortName like @0", AnsiLike(vModel.AirlineInfo.ShortName));
            }

            return dao.Pager(vModel.AirelinePageList.PageIndex, vModel.AirelinePageList.PageSize, sql.SQL, sql.Arguments);
        }

        public object Add(BaseAirlineModel model)
        {
            return dao.Insert(model);
        }

        public int Update(BaseAirlineModel model)
        {
            return dao.Update(model);
        }

        public int Delete(int id)
        {
            return dao.Update("Set IsValid=0 where Id=@0 ",id);
        }

        public BaseAirlineModel GetAirlineById(int id)
        {
            return dao.GetById(id);
        }

        public BaseAirlineModel GetAirlineByCode(string code, string Id)
        {
            var sql = new Sql();
            sql.Append(" select * from BaseAirlines where  Code= @0", code);
            if (!Id.IsNullOrEmpty())
            {
                sql.Append(" and Id<>@0", Id);
            }

            return dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        #endregion 航空公司信息操作相关方法

        /// <summary>
        /// 获取航空公司列表信息
        /// </summary>
        /// <returns></returns>
        public List<BaseAirlineModel> GetAirlineList()
        {
            var sql = new Sql();
            sql.Append(" select * from BaseAirlines where 1=1 ");

            return dao.Query(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 获取开班计划航班信息
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public List<TpTourFlightModel> GetTpTourFlightList(int tourId)
        {
            var sql = new Sql();
            sql.Append(" select * from TpTourFlight where TourId=@0 ", tourId);

            return dao.Query<TpTourFlightModel>(sql.SQL, sql.Arguments).ToList();
        }
    }
}