using Lvy.Models;
using Lvy.Trip.Biz.Base;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    public class TagController : BaseController
    {
        private readonly BaseTagBiz _biz = new BaseTagBiz();

        public ActionResult Search(TagVModel vModel)
        {
            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.TagPagedList = _biz.GetTagPagedList(vModel);
            InitPage();
            if (Request.IsAjaxRequest())
                return PartialView("List", vModel);
            return View(vModel);
        }

        public ActionResult AddTag()
        {
            InitPage();
            return View("UCEdit", new TagVModel());
        }

        public ActionResult Edit(int Id)
        {
            var vModel = new TagVModel();
            vModel.TagModel = _biz.GetTagById(Id);
            InitPage();
            return View("UCEdit", vModel);
        }

        [HttpPost]
        public ActionResult Edit(TagVModel vModel)
        {
            vModel.TagModel.ModifiedBy = UserInfo.Code;
            vModel.TagModel.ModifiedTime = DateTime.Now;
            if (vModel.TagModel.Id == 0)
            {
                vModel.TagModel.OwnerCode = UserInfo.OwnerCode;
                vModel.TagModel.IsValid = 1;
                _biz.Add(vModel.TagModel);
            }
            else
                _biz.Update(vModel.TagModel);

            return Json(new { code = "0", msg = "" });
        }

        public ActionResult Delete(int Id)
        {
            _biz.Delete(Id);

            return Json(new { code = "0", msg = "" });
        }

        public ActionResult CheckTagName(string name, int tagId)
        {
            bool flag = _biz.CheckName(name, tagId, UserInfo.OwnerCode);
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
        protected override void InitPage()
        {
            ViewBag.TagTypeBean = DictionaryTools.GetEnumsBys(Enums.ProductTypeEnum).ToSelectListFor();
        }
    }
}