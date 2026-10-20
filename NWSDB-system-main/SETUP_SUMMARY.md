# NWSDB Kubernetes Deployment - Complete Setup Summary

## 📦 What Was Created

### 🐳 Docker Configuration (3 files)

#### Container Images
1. **src/NWSDB.WebApp/Dockerfile**
   - Multi-stage build for Razor Pages frontend
   - .NET 9 SDK → ASP.NET 9 runtime
   - Non-root user, read-only filesystem
   - Health checks included

2. **src/NWSDB.WebApi/Dockerfile**
   - Multi-stage build for ASP.NET Core API backend
   - .NET 9 SDK → ASP.NET 9 runtime
   - Non-root user, read-only filesystem
   - Health checks included

3. **.dockerignore**
   - Optimized Docker build context
   - Excludes unnecessary files

### 🎯 Local Development (1 file)

4. **docker-compose.yml**
   - Complete development environment with both services
   - Persistent volumes for database and logs
   - Health checks
   - Network connectivity between services

### ☸️ Kubernetes Manifests (8 files in k8s/ directory)

#### Infrastructure
5. **k8s/namespace.yaml**
   - Dedicated namespace: `nwsdb`
   - Organized resource isolation

#### Configuration Management
6. **k8s/configmap.yaml**
   - Non-sensitive application settings
   - ASPNETCORE_ENVIRONMENT configuration
   - Feature flags
   - Connection strings template

7. **k8s/secrets.yaml.template**
   - Template for sensitive data
   - JWT secret key
   - Database credentials
   - ACR registry credentials
   - **ACTION**: Copy to secrets.yaml and populate with real values

#### Deployments
8. **k8s/webapp-deployment.yaml**
   - Razor Pages frontend deployment
   - Configuration: 2-10 replicas with autoscaling
   - Rolling updates (maxSurge: 1, maxUnavailable: 0)
   - Health checks (readiness + liveness)
   - Init container to wait for API
   - Security context (non-root, read-only FS)
   - Resource limits: 250m CPU / 256Mi memory (request), 500m / 512Mi (limit)
   - Pod anti-affinity for high availability

9. **k8s/webapi-deployment.yaml**
   - ASP.NET Core API backend deployment
   - Configuration: 3-15 replicas with autoscaling
   - Rolling updates (maxSurge: 1, maxUnavailable: 0)
   - Enhanced health checks
   - Security context (non-root, read-only FS)
   - Resource limits: 300m CPU / 512Mi memory (request), 1000m / 1Gi (limit)
   - Database volume mount ready (currently emptyDir)
   - Pod anti-affinity for high availability

#### Networking & Services
10. **k8s/services.yaml**
	- ServiceAccounts with minimal RBAC
	- ClusterIP services for internal communication
	- LoadBalancer services for external access (optional)
	- Session affinity configuration
	- Health check annotations for monitoring

#### Ingress & Networking
11. **k8s/ingress.yaml**
	- Ingress for external traffic routing
	- Host-based routing for WebApp and WebApi
	- TLS configuration (commented, ready to enable)
	- Security headers
	- CORS configuration
	- NetworkPolicy for traffic control

#### Auto-Scaling
12. **k8s/hpa.yaml**
	- HorizontalPodAutoscaler for WebApp
	  - 2-10 replicas
	  - CPU target: 70%
	  - Memory target: 80%
	- HorizontalPodAutoscaler for WebApi
	  - 3-15 replicas
	  - CPU target: 60%
	  - Memory target: 75%
	- Scaled down/up behavior configuration

### 📚 Documentation (4 files)

13. **KUBERNETES_DEPLOYMENT_GUIDE.md** (Comprehensive Guide)
	- Prerequisites and tool installation
	- Project structure overview
	- Local development setup
	- Container image building and pushing
	- Step-by-step Kubernetes deployment
	- Configuration management strategies
	- Common operations and troubleshooting
	- Monitoring and logging setup
	- Scaling and performance tuning
	- Database persistence options
	- Security best practices

14. **KUBERNETES_QUICK_REFERENCE.md** (Quick Cheat Sheet)
	- Quick start for docker-compose
	- Quick start for container builds
	- Quick start for Kubernetes deployment
	- Configuration update instructions
	- Monitoring and status checks
	- Common commands reference
	- Pre-production checklist

15. **deploy.ps1** (PowerShell Deployment Script)
	- Automated build, push, and deploy
	- Actions: build, push, deploy, all, status, logs, rollback
	- Color-coded output for easy reading
	- Error handling and status reporting
	- ACR authentication
	- Kubernetes resource creation

16. **deploy.sh** (Bash Deployment Script)
	- Same functionality as PowerShell version
	- For Linux/macOS users
	- Bash-compatible with error handling
	- Color-coded output
	- Environment variable support

---

## 🚀 Next Steps

### 1. Prepare Azure Resources
```bash
# Create resource group
az group create --name nwsdb-rg --location eastus

# Create container registry
az acr create --resource-group nwsdb-rg --name nwsdbacr --sku Basic

# Create AKS cluster
az aks create \
  --resource-group nwsdb-rg \
  --name nwsdb-aks \
  --node-count 3 \
  --attach-acr nwsdbacr
```

### 2. Setup Local Development
```bash
# Test locally with Docker Compose
docker-compose up -d
# Access WebApp: http://localhost:5000
# Access WebApi: http://localhost:5001
docker-compose down
```

### 3. Build and Push Container Images
```bash
# Using PowerShell
.\deploy.ps1 -Action build -ImageTag 1.0.0
.\deploy.ps1 -Action push

# Or using Bash
./deploy.sh build prod 1.0.0
./deploy.sh push prod 1.0.0
```

### 4. Prepare Secrets
```bash
# Copy and customize
cp k8s/secrets.yaml.template k8s/secrets.yaml

# Edit with your values
# - Database credentials
# - JWT secret key
# - ACR credentials
# - Email/SMTP settings
```

### 5. Deploy to Kubernetes
```bash
# Using deployment script
.\deploy.ps1 -Action deploy

# Or manual deployment
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/webapi-deployment.yaml
kubectl apply -f k8s/webapp-deployment.yaml
kubectl apply -f k8s/services.yaml
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml
```

### 6. Verify Deployment
```bash
# Check status
kubectl get all -n nwsdb
kubectl get pods -n nwsdb
kubectl get svc -n nwsdb
kubectl get hpa -n nwsdb

# View logs
kubectl logs -f deployment/nwsdb-webapp -n nwsdb
kubectl logs -f deployment/nwsdb-webapi -n nwsdb
```

### 7. Configure DNS and Ingress
```bash
# Get ingress IP
kubectl get ingress -n nwsdb

# Point DNS records to ingress IP
# nwsdb.example.com → <ingress-ip>
# api.nwsdb.example.com → <ingress-ip>

# Update k8s/ingress.yaml with real domains
```

### 8. Enable HTTPS
```bash
# Install cert-manager
helm repo add jetstack https://charts.jetstack.io
helm install cert-manager jetstack/cert-manager -n cert-manager --create-namespace

# Update ingress.yaml to use TLS
# Configure cert-manager ClusterIssuer for Let's Encrypt
```

---

## 📋 File Checklist

### Docker Files
- [ ] src/NWSDB.WebApp/Dockerfile
- [ ] src/NWSDB.WebApi/Dockerfile
- [ ] .dockerignore
- [ ] docker-compose.yml

### Kubernetes Manifests
- [ ] k8s/namespace.yaml
- [ ] k8s/configmap.yaml
- [ ] k8s/secrets.yaml.template (→ copy to secrets.yaml and customize)
- [ ] k8s/webapp-deployment.yaml
- [ ] k8s/webapi-deployment.yaml
- [ ] k8s/services.yaml
- [ ] k8s/ingress.yaml
- [ ] k8s/hpa.yaml

### Documentation
- [ ] KUBERNETES_DEPLOYMENT_GUIDE.md
- [ ] KUBERNETES_QUICK_REFERENCE.md
- [ ] deploy.ps1
- [ ] deploy.sh

### Pre-Deployment Tasks
- [ ] Review and customize configmap.yaml
- [ ] Copy secrets.yaml.template to secrets.yaml
- [ ] Populate secrets.yaml with real credentials
- [ ] Update ingress.yaml domain names
- [ ] Set environment variables (ACR_USERNAME, ACR_PASSWORD)
- [ ] Authenticate with ACR
- [ ] Build Docker images
- [ ] Push images to registry

---

## 🔐 Security Considerations

### Implemented
✅ Non-root containers (UID 1000)
✅ Read-only root filesystems
✅ Dropped capabilities
✅ Resource limits and requests
✅ Health checks
✅ Pod anti-affinity
✅ RBAC with service accounts
✅ NetworkPolicy for traffic control
✅ Security context

### Recommended for Production
- [ ] Use Azure Key Vault for secrets (CSI driver)
- [ ] Enable Pod Security Policies
- [ ] Scan container images for vulnerabilities
- [ ] Use TLS for all communications
- [ ] Implement network policies
- [ ] Enable audit logging
- [ ] Use sealed secrets or similar
- [ ] Regular security updates
- [ ] Monitor for anomalies
- [ ] Backup configurations and data

---

## 📊 Resource Overview

### WebApp Resources
- **CPU Request**: 250m (min guaranteed)
- **CPU Limit**: 500m (max allowed)
- **Memory Request**: 256Mi
- **Memory Limit**: 512Mi
- **Min Replicas**: 2
- **Max Replicas**: 10

### WebApi Resources
- **CPU Request**: 300m (min guaranteed)
- **CPU Limit**: 1000m (max allowed)
- **Memory Request**: 512Mi
- **Memory Limit**: 1Gi
- **Min Replicas**: 3
- **Max Replicas**: 15

---

## 🆘 Troubleshooting Quick Links

- **Pods not starting**: Check image availability, resource limits, and node capacity
- **Service not accessible**: Verify endpoints, network policies, and service selectors
- **Ingress not working**: Ensure ingress controller is installed and DNS is configured
- **Database errors**: Check volume mounts, file permissions, and database schema
- **High latency**: Monitor HPA, adjust resource limits, check node usage
- **Failed deployments**: Check events with `kubectl get events -n nwsdb`

See **KUBERNETES_DEPLOYMENT_GUIDE.md** for detailed troubleshooting.

---

## 📞 Support Resources

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Azure Kubernetes Service (AKS)](https://docs.microsoft.com/azure/aks/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/dotnet/core/)
- [Docker Documentation](https://docs.docker.com/)
- [Helm Documentation](https://helm.sh/docs/)

---

## 📝 Version Information

- **Kubernetes Version**: 1.25+
- **.NET Version**: 9.0
- **Docker**: 20.10+
- **kubectl**: 1.25+

---

**Setup Complete!** 🎉

You now have a production-ready Kubernetes deployment configuration for the NWSDB system. Follow the "Next Steps" section above to get started with deployment.

For more detailed information, refer to **KUBERNETES_DEPLOYMENT_GUIDE.md**.
