using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.TourDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.VModels.Tour;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 报团
    /// </summary>
    public class BalanceController : BaseController
    {
        private readonly TourBalanceBiz _biz = new TourBalanceBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TpOrderPayInBiz _payinBiz = new TpOrderPayInBiz();
        private readonly InvoiceBiz _invoiceBiz = new InvoiceBiz();

        private ILog logger = LogManager.GetLogger("BalanceController");

        /// <summary>
        /// 单团核算-视图
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Search(TourBalanceVModel vModel)
        {
            InitPage();
            vModel.Condition.IsPackage = 2;
            //页面第一次加载时设置条件初始值
            vModel.Balances = _biz.GetPageList(vModel, UserInfo.OwnerCode);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);

            return View(vModel);
        }

        // GET: Balance/Create
        public ActionResult Create()
        {
            TourBalanceVModel model = new TourBalanceVModel();
            InitPage();
            return View(model);
        }

        // POST: Balance/Create
        [HttpPost]
        public ActionResult Create(TourBalanceVModel model)
        {
            try
            {
                model.Balance.MasterOrderCode = DBTools.GetSeqNo("TourBalance");
                model.Balance.IsPackage = 2;
                model.Balance.Type = 9;
                model.Balance.OwnerCode = GlobalContext.Current.UserInfo.OwnerCode;
                model.Balance.CreatedTime = DateTime.Now;
                model.Balance.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.Balance.ModifiedTime = DateTime.Now;
                model.Balance.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                _biz.AddBalance(model.Balance);

                return RedirectToAction("Search");
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                InitPage();
                return View(model);
            }
        }

        // GET: Test/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Balance/Edit/5
        /// <summary>
        /// 核算明细-视图
        /// </summary>
        /// <param name="id">总订单号</param>
        /// <returns></returns>
        public ActionResult Edit(string id)
        {
            string ownerCode = GlobalContext.Current.OwnerCode;
            ViewBag.Suppliers = new CustomerBiz().GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name,
                Help1 = DictionaryTools.GetEnumValue(Enums.PaymentTypeEnum, a.PaymentType.ToString()),
                Help2 = a.PaymentType.ToString()
            });
            InitPage();
            return View(UpdateTour(id));
        }

        // POST: Balance/Edit/5
        [HttpPost]
        public ActionResult Edit(TourBalanceVModel vModel)
        {
            try
            {
                var orderCode = vModel.Balance.MasterOrderCode;
                var tourBalance = _biz.GetBalanceByOrderCode(orderCode);  //获取单团
                var paylist = _payinBiz.GetPayInList(orderCode);

                // 更新字段
                tourBalance.ProductName = vModel.Balance.ProductName;
                tourBalance.TeamId = vModel.Balance.TeamId;
                tourBalance.SalesTeamId = vModel.Balance.SalesTeamId;
                tourBalance.TourNo = vModel.Balance.TourNo;
                tourBalance.OutDate = vModel.Balance.OutDate;
                tourBalance.AgentCode = vModel.Balance.AgentCode;
                tourBalance.TravelDays = vModel.Balance.TravelDays;
                tourBalance.GuideName = vModel.Balance.GuideName;
                tourBalance.SalerCode = vModel.Balance.SalerCode;

                tourBalance.TouristName = vModel.Balance.TouristName;
                tourBalance.TouristPhone = vModel.Balance.TouristPhone;
                tourBalance.Num = vModel.Balance.Num;
                tourBalance.AuditPax = vModel.Balance.AuditPax;
                tourBalance.ChildPax = vModel.Balance.ChildPax;
                tourBalance.OldPax = vModel.Balance.OldPax;
                tourBalance.AuditPax = vModel.Balance.AuditPax;

                //TODO
                tourBalance.YingShou = paylist.Sum(t => t.Amount);
                tourBalance.YiShou = paylist.Where(t => t.State == 20).Sum(t => t.Amount);
                tourBalance.TotalCost = vModel.CostList.Sum(t => t.ItemCost);
                tourBalance.MaoLi = tourBalance.YiShou - tourBalance.TotalCost;
                _biz.UpdateBalance(tourBalance);

                // 更新成本
                var Costs = _biz.GetCostsByOrderCode(orderCode);
                foreach (var costRule in Costs)
                {
                    // 更新数据和添加
                    var d = vModel.CostList.Where(t => t.Id == costRule.Id).FirstOrDefault();
                    if (d != null)
                    {
                        costRule.Item = d.Item;
                        costRule.Cost = d.Cost;
                        costRule.Currency = d.Currency;
                        costRule.ROE = d.ROE;
                        costRule.Num = d.Num;
                        costRule.ItemCost = d.ItemCost;
                        costRule.Remark = d.Remark;
                        costRule.PaymentType = d.PaymentType;
                        costRule.SupplierId = d.SupplierId;
                        costRule.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                        costRule.ModifiedTime = DateTime.Now;
                    }
                    else
                    {
                        costRule.IsValid = 0;
                    }
                    _biz.UpdateCost(costRule);
                }

                // 添加新的
                foreach (var d in vModel.CostList.Where(t => t.Id == default(int)))
                {
                    TpTourCostModel tourCost = new TpTourCostModel();
                    tourCost.MasterOrderCode = orderCode;
                    tourCost.Code = DBTools.GetSeqNo("TourCost");
                    tourCost.SupplierId = d.SupplierId;
                    tourCost.Item = d.Item;
                    tourCost.Cost = d.Cost;
                    tourCost.Currency = d.Currency;
                    tourCost.ROE = d.ROE;
                    tourCost.Num = d.Num;
                    tourCost.ItemCost = d.ItemCost;
                    tourCost.Remark = d.Remark;
                    tourCost.PaymentType = d.PaymentType;
                    tourCost.IsValid = 1;
                    tourCost.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                    tourCost.ModifiedTime = DateTime.Now;
                    _biz.SaveCost(tourCost);
                }
                return SaveResult("1", Url.Action("Edit", new { tourId = orderCode }));
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
            }
            return SaveResult("0", Url.Action("Edit", new { tourId = vModel.Balance.TourId }));
        }

        /// <summary>
        /// 添加一条成本
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult AddRowCost(int rowIndex, string orderCode)
        {
            ViewBag.RowIndex = rowIndex;
            TpTourCostModel vModel = new TpTourCostModel();
            vModel.MasterOrderCode = orderCode;
            string ownerCode = GlobalContext.Current.OwnerCode;

            //ViewBag.Suppliers = _customerBiz.GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            //{
            //    Key = a.Code,
            //    Value = a.Name,
            //    Help1 = DictionaryTools.GetEnumValue(Enums.PaymentTypeEnum, a.PaymentType.ToString()),
            //    Help2 = a.PaymentType.ToString()
            //}).ToList();
            return PartialView("UCRowCost", vModel);
        }

        /// <summary>
        /// 上传缴款单凭证
        /// </summary>
        /// <returns></returns>
        public ActionResult AddUploadFile(TourBalanceVModel vModel)
        {
            string filename = "";
            string fileExt = "";
            TpTourBalanceModel orderModel = _biz.GetBalanceByOrderCode(vModel.FileModel.MasterOrderCode);
            if (orderModel != null)
            {
                string FilePath = UploadTourFile(vModel.FileModel.MasterOrderCode, "tourFileName", ref filename, ref fileExt);

                TourFileModel model = new TourFileModel();
                model.SourceType = vModel.FileModel.SourceType;
                model.MasterOrderCode = vModel.FileModel.MasterOrderCode;
                model.FileName = filename;
                model.FilePath = FilePath;
                model.CreatedTime = DateTime.Now;
                model.Remark = vModel.FileModel.Remark;
                model.IsDel = 0;
                model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.MediaType = WebToolKit.GetFileMedia(fileExt);
                _biz.AddTourFile(model);
            }

            vModel.FileList = _biz.GetFileList(vModel.FileModel.MasterOrderCode);
            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();

            return PartialView("UCFile", vModel);
        }

        private string UploadTourFile(string orderCode, string requestFileName, ref string file_name, ref string file_extension)
        {
            HttpPostedFileBase file = Request.Files[requestFileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            file_name = file.FileName;
            file_extension = Path.GetExtension(file.FileName);
            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = string.Format(@"package\{0}", orderCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        public ActionResult DeleteTourFile(int id)
        {
            TourFileModel model = _biz.GetFileById(id);
            _biz.DeleteTourFile(id);

            // 重新查询
            TourBalanceVModel md = new TourBalanceVModel();
            md.FileList = _biz.GetFileList(model.MasterOrderCode);
            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).ToSelectListFor();
            return PartialView("UCFile", md);
        }

        #region 缴款部分

        /// <summary>
        /// 新增缴款单页面
        /// </summary>
        /// <param name="id">缴款单Id</param>
        /// <returns></returns>
        public ActionResult EditPayIn(int id, string orderCode, int type = 9)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            vModel.Balance = _biz.GetBalanceByOrderCode(orderCode);
            vModel.FileList = _biz.GetFileList(orderCode);

            if (id != default(int))
            {
                vModel.PayInModel = _payinBiz.GetById(id);
            }
            else
            {
                vModel.PayInModel = new TpOrderPayInModel();
                vModel.PayInModel.OrderCode = orderCode;
                vModel.PayInModel.CustomerName = vModel.Balance.AgentName;
                vModel.PayInModel.PayInBy = vModel.Balance.Saler;
                vModel.PayInModel.Type = type;

                if (!vModel.PayInModel.CustomerCode.IsNullOrEmpty())
                {
                    // 获得税号
                    var custModel = _customerBiz.GetById(vModel.PayInModel.CustomerCode);
                    if (custModel != null)
                    {
                        vModel.PayInModel.TaxNumber = custModel.TaxNumber;
                        vModel.PayInModel.CustomerName = custModel.Name;
                    }
                }
            }

            ViewBag.PayType = DictionaryTools.GetEnumsBy(Enums.PayTypeEnum).ToSelectListFor();
            ViewData["UserList"] = _accountBiz.GetAccountByTeam(GlobalContext.Current.OwnerCode, vModel.Balance.SalesTeamId).ToSelectListFor(t => t.Code, t => t.Name);
            return View(vModel);
        }

        public ActionResult SavePayIn(TourBalanceVModel vModel)
        {
            if (!string.IsNullOrEmpty(vModel.PayInModel.TaxNumber))
            {
                //更新掉客户的税号信息.
                _customerBiz.UpdateTaxNumber(vModel.PayInModel.CustomerCode, vModel.PayInModel.TaxNumber);
            }

            #region 上传凭证信息

            HttpPostedFileBase file = Request.Files["bankFile"];
            if (file != null && file.ContentLength > 0)
            {
                string filename = "";
                string filenameExt = "";
                string FilePath = UploadTourFile(vModel.PayInModel.OrderCode, "bankFile", ref filename, ref filenameExt);

                TourFileModel model = new TourFileModel();
                model.MasterOrderCode = vModel.PayInModel.OrderCode;
                model.FileName = filename;
                model.FilePath = FilePath;
                model.CreatedTime = DateTime.Now;
                model.Remark = vModel.PayInModel.Remark;
                model.IsDel = 0;
                model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.MediaType = WebToolKit.GetFileMedia(filenameExt);
                model.SourceType = "2";

                vModel.PayInModel.BillFileId = _biz.AddTourFile(model);
            }
            else if (!string.IsNullOrEmpty(vModel.selectBank))
            {
                var f = _biz.GetFileById(Convert.ToInt32(vModel.selectBank));
                if (f != null)
                    vModel.PayInModel.BillFileId = f.Id;
            }

            #endregion 上传凭证信息

            #region 上传账单

            HttpPostedFileBase file1 = Request.Files["billFile"];
            if (file1 != null && file1.ContentLength > 0)
            {
                string filename = "";
                string filenameExt = "";
                string FilePath = UploadTourFile(vModel.PayInModel.OrderCode, "billFile", ref filename, ref filenameExt);

                TourFileModel model = new TourFileModel();
                model.MasterOrderCode = vModel.PayInModel.OrderCode;
                model.FileName = filename;
                model.FilePath = FilePath;
                model.CreatedTime = DateTime.Now;
                model.Remark = vModel.PayInModel.Remark;
                model.IsDel = 0;
                model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.MediaType = WebToolKit.GetFileMedia(filenameExt);
                model.SourceType = "4";

                vModel.PayInModel.BankFileId = _biz.AddTourFile(model);
            }
            else if (!string.IsNullOrEmpty(vModel.selectBill))
            {
                var f = _biz.GetFileById(Convert.ToInt32(vModel.selectBill));
                if (f != null)
                    vModel.PayInModel.BankFileId = f.Id;
            }

            #endregion 上传账单

            if (vModel.PayInModel.Id == 0)
            {
                vModel.PayInModel.State = 0;//未确认的状态
                vModel.PayInModel.CreatedTime = DateTime.Now;
                vModel.PayInModel.IsValid = 1;
                vModel.PayInModel.Type = vModel.Balance.Type;
                _payinBiz.AddPayIn(vModel.PayInModel);   // 保存
            }
            else
            {
                var entity = _payinBiz.GetById(vModel.PayInModel.Id);
                entity.Item = vModel.PayInModel.Item;
                entity.CustomerCode = vModel.PayInModel.CustomerCode;
                entity.TaxNumber = vModel.PayInModel.TaxNumber;
                entity.Remitter = vModel.PayInModel.Remitter;
                entity.Amount = vModel.PayInModel.Amount;
                entity.PaymentType = vModel.PayInModel.PaymentType;
                entity.Remark = vModel.PayInModel.Remark;
                entity.PayInBy = vModel.PayInModel.PayInBy;

                _payinBiz.Update(entity);
            }

            // 部门报团 更新应收款
            if (vModel.Balance.IsPackage == 2)
            {
                _biz.UpdateBalanceAmount(vModel.Balance.Type, vModel.PayInModel.OrderCode);
            }

            return Json(new { Code = "1", Message = "添加成功" });
        }

        public ActionResult ReLoadPayIn(string orderCode)
        {
            var vModel = new TourBalanceVModel();
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);
            vModel.Balance.MasterOrderCode = orderCode;
            return PartialView("UCPayIn", vModel);
        }

        #endregion 缴款部分

        #region 发票部分

        /// <summary>
        ///  新增发票申请
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="orderCode">订单编号</param>
        /// <param name="lineType">线路类型 出境/国内</param>
        /// <returns></returns>
        public ActionResult CreateInvoice(int Id, string orderCode, string lineType)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            vModel.Balance = _biz.GetBalanceByOrderCode(orderCode);
            if (Id > 0)
            {
                vModel.InvoiceModel = _invoiceBiz.GetInvoiceById(Id);
            }
            else
            {
                vModel.InvoiceModel.OrderCode = orderCode;
            }
            if (lineType == "3")
            {
                // 出境
                ViewBag.InvoiceTitleEnum = DictionaryTools.GetEnumsBy(Enums.OutboundInvoiceTitleEnum).ToSelectListFor(k => k.Value, v => v.Value);
            }
            else
            {
                ViewBag.InvoiceTitleEnum = DictionaryTools.GetEnumsBy(Enums.InboundInvoiceTitleEnum).ToSelectListFor(k => k.Value, v => v.Value);
            }

            return View("EditInvoice", vModel);
        }

        /// <summary>
        /// 保存发票申请
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SaveInvoce(TourBalanceVModel vModel)
        {
            vModel.InvoiceModel.CreatedTime = DateTime.Now;
            _invoiceBiz.AddInvoice(vModel.InvoiceModel);

            return Json(new { Code = "1", Message = "添加成功" });
        }

        public ActionResult ReLoadInvoice(string orderCode)
        {
            var vModel = new TourBalanceVModel();
            vModel.Invoices = _invoiceBiz.GetInvoiceList(orderCode);
            vModel.Balance.MasterOrderCode = orderCode;
            return PartialView("UcInvoice", vModel);
        }

        #endregion 发票部分

        private TourBalanceVModel UpdateTour(string orderCode)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            vModel.CostList = _biz.GetCostsByOrderCode(orderCode);
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);
            vModel.FileList = _biz.GetFileList(orderCode);
            vModel.Invoices = _invoiceBiz.GetInvoiceList(orderCode);
            vModel.Balance = _biz.GetBalanceByOrderCode(orderCode);  //获取单团

            // sum
            vModel.SumCost = new FinanceTotalModel();
            vModel.SumCost.XianShou = vModel.CostList.Where(a => a.PaymentType == 1).Sum(a => a.ItemCost);
            vModel.SumCost.Qiandan = vModel.CostList.Where(a => a.PaymentType != 1).Sum(a => a.ItemCost);
            vModel.SumCost.SumTolCost = vModel.SumCost.XianShou + vModel.SumCost.Qiandan;

            return vModel;
        }

        protected override void InitPage()
        {
            // 线路类型
            ViewBag.ProductTypes = DictionaryTools.GetEnumsBy(Enums.ProductAllTypeEnum).ToSelectListFor("", "", "--请选择类型--");
            //结算状态
            ViewBag.SettlementStateBean = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key = "1",Value = "已结算"},
                                         new KeyValueBean{Key="0",Value="未结算"}
                                     }.ToSelectListFor();
            ViewBag.Teams = _teamBiz.GetBalanceTeams(OwnerCode).ToSelectListFor(t => t.TeamID, t => t.TeamName, "", "", "--请选择部门--");
            //取得门店列表
            ViewBag.Branchs = _customerBiz.GetAllBranch().ToSelectListFor(t => t.Code, t => t.Name);

            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();
        }
    }
}