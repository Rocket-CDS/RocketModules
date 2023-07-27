using DNNrocketAPI.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using RocketEcommerceAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RocketEcommerceMod
{
    public partial class Edit : ModuleSettingsBase
    {
        private string _systemkey;
        private string _moduleRef;
        private string _articleId;
        private SessionParams _sessionParam;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);
                // Get systemkey from module name. (remove mod, add "API")
                var moduleName = base.ModuleConfiguration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";
                _articleId = DNNrocketUtils.RequestParam(Context, "pid");
                string skinSrcAdmin = "?SkinSrc=rocketadmin";
                if (DNNrocketUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    if (_articleId == null || _articleId == "")
                        Response.Redirect(EditUrl() + skinSrcAdmin, false);
                    else
                        Response.Redirect(EditUrl("pid", _articleId) + skinSrcAdmin, false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.Set("pid", _articleId);
                _sessionParam.Set("moduleedit", "True");

                var strHeader1 = RocketEcommerceAPIUtils.AdminHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "adminheader.cshtml");
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
                var strOut = RocketEcommerceAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml");
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