using Common.Logging;
using Lvy.Models.ProductDB;
using Lvy.Models.SiteDB;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Booking;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Biz.Weixin;
using Lvy.Trip.Weixin;
using Lvy.Trip.Weixin.Controllers;
using Lvy.Trip.Weixin.Models;
using Lvy.VModels.Product;
using Lvy.Web.Common.Cache;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Helpers;
using Senparc.Weixin.MP.TenPayLibV3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CCT.Weixin.Controllers
{
    /// <summary>
    /// 微店产品列表，详细和预定
    /// </summary>
    [AllowAnonymous]
    public class LineController : BaseController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LineController));

        private TpLineBiz lineBiz = new TpLineBiz();
        private SiteNavBiz navBiz = new SiteNavBiz();
        private MemberBiz memberBiz = new MemberBiz();
        private TpLineTourPlanBiz planBiz = new TpLineTourPlanBiz();
        private BookingBiz bookingBiz = new BookingBiz();
        private SiteNavBiz _navBiz = new SiteNavBiz();
        private SiteBannerBiz _bannerBiz = new SiteBannerBiz();
        private SearchProductBiz _searchProductBiz = new SearchProductBiz();

        private JSSDKHelper helper = new JSSDKHelper();
        private static TenPayV3Info _tenPayV3Info;

        public static TenPayV3Info TenPayV3Info
        {
            get
            {
                if (_tenPayV3Info == null)
                {
                    _tenPayV3Info =
                        TenPayV3InfoCollection.Data[System.Configuration.ConfigurationManager.AppSettings["TenPayV3_MchId"]];
                }
                return _tenPayV3Info;
            }
        }

        public LineController()
        {
            // 微信
            AccessTokenContainer.Register(appId, secret);
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="outCity"></param>
        /// <returns></returns>
        public ActionResult Index(string outCity)
        {
            string region = "";
            if (String.IsNullOrEmpty(outCity))
                outCity = "31";
            Session["OutCity"] = outCity;

            WapModel model = new WapModel();

            #region 取得线路导航

            var CacheNavKey = "CacheKey=Line|List|NavList:" + outCity + region;
            var _NavList = CacheContext.Current.Get(CacheNavKey);
            if (_NavList == null)
            {
                model.NavList = _navBiz.SearchList("W001", outCity);
                CacheContext.Current.Add(CacheNavKey, model.NavList);
            }
            else
                model.NavList = (IList<SiteNavItemModel>)_NavList;

            #endregion 取得线路导航

            // 轮播图
            ViewData["SiteBanner"] = _bannerBiz.GetBanner("W001");

            // 推荐线路
            ViewData["W001L1"] = _searchProductBiz.GetHotTours("W001L1", OwnerCode);

            return View(model);
        }

        public ActionResult Index2(string outCity, int itemId)
        {
            if (String.IsNullOrEmpty(outCity))
                outCity = "31";
            Session["OutCity"] = outCity;

            // 父节点
            WapModel model = new WapModel();
            SiteNavItemModel nav = _navBiz.GetNavItem(itemId);
            model.Code = itemId;
            model.Title = nav.Name;

            // 子节点
            var CacheNavKey = "CacheKey=Home|Index|NavList:" + outCity + itemId;
            var _NavList = CacheContext.Current.Get(CacheNavKey);
            if (_NavList == null)
            {
                model.NavList = _navBiz.SearchList("W001", outCity, itemId);
                CacheContext.Current.Add(CacheNavKey, model.NavList);
            }
            else
                model.NavList = (IList<SiteNavItemModel>)_NavList;

            return View(model);
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="outCity">出发城市代码</param>
        /// <param name="itemId"></param>
        /// <param name="pid"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public ActionResult List(string outCity, int itemId, string pid, string words)
        {
            try
            {
                // Get JsApi
                ViewBag.JsApi = JSSDKHelper.GetJsSdkUiPackage(appId, secret, Request.Url.AbsoluteUri);
            }
            catch (Exception)
            {
            }

            // Content
            Session["OutCity"] = outCity;

            SearchTourVModel model = new SearchTourVModel();
            model.OwnerCode = Configs.OwnerCode;
            string region = "";
            if (itemId != 0)
            {
                SiteNavItemModel nav = navBiz.GetNavItem(itemId);
                region = nav.Region;
                model.NavCondition.Title2 = nav.Name;
                model.NavCondition.ParentID = nav.ParentID;
                model.NavCondition.ImgUrl = nav.ImageUrl;
            }
            else
            {
                model.NavCondition.Title2 = "查询结果";
            }

            model.NavCondition.ItemID = itemId;
            model.NavCondition.Region = region;
            model.NavCondition.Words = words;
            model.NavCondition.OutCity = outCity;

            // LineQModel qmodel = lineService.SearchAllLine(model);
            var CacheKey = "CacheKey=Product|List|List:" + outCity + itemId;
            var _getModel = CacheContext.Current.Get(CacheKey);
            if (_getModel == null)
            {
                model = planBiz.SearchLinq(model);
                CacheContext.Current.Add(CacheKey, model, Configs.cacheDateTime);
            }
            else
            {
                model = (SearchTourVModel)_getModel;
            }

            return View(model);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="woid"></param>
        /// <param name="code">微信OAuth参数</param>
        /// <param name="state">微信OAuth参数</param>
        /// <returns></returns>
        public ActionResult Details(string id, int tourId, string woid, string wxshare, string code, string state)
        {
            TpLineModel lineModel = lineBiz.GetLineById(id);
            lineModel.Tours = planBiz.GetToursByLine(id, true);
            lineModel.RouteInfo = bookingBiz.GetLineRoute(id, tourId);
            lineModel.PicList = lineBiz.GetLineFileList(id).Where(m => m.SourceType == "12").ToList();
            try
            {
                ViewBag.JsApi = JSSDKHelper.GetJsSdkUiPackage(appId, secret, Request.Url.AbsoluteUri);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                // return Content("数据错误。");
            }
            Session["OutCity"] = lineModel.DepartDest;
            if (!string.IsNullOrEmpty(woid))
                ViewBag.WeixinUser = memberBiz.GetMemberByAccount(woid); // 上家

            // 微信转发
            if (!string.IsNullOrEmpty(code))
            {
                var result = OAuthApi.GetAccessToken(appId, secret, code);
                if (result.errcode == ReturnCode.请求成功)
                {
                    Member user = memberBiz.GetMemberByOpenID(result.openid);
                    if (user != null)
                    {
                        // 直客同业都可以分享，无须绑定 user.Binding == 1
                        if (string.IsNullOrEmpty(user.PhoneNumber) || string.IsNullOrEmpty(user.RealName))
                        {
                            ViewData["Message"] = "您还没有补充公司和个人信息！";
                            ViewBag.Incomplete = "true";
                            Session["ProductCode"] = id;
                            Session["ProductName"] = lineModel.LineName;
                            return View("Binding", user);
                        }
                        else
                        {
                            //成功
                            Session["WeixinUser"] = user; // 当前用户
                            if (!string.IsNullOrEmpty(wxshare))
                                ViewBag.WeixinUser = user; // 转发用户
                        }
                        if (user.Subscribe == "0") // 取消关注的用户
                        {
                            ViewData["Message"] = "赶快关注我们微信，更多惊喜等着您！";
                        }
                    }
                    else
                    {
                        ViewData["Message"] = "您还没有关注我的微信服务号！";
                    }
                    ViewBag.UserInfo = user;
                }
            }

            return View(lineModel);
        }

        /// <summary>
        /// WAP日历使用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetCalendar(string id)
        {
            var plans = planBiz.GetByLineId(id, true);
            var rr = (from ss in plans
                      select new
                      {
                          title = ss.Price.ToString("￥00") + "\n余位:" + ">9",
                          //title = ss.Price.ToString("￥00") + "\n余位:" + (ss.PAX3 > 9 ? ">9" : ss.PAX3.ToString()),
                          start = ss.OutDate.ToDateFormat(),
                          // backgroundColor = (ss.PAX3 > 0) ? "#66cc99" : "#FF6666"
                          backgroundColor = "#66cc99"
                      }).ToList();

            //{
            //  title: 'Click for Google',
            //  start: new Date(y, m, 28),
            //  end: new Date(y, m, 29),
            //  url: 'http://google.com/',
            //  backgroundColor: '#3c8dbc', //Primary (light-blue)
            //  borderColor: '#3c8dbc' //Primary (light-blue)
            //}
            return Json(rr, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult List(string code, string state)
        //{
        //    MemberModel member = new MemberModel();

        //    var CacheKey = "CacheKey=Line|List|List:1002";
        //    var _getModel = DataCache.GetCache(CacheKey);
        //    var products = new List<ProductModel>();
        //    if (_getModel == null)
        //    {
        //        products = service.GetProductList();
        //        DataCache.SetCache(CacheKey, products, DateTime.Now.AddMinutes(Configs.cacheDateTime), TimeSpan.Zero);
        //    }
        //    else
        //    {
        //        products = ((List<ProductModel>)_getModel);
        //    }

        //    return View(products);
        //}

        ///// <summary>
        ///// 支付购买
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="hc"></param>
        ///// <returns></returns>
        //public ActionResult Index(int id)
        //{
        //    var product = service.GetProduct(id);
        //    if (product == null)
        //    {
        //        return Content("商品信息不存在，或非法进入！2003");
        //    }

        //    return View(product);
        //}

        //[HttpPost]
        //[AjaxOnly]
        //public ActionResult AddCart(string openID, int itemID)
        //{
        //    var num = service.CartAddProduct(openID, itemID);
        //    MemberModel cuser = (MemberModel)HttpContext.Session["WeixinUser"];
        //    if (cuser != null)
        //    {
        //        cuser.CartProductNum = num;
        //    }
        //    return Json(num);
        //}
        //[HttpPost]
        //[AjaxOnly]
        //public ActionResult RemoveCart(int MemberID, string items)
        //{
        //    string[] di = items.Split(',');
        //    foreach (string works in di)
        //    {
        //        if (!string.IsNullOrWhiteSpace(works))
        //        {
        //            int itemID = Convert.ToInt32(works);
        //            service.CartDelProduct(MemberID, itemID);
        //        }
        //    }

        //    int num = service.CartProductNum(MemberID);
        //    MemberModel cuser = (MemberModel)HttpContext.Session["WeixinUser"];
        //    if (cuser != null)
        //    {
        //        cuser.CartProductNum = num;
        //    }

        //    return Json(num);
        //}

        //public ActionResult ShoppingCart()
        //{
        //    WeixinService weixinService = new WeixinService();
        //    MemberModel cuser = (MemberModel)HttpContext.Session["WeixinUser"];
        //    //var sessionuser = weixinService.getMember(openIdResult.openid);
        //    //Session["CurrentMember"] = weixinService.getMember("oHWMJj_rVDDSbxN_Y0n7BWMi6zYo");
        //    cuser.WxCardList = weixinService.GetAppCard(cuser.OpenID);
        //    //Session["CurrentMember"] = sessionuser;

        //    CartModel model = service.GetCart(cuser.MemberID);
        //    return View(model);
        //}

        ///// <summary>
        ///// 购物车订单生成
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public ActionResult CartSubmit(OrderModel model)
        //{
        //    string tid = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);
        //    MemberModel member = (MemberModel)Session["WeixinUser"];
        //    if (member == null)
        //    {
        //        return Content("当前微笑那客户不存在！");
        //    }

        //    // 验证卡券
        //    string WeixinCardID = "";
        //    if (!string.IsNullOrEmpty(model.WeixinCardCode))
        //    {
        //        var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);
        //        var result = CardApi.CardConsume(accessToken, model.WeixinCardCode);
        //        if (result.errcode != ReturnCode.请求成功)
        //        {
        //            logger.Warn("微信卡券核销失败。code：" + model.WeixinCardCode + result.errmsg);
        //            return Content("微信卡券核销失败！请重新下单");
        //        }
        //        WeixinCardID = result.card.card_id;
        //    }

        //    string[] di = model.ProductItem.Split(';');
        //    List<SubOrder> subs = new List<SubOrder>();
        //    foreach (string dd in di)
        //    {
        //        if (!string.IsNullOrWhiteSpace(dd))
        //        {
        //            string[] dt = dd.Split(','); // [项目ID ，数量，金额]

        //            // 添加项目
        //            subs.Add(new SubOrder
        //            {
        //                NumIid = long.Parse(dt[0]),
        //                Title = service.getProductItem(long.Parse(dt[0])).ProductTitle,
        //                Num = long.Parse(dt[1]),
        //                Price = dt[2]
        //            });
        //        }
        //    }

        //    Order order = new Order
        //    {
        //        Tid = tid,
        //        HostID = 5,
        //        Status = "0",    // 未支付
        //        Payment = model.Payment,
        //        BuyerNick = member.NickName,
        //        BuyerOpenid = member.OpenID,
        //        NumIid = model.NumIid,
        //        Title = subs.First().Title,
        //        Num = (subs.Count > 1 ? 0 : subs.First().Num),
        //        Price = (subs.Count > 1 ? "" : subs.First().Price),
        //        Created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        CreatedDate = DateTime.Now,
        //        ReceiverName = model.ReceiverName,
        //        ReceiverPhone = model.ReceiverPhone,
        //        ReceiverMobile = model.ReceiverMobile,
        //        ReceiverAddress = model.ReceiverAddress,
        //        // ReceiverCity = result.receiver_city,
        //        // ReceiverProvince = result.receiver_province,
        //        OrderFrom = "5",
        //        BuyerMessage = model.Remarks,
        //        InvoiceTitle = model.InvoiceTitle,
        //        NeedInvoice = (model.NeedInvoice == "true" ? "1" : "0"),
        //        WeixinCardID = WeixinCardID,
        //        WeixinCardCode = model.WeixinCardCode
        //    };
        //    OrderService oservice = new OrderService();
        //    oservice.AddOrder(order);

        //    // 保存客人地址
        //    if (member.AddressList.Count == 0)
        //    {
        //        wservice.AddAdress(new MemberAddress
        //        {
        //            MemberID = member.MemberID,
        //            HostID = member.HostID,
        //            ConsigneeAddress = model.ReceiverAddress,
        //            ConsigneeAlias = "默认",
        //            ConsigneeMobile = model.ReceiverPhone,
        //            ConsigneeName = model.ReceiverName,
        //            CreatedDate = DateTime.Now,
        //            IsDefault = 1
        //        });
        //    }

        //    model.Tid = order.Tid;
        //    foreach (SubOrder item in subs)
        //    {
        //        item.Tid = model.Tid;
        //        oservice.AddSubOrder(item);
        //    }
        //    //// 清理购物车
        //    //service.CartClearProduct(member.MemberID);

        //    //判断是否正在微信端
        //    var userAgent = Request.UserAgent;
        //    if (BroswerUtility.SideInWeixinBroswer(HttpContext))
        //    {
        //        //正在微信端，直接跳转到微信支付页面
        //        return RedirectToAction("Pay", new { orderId = model.Tid });
        //    }
        //    else
        //    {
        //        //在PC端打开，提供二维码扫描进行支付
        //        return View(model);
        //    }
        //}

        ///// <summary>
        ///// 显示二维码
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <returns></returns>
        //public ActionResult ProductPayCode(string orderId)
        //{
        //    OrderService service = new OrderService();
        //    var order = service.getOrderById(orderId);
        //    if (order == null)
        //    {
        //        return Content("商品信息不存在，或非法进入！2004");
        //    }

        //    var url = string.Format("http://wap.sh-cct.cn/Line/Pay?orderId={0}&t={1}", orderId, DateTime.Now.Ticks);

        //    BitMatrix bitMatrix;
        //    bitMatrix = new MultiFormatWriter().encode(url, BarcodeFormat.QR_CODE, 600, 600);
        //    BarcodeWriter bw = new BarcodeWriter();

        //    var ms = new MemoryStream();
        //    var bitmap = bw.Write(bitMatrix);
        //    bitmap.Save(ms, ImageFormat.Png);
        //    //return File(ms, "image/png");
        //    ms.WriteTo(Response.OutputStream);
        //    Response.ContentType = "image/png";
        //    return null;
        //}

        ///// <summary>
        ///// 显示预定页面（关注先）
        ///// </summary>
        ///// <param name="code"></param>
        ///// <param name="state"></param>
        ///// <returns></returns>
        //public ActionResult Order(string code, string state)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //        {
        //            return Content("您拒绝了授权！");
        //        }

        //        WeixinService weixinService = new WeixinService();
        //        ProductModel product = null;
        //        if (!string.IsNullOrEmpty(state))
        //        {
        //            product = service.GetProduct(Convert.ToInt32(state));
        //            if (product == null)
        //            {
        //                return Content("商品信息不存在，或非法进入！1002");
        //            }
        //        }

        //        //通过，用code换取access_token
        //        var openIdResult = OAuthApi.GetAccessToken(TenPayV3Info.AppId, TenPayV3Info.AppSecret, code);
        //        if (openIdResult.errcode != ReturnCode.请求成功)
        //        {
        //            return Content("错误：" + openIdResult.errmsg);
        //        }

        //        // 当前用户OpenID 保存Session
        //        var sessionuser = weixinService.getMember(openIdResult.openid);
        //        //Session["CurrentMember"] = weixinService.getMember("oHWMJj_rVDDSbxN_Y0n7BWMi6zYo");
        //        sessionuser.WxCardList = weixinService.GetAppCard(openIdResult.openid);
        //        Session["CurrentMember"] = sessionuser;

        //        return View(product);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("weixin zhifu failure.", ex);
        //        return Content("error");
        //    }
        //}

        ///// <summary>
        ///// 订单详细
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public ActionResult OrderDetails(int id)
        //{
        //    OrderService oservice = new OrderService();
        //    OrderModel order = oservice.GetOrderDetails(id);
        //    return View(order);
        //}

        ///// <summary>
        ///// 提交订单
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult OrderSubmit(OrderModel model)
        //{
        //    logger.Info("OrderSubmit Start");
        //    OrderService service = new OrderService();
        //    string tid = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);
        //    MemberModel member = (MemberModel)Session["CurrentMember"];
        //    if (member == null)
        //    {
        //        return Content("当前微信客户不存在！");
        //    }

        //    // 验证卡券
        //    string WeixinCardID = "";
        //    decimal discount = 0;   // 卡券折扣金额（代金券）
        //    if (!string.IsNullOrEmpty(model.WeixinCardCode))
        //    {
        //        var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);
        //        var result = CardApi.CardConsume(accessToken, model.WeixinCardCode);
        //        if (result.errcode != ReturnCode.请求成功)
        //        {
        //            logger.Warn("微信卡券核销失败。code：" + model.WeixinCardCode + result.errmsg);
        //            return Content("微信卡券核销失败！请重新下单");
        //        }
        //        WeixinCardID = result.card.card_id;
        //        discount = wservice.GetCardDis(WeixinCardID);
        //    }
        //    if ((Convert.ToDecimal(model.Price) * model.Num) - discount == Convert.ToDecimal(model.Payment))
        //    {
        //        // 金额正确
        //    }
        //    else
        //    {
        //        logger.Warn("订单金额不对。Payment：" + model.Payment + ",Price*Num-discount" + ((Convert.ToDecimal(model.Price) * model.Num) - discount));
        //    }

        //    Order order = new Order
        //    {
        //        Tid = tid,
        //        HostID = 5,
        //        Status = "0",    // 未支付
        //        Payment = model.Payment,
        //        BuyerNick = member.NickName,
        //        BuyerOpenid = member.OpenID,
        //        NumIid = model.NumIid,
        //        Title = model.Title,
        //        Num = model.Num,
        //        Price = model.Price,
        //        Created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        CreatedDate = DateTime.Now,
        //        //ProductImg = result.product_img,
        //        //ProductSku = result.product_sku,
        //        ReceiverName = model.ReceiverName,
        //        ReceiverPhone = model.ReceiverPhone,
        //        ReceiverMobile = model.ReceiverMobile,
        //        ReceiverAddress = model.ReceiverAddress,
        //        // ReceiverCity = result.receiver_city,
        //        // ReceiverProvince = result.receiver_province,
        //        OrderFrom = "5",
        //        BuyerMessage = model.Remarks,
        //        InvoiceTitle = model.InvoiceTitle,
        //        NeedInvoice = (model.NeedInvoice == "true" ? "1" : "0"),
        //        WeixinCardID = WeixinCardID,
        //        WeixinCardCode = model.WeixinCardCode
        //    };
        //    service.AddOrder(order);
        //    model.Tid = order.Tid;

        //    // 保存客人地址
        //    if (member.AddressList.Count == 0)
        //    {
        //        wservice.AddAdress(new MemberAddress
        //        {
        //            MemberID = member.MemberID,
        //            HostID = member.HostID,
        //            ConsigneeAddress = model.ReceiverAddress,
        //            ConsigneeAlias = "默认",
        //            ConsigneeMobile = model.ReceiverPhone,
        //            ConsigneeName = model.ReceiverName,
        //            CreatedDate = DateTime.Now,
        //            IsDefault = 1
        //        });
        //    }

        //    //判断是否正在微信端
        //    var userAgent = Request.UserAgent;
        //    if (BroswerUtility.SideInWeixinBroswer(HttpContext))
        //    {
        //        logger.Info("OrderSubmit End");
        //        //正在微信端，直接跳转到微信支付页面
        //        return RedirectToAction("Pay", new { orderId = model.Tid });
        //    }
        //    else
        //    {
        //        //在PC端打开，提供二维码扫描进行支付
        //        return View(model);
        //    }
        //}

        //[HttpPost]
        //public ActionResult OrderPayResult(string id, string payTime)
        //{
        //    logger.Info("更新订单状态tid" + id);
        //    OrderService service = new OrderService();
        //    service.OrderPayUpdate(id, payTime);
        //    return Content("sucess");
        //}

        ///// <summary>
        ///// 获取用户的OpenId
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult Pay(string orderId)
        //{
        //    logger.Info("Pay Start");
        //    var returnUrl = string.Format("http://wap.sh-cct.cn/Line/JsApi");
        //    var url = OAuthApi.GetAuthorizeUrl(TenPayV3Info.AppId, returnUrl, orderId, OAuthScope.snsapi_userinfo);

        //    return Redirect(url);
        //}

        //public ActionResult JsApi(string code, string state)
        //{
        //    logger.Info("JsApi Start");
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //        {
        //            return Content("您拒绝了授权！");
        //        }

        //        //if (!state.Contains("|"))
        //        //{
        //        //    //这里的state其实是会暴露给客户端的，验证能力很弱，这里只是演示一下
        //        //    //实际上可以存任何想传递的数据，比如用户ID，并且需要结合例如下面的Session["OAuthAccessToken"]进行验证
        //        //    return Content("验证失败！请从正规途径进入！1001");
        //        //}

        //        OrderService service = new OrderService();
        //        OrderModel order = null;
        //        if (!string.IsNullOrEmpty(state))
        //        {
        //            //获取产品信息
        //            //var stateData = state.Split('|');
        //            order = service.GetOrderDetails(state);
        //            if (order == null)
        //            {
        //                return Content("订单不存在，或非法进入！1002");
        //            }
        //            ViewData["order"] = order;
        //        }

        //        //通过，用code换取access_token
        //        var openIdResult = OAuthApi.GetAccessToken(TenPayV3Info.AppId, TenPayV3Info.AppSecret, code);
        //        if (openIdResult.errcode != ReturnCode.请求成功)
        //        {
        //            return Content("错误：" + openIdResult.errmsg);
        //        }

        //        string timeStamp = "";
        //        string nonceStr = "";
        //        string paySign = "";

        //        //创建支付应答对象
        //        RequestHandler packageReqHandler = new RequestHandler(null);
        //        //初始化
        //        packageReqHandler.Init();

        //        DateTime dt = DateTime.UtcNow;
        //        timeStamp = GetTimestamp(dt);
        //        nonceStr = TenPayV3Util.GetNoncestr();

        //        //设置package订单参数
        //        packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);                        //公众账号ID
        //        packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);                       //商户号
        //        packageReqHandler.SetParameter("nonce_str", nonceStr);                              //随机字符串
        //        packageReqHandler.SetParameter("body", order.Title);                                //商品信息
        //        packageReqHandler.SetParameter("out_trade_no", order.Tid);                          //商家订单号
        //        packageReqHandler.SetParameter("total_fee", string.Format("{0:##0}", (Convert.ToDecimal(order.Payment) * 100)));   //商品金额,以分为单位(money * 100).ToString()
        //        packageReqHandler.SetParameter("spbill_create_ip", Request.UserHostAddress);        //用户的公网ip，不是商户服务器IP
        //        packageReqHandler.SetParameter("notify_url", TenPayV3Info.TenPayV3Notify);          //接收财付通通知的URL
        //        packageReqHandler.SetParameter("trade_type", TenPayV3Type.JSAPI.ToString());        //交易类型
        //        packageReqHandler.SetParameter("openid", openIdResult.openid);                      //用户的openId

        //        string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
        //        packageReqHandler.SetParameter("sign", sign);                       //签名

        //        string data = packageReqHandler.ParseXML();
        //        //logger.Info("zhifu XML:" + data);

        //        var result = TenPayV3.Unifiedorder(data);

        //        //logger.Info("zhifu return XML:" + result);
        //        var res = XDocument.Parse(result);
        //        string returnCode = res.Element("xml").Element("return_code").Value;
        //        string resultCode = res.Element("xml").Element("result_code").Value;

        //        if (returnCode == "SUCCESS" && resultCode != "FAIL")
        //        {
        //            string prepayId = res.Element("xml").Element("prepay_id").Value;
        //            //设置支付参数
        //            RequestHandler paySignReqHandler = new RequestHandler(null);
        //            paySignReqHandler.SetParameter("appId", TenPayV3Info.AppId);
        //            paySignReqHandler.SetParameter("timeStamp", timeStamp);
        //            paySignReqHandler.SetParameter("nonceStr", nonceStr);
        //            paySignReqHandler.SetParameter("package", string.Format("prepay_id={0}", prepayId));
        //            paySignReqHandler.SetParameter("signType", "MD5");
        //            paySign = paySignReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);

        //            ViewData["appId"] = TenPayV3Info.AppId;
        //            ViewData["timeStamp"] = timeStamp;
        //            ViewData["nonceStr"] = nonceStr;
        //            ViewData["package"] = string.Format("prepay_id={0}", prepayId);
        //            ViewData["paySign"] = paySign;
        //            ViewData["PayTime"] = dt.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");

        //            logger.Info("JsApi End");
        //            return View();
        //        }
        //        else
        //        {
        //            string returnMsg = res.Element("xml").Element("return_msg").Value;
        //            string errorMsg = res.Element("xml").Element("err_code_des").Value;

        //            return Content(string.Format("返回值：{0},错误信息：{1}", returnMsg, errorMsg));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("weixin zhifu failure.", ex);
        //        return Content("error");
        //    }
        //}

        /// <summary>
        /// 微信支付返回结果页
        /// </summary>
        /// <returns></returns>
        public ActionResult PayNotifyUrl()
        {
            ResponseHandler resHandler = new ResponseHandler(null);

            string return_code = resHandler.GetParameter("return_code");
            string return_msg = resHandler.GetParameter("return_msg");

            string res = null;

            resHandler.SetKey(TenPayV3Info.Key);
            //验证请求是否从微信发过来（安全）
            if (resHandler.IsTenpaySign())
            {
                res = "success";

                //正确的订单处理
            }
            else
            {
                res = "wrong";

                //错误的订单处理
            }

            var fileStream = System.IO.File.OpenWrite(Server.MapPath("~/1.txt"));
            fileStream.Write(Encoding.Default.GetBytes(res), 0, Encoding.Default.GetByteCount(res));
            fileStream.Close();

            string xml = string.Format(@"<xml>
   <return_code><![CDATA[{0}]]></return_code>
   <return_msg><![CDATA[{1}]]></return_msg>
</xml>", return_code, return_msg);

            return Content(xml, "text/xml");
        }

        private static string GetTimestamp(DateTime dt)
        {
            TimeSpan ts = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}