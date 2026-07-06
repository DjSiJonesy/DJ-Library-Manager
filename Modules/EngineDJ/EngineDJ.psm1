# ============================================================================
# Module: EngineDJ
# File:   EngineDJ.psm1
# Version: 1.0.0
#
# DJ Library Manager
# ============================================================================

Set-StrictMode -Version Latest

# Get the root of this module
$ModuleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

#
# Load Private Functions
#

$PrivateFolder = Join-Path $ModuleRoot 'Private'

if (Test-Path $PrivateFolder) {

    Get-ChildItem `
        -Path $PrivateFolder `
        -Filter '*.ps1' `
        -File |
    Sort-Object Name |
    ForEach-Object {

        . $_.FullName

    }

}

#
# Load Public Functions
#

$PublicFolder = Join-Path $ModuleRoot 'Public'

if (Test-Path $PublicFolder) {

    Get-ChildItem `
        -Path $PublicFolder `
        -Filter '*.ps1' `
        -File |
    Sort-Object Name |
    ForEach-Object {

        . $_.FullName

    }

}

#
# Export Public Functions
#

Export-ModuleMember -Function (

    Get-ChildItem `
        -Path $PublicFolder `
        -Filter '*.ps1' `
        -File |
    Select-Object -ExpandProperty BaseName

)
