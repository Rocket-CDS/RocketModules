# DNN MVC Dynamic Skin Changes - Solution Documentation

## Problem Statement

DNN Platform's MVC Pipeline does not support dynamic skin changes at runtime. When using URL parameters like `?ctl=Edit` or `?ctl=Module` with MVC modules, the skin cannot be changed programmatically through:
- HTTP Modules
- `IPageContributor.ConfigurePage()` 
- Setting `PortalSettings.ActiveTab.SkinSrc`

This is due to DNN's MVC architecture where `SkinModelFactory.CreateSkinModel()` reads the skin from a fresh/cached `PortalSettings` instance that ignores runtime modifications.

## Root Cause Analysis

### DNN MVC Pipeline Execution Order

```
1. HTTP Request arrives
2. DnnMvcPageHandler.ProcessRequest() starts
3. MVC Module Control loads
4. IPageContributor.ConfigurePage() executes ? We can set skin here
5. DnnPageController initializes
6. SkinModelFactory.CreateSkinModel() loads skin ? DNN reads fresh PortalSettings
7. Skin is rendered
```

**The Issue**: Step 6 gets `PortalSettings` from cache/database, ignoring changes made in step 4.

### Code Location

File: `DotNetNuke.Web.MvcPipeline\ModelFactories\SkinModelFactory.cs`

Lines 148-158:
```csharp
// load assigned skin
if (skin == null)
{
    // DNN-6170 ensure skin value is culture specific
    skinSource = Globals.IsAdminSkin() 
        ? PortalController.GetPortalSetting(...) 
        : portalSettings.ActiveTab.SkinSrc;  // ? Always reads from cached PortalSettings
    
    if (!string.IsNullOrEmpty(skinSource))
    {
        skinSource = SkinController.FormatSkinSrc(skinSource, portalSettings);
        skin = this.LoadSkin(pageController, skinSource);
    }
}
```

## Recommended Solution

### Option 1: HttpContext.Items Override (RECOMMENDED)

This is the **minimal, non-breaking change** to DNN Platform that enables dynamic skin changes.

#### Changes Required to DNN Platform

**File**: `DotNetNuke.Web.MvcPipeline\ModelFactories\SkinModelFactory.cs`

**Line**: 148 (in the `CreateSkinModel` method)

**Replace this:**
```csharp
// load assigned skin
if (skin == null)
{
    // DNN-6170 ensure skin value is culture specific
    skinSource = Globals.IsAdminSkin() 
        ? PortalController.GetPortalSetting(this.portalController, "DefaultAdminSkin", portalSettings.PortalId, this.hostSettings.DefaultPortalSkin, portalSettings.CultureCode) 
        : portalSettings.ActiveTab.SkinSrc;
    
    if (!string.IsNullOrEmpty(skinSource))
    {
        skinSource = SkinController.FormatSkinSrc(skinSource, portalSettings);
        skin = this.LoadSkin(pageController, skinSource);
    }
}
```

**With this:**
```csharp
// load assigned skin
if (skin == null)
{
    // CHECK FOR DYNAMIC SKIN OVERRIDE FIRST (for extensibility)
    var dynamicSkinOverride = HttpContext.Current?.Items["DNN_SkinOverride"] as string;
    
    if (!string.IsNullOrEmpty(dynamicSkinOverride))
    {
        // Use the dynamic override
        skinSource = dynamicSkinOverride;
    }
    else
    {
        // DNN-6170 ensure skin value is culture specific
        skinSource = Globals.IsAdminSkin() 
            ? PortalController.GetPortalSetting(this.portalController, "DefaultAdminSkin", portalSettings.PortalId, this.hostSettings.DefaultPortalSkin, portalSettings.CultureCode) 
            : portalSettings.ActiveTab.SkinSrc;
    }
    
    if (!string.IsNullOrEmpty(skinSource))
    {
        skinSource = SkinController.FormatSkinSrc(skinSource, portalSettings);
        skin = this.LoadSkin(pageController, skinSource);
    }
}
```

**Total Changes**: 7 lines added (non-breaking, backward compatible)

### Implementation in Your MVC Module

Once the DNN Platform change is made, implement in your MVC control:

#### EditControl.cs

```csharp
using DNNrocketAPI.Components;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
using System;
using System.Web;

namespace RocketContentRazor.Controls
{
    public class EditControl : RazorModuleControlBase, IPageContributor
    {
        public void ConfigurePage(PageConfigurationContext context)
        {
            try
            {
                // Set the dynamic skin override in HttpContext.Items
                // This will be read by DNN's SkinModelFactory.CreateSkinModel()
                HttpContext.Current.Items["DNN_SkinOverride"] = 
                    "/Portals/_default/Skins/rocketedit/rocketedit.ascx";
                
                LogUtils.LogSystem("EditControl: Set skin override to rocketedit");
                
                // Rest of your ConfigurePage code...
                _moduleRef = PortalSettings.PortalId + "_ModuleID_" + ModuleContext.ModuleId;
                // ... etc
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
            }
        }

        public override IRazorModuleResult Invoke()
        {
            // Your existing Invoke code...
        }
    }
}
```

#### SettingsControl.cs

```csharp
public class SettingsControl : RazorModuleControlBase, IPageContributor
{
    public void ConfigurePage(PageConfigurationContext context)
    {
        try
        {
            // Set the dynamic skin override
            HttpContext.Current.Items["DNN_SkinOverride"] = 
                "/Portals/_default/Skins/rocketedit/rocketedit.ascx";
            
            LogUtils.LogSystem("SettingsControl: Set skin override to rocketedit");
            
            context.PageService.SetTitle("Rocket Content Settings");
        }
        catch (Exception ex)
        {
            LogUtils.LogException(ex);
        }
    }
    
    // ... rest of code
}
```

## Skin File Structure

### Directory Layout

```
/Portals/_default/Skins/rocketedit/
??? rocketedit.ascx              (Webforms skin - optional)
??? skin.css                     (Skin styles)
??? Views/
    ??? rocketedit.cshtml        (MVC skin - REQUIRED)
```

### MVC Skin File Template

**File**: `/Portals/_default/Skins/rocketedit/Views/rocketedit.cshtml`

```razor
@using DotNetNuke.Web.MvcPipeline.Models
@using DotNetNuke.Web.MvcPipeline.Skins
@model SkinModel

<!DOCTYPE html>
<html lang="@Model.CultureCode">
<head>
    @Html.SkinHead(Model)
    <style>
        .rocket-edit-skin {
            background: #f0f0f0;
            padding: 20px;
        }
    </style>
</head>
<body class="@Model.BodyClass rocket-edit-mode">
    <div class="rocket-edit-skin">
        <header>
            <h1>Rocket Content Editor</h1>
        </header>
        
        <main>
            @Html.Pane(Model, "ContentPane")
        </main>
        
        <footer>
            <!-- Optional footer -->
        </footer>
    </div>
    
    @Html.SkinPartial(Model, "_ToastMessage")
</body>
</html>
```

**CRITICAL**: The `@model` directive MUST be `SkinModel`, not `PageModel`.

## Alternative Solution: Reusable Helper

Create a helper class to centralize skin override logic:

### RocketSkinHelper.cs

```csharp
using System;
using System.Web;
using DNNrocketAPI.Components;
using DotNetNuke.Entities.Portals;

namespace RocketContentRazor.Helpers
{
    public static class RocketSkinHelper
    {
        private const string SKIN_OVERRIDE_KEY = "DNN_SkinOverride";
        
        /// <summary>
        /// Set a dynamic skin override for the current request
        /// </summary>
        public static void SetSkinOverride(PortalSettings portalSettings, string skinName)
        {
            if (portalSettings == null || string.IsNullOrEmpty(skinName))
                return;
            
            var skinPath = $"/Portals/_default/Skins/{skinName}/{skinName}.ascx";
            
            HttpContext.Current.Items[SKIN_OVERRIDE_KEY] = skinPath;
            
            LogUtils.LogSystem($"RocketSkinHelper: Set skin override to {skinPath}");
        }
        
        /// <summary>
        /// Set skin based on the ctl parameter
        /// </summary>
        public static void SetSkinByControlKey(PortalSettings portalSettings, string controlKey)
        {
            string skinName = null;
            
            switch (controlKey?.ToLower())
            {
                case "edit":
                case "module":
                    skinName = "rocketedit";
                    break;
                case "adminpanel":
                    skinName = "rocketadmin";
                    break;
                case "recyclebin":
                    skinName = "rocketedit";
                    break;
            }
            
            if (!string.IsNullOrEmpty(skinName))
            {
                SetSkinOverride(portalSettings, skinName);
            }
        }
    }
}
```

### Usage in Controls

```csharp
public void ConfigurePage(PageConfigurationContext context)
{
    try
    {
        // Simple one-line skin override
        RocketSkinHelper.SetSkinOverride(PortalSettings, "rocketedit");
        
        // OR use ctl parameter detection
        var ctl = HttpContext.Current.Request.QueryString["ctl"];
        RocketSkinHelper.SetSkinByControlKey(PortalSettings, ctl);
        
        // Rest of your code...
    }
    catch (Exception ex)
    {
        LogUtils.LogException(ex);
    }
}
```

## Testing the Solution

### 1. Apply DNN Platform Changes

```bash
cd C:\DNNdevNoForms\Dnn.Platform\DNN Platform\DotNetNuke.Web.MvcPipeline
# Edit SkinModelFactory.cs as described above
# Rebuild DNN Platform
# Copy DotNetNuke.Web.MvcPipeline.dll to your DNN bin folder
```

### 2. Update Your Module Controls

Add the skin override code to:
- `EditControl.ConfigurePage()`
- `SettingsControl.ConfigurePage()`
- Any other controls that need custom skins

### 3. Test URLs

- **Edit**: `http://yoursite.com/page?ctl=Edit&mid=123`
- **Settings**: `http://yoursite.com/page?ctl=Module&mid=123`
- **Admin Panel**: `http://yoursite.com/page?ctl=AdminPanel&mid=123`

### 4. Verify in Browser

1. Navigate to edit page
2. View page source
3. Look for: `<link href="/Portals/_default/Skins/rocketedit/..."`
4. Verify rocketedit skin is loaded

## Troubleshooting

### Problem: Skin still not changing

**Check 1**: Verify DNN Platform changes were applied
```csharp
// Add this temporarily to SkinModelFactory.cs line 148
LogUtils.LogSystem($"SkinModelFactory: Dynamic override = {HttpContext.Current?.Items["DNN_SkinOverride"]}");
```

**Check 2**: Verify HttpContext.Items is being set
```csharp
// In ConfigurePage
HttpContext.Current.Items["DNN_SkinOverride"] = "/Portals/_default/Skins/rocketedit/rocketedit.ascx";
LogUtils.LogSystem($"Set override: {HttpContext.Current.Items["DNN_SkinOverride"]}");
```

**Check 3**: Clear DNN cache
```bash
# In DNN
Settings > Site Settings > Advanced > Clear Cache
```

### Problem: Compilation error in rocketedit.cshtml

**Error**: `CS0103: The name 'model' does not exist`

**Solution**: Change `@model PageModel` to `@model SkinModel`

### Problem: Skin file not found

**Error**: Skin not loading

**Solution**: Verify file path structure
```
/Portals/_default/Skins/rocketedit/Views/rocketedit.cshtml
```

Note: `Views` folder is REQUIRED for MVC skins.

## Benefits of This Solution

1. ? **Non-Breaking**: Existing DNN functionality unchanged
2. ? **Backward Compatible**: Works with all existing skins
3. ? **Minimal Code**: Only 7 lines added to DNN Platform
4. ? **Extensible**: Other modules can use the same mechanism
5. ? **No Performance Impact**: Simple dictionary lookup
6. ? **Works for All MVC Modules**: Not specific to RocketContent

## Future Enhancements

### Option 2: ISkinProvider Interface (More Robust)

For a more enterprise solution, DNN could add an extensibility interface:

```csharp
namespace DotNetNuke.Web.MvcPipeline.Interfaces
{
    public interface ISkinProvider
    {
        string GetSkinSrc(PortalSettings portalSettings, string controlKey);
        int Priority { get; } // For multiple providers
    }
}
```

Then in `SkinModelFactory`:
```csharp
var skinProviders = Globals.GetCurrentServiceProvider()
    .GetServices<ISkinProvider>()
    .OrderBy(p => p.Priority);

foreach (var provider in skinProviders)
{
    var customSkin = provider.GetSkinSrc(portalSettings, controlKey);
    if (!string.IsNullOrEmpty(customSkin))
    {
        skinSource = customSkin;
        break;
    }
}
```

This allows for dependency injection and multiple skin providers.

## Contributing to DNN Platform

To propose this change to DNN Platform:

1. **Fork the Repository**
   ```bash
   # Fork https://github.com/dnnsoftware/Dnn.Platform
   ```

2. **Create Feature Branch**
   ```bash
   git checkout -b feature/mvc-dynamic-skin-override
   ```

3. **Make Changes**
   - Edit `SkinModelFactory.cs` as described
   - Add XML documentation comments
   - Add unit tests if possible

4. **Create Pull Request**
   - Title: "Add HttpContext.Items skin override support for MVC modules"
   - Description: Reference this documentation
   - Tag: `enhancement`, `mvc`

5. **Community Discussion**
   - Post in DNN Forums: https://forums.dnncommunity.org/
   - Reference this use case

## Summary

The **recommended solution** is to:

1. ? Add 7 lines to `DotNetNuke.Web.MvcPipeline\ModelFactories\SkinModelFactory.cs`
2. ? Use `HttpContext.Current.Items["DNN_SkinOverride"]` in `ConfigurePage()`
3. ? Create MVC skin at `/Portals/_default/Skins/rocketedit/Views/rocketedit.cshtml`

This provides a clean, extensible solution that works for all DNN MVC modules.

## References

- DNN Platform Repository: https://github.com/dnnsoftware/Dnn.Platform
- DNN MVC Pipeline Documentation: https://dnndocs.com/
- This implementation: RocketContentRazor module

---

**Document Version**: 1.0  
**Last Updated**: 2024-02-14  
**Author**: Rocket CDS Team  
**License**: MIT
