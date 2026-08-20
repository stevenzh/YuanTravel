using Arch.Common;
using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Base;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 景区管理
    /// </summary>
    public class PlaceController : BaseController
    {
        private BasePlaceBiz biz = new BasePlaceBiz();

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected override void InitPage()
        {
            ViewBag.PlaceLevels = DictionaryTools.GetEnumsBy(Enums.PlaceLevelEnum).ToSelectListFor();
        }

        public ActionResult Search(BasePlaceVModel vModel)
        {
            if (vModel.PagedList == null)
                vModel.PagedList = new PagedList<BasePlaceModel>();
            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.PagedList = biz.GetPagedList(vModel);

            InitPage();
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        public ActionResult Create()
        {
            InitPage();
            return View(new BasePlaceModel());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(BasePlaceModel model)
        {
            model.PlaceCode = DBTools.GetSeqNo("BasePlace");
            model.PinYin = model.PlaceName.ConvertPinYin();
            model.JPinYin = model.PinYin.IsNullOrEmpty() ? "" : model.PinYin.ConvertJPinYin();
            int result = biz.Create(model, GlobalContext.Current.UserInfo);
            return RedirectToAction("Search");
        }

        public ActionResult Edit(int placeId = 0)
        {
            InitPage();
            var model = biz.GetPlaceById(placeId);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(BasePlaceModel model)
        {
            var entity = biz.GetPlaceById(model.PlaceId);
            if (null != entity)
            {
                if (entity.PlaceName != model.PlaceName)
                {
                    entity.PinYin = model.PlaceName.ConvertPinYin();
                    entity.JPinYin = model.PinYin.IsNullOrEmpty() ? "" : model.PinYin.ConvertJPinYin();
                    entity.PlaceName = model.PlaceName;
                }
                entity.DestinationStr = model.DestinationStr;
                entity.OpenTime = model.OpenTime;
                entity.PlaceLevel = model.PlaceLevel;
                entity.SimpleDesc = model.SimpleDesc;
                entity.PlaceDesc = model.PlaceDesc;
                int result = biz.Update(entity, GlobalContext.Current.UserInfo);
            }
            return RedirectToAction("Search");
        }

        public ActionResult Delete(int placeId = 0)
        {
            var model = biz.GetPlaceById(placeId);
            if (null != model)
            {
                model.IsValid = 0;
                int result = biz.Update(model, GlobalContext.Current.UserInfo);
            }
            return RedirectToAction("Search");
        }

        #region 景区图片管理

        public ActionResult PhotoView(string placeCode)
        {
            var models = new List<BasePlacePhotoModel>();
            if (!placeCode.IsNullOrEmpty())
                models = biz.GetPhotos(placeCode);
            return PartialView("UCPhotoView", models);
        }

        /// <summary>
        /// 景区图片插入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadPhoto(BasePlacePhotoModel model)
        {
            int fileSize = 0;
            var path = ToUploadPhoto("UploadFile", ref fileSize, model.PlaceCode);
            if (string.IsNullOrEmpty(path))
                return Content("0");

            model.FileSize = fileSize;
            model.Path = path;
            model.IsValid = 1;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            var flag = biz.AddPhoto(model);
            return Content(flag);
        }

        /// <summary>
        /// 文档插图保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UploadDocPhoto(BasePlacePhotoModel model)
        {
            int fileSize = 0;
            var path = ToUploadPhoto("UploadFile", ref fileSize, model.PlaceCode);
            if (string.IsNullOrEmpty(path))
                return Content("0");

            model.FileSize = fileSize;
            model.Path = path;
            model.IsValid = 1;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            model.FileName = "文档插图";
            biz.AddPhoto(model);
            return Content(AppSetting.Get("UploadFileRoot") + path);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        private string ToUploadPhoto(string fileName, ref int fileSize, string placeCode)
        {
            HttpPostedFileBase file = Request.Files[fileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;
            // 字节换算成K
            fileSize = file.ContentLength / 1024;
            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = @"{0}\{1}".With("place", placeCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        public ActionResult DeletePhoto(int Id)
        {
            var row = biz.DeletePhoto(Id);
            return Content(row.ToString());
        }

        #endregion 景区图片管理
    }
}