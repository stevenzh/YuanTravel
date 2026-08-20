using Common.Logging;
using Lvy.Models;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Trip.Biz.Crm;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    /// <summary>
    /// 产品管理
    /// </summary>
    public class ProductAdminController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(ProductAdminController));
        private ProductBiz _biz = new ProductBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();

        // GET: ProductAdmin
        public ActionResult Index()
        {
            try
            {
                InitData();
                var model = new VisaInformationQModel();
                model.VisaInformationList = new PagedList<VisaInformationModel>();
                ViewData["IsAdderRoler"] = GlobalContext.Current.LoginUserRoles.Where(a => a.Name.Equals("签证操作")).ToList().Count() > 0 ? true : false;//当前用户的角色是不是 产品录入员
                ViewData["IsProductManageRoler"] = GlobalContext.Current.LoginUserRoles.Where(a => a.Name.Equals("签证总监")).ToList().Count() > 0 ? true : false;//当前用户的是不是  产品经理

                return View("~/Views/Visa/ProductAdmin/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return RedirectToAction("Error", "Error");
            }
        }

        public ActionResult QueryProduct(VisaInformationQModel qModel)
        {
            try
            {
                qModel.OwnerCode = UserInfo.OwnerCode;
                qModel = _biz.GetInfoByCondition(qModel);
                ViewData["IsAdderRoler"] = GlobalContext.Current.LoginUserRoles.Where(a => a.Name.Equals("签证操作")).ToList().Count() > 0 ? true : false;//当前用户的角色是不是 产品录入员
                ViewData["IsProductManageRoler"] = GlobalContext.Current.LoginUserRoles.Where(a => a.Name.Equals("签证总监")).ToList().Count() > 0 ? true : false;//当前用户的是不是  产品经理

                return View("~/Views/Visa/ProductAdmin/PageList.cshtml", qModel);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        /// 复制签证产品
        /// </summary>
        /// <param name="InformationCode"></param>
        /// <returns></returns>
        public ActionResult CopyProduct(string InformationCode)
        {
            try
            {
                string infoCode = _biz.CopyProduct(InformationCode, UserInfo);
                return Json(new { code = "1", infoCode = infoCode });
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return RedirectToAction("Error", "Error");
            }
        }
        public void InitData()
        {
            //产品部门
            ViewData["TeamList"] = _teamBiz.GetTeams("6", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName, "", "", "-选择部门-");
            ViewData["VisaType"] = DictionaryTools.GetEnumsBy(Enums.VisaTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证种类-");
            ViewData["Continent"] = DictionaryTools.GetEnumsBy(Enums.ContinentEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择洲-");
            ViewData["VisaArea"] = DictionaryTools.GetEnumsBy(Enums.VisaAreaEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择领区-");
            ViewData["State"] = DictionaryTools.GetEnumsBy(Enums.VisaStateEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择产品状态-");
            ViewData["VTypeList"] = DictionaryTools.GetEnumsBy(Enums.VisaVTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证类型-");
        }

        /// <summary>
        /// 产品下线
        /// </summary>
        /// <param name="formValue"></param>
        /// <returns></returns>
        public ActionResult SetProductDownLine(VisaInformationModel formValue)
        {
            try
            {
                if (GlobalContext.Current.FunctionList.Where(a => a.Name == "签证总监").Count() > 0)
                {
                    var model = new VisaInformationModel() { InformationCode = formValue.InformationCode };
                    model.State = 6;
                    _biz.SetState(model, UserInfo, WebToolKit.GetClientIp());

                    return Json(new { Code="1", Message="Success"});
                }
                else
                {
                    return Json(new { Code = "2", Message = "NoAuthorityAccess" });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return Json(new { Code = "0", Message = ex.Message });
            }
        }

    }
}