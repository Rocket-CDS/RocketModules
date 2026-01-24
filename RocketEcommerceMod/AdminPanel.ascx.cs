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
    public partial class AdminPanel : RocketAdminPanelBase
    {
        private string _systemkey;
        private string _moduleRef;
        private SessionParams _sessionParam;
        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                var articleid = DNNrocketUtils.RequestParam(Context, "articleid");

                // Get systemkey from module name. (remove mod, add "API")
                var moduleName = base.ModuleConfiguration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";

                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.Set("articleid", articleid);
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                PageIncludes.RemoveCssFile(Page, "skin.css"); //DNN always tries to load a skin.css, even if it does not exists.
                var strHeader1 = RocketEcommerceAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "AdminPanelheader.cshtml");
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
                var strOut = RocketEcommerceAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "AdminPanelLoad.cshtml");
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