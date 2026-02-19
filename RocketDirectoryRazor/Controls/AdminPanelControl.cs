using DNNrocketAPI.Components;
using DotNetNuke.Abstractions.Pages;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using RocketDirectoryRazor.Models;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Runtime.Remoting.Contexts;
using System.Web.UI;

namespace RocketDirectoryRazor.Controls
{
    public class AdminPanelControl : RazorModuleControlBase, IPageContributor
    {
        private string _systemkey;
        private string _moduleRef;
        private int _articleId;
        private SessionParams _sessionParam;

        public AdminPanelControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketDirectoryRazor/App_LocalResources/RocketDirectory.resx";
        }

        public override string ControlName => "AdminPanel";

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {
                // Get systemkey from module name. (remove "razor", add "API")
                var moduleName = base.ModuleConfiguration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 5) + "api";
                var articleIdStr = System.Web.HttpContext.Current.Request.QueryString["articleid"];
                _articleId = int.TryParse(articleIdStr, out var articleId) ? articleId : 0;
                _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = ModuleContext.TabId;
                _sessionParam.ModuleId = ModuleContext.ModuleId;    
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.Set("articleid", _articleId.ToString());
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.CultureCodeEdit = DNNrocketUtils.GetEditCulture();

                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                context.ClientResourceController.RemoveStylesheetByName("skin.css");

                var strHeader2 = RocketDirectoryAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "AdminPanelheader.cshtml");
                if (!string.IsNullOrWhiteSpace(strHeader2)) context.PageService.AddToHead(new PageTag(strHeader2, 999));
              
                // Set page title
                context.PageService.SetTitle("Edit " + moduleName);

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
                // Get rendered admin content
                var strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalSettings.PortalId, _systemkey, _moduleRef, _sessionParam, "AdminPanelLoad.cshtml");
                var model = new ArticleViewModel
                {
                    PortalId = PortalSettings.PortalId,
                    ModuleId = ModuleContext.ModuleId,
                    TabId = ModuleContext.TabId,
                    ModuleRef = _moduleRef,
                    CultureCode = _sessionParam.CultureCodeEdit,
                    IsEditable = true,
                    RenderedContent = strOut
                };
                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketDirectoryRazor Edit Error", ex.Message);
            }
        }
    }
}
