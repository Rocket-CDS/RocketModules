using DNNrocketAPI.Components;
using DotNetNuke.Services.Exceptions;
using RocketForms.Components;
using Simplisity;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RocketFormsMod
{
    public partial class Settings : RocketModuleSettingsBase
    {
        private string _moduleRef;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);
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
            // Apply admin skin when entering edit mode
            if (!HasAdminSkinCookie())
            {
                ApplyAdminSkinCookie();
            }
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

                var strOut = RocketFormsUtils.DisplaySystemView(PortalId, _moduleRef, sessionParam, "ModuleSettingsLoad.cshtml");

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