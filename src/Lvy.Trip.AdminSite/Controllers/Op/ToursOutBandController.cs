using Lvy.Models;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.VModels.Op;
using Lvy.VModels.Order;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Op
{
    public class ToursOutBandController : BaseController
    {
        private readonly TpLineTourPlanBiz _tourPlanBiz = new TpLineTourPlanBiz();
        private readonly GuideBiz _guideBiz = new GuideBiz();
        private readonly TravellerBiz _travellerBiz = new TravellerBiz();

        // GET: ToursOutBand
        public ActionResult Search(SearchTourVModel searchTourVModel)
        {
            if (searchTourVModel == null)
            {
                searchTourVModel = new SearchTourVModel();
            }
            if (searchTourVModel.Condition == null)
                searchTourVModel.Condition = new TourConditionModel
                {
                    RecommendType = -1 //RecommendType赋值为-1是为了初始化时不选中推荐类型
                };
            if (searchTourVModel.TourList == null)
                searchTourVModel.TourList = new PagedList<TourInfoVModel>();
            ViewBag.RecommendType = new List<KeyValueBean> { new KeyValueBean { Key = "0", Value = "普通" }, new KeyValueBean { Key = "1", Value = "特价" } };

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            TeamBiz _TeamBiz = new TeamBiz();
            var teams = new List<SelectListItem>();
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                teams = _TeamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
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

            // 查询列表
            searchTourVModel.TourList = _tourPlanBiz.GetTourList(searchTourVModel, GlobalContext.Current.UserInfo);

            if (Request.IsAjaxRequest())
            {
                return PartialView("UCSearchList", searchTourVModel);
            }

            return View(searchTourVModel);
        }

        public ActionResult TourListView(SearchTourVModel searchTourVModel)
        {
            List<TourOutBandHeadVModel> cacheList = TempData["TourOutBandInfo"] as List<TourOutBandHeadVModel>;
            //缓存信息
            //  List<TourOutBandHeadVModel> cacheList= CacheContext.Current.Get(GlobalContext.Current.UserInfo.Code) as List<TourOutBandHeadVModel>;
            if (cacheList == null)
            {
                cacheList = new List<TourOutBandHeadVModel>();
            }
            string strTourIds = "";
            List<int> tourIds = searchTourVModel.TourIdsList;
            for (int i = 0; i < tourIds.Count; i++)
            {
                var list = cacheList.Where(a => a.OutBandHeadModel.TourId == tourIds[i]).ToList();
                if (list != null && list.Count > 0)
                {
                    continue;
                }

                strTourIds += tourIds[i] + ",";
            }
            strTourIds = strTourIds.TrimEnd(',');
            if (strTourIds != "")
            {
                List<TourOutBandHeadVModel> listTourOutBandHead = new List<TourOutBandHeadVModel>();
                var headList = _tourPlanBiz.GetTourOutBandHeadInfo(strTourIds);
                foreach (var item in headList)
                {
                    TourOutBandHeadVModel model = new TourOutBandHeadVModel();
                    model.OutBandHeadModel = item;
                    listTourOutBandHead.Add(model);
                }
                List<PrintTravellerVModel> listTpTraveller = _travellerBiz.GetByTourId(strTourIds);
                foreach (var item in listTourOutBandHead)
                {
                    var travellerList = listTpTraveller.Where(a => a.TourId == item.OutBandHeadModel.TourId).ToList();
                    if (travellerList.Count > 0)
                    {
                        item.TpTravellerList.AddRange(travellerList);
                    }
                }
                cacheList.InsertRange(0, listTourOutBandHead);
            }
            ViewBag.listTourOutBandHead = cacheList;

            TempData["TourOutBandInfo"] = cacheList;
            TempData.Keep("TourOutBandInfo");
            return View();
        }

        public ActionResult BackTourListView(List<TravellerCheckedVModel> TravellerCheckedList)
        {
            List<TourOutBandHeadVModel> cacheList = TempData["TourOutBandInfo"] as List<TourOutBandHeadVModel>;

            if (cacheList != null)
            {
                if (TravellerCheckedList != null && TravellerCheckedList.Count > 0)
                {
                    foreach (var item in TravellerCheckedList)
                    {
                        foreach (var item2 in cacheList)
                        {
                            var model = item2.TpTravellerList.Where(a => a.Id == item.Id).FirstOrDefault();
                            if (model != null)
                            {
                                model.IsChecked = item.IsChecked;
                                break;
                            }
                        }
                    }
                    TempData["TourOutBandInfo"] = cacheList;
                    TempData.Keep("TourOutBandInfo");
                    // CacheContext.Current.Add(GlobalContext.Current.UserInfo.Code, cacheList, 60);
                }
            }
            return RedirectToAction("Search");
        }

        public ActionResult PrintSet(List<TravellerCheckedVModel> TravellerCheckedList)
        {
            List<TourOutBandHeadVModel> cacheList = TempData["TourOutBandInfo"] as List<TourOutBandHeadVModel>;
            TourOutBandHeadVModel vModel = new TourOutBandHeadVModel();
            if (cacheList != null && cacheList.Count > 0)
            {
                //设置选中的游客信息
                if (TravellerCheckedList != null && TravellerCheckedList.Count > 0)
                {
                    foreach (var item in TravellerCheckedList)
                    {
                        foreach (var item2 in cacheList)
                        {
                            var model = item2.TpTravellerList.Where(a => a.Id == item.Id).FirstOrDefault();
                            if (model != null)
                            {
                                model.IsChecked = item.IsChecked;
                                break;
                            }
                        }
                    }
                }
                //清除掉未选中的游客人.
                foreach (var item in cacheList)
                {
                    var list = item.TpTravellerList.Where(a => a.IsChecked == false).ToList();
                    foreach (var item2 in list)
                    {
                        item.TpTravellerList.Remove(item2);
                    }
                }
                int TravellerCount = 0;
                int ManCount = 0;
                int WomenCount = 0;
                //计算人数
                foreach (var item in cacheList)
                {
                    TravellerCount += item.TpTravellerList.Count();
                    ManCount += item.TpTravellerList.Where(a => a.Sex == "1").Count();
                    WomenCount += item.TpTravellerList.Where(a => a.Sex == "2").Count();
                }
                List<KeyValueBean> listLeader = new List<KeyValueBean>();
                foreach (var item in cacheList)
                {
                    item.OutBandHeadModel.TravellerCount = TravellerCount;
                    item.OutBandHeadModel.ManCount = ManCount;
                    item.OutBandHeadModel.WomenCount = WomenCount;
                    item.OutBandHeadModel.IsContainsLingDui = true;
                    //获取游客中领队信息
                    //var model = item.TpTravellerList.Where(a => a.IsLeader==true).FirstOrDefault();
                    //if (model!=null)
                    //{
                    //   // var guidModel=_guideBiz.GetById(model.LeaderNo);
                    //    KeyValueBean bean = new KeyValueBean();
                    //    bean.Key = model.Id.ToString();
                    //    bean.Value = model.Name;
                    //    listLeader.Add(bean);
                    //}
                }
                vModel = cacheList[0];
            }
            TempData["TourOutBandInfo"] = cacheList;
            TempData.Keep("TourOutBandInfo");
            ViewBag.GuideList = _guideBiz.GetGuideList(GlobalContext.Current.OwnerCode);
            return View(vModel);
        }

        public ActionResult PrintTraveller(TourOutBandHeadVModel vModel)
        {
            OutBandHeadModel headModel = new OutBandHeadModel();
            List<PrintTravellerVModel> listTraveller = new List<PrintTravellerVModel>();
            List<TourOutBandHeadVModel> cacheList = TempData["TourOutBandInfo"] as List<TourOutBandHeadVModel>;
            if (cacheList != null && cacheList.Count > 0)
            {
                #region 数据整理

                //获取游客信息列表
                int idx = 0;
                foreach (var item in cacheList)
                {
                    if (idx == 0)
                    {
                        headModel = cacheList[idx].OutBandHeadModel;
                    }
                    listTraveller.AddRange(item.TpTravellerList);
                    idx++;
                }
                //设置表头信息数据。
                headModel.ZuTuanNo = vModel.OutBandHeadModel.ZuTuanNo;
                headModel.TourNo = vModel.OutBandHeadModel.TourNo;
                headModel.Years = vModel.OutBandHeadModel.Years;
                headModel.LineName = vModel.OutBandHeadModel.LineName;
                headModel.ZuTuanName = vModel.OutBandHeadModel.ZuTuanName;
                headModel.ZuTuanContact = vModel.OutBandHeadModel.ZuTuanContact;
                headModel.ReceptionName = vModel.OutBandHeadModel.ReceptionName;
                headModel.ReceptionContact = vModel.OutBandHeadModel.ReceptionContact;
                headModel.IsContainsEnterDateAndPosition = vModel.OutBandHeadModel.IsContainsEnterDateAndPosition;
                headModel.IsContainsLingDui = vModel.OutBandHeadModel.IsContainsLingDui;
                headModel.GuideName = vModel.OutBandHeadModel.GuideName;
                headModel.GuideNo = vModel.OutBandHeadModel.GuideNo;

                #endregion 数据整理
            }
            //获取团信息
            TempData["TourTraveller"] = listTraveller;
            TempData["TourHeadInfo"] = headModel;
            TempData.Keep();
            TempData.Remove("TourOutBandInfo");
            return View();
        }
    }
}