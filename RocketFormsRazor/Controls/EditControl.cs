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
    public class EditControl : RazorModuleControlBase, IPageContributor
    {
        private string _moduleRef;
        private SessionParams _sessionParam;

        public EditControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketFormsRazor/App_LocalResources/RocketForm.resx";
        }

        public override string ControlName => "Edit";

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {
                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                var strHeader = RocketFormsUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "AdminHeader.cshtml");
                if (!string.IsNullOrWhiteSpace(strHeader))
                {
                    context.PageService.AddToHead(new DotNetNuke.Abstractions.Pages.PageTag(strHeader, 999));
                }
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

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                var strOut = RocketFormsUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml");

                var model = new FormViewModel
                {
                    RenderedContent = strOut
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketFormsRazor Edit Error", ex.Message);
            }
        }
    }
}
