# NWSDB Kubernetes Deployment Guide

This guide provides comprehensive instructions for deploying the NWSDB system to Kubernetes and managing it in production.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Project Structure](#project-structure)
3. [Local Development](#local-development)
4. [Container Images](#container-images)
5. [Kubernetes Deployment](#kubernetes-deployment)
6. [Configuration Management](#configuration-management)
7. [Troubleshooting](#troubleshooting)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Scaling and Performance](#scaling-and-performance)

---

## Prerequisites

### Required Tools
- **Docker**: v20.10+ ([Install](https://docs.docker.com/get-docker/))
- **kubectl**: v1.25+ ([Install](https://kubernetes.io/docs/tasks/tools/))
- **Kubernetes Cluster**: v1.25+ (AKS, EKS, GKE, or local Kind/Minikube)
- **Helm** (optional): For templating ([Install](https://helm.sh/docs/intro/install/))

### Azure Prerequisites (if deploying to AKS)
- Azure subscription
- Azure CLI: `az` command-line tool
- Azure Container Registry (ACR) account
- AKS cluster provisioned

### Environment Variables
```bash
# Container Registry
export ACR_REGISTRY=nwsdbacr.azurecr.io
export ACR_USERNAME=<your-acr-username>
export ACR_PASSWORD=<your-acr-password>

# Kubernetes Cluster
export CLUSTER_NAME=nwsdb-aks
export RESOURCE_GROUP=nwsdb-rg
export REGION=eastus
```

---

## Project Structure

```
NWSDB-system/
├── src/
│   ├── NWSDB.WebApp/           # Razor Pages frontend
│   │   └── Dockerfile
│   ├── NWSDB.WebApi/           # ASP.NET Core API backend
│   │   └── Dockerfile
│   ├── NWSDB.Core/             # Core business logic
│   ├── NWSDB.Data/             # Data access layer
│   └── NWSDB.Services/         # Supporting services
├── k8s/                        # Kubernetes manifests
│   ├── namespace.yaml          # Namespace definition
│   ├── configmap.yaml          # Non-sensitive configuration
│   ├── secrets.yaml.template   # Sensitive configuration template
│   ├── webapp-deployment.yaml  # WebApp deployment
│   ├── webapi-deployment.yaml  # WebApi deployment
│   ├── services.yaml           # Service definitions
│   ├── ingress.yaml            # Ingress configuration
│   └── hpa.yaml                # Autoscaling configuration
├── docker-compose.yml          # Local development environment
└── KUBERNETES_DEPLOYMENT.md    # This file
```

---

## Local Development

### Quick Start with Docker Compose

1. **Clone the repository** and navigate to the root directory:
```bash
cd /path/to/NWSDB-system-main
```

2. **Build and start services**:
```bash
docker-compose up -d
```

3. **Access applications**:
   - **WebApp**: http://localhost:5000
   - **WebApi**: http://localhost:5001

4. **View logs**:
```bash
docker-compose logs -f nwsdb-webapp
docker-compose logs -f nwsdb-webapi
```

5. **Stop services**:
```bash
docker-compose down
```

### Building Docker Images Locally

```bash
# Build WebApp image
docker build -f src/NWSDB.WebApp/Dockerfile -t nwsdbacr.azurecr.io/nwsdb-webapp:1.0.0 .

# Build WebApi image
docker build -f src/NWSDB.WebApi/Dockerfile -t nwsdbacr.azurecr.io/nwsdb-webapi:1.0.0 .

# Run containers manually
docker run -p 5000:8080 nwsdbacr.azurecr.io/nwsdb-webapp:1.0.0
docker run -p 5001:8080 nwsdbacr.azurecr.io/nwsdb-webapi:1.0.0
```

---

## Container Images

### Building and Pushing to Azure Container Registry

1. **Login to ACR**:
```bash
az acr login --name nwsdbacr
# or
docker login nwsdbacr.azurecr.io -u $ACR_USERNAME -p $ACR_PASSWORD
```

2. **Build images**:
```bash
# Build with ACR (recommended for CI/CD)
az acr build --registry nwsdbacr --image nwsdb-webapp:1.0.0 --file src/NWSDB.WebApp/Dockerfile .
az acr build --registry nwsdbacr --image nwsdb-webapi:1.0.0 --file src/NWSDB.WebApi/Dockerfile .

# Or build locally and push
docker build -f src/NWSDB.WebApp/Dockerfile -t nwsdbacr.azurecr.io/nwsdb-webapp:1.0.0 .
docker build -f src/NWSDB.WebApi/Dockerfile -t nwsdbacr.azurecr.io/nwsdb-webapi:1.0.0 .

docker push nwsdbacr.azurecr.io/nwsdb-webapp:1.0.0
docker push nwsdbacr.azurecr.io/nwsdb-webapi:1.0.0
```

3. **Verify images in registry**:
```bash
az acr repository list --name nwsdbacr
az acr repository show-tags --name nwsdbacr --repository nwsdb-webapp
az acr repository show-tags --name nwsdbacr --repository nwsdb-webapi
```

### Image Versioning Strategy
- Use semantic versioning: `v1.0.0`, `v1.1.0`, etc.
- Tag releases in git: `git tag v1.0.0 && git push origin v1.0.0`
- Update image tags in Kubernetes manifests for each deployment

---

## Kubernetes Deployment

### Step 1: Create Namespace and Secrets

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Create secrets from template
cp k8s/secrets.yaml.template k8s/secrets.yaml
# Edit k8s/secrets.yaml with your actual credentials

# Create ACR secret for image pulls
kubectl create secret docker-registry acr-secret \
  --docker-server=nwsdbacr.azurecr.io \
  --docker-username=$ACR_USERNAME \
  --docker-password=$ACR_PASSWORD \
  --docker-email=your-email@example.com \
  -n nwsdb

# Apply secrets
kubectl apply -f k8s/secrets.yaml
```

### Step 2: Deploy Configuration

```bash
# Deploy ConfigMap
kubectl apply -f k8s/configmap.yaml

# Verify ConfigMap
kubectl get configmap -n nwsdb
kubectl describe configmap nwsdb-config -n nwsdb
```

### Step 3: Deploy Applications

```bash
# Deploy WebApi first (since WebApp depends on it)
kubectl apply -f k8s/webapi-deployment.yaml

# Wait for WebApi to be ready
kubectl rollout status deployment/nwsdb-webapi -n nwsdb

# Deploy WebApp
kubectl apply -f k8s/webapp-deployment.yaml

# Wait for WebApp to be ready
kubectl rollout status deployment/nwsdb-webapp -n nwsdb
```

### Step 4: Expose Services

```bash
# Deploy Services
kubectl apply -f k8s/services.yaml

# Verify services
kubectl get svc -n nwsdb
```

### Step 5: Configure Ingress (Optional)

```bash
# Deploy Ingress (requires ingress controller)
kubectl apply -f k8s/ingress.yaml

# Verify Ingress
kubectl get ingress -n nwsdb
kubectl describe ingress nwsdb-ingress -n nwsdb
```

### Step 6: Enable Autoscaling

```bash
# Deploy HPA
kubectl apply -f k8s/hpa.yaml

# Monitor autoscaling
kubectl get hpa -n nwsdb --watch
```

### Complete Deployment Script

```bash
#!/bin/bash
set -e

NAMESPACE="nwsdb"

echo "Creating namespace..."
kubectl apply -f k8s/namespace.yaml

echo "Creating ACR secret..."
kubectl create secret docker-registry acr-secret \
  --docker-server=nwsdbacr.azurecr.io \
  --docker-username=$ACR_USERNAME \
  --docker-password=$ACR_PASSWORD \
  -n $NAMESPACE

echo "Deploying configuration..."
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml

echo "Deploying WebApi..."
kubectl apply -f k8s/webapi-deployment.yaml
kubectl rollout status deployment/nwsdb-webapi -n $NAMESPACE

echo "Deploying WebApp..."
kubectl apply -f k8s/webapp-deployment.yaml
kubectl rollout status deployment/nwsdb-webapp -n $NAMESPACE

echo "Exposing services..."
kubectl apply -f k8s/services.yaml

echo "Setting up Ingress..."
kubectl apply -f k8s/ingress.yaml

echo "Enabling autoscaling..."
kubectl apply -f k8s/hpa.yaml

echo "Deployment complete!"
kubectl get all -n $NAMESPACE
```

---

## Configuration Management

### Using ConfigMaps

ConfigMaps store non-sensitive configuration data:

```bash
# View ConfigMap
kubectl get configmap -n nwsdb
kubectl describe configmap nwsdb-config -n nwsdb

# Update ConfigMap (edit and apply)
kubectl edit configmap nwsdb-config -n nwsdb

# Or update from file
kubectl apply -f k8s/configmap.yaml
```

### Using Secrets

Secrets store sensitive data (encrypted at rest in etcd):

```bash
# Create secret
kubectl create secret generic nwsdb-secrets \
  --from-literal=jwt-secret-key=your-key \
  --from-literal=db-password=your-password \
  -n nwsdb

# View secrets (encoded)
kubectl get secret -n nwsdb
kubectl get secret nwsdb-secrets -o yaml -n nwsdb

# For production, use:
# - Azure Key Vault with CSI driver
# - Sealed Secrets
# - HashiCorp Vault
```

### Environment-Specific Configuration

Create separate files for different environments:

```
k8s/
├── configmap.yaml           # Base configuration
├── configmap-dev.yaml       # Development overrides
├── configmap-staging.yaml   # Staging overrides
└── configmap-prod.yaml      # Production overrides
```

Apply with kustomize or separate namespace:

```bash
# Deploy to dev
kubectl apply -f k8s/configmap-dev.yaml -n nwsdb-dev

# Deploy to staging
kubectl apply -f k8s/configmap-staging.yaml -n nwsdb-staging

# Deploy to production
kubectl apply -f k8s/configmap-prod.yaml -n nwsdb-prod
```

---

## Common Operations

### Check Deployment Status

```bash
# Check all resources in namespace
kubectl get all -n nwsdb

# Check pods
kubectl get pods -n nwsdb
kubectl describe pod <pod-name> -n nwsdb

# Check events
kubectl get events -n nwsdb --sort-by='.lastTimestamp'
```

### View Logs

```bash
# WebApp logs
kubectl logs -f deployment/nwsdb-webapp -n nwsdb

# WebApi logs
kubectl logs -f deployment/nwsdb-webapi -n nwsdb

# Specific pod
kubectl logs -f nwsdb-webapp-<pod-id> -n nwsdb
```

### Execute Commands in Container

```bash
# Interactive shell
kubectl exec -it <pod-name> -n nwsdb -- /bin/bash

# Run command
kubectl exec <pod-name> -n nwsdb -- curl http://localhost:8080/health
```

### Update Deployment

```bash
# Update image
kubectl set image deployment/nwsdb-webapp \
  webapp=nwsdbacr.azurecr.io/nwsdb-webapp:1.1.0 \
  -n nwsdb

# Scale deployment
kubectl scale deployment nwsdb-webapp --replicas=5 -n nwsdb

# Rolling restart
kubectl rollout restart deployment/nwsdb-webapp -n nwsdb

# Rollback to previous version
kubectl rollout undo deployment/nwsdb-webapp -n nwsdb
kubectl rollout history deployment/nwsdb-webapp -n nwsdb
```

---

## Troubleshooting

### Pod Not Starting

```bash
# Check pod status
kubectl describe pod <pod-name> -n nwsdb

# Common issues:
# 1. Image pull errors - verify image exists in registry
# 2. Resource constraints - check node resources
# 3. Readiness/liveness probe failures - check health endpoints

# View logs for errors
kubectl logs <pod-name> -n nwsdb --previous
```

### Service Not Accessible

```bash
# Check service
kubectl get svc -n nwsdb
kubectl describe svc nwsdb-webapp -n nwsdb

# Check endpoints
kubectl get endpoints -n nwsdb

# Test connectivity from within cluster
kubectl run -it --rm debug --image=busybox --restart=Never -- /bin/sh
# Inside shell: wget -O- http://nwsdb-webapp:8080
```

### Ingress Not Working

```bash
# Check ingress controller
kubectl get pods -n ingress-nginx
kubectl describe ingress nwsdb-ingress -n nwsdb

# Check DNS resolution
nslookup nwsdb.example.com
```

### Database Connection Issues

```bash
# Check volume mounts
kubectl get pods -n nwsdb -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.spec.volumes[*].name}{"\n"}{end}'

# Exec into pod and test connection
kubectl exec -it <webapi-pod> -n nwsdb -- sqlite3 /data/nwsdb_system.db ".tables"
```

---

## Monitoring and Logging

### Prometheus and Grafana

The deployments include Prometheus scrape annotations:

```bash
# Install Prometheus operator (via Helm)
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm install kube-prometheus-stack prometheus-community/kube-prometheus-stack

# Access Grafana dashboard
kubectl port-forward svc/kube-prometheus-stack-grafana 3000:80 -n monitoring
# Open http://localhost:3000 (default: admin/prom-operator)
```

### Application Logs

```bash
# Centralized logging with ELK or Loki
kubectl logs -f deployment/nwsdb-webapp -n nwsdb | jq .
```

### Health Checks

All pods include liveness and readiness probes:

```bash
# Test health endpoint
kubectl exec <pod-name> -n nwsdb -- curl -s http://localhost:8080/health | jq .
```

---

## Scaling and Performance

### Horizontal Pod Autoscaling

HPAs automatically scale based on metrics:

```bash
# Monitor HPA status
kubectl get hpa -n nwsdb
kubectl describe hpa nwsdb-webapi-hpa -n nwsdb

# Current metrics
kubectl get hpa nwsdb-webapi-hpa -n nwsdb --watch
```

**WebApp HPA**:
- Min replicas: 2
- Max replicas: 10
- Target CPU: 70%
- Target Memory: 80%

**WebApi HPA**:
- Min replicas: 3
- Max replicas: 15
- Target CPU: 60%
- Target Memory: 75%

### Load Testing

```bash
# Install hey for load testing
go install github.com/rakyll/hey@latest

# Generate load
hey -n 10000 -c 100 http://nwsdb.example.com/

# Monitor HPA response
kubectl get hpa -n nwsdb --watch
```

### Resource Optimization

Review and adjust resource requests/limits in deployments:

```yaml
resources:
  requests:
	cpu: 250m      # Minimum guaranteed
	memory: 256Mi
  limits:
	cpu: 500m      # Maximum allowed
	memory: 512Mi
```

---

## Database Persistence

### Using PersistentVolumeClaims (PVC)

For production, update WebApi deployment to use persistent storage:

```yaml
# Uncomment in webapi-deployment.yaml
volumes:
- name: database
  persistentVolumeClaim:
	claimName: nwsdb-database-pvc
```

Create PVC:

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: nwsdb-database-pvc
  namespace: nwsdb
spec:
  accessModes:
	- ReadWriteOnce
  storageClassName: managed-premium  # Or your storage class
  resources:
	requests:
	  storage: 10Gi
```

### External Database

For production, use managed database services:

1. **Azure SQL Database** or **SQL Server**
2. **Azure Database for PostgreSQL**
3. **Amazon RDS**
4. **Google Cloud SQL**

Update connection string in `configmap.yaml` and `secrets.yaml`.

---

## Security Best Practices

✅ **Implemented in manifests**:
- Non-root user (UID 1000)
- Read-only root filesystem
- Security context with dropped capabilities
- Network policies for traffic control
- RBAC with service accounts
- Resource limits and requests

✅ **Additional recommendations**:
- Use secrets encryption at rest (Azure Key Vault)
- Enable Pod Security Policies
- Scan images for vulnerabilities
- Implement network policies
- Enable audit logging
- Use TLS for all communications
- Restrict ingress/egress traffic

---

## Troubleshooting Commands Reference

```bash
# Get overall cluster health
kubectl cluster-info
kubectl get nodes

# Namespace operations
kubectl get ns
kubectl get all -n nwsdb

# Deployment troubleshooting
kubectl describe deployment nwsdb-webapp -n nwsdb
kubectl get events -n nwsdb --sort-by='.lastTimestamp'

# Pod debugging
kubectl logs pod <pod-name> -n nwsdb
kubectl describe pod <pod-name> -n nwsdb
kubectl exec -it <pod-name> -n nwsdb -- bash

# Service debugging
kubectl get svc -n nwsdb
kubectl describe svc nwsdb-webapp -n nwsdb
kubectl get endpoints -n nwsdb

# HPA debugging
kubectl get hpa -n nwsdb
kubectl top nodes
kubectl top pods -n nwsdb
```

---

## Support and Resources

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Azure Kubernetes Service (AKS)](https://docs.microsoft.com/azure/aks/)
- [Docker Documentation](https://docs.docker.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/dotnet/core/)
- [Prometheus Operator](https://prometheus-operator.dev/)

---

**Last Updated**: 2024
**Kubernetes Version**: 1.25+
**.NET Version**: 9.0
