using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Weixin;
using Lvy.Trip.Weixin.Models;
using Lvy.Trip.Weixin.Mvc.Attributes;
using Lvy.VModels.Order;
using Lvy.Web.Common;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 管理首页
    /// </summary>
    public class AdminController : AdminBaseController
    {
        private ILog logger = LogManager.GetLogger("AdminController");
        private MemberBiz service = new MemberBiz();
        private OrderBiz _orderBiz = new OrderBiz();
        private TpTourPlanBiz _planBiz = new TpTourPlanBiz();
        private AccountBiz accountBiz = new AccountBiz();

        /// <summary>
        /// 后台首页
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Index()
        {
            CrmAccountModel user = GlobalContext.Current.UserInfo;
            if (user == null)
                return RedirectToAction("Login", "Account", new { @ReturnUrl = "/admin" });

            // 当前用户
            MemberQR qr = new QrBiz().getQrByEmployee(user.Code);
            ViewData["MyQR"] = qr;

            // 提交审核的微信客户
            ViewData["MP"] = service.GetLastMember(user.Name, 5);

            // 销售员显示未完成订单
            TpOrderVModel vm = new TpOrderVModel();
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "销售"))
            {
                vm.SalerCode = GlobalContext.Current.UserInfo.Code;
            }
            else if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                vm.CrmTeamId = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2).FirstOrDefault().TeamID;

                // 计调显示处理的订单 和 最近的开班计划
                string[] teams = GlobalContext.Current.LoginUserTeams.Select(t => t.TeamID).ToArray();
                ViewData["LP"] = _planBiz.GetOpTours(teams, 5, UserInfo);
            }
            ViewData["MO"] = _orderBiz.GetTaskOrderList(vm, 5, UserInfo);

            return View();
        }

        /// <summary>
        /// 微信登陆页面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Weixin(string code, string state)
        {
            if (InWeixin(code, state))
            {
                return RedirectToAction("Index", "Admin");
            }

            ViewData["Message"] = "无权访问！";
            return View();
        }

        #region 微信客户绑定后台用户

        /// <summary>
        /// 用户绑定，用户设置
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult BindingAccount(string code, string state, string sceneId)
        {
            int headache = 0;
            if (string.IsNullOrEmpty(code))
            {
                headache = 1;
                ViewData["Message"] = "您拒绝了授权！";
            }
            logger.Info("state:" + state);

            //MemberModel test = service.getMember("odLqHjmDEl7LcCdZc_oHrkIq9z4g");
            //Session["WeixinUser"] = service.getMember(test.OpenID);
            //return View(test);

            //通过，用code换取access_token
            try
            {
                var result = OAuthApi.GetAccessToken(appId, secret, code);

                if (result.errcode != ReturnCode.请求成功)
                {
                    headache = 1;
                    ViewData["Message"] = "错误：" + result.errmsg;
                }

                if (headache == 1)
                    return View("Message");

                Member user = service.GetMemberByOpenID(result.openid);

                // 双向绑定
                var model = accountBiz.GetAccountCustomer(state);
                TempData["AccountModel"] = model;

                return View(user);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                ViewData["Message"] = "微信授权失败， 请重新再试！" + ex.Message;
                return View("Message");
            }
        }

        /// <summary>
        /// 账号绑定
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AccountSubmit(string openID, string accountCode)
        {
            try
            {
                Member user = service.GetMemberByOpenID(openID);
                var model = accountBiz.GetAccountCustomer(accountCode);

                if (user != null)
                {
                    user.EmployeeID = accountCode;
                    if (model.OwnerCode == Configs.OwnerCode)
                        user.IsEmployee = 1;
                    service.SaveMember(user);
                }

                // 后台设定
                model.OpenID = openID;
                accountBiz.Update(model);

                //if (send > 0)
                //{
                var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fadmin%2Fweixin%2F%3F&response_type=code&scope=snsapi_base&state=#wechat_redirect";
                var testData = new MessageData()
                {
                    first = new TemplateDataItem("您好，您已经绑定成功，点击消息接下来的操作。"),
                    keyword1 = new TemplateDataItem(model.LoginName),
                    keyword2 = new TemplateDataItem("绑定成功，会帮助您更好的业务合作。"),
                    remark = new TemplateDataItem("")
                };
                var result = TemplateApi.SendTemplateMessage(accessToken, openID, "HeAnhQ1ost8YHY4LmbUQ5XAAM_8C_jJIgcdLIt8Uad8", url, testData);
                //}

                ViewData["Message"] = "绑定成功！您可以点击“返回”按钮，继续发送查询消息。";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "绑定失败！网络故障，请稍后再试。";
                logger.Warn("客户绑定失败", ex);
                return Content("绑定失败！网络故障，请稍后再试。");
            }
        }

        #endregion 微信客户绑定后台用户

        #region 后台登陆验证

        [AllowAnonymous]
        public ActionResult ErpLogin(string code, string state, string sceneId)
        {
            if (string.IsNullOrEmpty(code))
            {
                ViewData["Message"] = "您拒绝了授权！";
            }
            logger.Info("state:" + state);

            //通过，用code换取access_token
            try
            {
                var result = OAuthApi.GetAccessToken(appId, secret, code);

                if (result.errcode != ReturnCode.请求成功)
                {
                    ViewData["Message"] = "错误：" + result.errmsg;
                }

                // 后台账号
                var model = accountBiz.GetAccountByOpenID(result.openid);
                logger.Info("Weixin OpenID:" + result.openid);
                if (model == null)
                {
                    //因为这里还不确定用户是否关注本微信，所以只能试探性地获取一下
                    OAuthUserInfo userInfo = null;

                    //已关注，可以得到详细信息
                    userInfo = OAuthApi.GetUserInfo(result.access_token, result.openid);
                    service.UpdateMember(OwnerCode, result.openid, "1", userInfo.nickname, userInfo.sex, userInfo.city, userInfo.province, userInfo.country, userInfo.headimgurl, "", DateTime.MinValue);
                    // 重新获取
                    model = accountBiz.GetAccountByOpenID(result.openid);
                }

                ViewData["AccountModel"] = model;
                ViewData["SceneId"] = state;

                return View();
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                ViewData["Message"] = "微信授权失败， 请重新再试！" + ex.Message;
                return View("Message");
            }
        }

        [AllowAnonymous]
        public ActionResult AgreeLogin(string sceneId, string accountCode)
        {
            logger.Info("target webservice:" + accountCode);
            new LinkService.LinkServiceSoapClient().WeixinQrRtn(sceneId, accountCode);
            return View();
        }

        #endregion 后台登陆验证
    }
}