using DNNrocketAPI.Components;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using RocketDirectoryAPI.Components;
using RocketDirectoryRazor.Models;
using Simplisity;
using System;

namespace RocketDirectoryRazor.Controls
{
    public class SettingsControl : RazorModuleControlBase, IPageContributor
    {
        private string _systemkey = "RocketDirectoryAPI";
        private string _moduleRef;
        private SessionParams _sessionParam;

        public SettingsControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketDirectoryRazor/App_LocalResources/RocketDirectory.resx";
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
                var moduleName = base.ModuleConfiguration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 5) + "api";
                _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = ModuleContext.TabId;
                _sessionParam.ModuleId = ModuleContext.ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                var strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "ModuleSettingsLoad.cshtml");

                // Create simple view model with just the rendered HTML
                var model = new DirectoryViewModel
                {
                    RenderedContent = strOut
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketDirectoryRazor Settings Error", ex.Message);
            }
        }
    }
}

