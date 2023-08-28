using DNNrocketAPI.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using RocketContentAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RocketContentMod
{
    public partial class AppTheme : ModuleSettingsBase
    {
        private string _moduleRef;
        private SessionParams _sessionParam;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                string skinSrcAdmin = "?SkinSrc=rocketedit";
                if (DNNrocketUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    Response.Redirect(EditUrl(DNNrocketUtils.RequestParam(Context, "ctl")) + skinSrcAdmin, false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                var strHeader1 = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "AdminHeader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader1);
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
                var strOut = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "AppThemeAdmin.cshtml");
                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}