@{

# ============================================================================
# Module Manifest
# ============================================================================
RootModule        = 'Serato.psm1'
ModuleVersion     = '1.0.0'
GUID              = 'a32de983-ac79-4dc5-9805-89a53f722e33'

Author            = 'Simon Jones'
CompanyName       = 'SM Sounds'
Copyright         = '(c) Simon Jones. All rights reserved.'

Description       = 'Serato module for DJ Library Manager.'

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
