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
    public class EditControl : RazorModuleControlBase, IPageContributor
    {
        private string _systemkey;
        private string _moduleRef;
        private int _articleId;
        private SessionParams _sessionParam;

        public EditControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketDirectoryRazor/App_LocalResources/RocketDirectory.resx";
        }

        public override string ControlName => "Edit";

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

                context.ClientResourceController.RemoveStylesheetByName("skin.css");

                var strHeader2 = RocketDirectoryAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "adminheader.cshtml");
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
                _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = ModuleContext.TabId;
                _sessionParam.ModuleId = ModuleContext.ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.CultureCodeEdit = DNNrocketUtils.GetEditCulture();

                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCodeEdit);

                var moduleSettings = new ModuleContentLimpet(PortalSettings.PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                // Get rendered admin content
                var strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml");

                var articleData = RocketDirectoryAPIUtils.GetArticleData(PortalId, _articleId, _sessionParam.CultureCodeEdit, _systemkey, false);
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
                    ModuleSettings = moduleSettings,
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
