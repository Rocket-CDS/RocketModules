using DNNrocketAPI.Components;
using DotNetNuke.Abstractions.ClientResources;
using DotNetNuke.Abstractions.Pages;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.ClientDependency;
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
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;

namespace RocketContentRazor.Controls
{
    public class ViewControl : RazorModuleControlBase, IPageContributor
    {
        private const string _systemkey = "rocketcontentapi";
        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;
        private bool _hasEditAccess;
        private int _portalId => PortalSettings.PortalId;


        public ViewControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketContentRazor/App_LocalResources/RocketContent.resx";
        }

        public override string ControlName => "View";

        private bool CanUserEditModule()
        {
            if (UserId <= 0) return false;

            // Get the module info from ModuleContext
            var moduleInfo = ModuleContext.Configuration;
            if (moduleInfo != null)
            {
                // Use ModulePermissionController to check edit permissions
                return ModulePermissionController.CanEditModuleContent(moduleInfo);
            }

            return false;
        }

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {
                _moduleRef = _portalId + "_ModuleID_" + ModuleContext.ModuleId;
                _hasEditAccess = CanUserEditModule();

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = ModuleContext.TabId;
                _sessionParam.ModuleId = ModuleContext.ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                _moduleSettings = new ModuleContentLimpet(_portalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

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

                var appThemeSystem = AppThemeUtils.AppThemeSystem(_portalId, _systemkey);
                var portalData = new PortalLimpet(_portalId);
                var appTheme = new AppThemeLimpet(_moduleSettings.PortalId, _moduleSettings.AppThemeAdminFolder, _moduleSettings.AppThemeAdminVersion, _moduleSettings.ProjectName);

                var dependencyLists = DNNrocketUtils.InjectDependencies(_moduleRef, appTheme, _moduleSettings.ECOMode, PortalSettings.ActiveTab.SkinSrc, portalData.EngineUrlWithProtocol, appThemeSystem.AppThemeVersionFolderRel);

                foreach (var dep in dependencyLists)
                {
                    if (dep.ctrltype == "css")
                    {
                        context.ClientResourceController.RegisterStylesheet(dep.url, FileOrder.Css.ModuleCss);
                    }
                    else if (dep.ctrltype == "js")
                    {
                        if (dep.url == "{jquery}")
                        {
                            JavaScript.RequestRegistration(CommonJs.jQuery);
                        }
                        else
                        {
                            context.ClientResourceController.RegisterScript(dep.url, FileOrder.Js.DefaultPriority, true);
                        }
                    }
                }
                var strHeader2 = RocketContentAPIUtils.DisplayView(_portalId, _systemkey, _moduleRef, "", _sessionParam, "viewheader.cshtml", "", _moduleSettings.DisableCache);
                if (!string.IsNullOrWhiteSpace(strHeader2)) context.PageService.AddToHead(new PageTag(strHeader2, 999));

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
                _hasEditAccess = CanUserEditModule();

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
                    strOut = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml");
                    string[] parameters;
                    parameters = new string[1];
                    parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                    var redirectUrl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();
                    strOut = strOut.Replace("{redirecturl}", redirectUrl);
                    CacheUtils.ClearAllCache(_moduleRef);
                }
                if (_hasEditAccess)
                {
                    var editbuttonkey = "editbuttons" + _moduleRef + "_" + UserId + "_" + _sessionParam.CultureCode;
                    var viewButtonsOut = CacheUtils.GetCache(editbuttonkey, _moduleRef);
                    if (viewButtonsOut == null)
                    {
                        string[] parameters;
                        parameters = new string[1];
                        parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                        var settingsurl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();

                        var userParams = new UserParams("ModuleID:" + ModuleId, true);
                        userParams.Set("editurl", this.EditUrl());
                        userParams.Set("settingsurl", this.EditUrl("Settings"));
                        userParams.Set("appthemeurl", this.EditUrl("AppTheme"));
                        userParams.Set("adminpanelurl", this.EditUrl("AdminPanel"));
                        userParams.Set("recyclebinurl", this.EditUrl("RecycleBin"));
                        userParams.Set("viewtabid", this.PortalSettings.ActiveTab.TabID.ToString());

                        viewButtonsOut = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "ViewEditButtons.cshtml", true, false);
                        CacheUtils.SetCache(editbuttonkey, viewButtonsOut, _moduleRef);
                    }
                    strOut = viewButtonsOut + strOut;
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