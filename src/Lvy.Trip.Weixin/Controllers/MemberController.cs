using Common.Logging;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Weixin;
using Lvy.Trip.Weixin.Models;
using Lvy.VModels.Base;
using Lvy.VModels.Order;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using System;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 微信前端  个人中心
    /// </summary>
    public class MemberController : BaseController
    {
        private ILog logger = LogManager.GetLogger("MemberController");
        private OrderBiz orderBiz = new OrderBiz();
        private MemberBiz memberBiz = new MemberBiz();
        private AccountBiz accountBiz = new AccountBiz();

        //
        // GET: /Account/Login
        public ActionResult SignIn()
        {
            return View();
        }

        public ActionResult SignUp()
        {
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVModel vModel)
        {

            if (TempData["ReturnUrl"] != null)
                return Redirect(TempData["ReturnUrl"].ToString());

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Session.Remove("WeixinUser");
            return RedirectToAction("Index", "Home");
        }
        
        [Authorize]
        public ActionResult MemberProfile()
        {
            return View("Profile");
        }

        [Authorize]
        public ActionResult Settings()
        {
            if (Session["WeixinUser"]== null)
                return Content("服务器错误。");




            return View();
        }

        /// <summary>
        /// 我的订单（作为下单客户）
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Order(string code, string state)
        {
            TpOrderVModel qmodel = new TpOrderVModel();
            qmodel.OwnerCode = Configs.OwnerCode;
            InWeixin(code, state);

            if (Session["WeixinUser"] != null)
            {
                var member = (Member)Session["WeixinUser"];

                if (!string.IsNullOrEmpty(member.EmployeeID))
                {
                    var account = accountBiz.GetAccountCustomer(member.EmployeeID);

                    if (account.OwnerCode == Configs.OwnerCode)
                    {
                        // 公司员工
                        // 如果是公司销售
                        qmodel.SalerCode = member.EmployeeID;
                    }
                    else
                    {
                        // 外部联系人
                        // 关联账号没有关闭，所属客户正常
                        qmodel.CustomerCode = account.CustomerCode;
                    }
                    qmodel.OrderModels = orderBiz.GetOrderList(qmodel);  // 微信
                }
            }
            return View(qmodel);
        }

        #region 客户绑定销售

        /// <summary>
        /// 用户绑定，用户设置
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Binding(string code, string state)
        {
            int headache = 0;
            if (string.IsNullOrEmpty(code))
            {
                headache = 1;
                ViewData["Message"] = "您拒绝了授权！";
            }
            if (state != "JeffreySu")
            {
                headache = 1;
                ViewData["Message"] = "验证失败！请从正规途径进入！";
            }

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

                //下面2个数据也可以自己封装成一个类，储存在数据库中（建议结合缓存），每一个人的access_token是不一样的
                //Session["OAuthAccessTokenStartTime"] = DateTime.Now;
                //Session["OAuthAccessToken"] = result;

                Member user = memberBiz.GetMemberByOpenID(result.openid);
                logger.Info("Weixin OpenID:" + result.openid);
                if (user == null)
                {
                    //因为这里还不确定用户是否关注本微信，所以只能试探性地获取一下
                    OAuthUserInfo userInfo = null;

                    //已关注，可以得到详细信息
                    userInfo = OAuthApi.GetUserInfo(result.access_token, result.openid);
                    memberBiz.UpdateMember(OwnerCode, result.openid, "1", userInfo.nickname, userInfo.sex, userInfo.city, userInfo.province, userInfo.country, userInfo.headimgurl, "", DateTime.MinValue);
                    // 重新获取
                    user = memberBiz.GetMemberByOpenID(result.openid);
                }
                if (string.IsNullOrEmpty(user.RealName)) user.RealName = user.NickName;

                Session["WeixinUser"] = user;
                return View(user);
            }
            catch (Exception ex)
            {
                //未关注，只能授权，无法得到详细信息
                //这里的 ex.JsonResult 可能为："{\"errcode\":40003,\"errmsg\":\"invalid openid\"}"
                logger.Error("", ex);
                ViewData["Message"] = "微信授权失败， 请重新再试！" + ex.Message;
                return View("Message");
            }
        }

        /// <summary>
        /// 提交绑定信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Submit(Member user)
        {
            try
            {
                // 验证输入销售是否正确， 发送微信到销售
                int send = BindingUser(user);
                if (send > 0)
                {
                    string openid = memberBiz.GetOpenID(user.Sales);
                    if (!string.IsNullOrEmpty(openid))
                    {
                        var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                        string url = "http://yuanwx.sh-cct.cn/member/details/" + user.OpenID;
                        var testData = new MessageData()
                        {
                            first = new TemplateDataItem("您好，您的客户提交信息审核。"),
                            keyword1 = new TemplateDataItem(user.CustomerName),
                            keyword2 = new TemplateDataItem(user.RealName),
                            keyword3 = new TemplateDataItem(user.PhoneNumber),
                            keyword4 = new TemplateDataItem(DateTime.Now.ToShortDateString()),
                            remark = new TemplateDataItem("审核客户信息后请及时确认。")
                        };

                        var result = TemplateApi.SendTemplateMessage(accessToken, openid, "dbtBNSrwDX2qG4qXu9pAGTj172O5_ZqvVkPxQlvnpDw", url, testData);
                    }
                }
                Session["WeixinUser"] = memberBiz.GetMemberByOpenID(user.OpenID); // 参数修改
                ViewData["Message"] = "绑定成功！您可以点击“返回”按钮，继续发送查询消息。";
                return View("Logo");
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "绑定失败！网络故障，请稍后再试。";
                logger.Warn("客户绑定失败", ex);
                return View("Message");
            }
        }

        private int BindingUser(Member u)
        {
            int sendmsg = 0;
            var sales = accountBiz.GetAccountByName(Configs.OwnerCode, u.Sales);

            Member user = memberBiz.GetMemberByOpenID(u.OpenID);
            if (user != null)
            {
                user.RealName = u.RealName;
                user.PhoneNumber = u.PhoneNumber;
                user.CustomerName = u.CustomerName;
                user.HideShared = u.HideShared;
                user.LogoUrl = u.LogoUrl;

                if (sales != null)
                {
                    user.Binding = 1;
                    user.Sales = sales.Name;
                    user.SalesID = sales.Code;
                    if (user.Approved == 0)
                        sendmsg = 1;
                }

                memberBiz.SaveMember(user);
            }

            return sendmsg;
        }

        public ActionResult GetMedia(string id, string openID)
        {
            try
            {
                var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                string file = string.Empty;
                string strpath = string.Empty;
                string savepath = string.Empty;
                string stUrl = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", accessToken, id);
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(stUrl);
                req.Method = "GET";
                using (WebResponse wr = req.GetResponse())
                {
                    HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();
                    strpath = myResponse.ResponseUri.ToString();
                    WebClient mywebclient = new WebClient();
                    file = string.Format("{0:yyyyMMddHHmmssfff}{1}.jpg", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4));
                    savepath = Server.MapPath("~/upload/clogo/") + file;

                    try
                    {
                        mywebclient.DownloadFile(strpath, savepath);
                    }
                    catch (Exception ex)
                    {
                        savepath = ex.ToString();
                    }
                }

                // 更新客户Logo
                Member user1 = memberBiz.GetMemberByOpenID(openID);
                if (user1 != null)
                {
                    user1.LogoUrl = "http://yuanwx.sh-cct.cn/upload/clogo/" + file;
                    memberBiz.SaveMember(user1);
                }

                var json = new
                {
                    Success = "ok",
                    Message = "http://yuanwx.sh-cct.cn/upload/clogo/" + file
                };

                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error("保存微信文件失败.", ex);
                var json = new { Success = false, Message = ex.Message };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion 客户绑定销售
    }
}