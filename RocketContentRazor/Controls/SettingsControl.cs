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
    public class SettingsControl : RazorModuleControlBase, IPageContributor
    {
        private const string _systemkey = "rocketcontentapi";
        private string _moduleRef;
        private SessionParams _sessionParam;

        public SettingsControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketContentRazor/App_LocalResources/RocketContent.resx";
        }

        public override string ControlName => "Settings";

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {              
                // Set page title
                context.PageService.SetTitle("Rocket Content Settings");                
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogSystem($"SettingsControl.ConfigurePage ERROR: {ex.Message}");
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

                // Get rendered settings content from RocketContentAPI
                var strOut = RocketContentAPIUtils.DisplaySystemView(
                    PortalSettings.PortalId, 
                    _moduleRef, 
                    _sessionParam, 
                    "ModuleSettingsLoad.cshtml", 
                    true, 
                    false);

                // Create simple view model with just the rendered HTML
                var model = new ContentViewModel
                {
                    RenderedContent = strOut
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketContentRazor Settings Error", ex.Message);
            }
        }
    }
}

