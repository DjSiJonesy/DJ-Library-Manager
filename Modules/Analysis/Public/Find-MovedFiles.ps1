function Find-MovedFiles {

<#
.SYNOPSIS
Finds media files that have been moved.

.DESCRIPTION
Matches missing library entries against files found on disk
using filename, extension and filesize to identify likely
relocated media.

Returns the single best match for each missing media item.

.NOTES
DJ Library Manager
#>

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [object[]]
        $MissingFiles,

        [Parameter(Mandatory)]
        [object[]]
        $Files

    )

    #
    # Build filename lookup
    #

    $Lookup = @{}

    foreach ($File in $Files) {

        $Key = $File.FileName.ToUpperInvariant()

        if (-not $Lookup.ContainsKey($Key)) {

            $Lookup[$Key] = New-Object System.Collections.Generic.List[object]

        }

        $Lookup[$Key].Add($File)

    }

    #
    # Find best relocation for each missing file
    #

    $MovedFiles = New-Object System.Collections.Generic.List[object]

    foreach ($Track in $MissingFiles) {

        $Key = [IO.Path]::GetFileName($Track.FilePath).ToUpperInvariant()

        if (-not $Lookup.ContainsKey($Key)) {
            continue
        }

        $BestCandidate = $null
        $BestScore = -1

        foreach ($Candidate in $Lookup[$Key]) {

            $Score = 0

            #
            # Filename
            #

            if ($Candidate.FileName -ieq
                [IO.Path]::GetFileName($Track.FilePath)) {

                $Score += 50

            }

            #
            # Extension
            #

            if ($Candidate.Extension -ieq
                [IO.Path]::GetExtension($Track.FilePath)) {

                $Score += 10

            }

            #
            # File size
            #

            if ($Candidate.FileSize -eq $Track.FileSize) {

                $Score += 40

            }

            if ($Score -gt $BestScore) {

                $BestScore = $Score
                $BestCandidate = $Candidate

            }

        }

        if ($BestCandidate -and $BestScore -ge 90) {

            $MovedFiles.Add(

                [PSCustomObject]@{

                    Original = $Track

                    NewFile  = $BestCandidate

                    Score    = $BestScore

                }

            )

        }

    }

    return $MovedFiles

}