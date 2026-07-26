<#
.SYNOPSIS
  Documents / refreshes the assembled Title Editor OpenAPI bootstrap.
.DESCRIPTION
  Without JAMF_TITLE_EDITOR_URL, keeps/refreshes openapi/jamf-title-editor.schema.json
  (hand-assembled from developer.jamf.com reference pages: auth, softwaretitles, sources, users, patches, preferences).
  With JAMF_TITLE_EDITOR_URL, delegates to Fetch-JamfTitleEditorSchema.ps1.
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = $env:JAMF_TITLE_EDITOR_URL,
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }

if (-not [string]::IsNullOrWhiteSpace($BaseUrl)) {
    & (Join-Path $scriptDir "Fetch-JamfTitleEditorSchema.ps1") -BaseUrl $BaseUrl -OutputPath $OutputPath
    return
}

if (-not $OutputPath) {
    $OutputPath = Join-Path $scriptDir "..\openapi\jamf-title-editor.schema.json"
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

if (-not (Test-Path $OutputPath)) {
    throw "Bootstrap schema missing at $OutputPath. Restore openapi/jamf-title-editor.schema.json from source control."
}

Write-Host "Using assembled Title Editor bootstrap schema at $OutputPath"
Write-Host "Set JAMF_TITLE_EDITOR_URL to fetch a full instance schema instead."
