function Invoke-RecoveryPlan {

<#
.SYNOPSIS
Executes an approved Recovery Plan.

.DESCRIPTION
Coordinates execution of all approved recovery actions.

Individual repair operations are delegated to the appropriate
Recovery functions.

.PARAMETER Plan
A DJLM.RecoveryPlan object.

.PARAMETER Database
A provider database object.

.PARAMETER Commit
When specified, approved changes are written back to the
provider database.

Without -Commit, only preview operations are performed.

.EXAMPLE
Invoke-RecoveryPlan `
    -Plan $Plan

.EXAMPLE
Invoke-RecoveryPlan `
    -Plan $Plan `
    -Database $Database

.EXAMPLE
Invoke-RecoveryPlan `
    -Plan $Plan `
    -Database $Database `
    -Commit

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Plan,

        $Database,

        [switch]
        $Commit

    )

    Write-Log "Executing recovery plan..." -Level Information

    #
    # Preview only
    #

    if (-not $Database) {

        Write-Log "Running in preview mode." -Level Information

        Repair-MovedFiles `
            -Plan $Plan

        Write-Log "Recovery preview complete." -Level Success

        return

    }

    #
    # Update provider database in memory
    #

    Repair-MovedFiles `
        -Plan $Plan `
        -Database $Database

    #
    # Save if requested
    #

    if ($Commit) {

        Save-VirtualDJDatabase `
            -Database $Database `
            -Backup

        Write-Log "Recovery plan committed." -Level Success

    }
    else {

        Write-Log "Recovery plan applied in memory only." -Level Information

    }

}