using DNNrocketAPI.Components;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using RocketContent.Components;
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

namespace RocketContentMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private const string _systemkey = "rocketcontent";
        private bool _hasEditAccess;
        private string _moduleRef;
        private SessionParams _sessionParam;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                var cmd = RequestParam(Context, "cmd");
                if (cmd == "clearcache" && UserUtils.IsAdministrator()) CacheUtils.ClearAllCache(_moduleRef);
                if (cmd == "recycleapppool" && UserUtils.IsSuperUser())
                {
                    DNNrocketUtils.RecycleApplicationPool();
                    Response.Redirect(Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString(), false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

                var strHeader1 = RocketContentUtils.DisplayView(PortalId, _moduleRef, "", _sessionParam, "viewfirstheader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader1);

                var strHeader2 = RocketContentUtils.DisplayView(PortalId, _moduleRef, "", _sessionParam, "viewlastheader.cshtml");
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader2, 0);

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);

            var strOut = RocketContentUtils.DisplayView(PortalId, _moduleRef, "", _sessionParam);
            if (strOut == "loadsettings")
            {
                strOut = RocketContentUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml");
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                var redirectUrl = Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", parameters).ToString() + "#msSpecificSettings";
                strOut = strOut.Replace("{redirecturl}", redirectUrl);
                CacheUtils.ClearAllCache(_moduleRef);
            }
            if (_hasEditAccess)
            {
                PageIncludes.IncludeCssFile(Page, "w3css", "/DesktopModules/DNNrocket/css/w3.css");
                PageIncludes.IncludeCssFile(Page, "w3csstheme", "/DesktopModules/DNNrocket/API/Themes/config-w3/1.0/css/rocketcds-theme.css");
                PageIncludes.IncludeCssFile(Page, "fontsroboto", "https://fonts.googleapis.com/css?family=Roboto:regular,bold,italic,thin,light,bolditalic,black,medium");
                PageIncludes.IncludeCssFile(Page, "materialicons", "https://fonts.googleapis.com/icon?family=Material+Icons");
                _sessionParam.Set("editurl", EditUrl());
                _sessionParam.Set("returnurl", @GeneralUtils.EnCode(HttpUtility.UrlEncode(Context.Request.Url.ToString())));
                strOut = RocketContentUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "ViewEditButtons.cshtml") + strOut;
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
                actions.Add(GetNextActionID(), Localization.GetString("EditModule", this.LocalResourceFile), "", "", "", EditUrl(), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("apptheme", this.LocalResourceFile), "", "", "register.gif", "/SysAdmin/rocketapptheme?moduleref=" + moduleSettings.ModuleRef + "&appthemefolder=" + moduleSettings.AppThemeAdminFolder + "&appversionfolder=" + moduleSettings.AppThemeAdminVersion + "&project=" + moduleSettings.ProjectName + "&rtn=" + @GeneralUtils.EnCode(HttpUtility.UrlEncode(Context.Request.Url.ToString())), false, SecurityAccessLevel.Admin, true, false);
                var editappthemeview = false;
                if (moduleSettings.HasAppThemeView)
                {
                    if (moduleSettings.AppThemeAdminFolder != moduleSettings.AppThemeViewFolder || moduleSettings.AppThemeAdminVersion != moduleSettings.AppThemeViewVersion)
                    {
                        editappthemeview = true;
                    }
                }
                if (editappthemeview)
                {
                    actions.Add(GetNextActionID(), Localization.GetString("appthemeview", this.LocalResourceFile), "", "", "register.gif", "/SysAdmin/rocketapptheme?moduleref=" + moduleSettings.ModuleRef + "&appthemefolder=" + moduleSettings.AppThemeViewFolder + "&appversionfolder=" + moduleSettings.AppThemeViewVersion + "&project=" + moduleSettings.ProjectName + "&rtn=" + @GeneralUtils.EnCode(HttpUtility.UrlEncode(Context.Request.Url.ToString())), false, SecurityAccessLevel.Admin, true, false);
                }
                actions.Add(GetNextActionID(), Localization.GetString("clearcache", this.LocalResourceFile), "", "", "action_refresh.gif", Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?cmd=clearcache", false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("recycleapppool", this.LocalResourceFile), "", "", "restore.gif", Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?cmd=recycleapppool", false, SecurityAccessLevel.Host, true, false);

                return actions;
            }
        }

        #endregion

    }
}