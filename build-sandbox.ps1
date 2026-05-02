param(
    [ValidateSet("restore", "build", "test")]
    [string]$Command = "build",

    [string]$TestFilter
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sandboxTmp = Join-Path $repoRoot ".sandbox-tmp"

$paths = @(
    $sandboxTmp,
    (Join-Path $sandboxTmp "nuget-packages"),
    (Join-Path $sandboxTmp "nuget-http"),
    (Join-Path $sandboxTmp "nuget-scratch"),
    (Join-Path $sandboxTmp "dotnet-home")
)

foreach ($path in $paths) {
    New-Item -ItemType Directory -Force -Path $path | Out-Null
}

$env:TEMP = $sandboxTmp
$env:TMP = $sandboxTmp
$env:TMPDIR = $sandboxTmp

$env:DOTNET_CLI_HOME = Join-Path $sandboxTmp "dotnet-home"
$env:NUGET_PACKAGES = Join-Path $sandboxTmp "nuget-packages"
$env:NUGET_HTTP_CACHE_PATH = Join-Path $sandboxTmp "nuget-http"
$env:NUGET_SCRATCH = Join-Path $sandboxTmp "nuget-scratch"

$env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = "1"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = "0"

Push-Location $repoRoot
try {
    if ($Command -eq "restore") {
        dotnet restore --ignore-failed-sources
        exit $LASTEXITCODE
    }

    dotnet restore --ignore-failed-sources
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    if ($Command -eq "build") {
        dotnet build --no-restore
        exit $LASTEXITCODE
    }

    if ($TestFilter) {
        dotnet test --no-restore --filter $TestFilter
        exit $LASTEXITCODE
    }

    dotnet test --no-restore
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
