param(
    [Parameter(Mandatory = $True)]
    [string]$environment,
    [Parameter(Mandatory = $False)]
    [string]$migrationName
)

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDirectory
$apiProjectPath = Join-Path $repoRoot 'YourGamesList.API/YourGamesList.API.csproj'
$databaseProjectPath = Join-Path $repoRoot 'YourGamesList.Database/YourGamesList.Database.csproj'

$dotnetEfVersion = "10.0.10"
$microsoftEntityFrameworkCoreDesignVersion = "10.0.10"

Write-Debug "Script directory: $scriptDirectory"
Write-Debug "Repo root: $repoRoot"
Write-Debug "Api project path: $apiProjectPath"
Write-Debug "Database project path: $databaseProjectPath"

if (!(Test-Path $databaseProjectPath))
{
    throw "Database project file not found: $databaseProjectPath"
}

if (!(Test-Path $apiProjectPath))
{
    throw "API project file not found: $apiProjectPath"
}

Push-Location $repoRoot

$toolManifestPath = Join-Path $repoRoot 'dotnet-tools.json'

try
{
    if (!(Test-Path $toolManifestPath))
    {
        Write-Host "Create new tool manifest"
        dotnet new tool-manifest --force
    }

    Write-Host "Setting 'ENV' environment variable to '$environment'"
    $env:ENV = $environment

    Write-Host "Install dotnet tool 'dotnet-ef'"
    dotnet tool install --local dotnet-ef --version $dotnetEfVersion

    Write-Host "Add package 'Microsoft.EntityFrameworkCore.Design'"
    dotnet add $apiProjectPath package Microsoft.EntityFrameworkCore.Design --version $microsoftEntityFrameworkCoreDesignVersion

    if (![string]::IsNullOrEmpty($migrationName))
    {
        Write-Host "Create new migration '$migrationName'"
        dotnet ef migrations add $migrationName --project $databaseProjectPath --startup-project $apiProjectPath
    }
    else
    {
        Write-Host "Skipping migration"
    }
    
    Write-Host "Update database"
    dotnet ef database update --project $databaseProjectPath --startup-project $apiProjectPath
}
finally
{
    Write-Host "Resetting 'ENV' environment variable"
    $env:ENV = $null

    try
    {
        dotnet tool uninstall --local dotnet-ef
    }
    catch
    {
        Write-Warning "Unable to uninstall dotnet-ef from the local tool manifest."
    }

    try
    {
        Write-Host "Remove package 'Microsoft.EntityFrameworkCore.Design'"
        dotnet remove $apiProjectPath package Microsoft.EntityFrameworkCore.Design
    }
    catch
    {
        Write-Warning "Unable to remove package 'Microsoft.EntityFrameworkCore.Design'."
    }

    if (Test-Path $toolManifestPath)
    {
        Remove-Item $toolManifestPath -Force
    }



    Pop-Location
}
