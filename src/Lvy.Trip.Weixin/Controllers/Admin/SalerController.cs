using Common.Logging;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Weixin;
using Lvy.VModels.Weixin;
using Lvy.Web.Common;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 销售员首页
    /// </summary>
    [Authorize]
    public class SalerController : AdminBaseController
    {
        private ILog logger = LogManager.GetLogger("SalerController");

        private static readonly MemberBiz _memberBiz = new MemberBiz();
        private static string ownerCode = WebConfigurationManager.AppSettings["OwnerCode"];

        /// <summary>
        /// 销售关联微信客户列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Index(MemberQModel model)
        {
            model.Sales = UserInfo.Name;
            model.OwnerCode = ownerCode;
            if (model.MemberPageList == null)
                model.MemberPageList = new PagedList<Member>();
            model.MemberPageList = _memberBiz.GetPageMember(model);

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

            return View(model);
        }

        public ActionResult PageList(MemberQModel model)
        {
            CrmAccountModel user = GlobalContext.Current.UserInfo;
            model.Sales = user.Name;
            model.OwnerCode = ownerCode;
            model.MemberPageList = _memberBiz.GetPageMember(model);
            return PartialView("PageList", model);
        }
    }
}