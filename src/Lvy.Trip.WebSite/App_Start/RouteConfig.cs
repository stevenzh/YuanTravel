using Lvy.Web.Common.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lvy.Trip.WebSite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "booking", // Route name
                "Online/Booking/{tourId}",// URL with parameters
                new { controller = "Online", action = "Booking", tourId = UrlParameter.Optional }
            );

            routes.MapRoute(
                "SearchProduct", // Route name
                "Online/SearchProduct/{arriveDest}/{LineType}",// URL with parameters
                new { controller = "Online", action = "SearchProduct", LineType = UrlParameter.Optional, arriveDest = UrlParameter.Optional }
            );
            //routes.MapRoute(
            //"SearchProduct1", // Route name
            //"Online/SearchProduct/{arriveDest}",// URL with parameters
            //new { controller = "Online", action = "SearchProduct", arriveDest = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { TenantAccess = new TenantRouteConstraint() }
            );
        }
    }
}
