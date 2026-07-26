<#
.SYNOPSIS
  Downloads the Jamf Pro OpenAPI schema into openapi/jamf-pro.schema.json.

.PARAMETER SchemaUrl
  Full URL to the OpenAPI document. Defaults to the public Jamf dummy instance.
#>
[CmdletBinding()]
param(
    [string]$SchemaUrl = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $SchemaUrl) {
    $SchemaUrl = if ($env:JAMF_SCHEMA_URL) { $env:JAMF_SCHEMA_URL } else { "https://dummy.jamfcloud.com/api/schema" }
}
if (-not $OutputPath) {
    $OutputPath = Join-Path $scriptDir "..\openapi\jamf-pro.schema.json"
}

$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $directory | Out-Null

Write-Host "Fetching OpenAPI schema from $SchemaUrl ..."
$headers = @{
    "Accept" = "application/json"
}
if ($env:JAMF_BEARER_TOKEN) {
    $headers["Authorization"] = "Bearer $($env:JAMF_BEARER_TOKEN)"
}

Invoke-WebRequest -Uri $SchemaUrl -Headers $headers -OutFile $OutputPath -UseBasicParsing
$size = (Get-Item $OutputPath).Length
Write-Host "Wrote $OutputPath ($size bytes)."
