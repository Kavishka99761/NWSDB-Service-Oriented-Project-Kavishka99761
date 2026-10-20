#!/usr/bin/env pwsh

<#
.SYNOPSIS
	NWSDB Kubernetes deployment automation script
.DESCRIPTION
	Automates building, pushing, and deploying NWSDB containers to Kubernetes
.PARAMETER Action
	Action to perform: build, push, deploy, all, status, logs, rollback
.PARAMETER Environment
	Target environment: dev, staging, prod
.PARAMETER ImageTag
	Docker image tag (default: latest)
.EXAMPLE
	.\deploy.ps1 -Action build -ImageTag 1.0.0
	.\deploy.ps1 -Action push
	.\deploy.ps1 -Action deploy -Environment prod
#>

param(
	[ValidateSet('build', 'push', 'deploy', 'all', 'status', 'logs', 'rollback')]
	[string]$Action = 'status',

	[ValidateSet('dev', 'staging', 'prod')]
	[string]$Environment = 'prod',

	[string]$ImageTag = 'latest',

	[string]$Namespace = 'nwsdb'
)

# Configuration
$Script:ACR_REGISTRY = "nwsdbacr.azurecr.io"
$Script:ACRUsername = $env:ACR_USERNAME
$Script:ACRPassword = $env:ACR_PASSWORD
$Script:RepositoryRoot = (Get-Location).Path

# Colors
$Green = [System.ConsoleColor]::Green
$Yellow = [System.ConsoleColor]::Yellow
$Red = [System.ConsoleColor]::Red
$Cyan = [System.ConsoleColor]::Cyan

function Write-Success {
	param([string]$Message)
	Write-Host "✓ $Message" -ForegroundColor $Green
}

function Write-Info {
	param([string]$Message)
	Write-Host "ℹ $Message" -ForegroundColor $Cyan
}

function Write-Warning {
	param([string]$Message)
	Write-Host "⚠ $Message" -ForegroundColor $Yellow
}

function Write-Error {
	param([string]$Message)
	Write-Host "✗ $Message" -ForegroundColor $Red
}

function Invoke-Build {
	Write-Info "Building Docker images with tag: $ImageTag"

	try {
		# Build WebApp
		Write-Info "Building NWSDB.WebApp..."
		docker build -f src/NWSDB.WebApp/Dockerfile `
					 -t "$Script:ACR_REGISTRY/nwsdb-webapp:$ImageTag" `
					 -t "$Script:ACR_REGISTRY/nwsdb-webapp:latest" `
					 .
		if ($LASTEXITCODE -ne 0) { throw "WebApp build failed" }
		Write-Success "WebApp image built: $Script:ACR_REGISTRY/nwsdb-webapp:$ImageTag"

		# Build WebApi
		Write-Info "Building NWSDB.WebApi..."
		docker build -f src/NWSDB.WebApi/Dockerfile `
					 -t "$Script:ACR_REGISTRY/nwsdb-webapi:$ImageTag" `
					 -t "$Script:ACR_REGISTRY/nwsdb-webapi:latest" `
					 .
		if ($LASTEXITCODE -ne 0) { throw "WebApi build failed" }
		Write-Success "WebApi image built: $Script:ACR_REGISTRY/nwsdb-webapi:$ImageTag"
	}
	catch {
		Write-Error "Build failed: $_"
		exit 1
	}
}

function Invoke-Push {
	Write-Info "Pushing images to ACR..."

	if (-not $Script:ACRUsername -or -not $Script:ACRPassword) {
		Write-Warning "ACR_USERNAME and ACR_PASSWORD environment variables not set"
		Write-Info "Setting up ACR login..."
		& az acr login --name "nwsdbacr"
	} else {
		Write-Info "Logging into ACR with credentials..."
		echo "$Script:ACRPassword" | docker login "$Script:ACR_REGISTRY" -u "$Script:ACRUsername" --password-stdin
	}

	try {
		# Push WebApp
		Write-Info "Pushing WebApp image..."
		docker push "$Script:ACR_REGISTRY/nwsdb-webapp:$ImageTag"
		if ($LASTEXITCODE -ne 0) { throw "WebApp push failed" }
		Write-Success "WebApp image pushed"

		# Push WebApi
		Write-Info "Pushing WebApi image..."
		docker push "$Script:ACR_REGISTRY/nwsdb-webapi:$ImageTag"
		if ($LASTEXITCODE -ne 0) { throw "WebApi push failed" }
		Write-Success "WebApi image pushed"
	}
	catch {
		Write-Error "Push failed: $_"
		exit 1
	}
}

function Invoke-Deploy {
	Write-Info "Deploying to Kubernetes ($Namespace namespace)..."

	try {
		# Create namespace
		Write-Info "Creating namespace..."
		kubectl apply -f k8s/namespace.yaml
		Write-Success "Namespace ready"

		# Create ACR secret
		Write-Info "Creating ACR secret..."
		kubectl delete secret acr-secret -n $Namespace 2>$null
		kubectl create secret docker-registry acr-secret `
			--docker-server=$Script:ACR_REGISTRY `
			--docker-username=$Script:ACRUsername `
			--docker-password=$Script:ACRPassword `
			-n $Namespace
		Write-Success "ACR secret created"

		# Deploy configuration
		Write-Info "Deploying configuration..."
		kubectl apply -f k8s/configmap.yaml -n $Namespace
		kubectl apply -f k8s/secrets.yaml -n $Namespace
		Write-Success "Configuration deployed"

		# Deploy WebApi
		Write-Info "Deploying WebApi..."
		kubectl apply -f k8s/webapi-deployment.yaml -n $Namespace
		kubectl rollout status deployment/nwsdb-webapi -n $Namespace --timeout=5m
		Write-Success "WebApi deployed"

		# Deploy WebApp
		Write-Info "Deploying WebApp..."
		kubectl apply -f k8s/webapp-deployment.yaml -n $Namespace
		kubectl rollout status deployment/nwsdb-webapp -n $Namespace --timeout=5m
		Write-Success "WebApp deployed"

		# Deploy services
		Write-Info "Exposing services..."
		kubectl apply -f k8s/services.yaml -n $Namespace
		Write-Success "Services exposed"

		# Deploy ingress
		Write-Info "Configuring ingress..."
		kubectl apply -f k8s/ingress.yaml -n $Namespace
		Write-Success "Ingress configured"

		# Enable autoscaling
		Write-Info "Enabling autoscaling..."
		kubectl apply -f k8s/hpa.yaml -n $Namespace
		Write-Success "Autoscaling enabled"

		Write-Success "Deployment complete!"
		Invoke-Status
	}
	catch {
		Write-Error "Deployment failed: $_"
		exit 1
	}
}

function Invoke-Status {
	Write-Info "Cluster status ($Namespace namespace):"
	Write-Host ""

	# Deployments
	Write-Info "Deployments:"
	kubectl get deployment -n $Namespace

	Write-Host ""

	# Pods
	Write-Info "Pods:"
	kubectl get pods -n $Namespace

	Write-Host ""

	# Services
	Write-Info "Services:"
	kubectl get svc -n $Namespace

	Write-Host ""

	# HPA
	Write-Info "Autoscaling:"
	kubectl get hpa -n $Namespace

	Write-Host ""

	# Ingress
	Write-Info "Ingress:"
	kubectl get ingress -n $Namespace
}

function Invoke-Logs {
	Write-Info "WebApp logs:"
	kubectl logs -f deployment/nwsdb-webapp -n $Namespace --tail=50
}

function Invoke-Rollback {
	Write-Warning "Rolling back deployments..."

	try {
		kubectl rollout undo deployment/nwsdb-webapp -n $Namespace
		kubectl rollout undo deployment/nwsdb-webapi -n $Namespace
		Write-Success "Rollback complete"
	}
	catch {
		Write-Error "Rollback failed: $_"
		exit 1
	}
}

# Main execution
Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "    NWSDB Kubernetes Deployment Script" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

switch ($Action) {
	'build' {
		Invoke-Build
	}
	'push' {
		Invoke-Push
	}
	'deploy' {
		Invoke-Deploy
	}
	'all' {
		Invoke-Build
		Invoke-Push
		Invoke-Deploy
	}
	'status' {
		Invoke-Status
	}
	'logs' {
		Invoke-Logs
	}
	'rollback' {
		Invoke-Rollback
	}
	default {
		Write-Error "Unknown action: $Action"
		exit 1
	}
}

Write-Host ""
Write-Host "✓ Done!" -ForegroundColor Green
Write-Host ""
