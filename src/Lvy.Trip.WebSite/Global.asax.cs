using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using PetaPoco;
using System.Collections.Generic;
using System.Text.Json;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lvy.Trip.WebSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            RegisterCache();
        }

        private void RegisterCache()
        {
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