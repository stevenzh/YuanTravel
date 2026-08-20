using Common.Logging;
using Lvy.Models.HotelDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Biz.Ticket;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.VModels.Hotel;
using Lvy.VModels.Online;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 前台推荐管理
    /// </summary>
    public class HotListController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(HotListController));
        private readonly SiteNavBiz _biz = new SiteNavBiz();
        private readonly SearchProductBiz _searchBiz = new SearchProductBiz();
        private readonly TktProductBiz _ticketBiz = new TktProductBiz();
        private readonly HotelBiz _hotelBiz = new HotelBiz();

        /// <summary>
        /// 查询推荐模块列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            try
            {
                var qmodel = new HotModuleQModel
                {
                    HotModuleList = _biz.SearchModuleList(UserInfo.OwnerCode)
                };
                return View("~/Views/Site/HotList/Index.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 根据推荐模块编号查询签证推荐列表
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        public ActionResult HotList(int ItemID)
        {
            try
            {
                HotProductQModel qmodel = new HotProductQModel();

                qmodel.ModuleModel = _biz.GetNavItemByID(ItemID);

                if (qmodel.ModuleModel.ProductType == "1")
                {
                    qmodel.HotProductList = _biz.GetLineList(ItemID);
                }
                else if (qmodel.ModuleModel.ProductType == "3")
                {
                    qmodel.HotProductList = _biz.GetVisaList(ItemID);
                }
                else if (qmodel.ModuleModel.ProductType == "4")
                {
                    qmodel.HotProductList = _biz.GetHotelList(ItemID);
                }
                else if (qmodel.ModuleModel.ProductType == "9")
                {
                    qmodel.HotProductList = _biz.GetTicketList(ItemID);
                }

                return View("~/Views/Site/HotList/HotList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 删除签证推荐线路
        /// </summary>
        /// <param name="ListID"></param>
        public void DeleteHotVisa(int ListID)
        {
            try
            {
                _biz.DeleteHotVisa(ListID);
                RemoveWebCache("1002");
                RemoveWebCache("1003");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #region 线路产品

        public ActionResult AddLineProduct(int ItemID)
        {
            try
            {
                ViewData["VTypeList"] = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum).ToSelectListFor();
                var vModel = new SearchProductVModel { OwnerCode = OwnerCode };
                var qmodel = new HotProductQModel
                {
                    ItemID = ItemID,
                    LineList = _searchBiz.GetProListByCondition(vModel, 1, 10)
                };
                return View("~/Views/Site/HotList/AddLineProduct.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult SearchLineProduct(SearchProductVModel qmodel, int PagedIndex, int PagedSize)
        {
            try
            {
                HotProductQModel proqmodel = new HotProductQModel();
                proqmodel.LineList = _searchBiz.GetProListByCondition(qmodel, PagedIndex, PagedSize);

                return View("~/Views/Site/HotList/LineProductList.cshtml", proqmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 线路产品

        #region 门票

        public ActionResult AddTicketProduct(int ItemID)
        {
            try
            {
                ViewData["VTypeList"] = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum).ToSelectListFor();
                var qmodel = new HotProductQModel
                {
                    ItemID = ItemID,
                    TicketList = _ticketBiz.GetPagedTicket(new SearchTicketVModel(), OwnerCode)
                };
                return View("~/Views/Site/HotList/AddTicketProduct.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult SearchTicketProduct(SearchTicketVModel qmodel)
        {
            try
            {
                HotProductQModel proqmodel = new HotProductQModel();
                proqmodel.TicketList = _ticketBiz.GetPagedTicket(qmodel, OwnerCode);

                return View("~/Views/Site/HotList/TicketProductList.cshtml", proqmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 门票

        #region 签证部分

        /// <summary>
        /// 添加签证推荐线路初始化
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        public ActionResult AddVisaProduct(int ItemID)
        {
            try
            {
                ViewData["VTypeList"] = DictionaryTools.GetEnumsBy(Enums.VisaVTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证类型-");
                var qmodel = new HotProductQModel
                {
                    ItemID = ItemID,
                    ProductList = _searchBiz.GetProListByCondition(new VisaInformationQModel() { Info = new VisaInformationModel() { State = 5, VType = 1 } }, 1, 10)
                };
                return View("~/Views/Site/HotList/AddVisaProduct.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 根据条件分页查询签证产品列表
        /// </summary>
        /// <param name="qmodel"></param>
        /// <param name="PagedIndex"></param>
        /// <param name="PagedSize"></param>
        /// <returns></returns>
        public ActionResult SearchVisaProduct(HotProductQModel qmodel, int PagedIndex, int PagedSize)
        {
            try
            {
                var proqmodel = new VisaInformationQModel
                {
                    Info = qmodel.ProductInfo
                };
                qmodel.ProductList = _searchBiz.GetProListByCondition(proqmodel, PagedIndex, PagedSize);

                return View("~/Views/Site/HotList/VisaProductList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 签证部分

        /// <summary>
        /// 添加签证推荐线路
        /// </summary>
        /// <param name="qmodel"></param>
        public string AddVisaToList(HotProductQModel qmodel)
        {
            try
            {
                var result = _biz.CheckProIsExist(qmodel);
                if (result.IsNullOrEmpty())
                {
                    // 不存在 就添加到数据库
                    qmodel.CreatedBy = UserInfo.Code;
                    _biz.AddProToHotVisa(qmodel);
                    RemoveWebCache("1002");
                    RemoveWebCache("1003");
                    return "";
                }
                else
                    return result.Substring(0, result.Length - 1);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 保存签证推荐线路排序
        /// </summary>
        /// <param name="qmodel"></param>
        public void SaveHotVisaSort(HotProductQModel qmodel)
        {
            try
            {
                _biz.SaveHotVisaSort(qmodel);
                //RemoveWebCache("1002");
                //RemoveWebCache("1003");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #region 酒店

        public ActionResult AddHotel(int ItemID)
        {
            try
            {
                var qmodel = new HotProductQModel
                {
                    ItemID = ItemID,
                    HotelPageList = _hotelBiz.GetPagedList(new VModels.Hotel.HotelVModel()
                    {
                        OwnerCode = UserInfo.OwnerCode,
                        HotelModel = new HotelModel() { HotelState = 3 }
                    })
                };
                return View("~/Views/Site/HotList/AddHotel.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult SearchHotelProduct(HotProductQModel qmodel, int PagedIndex, int PagedSize)
        {
            try
            {
                VModels.Hotel.HotelVModel vModel = new VModels.Hotel.HotelVModel();
                vModel.OwnerCode = UserInfo.OwnerCode;
                vModel.HotelModel = new HotelModel()
                {
                    HotelCode = qmodel.ProductCode,
                    HotelName = qmodel.ProductName,
                    HotelState = 3

                };
                qmodel.HotelPageList = _hotelBiz.GetPagedList(vModel);

                return View("~/Views/Site/HotList/HotelList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 酒店
    }
}