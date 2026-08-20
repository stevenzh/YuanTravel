using Lvy.Models.SiteDB;
using Lvy.Trip.Dao.Site;
using Lvy.VModels.Site;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Site
{
    /// <summary>
    /// 微信模板管理
    /// </summary>
    public class SiteBannerBiz : BaseBiz
    {
        private readonly SiteBannerDao _dao = new SiteBannerDao();

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="qmodel"></param>
        /// <returns></returns>
        public List<SiteBannerModel> GetBannerList(BannerVModel qmodel)
        {
            var sql = new Sql();
            sql.Append(" SELECT * FROM site_banners WHERE OwnerCode=@0 ", qmodel.OwnerCode);

            if (!string.IsNullOrEmpty(qmodel.Name))
                sql.Append(" AND Name like @0", AnsiLike(qmodel.Name));
            if (!string.IsNullOrEmpty(qmodel.Type))
                sql.Append(" AND Type = @0", Ansi(qmodel.Type));

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public SiteBannerModel GetBannerByID(int id)
        {
            return _dao.GetById(id);
        }

        public void SaveBanner(SiteBannerModel model)
        {
            if (model.BannerID == default(int))
            {
                _dao.Insert(model);
            }
            else
            {
                _dao.Update(model);
            }
        }

        public void AddNav(SiteBannerModel model)
        {
            _dao.Insert(model);
        }

        public List<SiteBannerModel> GetBanner(string type)
        {
            var sql = new Sql();
            sql.Append(" SELECT * FROM site_banners WHERE `Type`=@0 ", type);

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }
    }
}