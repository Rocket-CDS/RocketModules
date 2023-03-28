using DNNrocketAPI.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
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
    public partial class Edit : ModuleSettingsBase
    {
        private const string _systemkey = "rocketdirectoryapi";
        private string _moduleRef;
        private string _articleId;
        private SessionParams _sessionParam;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);
                _articleId = DNNrocketUtils.RequestParam(Context, "articleid");
                string skinSrcAdmin = "?SkinSrc=%2fDesktopModules%2fDNNrocket%2fRocketPortal%2fSkins%2frocketportal%2frocketedit";
                if (DNNrocketUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    if (_articleId == null || _articleId == "")
                        Response.Redirect(EditUrl() + skinSrcAdmin, false);
                    else
                        Response.Redirect(EditUrl("articleid", _articleId) + skinSrcAdmin, false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.Set("articleid", _articleId);
                _sessionParam.Set("moduleedit", "True");

                //var strHeader1 = RocketDirectoryAPIUtils.DisplayAdminView(PortalId, _moduleRef, "", _sessionParam, "adminfirstheader.cshtml");
                //PageIncludes.IncludeTextInHeader(Page, strHeader1);

                //var strHeader2 = RocketDirectoryAPIUtils.DisplayAdminView(PortalId, _moduleRef, "", _sessionParam, "adminlastheader.cshtml");
                //PageIncludes.IncludeTextInHeaderAt(Page, strHeader2, 0);

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
                var strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml");
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