Write-Host "=== Checking for Package Updates ===" -ForegroundColor Green
$outdated = dotnet list package --outdated
$outdated | ForEach-Object { Write-Host $_ }

# Check if there are any updates available before parsing
$updateLines = $outdated | Where-Object { $_ -match '^\s*>\s*' }

if (-not $updateLines) {
    Write-Host "`n=== No packages to update ===" -ForegroundColor Green
    return
}

Write-Host "`n=== Parsing Package Names ===" -ForegroundColor Yellow
$packages = @()

# Process only the lines we know have updates (optimization)
foreach ($line in $updateLines) {
    $parts = $line.Trim() -split '\s+', 5
    if ($parts.Length -ge 2) {
        $packageName = $parts[1]
        $requested = if ($parts.Length -ge 3) { $parts[2] } else { "Unknown" }
        $resolved = if ($parts.Length -ge 4) { $parts[3] } else { "Unknown" }
        $latest = if ($parts.Length -ge 5) { $parts[4] } else { "Unknown" }
        
        # Skip packages not available in sources
        if ($latest -match "Not found|^Not$|^found$") {
            Write-Host "Skipping $packageName - not available in configured sources" -ForegroundColor Gray
            continue
        }
        
        Write-Host "Found outdated package: $packageName ($resolved -> $latest)" -ForegroundColor Magenta
        $packages += $packageName
    }
}

if ($packages.Count -eq 0) {
    Write-Host "`nAll packages found are not available in configured sources" -ForegroundColor Yellow
    return
}

$packages = $packages | Sort-Object -Unique
Write-Host "`nPackages to update: $($packages -join ', ')" -ForegroundColor Yellow

Write-Host "`n=== Updating Packages ===" -ForegroundColor Yellow
$successCount = 0
$skipCount = 0
$failCount = 0

foreach ($pkg in $packages) {
    Write-Host "Checking $pkg..." -ForegroundColor Cyan
    
    $updateOutput = dotnet add package $pkg 2>&1
    $outputString = $updateOutput -join "`n"
    
    if ($outputString -match "version '[\d\.]+' updated in file" -or $outputString -match "Adding PackageReference") {
        if ($outputString -match "Assets file has not changed|Skipping assets file writing") {
            Write-Host "$pkg is already up to date" -ForegroundColor Yellow
            $skipCount++
        } else {
            Write-Host "$pkg updated successfully" -ForegroundColor Green  
            $successCount++
        }
    } elseif ($outputString -match "error|failed" -and $outputString -notmatch "NotFound https://api\.nuget\.org") {
        Write-Host "$pkg failed to update" -ForegroundColor Red
        $failCount++
    } else {
        Write-Host "$pkg processed" -ForegroundColor Green
        $successCount++
    }
}

# Summary
Write-Host "`n=== Update Complete ===" -ForegroundColor Green
if ($successCount -gt 0) { Write-Host "Updated: $successCount packages" -ForegroundColor Green }
if ($skipCount -gt 0) { Write-Host "Already current: $skipCount packages" -ForegroundColor Yellow }
if ($failCount -gt 0) { Write-Host "Failed: $failCount packages" -ForegroundColor Red }
