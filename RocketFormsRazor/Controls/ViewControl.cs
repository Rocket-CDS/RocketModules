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
using RocketFormsRazor.Models;
using RocketForms.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Runtime.Remoting.Contexts;

namespace RocketFormsRazor.Controls
{
    public class ViewControl : RazorModuleControlBase, IPageContributor
    {
        private const string _systemkey = "rocketcontentapi";

        private bool _hasEditAccess;
        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;

        public ViewControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketFormsRazor/App_LocalResources/RocketForm.resx";
        }

        public override string ControlName => "View";

        private bool CanUserEditModule()
        {
            if (UserId <= 0)
            {
                return false;
            }

            var moduleInfo = ModuleContext.Configuration;
            if (moduleInfo != null)
            {
                return ModulePermissionController.CanEditModuleContent(moduleInfo);
            }

            return false;
        }

        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {
                _moduleRef = PortalId + "_ModuleID_" + ModuleId;
                _hasEditAccess = CanUserEditModule();

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (key != null)
                    {
                        var keyValue = Request.QueryString[key];
                        paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
                    }
                }

                _moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                var strHeader1 = RocketFormsUtils.DisplayView(PortalId, _systemkey, _moduleRef, "", _sessionParam, "viewfirstheader.cshtml");
                if (!string.IsNullOrWhiteSpace(strHeader1))
                {
                    context.PageService.AddToHead(new PageTag(strHeader1, 0));
                }

                var appThemeSystem = AppThemeUtils.AppThemeSystem(PortalId, _systemkey);
                var portalData = new PortalLimpet(PortalId);
                var appTheme = new AppThemeLimpet(_moduleSettings.PortalId, _moduleSettings.AppThemeAdminFolder, _moduleSettings.AppThemeAdminVersion, _moduleSettings.ProjectName);
                var dependencyLists = DNNrocketUtils.InjectDependencies(_moduleRef, appTheme, _moduleSettings.ECOMode, PortalSettings.ActiveTab.SkinSrc, portalData.EngineUrlWithProtocol, appThemeSystem.AppThemeVersionFolderRel, _moduleSettings.DisplayTemplate);

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

                var strHeader2 = RocketFormsUtils.DisplayView(PortalId, _systemkey, _moduleRef, "", _sessionParam, "viewlastheader.cshtml");
                if (!string.IsNullOrWhiteSpace(strHeader2))
                {
                    context.PageService.AddToHead(new PageTag(strHeader2, 999));
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
                _hasEditAccess = CanUserEditModule();

                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                var strOut = RocketFormsUtils.DisplayView(PortalId, _systemkey, _moduleRef, "", _sessionParam, "view.cshtml", "loadsettings");
                if (strOut == "loadsettings")
                {
                    strOut = string.Empty;
                    if (_hasEditAccess)
                    {
                        strOut = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml");
                        strOut = strOut.Replace("{redirecturl}", this.EditUrl("Settings"));
                        CacheUtils.ClearAllCache(_moduleRef);
                    }
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
                        userParams.Set("settingsurl", settingsurl);
                        userParams.Set("rocketconfigurl", this.EditUrl("Settings"));
                        userParams.Set("appthemeurl", this.EditUrl("AppTheme"));
                        userParams.Set("adminpanelurl", this.EditUrl("AdminPanel"));
                        userParams.Set("viewtabid", _sessionParam.TabId.ToString());

                        viewButtonsOut = RocketFormsUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "ViewEditButtons.cshtml");
                        CacheUtils.SetCache(editbuttonkey, viewButtonsOut, _moduleRef);
                    }
                    strOut = viewButtonsOut + strOut;
                }
                var model = new FormViewModel
                {
                    RenderedContent = strOut
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketFormsRazor Error", ex.Message);
            }
        }
    }
}
