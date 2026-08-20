using Arch.Common;
using Arch.Common.Utils;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Common;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Crm
{
    /// <summary>
    /// 客户（供应商和分销商）功能控制器
    ///
    /// 权限设置  销售总监产所有客户  销售组长 销售组所以客户  销售  只能查看编辑自己的客户
    /// 计调无权使用， 供应商专人操作
    /// </summary>
    public class CustomerController : BaseController
    {
        private ILog logger = LogManager.GetLogger("CustomerController");

        private readonly CustomerBiz _biz = new CustomerBiz();
        private readonly AccountBiz _accountBiz = new AccountBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly LogBiz _logBiz = new LogBiz();
        private readonly DictionaryBiz _commonBiz = new DictionaryBiz();

        /// <summary>
        /// 查询客户
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [LvyAuth]
        public virtual ActionResult Search(CustomerVModel vModel)
        {
            // 取得查询分页条件
            var q = (CustomerVModel)CacheContext.Current.Get(Consts.PageCustomerController + GlobalContext.Current.UserInfo.Code);
            if (q != null && vModel.FirstTime)
                vModel = q;

            ViewData["PaymentTypes"] = DictionaryTools.GetEnumsBy(Enums.PaymentTypeEnum).ToSelectListFor();
            ViewData["CustomerType"] = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key = "1",Value = "分销商"},
                                        // new KeyValueBean{Key="2",Value="供应商"},
                                         new KeyValueBean{Key="3",Value="门店"}
                                     }.ToSelectListFor();
            ViewData["CustomerState"] = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key = "0",Value = "未审核"},
                                         new KeyValueBean{Key="1",Value="已审核"},
                                         new KeyValueBean{Key="2",Value="审核不通过"}
                                     }.ToSelectListFor();

            var teams = new List<SelectListItem>();
            var sales = new List<SelectListItem>();

            #region 根据用户角色 锁定过滤

            if (GlobalContext.Current.IsSysAdmin || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                vModel.IsLeader = 1;
                teams = _teamBiz.GetBalanceTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (String.IsNullOrEmpty(vModel.Customer.TeamID))
                {
                    vModel.Customer.TeamID = teams.FirstOrDefault().Value;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                vModel.IsLeader = 1;
                //过滤页面分组显示数据.
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (String.IsNullOrEmpty(vModel.Customer.TeamID))
                {
                    vModel.Customer.TeamID = teams.Where(t => t.Value != "").FirstOrDefault().Value;
                    vModel.Customer.SalerCode = GlobalContext.Current.UserInfo.Code;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                vModel.IsLeader = 0;
                vModel.Customer.SalerCode = GlobalContext.Current.UserInfo.Code;
                teams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                vModel.Customer.TeamID = GlobalContext.Current.LoginUserTeams.FirstOrDefault().TeamID;
            }

            #endregion 根据用户角色 锁定过滤

            // 过滤销售
            if (!String.IsNullOrEmpty(vModel.Customer.TeamID))
                sales = _biz.GetTeamUsersByTeamId(vModel.Customer.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            else
                sales = _biz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            ViewBag.Teams = teams;
            ViewBag.Salers = sales;

            // 保存查询分页条件
            CacheContext.Current.Add(Consts.PageCustomerController + GlobalContext.Current.UserInfo.Code, vModel, Consts.OutputCacheDuration2);

            vModel.Customers = _biz.GetPagedList(vModel, UserInfo);
            vModel.FirstTime = false;
            if (Request.IsAjaxRequest())
                return PartialView("UCSearch", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 创建客户
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Create()
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
            var firstTeam = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1 || t.DepartCode == 7).FirstOrDefault();
            if (firstTeam != null)
            {
                model.TeamID = firstTeam.TeamID;
                model.SalerCode = GlobalContext.Current.UserInfo.Code;
                sales = _biz.GetTeamUsersByTeamId(model.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            }
            ViewBag.Salers = sales;

            return View(model);
        }

        /// <summary>
        /// 添加客户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(CrmCustomerModel model, string[] BusinessPermitSelect)
        {
            if (Request.Form["ParentName"].Trim().IsNullOrEmpty())
            {
                model.ParentCode = null;
            }
            model.Code = DBTools.GetSeqNo("CrmCustomer");
            model.IsOwner = false;
            model.ModifiedBy = UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            model.IsValid = 1;
            model.CustomerState = 0;// 已审核
            model.OwnerCode = OwnerCode;
            model.ActiveState = 2;
            model.CreatedTime = DateTime.Now;
            model.ReceiveDate = DateTime.Now;
            model.Rank = 1;//客户等级
            model.BusinessPermit = (BusinessPermitSelect == null ? "" : string.Join(",", BusinessPermitSelect));
            int result = Convert.ToInt32(_biz.Add(model));

            LogBiz.WriteCustomerLog(UserInfo.OwnerCode, model.Code, "", model.TeamID, GlobalContext.Current.UserInfo.Code, "客户新建.");

            return Json(new { Result = result, CustomerCode = model.Code });
        }

        /// <summary>
        /// 编辑客户
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Edit(string code)
        {
            var model = _biz.GetById(code);
            ViewBag.Regstration = new AccountBiz().GetCustomerRegistration(model.Code);
            ViewBag.CityList = _commonBiz .GetChildList(model.Province).ToSelectListFor(v => v.Id.ToString(), v => v.Name, model.City);
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
            ViewData["CheckAlert"] = "";

            #endregion 页面绑值

            ViewData["Alert"] = CheckCustomer(model);

            return View(model);
        }

        private string CheckCustomer(CrmCustomerModel model)
        {
            string alert = "";
            if (model.PaymentType > 1)  // 非现结客户
            {
                if (model.CreditLine == 0)
                {
                    alert = "非现结信用额度不能为0，";
                }

                var list = _biz.GetValidContract(model.Code);
                if (list.Count == 0)
                {
                    alert += "非现结客户需要上传有效代理合同，";
                }
            }

            return alert;
        }

        /// <summary>
        /// 保存客户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(CrmCustomerModel model, string[] BusinessPermitSelect)
        {
            if (Request.Form["ParentName"].Trim().IsNullOrEmpty())
            {
                model.ParentCode = null;
            }
            var entity = _biz.GetById(model.Code);
            if (entity.CustomerState != 1)  // 审核后不能修改
            {
                entity.Name = model.Name;
                entity.FastCode = model.FastCode.IsNullOrEmpty() ? "" : model.FastCode;
                entity.ChannelType = model.ChannelType;
                entity.Head = model.Head;
                entity.Mobile = model.Mobile;
                entity.Phone = model.Phone;
                entity.Province = model.Province;
                entity.City = model.City;
                entity.County = model.County;
                entity.Address = model.Address;
                entity.CreditLine = model.CreditLine;
                entity.PaymentType = model.PaymentType;
                if (GlobalContext.Current.OwnerCode != model.Code) // 如果是系统用户，不用修改客户类型
                    entity.IsOwner = model.IsOwner;
                entity.IsDistributors = model.IsDistributors;
                entity.HasChild = model.HasChild;
                entity.IsSupplier = model.IsSupplier;
                entity.IsBranch = model.IsBranch;
                entity.IsGroupTour = model.IsGroupTour;
                entity.RebateInBill = model.RebateInBill;
                entity.SalerCode = model.SalerCode;
                entity.ParentCode = model.ParentCode;
                entity.TeamID = model.TeamID;
                entity.TaxNumber = model.TaxNumber;
                entity.ImportTeam = model.ImportTeam;
                entity.BusinessPermit = (BusinessPermitSelect == null ? default(string) : string.Join(",", BusinessPermitSelect));
            }
            entity.Remarks = model.Remarks;
            entity.ModifiedBy = UserInfo.Code;
            entity.ModifiedTime = DateTime.Now;
            _biz.Update(entity);

            // clear cache
            CacheContext.Current.Remove(Consts.CustomerStrDic);

            return RedirectToAction("Search");
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <returns></returns>
        public ActionResult SetValidState(string code)
        {
            var obj = _biz.GetById(code);

            obj.IsValid = obj.IsValid == 1 ? 0 : 1;
            obj.ModifiedBy = UserInfo.Code;
            obj.ModifiedTime = DateTime.Now;
            _biz.Update(obj);

            LogBiz.WriteCustomerLog(UserInfo.OwnerCode, code, "", obj.TeamID, GlobalContext.Current.UserInfo.Code, "客户设置为" + (obj.IsValid == 1 ? "无效" : "有效"));

            return RedirectToAction("Search");
        }

        /// <summary>
        /// 提出审核到组长微信
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult SendAuditMsg(string code)
        {
            try
            {
                // 客户所属组 审核
                var model = _biz.GetById(code);
                var leader = _teamBiz.GetTeamLeader(UserInfo.OwnerCode, model.TeamID); //
                if (leader != null && !string.IsNullOrEmpty(leader.OpenID))
                {
                    var first = "您好，您的销售提交了新客户审核。";
                    var param1 = model.Name;
                    var param2 = "";
                    var param3 = "";
                    var param4 = DateTime.Now.ToString();
                    var remark = "申请人:" + GlobalContext.Current.UserInfo.Name;
                    SendMessagClient.SendTemplateMessage(leader.OpenID, "dbtBNSrwDX2qG4qXu9pAGTj172O5_ZqvVkPxQlvnpDw", first, param1, param2, param3, param4, code, remark);
                }

                return Content("yes");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
                //throw;
            }
        }

        #region 自定义函数

        /// <summary>
        /// 验证客户名称是否存在
        /// </summary>
        /// <param name="customerName"></param>
        /// <returns></returns>
        public ActionResult CheckCustomerName(string customerName, string code)
        {
            var ret = new { Code = "0", CustomerCode = "", IsValid = 1 };
            // 取得非当前客户，名称相同的客户，如果有那么重复
            var customer = _biz.GetByCustomerName(GlobalContext.Current.OwnerCode, customerName, code);
            if (customer != null)
                ret = new { Code = "1", CustomerCode = customer.Code, IsValid = customer.IsValid };
            return Json(ret);
        }

        /// <summary>
        /// 获取所属客户
        /// 页面下拉列表使用
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetCustomersPopup(string team, string keyword, bool hasChild = false)
        {
            var customers = DictionaryTools.GetCachedCustomerDict().Values
                 .Where(a => a.OwnerCode == GlobalContext.Current.OwnerCode && a.IsValid == 1 && a.HasChild == hasChild);
            if (!team.IsNullOrEmpty())
                customers = customers.Where(t => t.TeamID == team);
            if (keyword.IsNullOrEmpty())
            {
                customers = customers.OrderByDescending(a => a.Code).Take(12);
            }
            if (!keyword.IsNullOrEmpty())
            {
                customers = customers
                    .Where(
                        a =>
                        ((a.FastCode != null && a.FastCode.ToLower().Contains(keyword.ToLower())) || (a.Name != null && a.Name.Contains(keyword)))).Take(15);
            }

            return Json(customers.ToList(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 所有有效的包含未审核客户（Select2使用）
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="hasChild"></param>
        /// <returns></returns>
        public ActionResult GetCustomerSelect2(string team, string keyword, int page = 0, int size = 10, bool hasChild = false)
        {
            var customers = DictionaryTools.GetCachedCustomerDict().Values
                                               .Where(a => a.OwnerCode == GlobalContext.Current.OwnerCode && a.IsValid == 1);

            if (!String.IsNullOrEmpty(team))
                customers = customers.Where(a => a.TeamID == team);
            if (hasChild)
                customers = customers.Where(a => a.HasChild == hasChild);

            if (!String.IsNullOrEmpty(keyword))
            {
                //customers = customers.Where(a =>((a.FastCode != null && a.FastCode.ToLower().Contains(keyword.ToLower())) || (a.Name != null && a.Name.Contains(keyword))));
                customers = customers.Where(a => a.Name != null && a.Name.Contains(keyword));
            }

            int total = customers.Count();
            var list = (from vv in customers.OrderByDescending(a => a.Code)
                        select new
                        {
                            id = vv.Code,
                            text = vv.Name
                        }).Skip(page * size).Take(size).ToList();

            var model = new
            {
                rows = list,
                total = total
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得所有供应商
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetSupplierPoppup(string keyword)
        {
            IList<CrmCustomerModel> customers = null;

            customers = DictionaryTools.GetCachedCustomerDict().Values.Where(a => a.OwnerCode == GlobalContext.Current.OwnerCode && a.IsSupplier && a.IsValid == 1).ToList();
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

            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得所有供应商（Select2使用）
        /// </summary>
        /// <param name="fromCustomer"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetSupplierSelect2(string fromCustomer, string keyword)
        {
            IList<CrmCustomerModel> customers = null;

            customers = DictionaryTools.GetCachedCustomerDict().Values.Where(a => a.OwnerCode == GlobalContext.Current.OwnerCode && a.IsSupplier && a.IsValid == 1).ToList();
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

        /// <summary>
        /// 取得所有客户（分页）  // 废弃
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetCustomersPopup2(string keyword, int pageIndex, int pageSize)
        {
            var customers = DictionaryTools.GetCachedCustomerDict().Values.Where(a => a.OwnerCode == GlobalContext.Current.OwnerCode && a.IsValid == 1);

            if (!keyword.IsNullOrEmpty())
            {
                customers = customers.Where(a => ((a.FastCode != null && a.FastCode.ToLower().Contains(keyword.ToLower())) || (a.Name != null && a.Name.Contains(keyword))));
            }

            var list = customers.OrderByDescending(a => a.Code).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            var model = new
            {
                List = list,
                ReturnMsg = "0000",
                TotalCount = customers.Count()
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得所有平台类型客户
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPinCompany()
        {
            var list = DictionaryTools.GetCachedCustomerDict().Values.Where(a => a.IsValid == 1 && a.ChannelType == 2).ToList();
            return Json(list);
        }

        #endregion 自定义函数

        /// <summary>
        /// 初始化页面
        /// </summary>
        protected override void InitPage()
        {
            ViewBag.PaymentTypes = DictionaryTools.GetEnumsBy(Enums.PaymentTypeEnum).ToSelectListFor();
            ViewBag.ChannelTypes = DictionaryTools.GetEnumsBy(Enums.CustomerChannelEnum).ToSelectListFor();
            ViewBag.ProvinceList = _commonBiz.GetProvinceList().ToSelectListFor(v => v.Id.ToString(), v => v.Name);

            var teams = new List<SelectListItem>();
            var IsLeader = 1;
            if (GlobalContext.Current.IsSysAdmin || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                teams = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                //过滤页面分组显示数据.
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                teams = GlobalContext.Current.LoginUserTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                IsLeader = 0;
            }
            ViewBag.Teams = teams;
            ViewBag.IsLeader = IsLeader;
            ViewBag.ImportTeams = _teamBiz.GetTeams("1", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            ViewBag.SupplyProdutType = DictionaryTools.GetEnumsBy(Enums.ProductAllTypeEnum).ToSelectListForNoDefualt();
        }

        #region 客户选择框

        /// <summary>
        /// 选择客户
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SelectCustomer(SelectCustomerVModel vModel)
        {
            if (null == vModel.PagedCustomers) vModel.PagedCustomers = new PagedList<CrmCustomerModel>();
            vModel.PagedCustomers.PageSize = 12;
            vModel.PagedCustomers = _biz.SelectCustomerSource(vModel, OwnerCode);

            return PartialView("UCSelectCustomer", vModel);
        }

        /// <summary>
        /// 选择客户列表
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SelectCustomerList(SelectCustomerVModel vModel)
        {
            if (null == vModel.PagedCustomers) vModel.PagedCustomers = new PagedList<CrmCustomerModel>();
            vModel.PagedCustomers.PageSize = 12;
            vModel.PagedCustomers = _biz.SelectCustomerSource(vModel, OwnerCode);

            return PartialView("UCSelectCustomerList", vModel);
        }

        #endregion 客户选择框

        public ActionResult GetCityListByProvinceId(string provinceId)
        {
            return Json(_commonBiz.GetChildList(provinceId));
        }

        public ActionResult GetCountyListByCityId(string cityId)
        {
            return Json(_commonBiz.GetChildList(cityId));
        }

        #region 客户联系人信息操作方法

        public ActionResult ContactInfo(string code, AccountVModel vModel)
        {
            if (string.IsNullOrEmpty(vModel.Account.CustomerCode))
            {
                vModel.Account.CustomerCode = code;
            }

            vModel.Accounts = _biz.GetContactPagedList(vModel);

            if (Request.IsAjaxRequest())
                return PartialView("UCContactInfoList", vModel);
            return View(vModel);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="code">客户Code</param>
        /// <returns></returns>
        public ActionResult DeleteContactInfo(string contactId, string code)
        {
            _accountBiz.DeleteContact(contactId);

            AccountVModel vModel = new AccountVModel();
            vModel.Account.CustomerCode = code;
            vModel.Accounts = _biz.GetContactPagedList(vModel);
            return PartialView("UCContactInfoList", vModel);
        }

        /// <summary>
        /// 联系人添加/修改页面
        /// </summary>
        /// <param name="code">客户编码</param>
        /// <param name="contactId">联系人编码 编辑代入</param>
        /// <returns></returns>
        public ActionResult UCCreateContactInfo(string code, string contactId)
        {
            // 初始数据
            AccountVModel vModel = new AccountVModel();
            if (!string.IsNullOrEmpty(contactId))  // 修改联系人
            {
                vModel.Account = _accountBiz.GetById(contactId);
            }
            else                                   // 添加联系人
            {
                var c = _biz.GetById(code);
                vModel.Account.TeamID = c.TeamID;
                vModel.Account.CustomerCode = code;
                vModel.Account.SalerCode = c.SalerCode;  // 默认负责客户的销售
                vModel.Account.Sex = 1;
            }

            // ---------------------------------------------------------------------------------------------------
            var teams = new List<SelectListItem>();
            var sales = new List<SelectListItem>();

            // 根据用户角色 锁定过滤
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                vModel.IsLeader = 1;
                var temp = _teamBiz.GetSalesTeams(OwnerCode);
                teams = temp.ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))
            {
                vModel.IsLeader = 1;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                vModel.IsLeader = 0;
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                vModel.Account.SalerCode = GlobalContext.Current.UserInfo.Code;
            }

            ViewBag.Teams = teams;
            ViewBag.Salers = _biz.GetTeamUsersByTeamId(vModel.Account.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            // ---------------------------------------------------------------------------------------------------------

            vModel.SexBeans = DictionaryTools.GetEnumsBy(Enums.SexEnum);

            return View(vModel);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult AddContactInfo(AccountVModel vModel)
        {
            var customer = _biz.GetById(vModel.Account.CustomerCode);
            int result = 1;

            if (string.IsNullOrEmpty(vModel.Account.Code))   // 新增
            {
                vModel.Account.ModifiedTime = DateTime.Now;
                vModel.Account.Code = DBTools.GetSeqNo("CrmAccount");
                vModel.Account.IsValid = 1;
                vModel.Account.OwnerCode = GlobalContext.Current.OwnerCode;
                if (!string.IsNullOrEmpty(vModel.Account.Pwd))
                    vModel.Account.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
                if (customer.CustomerState == 1 && customer.TeamID == vModel.Account.TeamID)  // 如果客户审核过 那么添加的联系人默认审核通过 (条件是同一销售组)
                    vModel.Account.SalerState = 1;

                var rr = new List<String>();
                if (customer.IsDistributors)
                {
                    rr.Add("5");
                }
                if (customer.IsSupplier)
                {
                    rr.Add("4");
                }
                _accountBiz.AddContact(vModel.Account, rr.ToArray());
            }
            else    // 更新
            {
                var model = _accountBiz.GetById(vModel.Account.Code);
                model.Name = vModel.Account.Name;
                model.Email = vModel.Account.Email;
                model.Phone = vModel.Account.Phone;
                model.Mobile = vModel.Account.Mobile;
                model.Sex = vModel.Account.Sex;
                model.Remarks = vModel.Account.Remarks;
                model.TeamID = vModel.Account.TeamID;
                model.SalerCode = vModel.Account.SalerCode;
                if (!string.IsNullOrEmpty(vModel.Account.Pwd))
                    model.Pwd = Toolkit.Security.ToEncrypt(vModel.Account.Pwd);
                if (customer.CustomerState == 1 && customer.TeamID == vModel.Account.TeamID)  // 如果客户审核过 那么添加的联系人默认审核通过 (条件是同一销售组)
                    model.SalerState = 1;

                var rr = new List<String>();
                if (customer.IsDistributors)
                {
                    rr.Add("5");  // 分销商
                }
                if (customer.IsSupplier)
                {
                    rr.Add("4");  // 供应商
                }
                result = _accountBiz.UpdateContact(model, rr.ToArray());
            }

            return Content(result.ToString());
        }

        [HttpPost]
        public ActionResult GetTeamUserByTeamId(string teamId)
        {
            var sales = _biz.GetTeamUsersByTeamId(teamId, OwnerCode);

            #region 根据用户角色 锁定过滤

            int IsBoss = 0;
            int GroupLeader = 0;

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
                IsBoss = 1;
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))//
            {
                GroupLeader = 1;
            }
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                if (IsBoss == 0 && GroupLeader == 0)
                {
                    sales = _biz.GetTeamSales(OwnerCode).Where(a => a.Code == GlobalContext.Current.UserInfo.Code).ToList();
                }
            }

            #endregion 根据用户角色 锁定过滤

            return Json(sales, JsonRequestBehavior.AllowGet);
        }

        #endregion 客户联系人信息操作方法

        #region 折扣规则

        public ActionResult ProducPolicy(string code, CustomerPolicyVModel vModel)
        {
            if (string.IsNullOrEmpty(vModel.CustomerCode))
            {
                vModel.CustomerCode = code;
            }
            ViewBag.RebateBeans = DictionaryTools.GetEnumsBy(Enums.RebateEnum);
            vModel.Items = _biz.GetPolicyList(vModel.CustomerCode);

            if (Request.IsAjaxRequest())
                return PartialView("UCPolicyList", vModel);
            return View(vModel);
        }

        public ActionResult UCCreatePolicy(string code, long policyId)
        {
            CustomerPolicyVModel vModel = new CustomerPolicyVModel();
            vModel.PolicyEntity.CustomerCode = code;
            vModel.RebateBeans = DictionaryTools.GetEnumsBy(Enums.RebateEnum);
            ViewBag.BusinessItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum);

            if (policyId != 0)
            {
                vModel.PolicyEntity = _biz.GetPolicyById(policyId);
                if (!string.IsNullOrEmpty(vModel.PolicyEntity.Code))
                    vModel.PolicyEntity.RegionName = DictionaryBiz.GetCacheDests().Where(a => a.ParentStr == vModel.PolicyEntity.Code).FirstOrDefault().Name;
            }

            return View(vModel);
        }

        public ActionResult AddPolicy(CustomerPolicyVModel vModel)
        {
            long contactId = vModel.PolicyEntity.Id;
            object result = 0;
            if (contactId == 0)
            {
                result = _biz.AddPolicy(vModel.PolicyEntity);
            }
            else
            {
                var model = _biz.GetPolicyById(vModel.PolicyEntity.Id);
                model.ProductType = vModel.PolicyEntity.ProductType;
                model.RebateType = vModel.PolicyEntity.RebateType;
                model.Percent = vModel.PolicyEntity.Percent;
                model.MaxAmount = vModel.PolicyEntity.MaxAmount;
                model.Code = vModel.PolicyEntity.Code;

                result = _biz.UpdatePolicy(model);
            }
            return Content(result.ToString());
        }

        public ActionResult DeletePolicy(long policyId, string code)
        {
            _biz.DeletePolicy(policyId);

            CustomerPolicyVModel vModel = new CustomerPolicyVModel();
            vModel.CustomerCode = code;
            vModel.Items = _biz.GetPolicyList(vModel.CustomerCode);
            return PartialView("UCPolicyList", vModel);
        }

        #endregion 折扣规则

        #region 操作日志

        public ActionResult LogList(string code, CustomerVModel vModel)
        {
            if (string.IsNullOrEmpty(vModel.Customer.Code))
            {
                vModel.Customer.Code = code;
            }
            ViewBag.RebateBeans = DictionaryTools.GetEnumsBy(Enums.RebateEnum);
            vModel.LogList = _logBiz.GetPolicyList(vModel.LogList.PageIndex, vModel.LogList.PageSize, vModel.Customer.Code);

            if (Request.IsAjaxRequest())
                return PartialView("UCLogList", vModel);
            return View(vModel);
        }

        #endregion 操作日志

        #region 客户领用进页面

        public ActionResult UCSelectUse(CustomerVModel vModel)
        {
            if (vModel == null)
                vModel = new CustomerVModel();

            ViewBag.PaymentTypes = DictionaryTools.GetEnumsBy(Enums.PaymentTypeEnum).ToSelectListFor();

            var teams = new List<SelectListItem>();
            var sales = new List<SelectListItem>();

            #region 根据用户角色 锁定过滤

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                teams = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (String.IsNullOrEmpty(vModel.Customer.TeamID))
                {
                    vModel.Customer.TeamID = teams.FirstOrDefault().Value;
                }
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长")
                || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                //过滤页面分组显示数据.
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (String.IsNullOrEmpty(vModel.Customer.TeamID))
                {
                    vModel.Customer.TeamID = teams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }

            #endregion 根据用户角色 锁定过滤

            // 过滤销售
            if (!String.IsNullOrEmpty(vModel.Customer.TeamID))
                sales = _biz.GetTeamUsersByTeamId(vModel.Customer.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            else
                sales = _biz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            ViewBag.Teams = teams;
            ViewBag.Salers = sales;
            vModel.Customers = _biz.UCSelectUse(vModel, UserInfo);

            if (Request.IsAjaxRequest())
                return PartialView("UCSelectUse", vModel);
            return View(vModel);
        }

        #endregion 客户领用进页面

        #region 点击领用修改对应销售和客户状态

        /// <summary>
        /// 领用
        ///
        /// 规则：客户领用记录最后一个不同就可以领用
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult Lingyon(string code)
        {
            CustomerVModel vModel = new CustomerVModel();
            var ca = _biz.GetLastHold(code);

            if (ca == null)//如果是第一次被领用进此方法
            {
                CustomerHoldModel cao = new CustomerHoldModel();
                cao.CustomerCode = code;
                cao.SalerCode = UserInfo.Code;
                cao.HoldDate = DateTime.Now;
                cao.CreateDate = DateTime.Now;
                _biz.AddCustomerHold(cao);//新增一条数据到领用信息表

                //修改客户表状态和销售人
                _biz.UpdateHold(code, UserInfo.Code);

                vModel.Customers = _biz.UCSelectUse(vModel, UserInfo);
                return PartialView("UCSelectUseShaList", vModel);
            }
            else if (ca.SalerCode != UserInfo.Code)
            {
                CustomerHoldModel cao = new CustomerHoldModel();
                cao.CustomerCode = code;
                cao.SalerCode = UserInfo.Code;
                cao.HoldDate = DateTime.Now;
                cao.CreateDate = DateTime.Now;
                _biz.AddCustomerHold(cao);//新增一条数据到领用信息表

                //修改客户表状态和销售人
                _biz.UpdateHold(code, UserInfo.Code);

                vModel.Customers = _biz.UCSelectUse(vModel, UserInfo);
                return PartialView("UCSelectUseShaList", vModel);
            }
            else
            {
                return Json(new { Code = 1 });
            }
        }

        #endregion 点击领用修改对应销售和客户状态

        #region 客户附件管理

        public ActionResult Uploading(string code, CustomerFileVModel vModel)
        {
            if (vModel == null)
                vModel = new CustomerFileVModel();
            vModel.CustomerFile.CustomerCode = code;
            vModel.FilePageList = _biz.Uploadings(vModel);
            if (Request.IsAjaxRequest())
                return PartialView("Uploading", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 进入上传页面
        /// </summary>
        /// <param name="CustCode"></param>
        /// <returns></returns>
        public ActionResult FileCreate(string CustCode)
        {
            CustomerFileVModel vModel = new CustomerFileVModel();
            vModel.CustomerFile.CustomerCode = CustCode;
            return View(vModel);
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult Uploadfiles(CustomerFileVModel vModel)
        {
            CustomerFileModel model = new CustomerFileModel();
            model.CustomerCode = vModel.CustomerFile.CustomerCode;
            model.Subject = vModel.CustomerFile.Subject;
            model.StratDate = vModel.CustomerFile.StratDate;
            model.EndDate = vModel.CustomerFile.EndDate;
            model.Remark = vModel.CustomerFile.Remark;
            model.IsValid = 1;
            model.CreatedBy = GlobalContext.Current.UserInfo.Code;
            model.CreatedTime = DateTime.Now;

            string file_name = "";
            string logoPath = "";
            HttpPostedFileBase file = Request.Files["LogoFile"];
            logoPath = UploadingFile(file, vModel.CustomerFile.CustomerCode, ref file_name);
            if (file != null && file.ContentLength > 0)
            {
                model.FileName = file_name;
                model.FilePath = logoPath;
            }
            _biz.Zen(model);

            return RedirectToAction("Uploading", new { code = vModel.CustomerFile.CustomerCode });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult DeleteUploadfile(int Id, string code)
        {
            _biz.DeleteUploading(Id);

            CustomerFileVModel vModel = new CustomerFileVModel();
            vModel.CustomerFile.CustomerCode = code;
            vModel.FilePageList = _biz.Uploadings(vModel);
            return RedirectToAction("Uploading", new { code = vModel.CustomerFile.CustomerCode });
        }

        /// <summary>
        /// 进入修改页面
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="CustCode"></param>
        /// <returns></returns>
        public ActionResult UploadingEdit(int Id, string CustCode)
        {
            CustomerFileVModel vModel = new CustomerFileVModel();
            vModel.CustomerFile = _biz.UploadingId(Id);
            vModel.CustomerFile.CustomerCode = CustCode;
            return View(vModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult UploadingEditUpdate(CustomerFileVModel vModel)
        {
            CustomerFileModel model = new CustomerFileModel();
            model = _biz.UploadingId(vModel.CustomerFile.Id);
            model.CustomerCode = vModel.CustomerFile.CustomerCode;
            model.Subject = vModel.CustomerFile.Subject;
            model.StratDate = vModel.CustomerFile.StratDate;
            model.EndDate = vModel.CustomerFile.EndDate;
            model.Remark = vModel.CustomerFile.Remark;
            model.CreatedTime = DateTime.Now;
            model.CreatedBy = GlobalContext.Current.UserInfo.Code;

            string file_name = "";
            string logoPath = "";
            HttpPostedFileBase file = Request.Files["LogoFile"];
            logoPath = UploadingFile(file, vModel.CustomerFile.CustomerCode, ref file_name);
            if (file != null && file.ContentLength > 0)
            {
                model.FileName = file_name;
                model.FilePath = logoPath;
            }
            //if (!string.IsNullOrEmpty(logoPath))

            _biz.UploadingUpdate(model);
            return RedirectToAction("Uploading", new { code = vModel.CustomerFile.CustomerCode });
        }

        /// <summary>
        /// 上传功能
        /// </summary>
        /// <param name="file"></param>
        /// <param name="customerCode"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        private string UploadingFile(HttpPostedFileBase file, string customerCode, ref string file_name)
        {
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            file_name = file.FileName;
            string filename = string.Format("{0:yyyyMMddHHmmss}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = string.Format(@"customer\{0}", customerCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        /// <summary>
        /// 需要修改 //TODO
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult DownLoadFile(int Id)
        {
            CustomerFileModel model = _biz.Uploadingdownload(Id);
            if (model == null)
                return null;
            try
            {
                WebRequest.Create(AppSetting.Get("UploadFileRoot") + model.FilePath);
            }
            catch (Exception ex)
            {
                logger.Error("File not Found.", ex);
                return null;
            }

            byte[] fileData;
            try
            {
                using (WebClient client = new WebClient())
                {
                    fileData = client.DownloadData(AppSetting.Get("UploadFileRoot") + model.FilePath);

                    return File(fileData, "application/octet-stream", Server.UrlEncode(model.FileName));
                }
            }
            catch (Exception ex)
            {
                logger.Error("File download failure..", ex);
                return null;
            }
        }

        #endregion 客户附件管理

        #region 客户审核

        public ActionResult AuditCustomer(CustomerVModel vModel)
        {
            if (vModel == null)
                vModel = new CustomerVModel();

            ViewBag.PaymentTypes = DictionaryTools.GetEnumsBy(Enums.PaymentTypeEnum).ToSelectListFor();
            var teams = new List<SelectListItem>();
            var sales = new List<SelectListItem>();

            // 根据用户角色 锁定过滤
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售总监"))
            {
                teams = _teamBiz.GetSalesTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售组长"))//销售经理 没有这个角色。  改为销售组长
            {
                //过滤页面分组显示数据.
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 5 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
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

            ViewBag.Teams = teams;
            ViewBag.Salers = sales;
            vModel.Customers = _biz.GetNoAuditPagedList(vModel, UserInfo);

            if (Request.IsAjaxRequest())
                return PartialView("UCAuditCustomerList", vModel);
            return View(vModel);
        }

        /// <summary>
        /// 审核动作
        /// </summary>
        /// <param name="code"></param>
        /// <param name="remark"></param>
        /// <param name="state">1-通过 2-失败</param>
        /// <returns></returns>
        public ActionResult AuditCust(string code, string remark, int state = 0)
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

                // 微信通知销售
                var sales = _accountBiz.GetAccountCustomer(model.SalerCode);
                if (!string.IsNullOrEmpty(sales.OpenID))
                {
                    var first = string.Format("客户名称：{0}", model.Name);
                    var param1 = (state == 1 ? "客户审核通过" : "客户审核不通过");
                    var param3 = code;
                    //var remark1 = string.Format("remark");
                   
                    SendMessagClient.SendTemplateMessage(sales.OpenID, "H4wr3tCcSDvlVOR9J9cbomJjgajRyYzcrVJX_x3YLVA", first, param1, DateTime.Now.ToString(), param3, "", "", remark);
                }
            }

            return Json(new { Code = i }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 客户审核详情页
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult UCAuditCustomerDetail(string code)
        {
            var model = _biz.GetById(code);
            ViewBag.Regstration = new AccountBiz().GetCustomerRegistration(model.Code);
            ViewBag.CityList = _commonBiz.GetChildList(model.Province).ToSelectListFor(v => v.Id.ToString(), v => v.Name, model.City);

            #region 页面绑值

            InitPage();
            var sales = new List<SelectListItem>();
            // 过滤销售
            if (!String.IsNullOrEmpty(model.TeamID))
                sales = _biz.GetTeamUsersByTeamId(model.TeamID, OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            else
                sales = _biz.GetTeamSales(OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            ViewBag.Salers = sales;
            AccountVModel vModel = new AccountVModel();
            vModel.Account.CustomerCode = code;
            vModel.Accounts = _biz.GetContactPagedList(vModel);
            ViewBag.Accounts = vModel;

            #endregion 页面绑值

            ViewData["Alert"] = CheckCustomer(model);

            return View(model);
        }

        public ActionResult UCAuditCustContactInfo(string code, AccountVModel vModel)
        {
            if (string.IsNullOrEmpty(vModel.Account.CustomerCode))
            {
                vModel.Account.CustomerCode = code;
            }

            vModel.Accounts = _biz.GetContactPagedList(vModel);

            return PartialView("UCAuditCustContactInfo", vModel);
        }

        #endregion 客户审核
    }
}