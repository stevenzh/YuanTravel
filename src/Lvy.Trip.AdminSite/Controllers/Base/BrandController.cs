using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    public class BrandController : BaseController
    {
        private readonly BrandBiz biz = new BrandBiz();
        private TeamBiz _TeamBiz = new TeamBiz();

        public ActionResult Search(BrandVModel vModel)
        {
            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.BrandList = biz.GetBrandList(vModel);
            ViewBag.TeamBeans = _TeamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);

            if (Request.IsAjaxRequest())
                return PartialView("List", vModel);
            return View(vModel);
        }

        public ActionResult AddBrand()
        {
            ViewBag.TeamBeans = _TeamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            return View("UCEdit", new BrandVModel());
        }

        public ActionResult Edit(int Id)
        {
            var vModel = new BrandVModel();
            vModel.BrandModel = biz.GetBrandById(Id);
            ViewBag.TeamBeans = _TeamBiz.GetTeamsList(GlobalContext.Current.OwnerCode).Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);

            return View("UCEdit", vModel);
        }

        [HttpPost]
        public ActionResult Edit(BrandVModel vModel)
        {
            if (vModel.BrandModel.ID == default(int))
            {
                vModel.BrandModel.OwnerCode = UserInfo.OwnerCode;
                vModel.BrandModel.IsValid = 1;
                vModel.BrandModel.CreatedBy = GlobalContext.Current.UserInfo.Code;
                vModel.BrandModel.CreatedTime = DateTime.Now;
                biz.Add(vModel.BrandModel);
            }
            else
                biz.Update(vModel.BrandModel);

            return Json(new { code = "0", msg = "" });
        }

        public ActionResult Delete(int Id)
        {
            biz.Delete(Id);

            return Json(new { code = "0", msg = "" });
        }

        public ActionResult CheckBrandName(string name, string brandId)
        {
            bool flag = biz.CheckName(name, brandId);
            return Json(flag);
        }

        public ActionResult GetBrandList(string teamCode)
        {
            var list = DictionaryTools.GetCachedBrandDict().Select(t => t.Value).Where(a => a.TeamID == teamCode).ToList();
            var model = new
            {
                List = list,
                ReturnMsg = "0",
                TotalCount = list.Count()
            };

            return Json(model);
        }
    }
}