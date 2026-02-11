using DNNrocketAPI.Components;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using RocketContentRazor.Models;
using RocketPortal.Components;
using Simplisity;
using System;

namespace RocketContentRazor.Controls
{
    public class EditControl : RazorModuleControlBase, IPageContributor
    {
        private const string _systemkey = "rocketcontentapi";
        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;

        public EditControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketContentRazor/App_LocalResources/RocketContent.resx";
        }

        public override string ControlName => "Edit";

        public void ConfigurePage(PageConfigurationContext context)
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
                _sessionParam.CultureCodeEdit = DNNrocketUtils.GetEditCulture();

                _moduleSettings = new ModuleContentLimpet(PortalSettings.PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                // Request AJAX support
                context.ServicesFramework.RequestAjaxAntiForgerySupport();
                context.ServicesFramework.RequestAjaxScriptSupport();
                
                // Set page title
                context.PageService.SetTitle("Edit Rocket Content");
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
                _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = ModuleContext.TabId;
                _sessionParam.ModuleId = ModuleContext.ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.CultureCodeEdit = DNNrocketUtils.GetEditCulture();

                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCodeEdit);

                _moduleSettings = new ModuleContentLimpet(PortalSettings.PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                // Get rendered admin content
                var strOut = RocketContentAPIUtils.DisplaySystemView(PortalSettings.PortalId, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml", true, false);

                var articleData = RocketContentAPIUtils.GetArticleData(_moduleSettings, _sessionParam.CultureCodeEdit, false);
                var returnUrl = ModuleContext.NavigateUrl(this.ModuleContext.TabId, string.Empty, false);

                var model = new ArticleViewModel
                {
                    PortalId = PortalSettings.PortalId,
                    ModuleId = ModuleContext.ModuleId,
                    TabId = ModuleContext.TabId,
                    ModuleRef = _moduleRef,
                    CultureCode = _sessionParam.CultureCodeEdit,
                    IsEditable = true,
                    EditUrl = returnUrl,
                    ArticleData = articleData,
                    ArticleRows = articleData?.GetRows(),
                    ModuleSettings = _moduleSettings,
                    RenderedContent = strOut
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketContentRazor Edit Error", ex.Message);
            }
        }
    }
}
