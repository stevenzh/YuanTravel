using Arch.Common.Utils;
using log4net;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Biz;
using Lvy.Trip.Dao.Base;
using Lvy.Visa.Dao;
using Lvy.Visa.Models;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.Biz
{
    /// <summary>
    /// 领取
    /// </summary>
    public class DistrictBiz : BaseBiz
    {
        private DistrictDao _dao = new DistrictDao();
        private CountryInfoDao countryDao = new CountryInfoDao();
        private PhotoInfoDao _infoDao = new PhotoInfoDao();

        private ILog _logger = LogManager.GetLogger(typeof(DistrictBiz));

        #region 领区

        /// <summary>
        /// 根据国家编码查询领区分页列表
        /// </summary>
        /// <param name="contryCodeStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<VisaCountryConsularDistrictModel> SearchConsularDistrictPagedList(string contryCodeStr, int pageIndex, int pageSize, string xmlPath)
        {
            Sql sql = new Sql();
            sql.Append(@"select vc.*, bd.Name CountryName, bdd.Value VisaAreaValue from Visa_Country_ConsularDistrict vc
inner join BaseDestination bd on vc.CountryCode= bd.ParentStr
inner join BaseDictionaryDetail bdd on vc.ConsularDistrictKey=bdd.`Key` and bdd.Name='VisaAreaEnum' ");

            return _dao.Pager<VisaCountryConsularDistrictModel>(pageIndex, pageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据国家编码查询所有领区列表
        /// </summary>
        /// <param name="contryCodeStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IList<VisaCountryConsularDistrictModel> SearchConsularDistrictList(string visacountryCode)
        {
            Sql sql = new Sql();
            sql.Append(@"select vc.*, bd.Name CountryName, bdd.Value VisaAreaValue from Visa_Country_ConsularDistrict vc
inner join BaseDestination bd on vc.CountryCode= bd.ParentStr
inner join BaseDictionaryDetail bdd on vc.ConsularDistrictKey=bdd.`Key` and bdd.Name='VisaAreaEnum'
and vc.VisaCountryCode=@0", visacountryCode);

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 新增领区 保存
        /// </summary>
        /// <param name="model"></param>
        public void SaveConsularDis(VisaCountryConsularDistrictModel model)
        {
            _dao.Insert(model);
        }

        /// <summary>
        /// 修改领区
        /// </summary>
        /// <param name="model"></param>
        public void ModifyConsularDis(VisaCountryConsularDistrictModel model)
        {
            _dao.Execute(@"update Visa_Country_ConsularDistrict set ConsularDistrictKey=@1, ModifyDate=now(), ModifyBy=@2, AcceptRange=@3
where ConsularDistrictCode=@0", model.ConsularDistrictCode, model.ConsularDistrictKey, model.ModifyBy, model.AcceptRange);
        }

        /// <summary>
        /// 获取单个领区信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public VisaCountryConsularDistrictModel GetConsularDis(string code)
        {
            return _dao.FirstOrDefault(@"select vc.*, bd.Name CountryName from Visa_Country_ConsularDistrict vc
inner join BaseDestination bd on vc.CountryCode = bd.ParentStr
where vc.ConsularDistrictCode=@0 ", code);
        }

        /// <summary>
        /// 根据code删除领区
        /// </summary>
        /// <param name="ConsularDistrictCode"></param>
        public void DeleteConsularDis(string ConsularDistrictCode, bool isvalid)
        {
            _dao.Execute("UPDATE Visa_Country_ConsularDistrict set IsValid=@1 where ConsularDistrictCode=@0 ", ConsularDistrictCode, isvalid);
        }

        /// <summary>
        /// 检测领区是否已经存在
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsExistConsularDis(VisaCountryConsularDistrictModel model)
        {
            VisaCountryConsularDistrictModel entity = null;
            if (model.ConsularDistrictCode.IsNullOrEmpty())
                entity = _dao.FirstOrDefault("select * from Visa_Country_ConsularDistrict where ConsularDistrictKey=@0 and VisaCountryCode=@1", model.ConsularDistrictKey, model.CountryCode);
            else
                entity = _dao.FirstOrDefault("select * from Visa_Country_ConsularDistrict where ConsularDistrictKey=@0 and VisaCountryCode=@1 and ConsularDistrictCode<>@2", model.ConsularDistrictKey, model.CountryCode, model.ConsularDistrictCode);

            return (entity == null ? false : true);
        }

        #endregion 领区

        #region 国家

        /// <summary>
        /// 根据国家编码查询国家分页列表
        /// </summary>
        /// <returns></returns>
        public PagedList<VisaCountryInfoModel> SearchCountryPagedList(CountryConsularDistrictQModel model)
        {
            Sql sql = new Sql();
            sql.Append(@"select vc.*, bd.Name CountryName
from Visa_CountryInfo vc
inner join BaseDestination bd on vc.CountryCode= bd.ParentStr
WHERE vc.OwnerCode=@0", model.OwnerCode);

            if (!model.CountryCode .IsNullOrEmpty())
                sql.Append(" AND vc.CountryCode=@0 ", model.CountryCode);

            return countryDao.Pager<VisaCountryInfoModel>(model.countryPagedList.PageIndex, model.countryPagedList.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 保存签证国家信息
        /// </summary>
        /// <param name="model"></param>
        public string SaveCountryInfo(VisaCountryInfoModel model)
        {
            model.VisaCountryCode = "V" + DBTools.GetSeqNo("00");
            countryDao.Insert(model);

            return model.VisaCountryCode;
        }

        /// <summary>
        /// 修改国家信息
        /// </summary>
        /// <param name="model"></param>
        public void ModifyCountryInfo(VisaCountryInfoModel model)
        {
            //修改国家信息
            countryDao.Update("set CountryImgPath=@1, ConsularDistrictNotes=@2, ModifyBy=@3 where Id=@0", model.Id, model.CountryImgPath, model.ConsularDistrictNotes, model.ModifyBy);
            //修改国家下面所属领区的
            _dao.Execute("update Visa_Country_ConsularDistrict set CountryCode=@1 where VisaCountryCode=@0", model.VisaCountryCode, model.CountryCode);
        }

        /// <summary>
        /// 根据VisaCountryCode获取签证国家信息
        /// </summary>
        /// <param name="visaCounryCode"></param>
        /// <returns></returns>
        public VisaCountryInfoModel GetVisaCountryInfo(string visaCounryCode, string ownerCode)
        {
            return countryDao.FirstOrDefault(@"select vc.*, bd.Name CountryName 
from Visa_CountryInfo vc
inner join BaseDestination bd on vc.CountryCode = bd.ParentStr and VisaCountryCode=@0
where vc.OwnerCode=@1", visaCounryCode, ownerCode);
        }

        /// <summary>
        /// 检测国家是否已经存在
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsExistCountryInfo(VisaCountryInfoModel model)
        {
            VisaCountryInfoModel entity = null;
            if (model.VisaCountryCode.IsNullOrEmpty())
                entity = countryDao.FirstOrDefault("select * from Visa_CountryInfo where OwnerCode=@0 and CountryCode=@1",model.OwnerCode,  model.CountryCode);
            else
                entity = countryDao.FirstOrDefault("select * from Visa_CountryInfo where OwnerCode=@0 and CountryCode=@1 and VisaCountryCode<>@2", model.OwnerCode, model.CountryCode, model.VisaCountryCode);

            return (entity == null ? false : true);
        }

        #endregion 国家

        #region 图片

        /// <summary>
        /// 查询城市图片
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public PagedList<PhotoInfoModel> SearchCityImages(string VisaCountryParentStr, int index)
        {
            var result = _infoDao.Pager(index, 10, @"select pi.* from Photo_Info pi
left join Photo_Album pa on pi.AlbumId = pa.PhotoAlbumId
left join BaseDestination bd on pa.AreaId = bd.Id
where bd.ParentStr = @0", VisaCountryParentStr);

            return result;
        }

        /// <summary>
        /// 查询城市图片
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public PagedList<PhotoInfoModel> SearchCityImages(CountryConsularDistrictQModel model)
        {
            var photosList = SearchCityImages(model.CountryCode, model.ImagePagedIndex);
            foreach (var photomodel in photosList.Items)
            {
                SearchImageWidthAndHeight(photomodel);
            }
            return photosList;
        }

        /// <summary>
        /// 获取图片的大小
        /// </summary>
        /// <param name="model"></param>
        private void SearchImageWidthAndHeight(PhotoInfoModel model)
        {
            try
            {
                if (!model.Url.IsNullOrEmpty())
                {
                    System.Net.HttpWebRequest hwreq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(model.Url);
                    System.Net.HttpWebResponse hwrep1 = (System.Net.HttpWebResponse)hwreq.GetResponse();
                    System.Drawing.Image originalImage = System.Drawing.Image.FromStream(hwrep1.GetResponseStream());
                    model.PhotoWidth = originalImage.Width;
                    model.PhotoHeight = originalImage.Height;
                }
            }
            catch (Exception err)
            {
                _logger.Error("", err);
            }
        }

        #endregion 图片
    }
}