using Common.Logging;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Trip.Biz.Site;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    /// <summary>
    /// 预订签证产品
    /// </summary>
    public class BookingController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(BookingController));
        private ProductBiz productService = new ProductBiz();
        private VisaOrderBiz _biz = new VisaOrderBiz();
        private SearchProductBiz homeService = new SearchProductBiz();

        public ActionResult Search()
        {
            try
            {
                ViewData["VisaTypeList"] = DictionaryTools.GetEnumsBy(Enums.VisaTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证种类-");
                ViewData["LinqQuList"] = DictionaryTools.GetEnumsBy(Enums.VisaAreaEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择领区-");
                return View("~/Views/Visa/Booking/Search.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult ProductList(VisaBookingQModel qModel)
        {
            try
            {
                qModel.OwnerCode = UserInfo.OwnerCode;
                qModel.VisaInformationList = homeService.QueryOnLineProductPagedList(qModel);
                return View("~/Views/Visa/Booking/List.cshtml", qModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult ProductInfo(string InformationCode)
        {
            try
            {
                VisaInformationModel model = productService.GetProductByCode(InformationCode);
                if (null != model)
                {
                    var qModel = new OnLineProductQModel
                    {
                        VisaModel = model,
                        VisaDataModels = productService.GetVisaDataList(model.InformationCode),
                        VisaCategoryModels = productService.GetCategroyList(model.InformationCode),
                        VisaDataFileModels = productService.GetVisaMaterialFileList(model.InformationCode)
                    };
                    return View("~/Views/Visa/Booking/ProductInfo.cshtml", qModel);
                }
                return View("404");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult Bookinginfo(string id)
        {
            try
            {
                var qModel = new BookingQModel();
                //产品
                qModel.ProductModel = productService.GetProductByCode(id);
                return View("~/Views/Visa/Booking/BookingInfo.cshtml", qModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult GoBackBookingInfo()
        {
            try
            {
                var qModel = Session["VisaOrderData"] as BookingQModel;
                if (null != qModel)
                {
                    //产品
                    qModel.ProductModel = productService.GetProductByCode(qModel.ProductModel.InformationCode);
                }
                return View("~/Views/Visa/Booking/Bookinginfo.cshtml", qModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 订单确认
        /// </summary>
        /// <param name="qModel"></param>
        /// <returns></returns>
        public ActionResult BookingConfirm(BookingQModel qModel)
        {
            try
            {
                Session["VisaOrderData"] = qModel;
                return View("~/Views/Visa/Booking/BookingConfirm.cshtml", qModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 订单保存
        /// </summary>
        /// <returns></returns>
        public string AddBooking()
        {
            try
            {
                var qModel = Session["VisaOrderData"] as BookingQModel;
                if (null != qModel)
                {
                    qModel.OwnerCode = UserInfo.OwnerCode;
                    //产品
                    qModel.ProductModel = productService.GetProductByCode(qModel.ProductModel.InformationCode);
                    qModel.ClientIP = WebToolKit.GetClientIp();
                    _biz.SaveVisaOrder(qModel, UserInfo);
                    Session["VisaOrderData"] = null;
                    return qModel.OrderCode;
                }
                return "-1";
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return "-1";
            }
        }

        public ActionResult BookingOk(string ordercode)
        {
            try
            {
                ViewData["ordercode"] = ordercode;
                return View("~/Views/Visa/Booking/BookingOk.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }
    }
}