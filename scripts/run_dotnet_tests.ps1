$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDirectory
$solutionPath = Join-Path $repoRoot 'YourGamesList.slnx'

# Tool versions
$reportGeneratorVersion = "5.4.4"

# Generate timestamped output folder name
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$resultsFolderName = "dotnet_unit_tests-$timestamp"
$testResultsDir = Join-Path $repoRoot "TestResults/$resultsFolderName"
$coverageReportDir = Join-Path $testResultsDir "coverage_report"
$indexHtmlPath = Join-Path $coverageReportDir "index.html"

Write-Debug "Script directory: $scriptDirectory"
Write-Debug "Repo root: $repoRoot"
Write-Debug "Test results output dir: $testResultsDir"

if (!(Test-Path $solutionPath))
{
    throw "Solution file not found: $solutionPath"
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

    # Install or update dotnet-reportgenerator-globaltool
    Write-Host "Ensuring dotnet-reportgenerator-globaltool version $reportGeneratorVersion is installed..."
    $installOutput = dotnet tool install dotnet-reportgenerator-globaltool --version $reportGeneratorVersion 2>&1
    if ($LASTEXITCODE -ne 0)
    {
        Write-Host "Tool 'dotnet-reportgenerator-globaltool' is already present. Updating to version $reportGeneratorVersion..."
        dotnet tool update dotnet-reportgenerator-globaltool --version $reportGeneratorVersion
    }

    # create folder for test results
    Write-Host "Creating output directory: $testResultsDir"
    New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null

    # run tests
    Write-Host "Running unit tests and collecting coverage..."
    dotnet test $solutionPath `
        --configuration Release `
        --collect:"XPlat Code Coverage" `
        --results-directory $testResultsDir

    # generate report
    Write-Host "Generating HTML coverage report..."
    $coverageFiles = Join-Path $testResultsDir "**/*.cobertura.xml"

    dotnet reportgenerator `
        "-reports:$coverageFiles" `
        "-targetdir:$coverageReportDir" `
        "-reporttypes:Html;HtmlSummary" `
        "-filefilters:-*.g.cs;-*.generated.cs"

    Write-Host "Tests complete. Coverage report generated at:  $indexHtmlPath"

    # oper report
    if (Test-Path $indexHtmlPath)
    {
        Write-Host "Opening report in browser..."
        Start-Process $indexHtmlPath
    }
}
finally
{
    Pop-Location
}