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
    # Build provider update collection
    #

    $MovedFiles = foreach ($Action in $Actions) {

        $Action.Reference

    }

    Write-Log "Applying moved file repairs..." -Level Information

    try {

        $Updated = Update-MediaPaths `
            -Database $Database `
            -MovedFiles $MovedFiles

    }
    catch {

        Write-Log $_.Exception.Message -Level Error

        return [PSCustomObject]@{

            PSTypeName = 'DJLM.RepairResult'

            Updated = 0

            Failed = $Actions.Count

            Total = $Actions.Count

        }

    }

    #
    # Only mark actions as executed if the provider
    # reports the expected number of updates.
    #

    if ($Updated -eq $Actions.Count) {

        foreach ($Action in $Actions) {

            $Action.Executed = $true
            $Action.ExecutedDate = Get-Date

        }

        $Failed = 0

    }
    else {

        $Failed = $Actions.Count - $Updated

        Write-Log (
            "Expected to update $($Actions.Count) item(s) but provider updated $Updated."
        ) -Level Warning

    }

    Write-Log "$Updated moved file(s) updated." -Level Success

    if ($Failed -gt 0) {

        Write-Log "$Failed moved file(s) failed to update." -Level Warning

    }

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.RepairResult'

        Updated = $Updated

        Failed = $Failed

        Total = $Actions.Count

    }

}