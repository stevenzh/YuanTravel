using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using PetaPoco;
using System;

namespace Lvy.Trip.Biz.Base
{
    public class PhotoBiz : BaseBiz
    {
        private PhotoAlbumDao _albumDao = new PhotoAlbumDao();
        private PhotoInfoDao _infoDao = new PhotoInfoDao();

        /// <summary>
        /// 获取图片列表
        /// </summary>
        /// <param name="qModel"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<PhotoInfoModel> GetPhotoInfoListForPage(PhotoInfoQModel qModel, int pageIndex, int pageSize)
        {
            var sql = new Sql();
            sql.Append(@"SELECT t.*, b.ParentStr as AreaStr, c.AlbumName, b.Id as AreaId
FROM Photo_Info t
left join Photo_Album c on t.AlbumId = c.PhotoAlbumId
left join BaseDestination b on c.AreaId = b.Id
WHERE 1=1 ");
            if (qModel != null && qModel.Model != null)
            {
                if (!qModel.Model.Caption.IsNullOrEmpty())
                {
                    sql.Append(" and t.Caption like @0", AnsiLike(qModel.Model.Caption));
                }
                if (qModel.Model.AreaId > 0)
                {
                    sql.Append(" and b.ParentStr like @0", AnsiLike(string.Format("/{0}/", qModel.Model.AreaId)));
                }
                if (qModel.Model.Status >= 0)
                {
                    sql.Append(" and t.Status=@0 ", qModel.Model.Status);
                }
            }

            return _infoDao.Pager(pageIndex, pageSize, sql.ToString(), sql.Arguments);
        }

        /// <summary>
        /// 获取图片的大小
        /// </summary>
        /// <param name="model"></param>
        private void SearchImageWidthAndHeight(PhotoInfoModel model)
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

        /// <summary>
        /// 查询图册
        /// </summary>
        /// <param name="qModel"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<PhotoAlbumModel> GetPhotoAlbumListForPage(PhotoAlbumQModel qModel, int pageIndex, int pageSize)
        {
            var sql = new Sql();
            sql.Append(@"SELECT c.*, b.Name as AreaName
FROM Photo_Album c left join BaseDestination b on c.AreaId = b.Id
WHERE 1=1 ");
            if (qModel != null && qModel.Model != null)
            {
                if (!qModel.Model.AlbumName.IsNullOrEmpty())
                {
                    sql.Append(" and c.AlbumName like @0", AnsiLike(qModel.Model.AlbumName));
                }
                if (qModel.Model.AreaId > 0)
                {
                    sql.Append(" and c.AreaId = @0", qModel.Model.AreaId);
                }
                if (qModel.Model.Status >= 0)
                {
                    sql.Append(" and c.Status=@0 ", qModel.Model.Status);
                }
                if (qModel.Model.updateTime != null)
                {
                    DateTime end = qModel.Model.updateTime.Value.AddDays(1);
                    sql.Append(" and c.UpdateTs > @0 and c.UpdateTs<@1", qModel.Model.updateTime.ToDateFormat(), end.ToDateFormat());
                }
            }
            return _albumDao.Pager(pageIndex, pageSize, sql.ToString(), sql.Arguments);
        }

        /// <summary>
        /// 添加图片信息
        /// </summary>
        /// <param name="model"></param>
        public void AddPhotoInfo(PhotoInfoModel model)
        {
            model.CreateTs = DateTime.Now;
            model.UpdateTs = DateTime.Now;
            _infoDao.Insert(model);
        }

        /// <summary>
        /// 编辑图片信息
        /// </summary>
        /// <param name="model"></param>
        public void EditPhotoInfo(PhotoInfoModel model)
        {
            _infoDao.Update(" SET Url=@1, Caption=@2, Seq=@3, Status=@4, UpdateTs=now() WHERE PhotoId=@0 ", model.PhotoId, model.Url, model.Caption, model.Seq, model.Status);
        }

        /// <summary>
        /// 删除图片信息
        /// </summary>
        /// <param name="model"></param>
        public void DeletePhotoInfo(PhotoInfoModel model)
        {
            _infoDao.Delete(model);
        }

        /// <summary>
        /// 设置图片有效性
        /// </summary>
        /// <param name="model"></param>
        public void SetPhotoInfoValid(PhotoInfoModel model)
        {
            var entity = _infoDao.GetById(model.PhotoId);
            if (entity != null)
            {
                entity.Status = model.Status;
                entity.UpdateTs = DateTime.Now;
                entity.Operator = model.Operator;
                _infoDao.Update(entity);
            }
        }

        /// <summary>
        /// 设置图片顺序
        /// </summary>
        /// <param name="model"></param>
        public void SetPhotoInfoSeq(PhotoInfoModel model)
        {
            var entity = _infoDao.GetById(model.PhotoId);
            if (entity != null)
            {
                entity.Seq = model.Seq;
                entity.UpdateTs = DateTime.Now;
                entity.Operator = model.Operator;
                _infoDao.Update(entity);
            }
        }

        /// <summary>
        /// 添加相册信息
        /// </summary>
        /// <param name="model"></param>
        public long AddPhotoAlbum(PhotoAlbumModel model)
        {
            model.CreateTs = DateTime.Now;
            model.UpdateTs = DateTime.Now;
            return _albumDao.Insert(model).ToInt();
        }

        /// <summary>
        /// 编辑相册信息
        /// </summary>
        /// <param name="model"></param>
        public void EditAlbum(PhotoAlbumModel model)
        {
            var album = _albumDao.GetById(model.PhotoAlbumId);
            album.UpdateTs = DateTime.Now;
            album.AreaId = model.AreaId;
            album.AlbumName = model.AlbumName;
            album.CoverPhotoId = model.CoverPhotoId;
            album.Status = model.Status;
            album.Seq = model.Seq;
            album.Size = model.Size;
            album.Description = model.Description;
            album.Operator = model.Operator;

            _albumDao.Update(album);
        }

        /// <summary>
        /// 删除相册信息
        /// </summary>
        /// <param name="model"></param>
        public void DeleteAlbum(PhotoAlbumModel model)
        {
            _albumDao.Update(" set Status=0 where PhotoAlbumId=@0", model.PhotoAlbumId);
        }

        /// <summary>
        /// 设置相册有效性
        /// </summary>
        /// <param name="model"></param>
        public void SetAlbumValid(PhotoAlbumModel model)
        {
            var entity = _albumDao.GetById(model.PhotoAlbumId);
            if (entity != null)
            {
                entity.Status = model.Status;
                entity.UpdateTs = DateTime.Now;
                entity.Operator = model.Operator;
                _albumDao.Update(entity);
            }
        }

        /// <summary>
        /// 设置相册顺序
        /// </summary>
        /// <param name="model"></param>
        public void SetAlbumSeq(PhotoAlbumModel model)
        {
            var entity = _albumDao.GetById(model.PhotoAlbumId);
            if (entity != null)
            {
                entity.Seq = model.Seq;
                entity.UpdateTs = DateTime.Now;
                entity.Operator = model.Operator;
                _albumDao.Update(entity);
            }
        }

        /// <summary>
        /// 设置相册封面
        /// </summary>
        /// <param name="model"></param>
        public void SetAlbumCover(PhotoAlbumModel model)
        {
            var entity = _albumDao.GetById(model.PhotoAlbumId);
            if (entity != null)
            {
                entity.CoverPhotoId = model.CoverPhotoId;
                entity.UpdateTs = DateTime.Now;
                entity.Operator = model.Operator;
                _albumDao.Update(entity);
            }
        }

        /// <summary>
        /// 设置图册图片数量
        /// </summary>
        /// <param name="model"></param>
        public void SetAlbumSize(PhotoAlbumModel model)
        {
            var entity = _albumDao.GetById(model.PhotoAlbumId);
            if (entity != null)
            {
                entity.Size = model.Size;
                entity.UpdateTs = DateTime.Now;
                entity.Operator = model.Operator;
                _albumDao.Update(entity);
            }
        }

        /// <summary>
        /// 根据ID获取相册信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public PhotoAlbumModel GetAlbumDetailById(PhotoAlbumModel model)
        {
            var entity = _albumDao.GetById(model.PhotoAlbumId);
            if (entity.CoverPhotoId > 0)
            {
                entity.ConverPhoto = _infoDao.GetById(entity.CoverPhotoId);
            }
            return entity;
        }

        /// <summary>
        /// 根据ID获取图片信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public PhotoInfoModel GetPhotoDetailById(PhotoInfoModel model)
        {
            return _infoDao.GetById(model.PhotoId);
        }

        /// <summary>
        /// 获取相册图片数
        /// </summary>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public int PhotoSizeByAlbumId(long albumId)
        {
            return _infoDao.ExecuteScalar<Int32>("select count(*) from Photo_Info where AlbumId=@0", albumId);
        }

        /// <summary>
        /// 查询城市图片
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public PagedList<PhotoInfoModel> SearchCityImages(string parentStr, int index)
        {
            var result = _infoDao.Pager(index, 10, @"select pi.* from Photo_Info pi
left join Photo_Album pa on pi.AlbumId = pa.PhotoAlbumId
left join BaseDestination bd on pa.AreaId = bd.Id
where bd.ParentStr = @0", parentStr);

            return result;
        }
    }
}