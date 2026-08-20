using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Models.TourDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Booking;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Finance;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Common;
using Lvy.VModels.Base;
using Lvy.VModels.Booking;
using Lvy.VModels.Excel;
using Lvy.VModels.Op;
using Lvy.VModels.OpTour;
using Lvy.VModels.Product;
using Lvy.VModels.Tour;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XWPF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TourVModel = Lvy.VModels.Op.TourVModel;

namespace Lvy.Trip.AdminSite.Controllers.Op
{
    /// <summary>
    ///
    /// </summary>
    public class OpTourController : BaseController
    {
        #region 变量

        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly GuideBiz _guideBiz = new GuideBiz();
        private readonly TravellerBiz _travellerBiz = new TravellerBiz();
        private readonly TpTourPlanBiz _biz = new TpTourPlanBiz();
        private readonly OrderBiz _orderBiz = new OrderBiz();
        private readonly TpLineBusPointBiz _lineBusPointBiz = new TpLineBusPointBiz();
        private readonly BookingBiz _bookingBiz = new BookingBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly CustomerBiz _customerBiz = new CustomerBiz();
        private readonly TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private readonly TpLineBiz _lineBiz = new TpLineBiz();
        private readonly TpPriceBiz _priceBiz = new TpPriceBiz();
        private readonly TourBalanceBiz _balanceBiz = new TourBalanceBiz();

        private static readonly ILog logger = LogManager.GetLogger(typeof(OpTourController));

        #endregion 变量

        #region 团单管理

        /// <summary>
        /// 团订单信息-视图
        /// </summary>
        /// <param name="searchTourVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchTour(TourVModel searchTourVModel)
        {
            InitPage();
            //页面第一此加载时设置条件初始值
            //if (!Request.IsAjaxRequest())
            //{
            //    searchTourVModel.Condition.MinOutDate = DateTime.Now.AddMonths(-1).ToDateFormat();
            //    searchTourVModel.Condition.MaxOutDate = DateTime.Now.ToDateFormat();
            //}

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            var teams = new List<SelectListItem>();
            if (GlobalContext.Current.IsSysAdmin || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                teams = _teamBiz.GetOpTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else
            {
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(searchTourVModel.Condition.CrmTeamId) && teams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值
                {
                    searchTourVModel.Condition.CrmTeamId = teams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }
            ViewBag.AccountTeamBeans = teams;

            //根据条件获取团单列表信息
            searchTourVModel.TourList = _biz.GetTourList(searchTourVModel, UserInfo);

            if (Request.IsAjaxRequest())
                return PartialView("UCTourList", searchTourVModel);
            return View("SearchTour", searchTourVModel);
        }

        /// <summary>
        ///成团反转（成团->未成团，未成团->成团）
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult IsToured(int tourId)
        {
            var tpTourPlanModel = new TpTourPlanModel();
            //根据团Id获取团单对象
            tpTourPlanModel = _biz.GetTourById(tourId);
            if (tpTourPlanModel.AuditState > 1) // 团单制作中 后的状态，不能取消成团。
                return Content("audited");

            tpTourPlanModel.AuditState = tpTourPlanModel.AuditState == 1 ? 0 : 1; //0：未成团；1：已成团

            int result = _biz.UpdateTourPlan(tpTourPlanModel);
            return Content(result.ToString());
        }

        /// <summary>
        /// 补订单  前台下单保留
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>w
        [LvyAuth]
        public ActionResult AddOrder(int tourId)
        {
            BookingVModel vModel = new BookingVModel();
            vModel.Tour = _bookingBiz.GetTourById(tourId);
            vModel.LineModel = vModel.Tour.Line;
            vModel.Quota = _quotaBiz.GetQuotaByTour(tourId);
            //vModel.OutDateBeans = _bookingBiz.GetOutDateBeansByLineId(vModel.Tour.LineId);
            vModel.PriceModels = _priceBiz.GetValidPrices(tourId); //_bookingBiz.GetPricesByTourId(tourId);
            vModel.BusPoints = _bookingBiz.GetBusPoints(vModel.Tour.LineId);
            vModel.OrderSourceBean = DictionaryTools.GetEnumsBy(Enums.TourSourceEnum);
            //ViewBag.PassTypes = DictionaryTools.GetEnumsBy(Enums.PassTypeEnum).ToSelectListFor();
            ViewBag.SalesOfTeam = _teamBiz.GetSalesTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            ViewBag.Salers = _customerBiz.GetTeamSales(GlobalContext.Current.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            return View(vModel);
        }

        /// <summary>
        /// OP补单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveOrder(BookingVModel vModel)
        {
            string orderCode = "";
            vModel.OrderState = 2;
            vModel.TraceState = 40;
            OrderResultState code = _bookingBiz.BookingTrans(ref orderCode, vModel, GlobalContext.Current.UserInfo);
            // 更新占位
            _orderBiz.FreeQuota(vModel.TourId, orderCode, GlobalContext.Current.UserInfo.Code);

            return Json(new { StateCode = ((int)code).ToString(), OrderCode = orderCode });
        }

        /// <summary>
        /// 编辑游客名单
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult EditTourists(int tourId)
        {
            var vModel = new EditTouristsVModel
            {
                Tour = _biz.GetTourById(tourId),
                Quota = _quotaBiz.GetQuotaByTour(tourId),
                Line = _lineBiz.GetLineByTour(tourId),
                Tourists = _travellerBiz.GetByTourId(tourId),
                DestinationList = DictionaryTools.GetEnumsBy(Enums.OutCityEnum)
            };
            return View(vModel);
        }

        #region 核对游客信息

        /// <summary>
        /// 核对游客资料
        /// </summary>
        /// <param name="tourId">团号</param>
        /// <param name="tralId">游客ID</param>
        /// <returns></returns>
        public ActionResult CheckTourists(int tourId, int? tralId)
        {
            var vModel = new CheckTouristsVModel();
            var plan = _biz.GetTourByIds(tourId);
            var line = _lineBiz.GetLineById(plan.LineId);
            vModel.PassportExpiry = plan.OutDate.AddDays(180 + line.TravelDays);

            var list = _travellerBiz.GetByTourId(tourId);
            if (tralId != null)
            {
                vModel.TouristsInfo = list.FirstOrDefault(s => s.Id == tralId);
            }
            else
            {
                vModel.TouristsInfo = list.FirstOrDefault();
            }
            int id = vModel.TouristsInfo != null ? (int)vModel.TouristsInfo.Id : 0;
            vModel.TouristsId = id;
            vModel.TouristsFile = _travellerBiz.GetTouristsFileList(id);
            return View(vModel);
        }

        /// <summary>
        /// 保存游客资料
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckTourists(CheckTouristsVModel vModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var model = _travellerBiz.GetById(vModel.TouristsId);
                    model.Name = vModel.TouristsInfo.Name;
                    model.PinYin = vModel.TouristsInfo.PinYin;
                    model.Sex = vModel.TouristsInfo.Sex;
                    model.DateOfBirth = vModel.TouristsInfo.DateOfBirth;
                    model.PlaceOfBirth = vModel.TouristsInfo.PlaceOfBirth;
                    model.Phone = vModel.TouristsInfo.Phone;
                    model.PassType = vModel.TouristsInfo.PassType;
                    model.PassNo = vModel.TouristsInfo.PassNo;
                    model.DateOfExpiry = vModel.TouristsInfo.DateOfExpiry;
                    model.IsChecked = vModel.TouristsInfo.IsChecked;
                    if (vModel.TouristsInfo.PassType != 1)
                    {
                        model.DateOfIssue = vModel.TouristsInfo.DateOfIssue;
                        model.PlaceOfIssue = vModel.TouristsInfo.PlaceOfIssue;
                    }
                    bool result = _travellerBiz.Update(model) > 0;
                    if (result)
                    {
                        return Json(new { code = 200, message = "保存成功" });
                    }
                }
                return Json(new { code = 200, message = "保存失败" });
            }
            catch (Exception)
            {
                return Json(new { code = 200, message = "操作异常" });
            }
        }

        [HttpGet]
        public ActionResult GetPreTourists(int tourId, int tralId)
        {
            try
            {
                var vModel = new CheckTouristsVModel();
                vModel.TouristsInfo = _travellerBiz.GetPreByTourId(tralId, tourId);
                if (vModel.TouristsInfo != null)
                {
                    vModel.TouristsFile = _travellerBiz.GetTouristsFileList((int)vModel.TouristsInfo.Id);
                    vModel.TouristsId = (int)vModel.TouristsInfo.Id;
                }
                return Json(new { code = 200, data = vModel, message = "获取数据成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, message = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetNextTourists(int tourId, int tralId)
        {
            try
            {
                var vModel = new CheckTouristsVModel();
                vModel.TouristsInfo = _travellerBiz.GetNextByTourId(tralId, tourId);
                if (vModel.TouristsInfo != null)
                {
                    vModel.TouristsFile = _travellerBiz.GetTouristsFileList((int)vModel.TouristsInfo.Id);
                    vModel.TouristsId = (int)vModel.TouristsInfo.Id;
                }
                return Json(new { code = 200, data = vModel, message = "获取数据成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, message = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 保存游客名单
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SaveTourists(EditTouristsVModel vModel)
        {
            OrderResultState state = new TpTourPlanBiz().UpdataTourists(vModel);
            return Json(new { StateCode = ((int)state).ToString(), TourId = vModel.Tour.Id });
        }

        #endregion 核对游客信息

        /// <summary>
        /// 验证座位
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="seatNum"></param>
        /// <returns></returns>
        public string CheckSeatNum(int tourId, string seatNum)
        {
            var seatModel = new TpBusSeatBiz().GetBusSeatByTour(tourId);
            var seatNumList = seatModel.SeatModels;
            var seat = seatNumList.FirstOrDefault(p => p.No == seatNum);
            if (seat != null)
            {
                if (seat.State == 2)
                {
                    return ((int)OrderResultState.Code101).ToString();
                }
                else if (seat.State == 3)
                {
                    return ((int)OrderResultState.Code102).ToString();
                }
                else
                {
                    return ((int)OrderResultState.Code100).ToString();
                }
            }
            return ((int)OrderResultState.Code110).ToString();
        }

        /// <summary>
        /// 重新计算团库存
        /// 如果是汽车班，重算座位号
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult ReCalcTourQuota(int tourId)
        {
            _orderBiz.FreeQuota(tourId, "", GlobalContext.Current.UserInfo.Code);

            _biz.ReCalcTourQuota(tourId);
            return RedirectToAction("EditTourists", new { tourId = tourId });
        }

        #endregion 团单管理

        #region 单团核算

        /// <summary>
        /// 团OP审核通过（提交到财务）
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        [LvyAuth]
        public string AuditBalance(int tourId)
        {
            var tour = _biz.GetTourById(tourId);
            if (tour.AuditState == 0) // 未成团不能审核
                return "1";

            var tourBalance = _balanceBiz.GetBalanceByTourId(tourId);  //获取单团
            tour.AuditState = 3;
            tourBalance.OPAuditBy = GlobalContext.Current.UserInfo.Code;
            tourBalance.OPAuditTime = DateTime.Now;
            _balanceBiz.UpdateBalance(tourBalance);
            _biz.UpdateTourPlan(tour);
            return "2";
        }

        /// <summary>
        /// 单团核算
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult ShowTourBalance(int tourId, int isCaiWu)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            vModel.IsCaiWu = isCaiWu;
            UpdateTourBalance(tourId, vModel);
            string ownerCode = GlobalContext.Current.OwnerCode;
            ViewBag.Suppliers = new CustomerBiz().GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name,
                Help1 = DictionaryTools.GetEnumValue(Enums.PaymentTypeEnum, a.PaymentType.ToString()),
                Help2 = a.PaymentType.ToString()
            });
            return View(vModel);
        }

        /// <summary>
        /// 添加一条成本
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult AddRowCost(int rowIndex, string tourId)
        {
            ViewBag.RowIndex = rowIndex;
            TpTourCostModel vModel = new TpTourCostModel();
            vModel.MasterOrderCode = tourId;

            //var plan = _tourPlanBiz.GetTourAndLine(tourId);
            string ownerCode = GlobalContext.Current.OwnerCode;

            ViewBag.Suppliers = _customerBiz.GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name,
                Help1 = DictionaryTools.GetEnumValue(Enums.PaymentTypeEnum, a.PaymentType.ToString()),
                Help2 = a.PaymentType.ToString()
            }).ToList();
            return PartialView("TourBalance/UCRowCost", vModel);
        }

        /// <summary>
        /// 保存单团核算
        /// </summary>
        public ActionResult SaveTourBalance(TourBalanceVModel vModel)
        {
            var tourId = vModel.Balance.TourId.Value;
            var masterOrderCode = vModel.Balance.MasterOrderCode;
            var plan = _biz.GetTourById(tourId);
            if (vModel.IsCopy == false && plan.AuditState > 2) // OP提交财务后，不允许修改单团
                return AlertResult("OP提交财务，不允许修改单团!");

            var orders = _orderBiz.GetValidOrderByTourId(tourId); // 获取有效订单
            var tourBalance = _balanceBiz.GetBalanceByTourId(tourId, vModel.IsCopy);  //获取单团
            tourBalance.YiShou = orders.Sum(a => a.TolPaid);
            tourBalance.TotalCost = vModel.CostList.Sum(t => t.ItemCost);
            tourBalance.MaoLi = tourBalance.YingShou - tourBalance.TotalCost;
            _balanceBiz.UpdateBalance(tourBalance);

            // 更新成本
            var Costs = _balanceBiz.GetCostsByOrderCode(tourBalance.MasterOrderCode, vModel.IsCopy);
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
                _balanceBiz.UpdateCost(costRule);
            }

            // 添加新的
            foreach (var d in vModel.CostList.Where(t => t.Id == default(int)))
            {
                TpTourCostModel tourCost = new TpTourCostModel();
                tourCost.Code = DBTools.GetSeqNo("TourCost");
                tourCost.MasterOrderCode = masterOrderCode;
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
                tourCost.IsCopy = vModel.IsCopy;
                _balanceBiz.SaveCost(tourCost);
            }
            return SaveResult("1", Request.UrlReferrer.PathAndQuery);
        }

        /// <summary>
        /// 取得单团核算信息 填充MODEL
        /// 数据库为空，更新数据库
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="vModel"></param>
        private void UpdateTourBalance(int tourId, TourBalanceVModel vModel)
        {
            //var quota = new TpTourQuotaMapBiz().GetMapWithQuota(tourId);
            var orders = _orderBiz.GetValidCommonOrderByTourId(tourId); // 获取有效订单
            var tourBalance = _balanceBiz.GetBalanceByTourId(tourId);  //获取单团

            // vmodel对象复制
            vModel.Tour = _biz.GetTourById(tourId);
            vModel.Line = _lineBiz.GetLineById(vModel.Tour.LineId);
            vModel.Orders = orders;
            var leader = _orderBiz.GetLeaderOfTour(tourId);

            if (tourBalance == null)   //添加单团
            {
                var model = new TpTourBalanceModel();
                model.MasterOrderCode = DBTools.GetSeqNo("TourBalance");
                model.TourId = tourId;
                model.TeamId = vModel.Line.TeamID;
                model.LineId = vModel.Tour.LineId;
                model.ProductName = vModel.Line.LineName;
                model.Type = 1;
                model.IsPackage = 1;
                model.ProductType = 1;
                model.OutDate = vModel.Tour.OutDate;
                model.GuideName = "";   // 刚成团还没安排领队
                model.Num = orders.Sum(a => a.TravellerCount);  // 还没安排领队
                model.YingShou = orders.Sum(a => a.TolYsPrice);
                model.YiShou = orders.Sum(a => a.TolPaid);
                model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                model.ModifiedTime = DateTime.Now;
                model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.CreatedTime = DateTime.Now;
                model.IsCopy = false;
                model.OrderSource = 2;
                model.IsneedInvoice = 0;
                model.ContractType = 1;
                model.PaymentStatus = 1;
                model.OwnerCode = UserInfo.OwnerCode;
                model.TourNo = vModel.Tour.TourNo;

                // costs
                TpLineCostRuleBiz costRuleBiz = new TpLineCostRuleBiz();
                var costRules = costRuleBiz.GetByLineId(vModel.Tour.LineId);

                var tourCosts = new List<TpTourCostModel>();
                TpTourCostModel tourCost = null;
                foreach (var costRule in costRules)
                {
                    tourCost = new TpTourCostModel();
                    tourCost.MasterOrderCode = vModel.MasterOrderCode;
                    tourCost.SupplierId = costRule.SupplierId;
                    tourCost.Item = costRule.Item;
                    tourCost.Cost = costRule.Cost;
                    tourCost.Num = 1;
                    tourCost.ItemCost = 0;
                    tourCost.Remark = costRule.Remark;
                    tourCost.IsValid = 1;
                    tourCost.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                    tourCost.ModifiedTime = DateTime.Now;
                    tourCost.IsCopy = false;
                    tourCosts.Add(tourCost);
                }
                vModel.CostList = tourCosts;
                model.TotalCost = vModel.CostList.Sum(t => t.ItemCost);
                model.MaoLi = model.YingShou - model.TotalCost;

                vModel.Balance = model;

                //添加单团和成本
                _balanceBiz.SaveBalance(vModel);
            }
            else    // 更新单团
            {
                var model = _balanceBiz.GetBalanceByTourId(tourId);
                //model.TourId = tourId;
                //model.LineId = vModel.TourModel.LineId;
                if (leader.Count() > 0)
                    model.GuideName = leader.FirstOrDefault().Name;
                model.Num = orders.Sum(a => a.TravellerCount) + leader.Count();
                model.YingShou = orders.Sum(a => a.TolYsPrice);
                model.YiShou = orders.Sum(a => a.TolPaid);
                // 成本列表
                vModel.CostList = _balanceBiz.GetCostsByOrderCode(tourBalance.MasterOrderCode);
                model.TotalCost = vModel.CostList.Sum(t => t.ItemCost);
                model.MaoLi = model.YingShou - model.TotalCost;
                model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                model.ModifiedTime = DateTime.Now;
                //update balance
                _balanceBiz.UpdateBalance(model);
                // get view model
                vModel.Balance = model;
            }

            // sum
            vModel.SumCost = new FinanceTotalModel();
            vModel.SumCost.XianShou = vModel.CostModels.Where(a => a.PaymentType == 1).Sum(a => a.ItemCost);
            vModel.SumCost.Qiandan = vModel.CostModels.Where(a => a.PaymentType != 1).Sum(a => a.ItemCost);
            vModel.SumCost.SumTolCost = vModel.SumCost.XianShou + vModel.SumCost.Qiandan;
        }

        private TourBalanceVModel GetTourBalance(int tourId)
        {
            TourBalanceVModel vModel = new TourBalanceVModel();
            var quota = new TpTourQuotaMapBiz().GetMapWithQuota(tourId);
            var orders = _orderBiz.GetValidCommonOrderByTourId(tourId); // 获取有效订单

            // vmodel对象复制
            vModel.Tour = _biz.GetTourById(tourId);
            vModel.Line = _lineBiz.GetLineById(vModel.Tour.LineId);
            vModel.Orders = orders;
            var model = _balanceBiz.GetBalanceByTourId(tourId);
            vModel.CostList = _balanceBiz.GetCostsByOrderCode(model.MasterOrderCode);

            model.TourId = tourId;
            model.LineId = vModel.Tour.LineId;
            model.GuideName = "";
            model.Num = quota.Quota.UsedQuota;
            model.YingShou = orders.Sum(a => a.TolYsPrice);
            model.YiShou = orders.Sum(a => a.TolPaid);
            model.TotalCost = vModel.CostList.Sum(t => t.ItemCost);
            model.MaoLi = model.YingShou - model.TotalCost;
            model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            model.ModifiedTime = DateTime.Now;

            vModel.Balance = model;

            // sum
            vModel.SumCost = new FinanceTotalModel();
            vModel.SumCost.XianShou = vModel.CostModels.Where(a => a.PaymentType == 1).Sum(a => a.ItemCost);
            vModel.SumCost.Qiandan = vModel.CostModels.Where(a => a.PaymentType != 1).Sum(a => a.ItemCost);
            vModel.SumCost.SumTolCost = vModel.SumCost.XianShou + vModel.SumCost.Qiandan;

            return vModel;
        }

        public ActionResult PrintTourBalance(int tourId)
        {
            TourBalanceVModel qModel = GetTourBalance(tourId);
            TempData["OrderConfirmPrintVModel"] = qModel;
            return View("/Views/OpTour/TourBalance/PrintTourBalance.aspx", qModel);
        }

        #endregion 单团核算

        #region 导出接送单

        /// <summary>
        /// 根据线路类型和出发日期导出相应的接送单
        /// </summary>
        /// <param name="searchTourVModel"></param>
        /// <returns></returns>
        public ActionResult ExportJSExcel(TourVModel searchTourVModel)
        {
            var orderModels = new List<TpOrderModel>();
            var lineModels = new List<TpLineModel>();
            var lineBusPointModels = new List<TpLineBusPointModel>();
            var outDate = searchTourVModel.OutDate;
            int strCnt = 0;
            string lineTypeIds = "";
            //根据线路类型获取对应的线路信息
            if (!searchTourVModel.SelectedLineTypeIds.IsNullOrEmpty())
            {
                strCnt = searchTourVModel.SelectedLineTypeIds.Length - 1;
                lineTypeIds = searchTourVModel.SelectedLineTypeIds.Substring(0, strCnt);
            }
            lineModels = _lineBiz.GetIdsByLineTypes(lineTypeIds, UserInfo);
            int lineModelsCnt = lineModels.Count();
            string lineIds = "";
            int strLineIdsCnt = 0;
            if (lineModels == null || lineModelsCnt < 1)
            {
                string html = "<script> alert('无数据，请重新选择线路类型！'); window.location.href = \"/OpTour/SearchTour\" </script>";
                return Content(html);
            }
            if (lineModels != null && lineModelsCnt > 0)
            {
                foreach (var lineModel in lineModels)
                {
                    lineIds = lineIds + lineModel.LineId + ",";
                }
                strLineIdsCnt = lineIds.Length - 1;
                lineIds = lineIds.Substring(0, strLineIdsCnt);
            }

            //1.根据出团日期和线路类型获取已确认、已结算、新订单的信息列表
            orderModels = _orderBiz.GetByOutDate(outDate, lineIds, UserInfo);

            if (orderModels == null || orderModels.Count < 1)
            {
                string html = "<script> alert('无数据，请重新选择出发日期！'); window.location.href = \"/OpTour/SearchTour\" </script>";
                return Content(html);
            }
            if (orderModels.Count > 0)
            {
                var workBook = new HSSFWorkbook();
                var ms = new MemoryStream();
                //创建工作簿
                var sheet1 = workBook.CreateSheet("接送单");
                try
                {
                    #region 单元格格式设置

                    #region 格式一:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                    var cellStyle = workBook.CreateCellStyle();
                    var cFont = workBook.CreateFont();
                    cFont.IsBold = true; //字体加粗
                    cFont.FontName = "微软雅黑"; //字体名称
                    cFont.FontHeightInPoints = 10; //字号
                    cellStyle.VerticalAlignment = VerticalAlignment.Center; //垂直居中
                    cellStyle.Alignment = HorizontalAlignment.Left; //水平居左
                    cellStyle.BorderTop = BorderStyle.Thin; //上边框
                    cellStyle.BorderBottom = BorderStyle.Thin; //下边框
                    cellStyle.BorderLeft = BorderStyle.Thin; //左边框
                    cellStyle.BorderRight = BorderStyle.Thin; //右边框
                    cellStyle.SetFont(cFont);

                    #endregion 格式一:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                    #region 格式二:字体不加粗+微软雅黑+字号14+垂直居中+水平居中

                    var cFont2 = workBook.CreateFont();
                    var cellStyle2 = workBook.CreateCellStyle();
                    var hssfDataFormat = workBook.CreateDataFormat();
                    cFont2.IsBold = false; //字体加粗
                    cFont2.FontName = "微软雅黑"; //字体名称
                    cFont2.FontHeightInPoints = 12; //字号
                    cellStyle2.VerticalAlignment = VerticalAlignment.Center; //垂直居中
                    cellStyle2.Alignment = HorizontalAlignment.Center; //水平居中
                    cellStyle2.SetFont(cFont2);

                    #endregion 格式二:字体不加粗+微软雅黑+字号14+垂直居中+水平居中

                    #region 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居左

                    var cFont3 = workBook.CreateFont();
                    var cellStyle3 = workBook.CreateCellStyle();
                    cFont3.IsBold = false; //字体不加粗
                    cFont3.FontName = "微软雅黑"; //字体名称
                    cFont3.FontHeightInPoints = 10; //字号
                    cellStyle3.VerticalAlignment = VerticalAlignment.Center; //垂直居中
                    cellStyle3.Alignment = HorizontalAlignment.Left; //水平居中
                    cellStyle3.BorderTop = BorderStyle.Thin; //上边框
                    cellStyle3.BorderBottom = BorderStyle.Thin; //下边框
                    cellStyle3.BorderLeft = BorderStyle.Thin; //左边框
                    cellStyle3.BorderRight = BorderStyle.Thin; //右边框
                    cellStyle3.SetFont(cFont3);
                    //cellStyle3.DataFormat = hssfDataFormat.GetFormat("yyyy-MM-dd");

                    #endregion 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居左

                    #region 格式四:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                    var cellStyle4 = workBook.CreateCellStyle();
                    var cFont4 = workBook.CreateFont();
                    cFont4.IsBold = true; //字体加粗
                    cFont4.FontName = "微软雅黑"; //字体名称
                    cFont4.FontHeightInPoints = 10; //字号
                    cellStyle4.VerticalAlignment = VerticalAlignment.Center; //垂直居中
                    cellStyle4.Alignment = HorizontalAlignment.Left; //水平居左
                    cellStyle4.SetFont(cFont);

                    #endregion 格式四:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                    #endregion 单元格格式设置

                    #region 第一行

                    var row1 = sheet1.CreateRow(0);
                    sheet1.SetColumnWidth(0, 550);

                    row1.CreateCell(0);
                    row1.CreateCell(1);
                    row1.CreateCell(2);
                    row1.CreateCell(3);
                    //合并单元格
                    sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));
                    row1.GetCell(0).SetCellValue(outDate.ToDateTime().Month + "月" + outDate.ToDateTime().Day +
                                                 "号 接送单");
                    //单元格式设置
                    row1.GetCell(0).CellStyle = cellStyle2;

                    #endregion 第一行

                    //2.获取不得重复的上车点集合
                    IEnumerable<int> tempModels = orderModels.Select(a => a.LineBusPointId).Distinct<int>();
                    string strLineBusId = "";

                    foreach (var lineBusId in tempModels)
                    {
                        strLineBusId += lineBusId + ",";
                    }

                    strLineBusId = strLineBusId.Substring(0, strLineBusId.Length - 1);

                    //3.根据上车点获取上车点实例对象 跨数据库
                    lineBusPointModels = _lineBusPointBiz.GetBusPointByManyId(strLineBusId);

                    //int tempModelsCnt = tempModels.Count();
                    int rowIndex = 1;
                    foreach (var tempModel in tempModels)
                    {
                        var linBusPointModel = lineBusPointModels.Where(a => a.Id == tempModel.ToInt()).FirstOrDefault();
                        if (linBusPointModel == null)
                            linBusPointModel = new TpLineBusPointModel();

                        #region JS时间 上车点

                        string jsType = "";
                        if (linBusPointModel.JsType == 1)
                            jsType = "（只接不送）";
                        if (linBusPointModel.JsType == 2)
                            jsType = "（只送不接）";
                        if (linBusPointModel.JsType == 3)
                            jsType = "";
                        var row2 = sheet1.CreateRow(rowIndex);
                        row2.CreateCell(0);
                        row2.CreateCell(1);
                        row2.CreateCell(2);
                        row2.CreateCell(3);
                        sheet1.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 3));
                        row2.GetCell(0).SetCellValue(linBusPointModel.JsTime + linBusPointModel.BusPoint + jsType);
                        //单元格样式设置
                        row2.GetCell(0).CellStyle = cellStyle4;

                        #endregion JS时间 上车点

                        #region 表Title

                        var row3 = sheet1.CreateRow(rowIndex + 1);
                        row3.CreateCell(0).SetCellValue("线路名称");
                        row3.CreateCell(1).SetCellValue("姓名");
                        row3.CreateCell(2).SetCellValue("联系电话");
                        row3.CreateCell(3).SetCellValue("人数");
                        //单元格样式设置
                        row3.GetCell(0).CellStyle = cellStyle;
                        row3.GetCell(1).CellStyle = cellStyle;
                        row3.GetCell(2).CellStyle = cellStyle;
                        row3.GetCell(3).CellStyle = cellStyle;

                        #endregion 表Title

                        #region 循环加载订单数据

                        var tempOrderModels = orderModels.Where(a => a.LineBusPointId == tempModel.ToInt());
                        var ordersCnt = tempOrderModels.Count();

                        int rowIndex2 = rowIndex + 2;
                        int tolTravellerCnt = 0;
                        foreach (var tempOrderModel in tempOrderModels)
                        {
                            var row4 = sheet1.CreateRow(rowIndex2);
                            row4.CreateCell(0).SetCellValue(tempOrderModel.LineName);
                            row4.CreateCell(1).SetCellValue(tempOrderModel.LinkMan);
                            row4.CreateCell(2).SetCellValue(tempOrderModel.LinkPhone);
                            row4.CreateCell(3).SetCellValue(tempOrderModel.TravellerCount);
                            //单元格样式设置
                            row4.GetCell(0).CellStyle = cellStyle3;
                            row4.GetCell(1).CellStyle = cellStyle3;
                            row4.GetCell(2).CellStyle = cellStyle3;
                            row4.GetCell(3).CellStyle = cellStyle3;
                            rowIndex2++;
                            //人数总和
                            tolTravellerCnt += tempOrderModel.TravellerCount;
                        }

                        //求出每个上车点的人数总和
                        var row5 = sheet1.CreateRow(rowIndex2);
                        row5.CreateCell(0);
                        row5.CreateCell(1);
                        row5.CreateCell(2).SetCellValue("人数合计：");
                        row5.CreateCell(3).SetCellValue(tolTravellerCnt);
                        row5.GetCell(0).CellStyle = cellStyle3;
                        row5.GetCell(1).CellStyle = cellStyle3;
                        row5.GetCell(2).CellStyle = cellStyle;
                        row5.GetCell(3).CellStyle = cellStyle3;

                        rowIndex = ordersCnt == 1 ? rowIndex2 + ordersCnt + 1 : rowIndex2 + ordersCnt - 1;

                        #endregion 循环加载订单数据
                    }

                    #region 行高和列宽设置

                    sheet1.SetColumnWidth(0, 256 * 55);
                    sheet1.SetColumnWidth(1, 256 * 10);
                    sheet1.SetColumnWidth(2, 256 * 15);
                    sheet1.SetColumnWidth(3, 256 * 8);

                    #endregion 行高和列宽设置

                    workBook.Write(ms);
                    Response.AddHeader("Content-Disposition",
                                       "attachment; filename=" +
                                       HttpUtility.UrlEncode(outDate.ToDateTime().Month + "月" +
                                                             outDate.ToDateTime().Day + "号接送单.xls"));
                    Response.BinaryWrite(ms.ToArray());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    workBook = null;
                    ms.Close();
                    ms.Dispose();
                }
            }
            return Content("1");
        }

        #endregion 导出接送单

        #region 导出游客名单

        /// <summary>
        /// 导出游客名单（old）
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult TouristsList(int tourId)
        {
            var tpTourPlanModel = new TpTourPlanModel();
            var tpLineModel = new TpLineModel();
            var orderModels = new List<TpOrderModel>();
            var tpTravellerModels = new List<TpTravellerModel>();
            var lineBusPoint = new TpLineBusPointModel();

            //根据团Id获取团对象
            tpTourPlanModel = _biz.GetTourById(tourId);
            //根据线路Id获取线路对象
            tpLineModel = _lineBiz.GetLineById(tpTourPlanModel.LineId);

            //根据TourId获取订单信息列表
            orderModels = _orderBiz.GetOrdersByTourId(tourId);

            var workBook = new HSSFWorkbook();
            var ms = new MemoryStream();
            try
            {
                //创建工作簿
                var sheet1 = workBook.CreateSheet("巴士标准格式");

                #region 单元格格式设置

                #region 格式一:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                var cellStyle = workBook.CreateCellStyle();
                var cFont = workBook.CreateFont();
                cFont.IsBold = true;//字体加粗
                cFont.FontName = "微软雅黑";//字体名称
                cFont.FontHeightInPoints = 10;//字号
                cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle.SetFont(cFont);

                #endregion 格式一:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                #region 格式二:字体不加粗+微软雅黑+字号10+垂直居中+水平居左

                var cFont2 = workBook.CreateFont();
                var cellStyle2 = workBook.CreateCellStyle();
                var hssfDataFormat = workBook.CreateDataFormat();
                cFont2.IsBold = false;//字体加粗
                cFont2.FontName = "微软雅黑";//字体名称
                cFont2.FontHeightInPoints = 10;//字号
                cellStyle2.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle2.Alignment = HorizontalAlignment.Left;//水平居中
                cellStyle2.SetFont(cFont2);
                cellStyle2.DataFormat = hssfDataFormat.GetFormat("yyyy-MM-dd");

                #endregion 格式二:字体不加粗+微软雅黑+字号10+垂直居中+水平居左

                #region 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居中

                var cFont3 = workBook.CreateFont();
                var cellStyle3 = workBook.CreateCellStyle();
                cFont3.IsBold = false;//字体不加粗
                cFont3.FontName = "微软雅黑";//字体名称
                cFont3.FontHeightInPoints = 10;//字号
                cellStyle3.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle3.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle3.BorderTop = BorderStyle.Thin;//上边框
                cellStyle3.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle3.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle3.BorderRight = BorderStyle.Thin;//右边框
                cellStyle3.SetFont(cFont3);

                #endregion 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居中

                #endregion 单元格格式设置

                #region 第一行

                var row1 = sheet1.CreateRow(0);
                sheet1.SetColumnWidth(0, 550);

                //线路名称 列
                row1.CreateCell(1).SetCellValue("线路名称：");

                //合并单元格
                sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 2, 6));
                row1.CreateCell(2).SetCellValue(tpLineModel.LineName);

                //单元格式设置
                row1.GetCell(1).CellStyle = cellStyle;
                row1.GetCell(2).CellStyle = cellStyle2;

                #endregion 第一行

                #region 第二行

                var row2 = sheet1.CreateRow(1);
                row2.CreateCell(1).SetCellValue("出发日期：");
                sheet1.AddMergedRegion(new CellRangeAddress(1, 1, 2, 3));
                row2.CreateCell(2).SetCellValue(tpTourPlanModel.OutDate.ToDateFormat());

                //单元格样式设置
                row2.GetCell(1).CellStyle = cellStyle;
                row2.GetCell(2).CellStyle = cellStyle2;

                #endregion 第二行

                //汽车
                if (tpLineModel.TrafficType == 1)
                {
                    #region 第三行

                    var row3 = sheet1.CreateRow(2);
                    row3.CreateCell(1).SetCellValue("订单编号");
                    row3.CreateCell(2).SetCellValue("座位号");
                    row3.CreateCell(3).SetCellValue("客人姓名");
                    row3.CreateCell(4).SetCellValue("联系电话");
                    row3.CreateCell(5).SetCellValue("报价类型");
                    row3.CreateCell(6).SetCellValue("上车点");
                    row3.CreateCell(7).SetCellValue("订单备注");
                    row3.CreateCell(8).SetCellValue("分销商");

                    //单元格式设置
                    row3.GetCell(1).CellStyle = cellStyle3;
                    row3.GetCell(2).CellStyle = cellStyle3;
                    row3.GetCell(3).CellStyle = cellStyle3;
                    row3.GetCell(4).CellStyle = cellStyle3;
                    row3.GetCell(5).CellStyle = cellStyle3;
                    row3.GetCell(6).CellStyle = cellStyle3;
                    row3.GetCell(7).CellStyle = cellStyle3;
                    row3.GetCell(8).CellStyle = cellStyle3;

                    #endregion 第三行

                    #region 循环加载游客信息

                    var cn = orderModels.Count;
                    var rowIndex = 3;
                    for (int j = 0; j < cn; j++)
                    {
                        tpTravellerModels.Clear();
                        //订单对应的游客信息列表
                        tpTravellerModels = orderModels[j].TravellerModels;

                        var cnt = tpTravellerModels.Count;

                        for (int i = 0; i < cnt; i++)
                        {
                            var row = sheet1.CreateRow(rowIndex);
                            row.CreateCell(1);//订单编号
                            row.CreateCell(2).SetCellValue(tpTravellerModels[i].SeatNum);//座位号
                            row.CreateCell(3).SetCellValue(tpTravellerModels[i].Name);//客人姓名
                            row.CreateCell(4).SetCellValue(tpTravellerModels[i].Phone);//联系电话
                            row.CreateCell(5).SetCellValue(tpTravellerModels[i].PriceContent);//报价类型
                            row.CreateCell(6).SetCellValue(orderModels[j].LineBusPoint.IsNullOrEmpty() ? "" : orderModels[j].LineBusPoint.ToJsonDeserialize<TpLineBusPointModel>().BusPoint);//上车点
                            row.CreateCell(7);//订单备注
                            row.CreateCell(8);//分销商
                            //字体格式设置
                            row.GetCell(1).CellStyle = cellStyle3;
                            row.GetCell(2).CellStyle = cellStyle3;
                            row.GetCell(3).CellStyle = cellStyle3;
                            row.GetCell(4).CellStyle = cellStyle3;
                            row.GetCell(5).CellStyle = cellStyle3;
                            row.GetCell(6).CellStyle = cellStyle3;
                            row.GetCell(7).CellStyle = cellStyle3;
                            row.GetCell(8).CellStyle = cellStyle3;

                            rowIndex++;
                        }
                        if (cnt > 0)
                        {
                            lineBusPoint = _lineBusPointBiz.GetBusPointById(orderModels[j].LineBusPointId);
                            if (lineBusPoint == null)
                            {
                                lineBusPoint = new TpLineBusPointModel();
                            }

                            //合并行
                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 1, 1));//合并订单编号
                            sheet1.GetRow(rowIndex - cnt).GetCell(1).SetCellValue(orderModels[j].Id);//填充 订单编号

                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 6, 6));//合并上车点

                            string jsType = "";
                            if (lineBusPoint.JsType == 1)
                                jsType = "（只接不送）";
                            if (lineBusPoint.JsType == 2)
                                jsType = "（只送不接）";
                            if (lineBusPoint.JsType == 3)
                                jsType = "";

                            string temp = lineBusPoint.JsTime + lineBusPoint.BusPoint + jsType;
                            sheet1.GetRow(rowIndex - cnt).GetCell(6)
                                .SetCellValue(temp);//填充 上车点

                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 7, 7));//合并订单备注
                            sheet1.GetRow(rowIndex - cnt).GetCell(7).SetCellValue(orderModels[j].Remark);//填充 订单备注
                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 8, 8));//合并订单备注
                            var customer = DictionaryTools.GetCachedCustomer(orderModels[j].BookingCustomer);
                            sheet1.GetRow(rowIndex - cnt).GetCell(8).SetCellValue(customer.Name);//填充 分销商
                        }
                    }

                    #endregion 循环加载游客信息
                }
                else//其他交通类型
                {
                    #region 第三行

                    var row3 = sheet1.CreateRow(2);
                    row3.CreateCell(1).SetCellValue("订单编号");
                    row3.CreateCell(2).SetCellValue("客人姓名");
                    row3.CreateCell(3).SetCellValue("联系电话");
                    row3.CreateCell(4).SetCellValue("报价类型");
                    row3.CreateCell(5).SetCellValue("证件号码");
                    row3.CreateCell(6).SetCellValue("上车点");
                    row3.CreateCell(7).SetCellValue("订单备注");
                    row3.CreateCell(8).SetCellValue("分销商");

                    //单元格式设置
                    row3.GetCell(1).CellStyle = cellStyle3;
                    row3.GetCell(2).CellStyle = cellStyle3;
                    row3.GetCell(3).CellStyle = cellStyle3;
                    row3.GetCell(4).CellStyle = cellStyle3;
                    row3.GetCell(5).CellStyle = cellStyle3;
                    row3.GetCell(6).CellStyle = cellStyle3;
                    row3.GetCell(7).CellStyle = cellStyle3;
                    row3.GetCell(8).CellStyle = cellStyle3;

                    #endregion 第三行

                    #region 循环加载游客信息

                    var cn = orderModels.Count;
                    var rowIndex = 3;
                    for (int j = 0; j < cn; j++)
                    {
                        tpTravellerModels.Clear();
                        //订单对应的游客信息列表
                        tpTravellerModels = orderModels[j].TravellerModels;
                        var cnt = tpTravellerModels.Count;

                        for (int i = 0; i < cnt; i++)
                        {
                            var row = sheet1.CreateRow(rowIndex);
                            row.CreateCell(1);//订单编号
                            row.CreateCell(2).SetCellValue(tpTravellerModels[i].Name);//客人姓名
                            row.CreateCell(3).SetCellValue(tpTravellerModels[i].Phone);//联系电话
                            row.CreateCell(4).SetCellValue(tpTravellerModels[i].PriceContent);//报价类型
                            row.CreateCell(5).SetCellValue(tpTravellerModels[i].PassNo);//证件号码
                            row.CreateCell(6).SetCellValue(orderModels[j].LineBusPoint.IsNullOrEmpty() ? "" : orderModels[j].LineBusPoint.ToJsonDeserialize<TpLineBusPointModel>().BusPoint);//上车点
                            row.CreateCell(7);//订单备注
                            row.CreateCell(8);//分销商
                            //字体格式设置
                            row.GetCell(1).CellStyle = cellStyle3;
                            row.GetCell(2).CellStyle = cellStyle3;
                            row.GetCell(3).CellStyle = cellStyle3;
                            row.GetCell(4).CellStyle = cellStyle3;
                            row.GetCell(5).CellStyle = cellStyle3;
                            row.GetCell(6).CellStyle = cellStyle3;
                            row.GetCell(7).CellStyle = cellStyle3;
                            row.GetCell(8).CellStyle = cellStyle3;

                            rowIndex++;
                        }
                        if (cnt > 0)
                        {
                            lineBusPoint = _lineBusPointBiz.GetBusPointById(orderModels[j].LineBusPointId);
                            if (lineBusPoint == null)
                            {
                                lineBusPoint = new TpLineBusPointModel();
                            }
                            //合并行
                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 1, 1));//合并订单编号
                            sheet1.GetRow(rowIndex - cnt).GetCell(1).SetCellValue(orderModels[j].Id);//填充 订单编号

                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 6, 6));//合并上车点
                            sheet1.GetRow(rowIndex - cnt).GetCell(6).SetCellValue(lineBusPoint.BusPoint);//填充 上车点

                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 7, 7));//合并订单备注
                            sheet1.GetRow(rowIndex - cnt).GetCell(7).SetCellValue(orderModels[j].Remark);//填充 订单备注
                            sheet1.AddMergedRegion(new CellRangeAddress(rowIndex - cnt, rowIndex - 1, 8, 8));//合并订单备注
                            var customer = DictionaryTools.GetCachedCustomer(orderModels[j].BookingCustomer);
                            sheet1.GetRow(rowIndex - cnt).GetCell(8).SetCellValue(customer.Name);//填充 分销商
                        }
                    }

                    #endregion 循环加载游客信息

                    workBook.SetSheetName(0, "其他交通标准格式");
                }

                #region 行高和列宽设置

                sheet1.SetColumnWidth(1, 3850);
                sheet1.SetColumnWidth(3, 3850);
                sheet1.SetColumnWidth(4, 3850);
                sheet1.SetColumnWidth(5, 3850);
                sheet1.SetColumnWidth(6, 6850);
                sheet1.SetColumnWidth(7, 5850);
                sheet1.SetColumnWidth(8, 5850);

                #endregion 行高和列宽设置

                workBook.Write(ms);
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode("导游手册(游客名单).xls"));
                Response.BinaryWrite(ms.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                workBook = null;
                ms.Close();
                ms.Dispose();
            }
            return Content("");
        }

        /// <summary>
        /// 导出游客名单（new）
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult ExportTourists(int tourId)
        {
            var line = new TpLineBiz().GetLineByTour(tourId);
            if (line.TrafficType != 1)
            {
                // 非汽车班，沿用老的模板。暂不做修改。
                return TouristsList(tourId);
            }

            var tour = new TpTourPlanBiz().GetTourById(tourId);
            var orders = new OrderBiz().GetOrderByTourId(tourId); // 包含已取消订单
            var tourists = new TravellerBiz().GetByTourId(tourId, true, true);// 包含已取消游客
            var tourPirces = new TpPriceBiz().GetPrices(tourId);// 该团所有报价
            var viewModels = (from a in orders
                              where a.IsCancel == 0 && a.OrderState == 2
                              // 排序已取消
                              select new BusTouristsExcelVModel()
                              {
                                  OrderCode = a.Id.ToString(),
                                  Seats = ExportUtils.GetSeats(a.OrderCode, tourists),
                                  LinkMan = GetLinkMan(tourists, a),
                                  LinkPhone = GetLinkPhone(tourists, a),
                                  BookingCustomer = ExportUtils.GetBookingCustomer(a.BookingCustomer, OwnerCode),
                                  TravellerCount = a.TravellerCount,
                                  BusPoint = a.LineBusPoint.ToJsonDeserialize<TpLineBusPointModel>().BusPoint
                                           + "-" + a.LineBusPoint.ToJsonDeserialize<TpLineBusPointModel>().JsTime,
                                  ZiFei = ExportUtils.GetZifei(a.OrderCode, tourists),
                                  SingleRoom = ExportUtils.GetSingleRoom(a.OrderCode, tourists),
                                  PriceContents = ExportUtils.GetPriceContents(a, tourists, tourPirces),
                                  Remark = a.Remark
                              }).ToList();
            //汽车班excle模板
            BusTouristsTemplate(viewModels, tour);

            return Content("");
        }

        private string GetLinkMan(List<TpTravellerModel> models, TpOrderModel order)
        {
            var temps = models.Where(a => a.OrderCode == order.OrderCode).ToList();
            string returnValue = "";
            foreach (var traveller in temps)
            {
                if (traveller.Phone.IsNullOrEmpty())
                {
                    continue;
                }
                returnValue += traveller.Name + Environment.NewLine;
            }
            return returnValue;
        }

        private string GetLinkPhone(List<TpTravellerModel> models, TpOrderModel order)
        {
            var temps = models.Where(a => a.OrderCode == order.OrderCode).ToList();
            string returnValue = "";
            foreach (var traveller in temps)
            {
                if (traveller.Phone.IsNullOrEmpty())
                {
                    continue;
                }
                returnValue += traveller.Phone + Environment.NewLine;
            }
            return returnValue;
        }

        /// <summary>
        /// 汽车班excel模板
        /// </summary>
        /// <param name="vModels"></param>
        /// <param name="tour"></param>
        private void BusTouristsTemplate(List<BusTouristsExcelVModel> vModels, TpTourPlanModel tour)
        {
            var workBook = new HSSFWorkbook();
            var ms = new MemoryStream();
            //创建工作簿
            var sheet1 = workBook.CreateSheet("{0}游客名单".With(tour.Id));

            try
            {
                // set title
                var contents = "团号：{0}   出发日期：{1}".With(tour.Id + "-" + tour.LineName, tour.OutDate.ToDateFormat());

                Toolkit.Npoi.SetTitle(sheet1, 11, contents);
                Toolkit.Npoi.SetTitleStyle(workBook, sheet1);

                Toolkit.Npoi.SetTable(sheet1, vModels);
                Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                row.CreateCell(4).SetCellValue(vModels.Sum(a => a.TravellerCount));

                // 自适应宽度
                Toolkit.Npoi.AutoSetWidth(sheet1);

                workBook.Write(ms);
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode("{0}游客名单.xls".With(tour.Id)));
                Response.BinaryWrite(ms.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                workBook = null;
                ms = null;
            }
        }

        /// <summary>
        /// 非汽车班excel模板
        /// 暂不处理
        /// </summary>
        private void TouristsTemplate()
        {
        }

        #endregion 导出游客名单

        #region 导出单团对账单

        /// <summary>
        /// 导出单团对账单
        /// </summary>
        /// <param name="tourId">团号</param>
        /// <returns></returns>
        public ActionResult ExportDuiZhangDanExcel(int tourId)
        {
            // var line = new TpLineBiz().GetLineByTour(tourId);
            var tour = new TpTourPlanBiz().GetTourById(tourId);
            var orders = new OrderBiz().GetOrderByTourId(tourId); // 包含已取消订单
            var tourists = new TravellerBiz().GetByTourId(tourId, true, true);// 包含已取消游客
            var tourPrices = new TpPriceBiz().GetPrices(tourId); // 包含该团所有报价
            DuiZhangDanExcelVModel viewModel = new DuiZhangDanExcelVModel();
            viewModel.OutDate = tour.OutDate.ToDateFormat();
            viewModel.TourId = tourId;
            viewModel.TourName = tour.LineName;

            viewModel.Contents = (from a in orders
                                  where a.IsCancel != 1
                                  // 排序已取消
                                  select new DuiZhangDanContentVModel()
                                  {
                                      OrderCode = a.Id.ToString(),
                                      JoinOrderCode = a.JoinOrderCode,
                                      LinkMan = a.LinkMan,
                                      Managers = a.Managers,
                                      BookingCustomer = ExportUtils.GetBookingCustomer(a.BookingCustomer, OwnerCode),
                                      TravellerCount = a.TravellerCount,
                                      TolYsPrice = a.TolYsPrice,
                                      ZiFei = ExportUtils.GetZifei(a.OrderCode, tourists),
                                      SingleRoom = ExportUtils.GetSingleRoom(a.OrderCode, tourists),
                                      JsPrice = tourists.Where(b => b.OrderCode == a.OrderCode && b.State == 2).Sum(b => b.JiePrice + b.SongPrice),
                                      PriceContents = ExportUtils.GetPriceContents(a, tourists, tourPrices),
                                      Remark = a.Remark
                                  }).ToList();

            DuiZhangDanTemplate(viewModel);

            return Content("");
        }

        /// <summary>
        /// 对账单模板
        /// </summary>
        private void DuiZhangDanTemplate(DuiZhangDanExcelVModel viewModel)
        {
            var workBook = new HSSFWorkbook();
            var ms = new MemoryStream();
            //创建工作簿
            var sheet1 = workBook.CreateSheet("{0}单团对账单".With(viewModel.TourId));

            try
            {
                // set title
                var contents = "团号：{0}    团名：{1}    出发日期：{2}".With(viewModel.TourId, viewModel.TourName, viewModel.OutDate);

                Toolkit.Npoi.SetTitle(sheet1, 12, contents);
                Toolkit.Npoi.SetTitleStyle(workBook, sheet1);

                Toolkit.Npoi.SetTable(sheet1, viewModel.Contents);
                Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                row.CreateCell(6).SetCellValue((double)viewModel.Contents.Sum(a => a.TolYsPrice));
                row.CreateCell(5).SetCellValue(viewModel.Contents.Sum(a => a.TravellerCount));

                // 自适应宽度
                Toolkit.Npoi.AutoSetWidth(sheet1);

                workBook.Write(ms);
                Response.AddHeader("Content-Disposition",
                                   "attachment; filename=" +
                                   HttpUtility.UrlEncode("{0}单团对账单.xls".With(viewModel.TourId)));
                Response.BinaryWrite(ms.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                workBook = null;
                ms = null;
            }
        }

        #endregion 导出单团对账单

        #region 金棕榈名单导出

        public ActionResult Visitor(int tourId)
        {
            var tpTravellerModels = new List<TpTravellerModel>();
            var tpTourPlanModel = new List<TpTourPlanModel>();
            var tpLineRouteModel = new List<TpLineRouteModel>();
            var lineRouteExcelVModel = new List<LineRouteExcelVModel>();//团队基本行程信息实体类
            var tpLineRoute = new List<TpLineRouteModel>();
            var travellerModels = new List<TravellerModels>();//游客名单实体类

            var workBook = new HSSFWorkbook();
            var ms = new MemoryStream();

            #region 导出团队基本信息

            var sheet1 = workBook.CreateSheet("团队基本信息");
            var sheet2 = workBook.CreateSheet("团队基本行程信息");
            var sheet3 = workBook.CreateSheet("游客名单");
            try
            {
                var row = sheet1.CreateRow(0);
                row.CreateCell(0).SetCellValue("团号");
                row.CreateCell(1).SetCellValue("线路名称");
                row.CreateCell(2).SetCellValue("大交通");
                row.CreateCell(3).SetCellValue("地接社");
                row.CreateCell(4).SetCellValue("出境日期");
                row.CreateCell(5).SetCellValue("出境时间");
                row.CreateCell(6).SetCellValue("出发班次/车次");
                row.CreateCell(7).SetCellValue("出境岸口");
                row.CreateCell(8).SetCellValue("入境日期");
                row.CreateCell(9).SetCellValue("入境时间");
                row.CreateCell(10).SetCellValue("返回班次/车次");
                row.CreateCell(11).SetCellValue("入境口岸");
                row.CreateCell(12).SetCellValue("领队姓名");
                row.CreateCell(13).SetCellValue("领队证号");
                row.CreateCell(14).SetCellValue("线路行程");
                row.CreateCell(15).SetCellValue("拼团信息及备注");
                var rowInde = 1;
                var tourPlanExcelVModel = _biz.GetPlanById(tourId); //查询出团队基本信息
                tpLineRoute = _biz.Getpath(tourPlanExcelVModel.LineId);//查询出线路行程

                var row4 = sheet1.CreateRow(rowInde);
                row4.CreateCell(0).SetCellValue(tourPlanExcelVModel.TourNo);
                row4.CreateCell(1).SetCellValue(tourPlanExcelVModel.LineName);
                row4.CreateCell(2).SetCellValue(tourPlanExcelVModel.TrafficType);
                row4.CreateCell(3).SetCellValue(tourPlanExcelVModel.Totakecommuntiy);
                row4.CreateCell(4).SetCellValue(tourPlanExcelVModel.OutDate.ToDateFormat());
                row4.CreateCell(5).SetCellValue(tourPlanExcelVModel.DepartureTime);
                row4.CreateCell(6).SetCellValue(tourPlanExcelVModel.DepartBan);
                row4.CreateCell(7).SetCellValue(tourPlanExcelVModel.PortOfExit);
                row4.CreateCell(8).SetCellValue(tourPlanExcelVModel.EntryDate == null ? "" : tourPlanExcelVModel.EntryDate.Value.ToString("yyyy-MM-dd"));
                row4.CreateCell(9).SetCellValue(tourPlanExcelVModel.EntryTime);
                row4.CreateCell(10).SetCellValue(tourPlanExcelVModel.Returnregular);
                row4.CreateCell(11).SetCellValue(tourPlanExcelVModel.PortOfEntry);
                row4.CreateCell(12).SetCellValue(tourPlanExcelVModel.Name);
                row4.CreateCell(13).SetCellValue(tourPlanExcelVModel.TourCard);

                StringBuilder sb = new StringBuilder();
                foreach (var item in tpLineRoute)
                {
                    sb.Append("第" + item.Days + "天" + item.Title);
                    sb.Append("饮食：" + item.Catering);
                    sb.Append("住宿：" + item.Hotel);
                    sb.Append("路线行程：" + item.Contents);
                }
                row4.CreateCell(14).SetCellValue(sb.ToString());
                row4.CreateCell(15).SetCellValue(tourPlanExcelVModel.Remarks);

                #endregion 导出团队基本信息

                #region 导出团队基本行程信息

                var row2 = sheet2.CreateRow(0);
                row2.CreateCell(0).SetCellValue("团号");
                row2.CreateCell(1).SetCellValue("前往城市");
                row2.CreateCell(2).SetCellValue("前往国家/地区");
                row2.CreateCell(3).SetCellValue("游览行程");
                row2.CreateCell(4).SetCellValue("是否过境");
                row2.CreateCell(5).SetCellValue("天数");
                row2.CreateCell(6).SetCellValue("站点");
                var rowIndeq = 1;
                lineRouteExcelVModel = _biz.GetTourRouteInfoTourId(tourId);//查询出团队基本行程信息
                var cno = lineRouteExcelVModel.Count;
                for (int i = 0; i < cno; i++)
                {
                    var sheet2Row = sheet2.CreateRow(rowIndeq);
                    sheet2Row.CreateCell(0).SetCellValue(lineRouteExcelVModel[i].LineName);
                    sheet2Row.CreateCell(1).SetCellValue(lineRouteExcelVModel[i].City);
                    sheet2Row.CreateCell(2).SetCellValue(lineRouteExcelVModel[i].Contruny);
                    sheet2Row.CreateCell(3).SetCellValue(lineRouteExcelVModel[i].Title + lineRouteExcelVModel[i].Contents);
                    sheet2Row.CreateCell(4).SetCellValue(lineRouteExcelVModel[i].IsGuoJin);
                    sheet2Row.CreateCell(5).SetCellValue(lineRouteExcelVModel[i].Days);
                    sheet2Row.CreateCell(6).SetCellValue(lineRouteExcelVModel[i].zhandian);
                    rowIndeq++;
                }

                #endregion 导出团队基本行程信息

                #region 导出游客名单

                var row3 = sheet3.CreateRow(0);
                row3.CreateCell(0).SetCellValue("团号");
                row3.CreateCell(1).SetCellValue("姓名");
                row3.CreateCell(2).SetCellValue("英文名");
                row3.CreateCell(3).SetCellValue("性别");
                row3.CreateCell(4).SetCellValue("生日");
                row3.CreateCell(5).SetCellValue("出生地");
                row3.CreateCell(6).SetCellValue("联系方式(手机)");
                row3.CreateCell(7).SetCellValue("证件类型");
                row3.CreateCell(8).SetCellValue("证件号");
                row3.CreateCell(9).SetCellValue("签发地");
                row3.CreateCell(10).SetCellValue("发证日期");

                var rowIndex = 1;
                travellerModels = _biz.GetvisitoriId(tourId);//查询出游客信息
                var cnt = travellerModels.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var sheet3Row = sheet3.CreateRow(rowIndex);
                    sheet3Row.CreateCell(0).SetCellValue(travellerModels[i].TourNo);
                    sheet3Row.CreateCell(1).SetCellValue(travellerModels[i].Name);
                    sheet3Row.CreateCell(2).SetCellValue(travellerModels[i].PinYin);
                    sheet3Row.CreateCell(3).SetCellValue(travellerModels[i].Sex);
                    sheet3Row.CreateCell(4).SetCellValue(travellerModels[i].DateOfBirth == null ? "" : travellerModels[i].DateOfBirth.Value.ToString("yyyy-MM-dd"));
                    sheet3Row.CreateCell(5).SetCellValue(travellerModels[i].PlaceOfBirth);
                    sheet3Row.CreateCell(6).SetCellValue(travellerModels[i].Phone);
                    sheet3Row.CreateCell(7).SetCellValue(travellerModels[i].PassType);
                    sheet3Row.CreateCell(8).SetCellValue(travellerModels[i].PassNo);
                    sheet3Row.CreateCell(9).SetCellValue(travellerModels[i].PlaceOfIssue);
                    sheet3Row.CreateCell(10).SetCellValue(travellerModels[i].DateOfIssue == null ? "" : travellerModels[i].DateOfIssue.Value.ToString("yyyy-MM-dd"));
                    rowIndex++;
                }

                #endregion 导出游客名单

                workBook.Write(ms);
                Response.AddHeader("Content-Disposition",
                                   "attachment; filename=" +
                                   HttpUtility.UrlEncode("金棕榈名单.xls"));
                Response.BinaryWrite(ms.ToArray());
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
            }
            finally
            {
                workBook = null;
                ms = null;
            }
            return Content("");
        }

        #endregion 金棕榈名单导出

        //#region 游客信息导出
        //public ActionResult Listourists(int tourId)
        //{
        //    var travellerModels = new List<TouristModels>();//游客名单实体类
        //    var workBook = new HSSFWorkbook();
        //    var ms = new MemoryStream();
        //    var sheet1 = workBook.CreateSheet("游客信息");
        //    try
        //    {
        //        #region 导出游客信息
        //        var row3 = sheet1.CreateRow(0);
        //        row3.CreateCell(0).SetCellValue("姓名");
        //        row3.CreateCell(1).SetCellValue("英文名");
        //        row3.CreateCell(2).SetCellValue("性别");
        //        row3.CreateCell(3).SetCellValue("生日");
        //        row3.CreateCell(4).SetCellValue("出生地");
        //        row3.CreateCell(5).SetCellValue("联系方式(手机)");
        //        row3.CreateCell(6).SetCellValue("证件类型");
        //        row3.CreateCell(7).SetCellValue("证件号");
        //        row3.CreateCell(8).SetCellValue("签发地");
        //        row3.CreateCell(9).SetCellValue("发证日期");
        //        var rowIndex = 1;
        //        travellerModels = _tourPlanBiz.GTourisId(tourId);//查询出游客信息
        //        var cnt = travellerModels.Count;
        //        for (int i = 0; i < cnt; i++)
        //        {
        //            var sheet1Row = sheet1.CreateRow(rowIndex);
        //            sheet1Row.CreateCell(0).SetCellValue(travellerModels[i].Name);
        //            sheet1Row.CreateCell(1).SetCellValue(travellerModels[i].PinYin);
        //            sheet1Row.CreateCell(2).SetCellValue(travellerModels[i].Sex);
        //            sheet1Row.CreateCell(3).SetCellValue(travellerModels[i].DateOfBirth == null ? "" : travellerModels[i].DateOfBirth.Value.ToString("yyyy-MM-dd"));
        //            sheet1Row.CreateCell(4).SetCellValue(travellerModels[i].PlaceOfBirth);
        //            sheet1Row.CreateCell(5).SetCellValue(travellerModels[i].Phone);
        //            sheet1Row.CreateCell(6).SetCellValue(travellerModels[i].PassType);
        //            sheet1Row.CreateCell(7).SetCellValue(travellerModels[i].PassNo);
        //            sheet1Row.CreateCell(8).SetCellValue(travellerModels[i].PlaceOfIssue);
        //            sheet1Row.CreateCell(9).SetCellValue(travellerModels[i].DateOfIssue == null ? "" : travellerModels[i].DateOfIssue.Value.ToString("yyyy-MM-dd"));
        //            rowIndex++;
        //        }
        //        #endregion
        //        workBook.Write(ms);
        //        Response.AddHeader("Content-Disposition",
        //                           "attachment; filename=" +
        //                           HttpUtility.UrlEncode("游客信息.xls"));
        //        Response.BinaryWrite(ms.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    finally
        //    {
        //        workBook = null;
        //        ms = null;
        //    }
        //    return Content("");
        //}
        //#endregion

        #region 页面初始化

        /// <summary>
        /// 页面初始化
        /// </summary>
        protected override void InitPage()
        {
            //推荐方式
            ViewBag.RecommendType = DictionaryTools.GetEnumsBy(Enums.TuiJianTypeEnum).ToSelectListFor();
            //条件：是否成团
            ViewBag.IsToured = new List<KeyValueBean>
                                   {
                                       new KeyValueBean { Key = "", Value = "全部" },
                                       new KeyValueBean { Key = "0", Value = "未成团" },
                                       new KeyValueBean { Key = "1", Value = "已成团" }
                                   };
            //线路类型
            ViewBag.LineTypeCheckItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum);
        }

        #endregion 页面初始化

        #region 换团操作

        /// <summary>
        /// 选取换团
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SelectExchangeTour(ExchangeTourVModel vModel)
        {
            InitPage();
            vModel.OwnerCode = OwnerCode;

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            TeamBiz _TeamBiz = new TeamBiz();
            ViewBag.AccountTeamBeans = _TeamBiz.GetOpTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);

            if (vModel.OperationState == 0) vModel.OperationState = 1;  //初始状态为选择换团对象
            if (Request.IsAjaxRequest())
            {
                vModel.TourList = new TpTourQuotaMapBiz().GetExchangeTours(vModel);
                return PartialView("ExchangeTour/UCExchangeTourList", vModel);
            }
            return View("ExchangeTour/SelectExchangeTour", vModel);
        }

        /// <summary>
        /// 获取带操作的订单列表
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult SelectExchangeOrder(int tourId)
        {
            var vModel = new ExchangeTourVModel
            {
                OrderList = new OrderBiz().GetExchangeOrders(tourId, UserInfo),
                ExchangeFromTour = _biz.GetTourById(tourId)
            };
            return PartialView("ExchangeTour/UCSelectExchangeOrder", vModel);
        }

        /// <summary>
        /// 并入他团
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns>
        /// 0：成功
        /// 1：库存不足
        /// 2：座位号冲突
        /// 3：其他异常
        /// </returns>
        public ActionResult MergeToExistTour(ExchangeTourVModel vModel)
        {
            if (vModel.ExchangeOrders.IsNullOrEmpty() || vModel.ExchangeToTourId <= 0 || vModel.ExchangeFromTourId <= 0)
                return Json(new { State = 3 });

            #region 判断库存

            TourQuotaMapModel exchangeTourMap = new TpTourQuotaMapBiz().GetMapWithAll(vModel.ExchangeFromTourId);
            TourQuotaMapModel targetTourMap = new TpTourQuotaMapBiz().GetMapWithAll(vModel.ExchangeToTourId);

            OrderBiz orderBiz = new OrderBiz();
            Int32 touristCount = orderBiz.GetTouristCount(vModel.ExchangeOrders);
            if (touristCount > targetTourMap.Quota.UseQuota)
                return Json(new { State = 1 });    //库存不足

            //变更换团 源、目标 的库存
            QuotaModel exchangeQuota = exchangeTourMap.Quota;
            exchangeQuota.UsedQuota -= touristCount;
            exchangeQuota.UseQuota += touristCount;
            QuotaModel targetQuota = targetTourMap.Quota;
            targetQuota.UsedQuota += touristCount;
            targetQuota.UseQuota -= touristCount;

            #endregion 判断库存

            #region 判断座位（如果是汽车班）

            TpTourPlanModel exchangeTour = new TpTourPlanBiz().GetTourAndLine(vModel.ExchangeFromTourId);
            TpBusSeatModel exchangeBusSeat = null;  //源 座位表（释放）
            TpBusSeatModel targetBusSeat = null;    //目标 座位表（占位）
            IList<BusSeatModel> exchangeSeatModels = new List<BusSeatModel>();
            IList<BusSeatModel> targetSeatModels = new List<BusSeatModel>();
            IList<Int32> conflictSeatNo = new List<Int32>();     //冲突座位号List
            if (exchangeTour.Line.TrafficType == 1)
            {
                //汽车班次
                exchangeBusSeat = new TpBusSeatBiz().GetBusSeatByTour(vModel.ExchangeFromTourId);
                exchangeSeatModels = exchangeBusSeat.SeatModels;
                targetBusSeat = new TpBusSeatBiz().GetBusSeatByTour(vModel.ExchangeToTourId);
                targetSeatModels = targetBusSeat.SeatModels;

                IList<Int32> exchangeSeatNo = orderBiz.GetSeatList(vModel.ExchangeOrders);

                foreach (BusSeatModel seat in targetSeatModels)
                {
                    //var copySeatModel = new BusSeatModel { No = seat.No, State = seat.State };
                    if (exchangeSeatNo.Contains(seat.No.ToInt()))
                    {
                        if (seat.State != 1)
                        {
                            conflictSeatNo.Add(seat.No.ToInt());    //座位号冲突
                        }
                        else
                        {
                            seat.State = 2;    //占位
                            var exchangeSeat = exchangeSeatModels.FirstOrDefault(p => p.No == seat.No);
                            if (exchangeSeat != null)
                                exchangeSeat.State = 1; //释放
                        }
                    }
                }
            }
            if (conflictSeatNo.Count > 0)
            {
                return Json(new { State = 2, SeatNo = String.Join(",", conflictSeatNo) });
            }
            if (exchangeBusSeat != null && exchangeSeatModels != null)
                exchangeBusSeat.SeatDetail = exchangeSeatModels.ToJsonSerialize();
            if (targetBusSeat != null && targetSeatModels != null)
                targetBusSeat.SeatDetail = targetSeatModels.ToJsonSerialize();

            #endregion 判断座位（如果是汽车班）

            orderBiz.MergeToExistTour(vModel.ExchangeOrders, vModel.ExchangeToTourId, exchangeQuota, 
                targetQuota, exchangeBusSeat, targetBusSeat, UserInfo );
            return Json(new { State = 0 });
        }

        /// <summary>
        /// 分入新团
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="copyTour"></param>
        /// <param name="seatList"></param>
        /// <returns>
        /// 0：成功
        /// 1：库存不足
        /// 2：座位号冲突
        /// 3：其他异常
        /// </returns>
        public ActionResult SplitToNewTour(ExchangeTourVModel vModel, CopyTourVModel copyTour, List<BusSeatModel> seatList)
        {
            if (vModel.ExchangeOrders.IsNullOrEmpty() || vModel.ExchangeFromTourId <= 0)
                return Json(new { State = 3 });

            #region 判断库存

            TourQuotaMapModel exchangeTourMap = new TpTourQuotaMapBiz().GetMapWithAll(vModel.ExchangeFromTourId);

            OrderBiz orderBiz = new OrderBiz();
            Int32 touristCount = orderBiz.GetTouristCount(vModel.ExchangeOrders);
            if (touristCount > copyTour.Quota.PlanQuota - copyTour.Quota.HoldQuota)
                return Json(new { State = 1 });    //库存不足

            #endregion 判断库存

            #region 判断座位（如果是汽车班）

            TpTourPlanModel exchangeTour = new TpTourPlanBiz().GetTourAndLine(vModel.ExchangeFromTourId);
            TpBusSeatModel exchangeBusSeat = null;  //源 座位表（释放）
            IList<BusSeatModel> exchangeSeatModels = new List<BusSeatModel>();
            IList<Int32> conflictSeatNo = new List<Int32>();     //冲突座位号List
            IList<Int32> exchangeSeatNo = new List<Int32>();
            if (exchangeTour.Line.TrafficType == 1)
            {
                //汽车班次
                exchangeBusSeat = new TpBusSeatBiz().GetBusSeatByTour(vModel.ExchangeFromTourId);
                exchangeSeatModels = exchangeBusSeat.SeatModels;

                exchangeSeatNo = orderBiz.GetSeatList(vModel.ExchangeOrders);

                foreach (BusSeatModel seat in seatList)
                {
                    if (exchangeSeatNo.Contains(seat.No.ToInt()))
                    {
                        if (seat.State != 1)
                        {
                            conflictSeatNo.Add(seat.No.ToInt());    //座位号冲突
                        }
                        else
                        {
                            //新团期还未创建，暂不占位
                            //seat.State = 2;    //占位
                            var exchangeSeat = exchangeSeatModels.FirstOrDefault(p => p.No == seat.No);
                            if (exchangeSeat != null)
                                exchangeSeat.State = 1; //释放
                        }
                    }
                }
            }
            if (conflictSeatNo.Count > 0)
            {
                return Json(new { State = 2, SeatNo = String.Join(",", conflictSeatNo) });
            }
            if (exchangeBusSeat != null && exchangeSeatModels != null)
                exchangeBusSeat.SeatDetail = exchangeSeatModels.ToJsonSerialize();

            #endregion 判断座位（如果是汽车班）

            //保存复制的团期
            int copiedTourId = new TpLineTourPlanBiz().SaveCopyTour(copyTour, seatList, GlobalContext.Current.UserInfo);
            //创建完复制团期后，处理同并入他团
            if (copiedTourId > 0)
            {
                TourQuotaMapModel targetTourMap = new TpTourQuotaMapBiz().GetMapWithAll(copiedTourId);
                //变更换团 源、目标 的库存
                QuotaModel exchangeQuota = exchangeTourMap.Quota;
                exchangeQuota.UsedQuota -= touristCount;
                exchangeQuota.UseQuota += touristCount;
                QuotaModel targetQuota = targetTourMap.Quota;
                targetQuota.UsedQuota += touristCount;
                targetQuota.UseQuota -= touristCount;

                TpBusSeatModel targetBusSeat = null;
                if (seatList != null && seatList.Count > 0)
                {
                    targetBusSeat = new TpBusSeatBiz().GetBusSeatByTour(copiedTourId);
                    IList<BusSeatModel> targetSeatModels = targetBusSeat.SeatModels;
                    foreach (int no in exchangeSeatNo)
                    {
                        targetSeatModels.FirstOrDefault(p => p.No == no.ToString()).State = 2; //占位
                    }
                    targetBusSeat.SeatDetail = targetSeatModels.ToJsonSerialize();
                }

                orderBiz.MergeToExistTour(vModel.ExchangeOrders, copiedTourId, exchangeQuota, targetQuota, exchangeBusSeat,
                    targetBusSeat, UserInfo);
            }
            return Json(new { State = 0, TourId = copiedTourId });
        }

        #endregion 换团操作

        public ActionResult ChoiceGuide(GuideVModel vModel, int tourId)
        {
            if (vModel == null)
                vModel = new GuideVModel();
            vModel.GuidePageList.PageSize = 10;
            vModel.GuidePageList = _guideBiz.GetPagedList(vModel);

            ViewBag.TourId = tourId;

            if (Request.IsAjaxRequest())
                return PartialView("UCChoiceGuide", vModel);

            return View(vModel);
        }

        [HttpPost]
        public ActionResult AddGuideId(int tourId, int guideId)
        {
            try
            {
                _travellerBiz.AddTravellerByGuideId(tourId, guideId, UserInfo);
            }
            catch (Exception ex)
            {
                return Json(new { code = 201, message = "error:" + ex.Message });
            }

            return Json(new { code = 200, message = "OK" });
        }

        #region 客人名单导出

        /// <summary>
        /// 进入打印客户名单页面
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult UCGuest(int tourId)
        {
            var vModel = new EditTouristsVModel
            {
                Tour = new TpTourPlanBiz().GetTourById(tourId),
                Line = new TpLineBiz().GetLineByTour(tourId),
                Tourists = new TravellerBiz().GetByTourId(tourId),
            };
            return View(vModel);
        }

        /// <summary>
        /// 名单导出
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult Visitors(int tourId, EditTouristsVModel vModel)
        {
            var travellerModels = new List<GuestModels>();//游客名单实体类
            TpTourPlanModel ca = new TpTourPlanModel();
            var workBook = new HSSFWorkbook();
            var ms = new MemoryStream();
            var sheet3 = workBook.CreateSheet("游客名单");
            try
            {
                sheet3.DisplayGridlines = false;  //设置不显示网格线

                #region 格式一:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                var cellStyle = workBook.CreateCellStyle();
                var cFont = workBook.CreateFont();
                cFont.IsBold = true;//字体加粗
                cFont.FontName = "微软雅黑";//字体名称
                cFont.FontHeightInPoints = 13;//字号
                cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle.SetFont(cFont);

                #endregion 格式一:字体加粗+微软雅黑+字号10+垂直居中+水平居左

                #region 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居中

                var cFont2 = workBook.CreateFont();
                var cellStyle2 = workBook.CreateCellStyle();
                cFont2.IsBold = false;//字体不加粗
                cFont2.FontName = "微软雅黑";//字体名称
                cFont2.FontHeightInPoints = 10;//字号
                cellStyle2.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle2.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle2.BorderTop = BorderStyle.Thin;//上边框
                cellStyle2.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle2.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle2.BorderRight = BorderStyle.Thin;//右边框
                cellStyle2.SetFont(cFont2);

                #endregion 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居中

                #region 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居中+设置字体颜色

                var cFont3 = workBook.CreateFont();
                var cellStyle3 = workBook.CreateCellStyle();
                cFont3.IsBold = false;//字体不加粗
                cFont3.FontName = "微软雅黑";//字体名称
                cFont3.FontHeightInPoints = 10;//字号
                cellStyle3.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                cellStyle3.FillForegroundColor = ((short)22);//设置字体颜色
                cellStyle3.FillPattern = FillPattern.SolidForeground;//设置字体夜色
                cellStyle3.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle3.BorderTop = BorderStyle.Thin;//上边框
                cellStyle3.BorderBottom = BorderStyle.Thin;//下边框
                cellStyle3.BorderLeft = BorderStyle.Thin;//左边框
                cellStyle3.BorderRight = BorderStyle.Thin;//右边框
                cellStyle3.SetFont(cFont3);

                #endregion 格式三：字体不加粗+微软雅黑+字号10+垂直居中+水平居中+设置字体颜色

                #region 标题

                var contents = "团号 :{0} 路线：{1}".With(vModel.Tour.LineName, DictionaryTools.GetEnumValue(Enums.LineTypeEnum, vModel.Line.LineType.ToString()));
                //Toolkit.Npoi.SetTitle(sheet3, 15, contents);
                //Toolkit.Npoi.SetTitleStyle(workBook, sheet3);

                var row = sheet3.CreateRow(0);
                //合并单元格
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 1, 15));
                row.CreateCell(1).SetCellValue(contents);
                row.GetCell(1).CellStyle = cellStyle;

                #endregion 标题

                #region Excesl导出的文字

                var cao = 1;
                var row3 = sheet3.CreateRow(1);

                row3.CreateCell(cao).SetCellValue("序号");
                row3.GetCell(cao).CellStyle = cellStyle3;
                if (vModel.Visitor.Name2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("客人姓名");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.PinYin2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("英文名");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.Sex2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("性别");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.DateOfBirth2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("出身日期");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                cao++;
                row3.CreateCell(cao).SetCellValue("出身地");
                row3.GetCell(cao).CellStyle = cellStyle3;
                if (vModel.Visitor.daili2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("代理商");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.PassType2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("护照种类");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.PassNo2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("护照号");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.DateOfIssue2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("签发日期");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.DateOfExpiry2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("护照有效期");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                cao++;
                row3.CreateCell(cao).SetCellValue("客人电话");
                row3.GetCell(cao).CellStyle = cellStyle3;

                cao++;
                row3.CreateCell(cao).SetCellValue("领队");
                row3.GetCell(cao).CellStyle = cellStyle3;
                if (vModel.Visitor.PlaceOfIssue2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("护照签发地");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }
                if (vModel.Visitor.Remark2 == true)
                {
                    cao++;
                    row3.CreateCell(cao).SetCellValue("护照说明");
                    row3.GetCell(cao).CellStyle = cellStyle3;
                }

                #endregion Excesl导出的文字

                #region 从数据库查询出的数据

                var rao = 2;
                var rowIndex = 2;
                travellerModels = _biz.GuestiId(tourId);//查询出游客信息
                var cnt = travellerModels.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var kao = 1;
                    var sheet3Row = sheet3.CreateRow(rowIndex);
                    sheet3Row.CreateCell(kao).SetCellValue(rao);
                    sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    rao++;
                    if (vModel.Visitor.Name2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].Name);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.PinYin2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].PinYin);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.Sex2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].Sex);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.DateOfBirth2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].DateOfBirth == null ? "" : travellerModels[i].DateOfBirth.Value.ToString("yyyy-MM-dd"));
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    kao++;
                    sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].PlaceOfBirth);
                    sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    if (vModel.Visitor.daili2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].Booking);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.PassType2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].PassType);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.PassNo2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].PassNo);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.DateOfIssue2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].DateOfIssue == null ? "" : travellerModels[i].DateOfIssue.Value.ToString("yyyy-MM-dd"));
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.DateOfExpiry2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].DateOfExpiry == null ? "" : travellerModels[i].DateOfExpiry.Value.ToString("yyyy-MM-dd"));
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    kao++;
                    sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].Phone);
                    sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    kao++;
                    sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].LinName);
                    sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    if (vModel.Visitor.PlaceOfIssue2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].PlaceOfIssue);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    if (vModel.Visitor.Remark2 == true)
                    {
                        kao++;
                        sheet3Row.CreateCell(kao).SetCellValue(travellerModels[i].Remark);
                        sheet3Row.GetCell(kao).CellStyle = cellStyle2;
                    }
                    rowIndex++;

                    #endregion 从数据库查询出的数据
                }
                workBook.Write(ms);
                Response.AddHeader("Content-Disposition",
                                   "attachment; filename=" +
                                   HttpUtility.UrlEncode("客户名单.xls"));
                Response.BinaryWrite(ms.ToArray());
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
            }
            finally
            {
                workBook = null;
                ms = null;
            }
            return Content("");
        }

        #endregion 客人名单导出

        #region 核算成本凭证上传

        /// <summary>
        /// 凭证上传页面
        /// </summary>
        /// <param name="TourId"></param>
        /// <returns></returns>
        public ActionResult TourFile(int TourId)
        {
            TourVModel vModel = new TourVModel();
            vModel.TourId = TourId;
            vModel.TourFileList = _biz.GetTourFile(TourId);
            return PartialView("TourFile", vModel);
        }

        /// <summary>
        /// 凭证上传
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AddUploadTourFile(TourVModel vModel)
        {
            string filename = "";
            string FilePath = UploadTourFile(vModel.TourId, ref filename);
            TpTourFileModel model = new TpTourFileModel();
            model.TourID = vModel.TourId;
            model.FileName = filename;
            model.FilePath = FilePath;
            model.CreatedTime = DateTime.Now;
            model.IsDel = 0;
            model.SourceType = "";   //TODO

            _biz.AddTpTourFile(model);
            vModel.TourFileList = _biz.GetTourFile(vModel.TourId);
            return PartialView("TourFile", vModel);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="TourId"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        private string UploadTourFile(int TourId, ref string file_name)
        {
            HttpPostedFileBase file = Request.Files["TourFileName"];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            file_name = file.FileName;
            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = string.Format(@"tour\{0}", TourId);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteTourFile(int id)
        {
            TpTourFileModel model = _biz.GetTourFileById(id);
            _biz.DeleteTourFile(id);

            TourVModel vModel = new TourVModel();
            vModel.TourId = model.TourID;
            vModel.TourFileList = _biz.GetTourFile(vModel.TourId);
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("2")).ToList();
            return PartialView("TourFile", vModel);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DownTourFile(int id)
        {
            TpTourFileModel model = _biz.GetTourFileModel(id);
            if (model == null)
                return null;
            try
            {
                WebRequest.Create(AppSetting.Get("UploadFileRoot") + model.FilePath);
            }
            catch (Exception ex)
            {
                logger.Error("File not Found.", ex);
                return null;
            }

            byte[] fileData;
            try
            {
                using (WebClient client = new WebClient())
                {
                    fileData = client.DownloadData(AppSetting.Get("UploadFileRoot") + model.FilePath);

                    return File(fileData, "application/octet-stream", Server.UrlEncode(model.FileName));
                }
            }
            catch (Exception ex)
            {
                logger.Error("File download failure..", ex);
                return null;
            }
        }

        #endregion 核算成本凭证上传

        public ActionResult CheckTourTravellerCount(int tourId)
        {
            //TpOrderDao dao = new TpOrderDao();
            //string sql2 = @"select SUM(TravellerCount) as tprice from TpOrder where TourId=@0 and IsCancel=0 ";

            var quota = _quotaBiz.GetQuotaByTour(tourId);
            //var pax = dao.Query<int?>(sql2, tourId).FirstOrDefault();
            //int UseQuota = quota.PlanQuota - quota.HoldQuota;
            //if (pax != null) // 有游客人数的场合
            //{
            //    UseQuota = quota.PlanQuota - quota.HoldQuota - pax.Value;
            //}

            return Json(new { Code = "200", UseQuota = quota.UseQuota });
        }

        public ActionResult OpTourOrderInfo(int tourId, int fromId = 0, int lineId = 0)
        {
            var vModel = new EditTouristsVModel
            {
                Tour = new TpTourPlanBiz().GetTourById(tourId),
                Quota = new TpQuotaBiz().GetQuotaByTour(tourId),
                Line = new TpLineBiz().GetLineByTour(tourId),
                TpOrderList = _orderBiz.GetOrderByTourId(tourId),
                TpTourFileList = _biz.GetTourFileByTourId(tourId),
                //Tourists = new TravellerBiz().GetByTourId(tourId),
                DestinationList = DictionaryTools.GetEnumsBy(Enums.OutCityEnum)
            };

            ViewBag.fromId = fromId;
            ViewBag.lineId = lineId;
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("2")).ToList();

            return View(vModel);
        }

        #region 团附件上传操作相关方法

        public ActionResult UploadTourFile(int TourId, string FileSource)
        {
            string fileExt = "";
            string filename = "";
            int v = 0;

            // 修订版本的
            if (FileSource == "22")
            {
                v = _biz.UpdateTourNoticeVersion(TourId, "22");  // 出团通知
            }
            string FilePath = UploadTourFile(TourId, FileSource, v, ref filename, ref fileExt);

            TpTourFileModel model = new TpTourFileModel();
            model.TourID = TourId;
            model.FileName = filename;
            model.FilePath = FilePath;
            model.CreatedTime = DateTime.Now;
            model.IsDel = 0;
            model.CreatedBy = GlobalContext.Current.UserInfo.Code;
            model.MediaType = WebToolKit.GetFileMedia(fileExt);
            model.SourceType = FileSource;
            model.Revision = v + 1;
            _biz.AddTpTourFile(model);

            // 准备页面
            EditTouristsVModel vModel = new EditTouristsVModel();
            vModel.Tour.Id = TourId;
            vModel.TpTourFileList = _biz.GetTourFileByTourId(vModel.Tour.Id);
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("2")).ToList();
            return PartialView("UCAddTourFile", vModel);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="LineId"></param>
        /// <param name="TourId"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        private string UploadTourFile(int TourId, string FileSource, int v, ref string file_name, ref string file_extension)
        {
            HttpPostedFileBase file = Request.Files["TourFileName"];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            file_name = file.FileName;
            file_extension = Path.GetExtension(file.FileName);
            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            Stream stream = file.InputStream;

            request.FileStream = Toolkit.Image.StreamToBytes(stream);
            // 所属客户code\文件类型
            request.VirtualPath = string.Format(@"tour\{0}", TourId);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);
            string filepath = response.FilePath + response.FileName;

            //出团通知书模板 订单分发。
            if (FileSource == "22")
            {
                DistributeTourNotice(TourId, v, file_name, stream, filepath);
            }

            return filepath;
        }

        /// <summary>
        /// 产生出团通知书
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="v"></param>
        /// <param name="file_name"></param>
        /// <param name="stream"></param>
        private void DistributeTourNotice(int tourId, int v, string file_name, Stream stream, string filePath, bool isReplaceTxt = false)
        {
            try
            {
                //获取团下的订单信息
                var orderList = _orderBiz.GetOrderByTourId(tourId);
                //保存订单出团通知书文件信息
                //获取出团通知时文档数据流.

                byte[] bt = Toolkit.Image.StreamToBytes(stream);
                foreach (var item in orderList)
                {
                    // 文字替换 【通知人】
                    if (isReplaceTxt)
                    {
                        Stream me = new MemoryStream(bt);
                        XWPFDocument doc = new XWPFDocument(me);

                        #region 替换文件中的参数信息

                        string customerName = item.Managers + " " + item.CustomerName;

                        // 遍历段落
                        foreach (var para in doc.Paragraphs)
                        {
                            ReplaceKey(para, customerName);
                        }

                        //遍历表格
                        bool b = false;
                        foreach (var table in doc.Tables)
                        {
                            if (b) break;
                            foreach (var row in table.Rows)
                            {
                                if (b) break;
                                foreach (var cell in row.GetTableCells())
                                {
                                    if (b) break;
                                    foreach (var para in cell.Paragraphs)
                                    {
                                        b = ReplaceKey(para, customerName);
                                        if (b) break;
                                    }
                                }
                            }
                        }

                        #endregion 替换文件中的参数信息

                        #region 将文件上传到指定路径保存

                        MemoryStream ms = new MemoryStream();
                        doc.Write(ms);//写入到
                        string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file_name);
                        UploadFileRequest request = new UploadFileRequest();
                        request.FileName = filename;
                        request.FileStream = ms.GetBuffer();// Toolkit.Image.StreamToBytes();  //; Toolkit.Image.StreamToBytes(ms);

                        // 所属客户code\文件类型
                        request.VirtualPath = string.Format(@"order\{0}\notice", item.OrderCode);

                        //上传到指定的文件路径
                        UploadServiceClient client = new UploadServiceClient();
                        UploadFileResponse response = client.UploadFile(request);

                        #endregion 将文件上传到指定路径保存

                        filePath = response.FilePath + response.FileName;
                    }

                    #region 将路径信息写入到数据库表

                    _orderBiz.UpdateTourNoticeVersion(item.OrderCode, "2");

                    TpOrderFileModel model = new TpOrderFileModel();
                    model.KeyId = 0;
                    model.OrderCode = item.OrderCode;
                    model.FileName = file_name;
                    model.FilePath = filePath;
                    model.CreatedTime = DateTime.Now;
                    model.Remark = "";
                    model.IsDel = 0;
                    model.SourceType = "2";
                    model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                    model.MediaType = WebToolKit.GetFileMedia(Path.GetExtension(file_name));
                    model.Revision = v + 1;
                    _orderBiz.AddOrderFile(model);

                    #endregion 将路径信息写入到数据库表

                    // 发送消息给销售  出团通知更新  TODO
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private bool ReplaceKey(XWPFParagraph para, string customerName)
        {
            string text = para.ParagraphText;
            if (text.Contains("{$CUSTOMER_NAME}"))
            {
                para.ReplaceText("{$CUSTOMER_NAME}", customerName);
                return true;
            }
            return false;
        }

        private void ReplaceKey(XWPFParagraph para, Hashtable hh)
        {
            string text = para.ParagraphText;
            var runs = para.Runs;
            string styleid = para.Style;
            for (int i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                text = run.ToString();
                foreach (string p in hh.Keys)
                {
                    if (text.Contains("{$" + p + "}"))
                    {
                        text = text.Replace("{$" + p + "}", (string)hh[p]);
                    }
                }
                runs[i].SetText(text, 0);
            }
        }

        #endregion 团附件上传操作相关方法

        #region 开单

        public ActionResult CreateOpOrder(int tourId)
        {
            BookingVModel vModel = new BookingVModel();
            vModel.Tour = _bookingBiz.GetTourById(tourId);
            vModel.Quota = _quotaBiz.GetQuotaByTour(tourId);
            vModel.PriceModels = _priceBiz.GetValidPrices(tourId);

            ViewBag.SalesOfTeam = _teamBiz.GetSalesTeams(UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            //2。加载销售员列表。
            ViewBag.Salers = _customerBiz.GetTeamSales(GlobalContext.Current.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            return View("Booking/CreateOpOrder", vModel);
        }

        /// <summary>
        /// 获取当前团的剩余人数信息
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult GetTourUseQuota(int tourId)
        {
            var model = _quotaBiz.GetQuotaByTour(tourId);
            return Json(new { UseQuota = model.UseQuota });
        }

        /// <summary>
        /// OP 开单 ？
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SaveOpOrder(BookingVModel vModel)
        {
            vModel.Tour = _biz.GetTourById(vModel.TourId);
            vModel.Quota = _quotaBiz.GetQuotaByTour(vModel.TourId);
            vModel.LineModel = _lineBiz.GetLineById(vModel.Tour.LineId);
            vModel.PriceModels = _orderBiz.GetPricesByTourId(vModel.TourId);

            // 余位审核
            if (vModel.Quota.UseQuota < vModel.TravellerCount)
            {
                return Json(new { StateCode = OrderResultState.Code110 });
            }

            //1.保存订单信息
            //2.占用库存信息.
            string orderCode = "";
            vModel.DepositDate = DateTime.Now.AddHours(vModel.EffectiveHour);
            OrderResultState orderState = _orderBiz.SaveOpOrderTrans(vModel, ref orderCode, UserInfo);

            // 占位
            _orderBiz.FreeQuota(vModel.TourId, "", GlobalContext.Current.UserInfo.Code);

            if (orderState == OrderResultState.Code100)
            {
                // 记录日志
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderCode, vModel.SalerCode, GlobalContext.Current.UserInfo.Code, "开单");

                // 开单通知销售
                var sales = _accountBiz.GetAccountCustomer(vModel.SalerCode);
                if (!String.IsNullOrEmpty(sales.OpenID))
                {
                    string first = string.Format("{0}开单成功", sales.Name);
                    SendMessagClient.SendTemplateMessage(sales.OpenID, "jFkZkkv74K27HcZ6xnyaNV5elqSX7IdcYQHI4Nus170", first,
                       orderCode, vModel.LineModel.LineName, vModel.LineModel.LineId, "", "", "备注");
                }
            }

            return Json(new { StateCode = ((int)orderState).ToString() });
        }

        #endregion 开单
    }
}