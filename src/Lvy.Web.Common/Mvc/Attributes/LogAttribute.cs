using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Lvy.Model.Hotel;
using Lvy.Model.System;
using Lvy.Biz.Hotel;
using Lvy.Core.Tools;
using Microsoft.Practices.ServiceLocation;
using System.Web;
using System.Reflection;
using Lvy.Biz.Order;

namespace Lvy.Web.Common.Mvc.Attributes
{
    public class LogAttribute : ActionFilterAttribute
    {
        public int Type { get; set; }
        /// <summary>
        /// 日志记录分发
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            switch (Type)
            {
                case 0:
                    HotelOpartorLog(filterContext);
                    break;
                case 1:
                    OrderOperatorLog(filterContext);
                    break;
            }
        }
        /// <summary>
        /// 酒店日志记录，调用时请注意反射的方法参数中是否存在hotelId，如果不存在则不会记录日志。
        /// </summary>
        /// <param name="filterContext"></param>
        private void HotelOpartorLog(ActionExecutingContext filterContext)
        {
            try
            {
                LoginModel userInfo = filterContext.HttpContext.Session["UserInfo"] as LoginModel;
                string actionName = filterContext.HttpContext.Request.Url.ToString();
                string hotelId = null;
                foreach (var par in filterContext.ActionParameters)
                {
                    dynamic model = par.Value;
                    try
                    {
                        hotelId = model.HotelId.ToString();
                    }
                    catch
                    {
                        continue;
                    }
                }
                if (hotelId == null)//如果找不到hotelId则跳出日志记录，不在记录日志操作
                {
                    return;
                }
                HotelOpartorLogModel log = new HotelOpartorLogModel();
                log.OpeartorTime = DateTime.Now;
                log.Reasion = actionName;
                log.HotelId = Convert.ToInt32(hotelId);
                log.OperatorBy = userInfo.LoginId;
                log.Remark = "";
                IHotelOpartorLogService service = ServiceLocator.Current.GetInstance<IHotelOpartorLogService>();
                service.AddLog(log);
            }
            catch (Exception ex)
            {
                ex.WriteErrorLog();
            }
        }
        /// <summary>
        /// 订单日志操作，调用时请注意反射的方法参数中是否存在OrderId，如果不存在则不会记录日志。
        /// </summary>
        /// <param name="filterContext"></param>
        private void OrderOperatorLog(ActionExecutingContext filterContext)
        {
            try
            {
                LoginModel userInfo = filterContext.HttpContext.Session["UserInfo"] as LoginModel;
                string actionName = filterContext.ActionDescriptor.ActionName;
                HttpRequestBase Request = filterContext.HttpContext.Request;
                string url = Request.Url.ToString();
                string ClientIP = Request.UserHostAddress;
                StringBuilder content = new StringBuilder();
                string orderId = null;
                foreach (var par in filterContext.ActionParameters)
                {
                    content.Append(string.Format("Parameter Name:{0},Parameter Type:{1};", par.Key, par.Value.ToString()));
                    Type t = par.Value.GetType();
                    foreach (PropertyInfo info in t.GetProperties())
                    {
                        content.Append(string.Format("Propertie Name:{0},Propertie Value:{1};", info.Name, GetPropertyValue(par.Value, info.Name)));
                        if (info.Name.ToLower() == "orderid")
                            orderId = info.GetValue(info, null).ToString();
                    }
                }
                if (orderId == null)//不存在订单号则跳出日志记录，不在记录日志
                    return;
                OrderOperatorLogModel model = new OrderOperatorLogModel();
                model.LogContent = content.ToString();
                model.OperatorIP = ClientIP;
                model.OperatorBy = userInfo.LoginId;
                model.OperatorTime = DateTime.Now;
                model.OrderNo = orderId;
                model.Reasion = actionName;
                model.Remark = "";
                model.URL = url;
                IOrderOperatorLogService service = ServiceLocator.Current.GetInstance<IOrderOperatorLogService>();
                service.AddLog(model);
            }
            catch (Exception ex)
            {
                ex.WriteErrorLog();
            }
        }
        /// <summary>
        /// 通过反射查找Object中对应的值
        /// </summary>
        /// <param name="info">需要查找的Object</param>
        /// <param name="field">属性名称</param>
        /// <returns>这个属性存在并且不为null则返回实际的值，属性不存在或者等于null则返回null</returns>
        private object GetPropertyValue(Object info, string field)
        {
            if (info == null) { return null; }
            Type t = info.GetType();
            IEnumerable<System.Reflection.PropertyInfo> property = from pi in t.GetProperties() where pi.Name.ToLower() == field.ToLower() select pi;
            try
            {
                return property.First().GetValue(info, null);
            }
            catch
            {
                return null;
            }
        }
    }
}
