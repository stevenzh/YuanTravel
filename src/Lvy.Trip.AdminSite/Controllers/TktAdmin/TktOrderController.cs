using Arch.Common;
using Arch.Common.Models;
using Common.Logging;
using Lvy.Models.TourDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Ticket;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.TktManage
{
    /// <summary>
    /// 订单管理 | 任务单管理
    /// </summary>
    public class TktOrderController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(TktOrderController));
        private readonly TktOrderBiz _biz = new TktOrderBiz();
        private readonly TktTaskOrderBiz taskBiz = new TktTaskOrderBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TouristBiz touristBiz = new TouristBiz();
        private readonly TpOrderPayInBiz _payinBiz = new TpOrderPayInBiz();
        private readonly TourBalanceBiz _balanceBiz = new TourBalanceBiz();

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Search(TktOrderVModel vModel)
        {
            ViewBag.OrderStateBean = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor();
            vModel.OwnerCode = GlobalContext.Current.OwnerCode;
            // 获得数据
            vModel.Orders = _biz.GetPagedOrders(vModel, true, UserInfo);
            _biz.StatisticTktOrderSupplier(vModel, UserInfo);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);

            return View(vModel);
        }

        /// <summary>
        /// 编辑订单
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult EditOrder(string orderCode)
        {
            BookingVModel vModel = _biz.GetEditOrderModel(orderCode);
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);
            vModel.FileList = _balanceBiz.GetFileList(orderCode);

            ViewData["Teams"] = _teamBiz.GetSalesTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, t => t.TeamName, "", "", "--请选择部门--");
            if (string.IsNullOrEmpty(vModel.Order.SalesTeamId))
            {
                ViewData["Salers"] = _customerBiz.GetTeamSales(UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }
            else
            {
                ViewData["Salers"] = _customerBiz.GetTeamUsersByTeamId(vModel.Order.SalesTeamId, UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }
            ViewData["FileEnum"] = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();

            return View("EditOrder", vModel);
        }

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult UpdateOrder(BookingVModel vModel)
        {
            vModel.Order.ModifiedBy = UserInfo.Code;
            string[] result = _biz.UpdateOrder(vModel, UserInfo);

            if (result[0] == "0")
                return Json(new { State = 0, Msg = result[1] });
            return Json(new { State = 1, OrderCode = result[1], ReturnUrl = "/TktOrder/Search" });
        }

        public ActionResult GetCurrentPrices(string selectDate, string productId)
        {
            var vModel = new TktPriceListVModel
            {
                Product = new TktProductBiz().GetById(productId)
            };

            if (vModel.Product.PriceMode == 1)
                vModel.PriceList = _biz.GetCurrentPrices(productId);
            else
                vModel.PriceList = _biz.GetCurrentPrices(selectDate, productId);

            if (vModel.PriceList.Count > 0)
                return PartialView("UCPriceList", vModel);
            else
                return Content("<div style=\"color: red;\" align=\"center\">没有该天的报价！</div>");
        }

        public ActionResult Details(string orderCode)
        {
            var vModel = _biz.GetConfirmOrder(orderCode);
            return View(vModel);
        }

        /// <summary>
        /// 确认新订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Confirm(string orderId)
        {
            var model = _biz.GetOrderByCode(orderId);
            model.OrderState = 2;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            var flag = _biz.UpdateOrderInfo(model);
            return Content(flag.ToString());
        }

        /// <summary>
        /// 假删除订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Delete(string orderId)
        {
            var flag = _biz.DeleteOrderInfo(orderId, UserInfo);
            return Content(flag.ToString());
        }

        #region 游客维护

        public ActionResult AddTouristInfo(string orderCode)
        {
            VisaApplicanterModel model = new VisaApplicanterModel();
            model.OrderCode = orderCode;
            return View(model);
        }

        [HttpPost]
        public void AddTourist(VisaApplicanterModel model)
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

        public ActionResult ReLoadTourist(string orderCode)
        {
            var vModel = new BookingVModel();
            vModel.TravellerList = touristBiz.GetTouristList(orderCode);
            return PartialView("TouristList", vModel);
        }

        public ActionResult EditTouristInfo(VisaApplicanterModel model)
        {
            try
            {
                model = touristBiz.GetTouristInfo(model.OrderCode, model.Id);
                if (model != null)
                {
                    ViewData["CardTypeList"] = DictionaryTools.GetEnumsBy(Enums.PassTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择-");
                    return View(model);
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


                //VisaOperationHistoryModel historyModel = new VisaOperationHistoryModel();
                //historyModel.OrderCode = model.OrderCode;
                //historyModel.OperateContent = "修改出行人信息<br/>{材料分类："
                //    + model.CategoryName + "(old:" + entity.Categorycode + " | new:" + model.Categorycode + ")"
                //    + "，签证状态：" + model.VisaStateName + "(old:" + entity.Status + " | new:" + model.Status + ")"
                //    + "}";
                //if (entity.Categorycode != model.Categorycode)
                //{
                //    historyModel.OperateContent += "<br/>订单流转到【待材料收齐】";
                //}
                //orderService.SaveOrderOperateHistory(historyModel);

                return "1";
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return "-1";
            }
        }

        #endregion 游客维护

        #region 缴款记录

        public ActionResult ReLoadPayIn(string orderCode)
        {
            var vModel = new BookingVModel();
            vModel.PayInList = _payinBiz.GetPayInList(orderCode);
            return PartialView("UCPayIn", vModel);
        }
        #endregion

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
            var order = new BookingVModel();

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
            var vModel = new BookingVModel();
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

        #region 收款（暂时保留）
        /// <summary>
        /// 收款弹出层
        /// </summary>
        /// <returns></returns>
        public ActionResult ShouKuanDialog(string orderId)
        {
            var order = _biz.GetOrder(orderId);

            return PartialView("UCShouKuanDialog", order);
        }

        /// <summary>
        /// 收款弹出层
        /// </summary>
        /// <returns></returns>
        public ActionResult ShouKuanDialogMulti(string orderIds)
        {
            var orders = _biz.GetById(orderIds);

            return PartialView("UCShouKuanDialogMulti", orders);
        }

        /// <summary>
        /// 收款确认
        /// </summary>
        /// <returns></returns>
        public ActionResult ShouKuan(string orderId)
        {
            var model = _biz.GetOrder(orderId);
            model.PaymentStatus = 5;
            model.YiShou = model.YingShou;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            var flag = _biz.UpdateOrderInfo(model);
            return Content(flag.ToString());
        }

        /// <summary>
        /// 收款确认
        /// </summary>
        /// <returns></returns>
        public ActionResult ShouKuanMulti(string orderIds)
        {
            var models = _biz.GetById(orderIds);
            foreach (var model in models)
            {
                model.PaymentStatus = 5;
                model.YiShou = model.YingShou;
                model.ModifiedBy = UserInfo.Code;
                model.ModifiedTime = DateTime.Now;
            }
            var flag = _biz.UpdateOrderInfo(models);
            return Content(flag.ToString());
        }
        #endregion

        #region 任务单

        /// <summary>
        /// 编辑任务单
        /// </summary>
        /// <returns></returns>
        public ActionResult EditTaskOrder(string orderCode)
        {
            var vModel = taskBiz.GetTaskModel(orderCode, UserInfo.OwnerCode);
            return View(vModel);
        }

        /// <summary>
        /// 添加任务单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult AddTaskOrder(EditTaskOrderVModel vModel)
        {
            var orderCode = vModel.TaskOrder.MasterOrderCode;
            vModel.TaskOrder.ModifiedBy = UserInfo.Code;
            var result = new TktTaskOrderBiz().AddTaskOrder(vModel);
            return Json(new { TaskOrderId = result, OrderCode = orderCode }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新任务单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult UpdateTaskOrder(EditTaskOrderVModel vModel)
        {
            var taskOrderId = vModel.TaskOrder.ID;
            var orderCode = vModel.TaskOrder.MasterOrderCode;
            vModel.TaskOrder.ModifiedBy = UserInfo.Code;
            var result = taskBiz.UpdateTaskOrder(vModel);
            return Json(new { TaskOrderId = taskOrderId, OrderCode = orderCode }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 打印任务单
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public ActionResult PrintTaskOrder(int taskId = 0)
        {
            var vModel = taskBiz.GetPrintTaskModel(taskId, UserInfo, GlobalContext.Current.OwnerInfo);
            return View(vModel);
        }

        #endregion

        /// <summary>
        /// 下载门票账单（产品分组）
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public FileContentResult DownloadTicketInfo(TktOrderVModel vModel)
        {
            var start = vModel.DateRange.Replace("/", "");
            var fileName = HttpUtility.UrlEncode("门票账单-产品分组(") + start + ").xls";
            var details = _biz.BillGroupByProduct(vModel, UserInfo);
            foreach (var d in details)
            {
                var customer = DictionaryTools.GetCachedCustomer(d.BookingCustomer);
                d.FastCode = customer.FastCode;
                d.BookingCustomer = customer.Name;
            }
            var result = from p in details orderby p.ProductName, p.FastCode, p.OutDate select p;

            HSSFWorkbook workbook = new HSSFWorkbook();
            IFont font = workbook.CreateFont();
            font.FontHeightInPoints = 10;
            font.FontName = "微软雅黑";
            ICellStyle styleTitleLeft = workbook.CreateCellStyle();  //左上
            styleTitleLeft.SetFont(font);
            styleTitleLeft.BorderBottom = BorderStyle.Medium;
            styleTitleLeft.BorderLeft = BorderStyle.Medium;
            styleTitleLeft.BorderRight = BorderStyle.Thin;
            styleTitleLeft.BorderTop = BorderStyle.Medium;
            ICellStyle styleTitleMiddle = workbook.CreateCellStyle();  //上
            styleTitleMiddle.SetFont(font);
            styleTitleMiddle.BorderBottom = BorderStyle.Medium;
            styleTitleMiddle.BorderLeft = BorderStyle.Thin;
            styleTitleMiddle.BorderRight = BorderStyle.Thin;
            styleTitleMiddle.BorderTop = BorderStyle.Medium;
            ICellStyle styleTitleRight = workbook.CreateCellStyle();  //右上
            styleTitleRight.SetFont(font);
            styleTitleRight.BorderBottom = BorderStyle.Medium;
            styleTitleRight.BorderLeft = BorderStyle.Thin;
            styleTitleRight.BorderRight = BorderStyle.Medium;
            styleTitleRight.BorderTop = BorderStyle.Medium;
            ICellStyle styleContentLeft = workbook.CreateCellStyle();  //左
            styleContentLeft.SetFont(font);
            styleContentLeft.BorderBottom = BorderStyle.Thin;
            styleContentLeft.BorderLeft = BorderStyle.Medium;
            styleContentLeft.BorderRight = BorderStyle.Thin;
            styleContentLeft.BorderTop = BorderStyle.Thin;
            ICellStyle styleContentMiddle = workbook.CreateCellStyle();  //中
            styleContentMiddle.SetFont(font);
            styleContentMiddle.BorderBottom = BorderStyle.Thin;
            styleContentMiddle.BorderLeft = BorderStyle.Thin;
            styleContentMiddle.BorderRight = BorderStyle.Thin;
            styleContentMiddle.BorderTop = BorderStyle.Thin;
            ICellStyle styleContentRight = workbook.CreateCellStyle();  //右
            styleContentRight.SetFont(font);
            styleContentRight.BorderBottom = BorderStyle.Thin;
            styleContentRight.BorderLeft = BorderStyle.Thin;
            styleContentRight.BorderRight = BorderStyle.Medium;
            styleContentRight.BorderTop = BorderStyle.Thin;
            ICellStyle styleContentrBottom = workbook.CreateCellStyle();  //下
            styleContentrBottom.SetFont(font);
            styleContentrBottom.BorderTop = BorderStyle.Medium;

            ICellStyle styleProductName = workbook.CreateCellStyle();
            IFont fontProductName = workbook.CreateFont();
            fontProductName.FontHeightInPoints = 12;
            fontProductName.FontName = "微软雅黑";
            fontProductName.IsBold = true;
            styleProductName.SetFont(fontProductName);
            using (MemoryStream ms = new MemoryStream())
            {
                // 新增試算表。
                ISheet sheet = workbook.CreateSheet("门票账单样式 - 产品分组");
                if (details.Count > 0)
                {
                    sheet.SetColumnWidth(0, 1 * 256);
                    sheet.SetColumnWidth(1, 40 * 256);
                    sheet.SetColumnWidth(2, 12 * 256);
                    sheet.SetColumnWidth(4, 12 * 256);
                    var productNameTemp = string.Empty;
                    var rowIndex = 0;
                    var beginRowIndex = 0;
                    foreach (var d in details)
                    {
                        if (d.ProductName != productNameTemp)
                        {
                            if (rowIndex > 0)
                            {
                                //统计行
                                var endRowIndex = rowIndex;
                                IRow totalRow = sheet.CreateRow(rowIndex++);
                                totalRow.HeightInPoints = 18;
                                ICell celltotal1 = totalRow.CreateCell(1);
                                celltotal1.CellStyle = styleContentrBottom;
                                ICell celltotal2 = totalRow.CreateCell(2);
                                celltotal2.CellStyle = styleContentrBottom;
                                ICell celltotal3 = totalRow.CreateCell(3);
                                celltotal3.CellStyle = styleContentrBottom;
                                celltotal3.SetCellFormula("SUM(D" + (beginRowIndex + 1) + ":D" + endRowIndex + ")");
                                ICell celltotal4 = totalRow.CreateCell(4);
                                celltotal4.CellStyle = styleContentrBottom;
                                ICell celltotal5 = totalRow.CreateCell(5);
                                celltotal5.CellStyle = styleContentrBottom;
                                ICell celltotal6 = totalRow.CreateCell(6);
                                celltotal6.CellStyle = styleContentrBottom;
                                ICell celltotal7 = totalRow.CreateCell(7);
                                celltotal7.CellStyle = styleContentrBottom;
                                //间隔行
                                IRow blankRow = sheet.CreateRow(rowIndex++);
                                blankRow.HeightInPoints = 18;
                            }

                            //产品名称行
                            productNameTemp = d.ProductName;
                            IRow rowName = sheet.CreateRow(rowIndex);
                            rowName.Height = 18 * 20;
                            ICell cellProductName = rowName.CreateCell(1);
                            cellProductName.CellStyle = styleProductName;
                            cellProductName.SetCellValue("产品名称：" + d.ProductName);
                            sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 7));
                            rowIndex++;
                            //标题行
                            IRow rowTitle = sheet.CreateRow(rowIndex);
                            rowTitle.HeightInPoints = 18;
                            ICell cellTitle1 = rowTitle.CreateCell(1);
                            cellTitle1.CellStyle = styleTitleLeft;
                            cellTitle1.SetCellValue("分销商");
                            ICell cellTitle2 = rowTitle.CreateCell(2);
                            cellTitle2.CellStyle = styleTitleMiddle;
                            cellTitle2.SetCellValue("日期");
                            ICell cellTitle3 = rowTitle.CreateCell(3);
                            cellTitle3.CellStyle = styleTitleMiddle;
                            cellTitle3.SetCellValue("人数");
                            ICell cellTitle4 = rowTitle.CreateCell(4);
                            cellTitle4.CellStyle = styleTitleMiddle;
                            cellTitle4.SetCellValue("报价类型");
                            ICell cellTitle5 = rowTitle.CreateCell(5);
                            cellTitle5.CellStyle = styleTitleMiddle;
                            cellTitle5.SetCellValue("签单");
                            ICell cellTitle6 = rowTitle.CreateCell(6);
                            cellTitle6.CellStyle = styleTitleMiddle;
                            cellTitle6.SetCellValue("返佣");
                            ICell cellTitle7 = rowTitle.CreateCell(7);
                            cellTitle7.CellStyle = styleTitleRight;
                            cellTitle7.SetCellValue("备注");
                            rowIndex++;
                            beginRowIndex = rowIndex; //当遇到新产品名称，保存起始行索引
                        }
                        IRow row = sheet.CreateRow(rowIndex);
                        row.HeightInPoints = 18;
                        ICell cell1 = row.CreateCell(1);
                        cell1.CellStyle = styleContentLeft;
                        cell1.SetCellValue(d.BookingCustomer);
                        ICell cell2 = row.CreateCell(2);
                        cell2.CellStyle = styleContentMiddle;
                        cell2.SetCellValue(d.OutDate.ToDateFormat());
                        ICell cell3 = row.CreateCell(3);
                        cell3.CellStyle = styleContentMiddle;
                        cell3.SetCellValue(d.PeopleNum);
                        ICell cell4 = row.CreateCell(4);
                        cell4.CellStyle = styleContentMiddle;
                        cell4.SetCellValue(DictionaryTools.GetEnumValue(Enums.TktTypeEnum, d.TktType.ToString()));
                        ICell cell5 = row.CreateCell(5);
                        cell5.CellStyle = styleContentMiddle;
                        cell5.SetCellValue((d.TktType == 1 || d.TktType == 2) ? d.SysPrice.ToString() : null);
                        ICell cell6 = row.CreateCell(6);
                        cell6.CellStyle = styleContentMiddle;
                        cell6.SetCellValue((d.TktType == 3 || d.TktType == 4) ? d.SysPrice.ToString() : null);
                        ICell cell7 = row.CreateCell(7);
                        cell7.CellStyle = styleContentRight;
                        rowIndex++;
                    }
                    //最末统计行
                    IRow lastTotalRow = sheet.CreateRow(rowIndex);
                    lastTotalRow.HeightInPoints = 18;
                    ICell lastCelltotal1 = lastTotalRow.CreateCell(1);
                    lastCelltotal1.CellStyle = styleContentrBottom;
                    ICell lastCelltotal2 = lastTotalRow.CreateCell(2);
                    lastCelltotal2.CellStyle = styleContentrBottom;
                    ICell lastCelltotal3 = lastTotalRow.CreateCell(3);
                    lastCelltotal3.CellStyle = styleContentrBottom;
                    lastCelltotal3.SetCellFormula("SUM(D" + (beginRowIndex + 1) + ":D" + rowIndex + ")");
                    ICell lastCelltotal4 = lastTotalRow.CreateCell(4);
                    lastCelltotal4.CellStyle = styleContentrBottom;
                    ICell lastCelltotal5 = lastTotalRow.CreateCell(5);
                    lastCelltotal5.CellStyle = styleContentrBottom;
                    ICell lastCelltotal6 = lastTotalRow.CreateCell(6);
                    lastCelltotal6.CellStyle = styleContentrBottom;
                    ICell lastCelltotal7 = lastTotalRow.CreateCell(7);
                    lastCelltotal7.CellStyle = styleContentrBottom;
                }
                workbook.Write(ms);
                //Response.AddHeader("Content-Disposition", string.Format("attachment; filename=EmptyWorkbook.xls"));
                //Response.BinaryWrite(ms.ToArray());
                return File(ms.GetBuffer(), "application/vnd.ms-excel", fileName);
            }
        }

        /// <summary>
        /// 下载门票账单（分销商分组）
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public FileContentResult ReportCustomerGroup(TktOrderVModel vModel)
        {
            #region 获取数据

            var start = vModel.DateRange.Replace("/", "");
            var fileName = HttpUtility.UrlEncode("门票账单-分销商分组(") + start + ").xls";
            var details = _biz.BillGroupByCustomer(vModel, UserInfo);
            if (details == null || details.Count == 0)
                return null;

            foreach (var d in details)
            {
                var customer = DictionaryTools.GetCachedCustomer(d.BookingCustomer);
                d.FastCode = customer.FastCode;
                d.BookingCustomerName = customer.Name;
            }
            var result = from p in details orderby p.BookingCustomerName, p.FastCode, p.OutDate select p;

            #endregion 获取数据

            #region 创建工作簿 定义样式

            var workbook = new HSSFWorkbook();
            var commonFont = workbook.CreateFont();          //默认字体
            commonFont.FontHeightInPoints = 10;
            commonFont.FontName = "微软雅黑";
            var headFont = workbook.CreateFont();            //字体（分销商）
            headFont.FontHeightInPoints = 12;
            headFont.FontName = "微软雅黑";
            headFont.IsBold = true;
            var styleTitleLeft = workbook.CreateCellStyle();        //样式 左：粗边框，上：粗边框，右：细边框，下：粗边框
            styleTitleLeft.SetFont(commonFont);
            styleTitleLeft.BorderBottom = BorderStyle.Medium;
            styleTitleLeft.BorderLeft = BorderStyle.Medium;
            styleTitleLeft.BorderRight = BorderStyle.Thin;
            styleTitleLeft.BorderTop = BorderStyle.Medium;
            var styleTitleMiddle = workbook.CreateCellStyle();      //样式 左：细边框，上：粗边框，右：细边框，下：粗边框
            styleTitleMiddle.SetFont(commonFont);
            styleTitleMiddle.BorderBottom = BorderStyle.Medium;
            styleTitleMiddle.BorderLeft = BorderStyle.Thin;
            styleTitleMiddle.BorderRight = BorderStyle.Thin;
            styleTitleMiddle.BorderTop = BorderStyle.Medium;
            var styleTitleRight = workbook.CreateCellStyle();       //样式 左：细边框，上：粗边框，右：粗边框，下：粗边框
            styleTitleRight.SetFont(commonFont);
            styleTitleRight.BorderBottom = BorderStyle.Medium;
            styleTitleRight.BorderLeft = BorderStyle.Thin;
            styleTitleRight.BorderRight = BorderStyle.Medium;
            styleTitleRight.BorderTop = BorderStyle.Medium;
            var styleContentLeft = workbook.CreateCellStyle();      //样式 左：粗边框，上：细边框，右：细边框，下：细边框
            styleContentLeft.SetFont(commonFont);
            styleContentLeft.BorderBottom = BorderStyle.Thin;
            styleContentLeft.BorderLeft = BorderStyle.Medium;
            styleContentLeft.BorderRight = BorderStyle.Thin;
            styleContentLeft.BorderTop = BorderStyle.Thin;
            var styleContentMiddle = workbook.CreateCellStyle();    //样式 左：细边框，上：细边框，右：细边框，下：细边框
            styleContentMiddle.SetFont(commonFont);
            styleContentMiddle.BorderBottom = BorderStyle.Thin;
            styleContentMiddle.BorderLeft = BorderStyle.Thin;
            styleContentMiddle.BorderRight = BorderStyle.Thin;
            styleContentMiddle.BorderTop = BorderStyle.Thin;
            var styleContentRight = workbook.CreateCellStyle();     //样式 左：细边框，上：细边框，右：粗边框，下：细边框
            styleContentRight.SetFont(commonFont);
            styleContentRight.BorderBottom = BorderStyle.Thin;
            styleContentRight.BorderLeft = BorderStyle.Thin;
            styleContentRight.BorderRight = BorderStyle.Medium;
            styleContentRight.BorderTop = BorderStyle.Thin;
            var styleContentrBottom = workbook.CreateCellStyle();   //样式 左：无，上：粗边框，右：无，下：无
            styleContentrBottom.SetFont(commonFont);
            styleContentrBottom.BorderTop = BorderStyle.Medium;
            var styleCustomerName = workbook.CreateCellStyle();      //样式 左：无，上：无，右：无，下：无
            styleCustomerName.SetFont(headFont);

            #endregion 创建工作簿 定义样式

            using (var ms = new MemoryStream())
            {
                // 新增試算表。
                var sheet = workbook.CreateSheet("门票账单样式 - 产品分组");
                sheet.SetColumnWidth(0, 1 * 256);   //首列置空，列宽为1
                sheet.SetColumnWidth(1, 40 * 256);  //景点名称列
                sheet.SetColumnWidth(2, 12 * 256);  //日期列
                sheet.SetColumnWidth(4, 12 * 256);  //报价类型列
                sheet.SetColumnWidth(7, 23 * 256);  //导游列
                sheet.SetColumnWidth(8, 23 * 256);  //分销商联系人列

                var tempCustomerName = string.Empty;//客户名称（临时变量）
                var rowIndex = 0;                   //行索引（NPOI从0开始计）
                var sumBeginRow = 0;                //sum统计起始行（Excel中行号）
                foreach (var d in details)
                {
                    if (d.BookingCustomerName != tempCustomerName)
                    {
                        //当rowIndex大于0，则已遍历至新客户，需要为上一客户添加统计行，并空一行。
                        if (rowIndex > 0)
                        {
                            #region 统计行

                            var sumEndRow = rowIndex;   //Excel中行从1开始计，故当前行（统计行）的索引（NPOI）为上一客户Excel中的末行行号
                            var sumRow = sheet.CreateRow(rowIndex++);
                            sumRow.HeightInPoints = 18;
                            var sumCell1 = sumRow.CreateCell(1);
                            sumCell1.CellStyle = styleContentrBottom;
                            var sumCell2 = sumRow.CreateCell(2);
                            sumCell2.CellStyle = styleContentrBottom;
                            var sumCell3 = sumRow.CreateCell(3);
                            sumCell3.CellStyle = styleContentrBottom;
                            sumCell3.SetCellFormula("SUM(D" + sumBeginRow + ":D" + sumEndRow + ")");
                            var sumCell4 = sumRow.CreateCell(4);
                            sumCell4.CellStyle = styleContentrBottom;
                            var sumCell5 = sumRow.CreateCell(5);
                            sumCell5.CellStyle = styleContentrBottom;
                            var sumCell6 = sumRow.CreateCell(6);
                            sumCell6.CellStyle = styleContentrBottom;
                            var sumCell7 = sumRow.CreateCell(7);
                            sumCell7.CellStyle = styleContentrBottom;
                            var sumCell8 = sumRow.CreateCell(8);
                            sumCell8.CellStyle = styleContentrBottom;
                            var sumCell9 = sumRow.CreateCell(9);
                            sumCell9.CellStyle = styleContentrBottom;

                            #endregion 统计行

                            //间隔行
                            var blankRow = sheet.CreateRow(rowIndex++);
                        }
                        tempCustomerName = d.BookingCustomerName;

                        //客户名称行
                        var customerNameRow = sheet.CreateRow(rowIndex);
                        customerNameRow.HeightInPoints = 18;
                        var customerNameCell = customerNameRow.CreateCell(1);
                        customerNameCell.CellStyle = styleCustomerName;
                        customerNameCell.SetCellValue("分销商：（" + d.FastCode + "）" + d.BookingCustomerName);
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 9));    //合并单元格
                        rowIndex++;

                        #region 标题行

                        var titleRow = sheet.CreateRow(rowIndex++);

                        titleRow.HeightInPoints = 18;
                        var cellTitle1 = titleRow.CreateCell(1);
                        cellTitle1.CellStyle = styleTitleLeft;
                        cellTitle1.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle1.SetCellValue("景点名称");
                        var cellTitle2 = titleRow.CreateCell(2);
                        cellTitle2.CellStyle = styleTitleMiddle;
                        cellTitle2.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle2.SetCellValue("日期");
                        var cellTitle3 = titleRow.CreateCell(3);
                        cellTitle3.CellStyle = styleTitleMiddle;
                        cellTitle3.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle3.SetCellValue("人数");
                        var cellTitle4 = titleRow.CreateCell(4);
                        cellTitle4.CellStyle = styleTitleMiddle;
                        cellTitle4.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle4.SetCellValue("报价类型");
                        var cellTitle5 = titleRow.CreateCell(5);
                        cellTitle5.CellStyle = styleTitleMiddle;
                        cellTitle5.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle5.SetCellValue("签单");
                        var cellTitle6 = titleRow.CreateCell(6);
                        cellTitle6.CellStyle = styleTitleMiddle;
                        cellTitle6.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle6.SetCellValue("返佣");
                        var cellTitle7 = titleRow.CreateCell(7);
                        cellTitle7.CellStyle = styleTitleMiddle;
                        cellTitle7.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle7.SetCellValue("导游");
                        var cellTitle8 = titleRow.CreateCell(8);
                        cellTitle8.CellStyle = styleTitleMiddle;
                        cellTitle8.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle8.SetCellValue("分销商联系人");
                        var cellTitle9 = titleRow.CreateCell(9);
                        cellTitle9.CellStyle = styleTitleRight;
                        cellTitle9.CellStyle.Alignment = HorizontalAlignment.Center;
                        cellTitle9.SetCellValue("备注");
                        sumBeginRow = rowIndex + 1;     //当遇到新客户名称，保存起始行行号（NPOI索引+1）

                        #endregion 标题行
                    }

                    #region 内容行

                    IRow row = sheet.CreateRow(rowIndex++);
                    row.HeightInPoints = 18;
                    ICell cell1 = row.CreateCell(1);
                    cell1.CellStyle = styleContentLeft;
                    cell1.SetCellValue(d.ProductName);
                    ICell cell2 = row.CreateCell(2);
                    cell2.CellStyle = styleContentMiddle;
                    cell2.SetCellValue(d.OutDate.ToDateFormat());
                    ICell cell3 = row.CreateCell(3);
                    cell3.CellStyle = styleContentMiddle;
                    cell3.SetCellValue(d.PeopleNum);
                    ICell cell4 = row.CreateCell(4);
                    cell4.CellStyle = styleContentMiddle;
                    //cell4.SetCellValue(DictionaryTools.GetEnumValue(Enums.TktTypeEnum, d.TktType.ToString()));
                    cell4.SetCellValue(d.PriceType);
                    ICell cell5 = row.CreateCell(5);
                    cell5.CellStyle = styleContentMiddle;
                    cell5.SetCellValue((d.TktType == 1 || d.TktType == 2) ? d.SysPrice.ToString() : null);
                    ICell cell6 = row.CreateCell(6);
                    cell6.CellStyle = styleContentMiddle;
                    cell6.SetCellValue((d.TktType == 3 || d.TktType == 4) ? d.SysPrice.ToString() : null);
                    ICell cell7 = row.CreateCell(7);
                    cell7.CellStyle = styleContentMiddle;
                    cell7.SetCellValue(d.GuideName + "(" + d.GuidePhone + ")");
                    ICell cell8 = row.CreateCell(8);
                    cell8.CellStyle = styleContentMiddle;
                    cell8.SetCellValue(d.Managers + "(" + d.ManagerPhone + ")");
                    ICell cell9 = row.CreateCell(9);
                    cell9.CellStyle = styleContentRight;

                    #endregion 内容行
                }

                #region 最末统计行

                IRow lastTotalRow = sheet.CreateRow(rowIndex);
                lastTotalRow.HeightInPoints = 18;
                ICell lastCelltotal1 = lastTotalRow.CreateCell(1);
                lastCelltotal1.CellStyle = styleContentrBottom;
                ICell lastCelltotal2 = lastTotalRow.CreateCell(2);
                lastCelltotal2.CellStyle = styleContentrBottom;
                ICell lastCelltotal3 = lastTotalRow.CreateCell(3);
                lastCelltotal3.CellStyle = styleContentrBottom;
                lastCelltotal3.SetCellFormula("SUM(D" + sumBeginRow + ":D" + rowIndex + ")");
                ICell lastCelltotal4 = lastTotalRow.CreateCell(4);
                lastCelltotal4.CellStyle = styleContentrBottom;
                ICell lastCelltotal5 = lastTotalRow.CreateCell(5);
                lastCelltotal5.CellStyle = styleContentrBottom;
                ICell lastCelltotal6 = lastTotalRow.CreateCell(6);
                lastCelltotal6.CellStyle = styleContentrBottom;
                ICell lastCelltotal7 = lastTotalRow.CreateCell(7);
                lastCelltotal7.CellStyle = styleContentrBottom;
                ICell lastCelltotal8 = lastTotalRow.CreateCell(8);
                lastCelltotal8.CellStyle = styleContentrBottom;
                ICell lastCelltotal9 = lastTotalRow.CreateCell(9);
                lastCelltotal9.CellStyle = styleContentrBottom;

                #endregion 最末统计行

                workbook.Write(ms);

                return File(ms.GetBuffer(), "application/vnd.ms-excel", fileName);
            }
        }
    }
}