using Arch.Common;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Ticket;
using Lvy.VModels;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.TktManage
{
    /// <summary>
    /// 通用旅游产品管理
    /// </summary>
    public class TktManageController : BaseController
    {
        private ILog logger = LogManager.GetLogger("TktManageController");
        private readonly BaseTagBiz _tagBiz = new BaseTagBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly TktProductBiz _biz = new TktProductBiz();
        private readonly TktPriceRuleBiz ruleBiz = new TktPriceRuleBiz();

        #region Search

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchTicket(SearchTicketVModel vModel)
        {
            if (vModel.PagedTickets == null)
                vModel.PagedTickets = new PagedList<TktProductModel>();
            //产品部门
            ViewData["TeamList"] = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName, "", "", "-选择部门-");
            ViewData["ImportType"] = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key="0",Value="公司录入"},
                                         new KeyValueBean{Key="1",Value="外社录入"}
                                     }.ToSelectListFor();

            vModel.PagedTickets = _biz.GetPagedTicket(vModel, OwnerCode);
            if (Request.IsAjaxRequest())
                return PartialView("Ticket/UCTicketList", vModel);
            return View("Ticket/SearchTicket", vModel);
        }

        /// <summary>
        /// 改变上下线状态
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public string ChangeOnlineState(string productId)
        {
            var model = _biz.GetById(productId);
            if (null != model)
            {
                string currentState;
                switch (model.ProductState)
                {
                    case 2:
                        model.ProductState = 3;
                        currentState = "3";
                        break;

                    case 3:
                        model.ProductState = 2;
                        currentState = "2";
                        break;

                    default:
                        return "0";
                }
                if (_biz.Update(model, UserInfo) > 0)
                {
                    return currentState;
                }
            }
            return "error";
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public string GetState(string productId)
        {
            var model = _biz.GetById(productId);
            return model == null ? "error" : model.ProductState.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 检查是否产生订单
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public string CheckeOrdered(string productId)
        {
            var model = new TktOrderBiz().GetOrderDetailsByProductId(productId);
            if (null != model && model.Count > 0)
            {
                return "1";
            }
            return "0";
        }

        /// <summary>
        /// 删除门票
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteTicket(string productId)
        {
            var model = _biz.GetById(productId);
            if (model.ProductState != 0)
            {
                model.ProductState = 0;
                _biz.Update(model, UserInfo);
            }
            return RedirectToAction("SearchTicket");
        }

        /// <summary>
        /// 编辑专管员
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        public ActionResult EditTicketAdmin(string ticketId)
        {
            var vModel = new TktAdminBiz().GetEditLineAdminVModel(ticketId, UserInfo);
            return PartialView("Ticket/UCEditTicketAdmin", vModel);
        }

        /// <summary>
        /// 保存专管员
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ContentResult SaveTicketAdmin(EditTicketAdminVModel vModel)
        {
            return Content(new TktAdminBiz().SaveTicketAdmin(vModel));
        }

        #endregion Search

        #region Add

        /// <summary>
        /// 创建门票（初始化）
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateTicket()
        {
            var userInfo = GlobalContext.Current.UserInfo;
            var vModel = new EditTicketVModel
            {
                Operation = TicketOperation.Add,
                SupplierName = userInfo.CustomerName,
                TicketProduct = new TktProductModel
                {
                    SupplierCode = userInfo.CustomerCode,
                    TuiJianType = 1,
                    PriceMode = 1
                },
            };
            InitEditData();

            return View("Ticket/EditTicket", vModel);
        }

        /// <summary>
        /// 创建门票（提交）
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult SaveAdd(EditTicketVModel vModel)
        {
            string ticketId = _biz.AddTicket(vModel, UserInfo);
            return Json(new EidtTktResultModel("success", TicketOperation.Add, ticketId), JsonRequestBehavior.AllowGet);
        }

        #endregion Add

        #region Common

        /// <summary>
        /// 验证ProductName是否存在
        ///     存在：true，不存在：false
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public string ValidProductName(string productName = null)
        {
            return _biz.GetByName(productName, OwnerCode) != null ? "true" : "false";
        }

        #endregion Common

        #region Copy

        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public ActionResult CopyTicket(string productId)
        {
            var vModel = GetEditVModel(productId, TicketOperation.Copy);
            return View("Ticket/EditTicket", vModel);
        }

        /// <summary>
        /// 复制（保存）
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult SaveCopy(EditTicketVModel vModel)
        {
            string ticketId = _biz.CopyTicket(vModel, UserInfo);
            return Json(new EidtTktResultModel("success", TicketOperation.Copy, ticketId), JsonRequestBehavior.AllowGet);
        }

        #endregion Copy

        #region Private Common

        /// <summary>
        /// 获取初始化编辑对象
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private EditTicketVModel GetEditVModel(string productId, TicketOperation operation)
        {
            var ticket = _biz.GetById(productId);
            var vModel = new EditTicketVModel
            {
                Operation = operation,
                TicketProduct = ticket,
                ArriveDestName = DictionaryTools.GetDestNameStr(ticket.ArriveDest),
                SupplierName = DictionaryTools.GetCachedCustomer(ticket.SupplierCode).Name
            };
            return vModel;
        }

        private void InitEditData()
        {
            ViewData["ThemesList"] = _tagBiz.GetTags(UserInfo.OwnerCode, 2);
            ViewData["TeamList"] = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
        }

        #endregion Private Common

        #region Edit

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ActionResult EditTicket(string productId)
        {
            var vModel = GetEditVModel(productId, TicketOperation.Edit);
            InitEditData();
            return View("Ticket/EditTicket", vModel);
        }

        /// <summary>
        /// 编辑（保存）
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult SaveEdit(EditTicketVModel vModel)
        {
            try
            {
                string ticketId = vModel.TicketProduct.ProductId;
                _biz.SaveEdit(vModel);
                return Json(new EidtTktResultModel("success", TicketOperation.Edit, ticketId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new EidtTktResultModel("failure", TicketOperation.Edit, vModel.TicketProduct.PlaceCode), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Edit

        #region Picture

        public ActionResult PhotoView(string productId)
        {
            TktProductModel model = _biz.GetById(productId);

            if (!productId.IsNullOrEmpty())
                model.FileList = _biz.GetFileList(productId);
            return PartialView("Ticket/UCPhotoView", model);
        }

        public ActionResult SetPrimaryImage(int id, string productId)
        {
            TktFileModel fmodel = _biz.GetTktFileModel(id);
            int i = _biz.SetPrimaryPic(productId, fmodel.FilePath);

            // 重复 A
            TktProductModel model = _biz.GetById(productId);
            model.FileList = _biz.GetFileList(productId);
            return PartialView("Ticket/UCPhotoView", model);
        }

        public ActionResult DeletePicture(int id)
        {
            _biz.DeleteFile(id);

            return Json(new CommonJsonResult { Code="1", Message="" });
        }

        [HttpPost]
        public ActionResult UploadPhoto(TktFileModel model)
        {
            int fileSize = 0;
            var path = ToUploadPhoto("UploadFile", ref fileSize, model.ProductID);
            if (string.IsNullOrEmpty(path))
                return Content("0");

            model.FileSize = fileSize;
            model.FilePath = path;
            model.IsValid = 1;
            model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _biz.AddPhoto(model);
            return Content(model.FileID.ToString());
        }

        private string ToUploadPhoto(string fileName, ref int fileSize, string hotelCode)
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
            request.VirtualPath = @"{0}\{1}".With("hotel", hotelCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        #endregion

        #region Price

        /// <summary>
        /// 编辑报价
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public ActionResult EditPrice(string productId)
        {
            return View("Price/EditPrice", RenderEditPriceVModel(productId));
        }

        /// <summary>
        /// 初始化视图对象
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private EditPriceVModel RenderEditPriceVModel(string productId)
        {
            var ticket = _biz.GetById(productId);
            if (null == ticket)
                throw new NullReferenceException("根据门票Id：'" + productId + "'无法找到对应的产品。");

            var currentGeneral = ruleBiz.GetModel(productId, 1);
            var currentPriceList = currentGeneral == null
                ? new List<TktPriceModel> { new TktPriceModel { IsValid = 1, IsStandard = 1 } }
                : ruleBiz.GetPriceList(currentGeneral.Id);
            return new EditPriceVModel
            {
                Operation = currentGeneral == null ? 1 : 2,
                TkcketProduct = ticket,
                PriceRule = currentGeneral,
                PriceList = currentPriceList
            };
        }

        #region General

        /// <summary>
        /// 添加常规报价
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="generalPrice"></param>
        /// <returns></returns>
        public JsonResult AddGeneral(EditPriceVModel vModel, List<TktPriceModel> generalPrice)
        {
            vModel.PriceList = generalPrice;
            ruleBiz.AddGeneral(vModel, UserInfo);

            return Json(new CommonJsonResult { State = "success" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑常规报价
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="generalPrice"></param>
        /// <returns></returns>
        public ActionResult UpdateGeneral(EditPriceVModel vModel, List<TktPriceModel> generalPrice)
        {
            vModel.PriceList = generalPrice;
            ruleBiz.UpdateGeneral(vModel, UserInfo);

            return Json(new CommonJsonResult { State = "success" }, JsonRequestBehavior.AllowGet);
        }

        #endregion General

        #region Other

        /// <summary>
        /// 新增其他报价
        /// </summary>
        /// <returns></returns>
        public ActionResult AddOtherPrice(string productId)
        {
            var ticket = _biz.GetById(productId);
            if (null == ticket)
                throw new NullReferenceException("根据门票Id：'" + productId + "'无法找到对应的产品。");

            var currentGeneral = ruleBiz.GetModel(productId, 1);
            currentGeneral.RuleName = string.Empty;//新增，故清空
            var currentPriceList = ruleBiz.GetPriceList(currentGeneral.Id);
            var vModel = new OtherPriceVModel
            {
                Operation = 1,
                TkcketProduct = ticket,
                PriceRule = currentGeneral,
                PriceList = currentPriceList
            };
            return PartialView("Price/UCEditOtherPrice", vModel);
        }

        /// <summary>
        /// 新增其他报价（保存）
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="otherPrice"></param>
        /// <param name="selectedDays"></param>
        /// <returns></returns>
        public JsonResult SaveOtherPrice(OtherPriceVModel vModel, List<TktPriceModel> otherPrice, List<string> selectedDays)
        {
            vModel.PriceList = otherPrice;
            ruleBiz.AddOther(vModel, UserInfo);

            return Json(new CommonJsonResult { State = "success" }, JsonRequestBehavior.AllowGet);
        }

        #endregion Other

        #region Other List

        public ActionResult SearchOtherPriceList(int generalId = 0)
        {
            if (generalId == 0)
                return null;

            var generalRule = ruleBiz.GetModel(generalId);
            var productId = generalRule.ProductId;
            var otherRules = ruleBiz.GetModels(productId, 0);
            var ticket = _biz.GetById(productId);
            var vModel = new List<OtherPriceListVModel>();
            foreach (var rule in otherRules)
            {
                var model = new OtherPriceListVModel
                {
                    TktType = ticket.TktType,
                    PriceRule = rule,
                    PriceList = ruleBiz.GetPriceList(rule.Id)
                };
                vModel.Add(model);
            }
            return PartialView("Price/UCOtherPriceList", vModel);
        }

        /// <summary>
        /// 删除报价规则
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="ruleId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public JsonResult DeleteOtherRule(string productId, int ruleId = 0)
        {
            ruleBiz.DeleteOtherRule(productId, ruleId);

            return Json(new CommonJsonResult { State = "success" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RestoreOtherRule(string productId, int ruleId = 0)
        {
            ruleBiz.RestoreOtherRule(productId, ruleId);

            return Json(new CommonJsonResult { Code = "1", Message="Success" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑其他报价
        /// </summary>
        /// <returns></returns>
        public ActionResult EditOtherPrice(int ruleId = 0)
        {
            var rule = ruleBiz.GetModel(ruleId);
            var productId = rule.ProductId;
            var ticket = _biz.GetById(productId);
            var priceList = ruleBiz.GetPriceList(ruleId);

            var vModel = new OtherPriceVModel
            {
                Operation = 2,
                TkcketProduct = ticket,
                PriceRule = rule,
                PriceList = priceList
            };
            return PartialView("Price/UCEditOtherPrice", vModel);
        }

        /// <summary>
        /// 编辑其他报价（保存）
        /// </summary>
        /// <returns></returns>
        public JsonResult UpdateOtherPrice(OtherPriceVModel vModel, List<TktPriceModel> otherPrice)
        {
            vModel.PriceList = otherPrice;
            ruleBiz.UpdateOtherPrice(vModel, UserInfo);
            return Json(new CommonJsonResult { State = "success" }, JsonRequestBehavior.AllowGet);
        }

        #endregion Other List

        #endregion Price

        /// <summary>
        /// 获取产品（用于输入提示：目的地+产品）
        ///
        /// j.suggest.ticketsearch.js  废弃
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetProducts(string keyword)
        {
            List<BaseDestinationModel> dest = null;
            List<TktProductModel> product = null;
            if (keyword.IsNullOrEmpty())
            {
                dest = DictionaryBiz.GetCacheDests().Where(a => a.Level == 20)
                        .OrderByDescending(a => a.ClickCnt).Take(12).ToList();
                product = new DictionaryBiz().GetCacheProducts(OwnerCode).OrderByDescending(a => a.PinYin).Take(12).ToList();
            }

            if (!keyword.IsNullOrEmpty())
            {
                keyword = keyword.ToUpper();
                dest = DictionaryBiz.GetCacheDests()
                    .Where(
                        a =>
                        ((a.PinYin != null && a.PinYin.ToUpper().Contains(keyword)) ||
                        (a.JPinYin != null && a.JPinYin.ToUpper().Contains(keyword))
                        || (a.Name != null && a.Name.Contains(keyword))) && a.Level == 20).Take(12).ToList();
                product = new DictionaryBiz().GetCacheProducts(OwnerCode)
                    .Where(
                        a =>
                        (a.PinYin != null && a.PinYin.ToUpper().Contains(keyword)) ||
                        (a.JPinYin != null && a.JPinYin.ToUpper().Contains(keyword))
                        || (a.ProductName != null && a.ProductName.Contains(keyword))).Take(12).ToList();
            }

            return Json(new { Dest = dest, Product = product });
        }

        /// <summary>
        /// 获取产品（用于输入提示：产品）
        ///
        /// j.suggest.ticket.js   废弃
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetTickets(string keyword)
        {
            List<TktProductModel> product = null;
            if (keyword.IsNullOrEmpty())
            {
                product = new DictionaryBiz().GetCacheProducts(OwnerCode).OrderByDescending(a => a.PinYin).Take(12).ToList();
            }

            if (!keyword.IsNullOrEmpty())
            {
                keyword = keyword.ToUpper();
                product = new DictionaryBiz().GetCacheProducts(OwnerCode)
                    .Where(
                        a =>
                        (a.PinYin != null && a.PinYin.ToUpper().Contains(keyword)) ||
                        (a.JPinYin != null && a.JPinYin.ToUpper().Contains(keyword))
                        || (a.ProductName != null && a.ProductName.Contains(keyword))).Take(12).ToList();
            }

            return Json(product);
        }

        public ActionResult UCBatchPrice(string id)
        {
            BatchPriceVModel model = new BatchPriceVModel();
            model.ProductID = id;
            model.TkcketProduct = _biz.GetById(id);
            ViewData["RuleList"] = ruleBiz.GetModels(id).Where(m => m.IsValid==1).ToSelectListFor(m => m.Id.ToString(), m=> m.RuleName);

            return View("Price/UCBatchPrice", model);
        }
        [HttpPost]
        public ActionResult UCBatchPrice(BatchPriceVModel model)
        {
            ruleBiz.BatchPrice(model);

            return Json(new { Code = "1", Message = "" });
        }

        /// <summary>
        /// 取得线路所有开班（FullCalendar）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetCalendar(string id, string start, string end)
        {
            var rules = ruleBiz.GetModels(id);
            var plans = ruleBiz.GetMaps(id, start, end);

            var rr = (from mm in plans
                      join ss in rules on mm.RuleId equals ss.Id
                     // where mm.CurrentDate > DateTime.Now
                      select new
                      {
                          title = ss.RuleName + "\n" + ss.MarketPrice.ToString("￥00"),
                          start = mm.CurrentDate.ToDateFormat(),
                          backgroundColor = ss.BgColor,
                          extendedProps = new PlanExtendedModel
                          {
                              RuleName = ss.RuleName,
                              MarketPrice = ss.MarketPrice,
                              SettlePrice = ss.SettlePrice,
                              PlanQuota = mm.PlanQuota,
                              UsedQuota = mm.UsedQuota
                          }
                      }).ToList();

            //{
            //  title: 'Click for Google',
            //  start: new Date(y, m, 28),
            //  end: new Date(y, m, 29),
            //  url: 'http://google.com/',
            //  backgroundColor: '#3c8dbc', //Primary (light-blue)
            //  borderColor: '#3c8dbc' //Primary (light-blue)
            //}
            return Json(rr, JsonRequestBehavior.AllowGet);
        }
    }

    public class PlanExtendedModel
    {
        public string RuleName { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal SettlePrice { get; set; }
        public int PlanQuota { get; set; }
        public int UsedQuota { get; set; }
    }
}