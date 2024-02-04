# RocketModules

This is a collection of modules that are used as a UI between DNN and RocketCDS.

## RocketContentMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketContentAPI**  

## RocketDirectoryMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketDirectoryAPI**  

## RocketFormsMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketForms**  

## RocketBlogMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketDirectoryAPI**  
**RocketBlogAPI**  
**RocketDirectoryMod**  

## RocketBusinessMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketDirectoryAPI**  
**RocketBusinessAPI** [-]    
**RocketDirectoryMod**  

## RocketDocsMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketDocs**  

## RocketEcommerceMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketEcommerceAPI**  

## RocketEventsMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketDirectoryAPI**  
**RocketEventsAPI**  
**RocketDirectoryMod**  

## RocketIntraMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketIntraAPI**  [-]  

## RocketLibraryMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketDirectoryAPI**  
**RocketLibraryAPI**  [-]  
**RocketDirectoryMod**  

## RocketNewsMod
*Dependacy:*  
**DNNrocketAPI**  
**RocketDirectoryAPI**  
**RocketNewsAPI**  
**RocketDirectoryMod**  

*[-] = Not Open Source*

# Development with RocketDirectoryAPI

RocketDirectoryAPI is a directory system that can be used for different systems.  The systemkey defines what system the modules use.  

RocketDirectoryMod uses the "ModuleName" to calculate which system the modules uses.  The "Mod" seection of the name is replaced with "API".  This gives access to the system that the modules has been designed to use.  

Only the "RocketDirectoryMod" module has any active code, the other modules that use RocketDirectoryAPI are only shortcuts to RocketDirectoryMod, with helpful names.  

## Creating a RocketDirectoryAPI

There is only a need to create a new module if a new system is created that is using RocketDirectoryAPI.  
The easiest way is to copy the RocketBusinessMod project and rename all "RocketBusiness" to you new project name. In both uppercase and lowercase.    
There is no active code, only the DNN manifest and posssibly an icon.  It is also using DNNpacker, but that is optional.

**REMEMBER: The Name of the module MUST match the systemkey with the word "Mod" added.  "[SystemKey]Mod"**
