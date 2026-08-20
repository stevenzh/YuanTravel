using Arch.Common.Utils;
using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.VModels.Finance;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Finance
{
    /// <summary>
    /// 收款 (老系统迁移过来，无其他关联)
    /// </summary>
    public class ProceedsController : BaseController
    {
        private VTProceedsBiz proceedsService = new VTProceedsBiz();
        private readonly TeamBiz _biz = new TeamBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();

        private int pageSize = 20;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ProceedsController));

        /// <summary>
        /// 收款管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(ProceedsSearchQModel qmodel)
        {
            try
            {
                ViewBag.Teams = _biz.GetTeamsList(GlobalContext.Current.UserInfo.OwnerCode).ToSelectListFor(k => k.TeamID, v => v.TeamName);

                qmodel.ProceedsPageList.PageSize = pageSize;
                qmodel.ProceedsPageList = proceedsService.GetQueryPayIns(qmodel);
            }
            catch (Exception err)
            {
                logger.Error("", err);
            }
            return View(qmodel);
        }

        /// <summary>
        /// 缴款单列表
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="pagedIndex"></param>
        /// <returns></returns>
        public ActionResult CollectionQueryList(ProceedsSearchQModel searchModel, int pagedIndex)
        {
            searchModel.ProceedsPageList.PageIndex = pagedIndex;
            searchModel.ProceedsPageList.PageSize = this.pageSize;

            ProceedsSearchQModel qmodel = new ProceedsSearchQModel();
            qmodel.ProceedsPageList = proceedsService.GetQueryPayIns(searchModel);

            return View(qmodel);
        }

        /// <summary>
        /// 添加缴款单初始
        /// </summary>
        /// <returns></returns>
        public ActionResult GoCollection()
        {
            ViewBag.CollectionType = DictionaryTools.GetEnumsBy(Enums.PayTypeEnum).ToSelectListFor(k => k.Value, v => v.Value);
            ViewBag.Teams = _biz.GetTeamsList(GlobalContext.Current.UserInfo.OwnerCode).ToSelectListFor(k => k.TeamID, v => v.TeamName);
            //  ViewBag.CustomerList = _accountBiz.GetAllAccount(GlobalContext.Current.UserInfo.OwnerCode).ToSelectListFor(t => t.Code, t => t.Name);

            return View();
        }

        /// <summary>
        /// 保存新加缴款单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCollectionInfo(VTProceedsModel model)
        {
            string result = string.Empty;
            try
            {
                var name = GlobalContext.Current.UserInfo.Name;
                model.CheckId = GlobalContext.Current.UserInfo.Code;
                model.CheckName = GlobalContext.Current.UserInfo.Name;
                model.ProceedsCode = DBTools.GetSeqNo("VT_Proceeds", "JK", "");
                model.ProceedsDate = DateTime.Now;
                model.IsReceive = 0;                //未处理
                model.IsValid = 1;                   //无效
                model.ChargerDept = model.ChargerDept;

                proceedsService.AddCollection(model);
                result = "1" + "|" + model.ProceedsCode;
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                result = "0";
            }

            return Content(result);
        }

        [HttpPost]
        public ActionResult UpdateCollectionInfo(VTProceedsModel model)
        {
            string result = string.Empty;
            try
            {
                model.CheckId = GlobalContext.Current.UserInfo.Code;
                model.CheckName = GlobalContext.Current.UserInfo.Name;
                model.IsReceive = 1;
                bool status = this.proceedsService.UpdatePaymentStatu(model.Id);
                var jObj = new
                {
                    Status = "1",
                    ProceedsRefundCode = model.ProceedsCode,
                    CheckName = model.CheckName,
                };
                return Json(jObj);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { Status = "0" });
            }
        }

        public ActionResult GetCollectionDetail(string orderCode)
        {
            VTProceedsModel model = proceedsService.GetPayInModel(orderCode);
            return View(model);
        }

        public ActionResult CollectionPrint(string collectionCode)
        {
            var model = this.proceedsService.GetPayInModel(collectionCode);
            return View(model);
        }

        public ActionResult CollectionConfirm(string collectionCode)
        {
            var model = this.proceedsService.GetPayInModel(collectionCode);
            return View(model);
        }

        public ActionResult ExportExcel(ProceedsSearchQModel model)
        {
            try
            {
                List<VTProceedsModel> list = proceedsService.GetQueryProceeds(model);
                var table = new StringBuilder();

                foreach (var proceed in list)
                {
                    table.Append("<Row>\r\n");
                    table.Append("\t\t<Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + proceed.ProceedsCode + "</Data></Cell>\r\n");
                    table.Append("\t\t<Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + proceed.ReceiveSum + "</Data></Cell>\r\n");
                    table.Append("\t\t<Cell ss:StyleID=\"s73\"><Data ss:Type=\"DateTime\">" + string.Format("{0:s}", proceed.ProceedsDate) + "</Data></Cell>\r\n");
                    table.Append("\t\t<Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + proceed.ChargerHost + "</Data></Cell>\r\n");
                    table.Append("\t\t<Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + proceed.ChargerDept + "</Data></Cell>\r\n");
                    table.Append("\t\t<Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + proceed.ChargerName + "</Data></Cell>\r\n");
                    table.Append("\t\t<Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + proceed.CheckName + "</Data></Cell>\r\n");
                    table.Append("</Row>\r\n");
                }

                var strb = new StringBuilder();
                strb.Append(System.IO.File.ReadAllText(Server.MapPath("~/XMLDocument/Proceeds.xml")));
                strb.Replace("${TableCollection}", table.ToString());

                string fileName = "缴款单" + string.Format("{0:yyyyMMddHHmmssfff}", DateTime.Now) + ".xml";
                ResponseExcel(fileName, strb);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
            }
            return Content("Success");
        }

        public static void ResponseExcel(string fileName, StringBuilder strb)
        {
            HttpContext curContext = System.Web.HttpContext.Current;
            curContext.Response.Clear();
            curContext.Response.Buffer = true;
            curContext.Response.Charset = "UTF-8";
            curContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            curContext.Response.ContentEncoding = Encoding.GetEncoding("UTF-8");
            curContext.Response.ContentType = "application/ms-excel";
            curContext.Response.Write(strb.ToString());
            curContext.Response.End();
        }
    }
}