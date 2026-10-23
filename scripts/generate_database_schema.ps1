param(
    [Parameter(Mandatory = $True)]
    [string]$environment,
    [Parameter(Mandatory = $False)]
    [string]$migrationName
)

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDirectory
$apiProjectPath = Join-Path $repoRoot 'YourGamesList.Api/YourGamesList.Api.csproj'
$databaseProjectPath = Join-Path $repoRoot 'YourGamesList.Database/YourGamesList.Database.csproj'

# Tool versions
$dotnetEfVersion = "10.0.13"
$microsoftEntityFrameworkCoreDesignVersion = "10.0.13"

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

$configFolder = Join-Path $repoRoot '.config'
$toolManifestPath = Join-Path $configFolder 'dotnet-tools.json'

try
{
    # Check if manifest already exists
    if (!(Test-Path $toolManifestPath))
    {
        Write-Host "No tool manifest found. Creating a new one at .config/dotnet-tools.json..."
        dotnet new tool-manifest --output $configFolder
    }

    # Install or update dotnet-ef package
    Write-Host "Ensuring dotnet-ef version $dotnetEfVersion is installed..."
    $installOutput = dotnet tool install dotnet-ef --version $dotnetEfVersion 2>&1
    if ($LASTEXITCODE -ne 0)
    {
        Write-Host "Tool 'dotnet-ef' is already present. Updating to version $dotnetEfVersion..."
        dotnet tool update dotnet-ef --version $dotnetEfVersion
    }

    # add 'Microsoft.EntityFrameworkCore.Design' package to api project
    Write-Host "Ensuring package 'Microsoft.EntityFrameworkCore.Design' ($microsoftEntityFrameworkCoreDesignVersion) is added to API project..."
    dotnet add $apiProjectPath package Microsoft.EntityFrameworkCore.Design --version $microsoftEntityFrameworkCoreDesignVersion

    Write-Host "Setting 'ENV' environment variable to '$environment'"
    $env:ENV = $environment

    # run mingration if requested
    if (![string]::IsNullOrEmpty($migrationName))
    {
        Write-Host "Creating new migration '$migrationName'..."
        dotnet ef migrations add $migrationName --project $databaseProjectPath --startup-project $apiProjectPath
    }
    else
    {
        Write-Host "Skipping migration creation."
    }

    Write-Host "Updating database..."
    dotnet ef database update --project $databaseProjectPath --startup-project $apiProjectPath
}
finally
{
    dotnet remove $apiProjectPath package Microsoft.EntityFrameworkCore.Design
    Write-Host "Resetting 'ENV' environment variable"
    $env:ENV = $null
    Pop-Location
}