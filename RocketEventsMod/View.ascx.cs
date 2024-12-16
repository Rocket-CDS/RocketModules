using DNNrocketAPI.Components;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using RocketDirectoryAPI.Components;
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
using RazorEngine.Text;
using System.Security.Cryptography;
using System.Text;
using Rocket.AppThemes.Components;
using static DotNetNuke.Entities.Portals.PortalSettings;

namespace RocketEventsMod
{
    public partial class View : PortalModuleBase, IActionable
    {
        private string _systemkey;
        private bool _hasEditAccess;
        private string _moduleRef;
        private SessionParams _sessionParam;
        private ModuleContentLimpet _moduleSettings;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                // Get systemkey from module name. (remove mod, add "API")
                var moduleName = base.ModuleConfiguration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";
                _moduleRef = PortalId + "_ModuleID_" + ModuleId;

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                var context = HttpContext.Current;
                var urlparams = new Dictionary<string,string>();
                var paramInfo = new SimplisityInfo();
                // get all query string params
                foreach (string key in context.Request.QueryString.AllKeys)
                {
                    if (key != null)
                    {
                        var keyValue = context.Request.QueryString[key];
                        paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
                        urlparams.Add(key.ToLower(), keyValue);
                    }
                }

                var jsonparams = DNNrocketUtils.GetCookieValue("simplisity_sessionparams");
                if (jsonparams != "")
                {
                    try
                    {
                        var simplisity_sessionparams = SimplisityJson.DeserializeJson(jsonparams, "cookie");
                        paramInfo.AddXmlNode(simplisity_sessionparams.XMLData, "cookie", "genxml");
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = TabId;
                _sessionParam.ModuleId = ModuleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.Url = context.Request.Url.ToString();
                _sessionParam.UrlFriendly = DNNrocketUtils.NavigateURL(TabId, urlparams);
                if (urlparams.ContainsKey("search") && !String.IsNullOrEmpty(urlparams["search"])) _sessionParam.SearchText = urlparams["search"];
                if (urlparams.ContainsKey("page") && GeneralUtils.IsNumeric(urlparams["page"])) _sessionParam.Page = Convert.ToInt32(urlparams["page"]);

                _moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                // Display Month (from URL)
                var calMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var calYear = _sessionParam.GetInt("calyear");
                if (calYear == 0) calYear = DateTime.Now.Year;
                var calMonth = _sessionParam.GetInt("calmonth");
                if (calMonth == 0) calMonth = DateTime.Now.Month;
                if (calMonth > 0 && calYear > 0) calMonthStartDate = new DateTime(calYear, calMonth, 1, 0, 0, 0).Date;

                // if we have "Search" in the URL params use it as searchtext.
                if (_sessionParam.Get("search") != "") _sessionParam.Set("searchtext", _sessionParam.Get("search"));
                var searchText = _sessionParam.Get("searchtext");
                var yDate = _sessionParam.GetInt("year");
                var mDate = _sessionParam.GetInt("month");
                
                if (mDate > 0 && yDate> 0)
                {
                    var monthStartDate = DateTime.Now;
                    var monthEndDate = DateTime.Now;
                    if (searchText == "")
                    {
                        if (mDate == 0)
                        {
                            monthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-2).Month, 1, 0, 0, 0).Date;
                            monthEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 0, 0, 0).Date;
                        }
                        else
                        {
                            monthStartDate = new DateTime(yDate, mDate, 1, 0, 0, 0).Date;
                            monthEndDate = new DateTime(yDate, mDate, DateTime.DaysInMonth(yDate, mDate), 0, 0, 0).Date;
                        }
                    }
                    else
                    {
                        // do search on 3 years
                        monthStartDate = DateTime.Now.AddYears(-2).Date;
                        monthEndDate = DateTime.Now.AddYears(1).Date;
                    }
                    _sessionParam.Set("searchdate1", monthStartDate.ToString("O"));
                    _sessionParam.Set("searchdate2", monthEndDate.ToString("O"));
                }

                // Add default searchdates
                var viewdays = _moduleSettings.Record.GetXmlPropertyInt("genxml/settings/viewdays");
                var searchdate1 = DateTime.Now.AddYears(-2).Date.ToString("O");
                var searchdate2 = DateTime.Now.AddDays(viewdays).Date.ToString("O");
                if (_sessionParam.Get("searchdate1") == "") _sessionParam.Set("searchdate1", searchdate1);
                if (_sessionParam.Get("searchdate2") == "") _sessionParam.Set("searchdate2", searchdate2);


                var appThemeSystem = AppThemeUtils.AppThemeSystem(PortalId, _systemkey);
                var portalData = new PortalLimpet(PortalId);
                var appTheme = new AppThemeLimpet(_moduleSettings.PortalId, _moduleSettings.AppThemeAdminFolder, _moduleSettings.AppThemeAdminVersion, _moduleSettings.ProjectName);
                DNNrocketUtils.InjectDependacies(_moduleRef, Page, appTheme, _moduleSettings.ECOMode, PortalSettings.ActiveTab.SkinSrc, portalData.EngineUrlWithProtocol, appThemeSystem.AppThemeVersionFolderRel);

                var strHeader2 = RocketDirectoryAPIUtils.ViewHeader(PortalId, _systemkey, _moduleRef, _sessionParam, "viewlastheader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader2);

                if (_hasEditAccess)
                {
                    // Set langauge, so editing with simplity gets correct language
                    var lang = DNNrocketUtils.GetCurrentCulture();
                    DNNrocketUtils.SetCookieValue("simplisity_editlanguage", lang);
                    var qlang = HttpContext.Current.Request.QueryString["language"];
                    if (qlang != null && DNNrocketUtils.ValidCulture(qlang)) lang = qlang;
                    DNNrocketUtils.SetCookieValue("simplisity_language", lang);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            if (_moduleSettings.InjectJQuery) JavaScript.RequestRegistration(CommonJs.jQuery);

            var strOut = RocketDirectoryAPIUtils.DisplayView(PortalId, _systemkey, _moduleRef,  _sessionParam,"", "loadsettings");
            if (strOut == "loadsettings")
            {
                strOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml", false);
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                var redirectUrl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();
                strOut = strOut.Replace("{redirecturl}", redirectUrl);
                CacheUtils.ClearAllCache(_systemkey + PortalId);
            }
            if (_hasEditAccess)
            {
                var editbuttonkey = "editbuttons" + _moduleRef + "_" + UserId + "_" + _sessionParam.CultureCode;
                var viewButtonsOut = CacheUtils.GetCache(editbuttonkey, _moduleRef);
                if (viewButtonsOut == null)
                {
                    var articleid = RequestParam(Context, RocketDirectoryAPIUtils.UrlQueryArticleKey(PortalId, _systemkey));
                    string[] parameters;
                    parameters = new string[1];
                    parameters[0] = string.Format("{0}={1}", "ModuleId", ModuleId.ToString());
                    var settingsurl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();

                    var userParams = new UserParams("ModuleID:" + ModuleId, true);
                    if (GeneralUtils.IsNumeric(articleid))
                    {
                        _sessionParam.Set("articleid", articleid);
                        userParams.Set("editurl", EditUrl("articleid", articleid, "AdminPanel"));
                    }
                    userParams.Set("settingsurl", settingsurl);
                    userParams.Set("appthemeurl", EditUrl("AppTheme"));
                    userParams.Set("adminpanelurl", EditUrl("AdminPanel"));
                    userParams.Set("viewurl", Context.Request.Url.ToString()); // Legacy
                    userParams.Set("viewtabid", this.PortalSettings.ActiveTab.TabID.ToString());

                    viewButtonsOut = RocketDirectoryAPIUtils.DisplaySystemView(PortalId, _systemkey, _moduleRef, _sessionParam, "ViewEditButtons.cshtml");
                    CacheUtils.SetCache("editbuttons" + _moduleRef, viewButtonsOut, _moduleRef);
                }
                strOut = viewButtonsOut + strOut;
            }
            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);
        }

        private static string RequestParam(HttpContext context, string paramName)
        {
            string result = null;

            if (context.Request.Form.Count != 0)
            {
                result = Convert.ToString(context.Request.Form[paramName]);
            }

            if (result == null)
            {
                if (context.Request.QueryString.Count != 0)
                {
                    result = Convert.ToString(context.Request.QueryString[paramName]);
                }
            }

            return (result == null) ? String.Empty : result.Trim();
        }

        #region Optional Interfaces

        /// <summary>
        /// The ModuleActions builds the module menu, for actions available.
        /// </summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var moduleSettings = new ModuleContentLimpet(PortalId, _moduleRef, _systemkey, ModuleId, TabId);

                var actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), "Admin Panel", "", "", "edit_app.svg", EditUrl("AdminPanel"), false, SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion

    }
}