# ==============================================================================
# CONFIGURATION
# ==============================================================================
$oldSlnName = "BridgeCareCore.sln"
$newSlnName = "AssetFoxCore.sln"
$rootPath = Get-Location

# SAFETY CHECK: Git Status must be clean (Ignoring this script itself)
if (-not (Get-Command "git" -ErrorAction SilentlyContinue)) { Write-Error "Git required."; exit }
$gitStatus = git status --porcelain | Where-Object { $_ -notmatch "RenameSolution.ps1" }

if ($gitStatus) {
    Write-Error "CRITICAL: You have uncommitted changes (besides this script). Please commit or stash them before running."
    Write-Host "Uncommitted files:" -ForegroundColor Red
    $gitStatus | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    exit
}

# ==============================================================================
# 1. DEFINE NAMING RULES (Specific -> General)
# ==============================================================================
function Get-NewName ($oldName) {
    # 1. BridgeCareCore -> AssetFoxCore
    if ($oldName -eq "BridgeCareCore") { return "AssetFoxCore" }

    # 2. AppliedResearchAssociates.iAMCore -> AssetFox.AFCore (Specific Request)
    if ($oldName -eq "AppliedResearchAssociates.iAMCore") { return "AssetFox.AFCore" }

    # 3. AppliedResearchAssociates (Root Project) -> AssetFox
    if ($oldName -eq "AppliedResearchAssociates") { return "AssetFox" }

    # 4. AppliedResearchAssociates.iAM.* -> AssetFox.Core.*
    if ($oldName.StartsWith("AppliedResearchAssociates.iAM.")) { 
        return $oldName.Replace("AppliedResearchAssociates.iAM.", "AssetFox.Core.") 
    }

    # 5. AppliedResearchAssociates.* -> AssetFox.*
    if ($oldName.StartsWith("AppliedResearchAssociates.")) { 
        return $oldName.Replace("AppliedResearchAssociates.", "AssetFox.") 
    }

    return $oldName
}

# ==============================================================================
# 2. SCAN DISK & PLAN MOVES
# ==============================================================================
Write-Host "PHASE 1: Scanning Disk for Folders..." -ForegroundColor Cyan

# Get ALL top-level folders that match our targets
$folders = Get-ChildItem -Path $rootPath -Directory | Where-Object { 
    $_.Name.StartsWith("AppliedResearchAssociates") -or $_.Name.StartsWith("BridgeCareCore")
}

$changes = @()

foreach ($folder in $folders) {
    $oldName = $folder.Name
    $newName = Get-NewName -oldName $oldName

    if ($oldName -ne $newName) {
        $changes += [PSCustomObject]@{
            OldName = $oldName
            NewName = $newName
            OldFolderPath = $folder.FullName
            NewFolderPath = Join-Path $rootPath $newName
            OldProjFileName = "$oldName.csproj"
            NewProjFileName = "$newName.csproj"
        }
    }
}

# ==============================================================================
# 3. EXECUTE MOVES (History Preservation)
# ==============================================================================
Write-Host "PHASE 2: Moving Files via Git..." -ForegroundColor Yellow

foreach ($change in $changes) {
    # 1. Rename the .csproj file INSIDE the old folder first
    # This keeps the history attached to the file before we move the parent folder
    $oldProjFile = Join-Path $change.OldFolderPath $change.OldProjFileName
    $tempProjFile = Join-Path $change.OldFolderPath $change.NewProjFileName
    
    if (Test-Path $oldProjFile) {
        Write-Host "  Git MV File: $($change.OldProjFileName) -> $($change.NewProjFileName)"
        git mv "$oldProjFile" "$tempProjFile"
    }

    # 2. Rename the Folder itself
    if (Test-Path $change.OldFolderPath) {
        Write-Host "  Git MV Folder: $($change.OldName) -> $($change.NewName)"
        git mv "$($change.OldFolderPath)" "$($change.NewFolderPath)"
    }
}

# 3. Rename Solution File
if (Test-Path $oldSlnName) {
    Write-Host "  Git MV Solution: $oldSlnName -> $newSlnName"
    git mv "$oldSlnName" "$newSlnName"
}

# ==============================================================================
# 4. UPDATE SOLUTION CONTENT
# ==============================================================================
# We must update the SLN text now so the intermediate commit is valid
if (Test-Path $newSlnName) {
    $slnContent = Get-Content $newSlnName
    $newSlnContent = $slnContent
    foreach ($change in $changes) {
        for ($i=0; $i -lt $newSlnContent.Count; $i++) {
            # Update Project Name in SLN
            if ($newSlnContent[$i] -like "*= `"$($change.OldName)`",*") {
                $newSlnContent[$i] = $newSlnContent[$i].Replace("= `"$($change.OldName)`"", "= `"$($change.NewName)`"")
            }
            # Update Project Path in SLN
            # Note: We replace the whole relative path string
            $oldRelPath = "$($change.OldName)\$($change.OldProjFileName)"
            $newRelPath = "$($change.NewName)\$($change.NewProjFileName)"
            if ($newSlnContent[$i] -like "*$oldRelPath*") {
                $newSlnContent[$i] = $newSlnContent[$i].Replace($oldRelPath, $newRelPath)
            }
        }
    }
    $newSlnContent | Set-Content $newSlnName
}

# ==============================================================================
# 5. INTERMEDIATE COMMIT
# ==============================================================================
Write-Host "PHASE 3: Committing Moves (Locks in Git Blame)..." -ForegroundColor Cyan
git add .
git commit -m "Refactor: Rename Projects and Folders (Automated)"
Write-Host "  > Moves committed. History saved." -ForegroundColor Green

# ==============================================================================
# 6. UPDATE CODE CONTENT (References & Namespaces)
# ==============================================================================
Write-Host "PHASE 4: Updating Code and References..." -ForegroundColor Magenta

$allFiles = Get-ChildItem -Path $rootPath -Include *.cs, *.xaml, *.csproj, *.xml -Recurse | Where-Object { $_.FullName -notmatch "\\(bin|obj|\.git)\\" }

foreach ($file in $allFiles) {
    $content = Get-Content $file.FullName
    $newContent = $content
    $isModified = $false

    # --- A. Fix .csproj ProjectReferences ---
    if ($file.Extension -eq ".csproj") {
        foreach ($change in $changes) {
            # Update Include paths
            $oldRefFolder = "$($change.OldName)\"
            $newRefFolder = "$($change.NewName)\"
            
            # Replace Folder in Path
            if ($newContent -match [regex]::Escape($oldRefFolder)) {
                $newContent = $newContent -replace [regex]::Escape($oldRefFolder), $newRefFolder
                $isModified = $true
            }
            # Replace Filename in Path
            if ($newContent -match [regex]::Escape($change.OldProjFileName)) {
                $newContent = $newContent -replace [regex]::Escape($change.OldProjFileName), $change.NewProjFileName
                $isModified = $true
            }
        }
    }

    # --- B. Fix Namespaces (Strict Order) ---
    
    # 1. BridgeCareCore -> AssetFoxCore
    if ($newContent -match "BridgeCareCore") { 
        $newContent = $newContent -replace "BridgeCareCore", "AssetFoxCore"
        $isModified = $true 
    }

    # 2. AppliedResearchAssociates.iAMCore -> AssetFox.AFCore (Specific)
    if ($newContent -match "AppliedResearchAssociates\.iAMCore") { 
        $newContent = $newContent -replace "AppliedResearchAssociates\.iAMCore", "AssetFox.AFCore"
        $isModified = $true 
    }

    # 3. AppliedResearchAssociates.iAM.* -> AssetFox.Core.*
    # Note: We replace the prefix "AppliedResearchAssociates.iAM."
    if ($newContent -match "AppliedResearchAssociates\.iAM\.") { 
        $newContent = $newContent -replace "AppliedResearchAssociates\.iAM\.", "AssetFox.Core."
        $isModified = $true 
    }

    # 4. AppliedResearchAssociates -> AssetFox (Catch-all for root and general dot)
    # We carefully handle the dot to avoid replacing parts of other strings if any exist
    if ($newContent -match "AppliedResearchAssociates\.") { 
        $newContent = $newContent -replace "AppliedResearchAssociates\.", "AssetFox."
        $isModified = $true 
    }
    if ($newContent -match "AppliedResearchAssociates") { 
        $newContent = $newContent -replace "AppliedResearchAssociates", "AssetFox"
        $isModified = $true 
    }

    if ($isModified) {
        $newContent | Set-Content $file.FullName
    }
}

Write-Host "---------------------------------------------------------"
Write-Host "COMPLETE." -ForegroundColor Green
Write-Host "1. Folders and Files renamed & committed (History Preserved)."
Write-Host "2. Solution file updated."
Write-Host "3. Code namespaces updated (Staged as pending changes)."
Write-Host "4. PLEASE VERIFY, then run: git add . && git commit -m 'Refactor: Update namespaces'"
Write-Host "---------------------------------------------------------"