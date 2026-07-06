@{

# ============================================================================
# Module Manifest
# ============================================================================
RootModule        = 'Rekordbox.psm1'
ModuleVersion     = '1.0.0'
GUID              = '14eeb9a4-38d4-4e6e-801b-4c21104addb4'

Author            = 'Simon Jones'
CompanyName       = 'SM Sounds'
Copyright         = '(c) Simon Jones. All rights reserved.'

Description       = 'Rekordbox module for DJ Library Manager.'

PowerShellVersion = '7.0'

# ============================================================================
# Functions
# ============================================================================
FunctionsToExport = '*'
CmdletsToExport   = @()
VariablesToExport = @()
AliasesToExport   = @()

# ============================================================================
# Private Data
# ============================================================================
PrivateData = @{

    PSData = @{

        Tags = @(
            'DJ',
            'Library',
            'PowerShell'
        )

        ProjectUri = ''
        LicenseUri = ''
        ReleaseNotes = ''

    }

}

}
