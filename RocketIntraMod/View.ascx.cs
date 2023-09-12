using DNNrocketAPI.Components;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using RocketIntra.Components;
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
using System.Reflection;
using DotNetNuke.Common.Lists;

namespace RocketIntraMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private string _systemkey;
        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                _systemkey = "rocketintra";
                _moduleRef = PortalId + "_ModuleID_" + ModuleId;
                var cmd = RequestParam(Context, "action");
                if (cmd == "recycleapppool" && UserUtils.IsSuperUser())
                {
                    DNNrocketUtils.RecycleApplicationPool();
                    Response.Redirect(Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString(), false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                var context = HttpContext.Current;
                var paramInfo = new SimplisityInfo();
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.Url = context.Request.Url.ToString();
                _sessionParam.UrlFriendly = DNNrocketUtils.NavigateURL(TabId);

                var strHeader1 = RocketIntraUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "viewfirstheader.cshtml");
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader1, 0);
                var strHeader2 = RocketIntraUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "viewlastheader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader2);

                // Set langauge, so editing with simplity gets correct language
                var lang = DNNrocketUtils.GetCurrentCulture();
                if (HttpContext.Current.Request.QueryString["language"] != null) lang = HttpContext.Current.Request.QueryString["language"];
                DNNrocketUtils.SetCookieValue("simplisity_language", lang);
                DNNrocketUtils.SetCookieValue("simplisity_editlanguage", lang);

                _moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _systemkey, ModuleId, TabId);

                if (_moduleSettings.GetSettingInt("displaytype") == 1 && !UserUtils.IsSuperUser())
                {
                    Response.Redirect(EditUrl("AdminPanel").ToString(), false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            var hasEditAccess = false;
            if (UserId > 0) hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);
            if (hasEditAccess)
            {
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                var settingsurl = Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", parameters).ToString();
                var userParams = new UserParams("ModuleID:" + ModuleId, true);
                userParams.Set("adminpanelurl", EditUrl("AdminPanel"));
                userParams.Set("settingsurl", settingsurl);
                userParams.Set("viewurl", Context.Request.Url.ToString());
            }

            var strOut = "";
            if (_moduleSettings.Record.GetXmlPropertyInt("genxml/settings/displaytype") == 0)
            {
                strOut = RocketIntraUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "modulewelcome.cshtml");
            }
            if (_moduleSettings.Record.GetXmlPropertyInt("genxml/settings/displaytype") == 1)
            {
                strOut = RocketIntraUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "moduleadminbutton.cshtml");
            }
            if (_moduleSettings.GetSettingInt("displaytype") == 2)
            {
                var paramCmd = _moduleSettings.GetSetting("cmd");
                if (paramCmd.Contains("_"))
                {
                    var systemkey = _moduleSettings.GetSetting("systemkey");
                    if (systemkey == "") systemkey = paramCmd.Split('_')[0];
                    var interfacekey = _moduleSettings.GetSetting("interfacekey");
                    if (interfacekey == "") interfacekey = paramCmd.Split('_')[0];

                    var systemData = SystemSingleton.Instance(systemkey);
                    var rocketInterface = new RocketInterface(systemData.SystemInfo, interfacekey);
                    var paramInfo = new SimplisityInfo();
                    paramInfo.SetXmlProperty("genxml/hidden/moduleref", _moduleRef);
                    paramInfo.SetXmlProperty("genxml/hidden/moduleid", ModuleId.ToString());
                    paramInfo.SetXmlProperty("genxml/hidden/tabid", TabId.ToString());
                    var returnDictionary = DNNrocketUtils.GetProviderReturn(paramCmd, systemData.SystemInfo, rocketInterface, new SimplisityInfo(), paramInfo, ControlPath, "");

                    if (returnDictionary.ContainsKey("outputhtml"))
                    {
                        strOut = (string)returnDictionary["outputhtml"];
                    }
                }
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
                var actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), Localization.GetString("adminpanel", this.LocalResourceFile), "", "", "edit_app.svg", EditUrl("AdminPanel"), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("recycleapppool", this.LocalResourceFile), "", "", "restart_app.svg", Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID).ToString() + "?action=recycleapppool", false, SecurityAccessLevel.Host, true, false);

                return actions;
            }
        }

        #endregion

    }
}