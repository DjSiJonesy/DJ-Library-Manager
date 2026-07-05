function Get-RecoveryPlan {

<#
.SYNOPSIS
Generates a recovery plan from a library analysis.

.DESCRIPTION
Converts the results of a DJLM Library Analysis into a
provider-independent recovery plan.

No modifications are made to either the music library or
the provider database.

The returned Recovery Plan can be previewed, approved and
executed by later Recovery Engine components.

.PARAMETER Analysis
A DJLM.LibraryAnalysis object produced by
Get-LibraryAnalysis.

.EXAMPLE
$plan = Get-RecoveryPlan -Analysis $analysis

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        $Analysis

    )

    Write-Log "Generating recovery plan..." -Level Information

    $Actions = @()

    #
    # Moved Files
    #

    foreach ($Item in $Analysis.MovedFiles) {

        $Actions += New-RecoveryAction `
            -Type 'RepairMovedFile' `
            -Confidence $Item.Score `
            -Source $Item.Original.FilePath `
            -Target $Item.NewFile.FilePath `
            -Reference $Item `
            -Reason 'Matching media found at a new location.'

    }

    #
    # Missing Files
    #

    foreach ($Item in $Analysis.MissingFiles) {

        $Actions += New-RecoveryAction `
            -Type 'ReviewMissingFile' `
            -Confidence 0 `
            -Source $Item.FilePath `
            -Target $null `
            -Reference $Item `
            -Reason 'No matching file could be located.'

    }

    #
    # Orphan Files
    #

    foreach ($Item in $Analysis.OrphanFiles) {

        $Actions += New-RecoveryAction `
            -Type 'ImportOrphanFile' `
            -Confidence 100 `
            -Source $null `
            -Target $Item.FilePath `
            -Reference $Item `
            -Reason 'File exists on disk but is not referenced by the provider.'

    }

    #
    # Duplicate Tracks
    #

    foreach ($Item in $Analysis.DuplicateTracks) {

        $Actions += New-RecoveryAction `
            -Type 'ReviewDuplicate' `
            -Confidence $Item.Score `
            -Source $Item.Reference.FilePath `
            -Target $Item.Candidate.FilePath `
            -Reference $Item `
            -Reason 'Possible duplicate detected.'

    }

    Write-Log "Recovery plan generated." -Level Success

    return [PSCustomObject]@{

        PSTypeName = 'DJLM.RecoveryPlan'

        PlanDate = Get-Date

        ActionCount = $Actions.Count

        RequiresApproval = $true

        CanUndo = $false

        Actions = $Actions

    }

}