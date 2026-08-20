using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Arch.Common;
using Lvy.Models;
using Lvy.Trip.Biz.Base;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Lvy.Web.Common.FileUpload;
using System.Net;
using Common.Logging;
using Lvy.Models.BaseDB;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 商户功能
    /// </summary>
    public class FileResController : BaseController
    {
        ILog logger = LogManager.GetLogger("OrderController");

        private readonly BaseFileResBiz _biz = new BaseFileResBiz();
        /// <summary>
        /// 查询文件资源
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Search(FileResVModel vModel)
        {
            if (vModel == null)
                vModel = new FileResVModel();
            if (vModel.QueryModel == null)
                vModel.QueryModel = new BaseFileResModel();
            if (vModel.FileResModels == null)
                vModel.FileResModels = new PagedList<BaseFileResModel>();

            vModel.OwnerCode = GlobalContext.Current.OwnerCode;
            vModel.FileResModels = _biz.GetPager(vModel);
            ViewBag.FileResTypeBeans = DictionaryTools.GetEnumsBy(Enums.ResTypeEnum).ToSelectListFor();
            return View(vModel);
        }

        public ActionResult Create()
        {
            ViewBag.FileResTypeBeans = DictionaryTools.GetEnumsBy(Enums.ResTypeEnum).ToSelectListFor();
            return View();
        }

        public ActionResult Edit(int id)
        {
            var model = _biz.GetById(id);

            ViewBag.FileResTypeBeans = DictionaryTools.GetEnumsBy(Enums.ResTypeEnum).ToSelectListFor();
            return View(model);
        }


        public ActionResult Add(BaseFileResModel model)
        {
            int fileSize = 0;
            var path = UploadFile("UploadFile", model.ResType, ref fileSize);
            model.FileSize = fileSize;
            model.Path = path;
            model.OwnerCode = UserInfo.OwnerCode;
            model.IsValid = 1;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _biz.Add(model);

            return RedirectToAction("Search");
        }


        public ActionResult Update(BaseFileResModel model)
        {

            _biz.Update(model);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetValidState(int id)
        {
            var model = _biz.GetById(id);
            model.IsValid = model.IsValid == 1 ? 0 : 1;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _biz.Update(model);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 需要修改  //TODO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DownLoadFile(int id)
        {
            var model = _biz.GetById(id);
            if (model == null)
                return null;
            try
            {
                WebRequest.Create(AppSetting.Get("UploadFileRoot") + model.Path);
            }
            catch (Exception ex)
            {
                logger.Error("File not Found.", ex);
                return null;
            }

            byte[] fileData;
            try
            {
                using (WebClient client = new WebClient())
                {
                    fileData = client.DownloadData(AppSetting.Get("UploadFileRoot") + model.Path);

                    return File(fileData, "application/octet-stream", Server.UrlEncode(model.FileName));
                }
            }
            catch (Exception ex)
            {
                logger.Error("File download failure..", ex);
                return null;
            }
        }

        #region 私有方法

        /// <summary>
        /// 上传文件
        /// </summary>
        private string UploadFile(string fileName, int resType, ref int fileSize)
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
            request.VirtualPath = @"upload\{0}".With(resType == 1 ? "ticket" : "line");

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }
        #endregion


        #region kindeditor图片上传

        /// <summary>
        /// 本地图片上传
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadPhoto()
        {
            string fileTypes = "gif,jpg,jpeg,png,bmp";
            int maxSize = 1000000;   // 1M
            var result = new
            {
                error = 1,
                message = "请选择文件.",
                url = ""
            };

            HttpPostedFileBase file = Request.Files["imgFile"];
            if (file == null)
            {
                return Json(result);
            }

            string fileName = file.FileName;
            string fileExt = Path.GetExtension(fileName).ToLower();

            ArrayList fileTypeList = ArrayList.Adapter(fileTypes.Split(','));

            if (file.InputStream == null || file.InputStream.Length > maxSize)
            {
                result = new
                {
                    error = 1,
                    message = "上传文件大小超过限制.",
                    url = ""
                };

                return Json(result);
            }

            if (string.IsNullOrEmpty(fileExt) || Array.IndexOf(fileTypes.Split(','), fileExt.Substring(1).ToLower()) == -1)
            {
                result = new
                {
                    error = 1,
                    message = "上传文件扩展名是不允许的扩展名.",
                    url = ""
                };
                return Json(result);
            }

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = @"upload\{0}".With(GlobalContext.Current.UserInfo.CustomerCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            result = new
            {
                error = 0,
                message = "上传文件扩展名是不允许的扩展名.",
                url = AppSetting.Get("UploadFileRoot") + response.FilePath.Replace("\\", "/") + response.FileName
            };

            return Json(result); ;
        }

        #endregion
    }
}
