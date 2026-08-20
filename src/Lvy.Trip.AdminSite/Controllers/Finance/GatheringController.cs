using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.VModels.OpTour;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Finance
{
    /// <summary>
    /// 缴款单流程收款
    ///
    /// </summary>
    public class GatheringController : BaseController
    {
        private CustomerBiz _customerBiz = new CustomerBiz();
        private TpOrderPayInBiz _biz = new TpOrderPayInBiz();
        private OrderBiz _orderBiz = new OrderBiz();
        private TeamBiz _teamBiz = new TeamBiz();
        private TourBalanceBiz _balanceBiz = new TourBalanceBiz();

        // GET: Gathering
        public ActionResult Search(GatheringVModel vModel)
        {
            InitPage();

            vModel.OwnerCode = UserInfo.OwnerCode;
            // 当前用户
            var teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 9).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            if (string.IsNullOrEmpty(vModel.Condition.FrTeamId) && teams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值
            {
                vModel.Condition.FrTeamId = teams.Where(t => t.Value != "").FirstOrDefault().Value;
            }
            ViewBag.FrTeamBeans = teams;

            if (!string.IsNullOrEmpty(vModel.JieSuanState))
            {
                vModel.Condition.JieSuanState = vModel.JieSuanState;
            }
            vModel.TourPayInList = _biz.GetPagedList(vModel);
            vModel.TotalModel = _biz.GetFinanceSummary(vModel);

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);

            return View(vModel);
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected void InitPage()
        {
            // 产品部门
            ViewBag.ProductTeams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            ViewBag.IsTourOks = new List<KeyValueBean>
                                    {
                                        new KeyValueBean{Key ="1",Value = "已成团"},
                                        new KeyValueBean{Key = "0",Value = "未成团"}
                                    }.ToSelectListFor();
            //订单状态分类
            ViewBag.OrderStates = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor();
            //结算状态
            ViewBag.SettlementStateBean = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key = "1",Value = "已结算"},
                                         new KeyValueBean{Key="0",Value="未结算"}
                                     }.ToSelectListFor();

            ViewBag.OrderSource = DictionaryTools.GetEnumsBy(Enums.TourSourceEnum).ToSelectListFor();
            //所有订单状态
            ViewBag.AllOrderStates = DictionaryTools.GetEnumsBy(Enums.OrderStateEnum).ToSelectListFor();
            // 销售部门
            ViewBag.SalesTeams = _teamBiz.GetBalanceTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            //销售
            ViewBag.Salers = _customerBiz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
        }

        /// <summary>
        /// 详情页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult PayInDetail(int id, string orderCode)
        {
            TpOrderPayInModel model = _biz.GetOrderPayInModelById(id);
            if (model.Type == 1)  // 旅游线路
            {
                var order = _orderBiz.GetOrderByOrderCode(orderCode);
                var files = _orderBiz.GetOrderFileList(orderCode);
                model.TourNo = order.TourNo;
                model.ProductName = order.LineName;
                if (model.BillFileId != 0)
                    model.BillFileUrl = files.Where(t => t.Id == model.BillFileId).FirstOrDefault().FilePath;
                if (model.BankFileId != 0)
                    model.BankFileUrl = files.Where(t => t.Id == model.BankFileId).FirstOrDefault().FilePath;
            }
            else
            {
                var tour = _balanceBiz.GetBalanceByOrderCode(model.OrderCode);
                var files = _balanceBiz.GetFileList(model.OrderCode);
                model.TourNo = tour.TourNo;
                model.ProductName = tour.ProductName;
                if (model.BillFileId != 0)
                    model.BillFileUrl = files.Where(t => t.Id == model.BillFileId).FirstOrDefault().FilePath;
                if (model.BankFileId != 0)
                    model.BankFileUrl = files.Where(t => t.Id == model.BankFileId).FirstOrDefault().FilePath;
            }

            return View(model);
        }

        /// <summary>
        /// 收款确认
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ActionResult SingleOrderShowKuanCheck(int payInId)
        {
            var vModel = _biz.GetOrderPayInModelById(payInId);
            if (vModel.State < 20)
                return Content("true");
            return Content("false");
        }

        /// <summary>
        /// 缴款单缴费确认的方法
        /// </summary>
        /// <param name="payInId"></param>
        /// <returns></returns>
        public ActionResult PayInToConfirmed(int payInId)
        {
            try
            {
                var vModel = _biz.GetById(payInId);
                if (vModel.Type == 1)  // 旅游线路
                {
                    _orderBiz.ConfirmPay(vModel, GlobalContext.Current.UserInfo.Code);
                }
                else
                {
                    _balanceBiz.ConfirmPay(vModel, GlobalContext.Current.UserInfo.Code);
                }
                _balanceBiz.UpdateBalanceAmount(vModel.Type, vModel.OrderCode);

                return Json(new { Code = "200", Result = 1 });
            }
            catch (Exception)
            {
                return Json(new { Code = "200", Result = -1 });
            }
        }
    }
}