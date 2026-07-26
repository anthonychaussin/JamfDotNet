<#
.SYNOPSIS
  Downloads the Jamf Classic API Swagger/OpenAPI document into openapi/jamf-classic.swagger.yaml.
#>
[CmdletBinding()]
param(
    [string]$SchemaUrl = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $SchemaUrl) {
    $SchemaUrl = if ($env:JAMF_CLASSIC_SCHEMA_URL) {
        $env:JAMF_CLASSIC_SCHEMA_URL
    } else {
        "https://resources.jamf.com/open-api/10.25.0_CAPI.yaml"
    }
}
if (-not $OutputPath) {
    $OutputPath = Join-Path $scriptDir "..\openapi\jamf-classic.swagger.yaml"
}

$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $directory | Out-Null

Write-Host "Fetching Classic API schema from $SchemaUrl ..."
$headers = @{ "Accept" = "application/yaml, text/yaml, application/json, text/plain, */*" }
if ($env:JAMF_BEARER_TOKEN) {
    $headers["Authorization"] = "Bearer $($env:JAMF_BEARER_TOKEN)"
}

Invoke-WebRequest -Uri $SchemaUrl -Headers $headers -OutFile $OutputPath -UseBasicParsing
$size = (Get-Item $OutputPath).Length
Write-Host "Wrote $OutputPath ($size bytes)."
