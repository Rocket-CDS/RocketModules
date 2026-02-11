using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using DNNrocketAPI.Components;
using RocketContentAPI.Components;
using Simplisity;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RocketContentRazor.Services
{
    [SupportedModules("RocketContentRazor")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ContentController : DnnApiController
    {
        private const string _systemkey = "rocketcontentapi";

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetArticleData(int portalId, int moduleId)
        {
            try
            {
                var moduleRef = portalId + "_ModuleID_" + moduleId;
                var sessionParam = new SessionParams(new SimplisityInfo());
                sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                sessionParam.ModuleId = moduleId;
                sessionParam.ModuleRef = moduleRef;

                var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, _systemkey, moduleId, -1);
                var articleData = RocketContentAPIUtils.GetArticleData(moduleSettings, sessionParam.CultureCode);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    ArticleId = articleData.ArticleId,
                    Exists = articleData.Exists
                });
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage SaveArticle([FromBody] dynamic articleData)
        {
            try
            {
                // This endpoint would handle AJAX saves if needed
                // The existing RocketContentAPI already handles saves through its API
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
