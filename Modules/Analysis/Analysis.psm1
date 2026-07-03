# ============================================================================
# Module: Analysis
# Version: 1.0.0
#
# DJ Library Manager
# ============================================================================

Set-StrictMode -Version Latest

# Module root

$ModuleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

#
# Load Private Functions
#

$PrivateFolder = Join-Path $ModuleRoot 'Private'

if (Test-Path $PrivateFolder) {

    Get-ChildItem $PrivateFolder -Filter '*.ps1' -File |
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

    Get-ChildItem $PublicFolder -Filter '*.ps1' -File |
        Sort-Object Name |
        ForEach-Object {

            . $_.FullName

        }

}

#
# Export Public Functions
#

Export-ModuleMember -Function (

    Get-ChildItem $PublicFolder -Filter '*.ps1' -File |
        Select-Object -ExpandProperty BaseName

)