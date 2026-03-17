<#
.SYNOPSIS
    Creates the DashLook GitHub repo and pushes all commits.
.DESCRIPTION
    Run this script once after cloning / setting up the project.
    You need a GitHub Personal Access Token with 'repo' scope.

    Create token at: https://github.com/settings/tokens/new
    Required scope : repo (Full control of private repositories)

.EXAMPLE
    .\scripts\create-github-repo.ps1 -Token "ghp_xxxxxxxxxxxx"
    .\scripts\create-github-repo.ps1   # will prompt for token
#>

param(
    [string]$Token,
    [string]$RepoName  = "DashLook",
    [string]$Username  = "itrabbi24",
    [string]$Branch    = "main"
)

if (-not $Token) {
    $secure = Read-Host "GitHub Personal Access Token" -AsSecureString
    $Token  = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
}

$headers = @{
    Authorization = "token $Token"
    Accept        = "application/vnd.github.v3+json"
    "User-Agent"  = "DashLook-Setup"
}

Write-Host "Creating GitHub repository '$RepoName'..." -ForegroundColor Cyan

$body = @{
    name        = $RepoName
    description = "Instant file preview for Windows — press Space to peek at any file"
    homepage    = "https://itrabbi24.github.io/"
    private     = $false
    has_issues  = $true
    has_projects= $false
    has_wiki    = $false
    auto_init   = $false
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod `
        -Uri "https://api.github.com/user/repos" `
        -Method POST `
        -Headers $headers `
        -Body $body `
        -ContentType "application/json"

    Write-Host "Repository created: $($response.html_url)" -ForegroundColor Green
} catch {
    if ($_.Exception.Response.StatusCode -eq 422) {
        Write-Host "Repository already exists, continuing..." -ForegroundColor Yellow
    } else {
        Write-Host "Failed to create repo: $_" -ForegroundColor Red
        exit 1
    }
}

# Set main as default branch and push
$remoteUrl = "https://$Token@github.com/$Username/$RepoName.git"

git branch -M $Branch
git remote add origin "https://github.com/$Username/$RepoName.git"

# Use token in the URL for authentication
git remote set-url origin $remoteUrl
git push -u origin $Branch

# Reset remote URL to not include token (security)
git remote set-url origin "https://github.com/$Username/$RepoName.git"

Write-Host ""
Write-Host "Done! Repository is live at:" -ForegroundColor Green
Write-Host "https://github.com/$Username/$RepoName" -ForegroundColor Cyan
