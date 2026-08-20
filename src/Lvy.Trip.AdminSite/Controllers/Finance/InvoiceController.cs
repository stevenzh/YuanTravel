using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.VModels.Finance;
using Lvy.Web.Common;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Finance
{
    /// <summary>
    /// 发票管理
    /// </summary>
    public partial class InvoiceController : BaseController
    {
        private readonly InvoiceBiz _biz = new InvoiceBiz();
        private readonly FinanceBiz _financeBiz = new FinanceBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly OrderBiz _orderBiz = new OrderBiz();

        /// <summary>
        /// 查询付款列表
        /// </summary>
        /// <param name="invoiceVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Search(InvoiceVModel invoiceVModel)
        {
            InitPage();
            invoiceVModel.OwnerCode = UserInfo.OwnerCode;

            //获取订单列表信息
            invoiceVModel.InvoicePageList = _biz.GetPageList(invoiceVModel);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", invoiceVModel);
            return View(invoiceVModel);
        }

        /// <summary>
        /// 编辑成本跳转-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(int id)
        {
            var model = _biz.GetInvoiceById(id);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = _biz.GetInvoiceById(id);
            var order = _orderBiz.GetOrderLineTourist(model.OrderCode);
            if (order.Line.LineScope == 4)
            {
                // 出境
                ViewBag.InvoiceTitleEnum = DictionaryTools.GetEnumsBy(Enums.OutboundInvoiceTitleEnum).ToSelectListFor(k => k.Value, v => v.Value);
            }
            else
            {
                ViewBag.InvoiceTitleEnum = DictionaryTools.GetEnumsBy(Enums.InboundInvoiceTitleEnum).ToSelectListFor(k => k.Value, v => v.Value);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(TpInvoiceModel model)
        {
            _biz.Update(model);
            return Json(new { code = 0, message = "success" });
        }

        public ActionResult CheckInvoice(TpInvoiceModel model)
        {
            model.CheckedBy = GlobalContext.Current.UserInfo.Code;
            _biz.CheckInvoice(model);
            return Content("success");
        }

        public ActionResult SetValid(int id)
        {
            _biz.SetValid(id, 0);
            return Json(new { code = 0, message = "success" });
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected override void InitPage()
        {
            // 销售部门
            ViewBag.SalesTeams = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            //发票状态
            ViewBag.InvoiceStateBean = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key="1",Value="已开"},
                                         new KeyValueBean{Key="0",Value="申请"}
                                     }.ToSelectListFor();
        }
    }
}