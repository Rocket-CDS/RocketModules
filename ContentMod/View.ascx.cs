﻿using DNNrocketAPI.Components;
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

namespace RocketContentMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private const string _systemkey = "rocketcontent";
        private bool _hasEditAccess;
        private string _moduleRef;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                _moduleRef = PortalId + "_ModuleID:" + ModuleId;

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                var strHeader1 = RocketContentUtils.DisplayView(PortalId, _moduleRef, "viewfirstheader.cshtml");
                //PageIncludes.IncludeTextInHeader(Page, strHeader1);

                var strHeader2 = RocketContentUtils.DisplayView(PortalId, _moduleRef, "viewlastheader.cshtml");
                //PageIncludes.IncludeTextInHeaderAt(Page, strHeader2, 0);

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);

            var strOut = RocketContentUtils.DisplayView(PortalId, _moduleRef, "");
            if (strOut == "loadsettings")
            {
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                //var redirectUrl = Globals.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", parameters).ToString() + "#msSpecificSettings";

                var navigationManager = DependencyProvider?.GetRequiredService<INavigationManager>();
                var redirectUrl = navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", parameters);

                Response.Redirect(redirectUrl, true);
            }

            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);

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
                actions.Add(GetNextActionID(), Localization.GetString("EditModule", this.LocalResourceFile), "", "", "", EditUrl(), false, SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion

    }
}