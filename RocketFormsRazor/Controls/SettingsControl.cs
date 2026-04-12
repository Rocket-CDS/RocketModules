using DNNrocketAPI.Components;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using RocketForms.Components;
using RocketFormsRazor.Models;
using Simplisity;
using System;

namespace RocketFormsRazor.Controls
{
    public class SettingsControl : RazorModuleControlBase, IPageContributor
    {
        private string _moduleRef;

        public SettingsControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketFormsRazor/App_LocalResources/RocketForm.resx";
        }

        public override string ControlName => "Settings";

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {
                context.PageService.SetTitle("Rocket Form Settings");
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
            }
        }

        public override IRazorModuleResult Invoke()
        {
            try
            {
                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                var sessionParam = new SessionParams(new SimplisityInfo());
                sessionParam.TabId = TabId;
                sessionParam.ModuleId = ModuleId;
                sessionParam.ModuleRef = _moduleRef;
                sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                var strOut = RocketFormsUtils.DisplaySystemView(PortalId, _moduleRef, sessionParam, "ModuleSettingsLoad.cshtml");

                var model = new FormViewModel
                {
                    RenderedContent = strOut
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketFormsRazor Settings Error", ex.Message);
            }
        }
    }
}
