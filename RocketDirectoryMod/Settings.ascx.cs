using DNNrocketAPI.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using Newtonsoft.Json;
using RazorEngine;
using RocketDirectoryAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RocketDirectoryMod
{
    public partial class Settings : RocketModuleSettingsBase
    {
        private string _systemkey;
        private string _moduleRef;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);
                // Get systemkey from module name. (remove mod, add "API")
                var moduleName = base.ModuleConfiguration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.IsPostBack == false)
            {
                PageLoad();
            }
        }

        private void PageLoad()
        {
            try
            {
                var sessionParam = new SessionParams(new SimplisityInfo());
                sessionParam.TabId = TabId;
                sessionParam.ModuleId = ModuleId;
                sessionParam.ModuleRef = _moduleRef;
                var lang = "";
                if (HttpContext.Current.Request.QueryString["language"] != null) lang = HttpContext.Current.Request.QueryString["language"];
                DNNrocketUtils.SetCookieValue("simplisity_language", lang);

                var strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, sessionParam, "ModuleSettingsLoad.cshtml");

                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);

                // Add link to Admin
                var lit2 = new Literal();
                lit2.Text = "<a class='w3-button w3-round w3-theme-action w3-text-white w3-border' href='" + EditUrl("AdminPanel") + "' target='_blank'><span class=\"material-icons\">dataset</span></a>";
                phData2.Controls.Add(lit2);

            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

    }
}