using Lvy.Web.Common.Mvc;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lvy.Trip.AdminSite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "User", action = "Login", id = UrlParameter.Optional }, // Parameter defaults
                constraints: new { TenantAccess = new TenantRouteConstraint() }
            );
        }
    }
}