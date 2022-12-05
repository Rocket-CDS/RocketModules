using DNNrocketAPI.Components;
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

namespace RocketContentMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private const string _systemkey = "rocketcontent";
        private bool _hasEditAccess;
        private PortalLimpet _portalData;
        private string _moduleRef;
        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                _moduleRef = PortalId + "_ModuleID:" + ModuleId;
                _portalData = new PortalLimpet(PortalId);

                var sRec = new SimplisityRecord();
                sRec.SetXmlProperty("postdata/sfield/moduleref", _moduleRef);
                sRec.SetXmlProperty("postdata/sfield/projectname", "AppTheme-W3-CSS");
                sRec.SetXmlProperty("postdata/sfield/appthemeview", "HtmlContent");
                sRec.SetXmlProperty("postdata/sfield/appthemeviewversion", "");
                sRec.SetXmlProperty("postdata/sfield/modulename", ModuleId.ToString());
                sRec.SetXmlProperty("postdata/sfield/securitykeyedit", _portalData.SecurityKeyEdit);

                var jsonParam = JsonConvert.SerializeXmlNode(sRec.XMLDoc, Formatting.None, true);


                //_commReturn = CommUtils.CallRedirect("remote_publicview", "rocketcontent", "", jsonParam);

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                //var strHeader1 = _commReturn.FirstHeader;
                //PageIncludes.IncludeTextInHeader(Page, strHeader1);

                //var strHeader = _commReturn.LastHeader;
                //PageIncludes.IncludeTextInHeaderAt(Page, strHeader, 0);

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            var strOut = RocketContentUtils.DisplayView(PortalId, _moduleRef);
            if (strOut == "loadsettings") Response.Redirect(EditUrl("ModuleId",ModuleId.ToString(),"Module") + "#msSpecificSettings", true);
            //inject jQuery from DNN, to stop conflict with header.
            JavaScript.RequestRegistration(CommonJs.jQuery);

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