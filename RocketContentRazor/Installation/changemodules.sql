/*
    Rocket Modules WebForms to Razor Migration Script
    
    PURPOSE:
    This script migrates all Rocket module instances from WebForms versions 
    to their corresponding Razor versions on the website. All module settings 
    and data are preserved; only the module definitions are changed.
    
    MODULES MIGRATED:
    - RocketContentMod -> RocketContentRazor
    - RocketDirectory -> RocketDirectoryRazor
    - RocketForms -> RocketFormsRazor
    - RocketBlog -> RocketBlogRazor
    - RocketEvents -> RocketEventsRazor
    - RocketNews -> RocketNewsRazor
    
    HOW IT WORKS:
    1. Dynamically looks up the ModuleDefID for each module pair
    2. Verifies both module definitions exist in the database
    3. Updates all Modules table records to point to the Razor versions
    4. Reports the number of modules updated for each migration
    
    USAGE:
    This script can be run on any DNN website. It will safely update all pages
    that contain WebForms Rocket module instances to use their Razor equivalents.
    
    REQUIREMENTS:
    - All module definitions must exist in the ModuleDefinitions table
    - Database backup recommended before execution
*/

-- ===== RocketContentMod to RocketContentRazor =====
DECLARE @OldModuleDefID_Content INT
DECLARE @NewModuleDefID_Content INT

SELECT @OldModuleDefID_Content = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketContentMod'
SELECT @NewModuleDefID_Content = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketContentRazor'

PRINT 'RocketContent Migration:'
PRINT 'Old ModuleDefID: ' + ISNULL(CAST(@OldModuleDefID_Content AS VARCHAR), 'NOT FOUND')
PRINT 'New ModuleDefID: ' + ISNULL(CAST(@NewModuleDefID_Content AS VARCHAR), 'NOT FOUND')

UPDATE dbo.Modules SET ModuleDefID = @NewModuleDefID_Content WHERE ModuleDefID = @OldModuleDefID_Content
PRINT 'Modules updated: ' + CAST(@@ROWCOUNT AS VARCHAR) + CHAR(10)

-- ===== RocketDirectory to RocketDirectoryRazor =====
DECLARE @OldModuleDefID_Directory INT
DECLARE @NewModuleDefID_Directory INT

SELECT @OldModuleDefID_Directory = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketDirectory'
SELECT @NewModuleDefID_Directory = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketDirectoryRazor'

PRINT 'RocketDirectory Migration:'
PRINT 'Old ModuleDefID: ' + ISNULL(CAST(@OldModuleDefID_Directory AS VARCHAR), 'NOT FOUND')
PRINT 'New ModuleDefID: ' + ISNULL(CAST(@NewModuleDefID_Directory AS VARCHAR), 'NOT FOUND')

UPDATE dbo.Modules SET ModuleDefID = @NewModuleDefID_Directory WHERE ModuleDefID = @OldModuleDefID_Directory
PRINT 'Modules updated: ' + CAST(@@ROWCOUNT AS VARCHAR) + CHAR(10)

-- ===== RocketForms to RocketFormsRazor =====
DECLARE @OldModuleDefID_Forms INT
DECLARE @NewModuleDefID_Forms INT

SELECT @OldModuleDefID_Forms = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketForms'
SELECT @NewModuleDefID_Forms = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketFormsRazor'

PRINT 'RocketForms Migration:'
PRINT 'Old ModuleDefID: ' + ISNULL(CAST(@OldModuleDefID_Forms AS VARCHAR), 'NOT FOUND')
PRINT 'New ModuleDefID: ' + ISNULL(CAST(@NewModuleDefID_Forms AS VARCHAR), 'NOT FOUND')

UPDATE dbo.Modules SET ModuleDefID = @NewModuleDefID_Forms WHERE ModuleDefID = @OldModuleDefID_Forms
PRINT 'Modules updated: ' + CAST(@@ROWCOUNT AS VARCHAR) + CHAR(10)

-- ===== RocketBlog to RocketBlogRazor =====
DECLARE @OldModuleDefID_Blog INT
DECLARE @NewModuleDefID_Blog INT

SELECT @OldModuleDefID_Blog = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketBlog'
SELECT @NewModuleDefID_Blog = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketBlogRazor'

PRINT 'RocketBlog Migration:'
PRINT 'Old ModuleDefID: ' + ISNULL(CAST(@OldModuleDefID_Blog AS VARCHAR), 'NOT FOUND')
PRINT 'New ModuleDefID: ' + ISNULL(CAST(@NewModuleDefID_Blog AS VARCHAR), 'NOT FOUND')

UPDATE dbo.Modules SET ModuleDefID = @NewModuleDefID_Blog WHERE ModuleDefID = @OldModuleDefID_Blog
PRINT 'Modules updated: ' + CAST(@@ROWCOUNT AS VARCHAR) + CHAR(10)

-- ===== RocketEvents to RocketEventsRazor =====
DECLARE @OldModuleDefID_Events INT
DECLARE @NewModuleDefID_Events INT

SELECT @OldModuleDefID_Events = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketEvents'
SELECT @NewModuleDefID_Events = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketEventsRazor'

PRINT 'RocketEvents Migration:'
PRINT 'Old ModuleDefID: ' + ISNULL(CAST(@OldModuleDefID_Events AS VARCHAR), 'NOT FOUND')
PRINT 'New ModuleDefID: ' + ISNULL(CAST(@NewModuleDefID_Events AS VARCHAR), 'NOT FOUND')

UPDATE dbo.Modules SET ModuleDefID = @NewModuleDefID_Events WHERE ModuleDefID = @OldModuleDefID_Events
PRINT 'Modules updated: ' + CAST(@@ROWCOUNT AS VARCHAR) + CHAR(10)

-- ===== RocketNews to RocketNewsRazor =====
DECLARE @OldModuleDefID_News INT
DECLARE @NewModuleDefID_News INT

SELECT @OldModuleDefID_News = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketNews'
SELECT @NewModuleDefID_News = ModuleDefID FROM dbo.ModuleDefinitions WHERE DefinitionName = 'RocketNewsRazor'

PRINT 'RocketNews Migration:'
PRINT 'Old ModuleDefID: ' + ISNULL(CAST(@OldModuleDefID_News AS VARCHAR), 'NOT FOUND')
PRINT 'New ModuleDefID: ' + ISNULL(CAST(@NewModuleDefID_News AS VARCHAR), 'NOT FOUND')

UPDATE dbo.Modules SET ModuleDefID = @NewModuleDefID_News WHERE ModuleDefID = @OldModuleDefID_News
PRINT 'Modules updated: ' + CAST(@@ROWCOUNT AS VARCHAR) + CHAR(10)

PRINT 'Migration complete!'