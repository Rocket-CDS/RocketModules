﻿using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.UserControls;
using Newtonsoft.Json;
using RocketContent.Components;
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
    public partial class Settings : ModuleSettingsBase
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
                var sessionParam = new SessionParams(new SimplisityInfo());
                sessionParam.TabId = TabId;
                sessionParam.ModuleId = ModuleId;
                sessionParam.ModuleRef = _moduleRef;

                var strOut = RocketContentUtils.DisplaySystemView(PortalId, _moduleRef, sessionParam, "ModuleSettingsLoad.cshtml");

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