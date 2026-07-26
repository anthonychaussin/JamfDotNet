<#
.SYNOPSIS
  Regenerates the Jamf Classic Kiota client from openapi/jamf-classic.swagger.yaml.
#>
[CmdletBinding()]
param(
    [string]$SchemaPath = "",
    [string]$OutputPath = "",
    [string]$NamespaceName = "JamfDotNet.Classic.Generated",
    [string]$ClassName = "JamfClassicApiClient"
)

$ErrorActionPreference = "Stop"

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
if (-not $SchemaPath) {
    $SchemaPath = Join-Path $scriptDir "..\openapi\jamf-classic.swagger.yaml"
}
if (-not $OutputPath) {
    $OutputPath = Join-Path $scriptDir "..\src\JamfDotNet.Classic\Generated"
}
$SchemaPath = [System.IO.Path]::GetFullPath($SchemaPath)
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

if (-not (Test-Path $SchemaPath)) {
    throw "Schema not found at $SchemaPath. Run Fetch-JamfClassicSchema.ps1 first."
}

Push-Location $repoRoot
try {
    Write-Host "Restoring local tools..."
    dotnet tool restore | Out-Host

    if (Test-Path $OutputPath) {
        Write-Host "Cleaning $OutputPath ..."
        Remove-Item -Recurse -Force $OutputPath
    }
    New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

    Write-Host "Generating Kiota Classic client..."
    dotnet tool run kiota generate `
        --language CSharp `
        --openapi $SchemaPath `
        --output $OutputPath `
        --namespace-name $NamespaceName `
        --class-name $ClassName `
        --exclude-backward-compatible `
        --clean-output `
        --additional-data false

    Write-Host "Generation complete: $OutputPath"
}
finally {
    Pop-Location
}
