using System;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Web.MvcPipeline.Controllers;
using DotNetNuke.Web.MvcPipeline.Models;
using DotNetNuke.Web.MvcPipeline.ModelFactories;
using System.Web;
using DotNetNuke.Abstractions.ClientResources;
using DotNetNuke.Abstractions.Pages;
using DotNetNuke.Entities.Portals;

namespace RocketDirectoryRazor
{
    /// <summary>
    /// Extended SkinModelFactory that applies Rocket-specific skins for MVC requests.
    /// Implements ISkinModelFactory and decorates the base SkinModelFactory to inject
    /// custom skin selection logic before calling the base implementation.
    /// </summary>
    public class RocketSkinModelFactory : ISkinModelFactory
    {
        private readonly SkinModelFactory _baseSkinModelFactory;

        public RocketSkinModelFactory(
            INavigationManager navigationManager,
            IPaneModelFactory paneModelFactory,
            IClientResourceController clientResourceController,
            IPageService pageService,
            IHostSettings hostSettings,
            IPortalController portalController,
            IApplicationStatusInfo appStatus,
            IEventLogger eventLogger)
        {
            _baseSkinModelFactory = new SkinModelFactory(
                navigationManager, 
                paneModelFactory, 
                clientResourceController, 
                pageService, 
                hostSettings, 
                portalController, 
                appStatus, 
                eventLogger);
        }

        public SkinModel CreateSkinModel(DnnPageController pageController)
        {
            var cookieName = "_SkinSrc" + pageController.PortalSettings.PortalId;

            // Check if we should override the skin for Rocket modules
            var rocketSkinOverride = CheckForRocketSkinOverride(pageController);

            if (!string.IsNullOrEmpty(rocketSkinOverride))
            {
                // Set a temporary cookie that the base SkinModelFactory will use
                var skinPathWithoutExtension = rocketSkinOverride.Replace(".ascx", "");

                var skinCookie = new HttpCookie(cookieName, skinPathWithoutExtension)
                {
                    Path = "/",
                    Expires = DateTime.Now.AddMinutes(1)
                };

                pageController.Response.Cookies.Set(skinCookie);

                if (pageController.Request.Cookies[cookieName] == null)
                {
                    pageController.Request.Cookies.Set(skinCookie);
                }
            }
            else
            {
                // No Rocket skin override needed - clear any existing cookie
                CleanupRocketSkinCookie(pageController, cookieName);
            }

            return _baseSkinModelFactory.CreateSkinModel(pageController);
        }

        /// <summary>
        /// Removes the Rocket skin cookie from both request and response.
        /// </summary>
        private void CleanupRocketSkinCookie(DnnPageController pageController, string cookieName)
        {
            if (pageController.Request.Cookies[cookieName] != null)
            {
                // Expire the cookie immediately to remove it
                var expireCookie = new HttpCookie(cookieName)
                {
                    Path = "/",
                    Expires = DateTime.Now.AddDays(-1)
                };
                pageController.Response.Cookies.Set(expireCookie);

                // Remove from request cookies for this request
                pageController.Request.Cookies.Remove(cookieName);
            }
        }
        /// <summary>
        /// Checks if a Rocket-specific skin override should be applied.
        /// </summary>
        private string CheckForRocketSkinOverride(DnnPageController pageController)
        {
            try
            {
                var rawUrl = pageController.Request.RawUrl;

                // Check for ctl parameter in query string
                var ctlValue = pageController.Request.QueryString["ctl"];
                if (!string.IsNullOrEmpty(ctlValue))
                {
                    return GetRocketSkinForCtl(ctlValue);
                }

                // Check for ctl in URL path (e.g., /ctl/Edit/)
                if (rawUrl.IndexOf("/ctl/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var segments = rawUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < segments.Length - 1; i++)
                    {
                        if (segments[i].Equals("ctl", StringComparison.OrdinalIgnoreCase) && i + 1 < segments.Length)
                        {
                            return GetRocketSkinForCtl(segments[i + 1]);
                        }
                    }
                }
            }
            catch
            {
                // Silently fail - fall back to default behavior
            }

            return null;
        }

        /// <summary>
        /// Determines which Rocket skin to use based on the ctl parameter value.
        /// </summary>
        private string GetRocketSkinForCtl(string ctlValue)
        {
            if (string.IsNullOrEmpty(ctlValue)) return null;

            switch (ctlValue.ToLower())
            {
                case "adminpanel":
                    return "[G]Skins/rocketedit/rocketadmin.ascx";

                case "edit":
                case "rocketedit":
                case "RocketDirectoryRazor":
                case "apptheme":
                case "module":
                case "recyclebin":
                    return "[G]Skins/rocketedit/rocketedit.ascx";

                default:
                    return null;
            }
        }

    }
}
