using DNNrocketAPI.Components;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common.Utilities.Internal;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RocketDocsMod
{
    public class RocketDocsModUtils
    {
        public static void IncludeTextInHeader(Page page, string TextToInclude)
        {
            if (TextToInclude != "") page.Header.Controls.Add(new LiteralControl(TextToInclude));
        }
        public static void IncludeTextInHeaderAt(Page page, string TextToInclude, int addAt = 0)
        {
            if (addAt == 0) addAt = page.Header.Controls.Count;
            if (TextToInclude != "") page.Header.Controls.AddAt(addAt, new LiteralControl(TextToInclude));
        }
        public static string GetLocalizeString(string keyName, string resourceFile = "")
        {
            if (resourceFile == "") resourceFile = "/DesktopModules/RocketModules/RocketDocsMod/App_LocalResources/Mod.ascx.resx";
            return Localization.GetString(keyName, resourceFile);
        }
        public static string NavigateURL(int tabId, string[] param)
        {
            return Globals.NavigateURL(tabId, "", param).ToString();
        }
        public static string ReadTemplate(string template)
        {
            var razorTemplateFileMapPath = DNNrocketUtils.MapPath("/DesktopModules/RocketModules/RocketDocsMod/Themes/config-w3/1.0/default/" + template);
            var rtn = (string)CacheUtils.GetCache(razorTemplateFileMapPath);
            if (String.IsNullOrEmpty(rtn))
            {
                rtn = FileSystemUtils.ReadFile(razorTemplateFileMapPath);
                CacheUtils.SetCache(razorTemplateFileMapPath, rtn);
            }
            return rtn;
        }

    }
}
