﻿using DNNrocketAPI.Components;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using RocketDirectoryAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Abstractions;
using RazorEngine.Text;
using System.Security.Cryptography;
using System.Text;

namespace RocketDirectoryMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private string _systemkey;
        //private const string _systemkey = "rocketbusinessapi";
        private bool _hasEditAccess;
        private string _moduleRef;
        private SessionParams _sessionParam;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                // Get systemkey from module name. (remove mod, add "API")
                var moduleName = base.ModuleConfiguration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                var cmd = RequestParam(Context, "action");
                if (cmd == "clearcache" && UserUtils.IsAdministrator()) CacheUtils.ClearAllCache("portal" + PortalId);
                if (cmd == "recycleapppool" && UserUtils.IsSuperUser())
                {
                    DNNrocketUtils.RecycleApplicationPool();
                    Response.Redirect(Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString(), false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                var context = HttpContext.Current;
                var urlparams = new Dictionary<string,string>();
                var paramInfo = new SimplisityInfo();
                // get all query string params
                foreach (string key in context.Request.QueryString.AllKeys)
                {
                    if (key != null)
                    {
                        var keyValue = context.Request.QueryString[key];
                        paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
                        urlparams.Add(key, keyValue);
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
                        // ignore
                    }
                }
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.Url = context.Request.Url.ToString();
                _sessionParam.UrlFriendly = DNNrocketUtils.NavigateURL(TabId, urlparams);
                if (urlparams.ContainsKey("page") && GeneralUtils.IsNumeric(urlparams["page"])) _sessionParam.Page = Convert.ToInt32(urlparams["page"]);


                var strHeader1 = RocketDirectoryAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "viewfirstheader.cshtml");
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader1, 0);

                foreach (var dep in RocketDirectoryAPIUtils.DependanciesList(PortalId, _systemkey, _moduleRef, _sessionParam))
                {
                    var ctrltype = dep.GetXmlProperty("genxml/ctrltype");
                    var id = dep.GetXmlProperty("genxml/id");
                    var urlstr = dep.GetXmlProperty("genxml/url");
                    if (ctrltype == "css") PageIncludes.IncludeCssFile(Page, id, urlstr);
                    if (ctrltype == "js") PageIncludes.IncludeJsFile(Page, id, urlstr);
                }

                var strHeader2 = RocketDirectoryAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "viewlastheader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader2);

                // Set langauge, so editing with simplity gets correct language
                var lang = DNNrocketUtils.GetCurrentCulture();
                if (HttpContext.Current.Request.QueryString["language"] != null) lang = HttpContext.Current.Request.QueryString["language"];
                DNNrocketUtils.SetCookieValue("simplisity_language", lang);
                DNNrocketUtils.SetCookieValue("simplisity_editlanguage", lang);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            var moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);
            if (moduleSettings.InjectJQuery) JavaScript.RequestRegistration(CommonJs.jQuery);

            var strOut = RocketDirectoryAPIUtils.DisplayView(PortalId, _systemkey, _moduleRef,  _sessionParam);
            if (_hasEditAccess)
            {
                PageIncludes.IncludeCssFile(Page, "w3css", "/DesktopModules/DNNrocket/css/w3.css");
                PageIncludes.IncludeCssFile(Page, "w3csstheme", "/DesktopModules/DNNrocket/API/Themes/config-w3/1.0/css/rocketcds-theme.css");
                PageIncludes.IncludeCssFile(Page, "fontsroboto", "https://fonts.googleapis.com/css?family=Roboto:regular,bold,italic,thin,light,bolditalic,black,medium");
                PageIncludes.IncludeCssFile(Page, "materialicons", "https://fonts.googleapis.com/icon?family=Material+Icons");
                var articleid = RequestParam(Context, "articleid");
                if (GeneralUtils.IsNumeric(articleid))
                {
                    _sessionParam.Set("editurl", EditUrl("articleid", articleid, "AdminPanel"));
                }
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                var settingsurl = Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", parameters).ToString() + "#msSpecificSettings";
                _sessionParam.Set("settingsurl", settingsurl);
                _sessionParam.Set("appthemeurl", EditUrl("AppTheme"));
                _sessionParam.Set("adminpanelurl", EditUrl("AdminPanel"));
                _sessionParam.Set("returnurl", @GeneralUtils.EnCode(HttpUtility.UrlEncode(Context.Request.Url.ToString())));

                strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "ViewEditButtons.cshtml") + strOut;
            }
            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);
        }

        private static string RequestParam(HttpContext context, string paramName)
        {
            string result = null;

            if (context.Request.Form.Count != 0)
            {
                result = Convert.ToString(context.Request.Form[paramName]);
            }

            if (result == null)
            {
                if (context.Request.QueryString.Count != 0)
                {
                    result = Convert.ToString(context.Request.QueryString[paramName]);
                }
            }

            return (result == null) ? String.Empty : result.Trim();
        }

        #region Optional Interfaces

        /// <summary>
        /// The ModuleActions builds the module menu, for actions available.
        /// </summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _systemkey, ModuleId, TabId);

                var actions = new ModuleActionCollection();
                //actions.Add(GetNextActionID(), Localization.GetString("EditModule", this.LocalResourceFile), "", "", "edit.svg", EditUrl(), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("adminpanel", this.LocalResourceFile), "", "", "edit_app.svg", EditUrl("AdminPanel"), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("apptheme", this.LocalResourceFile), "", "", "edit_app.svg", EditUrl("AppTheme"), false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("clearcache", this.LocalResourceFile), "", "", "clear_cache.svg", Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?action=clearcache", false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("recycleapppool", this.LocalResourceFile), "", "", "restart_app.svg", Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?action=recycleapppool", false, SecurityAccessLevel.Host, true, false);

                return actions;
            }
        }

        #endregion

    }
}