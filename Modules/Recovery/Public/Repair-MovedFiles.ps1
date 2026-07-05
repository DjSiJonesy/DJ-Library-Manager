function Repair-MovedFiles {

<#
.SYNOPSIS
Previews moved file repairs.

.DESCRIPTION
Displays the approved RepairMovedFile recovery actions.

No modifications are made to either the provider database or
the filesystem.

This command exists to allow users to review proposed repairs
before execution support is introduced.

.PARAMETER Plan
A DJLM.RecoveryPlan object.

.EXAMPLE
Repair-MovedFiles -Plan $plan

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Plan

    )

    Write-Log "Previewing moved file repairs..." -Level Information

    $actions = $Plan.Actions |
        Where-Object {

            $_.Type -eq 'RepairMovedFile' -and
            $_.Approved -and
            -not $_.Executed

        }

    if (-not $actions) {

        Write-Host
        Write-Host "No approved moved file repairs."
        Write-Host

        return

    }

    Write-Section "Moved File Repair Preview"

    foreach ($action in $actions) {

        Write-Host "Confidence : $($action.Confidence)%"
        Write-Host "From       : $($action.Source)"
        Write-Host "To         : $($action.Target)"
        Write-Host

    }

    Write-Host ("Total Repairs : {0}" -f $actions.Count)
    Write-Host

    Write-Log "Preview complete." -Level Success

}