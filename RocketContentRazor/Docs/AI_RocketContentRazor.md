# RocketContentRazor Project Context

## Overview
This is a DNN MVC module using the Razor pipeline for content management.

We want the module to be compatible with but Webforms and MVC pipeline, althought setting will only be editable from Webforms.

## Key Conventions
- Target frameworks: .NET Framework 4.7.2, 4.8
- Uses DNN MVC Pipeline (RazorModuleControlBase)
- Module permissions checked via ModuleContext.Configuration
- Uses DNNrocketAPI components for data access

## Important Patterns
- ModuleContext.Configuration is used to get ModuleInfo (not this.ModuleConfiguration)
- Permission checks use ModulePermissionController.CanEditModuleContent(moduleInfo)
- Session params manage culture and module context

## Dependencies
- DotNetNuke.Web.MvcPipeline
- DNNrocketAPI.Components
- RocketContentAPI.Components


