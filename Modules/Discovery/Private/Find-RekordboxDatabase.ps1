function Find-RekordboxDatabase {

<#
.SYNOPSIS
Locates the rekordbox database.

.DESCRIPTION
Searches the standard rekordbox data locations for the master database
and returns information about the discovered database.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    $databasePath = Find-DatabaseFile `
        -SearchPaths @(
            "$env:APPDATA\Pioneer",
            "$env:APPDATA\rekordbox",
            "$env:LOCALAPPDATA\Pioneer",
            "$env:LOCALAPPDATA\rekordbox"
        ) `
        -FileName "master.db"

    $found = $null -ne $databasePath

    $lastModified = $null

    if ($found) {
        $lastModified = (Get-Item -LiteralPath $databasePath).LastWriteTime
    }

    [PSCustomObject]@{

        PSTypeName   = 'DJLM.ProviderDatabase'

        Provider     = 'rekordbox'

        Found        = $found

        DatabasePath = $databasePath

        LastModified = $lastModified

    }

}