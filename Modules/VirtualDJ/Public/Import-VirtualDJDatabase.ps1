function Import-VirtualDJDatabase {
<#
.SYNOPSIS
Imports a VirtualDJ database.xml file.

.DESCRIPTION
Loads a VirtualDJ database.xml file and returns the XML document.
This is the foundation of all VirtualDJ analysis within DJ Library
Manager.

.PARAMETER Path
Path to the VirtualDJ database.xml file.

.EXAMPLE
$db = Import-VirtualDJDatabase -Path "D:\VirtualDJ\database.xml"

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Path

    )

    Write-Log "Importing VirtualDJ database..."

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

        Path = (Resolve-Path $Path).Path

        Xml = $xml

        Loaded = Get-Date

    }

}