using Arch.Common;
using Common.Logging;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    public class VisaDataController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(VisaDataController));
        private DataBiz _biz = new DataBiz();
        private ProductBiz _productBiz = new ProductBiz();

        /// <summary>
        /// 签证材料页面初始化
        /// </summary>
        /// <param name="InformationCode"></param>
        /// <param name="CurrentTabNum"></param>
        /// <returns></returns>
        public ActionResult VisaDataPartial(string InformationCode, int CurrentTabNum)
        {
            try
            {
                var qmodel = new VisaDataQModel() { InformationCode = InformationCode };
                return View("~/Views/Visa/VisaData/VisaDataPartial.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;//return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        /// 新增材料页面初始化
        /// </summary>
        /// <returns></returns>
        public ActionResult AddVisaData(VisaDataModel model)
        {
            model.CategoryName = _biz.GetCategoryByCode(model.CategoryCode).CategoryName;
            return View("~/Views/Visa/VisaData/VisaDataAdd.cshtml", model);
        }

        /// <summary>
        /// 获取某个分类下面的材料列表
        /// </summary>
        /// <param name="qmodel"></param>
        /// <returns></returns>
        public ActionResult GetVisaData(VisaDataQModel qmodel)
        {
            try
            {
                qmodel.DataList = _biz.GetVisaDataList(new VisaDataModel() { InformationCode = qmodel.InformationCode, CategoryCode = qmodel.CategoryCode });
                return View("~/Views/Visa/VisaData/VisaDataList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;//return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        ///保存签证材料
        /// </summary>
        /// <param name="model"></param>
        [ValidateInput(false)]
        public void SaveVisaData(VisaDataModel model)
        {
            try
            {
                //附件处理
                if (model.DataFilesList != null && model.DataFilesList.Count() > 0)
                {
                    var i = 0;
                    foreach (var file in model.DataFilesList)
                    {
                        HttpPostedFileBase wordUpload = Request.Files["fileUploadVisa_" + i];
                        if (wordUpload.FileName != "")
                        {
                            file.FileUrl = SaveFile(wordUpload, model.InformationCode);
                            file.FileName = file.FileName;
                        }
                        i++;
                    }
                }

                
                _biz.SaveVisaDataSingle(model, UserInfo, WebToolKit.GetClientIp());
                //清产品详细页缓存
                VisaInformationModel promodel = new VisaInformationModel();
                promodel.InformationCode = model.InformationCode;
                promodel = _productBiz.GetProductBaseInfoByCode(promodel.InformationCode);
                if (promodel.State == 5)
                {
                    RemoveWebCacheByKey("CacheKey=Visa|ProductDetail|RecommendModule:" + promodel.InformationCode);
                }
                var jsStr = new StringBuilder();
                jsStr.Append("<script type=\"text/javascript\" language=\"javascript\">");
                jsStr.Append("var index = parent.layer.getFrameIndex(window.name);");
                jsStr.Append("  alert(\"保存成功！！\");");
                jsStr.Append(" parent.funGetVisaDatas('" + model.CategoryCode + "','" + model.InformationCode + "');");
                jsStr.Append(" parent.layer.close(index);");
                jsStr.Append("</script>");
                Response.Write(jsStr);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取产品下所有的材料分类列表
        /// </summary>
        /// <param name="informationCode"></param>
        /// <returns></returns>
        public ActionResult VisaCategoryList(string informationCode)
        {
            VisaDataQModel qmodel = new VisaDataQModel();
            qmodel.CategroyList = _productBiz.GetCategroyList(informationCode);
            qmodel.InformationCode = informationCode;
            return View("/Views/Visa/VisaData/VisaCategoryList.cshtml", qmodel);
        }

        /// <summary>
        /// 签证材料分类添加初始化页面
        /// </summary>
        /// <param name="InformationCode"></param>
        /// <returns></returns>
        public ActionResult VisaCategoryAdd(string InformationCode, int IsFirst)
        {
            try
            {
                var model = new VisaCategoryModel
                {
                    InformationCode = InformationCode,
                    IsFirst = IsFirst
                };
                return View("~/Views/Visa/VisaData/VisaCategoryAdd.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex; // return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        /// 根据产品编码获取该产品下面的列表信息
        /// </summary>
        /// <param name="InformationCode"></param>
        /// <returns></returns>
        public ActionResult GetCategoryList(string informationCode)
        {
            return Json(_productBiz.GetCategroyList(informationCode));
        }

        /// <summary>
        /// 保存签证材料分类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public string SaveVisaCategory(VisaCategoryModel model)
        {
            return _productBiz.SaveVisaCategroy(model, UserInfo, WebToolKit.GetClientIp());
        }

        /// <summary>
        /// 初始化签证材料编辑页面
        /// </summary>
        /// <param name="DataCode"></param>
        /// <returns></returns>
        public ActionResult ModifyVisaData(string DataCode)
        {
            try
            {
                var model = new VisaDataModel();
                model = _biz.GetVisaDataByCode(DataCode);
                if (model != null)
                {
                    model.CategoryName = _productBiz.GetNameByCode(model.CategoryCode).CategoryName;
                }
                return View("~/Views/Visa/VisaData/VisaDataModify.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex; //return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        /// 获取某个分类下面的签证材料条数
        /// </summary>
        /// <param name="qmodel"></param>
        /// <returns></returns>
        public int SearchVisaDatasCount(VisaDataQModel qmodel)
        {
            try
            {
                var model = new VisaDataModel() { InformationCode = qmodel.InformationCode, CategoryCode = qmodel.CategoryCode };
                int count = _biz.SearchVisaDatasCount(model);
                int tempCout = _biz.GetCountVisaDataSTemp(qmodel.InformationCode);//模板中的总数
                if (count == 0)
                {
                    if (tempCout == 0)
                    {
                        count = 1;
                    }
                }
                //if (count!=0)
                //{
                //    count= visadataService.SearchDatasCountNoTemp(model);
                //}
                return count;
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return -1;
            }
        }

        /// <summary>
        /// 获取产品下面所有的模板材料
        /// </summary>
        /// <param name="informationCode"></param>
        /// <returns></returns>
        public ActionResult SearchVisaDatasTemplate(string informationCode, string categoryCode)
        {
            try
            {
                var qmodel = new VisaDataQModel() { InformationCode = informationCode };
                qmodel.DataList = _biz.GetVisaDatasTemplate(informationCode);
                qmodel.CategoryCode = categoryCode;
                qmodel.InformationCode = informationCode;
                return View("~/Views/Visa/VisaData/VisaDataTemplateList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;//return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        /// 根据材料编码字符串查询材料列表
        /// </summary>
        /// <param name="VisaDatasStr">拼接材料字符串</param>
        /// <returns></returns>
        public void SaveDatasByVisaDataStr(string VisaDatasStr, string CategoryCode, string infoCode)
        {
            try
            {
                var qmodel = new VisaDataQModel();
                _biz.AddVisaDatasByCodeStr(VisaDatasStr, CategoryCode, UserInfo, WebToolKit.GetClientIp());
                //清产品详细页缓存
                VisaInformationModel promodel = new VisaInformationModel();
                promodel.InformationCode = infoCode;
                promodel = _productBiz.GetProductBaseInfoByCode(promodel.InformationCode);
                if (promodel.State == 5)
                {
                    RemoveWebCacheByKey("CacheKey=Visa|ProductDetail|RecommendModule:" + promodel.InformationCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #region 上传的文件保存

        /// <summary>
        /// 文件保存返回保存路径
        /// </summary>
        /// <param name="wordUpload"></param>
        /// <returns></returns>
        private string SaveFile(HttpPostedFileBase wordUpload, string productCode)
        {
            var fileExtension = Path.GetExtension(wordUpload.FileName);
            if (fileExtension.ToLower() != ".doc" && fileExtension.ToLower() != ".docx" && fileExtension.ToLower() != ".xls"
                  && fileExtension.ToLower() != ".xlsx" && fileExtension.ToLower() != ".pdf" && fileExtension.ToLower() != ".gif"
                  && fileExtension.ToLower() != ".jpg" && fileExtension.ToLower() != ".jepg" && fileExtension.ToLower() != ".bmp" && fileExtension.ToLower() != ".png")
            {
                return default(string);
            }

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(wordUpload.FileName);
            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(wordUpload.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = string.Format(@"visa\{0}", productCode);
            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);
            string fileUrl = AppSetting.Get("UploadFileRoot") + response.FilePath + response.FileName;

            return fileUrl;
        }

        #endregion 上传的文件保存
    }
}