using Arch.Common;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Booking;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Weixin;
using Lvy.Trip.Weixin.Models;
using Lvy.VModels.Booking;
using Lvy.VModels.Order;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 订单管理
    /// </summary>
    [Authorize]
    public class BookingController : AdminBaseController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BookingController));

        private MemberBiz _memberBiz = new MemberBiz();
        private AccountBiz _accountBiz = new AccountBiz();
        private OrderBiz _orderBiz = new OrderBiz();
        private TpLineTourPlanBiz _planBiz = new TpLineTourPlanBiz();
        private CustomerBiz _customerBiz = new CustomerBiz();
        private TpLineAdminBiz _lineAdminBiz = new TpLineAdminBiz();
        private BookingBiz _bookingBiz = new BookingBiz();
        private TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private TpPriceBiz _priceBiz = new TpPriceBiz();
        private TeamBiz _teamBiz = new TeamBiz();
        private TpLineBiz _lineBiz = new TpLineBiz();
        private TpTourPlanBiz _tourPlanBiz = new TpTourPlanBiz();
        private TpOrderPayInBiz payInBiz = new TpOrderPayInBiz();

        /// <summary>
        /// 我的订单（以当前用户为销售的订单）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Index(TpOrderVModel model)
        {
            model.OwnerCode = GlobalContext.Current.UserInfo.OwnerCode;

            // 分销商
            if (string.IsNullOrEmpty(model.Manager))
            {
                CrmAccountModel user = GlobalContext.Current.UserInfo;
                model.Manager = user.Name;
            }

            if (model.OutDateRange.IsNullOrEmpty())
                model.OutDateRange = DateTime.Today.ToDateFormat();

            model = _orderBiz.GetPageList(model, UserInfo);

            if (Request.IsAjaxRequest())
                return PartialView("PageList", model);

            return View(model);
        }

        public ActionResult PageList(TpOrderVModel model)
        {
            model.OwnerCode = GlobalContext.Current.UserInfo.OwnerCode;
            // OP
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调"))
            {
                model.CrmTeamId = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2).FirstOrDefault().TeamID;
            }

            _orderBiz.GetPageList(model, UserInfo);
            return PartialView("PageList", model);
        }

        /// <summary>
        /// OP开单
        /// </summary>
        /// <returns></returns>
        public ActionResult Reserve()
        {
            return View();
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Details(string id, string code, string state)
        {
            if (id == "undefined") return Content("");

            InWeixin(code, state);

            if (GlobalContext.Current.UserInfo == null)
            {
                string urls = "/Account/Login?url=" + Request.Url;
                return Redirect(urls);
            }

            var vModel = new OrderEditVModel();
            vModel.Order = _orderBiz.GetOrderLineTourist(id);
            vModel.LineModel = vModel.Order.Line;

            //上车点数据加载
            // vModel.LineBusPoints = InitBusPoints(vModel.Order);
            vModel.Prices = _orderBiz.GetPricesByTourId(vModel.Order.TourId);
            //获取游客信息
            vModel.Travellers = vModel.Order.TravellerModels;
            vModel.ListTourPayInModel = payInBiz.GetPayInList(id);
            vModel.FileList = _orderBiz.GetOrderFileList(id);
            foreach (var person in vModel.Travellers)
            {
                person.FileList = vModel.FileList.Where(t => t.SourceType == ((int)FileSourceType.Tourist).ToString() && t.KeyId == person.Id).ToList();
            }
            vModel.TourPlan = _planBiz.GetTourById(vModel.Order.TourId);

            //文件类型
            ViewBag.FileEnum = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 1).ToSelectListFor();

            try
            {
                //JSAPI
                ViewBag.JsApi = JSSDKHelper.GetJsSdkUiPackage(appId, secret, Request.Url.AbsoluteUri);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
            }

            ViewBag.CustomerList = _customerBiz.GetCustomerBySales(GlobalContext.Current.UserInfo.Code).ToSelectListFor(t => t.Code, t => t.Name);
            ViewBag.ContactList = _customerBiz.GetContactListByCustomerCode(vModel.Order.BookingCustomer).ToSelectListFor(t => t.Code, t => t.Name);

            List<SelectListItem> items1 = new List<SelectListItem>();
            items1.Add(new SelectListItem { Value = "1", Text = "自行结算", Selected = (vModel.Order.SettlePlatForm == 1 ? true : false) });
            items1.Add(new SelectListItem { Value = "2", Text = "平台结算", Selected = (vModel.Order.SettlePlatForm == 2 ? true : false) });
            items1.Add(new SelectListItem { Value = "3", Text = "父客户结算", Selected = (vModel.Order.SettlePlatForm == 3 ? true : false) });

            ViewBag.SettleSelect = items1;

            var items = new List<SelectListItem>();
            items.Insert(0, new SelectListItem()
            {
                Text = "请选择",
                Value = ""
            });
            if (vModel.Order.SettlePlatForm == 1)
            {
                //分销商
                items.Add(new SelectListItem()
                {
                    Text = vModel.Order.CustomerName,
                    Value = vModel.Order.BookingCustomer,
                    Selected = true
                });
            }
            else if (vModel.Order.SettlePlatForm == 2)
            {
                //平台
                var list = DictionaryTools.GetCachedCustomerDict().Values.Where(a => a.IsValid == 1 && a.ChannelType == 2).ToList();

                foreach (var item in list)
                {
                    items.Add(new SelectListItem()
                    {
                        Text = item.Name,
                        Value = item.Code,
                        Selected = (vModel.Order.SettleCustomer == item.Code ? true : false)
                    });
                }
            }
            else
            {
                // 父机构
                var parent = _customerBiz.GetById(vModel.Order.SettleCustomer);
                if (parent != null)
                    items.Add(new SelectListItem()
                    {
                        Text = parent.Name,
                        Value = parent.Code,
                        Selected = true
                    });
            }

            ViewBag.SettleList = items;

            return View(vModel);
        }

        public ActionResult GetPlatforms()
        {
            var list = DictionaryTools.GetCachedCustomerDict().Values.Where(a => a.IsValid == 1 && a.ChannelType == 2).ToList();
            return Json(list);
        }

        public ActionResult GetParentCompany(string custCode)
        {
            var model = _customerBiz.GetParentCrmCustomerModel(custCode, GlobalContext.Current.OwnerCode);
            return Json(model);
        }

        public ActionResult OrderInfo(string id)
        {
            var vModel = new OrderEditVModel();
            vModel.Order = _orderBiz.GetOrderLineTourist(id);
            return PartialView("OrderInfo", vModel);
        }

        public ActionResult TouristList(string id)
        {
            var vModel = new OrderEditVModel();
            vModel.Travellers = _orderBiz.GetTravellersByOrderCode(id);
            vModel.FileList = _orderBiz.GetOrderFileList(id);
            foreach (var person in vModel.Travellers)
            {
                person.FileList = vModel.FileList.Where(t => t.SourceType == ((int)FileSourceType.Tourist).ToString() && t.KeyId == person.Id).ToList();
            }

            return PartialView("TouristList", vModel);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="custcode"></param>
        /// <returns></returns>
        public ActionResult GetContactList(string custcode)
        {
            var model = new
            {
                contacts = _customerBiz.GetContactListByCustomerCode(custcode),
            };

            return Json(model);
        }

        public ActionResult TouristInfo(int id)
        {
            var vModel = _orderBiz.GetTravellerById(id);
            vModel.FileList = _orderBiz.GetOrderFileList(vModel.OrderCode).Where(t => t.KeyId == id).ToList();
            return PartialView("TouristInfo", vModel);
        }

        public ActionResult SaveTourist(TpTravellerModel model)
        {
            //_orderBiz.GetOrder(model.OrderCode);
            _orderBiz.UpdateTraveller(model);

            return Content("ok");
        }

        public ActionResult SaveOrder(TpOrderModel model)
        {
            //检查订单游客信息是否完整。不完整不允许确认订单。
            var OrderModel = _orderBiz.GetOrderLineTourist(model.OrderCode);

            OrderModel.BookingCustomer = model.BookingCustomer;
            OrderModel.SettlePlatForm = model.SettlePlatForm;
            OrderModel.SettleCustomer = model.SettleCustomer;

            OrderModel.ContactCode = model.ContactCode;
            OrderModel.Managers = model.Managers;
            OrderModel.ManagerPhone = model.ManagerPhone;
            OrderModel.LinkMan = model.LinkMan;
            OrderModel.LinkPhone = model.LinkPhone;
            OrderModel.RebateInBill = _customerBiz.GetById(model.SettleCustomer).RebateInBill;

            OrderModel.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            OrderModel.ModifiedTime = DateTime.Now;
            _orderBiz.Update(OrderModel);
            return Content("ok");
        }

        /// <summary>
        /// 微信端上传图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="orderCode"></param>
        /// <param name="remarks"></param>
        /// <param name="sourceType"></param>
        /// <param name="keyId">游客ID ，缴款ID </param>
        /// <returns></returns>
        public ActionResult GetMedia(string id, string orderCode, string remarks, string sourceType, int keyId)
        {
            logger.Info("param id:" + id);
            logger.Info("param orderCode:" + orderCode);
            logger.Info("param remarks:" + remarks);
            logger.Info("param sourceType:" + sourceType);
            logger.Info("param keyId:" + keyId);

            try
            {
                var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);

                MemoryStream ms = new MemoryStream();
                MediaApi.Get(accessToken, id, ms);
                ms.Seek(0, SeekOrigin.Begin);

                string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}.jpg", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4));
                var request = new UploadFileRequest();
                request.FileName = filename;
                request.FileStream = Toolkit.Image.StreamToBytes(ms);
                request.VirtualPath = @"order\" + orderCode;
                UploadServiceClient client = new UploadServiceClient();

                UploadFileResponse response = client.UploadFile(request);
                string url = response.FilePath + response.FileName;

                TpOrderFileModel entity = new TpOrderFileModel
                {
                    FileName = remarks + ".jpg",
                    FilePath = url,
                    OrderCode = orderCode,
                    Remark = remarks,
                    KeyId = keyId,
                    CreatedTime = DateTime.Now,
                    IsDel = 0,
                    CreatedBy = GlobalContext.Current.UserInfo.Code,
                    MediaType = MediaType.image.ToString(),
                    SourceType = sourceType
                };
                _orderBiz.AddOrderFileInfo(entity);

                // 文件上传成功，重要文件通知OP  付款凭证
                if (entity.SourceType == "20")
                {
                    var order = _orderBiz.GetOrderByOrderCode(orderCode);
                    if (order.TraceState < 40)
                    {
                        order.TraceState = 40;
                        _orderBiz.Update(order);

                        // 更新成功，发送微信消息通知OP
                        var op = _lineAdminBiz.GetLineAdmin(orderCode);
                        LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderCode, (op == null ? "" : op.Code), GlobalContext.Current.UserInfo.Code, "客户确认账单已上传。", 0);
                    }
                }

                var json = new
                {
                    Success = "ok",
                    Message = url
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

        public ActionResult DownLoadFile(int id)
        {
            TpOrderFileModel model = _orderBiz.GetOrderFileModel(id);
            if (model == null)
                return null;

            // 文件检查
            try
            {
                WebRequest.Create(AppSetting.Get("UploadFileRoot") + model.FilePath);
            }
            catch (Exception ex)
            {
                logger.Error("File not Found.", ex);
                return null;
            }
            if (model.SourceType == "3")
            {
                // 记录下载日志
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, model.OrderCode, "", GlobalContext.Current.UserInfo.Code, "账单下载，修订号：" + model.Revision, 0);
            }
            else if (model.SourceType == "5")
            {
                // 记录下载日志
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, model.OrderCode, "", GlobalContext.Current.UserInfo.Code, "出团通知下载，修订号：" + model.Revision, 0);
            }

            // 获取文件
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

        /// <summary>
        /// 设置OP
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UpdateOrderStatus(string orderCode, string newStatus, string statusText)
        {
            try
            {
                _orderBiz.UpdateOrderStatus(orderCode, newStatus);

                // 更新成功，发送微信消息通知OP
                var op = _lineAdminBiz.GetLineAdmin(orderCode);
                if (op != null && newStatus == "20")
                {
                    LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderCode, op.Code, GlobalContext.Current.UserInfo.Code, "状态变更：" + statusText);
                }
                else
                {
                    LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderCode, "", GlobalContext.Current.UserInfo.Code, "状态变更：" + statusText);
                }
            }
            catch (Exception ex)
            {
                logger.Error("订单设置OP失败", ex);
                return Content("error");
            }
            return Content("ok");
        }

        #region 开单

        public ActionResult CreateOpOrder(int tourId)
        {
            BookingVModel vModel = new BookingVModel();
            vModel.Tour = _bookingBiz.GetTourById(tourId);
            vModel.Quota = _quotaBiz.GetQuotaByTour(tourId);
            vModel.PriceModels = _priceBiz.GetValidPrices(tourId);

            ViewBag.SalesOfTeam = _teamBiz.GetTeams("5", OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            //2。加载销售员列表。
            ViewBag.Salers = _customerBiz.GetTeamSales(UserInfo.OwnerCode).ToSelectListFor(k => k.Code, v => v.Name);

            return View(vModel);
        }

        public ActionResult GetTeamUserByTeamId(string teamId)
        {
            var sales = _customerBiz.GetTeamUsersByTeamId(teamId, UserInfo.OwnerCode);

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
                    sales = _customerBiz.GetTeamSales(OwnerCode).Where(a => a.Code == GlobalContext.Current.UserInfo.Code).ToList();
                }
            }

            #endregion 根据用户角色 锁定过滤

            return Json(sales, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// OP 开单 ？
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult SaveOpOrder(BookingVModel vModel)
        {
            vModel.Tour = _tourPlanBiz.GetTourById(vModel.TourId);
            vModel.Quota = _quotaBiz.GetQuotaByTour(vModel.TourId);
            vModel.LineModel = _lineBiz.GetLineById(vModel.Tour.LineId);
            vModel.PriceModels = _orderBiz.GetPricesByTourId(vModel.TourId);

            // 余位审核
            if (vModel.Quota.UseQuota < vModel.TravellerCount)
            {
                return Json(new { StateCode = OrderResultState.Code110 });
            }

            string orderCode = "";
            OrderResultState orderState = _orderBiz.SaveOpOrderTrans(vModel, ref orderCode, UserInfo);

            // 占位
            _orderBiz.FreeQuota(vModel.TourId,"", GlobalContext.Current.UserInfo.Code);

            if (orderState == OrderResultState.Code100)
            {
                // 记录日志
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderCode, vModel.SalerCode, GlobalContext.Current.UserInfo.Code, "开单");

                // 开单通知销售
                var sales = _accountBiz.GetAccountCustomer(vModel.SalerCode);
                if (!String.IsNullOrEmpty(sales.OpenID))
                {
                    var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                    string first = string.Format("{0}开单成功", sales.Name);
                    // 新订单生成通知  开单
                    var testData = new OrderData()
                    {
                        first = new TemplateDataItem(first),
                        OrderId = new TemplateDataItem(orderCode),
                        ProductName = new TemplateDataItem(vModel.LineModel.LineName),
                        ProductId = new TemplateDataItem(vModel.LineModel.LineId),
                        remark = new TemplateDataItem("OP 开单，快去补充客人资料。")
                    };
                    //   string url = "http://yuan.sh-cct.cn/booking/details/" + param1;
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fbooking%2Fdetails%2F" + orderCode + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";

                    TemplateApi.SendTemplateMessage(accessToken, sales.OpenID, "jFkZkkv74K27HcZ6xnyaNV5elqSX7IdcYQHI4Nus170", url, testData);
                }
            }

            return Json(new { StateCode = ((int)orderState).ToString() });
        }

        #endregion 开单

        /// <summary>
        /// 复制与后台占位 （OrderController）
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult OrdersMakeSure(string OrderCode)
        {
            TpOrderVModel orderVModel = new TpOrderVModel();
            //检查订单游客信息是否完整。不完整不允许确认订单。
            orderVModel.OrderModel = _orderBiz.GetOrderLineTourist(OrderCode);

            orderVModel.OrderModel.OrderState = 2;
            // orderVModel.OrderModel.JieSuanState = 2;//未结算（订单确认，结算状态变更）
            orderVModel.OrderModel.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            orderVModel.OrderModel.ModifiedTime = DateTime.Now;
            _orderBiz.Update(orderVModel.OrderModel);
            // 更新库存
            _orderBiz.FreeQuota(orderVModel.OrderModel.TourId, OrderCode, GlobalContext.Current.UserInfo.Code);

            // 记录日志
            LogBiz.WriteOrderLog(UserInfo.OwnerCode, OrderCode, "", GlobalContext.Current.UserInfo.Code, "确认占位.", 0);

            // 通知销售
            var sales = _accountBiz.GetById(orderVModel.OrderModel.SalerCode);
            if (sales != null && !String.IsNullOrEmpty(sales.OpenID))
            {
                var first = string.Format("{0}您好,订单状态变更。", sales.Name);
                var remark = string.Format(@"客户名称：{0}
出团日期：{1}", orderVModel.OrderModel.CustomerName, orderVModel.OrderModel.OutDate.ToDateFormat());

                var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
                // 订单状态变更通知
                var testData = new OrderWeixinData()
                {
                    first = new TemplateDataItem(first),
                    orderId = new TemplateDataItem(OrderCode),
                    productName = new TemplateDataItem(orderVModel.OrderModel.Line.LineName),
                    orderPrice = new TemplateDataItem(orderVModel.OrderModel.InvoiceAmount.ToString()),
                    orderStatus = new TemplateDataItem("确认占位"),
                    remark = new TemplateDataItem(remark)
                };
                //string url = "http://yuanwx.sh-cct.cn/booking/details/" + param1;
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx5048293842056c7e&redirect_uri=http%3A%2F%2Fyuanwx.sh-cct.cn%2Fbooking%2Fdetails%2F" + OrderCode + "&response_type=code&scope=snsapi_base&state=JeffreySu#wechat_redirect";

                TemplateApi.SendTemplateMessage(accessToken, sales.OpenID, "8i7VY_GnnYnvTfmDRmntS079TzfJK2KmXV3LUOeOHM0", url, testData);
            }

            return Content("{\"code\"=\"1\", \"msg\"=\"success\"}");
        }
    }
}