using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Weixin;
using Lvy.VModels.Order;
using Lvy.VModels.Weixin;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.QrCode;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 微信客户管理
    /// </summary>
    [Authorize]
    public class MemberAdminController : AdminBaseController
    {
        private ILog logger = LogManager.GetLogger("MemberAdminController");
        private MemberBiz service = new MemberBiz();
        private OrderBiz oservice = new OrderBiz();
        private AccountBiz eservice = new AccountBiz();
        private CustomerBiz customerBiz = new CustomerBiz();
        private static string ownerCode = WebConfigurationManager.AppSettings["OwnerCode"];

        // GET: Member
        public ActionResult Index(MemberQModel model)
        {
            if (model.MemberPageList == null)
                model.MemberPageList = new Lvy.Models.PagedList<Member>();

            model.MemberPageList = service.GetPageMember(model);
            // 销售列表
            var sl = customerBiz.GetTeamSales(ownerCode).ToSelectListFor(k => k.Code, v => v.Name);
            sl.Add(new SelectListItem { Text = "所有", Value = "", Selected = true });
            ViewBag.SalesList = sl;
            // 关注列表
            List<SelectListItem> subscribelist = new List<SelectListItem>();
            subscribelist.Add(new SelectListItem { Text = "所有", Value = "", Selected = true });
            subscribelist.Add(new SelectListItem { Text = "关注", Value = "1" });
            subscribelist.Add(new SelectListItem { Text = "不关注", Value = "0" });
            ViewBag.SubscribeList = subscribelist;
            // 绑定列表
            List<SelectListItem> bindinglist = new List<SelectListItem>();
            bindinglist.Add(new SelectListItem { Text = "所有", Value = "", Selected = true });
            bindinglist.Add(new SelectListItem { Text = "绑定", Value = "1" });
            bindinglist.Add(new SelectListItem { Text = "未绑定", Value = "0" });
            ViewBag.BindingList = bindinglist;
            // 审核列表
            List<SelectListItem> approvedList = new List<SelectListItem>();
            approvedList.Add(new SelectListItem { Text = "所有", Value = "", Selected = true });
            approvedList.Add(new SelectListItem { Text = "已审核", Value = "1" });
            approvedList.Add(new SelectListItem { Text = "未审核", Value = "0" });
            ViewBag.ApprovedList = approvedList;
            // 是否公司员工
            List<SelectListItem> employeeList = new List<SelectListItem>();
            employeeList.Add(new SelectListItem { Text = "所有", Value = "", Selected = true });
            employeeList.Add(new SelectListItem { Text = "员工", Value = "1" });
            employeeList.Add(new SelectListItem { Text = "非员工", Value = "0" });
            ViewBag.EmployeeList = employeeList;

            return View(model);
        }

        public ActionResult PageList(MemberQModel model)
        {
            model.MemberPageList = service.GetPageMember(model);
            return PartialView("PageList", model);
        }

        //public ActionResult Stat()
        //{
        //    var model = service.MemberStat();
        //    ViewData["Sales"] = string.Format("[\"{0}\"]", string.Join("\",\"", model.Select(t => t.UserName).ToArray()));
        //    ViewData["SalesCount"] = string.Format("[{0}]", string.Join(",", model.Select(t => t.AllFans).ToArray()));
        //    ViewData["SalesWCount"] = string.Format("[{0}]", string.Join(",", model.Select(t => t.LastWeekFans).ToArray()));
        //    ViewData["Salesman"] = model;
        //    return View(model);
        //}

        /// <summary>
        /// 微信客户同步
        /// </summary>
        /// <returns></returns>
        public ActionResult SyncUser()
        {
            // 取得所有关注用户
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
            var result = UserApi.Get(accessToken, "");
            service.UnsubscribeAll();
            for (var i = 0; i < result.count; i++)
            {
                var ss = result.data.openid[i];
                accessToken = AccessTokenContainer.GetAccessToken(appId);
                UserInfoJson info = UserApi.Info(accessToken, ss);
                service.UpdateMember(OwnerCode, ss, "1", info.nickname, info.sex, info.city, info.province,
                    info.country, info.headimgurl, info.language, DateTimeHelper.GetDateTimeFromXml(info.subscribe_time));
                logger.Info(string.Format("Current row:{0},OpenID:{1}", i, ss));
            }
            return Json(new { Success = "true", Message = "更新成功！" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 单用户同步数据
        /// </summary>
        /// <param name="openID"></param>
        /// <returns></returns>
        public ActionResult SyncOneUser(string openID)
        {
            // 取得所有关注用户
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
            UserInfoJson info = UserApi.Info(accessToken, openID);
            if (info.subscribe == 0)
            {
                service.Unsubscribe(openID);
                logger.Info(string.Format("Current user is unsubscribe OpenID:{0}", openID));
            }
            else
                service.UpdateMember(OwnerCode, openID, info.subscribe.ToString(), info.nickname, info.sex, info.city, info.province,
                    info.country, info.headimgurl, info.language, DateTimeHelper.GetDateTimeFromXml(info.subscribe_time));

            return Json(new { Success = "true", Message = "更新成功！" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Location(string id)
        {
            MemberQModel qmo = new MemberQModel();
            qmo.Locations = new LocationBiz().getLocations(id);
            return View(qmo);
        }

        public ActionResult SendMessage(MemberMessage message)
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
            var result = CustomApi.SendText(accessToken, message.OpenID, message.Content);
            if (result.errmsg == "ok")
            {
                message.InOut = 1;
                message.IsCallBack = "1";
                message.CreatedDate = DateTime.Now;

                new MessageBiz().AddMessage(message);

                return Content("ok");
            }

            return Content("error");
        }

        public ActionResult MessageList(Member model)
        {
            ViewData["OpenID"] = model.OpenID;
            model.Messages = new MessageBiz().GetMessages(model.OpenID);
            return PartialView("MessageList", model);
        }

        // GET: Member/Edit/5
        public ActionResult Edit(int id)
        {
            // 销售列表
            List<SelectListItem> sl = eservice.GetAllAccount(Configs.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);
            sl.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewBag.SalesList = sl;
            // 绑定列表
            List<SelectListItem> bindinglist = new List<SelectListItem>();
            bindinglist.Add(new SelectListItem { Text = "绑定", Value = "1" });
            bindinglist.Add(new SelectListItem { Text = "未绑定", Value = "0" });
            ViewBag.BindingList = bindinglist;
            // 审核列表
            List<SelectListItem> approvedList = new List<SelectListItem>();
            approvedList.Add(new SelectListItem { Text = "已审核", Value = "1" });
            approvedList.Add(new SelectListItem { Text = "未审核", Value = "0" });
            ViewBag.ApprovedList = approvedList;
            // 分享是否显示电话
            List<SelectListItem> shareList = new List<SelectListItem>();
            shareList.Add(new SelectListItem { Text = "显示信息", Value = "True" });
            shareList.Add(new SelectListItem { Text = "隐藏信息", Value = "False" });
            ViewBag.ShareList = shareList;

            Member model = service.GetMemberByID(id);
            if (!string.IsNullOrEmpty(model.EmployeeID))
            {
                MemberQR qr = new QrBiz().getQrByEmployee(model.EmployeeID);
                ViewData["MyQR"] = qr;
                var qmodel = new TpOrderVModel { SalerCode = model.EmployeeID };
                model.OrderList = oservice.GetOrderList(qmodel);
                ViewData["OpenID"] = model.OpenID;
            }
            return View(model);
        }

        // POST: Member/Edit/5
        [HttpPost]
        public ActionResult Edit(Member model)
        {
            try
            {
                service.SaveMember(model);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// 客户信息
        /// </summary>
        /// <param name="id">OpenID</param>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            Member model = null;

            // 临时冗余， 最终用 service.getUser(id);
            if (id.Length > 5)  // OpenID
                model = service.GetMemberByOpenID(id);
            else
                model = service.GetMemberByID(Convert.ToInt32(id));

            if (!string.IsNullOrEmpty(model.EmployeeID))
            {
                MemberQR qr = new QrBiz().getQrByEmployee(model.EmployeeID);
                ViewData["MyQR"] = qr;
            }

            return View(model);
        }

        // Post: Branchs/Delete/5
        public ActionResult Delete(int id)
        {
            var row = service.DelMember(id).ToString();
            return Content(row);
        }

        [AllowAnonymous]
        public ActionResult CreateQr(int id, string employeeid)
        {
            CrmAccountModel user = GlobalContext.Current.UserInfo;
            var QrBiz = new QrBiz();
            var accessToken = AccessTokenContainer.GetAccessToken(appId);
            int newq = QrBiz.GetMaxQr() + 1;
            CreateQrCodeResult result = QrCodeApi.Create(accessToken, 0, newq, Senparc.Weixin.MP.QrCode_ActionName.QR_LIMIT_SCENE);  // 永久QRCode

            QrBiz.SaveQr(id, newq, result.ticket, employeeid, user.OwnerCode);
            return Content(result.ticket);
        }

        [AllowAnonymous]
        public ActionResult OrderDetails(string id)
        {
            TpOrderModel order = oservice.GetOrderLineTourist(id);
            return View(order);
        }
    }
}