@{

# ============================================================================
# Module Manifest
# ============================================================================
RootModule        = 'EngineDJ.psm1'
ModuleVersion     = '1.0.0'
GUID              = '76802a6e-5e95-4bbb-a70f-63ea78892222'

Author            = 'Simon Jones'
CompanyName       = 'SM Sounds'
Copyright         = '(c) Simon Jones. All rights reserved.'

Description       = 'EngineDJ module for DJ Library Manager.'

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
