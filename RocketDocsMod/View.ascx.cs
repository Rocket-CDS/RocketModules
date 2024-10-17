using System;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using Newtonsoft.Json;
using DotNetNuke.Common.Utilities;
using Simplisity;
using DotNetNuke.Entities.Users;
using System.Linq;
using System.Text;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DNNrocketAPI.Components;
using System.Web.UI;
using RazorEngine.Templating;
using System.Runtime.InteropServices.ComTypes;
using RocketDocs.Components;
using System.Collections.Generic;
using System.Web;
using RocketPortal.Components;

namespace RocketDocsMod
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : PortalModuleBase, IActionable
    {
        #region Event Handlers

        private bool _hasEditAccess;
        public string ModuleLabel { get; set; }
        
        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);


                var strHeader1 = RocketDocsModUtils.ReadTemplate("viewlastheader.cshtml");
                PageIncludes.IncludeTextInHeader(Page, strHeader1);

                var strHeader2 = RocketDocsModUtils.ReadTemplate("viewfirstheader.cshtml");
                PageIncludes.IncludeTextInHeaderAt(Page, strHeader2, 0); 

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {

                base.OnLoad(e);

                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void PageLoad()
        {
            //var basePage = (DotNetNuke.Framework.CDefault)this.Page;
            //if (!String.IsNullOrWhiteSpace(metaSEO.Title)) basePage.Title = metaSEO.Title;
            //if (!String.IsNullOrWhiteSpace(metaSEO.Description)) basePage.MetaDescription = metaSEO.Description;
            //if (!String.IsNullOrWhiteSpace(metaSEO.KeyWords)) basePage.MetaKeywords = metaSEO.KeyWords;
        }
        protected override void OnPreRender(EventArgs e)
        {
            string editFlag = Request.QueryString["edit"];
            var portalContent = new PortalContentLimpet(PortalId, DNNrocketUtils.GetCurrentCulture());             
            var strOut = "";
            var articleData = new ArticleLimpet(PortalId, ModuleId.ToString(), DNNrocketUtils.GetCurrentCulture());
            var razorTempl = RocketDocsModUtils.ReadTemplate("view.cshtml");
            if (_hasEditAccess && editFlag == "1")
            {
                razorTempl = RocketDocsModUtils.ReadTemplate("viewedit.cshtml");
            }

            var passSettings = new Dictionary<string, string>();
            passSettings.Add("hasedit", _hasEditAccess.ToString());
            passSettings.Add("moduleid", ModuleId.ToString());
            var param = new string[] { "edit", "1" };
            passSettings.Add("editurl", DNNrocketUtils.NavigateURL(TabId, param));
            var param2 = new string[] { "edit", "0" };
            passSettings.Add("viewurl", DNNrocketUtils.NavigateURL(TabId, param2));


            if (editFlag != "1")
            {
                var mdtext = articleData.Info.GetXmlProperty("genxml/lang/genxml/textbox/summarykbase");
                mdtext = RocketDocsUtils.ReplaceInjectTokens(mdtext);
                mdtext = RocketDocsUtils.ReplaceInjectRazorTokens(mdtext);
                mdtext = RocketDocsUtils.ReplaceClassUrlTokens(mdtext);
                // Don't save summaykbase, only display
                articleData.Info.SetXmlProperty("genxml/lang/genxml/textbox/summarykbase", mdtext);

                if (String.IsNullOrEmpty(articleData.Info.GetXmlProperty("genxml/lang/genxml/textbox/summarykbase")))
                {
                    //var gitTree = GitHubUtils.GetGitHubTree("https://api.github.com/repos/Rocket-CDS/Documentation/git/trees/main?recursive=1");

                    var tabInfo = DNNrocketUtils.GetTabInfo(PortalId, TabId);
                    var tabFile = tabInfo.TabName.Replace(" ", "").Replace(".md", "") + ".md";
                    var tabName = tabInfo.TabName.Replace(" ", "");
                    var parentTabName1 = "";
                    var parentTabName2 = "";
                    var parentTabName3 = "";
                    if (tabInfo.ParentId > 0)
                    {
                        var parentTabInfo = DNNrocketUtils.GetTabInfo(PortalId, tabInfo.ParentId);
                        parentTabName1 = parentTabInfo.TabName.Replace(" ", "");
                        if (parentTabInfo.ParentId > 0)
                        {
                            var parentTabInfo2 = DNNrocketUtils.GetTabInfo(PortalId, parentTabInfo.ParentId);
                            parentTabName2 = parentTabInfo2.TabName.Replace(" ", "");
                            if (parentTabInfo2.ParentId > 0)
                            {
                                var parentTabInfo3 = DNNrocketUtils.GetTabInfo(PortalId, parentTabInfo2.ParentId);
                                parentTabName3 = parentTabInfo3.TabName.Replace(" ", "");
                            }
                        }
                    }
                    var urlTabName1 = tabName + "/" + tabFile;
                    var urlTabName2 = tabName + "/" + tabFile;
                    if (parentTabName1 != "")
                    {
                        urlTabName1 = parentTabName1 + "/" + tabFile;
                        urlTabName2 = parentTabName1 + "/" + tabName + "/" + tabFile;
                    }
                    if (parentTabName2 != "")
                    {
                        urlTabName1 = parentTabName2 + "/" + parentTabName1 + "/" + tabFile;
                        urlTabName2 = parentTabName2 + "/" + parentTabName1 + "/" + tabName + "/" + tabFile;
                    }
                    if (parentTabName3 != "")
                    {
                        urlTabName1 = parentTabName3 + "/" + parentTabName2 + "/" + parentTabName1 + "/" + tabFile;
                        urlTabName2 = parentTabName3 + "/" + parentTabName2 + "/" + parentTabName1 + "/" + tabName + "/" + tabFile;
                    }


                    var tokenUrl1 = portalContent.GitRawUserContentUrl + portalContent.GitHubRepo.TrimEnd('/') + "/refs/heads/main/" + urlTabName1;
                    var tokenUrl2 = portalContent.GitRawUserContentUrl + portalContent.GitHubRepo.TrimEnd('/') + "/refs/heads/main/" + urlTabName2;
                    var mdtext2 = RocketDocsUtils.GetGitHubMarkdown(tokenUrl1);
                    if (mdtext2 == "FAIL") mdtext2 = RocketDocsUtils.GetGitHubMarkdown(tokenUrl2); // check for root document
                    if (mdtext2 == "FAIL") mdtext2 = "";
                    articleData.Info.SetXmlProperty("genxml/lang/genxml/textbox/summarykbase", mdtext2);
                }
            }

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, null, passSettings, null, true);
            if (pr.StatusCode != "00")
                strOut = pr.ErrorMsg;
            else
                strOut = pr.RenderedText;

            // inject jQuery from DNN, to stop conflict with header.
            JavaScript.RequestRegistration(CommonJs.jQuery);


            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);

        }


        #endregion


        #region Optional Interfaces

        /// <summary>
        /// The ModuleActions builds the module menu, for actions available.
        /// </summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                return actions;
            }
        }

        #endregion



    }

}
