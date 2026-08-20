using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Base;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    ///  图片上传
    /// </summary>
    public class PhotoController : BaseController
    {
        private readonly PhotoBiz _service;
        private ILog logger = LogManager.GetLogger("PhotoController");

        /// <summary>
        /// 相册管理
        /// </summary>
        /// <returns></returns>
        public ActionResult PhotoAlbum()
        {
            return View();
        }

        /// <summary>
        /// 分页获取相册信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ActionResult GetPhotoAlbumForPage(PhotoAlbumQModel info)
        {
            var result = _service.GetPhotoAlbumListForPage(info, info.PageIndex, info.PageSize);
            info.List = result.Items;
            info.Total = result.TotalCount;
            return Json(info);
        }

        public PhotoController()
        {
            _service = new PhotoBiz();
        }

        #region Ajax

        /// <summary>
        /// 分页获取图片信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>

        public ActionResult GetPhotoInfoForPage(PhotoInfoQModel info)
        {
            var result = _service.GetPhotoInfoListForPage(info, info.PageIndex, info.PageSize);
            info.List = result.Items;
            info.Total = result.TotalCount;
            return Json(info);
        }

        public ActionResult GetPictSize(string url)
        {
            if (!url.IsNullOrEmpty())
            {
                try
                {
                    string ll = url;
                    if (!url.ToLower().StartsWith("http://"))
                    {
                        ll = AppSetting.Get("UploadFileRoot") + url;
                    }

                    HttpWebRequest hwreq = (HttpWebRequest)HttpWebRequest.Create(ll);
                    HttpWebResponse hwrep1 = (HttpWebResponse)hwreq.GetResponse();
                    Image originalImage = Image.FromStream(hwrep1.GetResponseStream());
                    return Json(originalImage.Width.ToString() + "*" + originalImage.Height.ToString());
                }
                catch (Exception) { }
            }
            return Json("");
        }

        /// <summary>
        /// 添加图片信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddPhotoInfo(PhotoInfoModel model)
        {
            string msg = string.Empty;
            model.Operator = GlobalContext.Current.UserInfo.Name;
            string picroot = AppSetting.Get("UploadFileRoot");
            if (model.Url.StartsWith(picroot))
            {
                model.Url = model.Url.Substring(picroot.Length);
            }
            _service.AddPhotoInfo(model);
            if (model.AlbumId > 0)
            {
                PhotoAlbumModel albumModel = new PhotoAlbumModel();
                albumModel.Size = _service.PhotoSizeByAlbumId(model.AlbumId);
                albumModel.PhotoAlbumId = model.AlbumId;
                albumModel.Operator = UserInfo.Code;
                _service.SetAlbumSize(albumModel);
            }
            msg = "0000";
            return Content(msg);
        }

        /// <summary>
        /// 编辑图片信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditPhotoInfo(PhotoInfoModel model)
        {
            string msg = string.Empty;
            model.Operator = GlobalContext.Current.UserInfo.Name;
            string picroot = AppSetting.Get("UploadFileRoot");
            if (model.Url.StartsWith(picroot))
            {
                model.Url = model.Url.Substring(picroot.Length);
            }
            _service.EditPhotoInfo(model);
            msg = "0000";
            return Content(msg);
        }

        /// <summary>
        /// 删除图片信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult DeletePhotoInfo(PhotoInfoModel model)
        {
            string msg = string.Empty;
            long albumId = _service.GetPhotoDetailById(model).AlbumId;
            _service.DeletePhotoInfo(model);
            if (albumId > 0)
            {
                PhotoAlbumModel albumModel = new PhotoAlbumModel();
                albumModel.Size = _service.PhotoSizeByAlbumId(albumId);
                albumModel.PhotoAlbumId = albumId;
                albumModel.Operator = GlobalContext.Current.UserInfo.Code;
                _service.SetAlbumSize(albumModel);
            }
            msg = "0000";
            return Content(msg);
        }

        /// <summary>
        /// 设置图片有效性
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SetPhotoInfoValid(PhotoInfoModel model)
        {
            model.Operator = GlobalContext.Current.UserInfo.Name;
            string msg = "0000";
            _service.SetPhotoInfoValid(model);
            return Content(msg);
        }

        /// <summary>
        /// 设置图片顺序
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SetPhotoInfoSeq(PhotoInfoModel model)
        {
            model.Operator = GlobalContext.Current.UserInfo.Name;
            string msg = "0000";
            _service.SetPhotoInfoSeq(model);
            return Content(msg);
        }

        /// <summary>
        /// 设置相册封面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SetAlbumConver(PhotoAlbumModel model)
        {
            model.Operator = GlobalContext.Current.UserInfo.Name;
            string msg = "0000";
            _service.SetAlbumCover(model);
            return Content(msg);
        }

        /// <summary>
        /// 添加相册 信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddAlbum(PhotoAlbumModel model)
        {
            string msg = string.Empty;
            model.Operator = GlobalContext.Current.UserInfo.Code;
            msg = _service.AddPhotoAlbum(model).ToString();
            return Content(msg);
        }

        /// <summary>
        /// 编辑相册 信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditAlbum(PhotoAlbumModel model)
        {
            string msg = string.Empty;
            model.Operator = GlobalContext.Current.UserInfo.Code;
            _service.EditAlbum(model);
            msg = "0000";
            return Content(msg);
        }

        /// <summary>
        /// 删除相册 信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult DeleteAlbum(PhotoAlbumModel model)
        {
            string msg = string.Empty;
            _service.DeleteAlbum(model);
            msg = "0000";
            return Content(msg);
        }

        /// <summary>
        /// 设置相册是否有效
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SetAlbumValid(PhotoAlbumModel model)
        {
            model.Operator = GlobalContext.Current.UserInfo.Name;
            string msg = "0000";
            _service.SetAlbumValid(model);
            return Content(msg);
        }

        /// <summary>
        /// 设置相册 顺序
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SetAlbumSeq(PhotoAlbumModel model)
        {
            model.Operator = GlobalContext.Current.UserInfo.Name;
            string msg = "0000";
            _service.SetAlbumSeq(model);
            return Content(msg);
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public ActionResult UploadPhoto(string photoId, string albumId)
        {
            string fileUrl = string.Empty;
            HttpPostedFileBase file = Request.Files[0];

            if (file.InputStream.Length > 8388608)   // 1M
            {
                return Content("00000");
            }
            if (file != null && !string.IsNullOrEmpty(file.FileName))
            {
                fileUrl = UploadPhoto(file.FileName, photoId, albumId, file.InputStream);
            }
            return Content(fileUrl);
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="albumId"></param>
        /// <param name="photoUrl"></param>
        /// <returns></returns>
        public ActionResult DownLoadPhoto(string photoId, string albumId, string photoUrl)
        {
            string fileUrl = string.Empty;
            if (!string.IsNullOrEmpty(photoUrl))
            {
                WebClient client = new WebClient();

                var bytes = client.DownloadData(photoUrl);
                if (bytes.Length > 0)
                {
                    Stream stream = new MemoryStream(bytes);
                    fileUrl = UploadPhoto(photoUrl, photoId, albumId, stream);
                }
            }
            return Content(fileUrl);
        }

        #endregion Ajax

        #region View

        public ActionResult PhotoInfo(PhotoAlbumModel model)
        {
            if (model.PhotoAlbumId > 0)
            {
                model = _service.GetAlbumDetailById(model);
            }
            return View(model);
        }

        /// <summary>
        /// 图片编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult PhotoEdit(PhotoInfoModel model)
        {
            if (model.PhotoId > 0)
            {
                model = _service.GetPhotoDetailById(model);
            }
            return View(model);
        }

        #endregion View

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="albumId"></param>
        /// <param name="stream"></param>
        private string UploadPhoto(string resFileName, string photoId, string albumId, Stream stream)
        {
            byte[] picBuffer = new byte[stream.Length];
            stream.Read(picBuffer, 0, picBuffer.Length);
            stream.Seek(0, SeekOrigin.Begin);
            Stream thumbStream = new MemoryStream(picBuffer);

            PhotoAlbumModel albumModel = new PhotoAlbumModel();
            albumModel.PhotoAlbumId = Convert.ToInt64(albumId);
            albumModel = _service.GetAlbumDetailById(albumModel);

            string virtualPathRoot = string.Format(@"pic\{0}\{1}", albumModel.AreaId, albumId);
            string fileExtension = Path.GetExtension(resFileName);
            string fileName = "";
            string thumbName = "";
            if (!string.IsNullOrEmpty(photoId) && photoId != "0")
            {
                fileName = photoId;
                thumbName = photoId + "_1";
            }
            else
            {
                fileName = DateTime.Now.Ticks.ToString();
                thumbName = fileName + "_1";
            }
            // fileName = string.Format("{0}{1}", fileName, fileExtension);
            fileName = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4) + fileExtension);
            //thumbName = string.Format("{0}{1}", thumbName, fileExtension);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = fileName;
            request.FileStream = Toolkit.Image.StreamToBytes(thumbStream);
            // 所属客户code\文件类型
            request.VirtualPath = virtualPathRoot;
            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);
            string fileUrl = AppSetting.Get("UploadFileRoot") + response.FilePath + response.FileName;

            return fileUrl;
        }

        private Stream GetThumbPic(Stream stream)
        {
            return ImageTools.GreateMiniImage(stream, 208, 138, "Cut");
        }

        public ActionResult GetPhotoSize(string url)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(url);

                return Json(fileInfo.Length);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(1);
            }
        }
    }
}