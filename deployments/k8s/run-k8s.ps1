# run-k8s.ps1 — Starts the entire infrastructure in Kubernetes
Write-Host "========== [1/4] Starting Minikube cluster... ==========" -ForegroundColor Cyan
minikube start --listen-address=0.0.0.0
if ($LASTEXITCODE -ne 0) { Write-Error "Minikube start failed"; exit }

Write-Host "`n========== [2/4] Compiling C# and building Docker images... ==========" -ForegroundColor Cyan
dotnet publish MyStore.Infrastructure/MyStore.Infrastructure.csproj -c Release -o MyStore.Api/out /p:UseAppHost=false
dotnet publish MyStore.Api/MyStore.Api.csproj -c Release -o MyStore.Api/out /p:UseAppHost=false
docker build --target final-k8s -t mystoreapi:local -f MyStore.Api/Dockerfile .
docker build --target final-k8s -t mystorewarehouse:local -f MyStore.Warehouse/Dockerfile .

Write-Host "`n========== [3/4] Loading images into Minikube... ==========" -ForegroundColor Cyan
minikube image load mystoreapi:local
minikube image load mystorewarehouse:local

Write-Host "`n========== [4/4] Deploying manifests to Kubernetes... ==========" -ForegroundColor Cyan
kubectl apply -f deployments/k8s/

Write-Host "`nWaiting for pods to stabilize (15 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`nCurrent pods status:" -ForegroundColor Green
kubectl get pods

Write-Host "`nStarting Kubernetes Dashboard..." -ForegroundColor Green
minikube dashboard
