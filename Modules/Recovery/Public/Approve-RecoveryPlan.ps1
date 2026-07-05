function Approve-RecoveryPlan {

<#
.SYNOPSIS
Approves recovery actions within a Recovery Plan.

.DESCRIPTION
Marks selected recovery actions as approved.

Only approved actions may be executed by the Recovery Engine.

No changes are made to either the music library or the
provider database.

.PARAMETER Plan
A DJLM.RecoveryPlan object.

.PARAMETER Type
The type of recovery action to approve.

.PARAMETER All
Approve every action within the plan.

.EXAMPLE
$plan = Approve-RecoveryPlan `
    -Plan $plan `
    -Type RepairMovedFile

.EXAMPLE
$plan = Approve-RecoveryPlan `
    -Plan $plan `
    -All

.NOTES
DJ Library Manager
#>

    [CmdletBinding(DefaultParameterSetName = 'Type')]
    param(

        [Parameter(Mandatory)]
        $Plan,

        [Parameter(
            Mandatory,
            ParameterSetName = 'Type'
        )]
        [string]
        $Type,

        [Parameter(
            Mandatory,
            ParameterSetName = 'All'
        )]
        [switch]
        $All

    )

    Write-Log "Approving recovery actions..." -Level Information

    $approved = 0

    foreach ($action in $Plan.Actions) {

        if ($All -or $action.Type -eq $Type) {

            $action.Approved = $true
            $approved++

        }

    }

    Write-Log "$approved recovery action(s) approved." -Level Success

    return $Plan

}