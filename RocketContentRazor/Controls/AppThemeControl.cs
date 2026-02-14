using DNNrocketAPI.Components;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using RocketContentAPI.Components;
using RocketContentRazor.Models;
using Simplisity;
using System;

namespace RocketContentRazor.Controls
{
    public class AppThemeControl : RazorModuleControlBase, IPageContributor
    {
        private const string _systemkey = "rocketcontentapi";
        private string _moduleRef;
        private SessionParams _sessionParam;

        public AppThemeControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketContentRazor/App_LocalResources/RocketContent.resx";
        }

        public override string ControlName => "AppTheme";

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {              
                // Set page title
                context.PageService.SetTitle("Rocket Content App Theme");
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogSystem($"AppThemeControl.ConfigurePage ERROR: {ex.Message}");
                DNNrocketAPI.Components.LogUtils.LogException(ex);
            }
        }

        public override IRazorModuleResult Invoke()
        {
            try
            {
                _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = ModuleContext.TabId;
                _sessionParam.ModuleId = ModuleContext.ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                // Set cookie like the original ASCX control
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                // Get admin header content like the original ASCX control
                var adminHeader = RocketContentAPIUtils.DisplaySystemView(
                    PortalSettings.PortalId, 
                    _moduleRef, 
                    _sessionParam, 
                    "AdminHeader.cshtml");

                // Get rendered app theme content from RocketContentAPI
                var strOut = RocketContentAPIUtils.DisplaySystemView(
                    PortalSettings.PortalId, 
                    _moduleRef, 
                    _sessionParam, 
                    "AppThemeAdmin.cshtml", 
                    true, 
                    false);

                // Combine admin header with main content if both exist
                var combinedContent = "";
                if (!string.IsNullOrEmpty(adminHeader))
                    combinedContent += adminHeader;
                if (!string.IsNullOrEmpty(strOut))
                    combinedContent += strOut;

                // Create simple view model with just the rendered HTML
                var model = new ContentViewModel
                {
                    RenderedContent = combinedContent
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketContentRazor AppTheme Error", ex.Message);
            }
        }
    }
}