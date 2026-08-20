using Arch.Common;
using Common.Logging;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Trip.Biz.Crm;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    public class VisaProductController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(VisaProductController));
        private ProductBiz _biz = new ProductBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();

        /// <summary>
        /// 添加产品初始化
        /// </summary>
        /// <returns></returns>
        public ActionResult AddProduct(VisaInformationModel model)
        {
            try
            {
                if (GlobalContext.Current.FunctionList.Where(a => !a.URL.IsNullOrEmpty()
                          && a.URL.Equals("/VisaProduct/AddProduct")).Count() > 0)
                {
                    model.CreateByName = GlobalContext.Current.UserInfo.Name;
                    GetInitData();
                    model.VisaType = 1;
                    model.PayTimeLimit = 48;
                    model.OwnerCode = UserInfo.OwnerCode;
                    return View("~/Views/Visa/Product/AddProduct.cshtml", model);
                }
                else
                {
                    return RedirectToAction("NoAuthorityAccess", "Base");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;//return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        /// 产品基本信息修改页面初始化
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ModifyProduct(VisaInformationModel model)
        {
            try
            {
                GetInitData();
                model = _biz.GetProductBaseInfoByCode(model.InformationCode);
                if (model.VType == 1)
                {
                    return View("~/Views/Visa/Product/ModifyProduct.cshtml", model);
                }
                else
                {
                    return View("~/Views/Visa/Product/ModifyProductTeam.cshtml", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;//return RedirectToAction("Error", "Error");
            }
        }

        /// <summary>
        /// 修改产品页面初始化
        /// </summary>
        /// <param name="QModel"></param>
        /// <returns></returns>
        public ActionResult ShowTabModifyProduct(VisaInformationQModel model)
        {
            try
            {
                model.Info = _biz.GetProductBaseInfoByCode(model.InformationCode);
                return View("~/Views/Visa/Product/TabModifyProduct.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 检查产品名称是否存在
        /// </summary>
        /// <param name="InformationName"></param>
        /// <returns></returns>
        public string CheckProductNameIsExists(string InformationName, string InformationCode)
        {
            string str = "N";
            if (_biz.CheckProductNameIsExists(InformationName, InformationCode))
                str = "Y";
            return str;
        }

        /// <summary>
        /// 保存产品
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public void SaveProduct(VisaInformationModel model)
        {
            try
            {
                string pictureUrl = AppSetting.Get("UploadFileRoot");
                if (model.ImgUrl.StartsWith(pictureUrl))
                {
                    model.ImgUrl = model.ImgUrl.Substring(pictureUrl.Length);
                }

                if (model.InformationId > 0)
                {
                    model.LivePassportArea = (model.SelectPassportArea == null) ? "" : model.SelectPassportArea.Join(",");
                    _biz.UpdateProductBaseInfo(model, UserInfo, WebToolKit.GetClientIp());
                }
                else
                {
                    // 添加产品
                    model.LivePassportArea = (model.SelectPassportArea == null) ? "" : model.SelectPassportArea.Join(",");
                    model.CreateBy = UserInfo.Code;
                    model.CreateByName = UserInfo.Name;
                    _biz.AddProductBaseInfo(model, UserInfo, WebToolKit.GetClientIp());
                }
                if (model.State == 5)
                {
                    //删除缓存
                    RemoveWebCache("1001");
                    RemoveWebCacheByKey("CacheKey=Visa|ProductDetail|RecommendModule:" + model.InformationCode);
                }
                var jsStr = new StringBuilder();
                jsStr.Append("<script type=\"text/javascript\" language=\"javascript\">");
                jsStr.Append("  alert(\"保存成功！！\");");
                jsStr.Append("  window.location.href='/VisaProduct/ShowTabModifyProduct?InformationCode=" + model.InformationCode + "&CurrentTabNum=0';");
                jsStr.Append("</script>");
                Response.Write(jsStr);
                //return RedirectToAction("ShowTabModifyProduct", new VisaInformationModel() { InformationCode = formValues.InformationCode });
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex; //return RedirectToAction("Error", "Error");
            }
        }

        private void GetInitData()
        {
            //产品部门
            ViewData["TeamList"] = _teamBiz.GetTeams("6", UserInfo.OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            //面销类型
            ViewData["InterviewTypeList"] = DictionaryTools.GetEnumsBy(Enums.InterviewTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择面试方式-");
            //是否担保
            ViewData["IsDanBaoList"] = DictionaryTools.GetEnumsBy(Enums.IsValidEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证类型-");
            //是否可以加急办理
            ViewData["IsHurryList"] = DictionaryTools.GetEnumsBy(Enums.IsValidEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证类型-");
            //签证类型
            ViewData["VisaType"] = DictionaryTools.GetEnumsBy(Enums.VisaTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证种类-");
            //所属洲
            ViewData["Continent"] = DictionaryTools.GetEnumsBy(Enums.ContinentEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择洲-");
            //所属领区
            ViewData["VisaArea"] = DictionaryTools.GetEnumsBy(Enums.VisaAreaEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择领区-");
            //产品状态
            ViewData["State"] = DictionaryTools.GetEnumsBy(Enums.VisaStateEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择产品状态-");
            //类型
            ViewData["VTypeList"] = DictionaryTools.GetEnumsBy(Enums.VisaVTypeEnum).ToSelectListFor(t => t.Key, t => t.Value, "", "", "-选择签证类型-");
        }

        /// <summary>
        ///  流程管理页面初始化
        /// </summary>
        /// <param name="InformationCode"></param>
        /// <returns></returns>
        public ActionResult ProductProcessManage(string InformationCode)
        {
            var model = _biz.GetProductBaseInfoByCode(InformationCode);

            ViewData["IsAdderRoler"] = GlobalContext.Current.LoginUserRoles.Where(a => a.Name.Equals("签证操作")).ToList().Count() > 0 ? true : false;//当前用户的角色是不是 产品录入员
            ViewData["IsProductManageRoler"] = GlobalContext.Current.LoginUserRoles.Where(a => a.Name.Equals("签证总监")).ToList().Count() > 0 ? true : false;//当前用户的是不是  产品经理

            return View("~/Views/Visa/Product/ProductProcessManage.cshtml", model);
        }

        /// <summary>
        /// 修改产品状态
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UpdateProductState(VisaInformationModel model)
        {
            try
            {
                _biz.SetState(model, UserInfo, WebToolKit.GetClientIp());
                if (model.State == 5)
                {
                    //删除缓存
                    RemoveWebCache("1001");
                }
                return Json(new { Code = "1", Message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return Json(new { Code = "0", Message = ex.Message });
            }
        }

        /// <summary>
        /// 数据检测
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SearchProductData(VisaInformationModel model)
        {
            var message = "";
            model = _biz.GetProductBaseInfoByCode(model.InformationCode);
            VisaDataModel visadata = new VisaDataModel();
            visadata.InformationCode = model.InformationCode;
            if (model.IsCategory == 1)//有分类
            {
                var categroylist = _biz.GetCategroyList(model.InformationCode);
                if (categroylist != null && categroylist.Count() > 0)//签证分类列表
                {
                    foreach (var categroy in categroylist)
                    {
                        visadata.CategoryCode = categroy.CategoryCode;//分类编码
                        if (!_biz.IsExitVisaData(visadata))
                        {
                            message += "分类:【 " + categroy.CategoryName + " 】 下面必须要录入签证材料!!</br>";
                        }
                    }
                }
                else
                {
                    message = "产品分类为空！";
                }
            }
            else
            {
                if (!_biz.IsExitVisaData(visadata))
                {
                    message += "产品必须录入签证材料!!</br>";
                }
            }
            if (message.IsNullOrEmpty())
                message += "产品录入完整!!";
            return message;
        }

        /// <summary>
        /// 保存产品的备注
        /// </summary>
        /// <param name="model"></param>
        public ActionResult SaveProductOffLineRemarks(VisaInformationModel model)
        {
            try
            {
                _biz.SetState(model, UserInfo, WebToolKit.GetClientIp());
            }
            catch (Exception ex)
            {
                return Json(new { Code = "0", Message = ex.Message });
            }

            return Json(new { Code = "1", Message = "Success" });
        }


        /// <summary>
        /// 审核产品    此环节暂时跳过 提交审核就直接通过或驳回
        /// </summary>
        /// <param name="formValue"></param>
        /// <returns></returns>
        public ActionResult AuditProduct(VisaInformationModel formValue)
        {
            try
            {
                if (GlobalContext.Current.FunctionList.Where(a => !a.URL.IsNullOrEmpty() && a.URL.Contains("/VisaProduct/AuditProduct")).Count() > 0)
                {
                    var model = new VisaInformationModel() { InformationCode = formValue.InformationCode };
                    model.State = 3;//审核中
                    _biz.SetState(model, UserInfo, WebToolKit.GetClientIp());
                    return Json(new { Code = "1", Message = "Success" });
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