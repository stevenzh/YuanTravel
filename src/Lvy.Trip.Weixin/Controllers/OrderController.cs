using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using log4net;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.AdvancedAPIs;

namespace CCT.Weixin.Controllers
{

    [Authorize]
    public class OrderController : Controller
    {

        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];
        private static readonly ILog logger = LogManager.GetLogger(typeof(OrderController));


        //OrderService service = new OrderService();
        //WeixinService mservice = new WeixinService();
        //UserService tservice = new UserService();
        //EmployeeService eservice = new EmployeeService();

        //// GET: Order
        //public ActionResult Index(OrderQModel qmodel)
        //{
        //    qmodel.OrderPageList = service.GetOrderPageList(qmodel);
        //    return View(qmodel);
        //}
        //public ActionResult PageList(OrderQModel qmodel)
        //{
        //    var OrderList = service.GetOrderPageList(qmodel);
        //    return PartialView("PageList", OrderList);
        //}

        ///// <summary>
        ///// 订单详细
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public ActionResult Details(int id)
        //{
        //    OrderModel order = service.GetOrderDetails(id);
        //    ViewData["Tid"] = order.Tid;
        //    List<SelectListItem> items2 = new SelectList(dbcontent.Dictionaries.Where(t => t.Identifier == "OPOrderStatus").ToList(), "KeyValue", "Contents").ToList();
        //    ViewBag.OrderStatus = items2;

        //    // 用户列表
        //    List<SelectListItem> items1 = new SelectList(context.TFJ105.ToList(), "USER_CD", "USER_NM").ToList();
        //    // List<SelectListItem> items1 = new SelectList(eservice.GetEmployeeList(), "USER_CD", "USER_NM").ToList();  // 新数据库
        //    items1.Insert(0, new SelectListItem { Text = "请选择", Value = "" });
        //    ViewBag.Worker = items1;

        //    MemberModel member = mservice.getMember(order.BuyerOpenid);
        //    if (member != null)
        //    {
        //        ViewData["OpenID"] = member.OpenID;
        //        ViewData["Member"] = member;
        //    }

        //    return View(order);
        //}


        ///// <summary>
        ///// 客户微信记录
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public ActionResult MessageList(MemberModel model)
        //{
        //    ViewData["OpenID"] = model.OpenID;
        //    model.MessagePageList = mservice.GetMessages(model.OpenID, model.MessagePagedSetting.PagedSize, model.MessagePagedSetting.PagedSize);
        //    return PartialView("MessageList", model);
        //}
        ///// <summary>
        ///// 订单操作记录
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public ActionResult LogList(OrderModel model)
        //{
        //    ViewData["Tid"] = model.Tid;
        //    model.OrderLogs = service.GetOrderLogs(model.Tid);
        //    return PartialView("LogList", model);
        //}

        ///// <summary>
        ///// 订单更新（同步微新订单数据）
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult Update()
        //{
        //    int row = service.GetWeixinOrder();
        //    return Content("取得微信订单" + row + "条");
        //}

        ///// <summary>
        ///// 客服修改订单状态
        ///// </summary>
        ///// <param name="log"></param>
        ///// <param name="OpenID"></param>
        ///// <returns></returns>
        //public ActionResult WriteLog(OrderLogModel model)
        //{
        //    TFJ105 user = Session["AdminUser"] as TFJ105;
        //    // EmployeeModel user = Session["AdminUser"] as EmployeeModel; // 新数据
        //    try
        //    {
        //        OrderLog log = new OrderLog
        //        {
        //            Tid = model.Tid,
        //            Description = model.Description,
        //            Status = model.NewStatus,
        //            CreatedBy = 1, // user.USER_CD,
        //            SendMessage = model.SendMessage,
        //            Created = DateTime.Now
        //        };

        //        /// 改变订单状态
        //        TopWeb.DAL.Model.Order od = dbcontent.Orders.Where(t => t.Tid == model.Tid).FirstOrDefault();
        //        od.OpStatus = model.NewStatus;

        //        Member mb = dbcontent.Members.Where(t => t.OpenID == model.OpenID && t.Subscribe == "1").FirstOrDefault();
        //        if (model.SendMessage == "1") // 发送微信消息
        //        {
        //            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);
        //            var testData = new OrderWeixinData()
        //            {
        //                first = new TemplateDataItem(mb.NickName + "你好！订单状态改变通知。"),
        //                orderId = new TemplateDataItem(model.Tid.ToString()),
        //                productName = new TemplateDataItem(od.Title),
        //                orderPrice = new TemplateDataItem(od.Payment),
        //                orderStatus = new TemplateDataItem(model.NewStatusValue),
        //                remark = new TemplateDataItem(model.Description)
        //            };
        //            string url = "http://wx.sh-cct.cn/member/orderdetails/" + log.Tid;
        //            var result = TemplateApi.SendTemplateMessage(accessToken, model.OpenID, "_ju2dofElFheFcn2fGysDvM-0yI2GtTGBhmdbjT58Jg", "#FF0000", url, testData);
        //            if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
        //                log.SendResult = "1";
        //            else
        //                log.SendResult = "0";
        //        }

        //        dbcontent.OrderLogs.Add(log);
        //        dbcontent.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("订单操作流程发送失败", ex);
        //        return Content("error");
        //    }
        //    return Content("ok");
        //}

        ///// <summary>
        ///// 设置OP
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public ActionResult UpdateOrder(OrderModel model)
        //{
        //    try
        //    {
        //        TopWeb.DAL.Model.Order mb = dbcontent.Orders.Where(t => t.Tid == model.Tid).FirstOrDefault();
        //        if (mb != null)
        //        {
        //            mb.Operator = model.OperatorId;
        //        }
        //        mb.RealName = model.RealName;
        //        if (model.ReadyDate != default(DateTime))
        //            mb.ReadyDate = model.ReadyDate;
        //        if (model.FinishDate != default(DateTime))
        //            mb.FinishDate = model.FinishDate;
        //        mb.Remarks = model.Remarks;
        //        dbcontent.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("订单设置OP失败", ex);
        //        return Content("error");
        //    }
        //    return Content("ok");
        //}
    }
}