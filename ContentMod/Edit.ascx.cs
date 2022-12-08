using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using RocketContent.Components;
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
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);
                _moduleRef = PortalId + "_ModuleID:" + ModuleId;
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
                var strOut = RocketContentUtils.DisplayAdminView(PortalId, _moduleRef, "");
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