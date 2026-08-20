using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Trip.Biz.Base;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    /// <summary>
    /// 领区管理
    /// </summary>
    public class DistrictController : BaseController
    {
        private DistrictBiz _biz = new DistrictBiz();
        private DestinationBiz destinationBiz = new DestinationBiz();
        private ILog _logger = LogManager.GetLogger(typeof(DistrictController));

        /// <summary>
        /// 初始化领区管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(CountryConsularDistrictQModel qModel)
        {
            try
            {
                qModel.OwnerCode = UserInfo.OwnerCode;
                qModel.countryPagedList = _biz.SearchCountryPagedList(qModel);
                return View("~/Views/Visa/District/Index.cshtml", qModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 初始化添加领区页面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCountryConsularDistrict()
        {
            try
            {
                CountryConsularDistrictQModel qmodel = new CountryConsularDistrictQModel();
                qmodel.ConsularDistrictList = new List<VisaCountryConsularDistrictModel>();
                qmodel.country = new VisaCountryInfoModel() { };
                return View("~/Views/Visa/District/AddCountryConsularDistrict.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 初始化新增领区页面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddDistrict(VisaCountryConsularDistrictModel model)
        {
            try
            {
                InitData();
                model.CountryName = destinationBiz.GetByStr(model.CountryCode).Name;
                return View("~/Views/Visa/District/ConsularDistrict.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 修改领区页面
        /// </summary>
        /// <returns></returns>ModifyCountryConsularDistrict.cshtml
        public ActionResult ConsularDistrict(string code)
        {
            try
            {
                InitData();
                VisaCountryConsularDistrictModel model = new VisaCountryConsularDistrictModel();
                model = _biz.GetConsularDis(code);
                return View("~/Views/Visa/District/ConsularDistrict.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 删除领区
        /// </summary>
        /// <param name="code"></param>
        /// <param name="visacountryCode"></param>
        public ActionResult DeleteConsularDistrict(string code, string visacountryCode, string isvalid)
        {
            bool b = (isvalid == "1" ? true : false);
            _biz.DeleteConsularDis(code, b);

            //CountryConsularDistrictQModel qmodel = new CountryConsularDistrictQModel();
            //qmodel.ConsularDistrictList = _biz.SearchConsularDistrictList(visacountryCode);
            //return View("~/Views/Visa/District/ConsularDistrictList.cshtml", qmodel);
            return Content("1");
        }

        /// <summary>
        /// 修改领区页面
        /// </summary>
        /// <param name="visacountryCode"></param>
        /// <returns></retu
        public ActionResult ModifyDistrict(string visacountryCode)
        {
            try
            {
                CountryConsularDistrictQModel qmodel = new CountryConsularDistrictQModel();
                qmodel.ConsularDistrictList = _biz.SearchConsularDistrictList(visacountryCode);
                qmodel.country = _biz.GetVisaCountryInfo(visacountryCode, UserInfo.OwnerCode);
                return View("~/Views/Visa/District/ModifyDistrict.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 保存领区
        /// </summary>
        /// <param name="model"></param>
        public void SaveConsularDistrict(VisaCountryConsularDistrictModel model)
        {
            try
            {
                if (model.Id == 0)//新增
                {
                    model.CreateDate = DateTime.Now;
                    model.CreateBy = GlobalContext.Current.UserInfo.Code;
                    model.ModifyDate = DateTime.Now;
                    model.ConsularDistrictCode = "V" + DBTools.GetSeqNo("Visa_District");
                    _biz.SaveConsularDis(model);
                }
                else //修改
                {
                    model.ModifyBy = GlobalContext.Current.UserInfo.Code;
                    _biz.ModifyConsularDis(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 领区列表子页面
        /// </summary>
        /// <param name="visacountryCode"></param>
        /// <returns></returns>
        public ActionResult DistrictList(string visacountryCode)
        {
            try
            {
                CountryConsularDistrictQModel qmodel = new CountryConsularDistrictQModel();
                qmodel.ConsularDistrictList = _biz.SearchConsularDistrictList(visacountryCode);
                return View("~/Views/Visa/District/DistrictList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 检测领区是否已经存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCdKey(VisaCountryConsularDistrictModel model)
        {
            try
            {
                return Content(_biz.IsExistConsularDis(model).ToString().ToLower());
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #region 国家

        /// <summary>
        /// 查询领区分页列表
        /// </summary>
        /// <param name="qmodel"></param>
        /// <returns></returns>
        public ActionResult SearchCountry(CountryConsularDistrictQModel qmodel)
        {
            try
            {
                qmodel.OwnerCode = UserInfo.OwnerCode;
                qmodel.countryPagedList = _biz.SearchCountryPagedList(qmodel);
                return View("~/Views/Visa/District/PagedList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 保存国家信息
        /// </summary>
        /// <param name="model"></param>
        public ActionResult SaveCountryInfo(VisaCountryInfoModel model)
        {
            try
            {
                string pictureUrl = AppSetting.Get("UploadFileRoot");
                if (model.CountryImgPath.StartsWith(pictureUrl))
                {
                    model.CountryImgPath = model.CountryImgPath.Substring(pictureUrl.Length);
                }

                string visacontryCode = "";
                if (model.VisaCountryCode.IsNullOrEmpty())
                {
                    visacontryCode = _biz.SaveCountryInfo(model);
                }
                else
                {
                    visacontryCode = model.VisaCountryCode;
                    model.OwnerCode = UserInfo.OwnerCode;
                    _biz.ModifyCountryInfo(model);
                }

                CountryConsularDistrictQModel qmodel = new CountryConsularDistrictQModel();
                qmodel.ConsularDistrictList = _biz.SearchConsularDistrictList(model.VisaCountryCode);
                qmodel.country = _biz.GetVisaCountryInfo(model.VisaCountryCode, UserInfo.OwnerCode);
                return View("~/Views/Visa/District/CountryInfo.cshtml", qmodel);
                // return Content("1");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 检测国家是否已经存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCountry(VisaCountryInfoModel model)
        {
            try
            {
                model.OwnerCode = UserInfo.OwnerCode;
                return Content(_biz.IsExistCountryInfo(model).ToString().ToLower());
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 国家

        #region 图片

        /// <summary>
        /// 选择图片初始化
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ShowSelectImg(string VisaCountryParentStr)
        {
            try
            {
                CountryConsularDistrictQModel qmodel = new CountryConsularDistrictQModel();
                if (!VisaCountryParentStr.IsNullOrEmpty())
                {
                    qmodel.ImagePagedIndex = 1;
                    qmodel.CountryCode = VisaCountryParentStr;
                    qmodel.PhotoInfoList = _biz.SearchCityImages(qmodel);
                }
                else
                {
                    VisaCountryParentStr = "";
                    qmodel.CountryCode = VisaCountryParentStr;
                    qmodel.PhotoInfoList = new Lvy.Models.PagedList<Lvy.Models.BaseDB.PhotoInfoModel>();
                }
                return View("~/Views/Visa/District/SelectImage.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 查询城市图片
        /// </summary>
        /// <param name="cityParentStr"></param>
        /// <returns></returns>
        public ActionResult SearchCityImages(CountryConsularDistrictQModel qmodel)
        {
            try
            {
                qmodel.CountryCode = qmodel.VisaCountryParentStr;
                qmodel.CountryName = destinationBiz.GetByStr(qmodel.VisaCountryParentStr).Name;
                qmodel.PhotoInfoList = _biz.SearchCityImages(qmodel);
                return View("~/Views/Visa/District/ImageList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 图片

        #region 下拉框数据初始化

        public void InitData()
        {
            ViewData["VisaArea"] = DictionaryTools.GetEnumsBy(Enums.VisaAreaEnum).ToSelectListFor(t => t.Key, t => t.Value);
        }

        #endregion 下拉框数据初始化
    }
}