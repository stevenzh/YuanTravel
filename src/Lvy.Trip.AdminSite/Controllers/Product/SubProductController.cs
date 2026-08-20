using Common.Logging;
using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Product;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Security;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 子产品管理
    /// </summary>
    public class SubProductController : BaseController
    {
        private readonly TpProductBiz _biz = new TpProductBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private ILog logger = LogManager.GetLogger("SubProductController");

        public ActionResult Search(ProductVModel vModel)
        {
            var OpTeams = new List<SelectListItem>();
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                OpTeams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                OpTeams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(vModel.ProductModel.TeamCode) && OpTeams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值 ！不是总监不能为空
                {
                    vModel.ProductModel.TeamCode = OpTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "门店管理"))
            {
                OpTeams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2).ToSelectListFor(t => t.TeamID, v => v.TeamName);

                if (string.IsNullOrEmpty(vModel.ProductModel.TeamCode) && OpTeams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值 ！不是总监不能为空
                {
                    vModel.ProductModel.TeamCode = OpTeams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }
            else
            {
                // 不是OP
                OpTeams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.ProductPageList = _biz.GetPagedProduct(vModel);
            ViewBag.Teams = OpTeams;

            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        public ActionResult Edit(int Id)
        {
            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            TeamBiz _TeamBiz = new TeamBiz();

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长"))
            {
                var Teams = _TeamBiz.GetOpTeams(OwnerCode).Where(a => GlobalContext.Current.LoginUserTeams.Select(b => b.TeamID).Contains(a.TeamID)).ToList();
                if (Teams.Count == 0)
                {
                    ViewBag.Teams = Teams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                }
                else
                {
                    //默认第一个部门
                    ViewBag.Teams = Teams.ToSelectListFor(t => t.TeamID, v => v.TeamName, Teams.FirstOrDefault().TeamID);
                }
            }
            else
            {
                ViewBag.Teams = _TeamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }

            var vModel = new ProductVModel();
            if (Id != default(int))
                vModel.ProductModel = _biz.GetProductById(Id);
            ViewBag.ProductType = DictionaryTools.GetEnumsBy(Enums.ProductAllTypeEnum).ToSelectListFor();
            return View(vModel);
        }

        [HttpPost]
        public ActionResult Edit(ProductVModel vModel)
        {
            try
            {
                if (vModel.ProductModel.ProductID != default(int))
                {
                    var row = _biz.Update(vModel.ProductModel);
                }
                else
                {
                    vModel.ProductModel.OwnerCode = UserInfo.OwnerCode;
                    vModel.ProductModel.CreatedBy = UserInfo.Code;
                    vModel.ProductModel.CreatedTime = DateTime.Now;
                    vModel.ProductModel.IsValid = 1;
                    _biz.Add(vModel.ProductModel);
                }
                return Json(new { code = "0", msg = "保存成功" });
            }
            catch (Exception)
            {
                return Json(new { code = "100", msg = "服务器异常，请稍后再试" });
            }
        }

        public ActionResult Delete(int Id)
        {
            _biz.Delete(Id);
            var vModel = new ProductVModel();
            vModel.ProductPageList = _biz.GetPagedProduct(vModel);
            return PartialView("UCSearch", vModel);
        }

        public ActionResult CheckProductName(string name, string Id)
        {
            var flag = _biz.CheckProductName(name, Id);
            return Json(flag);
        }

        public ActionResult GetProductSelect2(string keyword)
        {
            IList<BaseAirlineModel> list = DictionaryTools.GetCachedAirlineDict().Values.ToList();
            if (keyword.IsNullOrEmpty())
            {
                list = list.OrderByDescending(a => a.Code).Take(12).ToList();
            }
            if (!keyword.IsNullOrEmpty())
            {
                list = list.Where(a => a.ShortName.Contains(keyword) || a.Code == keyword.ToUpper()).Take(15).ToList();
            }

            var model = new
            {
                incomplete_results = "false",
                items = list,
                total_count = list.Count
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSubProductInfo(int productId)
        {
            var model = new
            {
                code = '0',
                info = _biz.GetProductById(productId)
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}