#!/bin/bash

# NWSDB Kubernetes deployment automation script
# Usage: ./deploy.sh [action] [environment] [image-tag]
# Actions: build, push, deploy, all, status, logs, rollback
# Example: ./deploy.sh deploy prod v1.0.0

set -e

# Configuration
ACR_REGISTRY="nwsdbacr.azurecr.io"
ACR_USERNAME="${ACR_USERNAME:-}"
ACR_PASSWORD="${ACR_PASSWORD:-}"
NAMESPACE="${NAMESPACE:-nwsdb}"
REPOSITORY_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Arguments
ACTION="${1:-status}"
ENVIRONMENT="${2:-prod}"
IMAGE_TAG="${3:-latest}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Helper functions
info() {
	echo -e "${CYAN}ℹ $1${NC}"
}

success() {
	echo -e "${GREEN}✓ $1${NC}"
}

warning() {
	echo -e "${YELLOW}⚠ $1${NC}"
}

error() {
	echo -e "${RED}✗ $1${NC}"
}

build_images() {
	info "Building Docker images with tag: $IMAGE_TAG"

	# Build WebApp
	info "Building NWSDB.WebApp..."
	docker build -f src/NWSDB.WebApp/Dockerfile \
				 -t "$ACR_REGISTRY/nwsdb-webapp:$IMAGE_TAG" \
				 -t "$ACR_REGISTRY/nwsdb-webapp:latest" \
				 "$REPOSITORY_ROOT"
	success "WebApp image built: $ACR_REGISTRY/nwsdb-webapp:$IMAGE_TAG"

	# Build WebApi
	info "Building NWSDB.WebApi..."
	docker build -f src/NWSDB.WebApi/Dockerfile \
				 -t "$ACR_REGISTRY/nwsdb-webapi:$IMAGE_TAG" \
				 -t "$ACR_REGISTRY/nwsdb-webapi:latest" \
				 "$REPOSITORY_ROOT"
	success "WebApi image built: $ACR_REGISTRY/nwsdb-webapi:$IMAGE_TAG"
}

push_images() {
	info "Pushing images to ACR..."

	if [ -z "$ACR_USERNAME" ] || [ -z "$ACR_PASSWORD" ]; then
		warning "ACR credentials not set in environment"
		info "Logging into ACR with Azure CLI..."
		az acr login --name "nwsdbacr"
	else
		info "Logging into ACR with credentials..."
		echo "$ACR_PASSWORD" | docker login "$ACR_REGISTRY" -u "$ACR_USERNAME" --password-stdin
	fi

	# Push WebApp
	info "Pushing WebApp image..."
	docker push "$ACR_REGISTRY/nwsdb-webapp:$IMAGE_TAG"
	success "WebApp image pushed"

	# Push WebApi
	info "Pushing WebApi image..."
	docker push "$ACR_REGISTRY/nwsdb-webapi:$IMAGE_TAG"
	success "WebApi image pushed"
}

deploy() {
	info "Deploying to Kubernetes ($NAMESPACE namespace)..."

	# Create namespace
	info "Creating namespace..."
	kubectl apply -f "$REPOSITORY_ROOT/k8s/namespace.yaml"
	success "Namespace ready"

	# Create ACR secret
	info "Creating ACR secret..."
	kubectl delete secret acr-secret -n "$NAMESPACE" 2>/dev/null || true
	kubectl create secret docker-registry acr-secret \
		--docker-server="$ACR_REGISTRY" \
		--docker-username="$ACR_USERNAME" \
		--docker-password="$ACR_PASSWORD" \
		-n "$NAMESPACE"
	success "ACR secret created"

	# Deploy configuration
	info "Deploying configuration..."
	kubectl apply -f "$REPOSITORY_ROOT/k8s/configmap.yaml" -n "$NAMESPACE"
	kubectl apply -f "$REPOSITORY_ROOT/k8s/secrets.yaml" -n "$NAMESPACE" || true
	success "Configuration deployed"

	# Deploy WebApi
	info "Deploying WebApi..."
	kubectl apply -f "$REPOSITORY_ROOT/k8s/webapi-deployment.yaml" -n "$NAMESPACE"
	kubectl rollout status deployment/nwsdb-webapi -n "$NAMESPACE" --timeout=5m
	success "WebApi deployed"

	# Deploy WebApp
	info "Deploying WebApp..."
	kubectl apply -f "$REPOSITORY_ROOT/k8s/webapp-deployment.yaml" -n "$NAMESPACE"
	kubectl rollout status deployment/nwsdb-webapp -n "$NAMESPACE" --timeout=5m
	success "WebApp deployed"

	# Deploy services
	info "Exposing services..."
	kubectl apply -f "$REPOSITORY_ROOT/k8s/services.yaml" -n "$NAMESPACE"
	success "Services exposed"

	# Deploy ingress
	info "Configuring ingress..."
	kubectl apply -f "$REPOSITORY_ROOT/k8s/ingress.yaml" -n "$NAMESPACE"
	success "Ingress configured"

	# Enable autoscaling
	info "Enabling autoscaling..."
	kubectl apply -f "$REPOSITORY_ROOT/k8s/hpa.yaml" -n "$NAMESPACE"
	success "Autoscaling enabled"

	success "Deployment complete!"
	show_status
}

show_status() {
	echo ""
	info "Cluster status ($NAMESPACE namespace):"
	echo ""

	info "Deployments:"
	kubectl get deployment -n "$NAMESPACE"

	echo ""

	info "Pods:"
	kubectl get pods -n "$NAMESPACE"

	echo ""

	info "Services:"
	kubectl get svc -n "$NAMESPACE"

	echo ""

	info "Autoscaling:"
	kubectl get hpa -n "$NAMESPACE"

	echo ""

	info "Ingress:"
	kubectl get ingress -n "$NAMESPACE"
}

show_logs() {
	info "WebApp logs:"
	kubectl logs -f deployment/nwsdb-webapp -n "$NAMESPACE" --tail=50
}

rollback() {
	warning "Rolling back deployments..."
	kubectl rollout undo deployment/nwsdb-webapp -n "$NAMESPACE"
	kubectl rollout undo deployment/nwsdb-webapi -n "$NAMESPACE"
	success "Rollback complete"
}

# Main
echo ""
echo "════════════════════════════════════════════════════════"
echo -e "${CYAN}    NWSDB Kubernetes Deployment Script${NC}"
echo "════════════════════════════════════════════════════════"
echo ""

case "$ACTION" in
	build)
		build_images
		;;
	push)
		push_images
		;;
	deploy)
		deploy
		;;
	all)
		build_images
		push_images
		deploy
		;;
	status)
		show_status
		;;
	logs)
		show_logs
		;;
	rollback)
		rollback
		;;
	*)
		error "Unknown action: $ACTION"
		echo "Usage: $0 [build|push|deploy|all|status|logs|rollback] [environment] [image-tag]"
		exit 1
		;;
esac

echo ""
success "Done!"
echo ""
