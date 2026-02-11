# RocketContentRazor

A DNN Razor module implementation of RocketContentMod, providing a clean Razor-based interface for the RocketContentAPI system.

## Overview

RocketContentRazor is a Razor version of the RocketContentMod module, using the DNN MVC Pipeline (Razor implementation) to display and edit content managed by the RocketContentAPI system.

## Key Features

- **Razor Views**: Clean, maintainable Razor syntax (.cshtml files)
- **DNN MVC Pipeline**: Uses DNN's Razor module pipeline architecture
- **RocketContentAPI Integration**: Leverages the existing RocketContentAPI backend
- **AppTheme Support**: Full support for RocketContentAPI AppThemes
- **Edit Mode**: Integrated edit interface using RocketContentAPI's admin system
- **RESTful Services**: Web API controller for AJAX operations

## Project Structure

```
RocketContentRazor/
??? Controls/
?   ??? ViewControl.cs      # View mode Razor control
?   ??? EditControl.cs      # Edit mode Razor control
??? Models/
?   ??? ArticleViewModel.cs # View model for Razor views
??? Services/
?   ??? ServiceRouteMapper.cs   # API route configuration
?   ??? ContentController.cs    # Web API controller
??? Views/
?   ??? View.cshtml         # View mode template
?   ??? Edit.cshtml         # Edit mode template
??? Properties/
    ??? AssemblyInfo.cs     # Assembly information
```

## Architecture

### Controls

**ViewControl.cs**
- Implements `RazorModuleControlBase` and `IPageContributor`
- Renders content using RocketContentAPI's DisplayView method
- Provides rendered HTML to the Razor view
- Handles page meta configuration (title, description)

**EditControl.cs**
- Implements `RazorModuleControlBase` and `IPageContributor`
- Renders admin interface using RocketContentAPI's DisplaySystemView
- Supports AJAX for admin operations
- Handles edit mode initialization

### Models

**ArticleViewModel.cs**
- Contains all data needed for Razor views
- Includes ArticleData, ArticleRows, ModuleSettings
- Pre-rendered content from AppThemes
- Navigation URLs (Edit, Settings, etc.)

### Views

**View.cshtml**
- Displays the rendered content from RocketContentAPI
- Shows edit buttons for authorized users
- Clean, simple layout that wraps AppTheme content

**Edit.cshtml**
- Admin interface wrapper
- Includes the rendered admin content from AppThemes
- Initializes JavaScript for admin functionality

### Services

**ContentController.cs**
- Web API controller for AJAX operations
- GetArticleData endpoint for data retrieval
- SaveArticle endpoint for updates (delegates to RocketContentAPI)

## Dependencies

- **DotNetNuke.dll** - Core DNN framework
- **DotNetNuke.Web.Mvc.dll** - DNN MVC Pipeline support
- **DNNrocketAPI** - Core Rocket API components
- **RocketContentAPI** - Content management API
- **RocketPortal** - Portal services
- **Simplisity** - Data handling library

## Key Differences from RocketContentMod

1. **No ASCX Files**: Uses Razor (.cshtml) instead of WebForms (.ascx)
2. **MVC Pipeline**: Uses DNN's Razor module pipeline instead of WebForms lifecycle
3. **Cleaner Separation**: View logic separated from control logic
4. **Type-Safe Models**: Strongly-typed view models
5. **Modern Syntax**: Razor syntax is cleaner and more maintainable

## How It Works

1. **View Mode**:
   - ViewControl.Invoke() is called
   - Gets ArticleData from RocketContentAPI
   - Renders content using AppTheme templates
   - Passes data to View.cshtml
   - View.cshtml displays the rendered HTML

2. **Edit Mode**:
   - EditControl.Invoke() is called
   - Gets admin interface from RocketContentAPI
   - Renders admin templates
   - Passes data to Edit.cshtml
   - Edit.cshtml displays admin interface with JavaScript initialization

3. **Data Flow**:
   ```
   DNN ? RazorControl ? RocketContentAPI ? AppTheme ? Razor View ? Browser
   ```

## Benefits of Razor Implementation

- **Cleaner Code**: Razor syntax is more readable than ASCX
- **Better IntelliSense**: Stronger type checking in views
- **Modern Approach**: Follows current .NET best practices
- **Easier Maintenance**: Separation of concerns between control and view
- **Flexibility**: Easier to customize views without changing control logic

## Usage

1. Install the module in DNN
2. Add to a page
3. Configure AppTheme in settings
4. View displays content using selected AppTheme
5. Edit button launches admin interface

## Future Enhancements

- Additional AJAX endpoints for real-time updates
- Custom Razor helper methods for common operations
- Enhanced view models with computed properties
- View-specific CSS and JavaScript bundles
- Progressive enhancement for better performance

## Notes

- This module reuses all RocketContentAPI backend logic
- AppThemes from RocketContentAPI work without modification
- Edit functionality uses the same admin system as RocketContentMod
- Module settings and data storage are identical to RocketContentMod

## Version

Version: 1.0.0.0
Target Framework: .NET Framework 4.8
DNN Version: 9.x+

## Author

Rocket CDS
Copyright © 2026
