# Build CitySO as self-contained executable
# This script creates a standalone .exe file that includes .NET runtime

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath = ".\publish"
)

Write-Host "Building CitySO as self-contained executable..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Runtime: $Runtime" -ForegroundColor Gray
Write-Host "Output: $OutputPath" -ForegroundColor Gray
Write-Host ""

# Get the project directory
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = "CitySO\CitySO.csproj"

if (-not (Test-Path "$projectPath\$projectFile")) {
    Write-Host "Error: $projectFile not found in $projectPath" -ForegroundColor Red
    exit 1
}

# Clean previous build
if (Test-Path $OutputPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Build and publish as self-contained executable
Write-Host "Publishing application..." -ForegroundColor Cyan
& dotnet publish "$projectPath\$projectFile" `
    -c $Configuration `
    --self-contained `
    -r $Runtime `
    -o $OutputPath `
    --p:PublishSingleFile=true `
    --p:IncludeNativeLibrariesForSelfExtract=true `
    --p:PublishTrimmed=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Find the executable
$exePath = Get-ChildItem -Path $OutputPath -Name "*.exe" | Select-Object -First 1

if ($exePath) {
    Write-Host "" -ForegroundColor Gray
    Write-Host "✓ Build completed successfully!" -ForegroundColor Green
    Write-Host "Executable: $OutputPath\$exePath" -ForegroundColor Green
    Write-Host "" -ForegroundColor Gray
    Write-Host "The .exe file is ready to use on any Windows system with no dependencies." -ForegroundColor Gray
    Write-Host "File includes .NET runtime and all required libraries." -ForegroundColor Gray
} else {
    Write-Host "Error: Executable not found in output directory" -ForegroundColor Red
    exit 1
}
