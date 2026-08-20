using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lvy.Trip.Biz.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Models.BaseDB;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Lvy.VModels.Base;

namespace Lvy.Trip.AdminSite.Controllers
{
    public class GuideController : BaseController
    {
        private static readonly GuideBiz _biz = new GuideBiz();

        public ActionResult Search(GuideVModel vModel)
        {

            if (vModel == null)
                vModel = new GuideVModel();
            vModel.OwnerCode = GlobalContext.Current.OwnerCode;

            vModel.GuidePageList = _biz.GetPagedList(vModel);

            TeamBiz _TeamBiz = new TeamBiz();
            ViewBag.AccountTeamBeans = _TeamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);

            InitPage();
            if (Request.IsAjaxRequest())
                return PartialView("UCList", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult DeleteGuide(int Id)
        {
            _biz.DeleteGuide(Id);
            GuideVModel vModel = new GuideVModel();
            vModel.GuidePageList = _biz.GetPagedList(vModel);
            return PartialView("UCList", vModel);
        }


        /// <summary>
        /// 创建客户
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            InitPage();
            //部门下拉框显示
            TeamBiz _TeamBiz = new TeamBiz();
            ViewBag.AccountTeamBeans = _TeamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            return View();
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(GuideVModel vModel)
        {
            vModel.GuideModel.OwnerCode = GlobalContext.Current.OwnerCode;
            _biz.Add(vModel.GuideModel);

            // clear cache
            CacheContext.Current.Remove(Consts.AccountStrDic);
            return RedirectToAction("Search");
        }

        /// <summary>
        /// 修改显示
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Edit(int Id)
        {
            var vModel = new GuideVModel();
            vModel.GuideModel = _biz.GetGuideById(Id);
            //部门下拉框显示
            TeamBiz _TeamBiz = new TeamBiz();
            ViewBag.AccountTeamBeans = _TeamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);

            return View(vModel);
        }

        [HttpPost]
        public ActionResult Edit(GuideVModel vModel)
        {
            GuideModel Model = _biz.GetById(vModel.GuideModel.Id);

            Model.TeamID = vModel.GuideModel.TeamID;
            Model.Name = vModel.GuideModel.Name;
            Model.PinYin = vModel.GuideModel.PinYin;
            Model.Sex = vModel.GuideModel.Sex;
            Model.BirthDate = vModel.GuideModel.BirthDate;
            Model.BirthPlace = vModel.GuideModel.BirthPlace;
            Model.Card = vModel.GuideModel.Card;
            Model.Mobile = vModel.GuideModel.Mobile;
            Model.Tel = vModel.GuideModel.Tel;
            Model.Email = vModel.GuideModel.Email;
            Model.Address = vModel.GuideModel.Address;
            Model.TourKey = vModel.GuideModel.TourKey;
            Model.TourCard = vModel.GuideModel.TourCard;
            Model.CheckDate = vModel.GuideModel.CheckDate;
            Model.ICCard = vModel.GuideModel.ICCard;
            Model.LeadKey = vModel.GuideModel.LeadKey;
            Model.LeadCard = vModel.GuideModel.LeadCard;
            Model.DateStart = vModel.GuideModel.DateStart;
            Model.DateEnd = vModel.GuideModel.DateEnd;
            Model.Hzzl = vModel.GuideModel.Hzzl;
            Model.Hzno = vModel.GuideModel.Hzno;
            Model.HzAddress = vModel.GuideModel.HzAddress;
            Model.HzDate = vModel.GuideModel.HzDate;
            Model.HzEndDate = vModel.GuideModel.HzEndDate;
            Model.WorkType1 = vModel.GuideModel.WorkType1;
            Model.WorkType2 = vModel.GuideModel.WorkType2;
            Model.Remarks = vModel.GuideModel.Remarks;
            Model.WorkRemark = vModel.GuideModel.WorkRemark;

            _biz.Update(Model);
            // clear cache
            CacheContext.Current.Remove(Consts.AccountStrDic);
            return RedirectToAction("Search");
        }


    }
}