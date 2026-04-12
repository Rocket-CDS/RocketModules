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
    public class AppThemeControl : RazorModuleControlBase, IPageContributor
    {
        private string _moduleRef;

        public AppThemeControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketFormsRazor/App_LocalResources/RocketForm.resx";
        }

        public override string ControlName => "AppTheme";

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {
                context.PageService.SetTitle("Rocket Form App Theme");
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
                DNNrocketUtils.SetCookieValue("simplisity_language", sessionParam.CultureCode);

                var adminHeader = RocketFormsUtils.DisplaySystemView(PortalId, _moduleRef, sessionParam, "AdminHeader.cshtml");
                var strOut = RocketFormsUtils.DisplaySystemView(PortalId, _moduleRef, sessionParam, "AppThemeAdmin.cshtml");

                var combinedContent = string.Empty;
                if (!string.IsNullOrEmpty(adminHeader))
                {
                    combinedContent += adminHeader;
                }

                if (!string.IsNullOrEmpty(strOut))
                {
                    combinedContent += strOut;
                }

                var model = new FormViewModel
                {
                    RenderedContent = combinedContent
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketFormsRazor AppTheme Error", ex.Message);
            }
        }
    }
}
