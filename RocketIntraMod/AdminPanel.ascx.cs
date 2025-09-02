using DNNrocketAPI.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using RocketIntra.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static DotNetNuke.Entities.Portals.PortalSettings;

namespace RocketIntraMod
{
    public partial class AdminPanel : PortalModuleBase
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
                string skinSrcAdmin = "?SkinSrc=rocketadmin";
                string skinparm = DNNrocketUtils.RequestParam(Context, "SkinSrc");
                if (skinparm == "")
                {
                    Response.Redirect(EditUrl(DNNrocketUtils.RequestParam(Context, "ctl")) + skinSrcAdmin, false);
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                _systemkey = "rocketintra";
                _moduleRef = PortalId + "_ModuleID_" + ModuleId;
                _sessionParam = new SessionParams(new SimplisityInfo());
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.Set("articleid", articleid);
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

                PageIncludes.RemoveCssFile(Page, "skin.css"); //DNN always tries to load a skin.css, even if it does not exists.

                var strHeader1 = (string)CacheUtils.GetCache("rocketintra*headtext", PortalId.ToString());
                if (String.IsNullOrEmpty(strHeader1))
                {
                    var sysList = new List<string>();
                    sysList.Add(_systemkey.ToLower());
                    strHeader1 = RocketIntraUtils.AdminPanelHeader(PortalId, _systemkey, _moduleRef, _sessionParam);
                    var systemData = SystemSingleton.Instance("rocketintra");
                    var portalContent = new PortalContentLimpet(PortalId, _sessionParam.CultureCode);
                    var interfacelist = portalContent.PluginActveInterface(systemData);
                    foreach (var ri in interfacelist)
                    {
                        var sysKey = Path.GetFileNameWithoutExtension(ri.TemplateRelPath).ToLower();
                        if (!sysList.Contains(sysKey))
                        {
                            strHeader1 += RocketIntraUtils.DisplaySystemView(PortalId, sysKey, _moduleRef, _sessionParam, "AdminPanelheader.cshtml");
                            sysList.Add(sysKey);
                        }
                    }
                    CacheUtils.SetCache("rocketintra*headtext", strHeader1, PortalId.ToString());
                }
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
                var strOut = RocketIntraUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "AdminPanelLoad.cshtml");
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