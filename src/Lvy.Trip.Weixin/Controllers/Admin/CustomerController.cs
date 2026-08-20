using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Weixin.Models;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 客户管理 CRM
    /// </summary>
    [Authorize]
    public class CustomerController : AdminBaseController
    {
        private ILog logger = LogManager.GetLogger("CustomerController");

        private readonly CustomerBiz _biz = new CustomerBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly DictionaryBiz _commonBiz = new DictionaryBiz();
        public ActionResult Search(CustomerVModel vModel)
        {
            try
            {
                var teams = new List<SelectListItem>();
                var sales = new List<SelectListItem>();

                #region 根据用户角色 锁定过滤

                if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
                {
                    teams = _teamBiz.GetTeams("5", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                    if (String.IsNullOrEmpty(vModel.Customer.TeamID))
                    {
                        vModel.Customer.TeamID = teams.FirstOrDefault().Value;
                    }
                }
                else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
                {
                    //过滤页面分组显示数据.
                    teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                    if (String.IsNullOrEmpty(vModel.Customer.TeamID))
                    {
                        vModel.Customer.TeamID = teams.Where(t => t.Value != "").FirstOrDefault().Value;
                    }
                }
                else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
                {
                    vModel.Customer.SalerCode = GlobalContext.Current.UserInfo.Code;
                    teams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                    vModel.Customer.TeamID = GlobalContext.Current.LoginUserTeams.FirstOrDefault().TeamID;
                }

                // 过滤销售
                if (!String.IsNullOrEmpty(vModel.Customer.TeamID))
                    sales = _biz.GetTeamUsersByTeamId(vModel.Customer.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                else
                    sales = _biz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

                #endregion 根据用户角色 锁定过滤

                ViewBag.Teams = teams;
                ViewBag.Salers = sales;

                vModel.Customers = _biz.GetPagedList(vModel, UserInfo);
                if (Request.IsAjaxRequest())
                    return PartialView("UCSearch", vModel);
                return View(vModel);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                throw;
            }
        }

        public ActionResult Create()
        {
            CrmCustomerModel model = new CrmCustomerModel
            {
                IsDistributors = true,
                IsBranch = false,
                ChannelType = 1,
                PaymentType = 1,
                IsGroupTour = false,
                RebateInBill = true,
                HasChild = true,
            };
            InitPage();

            // 初始销售列表
            var sales = new List<SelectListItem>();
            var firstTeam = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).FirstOrDefault();
            if (firstTeam != null)
            {
                model.TeamID = firstTeam.TeamID;
                model.SalerCode = GlobalContext.Current.UserInfo.Code;
                sales = _biz.GetTeamUsersByTeamId(model.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }
            ViewBag.Salers = sales;

            return View("Edit", model);
        }

        public ActionResult Edit(string id)
        {
            var model = _biz.GetById(id);
            ViewBag.Regstration = new AccountBiz().GetCustomerRegistration(model.Code);
            ViewBag.CityList = _commonBiz.GetChildList(model.Province).ToSelectListFor(v => v.Id.ToString(), v => v.Name, model.City);
            ViewBag.CountyList = _commonBiz.GetChildList(model.City).ToSelectListFor(v => v.Id.ToString(), v => v.Name, model.County);

            #region 页面绑值

            InitPage();
            var sales = new List<SelectListItem>();
            // 过滤销售
            if (!String.IsNullOrEmpty(model.TeamID))
                sales = _biz.GetTeamUsersByTeamId(model.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            else
                sales = _biz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            ViewData["Salers"] = sales;

            #endregion 页面绑值

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Details(string code, string state, string id)
        {
            try
            {
                InWeixin(code, state);
                if (GlobalContext.Current.UserInfo == null)
                {
                    return Content("微信绑定信息获取失败.");
                }

                var model = _biz.GetById(id);
                model.ContactList = _biz.GetContactList(id);
                InitPage();
                var sales = new List<SelectListItem>();
                // 过滤销售
                if (!String.IsNullOrEmpty(model.TeamID))
                    sales = _biz.GetTeamUsersByTeamId(model.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
                else
                    sales = _biz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

                ViewBag.Salers = sales;
                return View(model);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                throw;
            }
        }

        /// <summary>
        /// 提交审核申请到微信
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult SendAuditMsg(string code)
        {
            try
            {
                var model = _biz.GetById(code);
                var leader = _teamBiz.GetTeamLeader(OwnerCode, model.TeamID);
                if (!string.IsNullOrEmpty(leader.OpenID))
                {
                    var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fsaler%2Fcustomerdetails%2F" + code + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                    var testData = new MessageData()
                    {
                        first = new TemplateDataItem("您好，您的客户提交信息审核。"),
                        keyword1 = new TemplateDataItem(model.Name),
                        keyword2 = new TemplateDataItem("未填写"),
                        keyword3 = new TemplateDataItem("未填写"),
                        keyword4 = new TemplateDataItem(DateTime.Now.ToShortDateString()),
                        remark = new TemplateDataItem("审核客户信息后请及时确认。")
                    };

                    var result = TemplateApi.SendTemplateMessage(accessToken, leader.OpenID, "dbtBNSrwDX2qG4qXu9pAGTj172O5_ZqvVkPxQlvnpDw", url, testData);
                }
                return Content("yes");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
                //throw;
            }
        }

        /// <summary>
        /// 客户审核
        /// </summary>
        /// <param name="code"></param>
        /// <param name="remark"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult AuditCust(string code, string remark, int state = 0)
        {
            try
            {
                var model = _biz.GetById(code);
                model.CustomerState = state;//设置为审核状态
                int i = _biz.Update(model);

                if (i > 0)
                {
                    // 如果审核通过 下属联系人同部门的审核通过
                    if (state == 1)
                    {
                        _accountBiz.UpdateContactState(model.TeamID, model.Code, 1);
                    }

                    // 记录日志
                    LogBiz.WriteCustomerLog(UserInfo.OwnerCode, model.Code, "", model.TeamID, GlobalContext.Current.UserInfo.Code, (state == 1 ? "客户审核通过" : "客户审核不通过"));

                    // 发消息销售
                    var sales = _accountBiz.GetAccountCustomer(model.SalerCode);
                    if (!string.IsNullOrEmpty(sales.OpenID))
                    {
                        var first = string.Format("客户名称：{0}", model.Name);
                        var param1 = (state == 1 ? "客户审核通过" : "客户审核不通过");

                        var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                        var testData = new SendMessageData()
                        {
                            first = new TemplateDataItem(first),
                            keyword1 = new TemplateDataItem(param1),
                            keyword2 = new TemplateDataItem(DateTime.Now.ToString()),
                            remark = new TemplateDataItem(remark)
                        };
                        string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fsaler%2Fcustomerdetails%2F" + code + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";
                        SendTemplateMessageResult result = TemplateApi.SendTemplateMessage(accessToken, sales.OpenID, "H4wr3tCcSDvlVOR9J9cbomJjgajRyYzcrVJX_x3YLVA", url, testData);

                        //if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
                        //    return "1";
                        //else
                        //    return "0";
                    }
                }

                return Json(new { Code = i }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                throw;
            }
        }

        /// <summary>
        /// 保存客户
        /// </summary>
        /// <returns></returns>
        public ActionResult Save(CrmCustomerModel model)
        {
            int result = 0;

            if (Request.Form["ParentName"].Trim().IsNullOrEmpty())
            {
                model.ParentCode = null;
            }
            if (model.Code.IsNullOrEmpty())
            {
                model.Code = DBTools.GetSeqNo("CrmCustomer");
                model.IsOwner = false;
                model.ModifiedBy = UserInfo.Code;
                model.ModifiedTime = DateTime.Now;
                model.IsValid = 1;
                model.CustomerState = 0;// 已审核
                model.OwnerCode = OwnerCode;
                model.ActiveState = 0;
                model.CreatedTime = DateTime.Now;
                model.ReceiveDate = DateTime.Now;
                model.Rank = 1;//客户等级
                result = Convert.ToInt32(_biz.Add(model));
            }
            else
            {
                var obj = _biz.GetById(model.Code);
                obj.Name = model.Name;
                obj.FastCode = model.FastCode.IsNullOrEmpty() ? "" : model.FastCode;
                //obj.ShortName = model.ShortName;
                obj.ChannelType = model.ChannelType;
                obj.Head = model.Head;
                obj.Mobile = model.Mobile;
                obj.Phone = model.Phone;
                obj.Province = model.Province;
                obj.City = model.City;
                obj.County = model.County;
                obj.Address = model.Address;
                obj.CreditLine = model.CreditLine;
                obj.PaymentType = model.PaymentType;
                if (GlobalContext.Current.UserInfo.OwnerCode != model.Code) // 如果是系统用户，不用修改客户类型
                    obj.IsOwner = model.IsOwner;
                obj.IsDistributors = model.IsDistributors;
                obj.HasChild = model.HasChild;
                obj.IsSupplier = model.IsSupplier;
                obj.IsBranch = model.IsBranch;
                obj.IsGroupTour = model.IsGroupTour;
                obj.RebateInBill = model.RebateInBill;
                obj.SalerCode = model.SalerCode;
                obj.Remarks = model.Remarks;
                obj.ModifiedBy = UserInfo.Code;
                obj.ModifiedTime = DateTime.Now;
                obj.ParentCode = model.ParentCode;
                obj.TeamID = model.TeamID;
                obj.TaxNumber = model.TaxNumber;

                result = _biz.Update(obj);
            }

            return Json(new { Result = result, CustomerCode = model.Code });
        }

        /// <summary>
        /// 验证客户名称是否存在
        /// </summary>
        /// <param name="customerName"></param>
        /// <returns></returns>
        public ActionResult CheckName(string customerName, string code)
        {
            // 取得非当前客户，名称相同的客户，如果有那么重复
            var customerModel = _biz.GetByCustomerName(GlobalContext.Current.UserInfo.OwnerCode, customerName, code);
            var result = customerModel == null ? 0 : 1;
            return Json(new { result = result });
        }

        protected void InitPage()
        {
            ViewBag.PaymentTypes = DictionaryTools.GetEnumsBy(Enums.PaymentTypeEnum).ToSelectListFor();
            ViewBag.ChannelTypes = DictionaryTools.GetEnumsBy(Enums.CustomerChannelEnum).ToSelectListFor();
            ViewBag.BusinessItems = DictionaryTools.GetEnumsBy(Enums.ProductAllTypeEnum).ToSelectListFor();
            ViewBag.ProvinceList = _commonBiz.GetProvinceList().ToSelectListFor(v => v.Id.ToString(), v => v.Name);

            var teams = new List<SelectListItem>();
            var IsLeader = 1;
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                teams = _teamBiz.GetTeams("5", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                //过滤页面分组显示数据.
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                teams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                IsLeader = 0;
            }
            ViewBag.Teams = teams;
            ViewBag.IsLeader = IsLeader;
        }

        /// <summary>
        /// 客户下拉使用 Select2, 包含未审核
        /// </summary>
        /// <param name="fromCustomer"></param>
        /// <param name="keyword"></param>
        /// <param name="hasChild"></param>
        /// <returns></returns>
        public ActionResult GetCustomerSelect2(string fromCustomer, string keyword, bool hasChild = false)
        {
            IList<CrmCustomerModel> customers = null;

            customers = DictionaryTools.GetCachedCustomerDict().Values
                .Where(a => a.OwnerCode == GlobalContext.Current.UserInfo.OwnerCode && a.IsValid == 1).ToList();
            if (hasChild)
            {
                customers = customers.Where(a => a.HasChild == hasChild).ToList();
            }
            if (keyword.IsNullOrEmpty())
            {
                customers = customers.OrderByDescending(a => a.Code).Take(12).ToList();
            }
            if (!keyword.IsNullOrEmpty())
            {
                customers = customers
                    .Where(
                        a =>
                        ((a.FastCode != null && a.FastCode.ToLower().Contains(keyword.ToLower())) || (a.Name != null && a.Name.Contains(keyword)))).Take(15).ToList();
            }

            var model = new
            {
                incomplete_results = "false",
                items = customers,
                total_count = customers.Count
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCityListByProvinceId(string provinceId)
        {
            return Json(_commonBiz.GetChildList(provinceId));
        }

        public ActionResult GetCountyListByCityId(string cityId)
        {
            return Json(_commonBiz.GetChildList(cityId));
        }
    }
}