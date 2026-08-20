using Arch.Common;
using Gma.QrCodeNet.Encoding.DataEncodation;
using Lvy.Models.SiteDB;
using Lvy.Trip.Biz.Site;
using Lvy.VModels.Site;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WebGrease.Extensions;

namespace Lvy.Trip.AdminSite.Controllers.Site
{
    /// <summary>
    /// 轮播图管理
    /// </summary>
    public class BannerController : BaseController
    {
        private readonly SiteBannerBiz _biz = new SiteBannerBiz();

        // GET: Banner
        public ActionResult Search(BannerVModel vModel)
        {
            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.BannerList = _biz.GetBannerList(vModel);
            ViewData["BannerType"] = DictionaryTools.GetEnumsBy(Enums.SiteBannerEnum).ToSelectListFor();

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        public ActionResult Edit(int id)
        {
            ViewData["BannerType"] = DictionaryTools.GetEnumsBy(Enums.SiteBannerEnum).ToSelectListFor();
            var model = _biz.GetBannerByID(id);
            return View(model);
        }

        public ActionResult Create()
        {
            ViewData["BannerType"] = DictionaryTools.GetEnumsBy(Enums.SiteBannerEnum).ToSelectListFor();
            return View();
        }

        public ActionResult Save(SiteBannerModel model)
        {
            string path = UploadLinePic("exampleInputFile", model.Type);
            if (!String.IsNullOrEmpty(path))
                model.PicturePath = path;

            if (model.BannerID == default(int))
            {
                model.OwnerCode = UserInfo.OwnerCode;
                model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.CreatedTime = DateTime.Now;
                _biz.SaveBanner(model);
            }
            else
            {
                var entity = _biz.GetBannerByID(model.BannerID);
                if (model.PicturePath.StartsWith("http"))
                    model.PicturePath = model.PicturePath.Substring(model.PicturePath.IndexOf("sh-cct.cn") + 10);

                entity.PicturePath = model.PicturePath;
                entity.Subject = model.Subject;
                entity.SortOrder = model.SortOrder;
                entity.LinkUrl = model.LinkUrl;
                entity.Type = model.Type;
                _biz.SaveBanner(entity);
            }

            return Json(new { code = "1", msg = "保存成功！" });
        }


        private string UploadLinePic(string fileName, string type)
        {
            HttpPostedFileBase file = Request.Files[fileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            var request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            request.VirtualPath = @"banner\" + type;
            UploadServiceClient client = new UploadServiceClient();

            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }
    }
}