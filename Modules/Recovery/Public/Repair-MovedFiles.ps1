function Repair-MovedFiles {

<#
.SYNOPSIS
Processes approved moved file recovery actions.

.DESCRIPTION
Processes approved RepairMovedFile recovery actions.

When no provider database is supplied, a preview of the
planned repairs is displayed.

When a provider database is supplied, the provider database
is updated in memory only.

Saving the updated database is the responsibility of the
caller.

.PARAMETER Plan
A DJLM.RecoveryPlan object.

.PARAMETER Database
A provider database object returned by the provider module.

.EXAMPLE
Repair-MovedFiles -Plan $Plan

.EXAMPLE
Repair-MovedFiles `
    -Plan $Plan `
    -Database $Database

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Plan,

        $Database

    )

    $Actions = $Plan.Actions | Where-Object {

        $_.Type -eq 'RepairMovedFile' -and
        $_.Approved -and
        -not $_.Executed

    }

    if (-not $Actions) {

        Write-Host
        Write-Host "No approved moved file repairs."
        Write-Host

        return

    }

    #
    # Preview Mode
    #

    if (-not $Database) {

        Write-Log "Previewing moved file repairs..." -Level Information

        Write-Section "Moved File Repair Preview"

        foreach ($Action in $Actions) {

            Write-Host ("Confidence : {0}%" -f $Action.Confidence)
            Write-Host ("From       : {0}" -f $Action.Source)
            Write-Host ("To         : {0}" -f $Action.Target)
            Write-Host

        }

        Write-Host ("Total Repairs : {0}" -f $Actions.Count)
        Write-Host

        Write-Log "Preview complete." -Level Success

        return

    }

    #
    # Update Provider Database
    #

    Write-Log "Applying moved file repairs..." -Level Information

    $Updated = 0
    $Failed  = 0

    foreach ($Action in $Actions) {

        $Result = Update-VirtualDJMediaPaths `
            -Database $Database `
            -OldPath $Action.Source `
            -NewPath $Action.Target

        if ($Result.Success) {

            $Action.Executed = $true
            $Action.ExecutedDate = Get-Date

            $Updated++

        }
        else {

            $Failed++

        }

    }

    Write-Log "$Updated moved file(s) updated." -Level Success

    if ($Failed -gt 0) {

        Write-Log "$Failed moved file(s) could not be updated." -Level Warning

    }

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.RepairResult'

        Updated = $Updated

        Failed = $Failed

        Total = $Actions.Count

    }

}