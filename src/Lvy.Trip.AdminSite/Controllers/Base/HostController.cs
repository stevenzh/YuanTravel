using Arch.Common;
using Arch.Common.Utils;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common.FileUpload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.Json;
using Lvy.Models;
using Lvy.Web.Common;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 平台功能-商户管理
    /// </summary>
    public class HostController : BaseController
    {
        private readonly HostBiz _biz = new HostBiz();

        /// <summary>
        /// 查询客户
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult Search(HostVModel vModel)
        {
            if (vModel == null)
                vModel = new HostVModel();

            vModel.Customers = _biz.GetHostPagedList(vModel);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 创建客户
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var p = DictionaryTools.GetEnumsBys(Enums.HostProfileEnum);
            SysPlatformModel model = new SysPlatformModel();
            var profile = (from s in p
                           select new KeyValueBean
                           {
                               Key = s.Value,
                               Value = ""
                           }).ToList();
            model.ProfileModels = p;

            return View();
        }

        /// <summary>
        /// 编辑客户
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(string code)
        {
            var model = _biz.GetHostBy(code);
            var s = string.IsNullOrEmpty(model.Profile) ? new List<KeyValueBean>() : JsonSerializer.Deserialize<List<KeyValueBean>>(model.Profile);
            var p = DictionaryTools.GetEnumsBys(Enums.HostProfileEnum);
            var profile = new List<KeyValueBean>();
            foreach (var item in p)
            {
                var d = s.Where(m => m.Key == item.Key).FirstOrDefault();
                profile.Add(new KeyValueBean { Key = item.Key, Value = (d == null ? "" : d.Value), Help1 = item.Value });
            }

            model.ProfileModels = profile;

            return View(model);
        }

        /// <summary>
        /// 添加客户
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult Add(SysPlatformModel model)
        {
            model.CrmCustomer.Code = DBTools.GetSeqNo("CrmCustomer");
            model.CrmCustomer.IsOwner = true;  // 系统管理者
            model.CrmCustomer.IsValid = 1;
            model.CrmCustomer.OwnerCode = model.CrmCustomer.Code;
            model.CrmCustomer.PaymentType = 0; // 没有支付类型
            model.CrmCustomer.ModifiedBy = UserInfo.Code;
            model.CrmCustomer.ModifiedTime = DateTime.Now;

            // 商户
            model.CustomerCode = model.CrmCustomer.Code;
            model.Name = model.Name;
            model.IconPath = UploadPic("IconPath", model.CrmCustomer.Code);
            model.SiteLogoPath = UploadPic("LogoPath", model.CrmCustomer.Code);
            model.IsValid = true;
            model.Profile = JsonSerializer.Serialize<List<KeyValueBean>>(model.ProfileModels);
            _biz.AddTrans(model);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 保存客户
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult Update(SysPlatformModel model)
        {
            var entity = _biz.GetHostBy(model.CustomerCode);
            entity.CrmCustomer.Name = model.Name;
            entity.CrmCustomer.ShortName = model.CrmCustomer.ShortName;
            entity.CrmCustomer.Head = model.CrmCustomer.Head;
            entity.CrmCustomer.Mobile = model.CrmCustomer.Mobile;
            entity.CrmCustomer.Phone = model.CrmCustomer.Phone;
            entity.CrmCustomer.Address = model.CrmCustomer.Address;
            entity.CrmCustomer.CreditLine = model.CrmCustomer.CreditLine;
            entity.CrmCustomer.Remarks = model.CrmCustomer.Remarks;
            entity.CrmCustomer.ModifiedBy = UserInfo.Code;
            entity.CrmCustomer.ModifiedTime = DateTime.Now;

            // 商户
            string path1 = UploadPic("IconPath", entity.CustomerCode);
            string path2 = UploadPic("LogoPath", entity.CustomerCode);
            if (!string.IsNullOrEmpty(path1))
                entity.IconPath = path1;
            if (!string.IsNullOrEmpty(path2))
                entity.SiteLogoPath = path2;
            entity.Name = model.Name;
            entity.Url = model.Url;
            entity.Profile = JsonSerializer.Serialize<List<KeyValueBean>>(model.ProfileModels);

            _biz.UpdateTrans(entity);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <returns></returns>
        public ActionResult SetValidState(string code)
        {
            var obj = _biz.GetHostBy(code);

            obj.IsValid = obj.IsValid ? false : true;
            //obj.ModifiedBy = UserInfo.Code;
            //obj.ModifiedTime = DateTime.Now;
            _biz.Update(obj);
            return Json(new { Code = 1, Message = "" });
        }

        #region 私有方法

        /// <summary>
        /// 上传文件
        /// </summary>
        private string UploadPic(string fileName, string customerCode)
        {
            HttpPostedFileBase file = Request.Files[fileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            var request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            request.VirtualPath = @"customer\" + customerCode;

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        #endregion 私有方法
    }
}