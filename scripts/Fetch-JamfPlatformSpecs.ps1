<#
.SYNOPSIS
  Downloads Jamf Platform OpenAPI specs into openapi/platform/.
#>
[CmdletBinding()]
param(
    [string]$SourceBaseUrl = "https://raw.githubusercontent.com/Jamf-Concepts/jamfplatform-go-sdk/main/api",
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $OutputDir) { $OutputDir = Join-Path $scriptDir "..\openapi\platform" }
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$specs = @(
    "blueprints_api.json",
    "device_inventory_api.json",
    "device_group_inventory_api.json",
    "device_management_action_api.json",
    "compliance_benchmark_engine.json",
    "declaration_reporting_service.json",
    "app_installer_deployments_api.json",
    "app_installer_global_settings_api.json",
    "app_installer_titles_api.json"
)

foreach ($spec in $specs) {
    $url = "$SourceBaseUrl/$spec"
    $out = Join-Path $OutputDir $spec
    Write-Host "Fetching $url ..."
    Invoke-WebRequest -Uri $url -OutFile $out -UseBasicParsing
    Write-Host "  -> $out ($((Get-Item $out).Length) bytes)"
}
