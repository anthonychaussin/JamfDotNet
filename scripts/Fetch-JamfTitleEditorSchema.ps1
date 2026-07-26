<#
.SYNOPSIS
  Fetches the Title Editor OpenAPI schema from an instance, or keeps the bootstrap snapshot.
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = $env:JAMF_TITLE_EDITOR_URL,
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $OutputPath) {
    $OutputPath = Join-Path $scriptDir "..\openapi\jamf-title-editor.schema.json"
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
    Write-Host "JAMF_TITLE_EDITOR_URL not set. Keeping bootstrap schema at $OutputPath"
    if (-not (Test-Path $OutputPath)) {
        throw "Bootstrap schema missing at $OutputPath"
    }
    return
}

$base = $BaseUrl.TrimEnd('/')
$candidates = @(
    "$base/api",
    "$base/v2/openapi.json",
    "$base/api/openapi.json",
    "$base/swagger.json"
)

$downloaded = $false
foreach ($url in $candidates) {
    try {
        Write-Host "Trying $url ..."
        Invoke-WebRequest -Uri $url -OutFile $OutputPath -UseBasicParsing | Out-Null
        Write-Host "Saved schema to $OutputPath"
        $downloaded = $true
        break
    }
    catch {
        Write-Warning "Failed: $url — $($_.Exception.Message)"
    }
}

if (-not $downloaded) {
    throw "Unable to fetch Title Editor schema. Keep using the bootstrap openapi/jamf-title-editor.schema.json"
}
