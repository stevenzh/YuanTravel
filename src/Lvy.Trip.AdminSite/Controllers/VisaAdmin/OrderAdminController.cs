using Arch.Common;
using Common.Logging;
using Lvy.Models.OrderDB;
using Lvy.Models.TourDB;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Site;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Linq;

namespace Lvy.Visa.AdminSite.Controllers
{
    public class OrderAdminController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(OrderAdminController));
        private readonly VisaOrderBiz orderService = new VisaOrderBiz();
        private readonly ProductBiz productService = new ProductBiz();
        private readonly SearchProductBiz homeService = new SearchProductBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly TpProductBiz _productBiz = new TpProductBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TpChildOrderBiz _childOrderBiz = new TpChildOrderBiz();
        private readonly TpOrderPayInBiz _payinBiz = new TpOrderPayInBiz();
        private readonly TouristBiz touristBiz = new TouristBiz();
        private readonly TourBalanceBiz _balanceBiz = new TourBalanceBiz();

        #region Action Method

        #region 订单搜索

        /// <summary>
        /// 订单管理 初始化
        /// </summary>
        public ActionResult Search(VisaOrderQModel model)
        {
            try
            {
                GetBaseData();
                model.OwnerCode = UserInfo.OwnerCode;
                model.visaOrderModelsList = orderService.SearchOrderList(model);

                return View("~/Views/Visa/OrderAdmin/Search.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        public ActionResult List(VisaOrderQModel qModel)
        {
            try
            {
                qModel.OwnerCode = UserInfo.OwnerCode;
                qModel.visaOrderModelsList = orderService.SearchOrderList(qModel);
                if (Request.IsAjaxRequest())
                    return PartialView("~/Views/Visa/OrderAdmin/List.cshtml", qModel);
                else
                    return View("~/Views/Visa/OrderAdmin/List.cshtml", qModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 处理订单初始化
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifyOrder(string OrderCode)
        {
            try
            {
                var master = orderService.GetMasterOrder(OrderCode);
                var orderModel = orderService.GetVisaOrderByCode(OrderCode);
                var model = new OrderQModel
                {
                    OrderCode = OrderCode,
                    MasterOrder = master,
                    OrderModel = orderModel,
                    ProductModel = homeService.GetVisaProductInfo(orderModel.ProductCode),
                    HistoryList = orderService.SearchOrderHistoryList(OrderCode),
                    TravellerList = touristBiz.GetTouristList(OrderCode),
                    ChildOrderList = orderService.SearchChildOrderList(OrderCode),
                    PayInList = _payinBiz.GetPayInList(OrderCode),
                    FileList = _balanceBiz.GetFileList(OrderCode)
                };

                ViewData["Teams"] = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, t => t.TeamName, "", "", "--请选择部门--");
                if (string.IsNullOrEmpty(model.MasterOrder.SalesTeamId))
                {
                    ViewData["Salers"] = _customerBiz.GetTeamSales(UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                }
                else
                {
                    ViewData["Salers"] = _customerBiz.GetTeamUsersByTeamId(model.MasterOrder.SalesTeamId, UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                }
                ViewData["FileEnum"] = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();

                return View("~/Views/Visa/OrderAdmin/ModifyOrder.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult ReLoadPayIn(string orderCode)
        {
            var vModel = new OrderQModel();
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);
            return PartialView("~/Views/Visa/OrderAdmin/UCPayIn.cshtml", vModel);
        }

        /// <summary>
        /// 修改订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyOrder(TpTourBalanceModel model)
        {
            try
            {
                if (model.OrderState == 5) { model.VisaOrder.SendVisaDate = DateTime.Now; }
                if (model.OrderState == 6) { model.VisaOrder.FinishVisaDate = DateTime.Now; }
                orderService.OrderModify(model, UserInfo, WebToolKit.GetClientIp());
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
            }

            return Content("1");
        }

        /// <summary>
        /// 修改预约出发日期
        /// </summary>
        /// <param name="model"></param>
        public void SaveOrderReadyDate(TpTourBalanceModel model)
        {
            try
            {
                orderService.SaveOrderReadyDate(model, UserInfo, WebToolKit.GetClientIp());
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
            }
        }

        /// <summary>
        /// 修改材料截止收取日期
        /// </summary>
        /// <param name="model"></param>
        public void SaveMaterialDeadline(VisaOrderModel model)
        {
            try
            {
                orderService.SaveMaterialDeadline(model, UserInfo, WebToolKit.GetClientIp());
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
            }
        }

        /// <summary>
        /// 修改操作员
        /// </summary>
        /// <param name="model"></param>
        public void SaveOperateName(VisaOrderModel model)
        {
            try
            {
                orderService.SaveOperateName(model, UserInfo, WebToolKit.GetClientIp());
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
            }
        }

        #endregion 订单搜索

        #region 订单基本信息和子订单

        public ActionResult ShowBaseInfo(string OrderCode)
        {
            try
            {
                IEnumerable<TpChildOrderModel> Qmodel = orderService.SearchChildOrderList(OrderCode);
                //TpTourBalanceModel model = orderService.GetVisaOrderDetails(OrderCode);
                //if (model.VisaOrder.OrderStatus < 5)
                //{ ViewData["OrderStatus"] = "OK"; }
                //else
                //{ ViewData["OrderStatus"] = "NO"; }
                return View("~/Views/Visa/OrderAdmin/ShowBaseInfo.cshtml", Qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult AddChildOrderInfo(int id, string OrderCode)
        {
            TpChildOrderModel model = new TpChildOrderModel();
            try
            {
                if (id > 0)
                {
                    //编辑
                    model = _childOrderBiz.GetTpChildOrderById(id);
                }
                else
                {
                    model.OrderCode = OrderCode;
                }

                TpTourBalanceModel master = orderService.GetMasterOrder(OrderCode);
                //if (master.VisaOrder.OrderStatus < 5)
                //{ ViewData["OrderStatus"] = "OK"; }
                //else
                //{ ViewData["OrderStatus"] = "NO"; }

                ViewData["DDLB"] = DictionaryTools.GetEnumsBy(Enums.ProductAllTypeEnum).ToSelectListFor();
                // 供应商
                ViewData["supplierList"] = _customerBiz.GetAllSupplier().ToSelectListFor(t => t.Code, t => t.Name);
                // 子产品列表
                ViewData["ProductList"] = _productBiz.GetProductByTeam(master.TeamId).ToSelectListFor(t => t.ProductID.ToString(), t => t.ProductName);

                return View("~/Views/Visa/OrderAdmin/AddChildOrderInfo.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 子订单保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveChildOrderInfo(TpChildOrderModel model)
        {
            if (model.Id != 0)
            {
                var entity = _childOrderBiz.GetTpChildOrderById(model.Id);
                var amount = entity.Amount;

                entity.ProductID = model.ProductID;
                entity.ProductName = model.ProductName;
                entity.ProductType = model.ProductType;
                entity.ModifiedTime = DateTime.Now;
                entity.SupplierCode = model.SupplierCode;
                entity.Remark = model.Remark;
                entity.UnitPrice = model.UnitPrice;
                entity.Amount = model.Amount;
                entity.Quantity = model.Quantity;

                orderService.OrderdetailUpdate(entity, UserInfo, WebToolKit.GetClientIp());

                // 更新订单金额
                if (amount != model.Amount)
                    orderService.ReCount(model.OrderCode);

                return "添加成功！";
            }
            else
            {
                model.CreatedTime = DateTime.Now;
                model.IsCancel = 1;
                orderService.OrderdetailAdd(model, UserInfo, WebToolKit.GetClientIp());
                // 更新订单金额
                orderService.ReCount(model.OrderCode);

                return "添加成功！";
            }
        }

        #endregion 订单基本信息和子订单

        /// <summary>
        ///   操作历史信息查询
        /// </summary>
        public ActionResult ShowHistoryInfo(string OrderCode)
        {
            try
            {
                IEnumerable<VisaOperationHistoryModel> Qmodel = orderService.SearchOrderHistoryList(OrderCode);
                return View(Qmodel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #region 签证资料确认核实

        public ActionResult UpdateVisaInfo(string val, string code)
        {
            try
            {
                orderService.SetModelStatus(val, code);
                return View("~/Views/Visa/OrderAdmin/VisaSubmitInfo.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult UpdateVisaCheckInfo(string val, string code)
        {
            try
            {
                orderService.SetModelStatus(val, code);
                return View("~/Views/Visa/OrderAdmin/VisaSubmitInfo.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public void UpdateFollowupDate(string OrderCode, DateTime? FollowupDate)
        {
            orderService.SaveFollowData(new VisaOrderModel { OrderCode = OrderCode, FollowupDate = FollowupDate });
            VisaOperationHistoryModel historyModel = new VisaOperationHistoryModel();
            historyModel.OrderCode = OrderCode;
            historyModel.OperateContent = "跟进日期更改为【" + FollowupDate + "】";
            historyModel.Ip = WebToolKit.GetClientIp();
            orderService.SaveOrderOperateHistory(historyModel, UserInfo);
        }

        #endregion 签证资料确认核实

        #region 取消订单和退款

        //取消订单
        public ActionResult ShowDetailInfo(int typeid)
        {
            try
            {
                return View("~/Views/Visa/OrderAdmin/ShowDetailInfo.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult OrderCancel(string OrderCode, decimal je)
        {
            try
            {
                ViewData["OrderCode"] = OrderCode;
                ViewData["Je"] = je;
                return View("~/Views/Visa/OrderAdmin/OrderCancel.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        //退款
        public ActionResult RequestRefund(string OrderCode, decimal je, int type)
        {
            try
            {
                ViewData["OrderCode"] = OrderCode;
                ViewData["Je"] = je;
                ViewData["types"] = type;
                return View("~/Views/Visa/OrderAdmin/RequestRefund.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 取消订单和退款

        public ActionResult ShowEcantractInfo(string OrderCode)
        {
            try
            {
                return View("~/Views/Visa/OrderAdmin/SHowEcantractInfo.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <returns></returns>
        public string SaveCancelOrder(string orderCode, string cancelType, string remark)
        {
            try
            {
                var master = orderService.GetMasterOrder(orderCode);
                var OrderModel = orderService.GetVisaOrderByCode(orderCode);

                if (OrderModel.TraceState != 8)
                {
                    //将订单置为已取消
                    if (master.YiShou > 0)
                    {
                        orderService.CancelOrder(orderCode, "202", "6");
                    }

                    //写入操作历史记录表
                    VisaOperationHistoryModel historyModel = new VisaOperationHistoryModel();
                    historyModel.OrderCode = OrderModel.OrderCode;
                    historyModel.OperateType = OrderModel.TraceState;
                    historyModel.OperateContent = "取消订单{" + cancelType + "" + remark + "}";
                    historyModel.Ip = WebToolKit.GetClientIp();
                    orderService.SaveOrderOperateHistory(historyModel, UserInfo);

                    if (master.YiShou > 0)
                    {
                        return "1";
                    }
                }
                else
                {
                    return "0";
                }
                return "3";
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return "-1";
            }
        }

        #endregion Action Method

        #region Private Method

        /// <summary>
        /// 获取基础数据
        /// </summary>
        private void GetBaseData()
        {
            ViewData["TeamList"] = _teamBiz.GetTeams("6", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName, "", "", "-选择部门-");
            ViewData["RepayTypeList"] = DictionaryTools.GetEnumsBy(Enums.PayTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择支付方式-");
            ViewData["OrderSourceList"] = DictionaryTools.GetEnumsBy(Enums.OrderSourceEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择订单来源-");
            ViewData["TraceStateList"] = DictionaryTools.GetEnumsBy(Enums.VisaOrderStatusEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择跟单状态-");
            ViewData["OrderStatusList"] = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择订单状态-");
            ViewData["RepayStatusList"] = DictionaryTools.GetEnumsBy(Enums.PayStatusEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择支付状态-");
            // ViewData["IsPaymentAppList"] = CCT.Web.Tools.Dictionary.FindDictionary("IsPaymentApplication"); 付款申请是否提交
        }

        #endregion Private Method

        #region 联系人信息

        /// <summary>
        /// 修改联系人信息
        /// </summary>
        /// <param name="model"></param>
        public void ModifyOrderContactInfo(TpTourBalanceModel model)
        {
            try
            {
                orderService.ModifyOrderContactInfo(model, UserInfo, WebToolKit.GetClientIp());
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 联系人信息

        #region 申请人

        /// <summary>
        /// 添加申请人（单签）初始化
        /// </summary>
        /// <param name="ordercode"></param>
        /// <returns></returns>
        public ActionResult AddTouristInfo(string ordercode)
        {
            try
            {
                ViewData["ordercode"] = ordercode;
                ViewData["List"] = orderService.GetApplicanterCategory(ordercode).ToSelectListFor(t => t.CategoryCode, t => t.CategoryName);
                return View("~/Views/Visa/OrderAdmin/AddTouristInfo.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 添加申请人（单签）
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        public void AddTouristInfo(VisaApplicanterModel model)
        {
            try
            {
                model.CreatedTime = DateTime.Now;
                model.MidifiedTime = DateTime.Now;
                model.IsValid = 1;
                model.Status = 1;
                touristBiz.AddTourist(model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 取消申请人（单签）
        /// </summary>
        /// <param name="id"></param>
        public void DeleteTouristInfo(int id)
        {
            try
            {
                touristBiz.DeleteTourist(id);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 修改（自助游，团队游）出游人信息初始化
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult EditTouristInfo(VisaApplicanterModel model)
        {
            try
            {
                var orderModel = orderService.GetVisaOrderByCode(model.OrderCode);
                model = touristBiz.GetTouristInfo(model.OrderCode, model.Id);
                if (model != null)
                {
                    ViewData["CardTypeList"] = DictionaryTools.GetEnumsBy(Enums.PassTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择-");
                    ViewData["CategoryList"] = GetCategoryDataSelectList(productService.GetCategroyList(orderModel.ProductCode));
                    ViewData["OrderModels"] = orderModel;
                    return View("~/Views/Visa/OrderAdmin/EditTouristInfo.cshtml", model);
                }
                return Content("<script type='text/javascript'>alert('操作失败，出行人数据不存在，请核实后再修改！！');</script>");
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 保存更改（自助游，团队游）出游人信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string UpdateTouristInfo(VisaApplicanterModel model)
        {
            try
            {
                var entity = touristBiz.GetTouristInfo(model.OrderCode, model.Id);
                if (null != entity)
                {
                    entity.Name = model.Name;
                    entity.Pinyin = OperateCommon.ConvertHanZiToPinYin(entity.Name);
                    entity.Sex = model.Sex;
                    entity.Birthday = model.Birthday;
                    entity.Phone = model.Phone;
                    entity.Categorycode = model.Categorycode;
                    entity.Status = model.Status;
                    entity.MidifiedTime = DateTime.Now;
                    entity.CardType = model.CardType;
                    entity.CardNo = model.CardNo;

                    touristBiz.UpdateTourist(entity);
                }

                VisaOperationHistoryModel historyModel = new VisaOperationHistoryModel();
                historyModel.OrderCode = model.OrderCode;
                historyModel.OperateContent = "修改出行人信息<br/>{材料分类："
                    + model.CategoryName + "(old:" + entity.Categorycode + " | new:" + model.Categorycode + ")"
                    + "，签证状态：" + model.VisaStateName + "(old:" + entity.Status + " | new:" + model.Status + ")"
                    + "}";
                if (entity.Categorycode != model.Categorycode)
                {
                    historyModel.OperateContent += "<br/>订单流转到【待材料收齐】";
                }
                historyModel.Ip = WebToolKit.GetClientIp();
                orderService.SaveOrderOperateHistory(historyModel, UserInfo);

                return "1";
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return "-1";
            }
        }

        #endregion 申请人

        #region 附件管理

        /// <summary>
        /// 上传缴款单凭证
        /// </summary>
        /// <returns></returns>
        public ActionResult AddUploadFile(TourFileModel vModel)
        {
            string filename = "";
            string fileExt = "";
            string FilePath = UploadTourFile(vModel.MasterOrderCode, "tourFileName", ref filename, ref fileExt);

            TourFileModel model = new TourFileModel();
            model.SourceType = vModel.SourceType;
            model.MasterOrderCode = vModel.MasterOrderCode;
            model.FileName = filename;
            model.FilePath = FilePath;
            model.CreatedTime = DateTime.Now;
            model.Remark = vModel.Remark;
            model.IsDel = 0;
            model.CreatedBy = GlobalContext.Current.UserInfo.Code;
            model.MediaType = WebToolKit.GetFileMedia(fileExt);
            _balanceBiz.AddTourFile(model);

            return Json(new { Code = 1, Message = "" });
        }

        public ActionResult JsonOrderInfo(string fileType, string orderCode)
        {
            var order = new OrderQModel();

            if (fileType == "1")
            {
                order.FileKeyList = (from d in touristBiz.GetTouristList(orderCode)
                                     select new Lvy.Models.KeyValueBean
                                     {
                                         Key = d.Id.ToString(),
                                         Value = d.Name
                                     }).ToList();


            }
            else if (fileType == "2")
            {
                order.FileKeyList = (from d in _payinBiz.GetPayInList(orderCode)
                                     select new Lvy.Models.KeyValueBean
                                     {
                                         Key = d.Id.ToString(),
                                         Value = "ID" + d.Id + ",金额:" + d.Amount
                                     }).ToList();
            }


            return Json(order, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReloadFile(string OrderCode)
        {
            var vModel = new OrderQModel();
            vModel.FileList = _balanceBiz.GetFileList(OrderCode);
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
            int row = _balanceBiz.DeleteTourFile(id);
            if (row > 0)
                return Json(new { Code = 1, Message = "" });
            else
                return Json(new { Code = 0, Message = "没发现文件." });
        }

        #endregion

        #region 订单详细

        /// <summary>
        /// 订单详细初始化
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderDetail(string OrderCode)
        {
            try
            {
                var master = orderService.GetMasterOrder(OrderCode);
                var orderMode = orderService.GetVisaOrderByCode(OrderCode);
                var model = new OrderQModel
                {
                    OrderCode = OrderCode,
                    MasterOrder = master,
                    OrderModel = orderMode,
                    ProductModel = productService.GetProductByCode(orderMode.ProductCode),
                    HistoryList = orderService.SearchOrderHistoryList(OrderCode),
                    TravellerList = touristBiz.GetTouristList(OrderCode),
                };

                return View("~/Views/Visa/OrderAdmin/OrderDetail.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 订单详细

        #region 订单确认

        /// <summary>
        /// 确认保存
        /// </summary>
        /// <param name="OrderCode"></param>
        public void ConfirmSaveVisaOrder(string OrderCode, DateTime? FollowupDate)
        {
            try
            {
                orderService.ConfirmSaveVisaOrder(OrderCode);
                orderService.OperationHistoryAdd(OrderCode, "【外呼确认保存】", UserInfo, WebToolKit.GetClientIp());

                var str = new StringBuilder();
                str.Append("<script type=\"text/javascript\" language=\"javascript\">");
                str.Append("    alert('订单已确认保存，订单流转｛待材料接收｝');");
                str.Append("    window.location.href='/FitVisaOrder/ModifyOrder?OrderCode=" + OrderCode + "'");
                str.Append("</script>");
                Response.Write(str);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 订单确认

        #region 支付信息

        /// <summary>
        /// 判断是否全额支付过了
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public string IsFullPayMent(string OrderCode)
        {
            try
            {
                if (orderService.IsFullPayMent(OrderCode))
                {
                    return "true";
                }
                return "false";
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return "false";
            }
        }

        /// <summary>
        /// 付款（临时用）
        /// </summary>
        /// <param name="OrderCode"></param>
        public void UpdatePayState(string OrderCode)
        {
            orderService.UpdatePayState(OrderCode);
        }

        #endregion 支付信息

        #region 审核材料

        /// <summary>
        /// 材料审核初始化页面
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult AuditOrderMaterials(string OrderCode)
        {
            try
            {
                var orderModel = orderService.GetMasterOrder(OrderCode);
                var model = new OrderQModel
                {
                    OrderCode = OrderCode,
                    MasterOrder = orderModel,
                    OrderModel = orderService.GetVisaOrderByCode(OrderCode),
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 保存审核的材料数据
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public void SaveAuditOrderMaterials(OrderQModel model)
        {
            try
            {
                orderService.SaveAuditOrderMaterials(model);

                var str = new StringBuilder();
                str.Append("<script type=\"text/javascript\">");
                str.Append("    var api = frameElement.api, W = api.opener;");
                str.Append("    alert('保存成功！！');");
                str.Append("    api.close();");
                str.Append("    W.window.location.reload();");
                //str.Append("    api.reload();");
                //str.Append("    window.location.href='/FitVisaOrder/ModifyOrder?OrderCode=" + model.OrderCode + "'");
                str.Append("</script>");
                Response.Write(str);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion 审核材料

        #region 重新加载订单处理页面

        /// <summary>
        /// 重新加载订单页面
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult ReloadOrderDetails(string OrderCode, bool IsModifyPage)
        {
            try
            {
                var master = orderService.GetMasterOrder(OrderCode);
                var orderModel = orderService.GetVisaOrderByCode(OrderCode);
                var model = new OrderQModel
                {
                    OrderCode = OrderCode,
                    MasterOrder = master,
                    OrderModel = orderModel,
                    ProductModel = productService.GetProductByCode(orderModel.ProductCode),
                    HistoryList = orderService.SearchOrderHistoryList(OrderCode),
                    TravellerList = touristBiz.GetTouristList(OrderCode),
                    ChildOrderList = orderService.SearchChildOrderList(OrderCode)
                };

                return View("~/Views/FitVisaOrder/OrderPartial.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return Content(ex.Message);
            }
        }

        #endregion 重新加载订单处理页面

        #region order methods

        /// <summary>
        /// 订单操作层
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderFlowLayer()
        {
            return View("~/Views/Visa/OrderAdmin/OrderFlowLayer.cshtml");
        }

        /// <summary>
        /// 订单签证材料初始化页面
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult VisaMaterial(string OrderCode)
        {
            try
            {
                var orderModel = orderService.GetMasterOrder(OrderCode);
                var model = new OrderQModel
                {
                    OrderCode = OrderCode,
                    MasterOrder = orderModel,
                    OrderModel = orderService.GetVisaOrderByCode(OrderCode),
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        #endregion order methods

        #region Private Method

        /// <summary>
        /// 获取产品分类SelectListItem列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private IList<SelectListItem> GetCategoryDataSelectList(IList<VisaCategoryModel> list)
        {
            List<SelectListItem> lists = new List<SelectListItem>
            {
                new SelectListItem { Text = "-- 请选择 --", Value = "" }
            };
            foreach (var temp in list)
            {
                SelectListItem sli = new SelectListItem
                {
                    Text = temp.CategoryName,
                    Value = temp.CategoryCode
                };
                lists.Add(sli);
            }
            return lists;
        }

        #endregion Private Method
    }
}