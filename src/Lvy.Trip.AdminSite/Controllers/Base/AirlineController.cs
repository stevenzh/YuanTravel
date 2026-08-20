using Lvy.Trip.Biz.Base;
using Lvy.VModels.Base;
using System;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 航空公司维护
    /// </summary>
    public class AirlineController : BaseController
    {
        private readonly AirlineBiz biz = new AirlineBiz();

        public ActionResult SearchAirline(AirlineVModel vModel)
        {
            if (vModel == null)
            {
                vModel = new AirlineVModel();
            }
            vModel.AirelinePageList = biz.GetPagedAirline(vModel);
            if (Request.IsAjaxRequest())
                return PartialView("UCAirlineSearch", vModel);
            return View(vModel);
        }

        public ActionResult AddAirline()
        {
            return View();
        }

        public ActionResult Add(AirlineVModel vModel)
        {
            try
            {
                vModel.AirlineInfo.IsValid = 1;
                biz.Add(vModel.AirlineInfo);
                return Json(new { code = "0", msg = "保存成功" });
            }
            catch (Exception)
            {
                return Json(new { code = "100", msg = "服务器异常，请稍后再试" });
            }
        }

        public ActionResult EditAirline(int Id)
        {
            var vModel = new AirlineVModel();
            vModel.AirlineInfo = biz.GetAirlineById(Id);
            return View(vModel);
        }

        public ActionResult Edit(AirlineVModel vModel)
        {
            try
            {
                var row = biz.Update(vModel.AirlineInfo);
                return Json(new { code = "0", msg = "保存成功" });
            }
            catch (Exception)
            {
                return Json(new { code = "100", msg = "服务器异常，请稍后再试" });
            }
        }

        public ActionResult DeleteAirline(int Id)
        {
            biz.Delete(Id);
            return Json(new { code = "0", message = "" });
        }

        public ActionResult CheckAirlineCode(string flightcode, string Id)
        {
            var model = biz.GetAirlineByCode(flightcode, Id);
            int result = 0;
            result = model == null ? 0 : 1;

            return Content(result.ToString());
        }
    }
}