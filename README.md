# RocketModules

This is a collection of modules that use RocketCDS.

# RocketContentAPI modules

## RocketContentMod

RocketContentMod is a module that connects to the RocketContentAPI.  AppThemes are selected in the module settings.

# RocketDirectoryAPI modules

## RocketDirectoryMod

RocketDirectoryMod is a module that connects to the RocketContentAPI.  AppThemes are selected in the directory admin settings.  
*Dependant on system:*  
*RocketDirectoryAPI*  

## RocketBusinessMod

RocketBusinessMod is a shortcut modules that points to the "RocketDirectoryMod".  When using this modules the modules name is used to created the systemkey for RocketCDS.  
*Dependant on system:*  
*RocketDirectoryAPI*   
*RocketBusinessAPI*  

# Development with RocketDirectoryAPI

RocketDirectoryAPI is a directory system that can be used for different systems.  The systemkey defines what system the modules use.  

RocketDirectoryMod uses the "ModuleName" to calculate which system the modules uses.  The "Mod" seection of the name is replaced with "API".  This gives access to the system that the modules has been designed to use.  

Only the "RocketDirectoryMod" module has any active code, the other modules that use RocketDirectoryAPI are only shortcuts to RocketDirectoryMod, with helpful names.  

## Creating a RocketDirectoryAPI

There is only a need to create a new module if a new system is created that is using RocketDirectoryAPI.  
The easiest way is to copy the RocketBusinessMod project and rename all "RocketBusiness" to you new project name. In both uppercase and lowercase.    
There is no active code, only the DNN manifest and posssibly an icon.  It is also using DNNpacker, but that is optional.

**REMEMBER: The Name of the module MUST match the systemkey with the word "Mod" added.  "[SystemKey]Mod"**
