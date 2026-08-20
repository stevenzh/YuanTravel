using Lvy.Models;
using Lvy.Models.JModels;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.WebSite.Mvc.Attributes;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Lvy.Trip.WebSite.Controllers
{
    public partial class LineAdminController
    {
        private readonly TpLineTourPlanBiz _tourPlanBiz = new TpLineTourPlanBiz();
        private readonly TpLineSuiteBiz _suiteBiz = new TpLineSuiteBiz();
        private readonly TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private readonly TpProductBiz _itemBiz = new TpProductBiz();
        private readonly AirlineBiz flghtBiz = new AirlineBiz();

        #region 团计划列表

        /// <summary>
        /// 查询团计划
        /// </summary>
        /// <param name="searchTourVModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchTour(SearchTourVModel searchTourVModel)
        {
            #region 查询预设值

            if (searchTourVModel.Condition == null)
                searchTourVModel.Condition = new TourConditionModel
                {
                    RecommendType = -1, //RecommendType赋值为-1是为了初始化时不选中推荐类型
                    SupplierCode = GlobalContext.Current.CustomerBy.Code,
                    IsImport = true
                };
            if (searchTourVModel.TourList == null)
                searchTourVModel.TourList = new PagedList<TourInfoVModel>();

            InitSearchTourData();

            if (GlobalContext.Current.IsSysAdmin) // 管理员看所有记录包含删除的
            {
                searchTourVModel.PlanStatus = "all";
            }

            #endregion 查询预设值

            // 查询列表
            searchTourVModel.TourList = _tourPlanBiz.GetTourList(searchTourVModel, GlobalContext.Current.UserInfo);

            if (Request.IsAjaxRequest())
                return PartialView("Tour/UCTourList", searchTourVModel);

            return View("Tour/SearchTour", searchTourVModel);
        }

        /// <summary>
        /// 查询团计划初始值
        /// </summary>
        private void InitSearchTourData()
        {
            ViewBag.RecommendType = new List<KeyValueBean> { new KeyValueBean { Key = "-1", Value = "全部" }, new KeyValueBean { Key = "0", Value = "普通" }, new KeyValueBean { Key = "1", Value = "特价" } };
        }

        /// <summary>
        /// 变更团计划上线、下线状态
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public int SaleOrClose(int tourId)
        {
            var model = _tourPlanBiz.GetTourById(tourId);
            int result = _tourPlanBiz.SaleOrClose(model, UserInfo.Code);
            return result;
        }

        /// <summary>
        /// 删除团计划
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult DeleteTour(int tourId)
        {
            int result = _tourPlanBiz.DeleteTour(tourId, UserInfo.Code);
            return Json(new { code = "0" });
        }

        public ActionResult RestoreTour(int tourId)
        {
            int result = _tourPlanBiz.RestoreTour(tourId, UserInfo.Code);
            return Json(new { code = "0" });
        }

        /// <summary>
        /// 复制团计划
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CopyTour(int id = 0)
        {
            var vModel = _tourPlanBiz.GetEditTour(id, OwnerCode);
            vModel.IsCopy = 1;
            vModel.Quota.UsedQuota = 0;
            vModel.Quota.UseQuota = vModel.Quota.PlanQuota - vModel.Quota.HoldQuota;
            return View("Tour/EditTour", vModel);
        }

        /// <summary>
        /// 显示价格
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult DisPlayPrice(int tourId = 0)
        {
            TpPriceBiz priceBiz = new TpPriceBiz();
            var models = priceBiz.GetValidPrices(tourId);
            return PartialView("Tour/UCDisplayPrice", models);
        }

        /// <summary>
        /// 验证团计划是否包含未取消的订单
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ContentResult CheckTourOrdered(int tourId)
        {
            var orders = new OrderBiz().GetValidOrderByTourId(tourId);
            if (orders != null && orders.Count > 0)
            {
                return Content("1");
            }
            return Content("0");
        }

        #endregion 团计划列表

        #region 添加团计划

        /// <summary>
        /// 添加团计划
        /// </summary>
        /// <param name="lineId">线路Id</param>
        /// <returns></returns>
        public ActionResult CreateTour(string lineId)
        {
            var model = new AddTourVModel()
            {
                LineId = lineId,
                Line = _tpLineBiz.GetLineById(lineId),
                SeatList = new List<BusSeatModel>(),
                TourPlan = new TpTourPlanModel { TourState = 3, TourType = 1 },//默认上线
                Quota = new QuotaModel(),
                TourFlightList = new List<TpTourFlightModel>(),
                AirlineList = flghtBiz.GetAirlineList(),
                PriceList = new List<TpPriceModel> { new TpPriceModel { PriceType = 1, PriceRemark = "成人价", SuitNum = 1, IsValid = 1, IsStandard = 1 } }
            };

            return View("Tour/CreateTour", model);
        }

        /// <summary>
        /// 添加团计划（提交）
        /// </summary>
        /// <param name="model"></param>
        /// <param name="seatList"></param>
        /// <param name="priceList"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult AddTour(AddTourVModel model, List<BusSeatModel> seatList, List<TpPriceModel> priceList)
        {
            model.SeatList = seatList;
            model.PriceList = priceList;
            _tourPlanBiz.AddTour(model, UserInfo.Code, OwnerCode);
            return RedirectToAction("SearchTour", new { lineId = model.LineId });
        }

        /// <summary>
        /// 根据座位数返回座位列表
        /// </summary>
        /// <param name="seatNum"></param>
        /// <returns></returns>
        public ActionResult CreateBusSeat(int seatNum = 0)
        {
            var jsonResult = string.Empty;
            List<BusSeatModel> models = null;
            if (seatNum > 0)
            {
                models = new List<BusSeatModel>();
                for (int i = 0; i < seatNum; i++)
                {
                    models.Add(new BusSeatModel() { No = (i + 1).ToString(CultureInfo.InvariantCulture), State = 1 });
                }
                //var serializer = new JavaScriptSerializer();
                //jsonResult = serializer.Serialize(models);
            }
            return PartialView("UCBusSeats", models);
        }

        public ActionResult RemovePrice(List<TpPriceModel> priceList, int rowIndex = -1)
        {
            if (rowIndex >= 0 && priceList.Count > 0)
            {
                priceList.RemoveAt(rowIndex);
            }
            return PartialView("Tour/UCAddPrice", priceList);
        }

        #endregion 添加团计划

        #region 编辑团计划

        /// <summary>
        /// 编辑团计划
        /// </summary>
        /// <param name="id">团计划Id</param>
        /// <returns></returns>
        public ActionResult EditTour(int id = 0)
        {
            var vModel = _tourPlanBiz.GetEditTour(id, OwnerCode);
            vModel.IsCopy = 0;
            return View("Tour/EditTour", vModel);
        }

        /// <summary>
        /// 保存团计划
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="seatList"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SaveTour(EditTourVModel vModel, List<BusSeatModel> seatList)
        {
            vModel.Tour.BookingLastDays = Request.Form["Tour.BookingLastDays"].ToDateTime();
            var serializer = new JavaScriptSerializer();
            if (seatList != null && seatList.Count > 0)
            {
                if (null == vModel.BusSeat)
                    vModel.BusSeat = new TpBusSeatModel();
                vModel.BusSeat.SeatNum = seatList.Count;
                vModel.BusSeat.SeatDetail = serializer.Serialize(seatList);
            }
            else
            {
                vModel.BusSeat = null;
            }
            _tourPlanBiz.SaveTour(vModel, GlobalContext.Current.UserInfo);

            // 计算余位  TODO

            return Json(new { code = 0, mssage = "" });
        }

        /// <summary>
        /// 在编辑团计划时用于重新创建座位表
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult ReCreateBusSeat(EditTourVModel vModel)
        {
            int seatNum = vModel.Quota.PlanQuota;
            int quotaId = vModel.Quota.Id;

            TpBusSeatBiz busSeatBiz = new TpBusSeatBiz();
            TpBusSeatModel oldBusSeat = busSeatBiz.GetBusSeatByTour(vModel.Tour.Id);//.GetBusSeat(vModel.Tour.Id, quotaId);
            List<BusSeatModel> seatList = new List<BusSeatModel>();
            if (seatNum > 0)
            {
                if (oldBusSeat != null)
                    seatList = oldBusSeat.SeatModels;

                if (seatNum >= seatList.Count)
                {
                    for (int i = seatList.Count; i < seatNum; i++)
                    {
                        seatList.Add(new BusSeatModel() { No = (i + 1).ToString(CultureInfo.InvariantCulture), State = 1 });
                    }
                }
                else
                {
                    for (int i = seatList.Count; i > seatNum; i--)
                    {
                        seatList.Remove(seatList.Find(p => p.No == i.ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
            if (vModel.IsCopy == 1)
            {
                foreach (var item in seatList)
                {
                    if (item.State == 2) item.State = 1;
                }
            }
            return PartialView("UCBusSeats", seatList);
        }

        /// <summary>
        /// 请求价格编辑列表
        /// </summary>
        /// <param name="id">团计划Id</param>
        /// <returns></returns>
        public ActionResult EditPrice(int id = 0)
        {
            TpPriceBiz priceBiz = new TpPriceBiz();
            List<TpPriceModel> priceModels = priceBiz.GetPrices(id);
            return PartialView("Tour/UCAddPrice", priceModels);
        }


        /// <summary>
        /// 批量修改团计划
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public ActionResult BatchEditTour(string lineId)
        {
            var vModel = new BatchEditTourVModel
            {
                Line = _tpLineBiz.GetLineById(lineId),
                LineNameSignList = _tourPlanBiz.GetDistinctNameTour(lineId),
                PriceList = new List<TpPriceModel> { new TpPriceModel { PriceType = 1, PriceRemark = "成人价", SuitNum = 1, IsValid = 1, IsStandard = 1 } },
                SelectedDays = new List<string>(),
                Tour = new TpTourPlanModel()
            };
            return View("Tour/BatchEditTour", vModel);
        }

        /// <summary>
        /// 保存
        ///
        /// 批量修改是以产品名称 和日期 需要修改 //TODO
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SaveBatchEditTour(BatchEditTourVModel vModel)
        {
            if (vModel.Tour.LineName.IsNullOrEmpty())
                return Json(new { State = 0, Msg = "团名不能为空。" });

            if (vModel.BeginDate.IsNullOrEmpty() || vModel.EndDate.IsNullOrEmpty())
            {
                return Json(new { State = 0, Msg = "起止日期不能为空。" });
            }
            _tourPlanBiz.SaveBatchEidtTour(vModel, UserInfo);

            return Json(new { State = 1, Url = Url.Action("SearchTour", new { lineId = vModel.Line.LineId }) });
        }

        #endregion 编辑团计划


        /// <summary>
        /// 返回复制团期视图
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult RenderCopyTour(int tourId = 0)
        {
            var line = _tpLineBiz.GetLineByTour(tourId);
            var map = new TpTourQuotaMapBiz().GetMapWithAll(tourId);
            var tour = map.Tour;
            /*
             * 因复制团期均为非共享，故无需考虑其库存与原始库存间的关系，
             * 此处仅填充原始库存的计划一项，其余均无需初始化。
             */
            var quota = new QuotaModel
            {
                PlanQuota = map.Quota.PlanQuota,
                UseQuota = map.Quota.PlanQuota,
                UsedQuota = 0,
                HoldQuota = 0
            };
            var priceList = new TpPriceBiz().GetPrices(tourId);
            var vModel = new CopyTourVModel
            {
                Line = line,
                Tour = tour,
                Quota = quota,
                PriceList = priceList
            };
            return PartialView("Tour/UCCopyTour", vModel);
        }

        /// <summary>
        /// 开班库存初始页
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public ActionResult UCTourStore(string lineId)
        {
            var line = _tpLineBiz.GetLineById(lineId);
            var priceList = new List<TpPriceModel>();
            priceList.Add(new TpPriceModel
            {
                PriceType = 1,
                PriceRemark = "成人价",
                SuitNum = 1,
                IsValid = 1,
                IsStandard = 1
            });

            var model = new AddTourVModel()
            {
                LineId = line.LineId,
                Line = line,
                SeatList = new List<BusSeatModel>(),
                TourPlan = new TpTourPlanModel { TourState = 3, TourType = 1 },//默认上线
                Quota = new QuotaModel(),
                AirlineList = flghtBiz.GetAirlineList(),
                PriceList = priceList,
                SuiteList = _suiteBiz.GetLineSuites(lineId),
            };

            return View("Tour/UCTourStore", model);
        }

        /// <summary>
        /// 取得线路所有开班（FullCalendar）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetCalendar(string id)
        {
            var plans = _tourPlanBiz.GetToursByLine(id);
            var rr = (from ss in plans
                      select new
                      {
                          title = ss.Price.ToString("￥00") + "\n余位:" + ss.UseQuota,
                          start = ss.OutDate.ToDateFormat(),
                          backgroundColor = "#66cc99",
                          extendedProps = new PlanExtendedModel { PlanId = ss.TourId }
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

        #region 库存团期相关操作方法


        /// <summary>
        /// 添加套餐
        /// </summary>
        /// <param name="product_id">线路id</param>
        /// <param name="trip_id">套餐id</param>
        /// <param name="name">套餐名称</param>
        /// <returns></returns>
        public ActionResult AddTourPackage(string product_id, string trip_id, string name)
        {
            TpLineSuiteModel model = new TpLineSuiteModel();
            model.LineId = product_id;
            model.PackageDescr = name;
            try
            {
                _suiteBiz.AddTourPackage(model);
            }
            catch (Exception ex)
            {
                return Json(new { code = 201, msg = "出错了" + ex.Message });
            }
            return Json(new { code = 200, msg = "OK" });
        }

        /// <summary>
        /// 修改套餐
        /// </summary>
        /// <param name="product_id">线路id</param>
        /// <param name="trip_id">套餐id</param>
        /// <param name="name">套餐名称</param>
        /// <returns></returns>

        public ActionResult EditTourPackage(string product_id, string trip_id, string name)
        {
            TpLineSuiteModel model = new TpLineSuiteModel();
            model.Id = Convert.ToInt32(trip_id);
            model.LineId = product_id;
            model.PackageDescr = name;
            try
            {
                _suiteBiz.EditTourPackage(model);
            }
            catch (Exception ex)
            {
                return Json(new { code = 201, msg = "出错了" + ex.Message });
            }
            return Json(new { code = 200, msg = "OK" });
        }

        /// <summary>
        /// 删除套餐
        /// </summary>
        /// <param name="product_id"></param>
        /// <param name="trip_id"></param>
        /// <returns></returns>
        public ActionResult DeleteTourPackage(int product_id, string trip_id)
        {
            try
            {
                _suiteBiz.DeleteTourPackage(trip_id);
            }
            catch (Exception ex)
            {
                return Json(new { code = 201, msg = "出错了" + ex.Message });
            }
            return Json(new { code = 200, msg = "OK" });
        }

        /// <summary>
        /// 保存开团信息方法。
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SavePriceStock(AddTourVModel vModel)
        {
            try
            {
                if (vModel.TourPlan.Id > 0)
                {
                    var editModel = _tourPlanBiz.GetEditTour(vModel.TourPlan.Id, OwnerCode);
                    //编辑
                    // EditTourVModel editModel = new EditTourVModel();

                    //Tour 信息
                    //editModel.Tour.TourSign = vModel.TourPlan.TourSign;
                    editModel.Tour.TuiJianType = vModel.TourPlan.TuiJianType;
                    editModel.Tour.TourType = vModel.TourPlan.TourType;
                    editModel.Tour.TourState = vModel.TourPlan.TourState;
                    editModel.Tour.MixedNum = vModel.TourPlan.MixedNum;
                    editModel.Tour.Remarks = vModel.TourPlan.Remarks;
                    editModel.Tour.AdditionInfo = vModel.TourPlan.AdditionInfo;

                    ///费用
                    editModel.Tour.VisaPrice = vModel.TourPlan.VisaPrice;
                    editModel.Tour.Tax = vModel.TourPlan.Tax;
                    editModel.Tour.SingleRoom = vModel.SingleRoom;
                    //editModel.Tour.TeJiaFanLi = vModel.TeJiaFanLi;
                    editModel.Tour.ZiFei = vModel.ZeiFei;
                    //editModel.Tour.LastKaiPiaoDate = vModel.TourPlan.LastKaiPiaoDate;

                    //Quota 信息
                    editModel.Quota.PlanQuota = vModel.Quota.PlanQuota;
                    editModel.Quota.HoldQuota = vModel.Quota.HoldQuota;

                    editModel.PriceList = vModel.PriceList;
                    editModel.TourFlightList = vModel.TourFlightList;
                    editModel.IsCopy = 0;
                    _tourPlanBiz.SaveTour(editModel, GlobalContext.Current.UserInfo);
                }
                else
                {
                    //添加
                    _tourPlanBiz.AddTour(vModel, UserInfo.Code, OwnerCode);
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 201, msg = ex.Message });
            }

            return Json(new { code = 200, msg = "OK" });
        }

        /// <summary>
        /// 获得编辑开班的数据（JSON）
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public ActionResult EditLoadPriceStock(int tourId)
        {
            var vModel = _tourPlanBiz.GetEditTour(tourId, OwnerCode);

            return Json(new { code = 200, msg = "OK", data = vModel });
        }

        /// <summary>
        /// 单天开班列表
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="outday"></param>
        /// <returns></returns>
        public ActionResult LoadTourStoreList(string lineId, string outday)
        {
            SearchTourVModel searchTourVModel = new SearchTourVModel();
            searchTourVModel.LineId = lineId;

            searchTourVModel.Condition.OutDateRange = outday.Replace("-", "/") + "-" + outday.Replace("-", "/");
            searchTourVModel.TourStoreList = _tourPlanBiz.GetTourStoreList(searchTourVModel, GlobalContext.Current.UserInfo);

            return PartialView("Tour/UCTourStoreList", searchTourVModel);
        }

        #endregion 库存团期相关操作方法
    }
    public class PlanExtendedModel
    {
        public int PlanId { get; set; }

    }
}