<#
.SYNOPSIS
  Regenerates Jamf Platform Kiota clients (one client per OpenAPI spec).
#>
[CmdletBinding()]
param(
    [string]$SpecsDir = "",
    [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ".."))
if (-not $SpecsDir) { $SpecsDir = Join-Path $scriptDir "..\openapi\platform" }
if (-not $OutputRoot) { $OutputRoot = Join-Path $scriptDir "..\src\JamfDotNet.Platform\Generated" }
$SpecsDir = [System.IO.Path]::GetFullPath($SpecsDir)
$OutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)

$map = @{
    "blueprints_api.json"                    = @{ Folder = "Blueprints"; Class = "BlueprintsApiClient"; Namespace = "JamfDotNet.Platform.Generated.Blueprints" }
    "device_inventory_api.json"              = @{ Folder = "Devices"; Class = "DevicesApiClient"; Namespace = "JamfDotNet.Platform.Generated.Devices" }
    "device_group_inventory_api.json"        = @{ Folder = "DeviceGroups"; Class = "DeviceGroupsApiClient"; Namespace = "JamfDotNet.Platform.Generated.DeviceGroups" }
    "device_management_action_api.json"      = @{ Folder = "DeviceActions"; Class = "DeviceActionsApiClient"; Namespace = "JamfDotNet.Platform.Generated.DeviceActions" }
    "compliance_benchmark_engine.json"       = @{ Folder = "Compliance"; Class = "ComplianceApiClient"; Namespace = "JamfDotNet.Platform.Generated.Compliance" }
    "declaration_reporting_service.json"     = @{ Folder = "DeclarationReporting"; Class = "DeclarationReportingApiClient"; Namespace = "JamfDotNet.Platform.Generated.DeclarationReporting" }
    "app_installer_deployments_api.json"     = @{ Folder = "AppInstallerDeployments"; Class = "AppInstallerDeploymentsApiClient"; Namespace = "JamfDotNet.Platform.Generated.AppInstallerDeployments" }
    "app_installer_global_settings_api.json" = @{ Folder = "AppInstallerSettings"; Class = "AppInstallerSettingsApiClient"; Namespace = "JamfDotNet.Platform.Generated.AppInstallerSettings" }
    "app_installer_titles_api.json"          = @{ Folder = "AppInstallerTitles"; Class = "AppInstallerTitlesApiClient"; Namespace = "JamfDotNet.Platform.Generated.AppInstallerTitles" }
}

Push-Location $repoRoot
try {
    dotnet tool restore | Out-Host
    New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

    foreach ($specName in $map.Keys) {
        $specPath = Join-Path $SpecsDir $specName
        if (-not (Test-Path $specPath)) {
            Write-Warning "Missing spec: $specPath"
            continue
        }
        $meta = $map[$specName]
        $out = Join-Path $OutputRoot $meta.Folder
        if (Test-Path $out) { Remove-Item -Recurse -Force $out }
        New-Item -ItemType Directory -Force -Path $out | Out-Null
        Write-Host "Generating $($meta.Class) from $specName ..."
        dotnet tool run kiota generate `
            --language CSharp `
            --openapi $specPath `
            --output $out `
            --namespace-name $meta.Namespace `
            --class-name $meta.Class `
            --exclude-backward-compatible `
            --clean-output `
            --additional-data false
    }
}
finally {
    Pop-Location
}
