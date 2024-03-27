using DNNrocketAPI.Components;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using RocketContentAPI.Components;
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
using System.Runtime.Remoting.Contexts;

namespace RocketContentMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private const string _systemkey = "rocketcontentapi";
        private bool _hasEditAccess;
        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);
                LogUtils.LogSystem("RocketContentMod: OnInit START");
                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                var moduleid = RequestParam(Context, "moduleid");
                var cmd = RequestParam(Context, "action");
                if (cmd == "clearcache" && moduleid == ModuleId.ToString() && UserUtils.IsAdministrator())
                {
                    CacheUtils.ClearAllCache("DNNrocketThumb");
                    CacheFileUtils.ClearAllCache(PortalId, _moduleRef);
                }
                if (cmd == "recycleapppool" && UserUtils.IsSuperUser())
                {
                    DNNrocketUtils.RecycleApplicationPool();
                    Response.Redirect(DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString(), false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                var context = HttpContext.Current;
                var urlparams = new Dictionary<string, string>();
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

                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                _moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                var strHeader1 = RocketContentAPIUtils.DisplayView(PortalId, _systemkey, _moduleRef, "", _sessionParam, "viewfirstheader.cshtml","", _moduleSettings.DisableCache);
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader1, 0);

                foreach (var dep in RocketContentAPIUtils.DependanciesList(PortalId, _moduleRef, _sessionParam))
                {
                    var ctrltype = dep.GetXmlProperty("genxml/ctrltype");
                    var id = dep.GetXmlProperty("genxml/id");
                    var urlstr = dep.GetXmlProperty("genxml/url");
                    var skinignore = dep.GetXmlProperty("genxml/ignoreonskin");
                    var ecofriendly = dep.GetXmlPropertyBool("genxml/ecofriendly");
                    if (dep.GetXmlProperty("genxml/ecofriendly") == "" || ecofriendly == _moduleSettings.ECOMode || _moduleSettings.ECOMode == false)
                    {
                        var ignoreFile = PageIncludes.IgnoreOnSkin(PortalSettings.ActiveTab.SkinSrc, skinignore);
                        if (ctrltype == "css" && !ignoreFile) PageIncludes.IncludeCssFile(Page, id, urlstr);
                        if (ctrltype == "js" && !ignoreFile)
                        {
                            if (urlstr.ToLower() == "{jquery}")
                                JavaScript.RequestRegistration(CommonJs.jQuery);
                            else
                                PageIncludes.IncludeJsFile(Page, id, urlstr);
                        }
                    }
                }

                var strHeader2 = RocketContentAPIUtils.DisplayView(PortalId, _systemkey, _moduleRef, "", _sessionParam, "viewlastheader.cshtml","", _moduleSettings.DisableCache);
                PageIncludes.IncludeTextInHeader(Page, strHeader2);

                LogUtils.LogSystem("RocketContentMod: OnInit END");
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
                LogUtils.LogSystem("RocketContentMod: ERROR - " + ex.ToString());
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            LogUtils.LogSystem("RocketContentMod: OnPreRender START");
            if (_moduleSettings.InjectJQuery) JavaScript.RequestRegistration(CommonJs.jQuery);

            var strOut = RocketContentAPIUtils.DisplayView(PortalId, _systemkey, _moduleRef, "", _sessionParam, "view.cshtml", "loadsettings", _moduleSettings.DisableCache);
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
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                var settingsurl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();

                var userParams = new UserParams("ModuleID:" + ModuleId, true);
                userParams.Set("editurl", EditUrl());
                userParams.Set("settingsurl", settingsurl);
                userParams.Set("appthemeurl", EditUrl("AppTheme"));
                userParams.Set("adminpanelurl", EditUrl("AdminPanel"));
                userParams.Set("recyclebinurl", EditUrl("RecycleBin"));                
                userParams.Set("viewurl", Context.Request.Url.ToString());

                strOut = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "ViewEditButtons.cshtml", true, false) + strOut;

            }
            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);
            LogUtils.LogSystem("RocketContentMod: OnPreRender END");
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
                var moduleSettings = new ModuleContentLimpet(PortalId, _systemkey, _moduleRef, ModuleId, TabId);

                var actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), Localization.GetString("EditModule", this.LocalResourceFile), "", "", "edit.svg", EditUrl(), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("apptheme", this.LocalResourceFile), "", "", "edit_app.svg", EditUrl("AppTheme"), false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("recyclebin", this.LocalResourceFile), "", "", "recycling.svg", EditUrl("RecycleBin"), false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("clearcache", this.LocalResourceFile), "", "", "clear_cache.svg", DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?action=clearcache&moduleid=" + ModuleId, false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("recycleapppool", this.LocalResourceFile), "", "", "restart_app.svg", DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?action=recycleapppool", false, SecurityAccessLevel.Host, true, false);

                return actions;
            }
        }

        #endregion

    }
}