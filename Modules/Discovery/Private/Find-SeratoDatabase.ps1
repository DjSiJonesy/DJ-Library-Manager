function Find-SeratoDatabase {

<#
.SYNOPSIS
Locates the Serato database.

.DESCRIPTION
Searches the standard Serato data locations for the music database and
returns information about the discovered database.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param()

    $databasePath = Find-DatabaseFile `
        -SearchPaths @(
            "$env:LOCALAPPDATA\Serato",
            "$env:LOCALAPPDATA\Serato\Serato DJ Pro",
            "$env:USERPROFILE\Music\_Serato_"
        ) `
        -FileName "database V2"

    $found = $null -ne $databasePath

    $lastModified = $null

    if ($found) {
        $lastModified = (Get-Item -LiteralPath $databasePath).LastWriteTime
    }

    [PSCustomObject]@{

        PSTypeName   = 'DJLM.ProviderDatabase'

        Provider     = 'Serato'

        Found        = $found

        DatabasePath = $databasePath

        LastModified = $lastModified

    }

}