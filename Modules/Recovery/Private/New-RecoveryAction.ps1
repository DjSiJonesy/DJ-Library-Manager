function New-RecoveryAction {

<#
.SYNOPSIS
Creates a standard DJLM Recovery Action object.

.DESCRIPTION
Constructs a provider-independent recovery action used by the
Recovery Engine.

This helper ensures every recovery action has a consistent
structure regardless of the analysis type that created it.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]
        $Type,

        [Parameter(Mandatory)]
        [ValidateRange(0,100)]
        [int]
        $Confidence,

        [string]
        $Source,

        [string]
        $Target,

        [Parameter(Mandatory)]
        $Reference,

        [Parameter(Mandatory)]
        [string]
        $Reason

    )

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.RecoveryAction'

        Type = $Type

        Status = 'Pending'

        Confidence = $Confidence

        Source = $Source

        Target = $Target

        Reference = $Reference

        Reason = $Reason

        Created = Get-Date

        Approved = $false

        Executed = $false

        ExecutedDate = $null

        UndoSupported = $false

    }

}