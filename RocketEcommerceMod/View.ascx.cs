﻿using DNNrocketAPI.Components;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using RocketEcommerceAPI.Components;
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

namespace RocketEcommerceMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private string _systemkey;
        private bool _hasEditAccess;
        private string _moduleRef;
        private SessionParams _sessionParam;
        private SimplisityInfo _paramInfo;
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
                    Response.Redirect(DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString(), false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                var context = HttpContext.Current;
                var urlparams = new Dictionary<string,string>();
                _paramInfo = new SimplisityInfo();
                // get all query string params
                foreach (string key in context.Request.QueryString.AllKeys)
                {
                    if (key != null)
                    {
                        var keyValue = context.Request.QueryString[key];
                        _paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
                        urlparams.Add(key, keyValue);
                    }
                }

                var jsonparams = DNNrocketUtils.GetCookieValue("simplisity_sessionparams");
                if (jsonparams != "")
                {
                    try
                    {
                        var simplisity_sessionparams = SimplisityJson.DeserializeJson(jsonparams, "cookie");
                        _paramInfo.AddXmlNode(simplisity_sessionparams.XMLData, "cookie", "genxml");
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                _sessionParam = new SessionParams(_paramInfo);
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.Url = context.Request.Url.ToString();
                _sessionParam.UrlFriendly = DNNrocketUtils.NavigateURL(TabId, urlparams);

                var moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _sessionParam.ModuleId, _sessionParam.TabId);

                var strHeader1 = RocketEcommerceAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "viewfirstheader.cshtml");
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader1, 0);

                foreach (var dep in RocketEcommerceAPIUtils.DependanciesList(PortalId, _moduleRef, _sessionParam))
                {
                    var ctrltype = dep.GetXmlProperty("genxml/ctrltype");
                    var id = dep.GetXmlProperty("genxml/id");
                    var urlstr = dep.GetXmlProperty("genxml/url");
                    var ecofriendly = dep.GetXmlPropertyBool("genxml/ecofriendly");
                    if (ecofriendly == moduleSettings.ECOMode || moduleSettings.ECOMode == false)
                    {
                        if (ctrltype == "css") PageIncludes.IncludeCssFile(Page, id, urlstr);
                        if (ctrltype == "js")
                        {
                            if (urlstr.ToLower() == "{jquery}")
                                JavaScript.RequestRegistration(CommonJs.jQuery);
                            else
                                PageIncludes.IncludeJsFile(Page, id, urlstr);
                        }
                    }
                }

                var strHeader2 = RocketEcommerceAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "viewlastheader.cshtml");
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
            var moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _sessionParam.ModuleId, _sessionParam.TabId);
            if (moduleSettings.InjectJQuery) JavaScript.RequestRegistration(CommonJs.jQuery);
            _sessionParam.Set("rtncmd", RequestParam(Context, "cmd")); // check if we have a bank return

            // form fields for return from external systems.
            if (Request.Form != null)
            {
                foreach (string key in Request.Form.AllKeys)
                {
                    _paramInfo.SetXmlProperty("genxml/form/" + key.Replace(" ",""), Request.Form[key]);
                }
            }

            var strOut = RocketEcommerceAPIUtils.DisplayView(PortalId, _systemkey, _moduleRef,  _sessionParam, _paramInfo);
            if (_hasEditAccess)
            {
                var articleid = RequestParam(Context, "pid");
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                var settingsurl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();

                var userParams = new UserParams("ModuleID:" + ModuleId, true);
                if (articleid == null || articleid == "")
                    userParams.Set("editurl", EditUrl());
                else
                    userParams.Set("editurl", EditUrl("pid", articleid));
                userParams.Set("settingsurl", settingsurl);
                userParams.Set("appthemeurl", EditUrl("AppTheme"));
                userParams.Set("adminpanelurl", EditUrl("AdminPanel"));
                userParams.Set("viewurl", Context.Request.Url.ToString());

                strOut = RocketEcommerceAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "ViewEditButtons.cshtml") + strOut;

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
                var moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, ModuleId, TabId);

                var actions = new ModuleActionCollection();
                //actions.Add(GetNextActionID(), Localization.GetString("EditModule", this.LocalResourceFile), "", "", "edit.svg", EditUrl(), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("adminpanel", this.LocalResourceFile), "", "", "edit_app.svg", EditUrl("AdminPanel"), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("apptheme", this.LocalResourceFile), "", "", "edit_app.svg", EditUrl("AppTheme"), false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("clearcache", this.LocalResourceFile), "", "", "clear_cache.svg", DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?action=clearcache", false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("recycleapppool", this.LocalResourceFile), "", "", "restart_app.svg", DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?action=recycleapppool", false, SecurityAccessLevel.Host, true, false);

                return actions;
            }
        }

        #endregion

    }
}