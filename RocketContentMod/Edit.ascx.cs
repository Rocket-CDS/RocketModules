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
    public partial class Edit : RocketModuleSettingsBase
    {
        private string _moduleRef;
        private SessionParams _sessionParam;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                PageIncludes.RemoveCssFile(Page, "skin.css"); //DNN always tries to load a skin.css, even if it does not exists.

                var strHeader1 = RocketContentAPIUtils.DisplayAdminView(PortalId, _moduleRef, "", _sessionParam, "adminfirstheader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader1);

                var strHeader2 = RocketContentAPIUtils.DisplayAdminView(PortalId, _moduleRef, "", _sessionParam, "adminlastheader.cshtml");
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader2, 0);

                var strHeaderAdmin = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "AdminHeader.cshtml",true,false);
                PageIncludes.IncludeTextInHeader(Page, strHeaderAdmin);

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
                var strOut = RocketContentAPIUtils.DisplaySystemView(PortalId, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml",true,false);
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