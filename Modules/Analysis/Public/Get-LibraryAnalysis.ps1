function Get-LibraryAnalysis {

<#
.SYNOPSIS
Performs a complete analysis of a DJ library.

.DESCRIPTION
Runs all available analysis routines and returns a single
DJLM analysis object.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [object[]]
        $Media,

        [Parameter(Mandatory)]
        [object[]]
        $Files

    )

    #
    # Duplicate Analysis
    #

    Write-Verbose "Running duplicate analysis..."

    $DuplicateTracks = Find-DuplicateTracks -Media $Media

    #
    # Missing File Analysis
    #

    Write-Verbose "Running missing file analysis..."

    $MissingFiles = Find-MissingFiles -Media $Media

    #
    # Moved File Analysis
    #

    Write-Verbose "Running moved file analysis..."

    $MovedFiles = Find-MovedFiles `
        -MissingFiles $MissingFiles `
        -Files $Files

    #
    # Build lookup of moved ORIGINAL paths
    #

    $MovedOriginalLookup = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )

    #
    # Build lookup of moved NEW paths
    #

    $MovedDestinationLookup = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )

    foreach ($Item in $MovedFiles) {

        $null = $MovedOriginalLookup.Add($Item.Original.FilePath)

        $null = $MovedDestinationLookup.Add($Item.NewFile.FilePath)

    }

    #
    # Remaining Missing Files
    #

    $RemainingMissingFiles = foreach ($Item in $MissingFiles) {

        if (-not $MovedOriginalLookup.Contains($Item.FilePath)) {

            $Item

        }

    }

    #
    # Orphan Analysis
    #

    Write-Verbose "Running orphan file analysis..."

    $OrphanFiles = Find-Orphans `
        -Media $Media `
        -Files $Files

    #
    # Remove files already identified as moved
    #

    $RemainingOrphanFiles = foreach ($Item in $OrphanFiles) {

        if (-not $MovedDestinationLookup.Contains($Item.FilePath)) {

            $Item

        }

    }

    #
    # Return Analysis Object
    #

    [PSCustomObject]@{

        PSTypeName = 'DJLM.LibraryAnalysis'

        AnalysisDate = Get-Date

        TotalMedia = $Media.Count

        TotalFiles = $Files.Count

        DuplicateTracks = $DuplicateTracks

        MovedFiles = $MovedFiles

        MissingFiles = $RemainingMissingFiles

        OrphanFiles = $RemainingOrphanFiles

    }

}