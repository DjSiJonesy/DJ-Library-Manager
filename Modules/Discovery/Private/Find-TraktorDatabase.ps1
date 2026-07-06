function Find-TraktorDatabase {

<#
.SYNOPSIS
Locates the Traktor database.

.DESCRIPTION
Searches the standard Traktor data locations for the collection
database and returns information about the discovered database.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    $databasePath = Find-DatabaseFile `
        -SearchPaths @(
            "$env:USERPROFILE\Documents\Native Instruments\Traktor 3.x",
            "$env:USERPROFILE\Documents\Native Instruments\Traktor 4.x",
            "$env:USERPROFILE\Documents\Native Instruments"
        ) `
        -FileName "collection.nml"

    $found = $null -ne $databasePath

    $lastModified = $null

    if ($found) {
        $lastModified = (Get-Item -LiteralPath $databasePath).LastWriteTime
    }

    [PSCustomObject]@{

        PSTypeName   = 'DJLM.ProviderDatabase'

        Provider     = 'Traktor'

        Found        = $found

        DatabasePath = $databasePath

        LastModified = $lastModified

    }

}