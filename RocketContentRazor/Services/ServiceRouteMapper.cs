using DotNetNuke.Web.Api;

namespace RocketContentRazor.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("RocketContentRazor", "default", "{controller}/{action}", new[] { "RocketContentRazor.Services" });
        }
    }
}
