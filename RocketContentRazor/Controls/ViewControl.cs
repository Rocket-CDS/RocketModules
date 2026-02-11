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
using System.Collections.Generic;

namespace RocketContentRazor.Controls
{
    public class ViewControl : RazorModuleControlBase, IPageContributor
    {
        private const string _systemkey = "rocketcontentapi";
        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;

        public ViewControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketContentRazor/App_LocalResources/RocketContent.resx";
        }

        public override string ControlName => "View";

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

                _moduleSettings = new ModuleContentLimpet(PortalSettings.PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                // Set page title and meta if article has data
                var articleData = RocketContentAPIUtils.GetArticleData(_moduleSettings, _sessionParam.CultureCode);
                if (articleData != null && articleData.Exists)
                {
                    var firstRow = articleData.GetRow(0);
                    if (firstRow != null)
                    {
                        var title = firstRow.Get("genxml/lang/genxml/textbox/title");
                        if (!string.IsNullOrEmpty(title))
                        {
                            context.PageService.SetTitle(title);
                        }
                    }
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
                _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = ModuleContext.TabId;
                _sessionParam.ModuleId = ModuleContext.ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                _moduleSettings = new ModuleContentLimpet(PortalSettings.PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                // Get rendered content from RocketContentAPI - this does ALL the model/rendering work
                var strOut = RocketContentAPIUtils.DisplayView(PortalSettings.PortalId, _systemkey, _moduleRef, "", _sessionParam, "view.cshtml", "loadsettings", _moduleSettings.DisableCache);

                if (strOut == "loadsettings")
                {
                    strOut = RocketContentAPIUtils.DisplaySystemView(PortalSettings.PortalId, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml");
                    CacheUtils.ClearAllCache(_moduleRef);
                }

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
                return Error("RocketContentRazor Error", ex.Message);
            }
        }
    }
}