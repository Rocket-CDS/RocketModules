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
    public partial class Edit : ModuleSettingsBase
    {
        private string _moduleRef;
        private SessionParams _sessionParam;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                string skinSrcAdmin = "?SkinSrc=%2fDesktopModules%2fDNNrocket%2fRocketPortal%2fSkins%2frocketportal%2frocketedit";
                if (DNNrocketUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    Response.Redirect(EditUrl() + skinSrcAdmin, false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;

                var strHeader1 = RocketContentAPIUtils.DisplayAdminView(PortalId, _moduleRef, "", _sessionParam, "adminfirstheader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader1);

                var strHeader2 = RocketContentAPIUtils.DisplayAdminView(PortalId, _moduleRef, "", _sessionParam, "adminlastheader.cshtml");
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader2, 0);

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
                var strOut = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml");
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