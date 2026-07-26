<#
.SYNOPSIS
  Optionally fetches Jamf School OpenAPI/Swagger docs from an instance.
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = $env:JAMF_SCHOOL_URL,
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $OutputPath) {
    $OutputPath = Join-Path $scriptDir "..\openapi\jamf-school.schema.json"
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
    Write-Host "JAMF_SCHOOL_URL not set. JamfDotNet.School remains hand-written (no OpenAPI snapshot required)."
    return
}

$base = $BaseUrl.TrimEnd('/')
$candidates = @(
    "$base/api/docs",
    "$base/api/swagger.json",
    "$base/api/openapi.json",
    "$base/swagger.json"
)

foreach ($url in $candidates) {
    try {
        Write-Host "Trying $url ..."
        Invoke-WebRequest -Uri $url -OutFile $OutputPath -UseBasicParsing | Out-Null
        Write-Host "Saved schema to $OutputPath"
        return
    }
    catch {
        Write-Warning "Failed: $url — $($_.Exception.Message)"
    }
}

Write-Warning "No School OpenAPI document found. Continue with the hand-written JamfDotNet.School client."
