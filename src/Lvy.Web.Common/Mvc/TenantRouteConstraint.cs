using Lvy.Web.Common.Cache;
using System.Web;
using System.Web.Routing;

namespace Lvy.Web.Common.Mvc
{
    public class TenantRouteConstraint : IRouteConstraint
    {

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var host = Consts.GetTestHost(httpContext.Request.Url.Host);
            var bag = CacheContext.Current.Get(Consts.HostCode + host);

            if (!values.ContainsKey("tenant"))
            {
                values.Add("tenant", bag);
            }

            return true;
        }
    }
}