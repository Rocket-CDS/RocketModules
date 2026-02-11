using System;
using System.Collections.Generic;
using RocketContentAPI.Components;

namespace RocketContentRazor.Models
{
    /// <summary>
    /// View model for displaying article content
    /// </summary>
    public class ArticleViewModel
    {
        public int PortalId { get; set; }
        public int ModuleId { get; set; }
        public int TabId { get; set; }
        public string ModuleRef { get; set; }
        public string CultureCode { get; set; }
        public bool IsEditable { get; set; }
        public string EditUrl { get; set; }
        public string SettingsUrl { get; set; }
        public string AppThemeUrl { get; set; }
        public string RecycleBinUrl { get; set; }
        
        // Article Data
        public ArticleLimpet ArticleData { get; set; }
        public List<ArticleRowLimpet> ArticleRows { get; set; }
        
        // Module Settings
        public ModuleContentLimpet ModuleSettings { get; set; }
        
        // Rendered Content
        public string RenderedContent { get; set; }
    }
}
