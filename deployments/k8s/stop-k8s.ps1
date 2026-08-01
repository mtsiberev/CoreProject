# stop-k8s.ps1 — Stops Minikube cluster and clears RAM
Write-Host "========== [1/2] Stopping Minikube cluster... ==========" -ForegroundColor Yellow
minikube stop

Write-Host "`n========== [2/2] Clearing background processes... ==========" -ForegroundColor Yellow
Stop-Process -Name kubectl -Force -ErrorAction SilentlyContinue

Write-Host "`n========== SUCCESS! RAM is fully cleared. ==========" -ForegroundColor Green
