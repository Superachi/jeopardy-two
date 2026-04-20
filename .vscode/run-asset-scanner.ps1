# Asset Scanner PowerShell Script
# This script builds and runs the AssetScanner tool to generate path constants

# Get the directory where this script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$AssetScannerDir = Join-Path $ProjectRoot "AssetScanner"

Write-Host "Asset Scanner - Building and running..." -ForegroundColor Green

try {
    # Change to AssetScanner directory
    Push-Location $AssetScannerDir
    
    # Build the project
    Write-Host "Building AssetScanner..." -ForegroundColor Yellow
    dotnet build --configuration Release | Out-Host
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    
    # Run the tool
    Write-Host "Running AssetScanner..." -ForegroundColor Yellow
    dotnet run --configuration Release | Out-Host
    
    if ($LASTEXITCODE -ne 0) {
        throw "AssetScanner execution failed"
    }
    
    Write-Host "Asset scanning completed successfully!" -ForegroundColor Green
}
catch {
    Write-Error "Error: $_"
    exit 1
}
finally {
    # Return to original directory
    Pop-Location
}
