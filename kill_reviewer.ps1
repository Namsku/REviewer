# Kill REviewer Process
Write-Host "Killing REviewer process..." -ForegroundColor Yellow

$processes = Get-Process -Name "REviewer" -ErrorAction SilentlyContinue

if ($processes) {
    Stop-Process -Name "REviewer" -Force
    Write-Host "REviewer process terminated." -ForegroundColor Green
} else {
    Write-Host "REviewer process not running." -ForegroundColor Cyan
}

# Also kill overlay and tracker if running
$overlay = Get-Process -Name "Overlay" -ErrorAction SilentlyContinue
if ($overlay) {
    Stop-Process -Name "Overlay" -Force
    Write-Host "Overlay process terminated." -ForegroundColor Green
}

$tracker = Get-Process -Name "Tracker" -ErrorAction SilentlyContinue
if ($tracker) {
    Stop-Process -Name "Tracker" -Force
    Write-Host "Tracker process terminated." -ForegroundColor Green
}

# Wait a moment to ensure file locks release
Start-Sleep -Seconds 2
Write-Host "Done." -ForegroundColor Green
