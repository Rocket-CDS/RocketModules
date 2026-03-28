using DNNrocketAPI.Components;
using DotNetNuke.Abstractions.ClientResources;
using DotNetNuke.Abstractions.Pages;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.ClientDependency;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using RocketDirectoryRazor.Models;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Web;
using System.Web.UI;

namespace RocketDirectoryRazor.Controls
{
    public class ViewControl : RazorModuleControlBase, IPageContributor
    {
        private const string _systemkey = "RocketDirectoryAPI";

        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;
        private bool _hasEditAccess;
        private int _portalId => PortalSettings.PortalId;


        public ViewControl()
        {
            LocalResourceFile = "~/DesktopModules/RocketModules/RocketDirectoryRazor/App_LocalResources/RocketDirectory.resx";
        }

        public override string ControlName => "View";

        private bool CanUserEditModule()
        {
            if (UserId <= 0) return false;

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
                EnsureRocketContext();

                if (IsPrimaryModuleInstance())
                {
                    ApplyPageMetaData(context);
                }

                var appThemeSystem = AppThemeUtils.AppThemeSystem(PortalId, _systemkey);
                var portalData = new PortalLimpet(PortalId);
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

                var headerTemplate = _moduleSettings.DisplayTemplate.ToLower().Replace(".cshtml", "header.cshtml");
                var strHeader2 = RocketDirectoryAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, headerTemplate);
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

        private bool IsPrimaryModuleInstance()
        {
            var moduleInfo = ModuleContext.Configuration;
            if (moduleInfo == null || moduleInfo.DesktopModule == null)
            {
                return true;
            }

            var moduleName = moduleInfo.DesktopModule.ModuleName;
            var firstModuleId = Int32.MaxValue;
            var tabModules = ModuleController.Instance.GetTabModules(TabId);

            foreach (var tabModule in tabModules)
            {
                var currentModule = tabModule.Value;
                if (currentModule != null && currentModule.DesktopModule != null && currentModule.DesktopModule.ModuleName == moduleName)
                {
                    if (currentModule.ModuleID < firstModuleId)
                    {
                        firstModuleId = currentModule.ModuleID;
                    }
                }
            }

            if (firstModuleId == Int32.MaxValue)
            {
                return true;
            }

            return ModuleId == firstModuleId;
        }

        public override IRazorModuleResult Invoke()
        {
            try
            {
                EnsureRocketContext();

                var strOut = RocketDirectoryAPIUtils.DisplayView(PortalId, _systemkey, _moduleRef, _sessionParam, "", "loadsettings");
                if (strOut == "loadsettings")
                {
                    strOut = "";
                    if (_hasEditAccess)
                    {
                        strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml", false);
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
                        userParams.Set("settingsurl", settingsurl);
                        userParams.Set("rocketconfigurl", this.EditUrl("Settings"));
                        userParams.Set("appthemeurl", this.EditUrl("AppTheme"));
                        userParams.Set("adminpanelurl", this.EditUrl("AdminPanel"));
                        userParams.Set("viewtabid", this.PortalSettings.ActiveTab.TabID.ToString());

                        viewButtonsOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "ViewEditButtons.cshtml");
                        CacheUtils.SetCache(editbuttonkey, viewButtonsOut, _moduleRef);
                    }
                    strOut = viewButtonsOut + strOut;
                }

                var model = new DirectoryViewModel
                {
                    RenderedContent = strOut
                };

                return View(model);
            }
            catch (Exception ex)
            {
                DNNrocketAPI.Components.LogUtils.LogException(ex);
                return Error("RocketDirectoryRazor Error", ex.Message);
            }
        }

        private void EnsureRocketContext()
        {
            if (_sessionParam == null || _moduleSettings == null || string.IsNullOrEmpty(_moduleRef))
            {
                SetRocketContext();
            }
        }

        private void ApplyPageMetaData(PageConfigurationContext context)
        {
            var urlParams = new Dictionary<string, string>();

            foreach (string key in Request.QueryString.AllKeys)
            {
                if (key != null)
                {
                    var keyValue = Request.QueryString[key];
                    urlParams[key.ToLower()] = keyValue;
                }
            }

            var metaPageData = PagesUtils.GetMetaData(PortalSettings.ActiveTab.TabID, Request.Url, urlParams);

            if (metaPageData.Redirect404)
            {
                return;
            }

            if (!String.IsNullOrEmpty(metaPageData.Title))
            {
                context.PageService.SetTitle(HttpUtility.HtmlEncode(metaPageData.Title));
            }

            if (!String.IsNullOrEmpty(metaPageData.Description))
            {
                context.PageService.AddToHead(new PageTag(BuildMetaTag("description", metaPageData.Description), 2));
            }

            if (!String.IsNullOrEmpty(metaPageData.KeyWords))
            {
                context.PageService.AddToHead(new PageTag(BuildMetaTag("keywords", metaPageData.KeyWords), 3));
            }

            if (!String.IsNullOrEmpty(metaPageData.CanonicalLinkUrl))
            {
                context.PageService.AddToHead(new PageTag("<link rel=\"canonical\" href=\"" + HttpUtility.HtmlAttributeEncode(metaPageData.CanonicalLinkUrl) + "\" />", 4));
            }

            if (!String.IsNullOrEmpty(metaPageData.AlternateLinkHtml))
            {
                context.PageService.AddToHead(new PageTag(metaPageData.AlternateLinkHtml, 5));
            }

            foreach (var metaDict in metaPageData.HtmlMeta)
            {
                context.PageService.AddToHead(new PageTag(BuildPropertyMetaTag(metaDict.Key, metaDict.Value), 6));
            }

            if (!String.IsNullOrEmpty(metaPageData.JsonLd))
            {
                context.PageService.AddToHead(new PageTag("<script type=\"application/ld+json\">" + metaPageData.JsonLd + "</script>", 7));
            }
        }

        private string BuildMetaTag(string name, string content)
        {
            return "<meta name=\"" + HttpUtility.HtmlAttributeEncode(name) + "\" content=\"" + HttpUtility.HtmlAttributeEncode(content) + "\" />";
        }

        private string BuildPropertyMetaTag(string property, string content)
        {
            if (property.StartsWith("twitter:", StringComparison.OrdinalIgnoreCase))
            {
                return "<meta name=\"" + HttpUtility.HtmlAttributeEncode(property) + "\" content=\"" + HttpUtility.HtmlAttributeEncode(content) + "\" />";
            }

            return "<meta property=\"" + HttpUtility.HtmlAttributeEncode(property) + "\" content=\"" + HttpUtility.HtmlAttributeEncode(content) + "\" />";
        }

        private void SetRocketContext()
        {
            _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;
            _hasEditAccess = CanUserEditModule();

            var urlparams = new Dictionary<string, string>();
            var paramInfo = new SimplisityInfo();

            foreach (string key in Request.QueryString.AllKeys)
            {
                if (key != null)
                {
                    var keyValue = Request.QueryString[key];
                    paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
                    urlparams.Add(key.ToLower(), keyValue);
                }
            }

            var jsonparams = DNNrocketUtils.GetCookieValue("simplisity_sessionparams");
            if (jsonparams != "")
            {
                try
                {
                    var simplisity_sessionparams = SimplisityJson.DeserializeJson(jsonparams, "cookie");
                    paramInfo.AddXmlNode(simplisity_sessionparams.XMLData, "cookie", "genxml");
                }
                catch (Exception)
                {
                }
            }

            _sessionParam = new SessionParams(paramInfo);
            _sessionParam.TabId = ModuleContext.TabId;
            _sessionParam.ModuleId = ModuleContext.ModuleId;
            _sessionParam.ModuleRef = _moduleRef;
            _sessionParam.Url = ModuleContext.NavigateUrl(this.TabId, string.Empty, false);
            _sessionParam.UrlFriendly = DNNrocketUtils.NavigateURL(TabId, urlparams);
            _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
            _sessionParam.CultureCodeEdit = DNNrocketUtils.GetEditCulture();

            if (urlparams.ContainsKey("search") && !String.IsNullOrEmpty(urlparams["search"]))
            {
                _sessionParam.SearchText = urlparams["search"];
            }

            if (urlparams.ContainsKey("page") && GeneralUtils.IsNumeric(urlparams["page"]))
            {
                _sessionParam.Page = Convert.ToInt32(urlparams["page"]);
            }

            _moduleSettings = new ModuleContentLimpet(PortalSettings.PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);
        }
    }
}