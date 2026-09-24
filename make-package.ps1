<#
make-package.ps1

Builds the mod projects, collects package files and assets, and creates zip packages for both targets.

Usage: run this script from PowerShell. By default the zips are created in the repo root.
Parameters:
  -OutputDir <path>   Directory where the final zips will be written (default: script folder)
  -Configuration <cfg> Build configuration (default: Release)
#>

param(
    [string]$OutputDir = $PSScriptRoot,
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
Write-Host "Repository root: $root"

# 1) Build the solution
Push-Location ($root)
Write-Host "Building solution using configuration: $Configuration"
dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    throw "dotnet build failed with exit code $LASTEXITCODE"
}
Pop-Location

# 2) Define packaging targets
$targets = @(
    @{
        Name          = "Thorn_Core"
        ProjectDir    = "ThornClient"
        PackageDir    = "package"
        AssemblyName  = "ThornClient.dll"
        DocName       = "ThornClient.xml"
        IncludeAssets = $true
    },
    @{
        Name          = "Thorn"
        ProjectDir    = "ThornClientModules"
        PackageDir    = "package_modules"
        AssemblyName  = "ThornClientModules.dll"
        DocName       = "ThornClientModules.xml"
        IncludeAssets = $false
    }
)

$createdZips = @()

foreach ($target in $targets) {
    Write-Host "`n--- Packaging $($target.Name) ---"
    $staging = Join-Path $root "package_build/$($target.Name)"
    $pluginDir = Join-Path $staging "plugins/$($target.Name)"

    # Prepare staging directory
    if (Test-Path $staging) {
        Write-Host "Removing existing staging folder: $staging"
        Remove-Item $staging -Recurse -Force
    }
    New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null

    # Copy binary and xml documentation if present
    $assemblyPath = Join-Path $root "$($target.ProjectDir)/bin/$Configuration/netstandard2.1/$($target.AssemblyName)"
    $docPath = Join-Path $root "$($target.ProjectDir)/bin/$Configuration/netstandard2.1/$($target.DocName)"

    if (Test-Path $assemblyPath) {
        Write-Host "Copying binary: $assemblyPath"
        Copy-Item -Path $assemblyPath -Destination $pluginDir -Force
    } else {
        throw "Assembly not found at $assemblyPath"
    }

    if (Test-Path $docPath) {
        Write-Host "Copying documentation: $docPath"
        Copy-Item -Path $docPath -Destination $pluginDir -Force
    }

    # Copy all files from package folder into staging
    $packageFolder = Join-Path $root $target.PackageDir
    if (-not (Test-Path $packageFolder)) { throw "Package folder not found at $packageFolder" }
    Write-Host "Copying package files from '$packageFolder' to staging"
    Copy-Item -Path (Join-Path $packageFolder '*') -Destination $staging -Recurse -Force

    # Try to read name/version from manifest.json for zip naming
    $manifestPath = Join-Path $packageFolder 'manifest.json'
    $pkgName = $target.Name
    $pkgVer = (Get-Date -Format yyyyMMddHHmmss)
    if (Test-Path $manifestPath) {
        try {
            $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
            if ($manifest.name) { $pkgName = $manifest.name }
            if ($manifest.version_number) { $pkgVer = $manifest.version_number }
        } catch {
            Write-Warning "Could not parse manifest.json for name/version in '$packageFolder'. Falling back to default name/timestamp."
        }
    }

    # Copy assets if requested
    if ($target.IncludeAssets) {
        $assetsSrc = Join-Path $root 'assets'
        $assetsDest = Join-Path $pluginDir 'assets'
        Write-Host "Creating assets destination: $assetsDest"
        New-Item -ItemType Directory -Path $assetsDest -Force | Out-Null
        if (Test-Path $assetsSrc) {
            Write-Host "Copying assets from '$assetsSrc' to '$assetsDest'"
            Copy-Item -Path (Join-Path $assetsSrc '*') -Destination $assetsDest -Recurse -Force
        } else {
            Write-Warning "Assets folder not found at: $assetsSrc"
        }
    }

    # Copy main icon to plugin dir
    $pkgIcon = Join-Path $staging 'icon.png'
    if (Test-Path $pkgIcon) {
        Copy-Item -Path $pkgIcon -Destination (Join-Path $pluginDir 'icon.png') -Force
    }

    # Create zip package
    $zipName = "$pkgName-$pkgVer.zip"
    $zipPath = Join-Path $OutputDir $zipName
    if (Test-Path $zipPath) {
        Write-Host "Removing existing zip: $zipPath"
        Remove-Item $zipPath -Force
    }
    Write-Host "Creating zip: $zipPath"

    # Compress everything inside the staging folder so package root contains package files and plugins/...
    Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath -Force

    Write-Host "Package created at: $zipPath"
    Write-Host "Staging folder retained at: $staging (remove if not needed)"

    $createdZips += $zipPath
}

# Return paths for scripts / automation
Write-Output $createdZips
