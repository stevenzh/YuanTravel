using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.Mvc.ModelBinders;
using PetaPoco;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lvy.Trip.AdminSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.Add(typeof(string), new StringTrimModelBinder());
            // ModelBinders.Binders.Add(typeof(int), new DateTimeModelBinder());

            DictionaryBiz.GetCacheDests();

            // 任务处理
            ISchedulerFactory sf = new Quartz.Impl.StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            sched.Start();

            RegisterCache();
        }

        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    Exception ex = Server.GetLastError();
        //    if (ex is HttpException && ((HttpException)ex).GetHttpCode() == 404)
        //    {
        //        Response.Redirect("~/Error/NotFound");
        //    }
        //}

        private void RegisterCache()
        {
            // 商户列表保存到缓存
            Database db = new Database("YuanDB");
            var hosts = db.Fetch<SysPlatformModel>("SELECT * FROM SysPlatform ");
            foreach (var bag in hosts)
            {
                if (string.IsNullOrEmpty(bag.Profile))
                    bag.ProfileModels = new List<KeyValueBean>();
                else
                    bag.ProfileModels = JsonSerializer.Deserialize<List<KeyValueBean>>(bag.Profile);
                foreach (var u in bag.UrlList)
                {
                    bag.CacheKey = Consts.HostCode + u;
                    bag.Url = u;
                    CacheContext.Current.Add(bag.CacheKey, bag);
                }
            }
        }
    }
}