using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Crm
{
    public class SupplierController : CustomerController
    {
        private ILog logger = LogManager.GetLogger("SupplierController");

        private readonly CustomerBiz _biz = new CustomerBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();

        [LvyAuth]
        public override ActionResult Search(CustomerVModel vModel)
        {
            // 取得查询分页条件
            var q = (CustomerVModel)CacheContext.Current.Get(Consts.PageSupplierController + GlobalContext.Current.UserInfo.Code);
            if (q != null && vModel.FirstTime)
                vModel = q;

            ViewBag.PaymentTypes = DictionaryTools.GetEnumsBy(Enums.PaymentTypeEnum).ToSelectListFor();

            // 仅显示供应商
            vModel.CustomerType = "2";

            // 保存查询分页条件
            CacheContext.Current.Add(Consts.PageSupplierController + GlobalContext.Current.UserInfo.Code, vModel, Consts.OutputCacheDuration2);

            vModel.Customers = _biz.GetPagedList(vModel, UserInfo);
            vModel.FirstTime = false;
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        // GET: Supplier/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Supplier/Create
        public override ActionResult Create()
        {
            CrmCustomerModel model = new CrmCustomerModel
            {
                IsDistributors = false,
                IsSupplier = true,
                IsBranch = false,
                ChannelType = 1,
                PaymentType = 1,
                IsGroupTour = false,
                RebateInBill = true,
                HasChild = true,
            };
            InitPage();

            return View(model);
        }

        // GET: Supplier/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Supplier/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

    }
}