function Find-VirtualDJDatabase {

<#
.SYNOPSIS
Locates the VirtualDJ database.

.DESCRIPTION
Searches the standard VirtualDJ installation locations for the
database.xml file and returns information about the discovered
database.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    $databasePath = Find-DatabaseFile `
        -SearchPaths @(
            "$env:LOCALAPPDATA\VirtualDJ"
        ) `
        -FileName "database.xml"

    $found = $null -ne $databasePath

    $lastModified = $null

    if ($found) {
        $lastModified = (Get-Item -LiteralPath $databasePath).LastWriteTime
    }

    [PSCustomObject]@{

        PSTypeName   = 'DJLM.ProviderDatabase'

        Provider     = 'VirtualDJ'

        Found        = $found

        DatabasePath = $databasePath

        LastModified = $lastModified

    }

}