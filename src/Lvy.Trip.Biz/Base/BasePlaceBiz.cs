using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Base;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Base
{
    /// <summary>
    /// 景区
    /// </summary>
    public class BasePlaceBiz : BaseBiz
    {
        private readonly static BasePlaceDao _dao = new BasePlaceDao();

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<BasePlaceModel> GetPagedList(BasePlaceVModel vModel)
        {
            var sql = new Sql();
            sql.Append(@"SELECT bp.*, bd.Name DestinationName 
FROM BasePlace bp 
LEFT JOIN BaseDestination bd ON bd.ParentStr=bp.DestinationStr
WHERE bp.OwnerCode=@0 AND bp.IsValid=1", vModel.OwnerCode);

            if (!vModel.PlaceName.IsNullOrEmpty())
                sql.Append(@" AND bp.PlaceName Like @0", AnsiLike(vModel.PlaceName.Trim()));
            if (!vModel.PlaceLevel.IsNullOrEmpty())
                sql.Append(@" AND bp.PlaceLevel Like @0", AnsiLike(vModel.PlaceLevel.Trim()));
            sql.Append(@" ORDER BY bp.ModifiedTime DESC");

            return _dao.Pager(vModel.PagedList.PageIndex, vModel.PagedList.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Create(BasePlaceModel model, CrmAccountModel currentUser)
        {
            model.IsValid = 1;
            model.ModifiedBy = currentUser.Code;
            model.ModifiedTime = DateTime.Now;
            model.DestinationStr = model.DestinationStr;
            return Convert.ToInt32(_dao.Insert(model));
        }

        public BasePlaceModel GetPlaceById(int placeId)
        {
            return _dao.GetById(placeId);
        }

        public BasePlaceModel GetPlaceByCode(string placeCode)
        {
            return _dao.FirstOrDefault("SELECT * FROM BasePlace WHERE PlaceCode=@0 ", placeCode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(BasePlaceModel model, CrmAccountModel currentUser)
        {
            model.ModifiedBy = currentUser.Code;
            model.ModifiedTime = DateTime.Now;
            return _dao.Update(model);
        }

        public List<BasePlacePhotoModel> GetPhotos(string PlaceCode)
        {
            string sql = " SELECT * FROM BasePlacePhoto WHERE PlaceCode=@0 AND IsValid=1";
            return _dao.Query<BasePlacePhotoModel>(sql, PlaceCode).ToList();
        }

        public string AddPhoto(BasePlacePhotoModel model)
        {
            return new BasePlacePhotoDao().Insert(model).ToString();
        }

        public int DeletePhoto(int Id)
        {
            return _dao.Execute(" UPDATE BasePlacePhoto SET IsValid=0 WHERE ID=@0 ", Id);
        }

        public static List<BasePlaceModel> GetPlaces()
        {
           return _dao.Fetch(@"SELECT PlaceId,PlaceCode,PlaceName,PinYin,SimpleDesc,OpenTime,PlaceLevel,DestinationStr,IsValid,IsFree
FROM BasePlace WHERE IsValid=1 ");
              
        }
    }
}