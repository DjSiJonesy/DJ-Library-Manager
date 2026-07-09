function Import-VirtualDJDatabase {

<#
.SYNOPSIS
Imports a VirtualDJ database.xml file.

.DESCRIPTION
Loads the configured VirtualDJ database and returns a
DJ Library Manager VirtualDJ database object.

If no path is supplied, the configured database path from
Settings.json is used.

.PARAMETER Path
Optional path to the VirtualDJ database.xml file.

.EXAMPLE
$db = Import-VirtualDJDatabase

.EXAMPLE
$db = Import-VirtualDJDatabase `
    -Path "D:\VirtualDJ\database.xml"

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [string]
        $Path

    )

    $Configuration = Get-Configuration

    #
    # Use configured path if one wasn't supplied
    #

    if ([string]::IsNullOrWhiteSpace($Path)) {

        if (-not $Configuration.Providers.VirtualDJ.DatabasePath) {

            throw "Providers.VirtualDJ.DatabasePath is not configured in Settings.json."

        }

        $Path = $Configuration.Providers.VirtualDJ.DatabasePath

    }

    Write-Log "Importing VirtualDJ database..." -Level Information

    if (-not (Test-Path $Path)) {

        throw "Database file not found:`n$Path"

    }

    try {

        [xml]$xml = Get-Content `
            -Path $Path `
            -Encoding UTF8 `
            -Raw

    }
    catch {

        throw "Unable to load XML.`n$($_.Exception.Message)"

    }

    Write-Log "VirtualDJ database loaded." -Level Success

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.VirtualDJDatabase'

        Path = (Resolve-Path $Path).Path

        Xml = $xml

        Loaded = Get-Date

    }

}