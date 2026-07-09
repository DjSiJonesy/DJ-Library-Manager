function Save-Database {

<#
.SYNOPSIS
Saves a provider database.

.DESCRIPTION
Dispatches the save operation to the appropriate provider
based on the supplied database object.

.PARAMETER Database
A provider database object.

.PARAMETER Backup
Creates a provider-specific backup before saving.

.NOTES
DJ Library Manager
#>

    [CmdletBinding(SupportsShouldProcess)]
    param(

        [Parameter(Mandatory)]
        $Database,

        [switch]
        $Backup

    )

    if ($null -eq $Database) {

        throw "A provider database must be supplied."

    }

    $databaseType = $Database.PSTypeNames[0]

    if (-not $PSCmdlet.ShouldProcess($databaseType, "Save database")) {

        return

    }

    switch ($databaseType) {

        'DJLM.VirtualDJDatabase' {

            return Save-VirtualDJDatabase `
                -Database $Database `
                -Backup:$Backup

        }

        'DJLM.RekordboxDatabase' {

            return Save-RekordboxDatabase `
                -Database $Database `
                -Backup:$Backup

        }

        default {

            throw (
                "Saving is not supported for database type '{0}'." -f
                $databaseType
            )

        }

    }

}