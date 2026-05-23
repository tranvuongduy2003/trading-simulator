# Sets GitHub Project v2 Status for issues on the board configured in .github/github-project.json
param(
    [Parameter(Mandatory = $true)]
    [int[]] $IssueNumber,

    [string] $StatusName,

    [string] $ConfigPath = ".github/github-project.json"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ConfigPath)) {
    Write-Error "Missing $ConfigPath - copy .github/github-project.json.example and set owner/projectNumber."
}

$config = Get-Content -Raw $ConfigPath | ConvertFrom-Json
$owner = $config.owner
$projectNumber = $config.projectNumber

if (-not $StatusName) {
    $StatusName = if ($config.statusOnPrCreated) { $config.statusOnPrCreated } else { "In review" }
}

$project = gh project view $projectNumber --owner $owner --format json | ConvertFrom-Json
$projectId = $project.id

$fieldsJson = gh project field-list $projectNumber --owner $owner --format json | ConvertFrom-Json
$statusField = $fieldsJson.fields | Where-Object { $_.name -eq "Status" }

if (-not $statusField) {
    Write-Error "Project #$projectNumber has no Status field."
}

function Normalize-StatusName([string]$value) {
    return ($value -replace "\s+", "").ToLowerInvariant()
}

$target = Normalize-StatusName $StatusName
$option = $statusField.options | Where-Object { (Normalize-StatusName $_.name) -eq $target }

if (-not $option) {
    $available = ($statusField.options.name -join ", ")
    Write-Error "Status '$StatusName' not found on project #$projectNumber. Available: $available. Add the option in GitHub Project settings or set statusOnPrCreated in $ConfigPath."
}

$itemsJson = gh project item-list $projectNumber --owner $owner --format json --limit 200 | ConvertFrom-Json

foreach ($num in $IssueNumber) {
    $item = $itemsJson.items | Where-Object { $_.content.number -eq $num } | Select-Object -First 1

    if (-not $item) {
        Write-Warning "Issue #$num is not on project #$projectNumber - add it with gh project item-add and the issue URL."
        continue
    }

    gh project item-edit `
        --project-id $projectId `
        --id $item.id `
        --field-id $statusField.id `
        --single-select-option-id $option.id | Out-Null

    Write-Host "Project Status -> '$($option.name)' for issue #$num"
}
