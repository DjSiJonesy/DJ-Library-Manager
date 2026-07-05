function Show-RecoveryPlan {

<#
.SYNOPSIS
Displays a summary of a DJLM Recovery Plan.

.DESCRIPTION
Shows a high-level summary of the recovery actions generated
by Get-RecoveryPlan.

This function is read-only and does not modify the library.

.PARAMETER Plan
A DJLM.RecoveryPlan object.

.EXAMPLE
Show-RecoveryPlan -Plan $plan

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Plan

    )

    Write-Host
    Write-Host "Recovery Plan"
    Write-Host ("─" * 13)
    Write-Host

    $summary = $Plan.Actions |
        Group-Object Type |
        Sort-Object Name

    foreach ($group in $summary) {

        $label = switch ($group.Name) {

            'RepairMovedFile'   { 'Repair Moved Files' }

            'ImportOrphanFile'  { 'Import Orphan Files' }

            'ReviewMissingFile' { 'Review Missing Files' }

            'ReviewDuplicate'   { 'Review Duplicate Tracks' }

            default             { $group.Name }

        }

        "{0,-30} {1,8}" -f $label, $group.Count |
            Write-Host

    }

    Write-Host

    Write-Host ("{0,-30} {1,8}" -f ('─' * 22), "")

    Write-Host ("{0,-30} {1,8}" -f "Total Actions", $Plan.ActionCount)

    Write-Host ("{0,-30} {1,8}" -f "Approval Required", $(if ($Plan.RequiresApproval) { "Yes" } else { "No" }))

    Write-Host ("{0,-30} {1,8}" -f "Undo Available", $(if ($Plan.CanUndo) { "Yes" } else { "No" }))

    Write-Host
}