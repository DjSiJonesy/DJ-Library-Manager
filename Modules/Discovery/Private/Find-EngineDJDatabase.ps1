function Find-EngineDJDatabase {

<#
.SYNOPSIS
Locates the Engine DJ database.

.DESCRIPTION
Searches the standard Engine DJ data locations for the database and
returns information about the discovered database.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    $databasePath = Find-DatabaseFile `
        -SearchPaths @(
            "$env:LOCALAPPDATA\Engine DJ",
            "$env:APPDATA\Engine DJ",
            "$env:USERPROFILE\Music\Engine Library"
        ) `
        -FileName "m.db"

    $found = $null -ne $databasePath

    $lastModified = $null

    if ($found) {
        $lastModified = (Get-Item -LiteralPath $databasePath).LastWriteTime
    }

    [PSCustomObject]@{

        PSTypeName   = 'DJLM.ProviderDatabase'

        Provider     = 'EngineDJ'

        Found        = $found

        DatabasePath = $databasePath

        LastModified = $lastModified

    }

}