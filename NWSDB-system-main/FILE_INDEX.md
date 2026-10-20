# 📦 NWSDB Kubernetes Cloud Deployment - File Index

## Overview
Complete Kubernetes deployment setup for NWSDB system (WebApp + WebApi) on .NET 9.

---

## 🚀 START HERE

### For Quick Setup
1. Read: **SETUP_SUMMARY.md** - Overview of all created files
2. Read: **KUBERNETES_QUICK_REFERENCE.md** - Quick commands
3. Run: Interactive deployment script (deploy.ps1 or deploy.sh)

### For Detailed Instructions
1. Read: **KUBERNETES_DEPLOYMENT_GUIDE.md** - Complete guide with troubleshooting

---

## 📁 File Structure

```
NWSDB-system-main/
├── 📄 .dockerignore                          # Docker build optimization
├── 📄 docker-compose.yml                     # Local dev environment
├── 🐚 deploy.ps1                              # PowerShell deployment script
├── 🐚 deploy.sh                               # Bash deployment script
│
├── src/
│   ├── NWSDB.WebApp/
│   │   └── 🐳 Dockerfile                     # WebApp container image
│   ├── NWSDB.WebApi/
│   │   └── 🐳 Dockerfile                     # WebApi container image
│   ├── NWSDB.Core/
│   ├── NWSDB.Data/
│   └── NWSDB.Services/
│
├── k8s/
│   ├── 📋 namespace.yaml                     # K8s namespace "nwsdb"
│   ├── 📋 configmap.yaml                     # Non-sensitive config
│   ├── 📋 secrets.yaml.template              # Sensitive config (COPY & CUSTOMIZE)
│   ├── 📋 webapp-deployment.yaml             # WebApp deployment (2-10 replicas)
│   ├── 📋 webapi-deployment.yaml             # WebApi deployment (3-15 replicas)
│   ├── 📋 services.yaml                      # Services & RBAC
│   ├── 📋 ingress.yaml                       # Ingress & NetworkPolicy
│   └── 📋 hpa.yaml                           # Horizontal Pod Autoscaler
│
├── 📚 SETUP_SUMMARY.md                       # Overview & checklist (THIS IS KEY READING)
├── 📚 KUBERNETES_QUICK_REFERENCE.md          # Cheat sheet & common commands
└── 📚 KUBERNETES_DEPLOYMENT_GUIDE.md         # Detailed guide & troubleshooting
```

---

## 📖 Documentation Files

### 1. **SETUP_SUMMARY.md** ⭐ START HERE
- **Purpose**: Overview of all created files and next steps
- **Read Time**: 5-10 minutes
- **Contents**:
  - Complete file listing with descriptions
  - What each file does
  - Next steps checklist
  - Resource overview
  - Security considerations
  - Pre-production checklist
- **Best for**: First-time readers, project overview

### 2. **KUBERNETES_QUICK_REFERENCE.md** ⭐ COMMAND REFERENCE
- **Purpose**: Quick reference for common tasks
- **Read Time**: 3-5 minutes
- **Contents**:
  - Quick start for docker-compose
  - Container build commands
  - Kubernetes deployment commands
  - Configuration management
  - Common debugging commands
  - Pre-deployment checklist
- **Best for**: Experienced users, quick lookups

### 3. **KUBERNETES_DEPLOYMENT_GUIDE.md** 📖 COMPLETE GUIDE
- **Purpose**: Comprehensive deployment guide
- **Read Time**: 20-30 minutes
- **Contents**:
  - Prerequisites and installations
  - Local development setup
  - Container image building
  - Step-by-step Kubernetes deployment
  - Configuration management
  - Common operations
  - Troubleshooting section
  - Monitoring and logging
  - Scaling and performance
  - Database persistence
  - Security best practices
- **Best for**: Complete reference, troubleshooting

---

## 🐳 Docker Files

### **src/NWSDB.WebApp/Dockerfile**
- Multi-stage build (SDK → Runtime)
- .NET 9 base image
- Non-root user (UID 1000)
- Read-only root filesystem
- Health checks included
- ~150MB final image size

### **src/NWSDB.WebApi/Dockerfile**
- Multi-stage build (SDK → Runtime)
- .NET 9 base image
- Non-root user (UID 1000)
- Read-only root filesystem
- Health checks included
- ~150MB final image size

### **.dockerignore**
- Optimizes build context
- Excludes: .git, bin, obj, node_modules, etc.
- Reduces build time and image size

### **docker-compose.yml**
- Local development environment
- Both WebApp and WebApi services
- Persistent volumes for data
- Health checks
- Network connectivity
- Environment variables
- Easy to run: `docker-compose up -d`

---

## ☸️ Kubernetes Manifest Files (k8s/)

### **namespace.yaml**
- Creates `nwsdb` namespace
- Namespace labels for identification
- ~5 lines

### **configmap.yaml**
- Non-sensitive configuration values
- Application environment variables
- Logging levels
- Feature flags
- Connection string templates
- ~30 lines

### **secrets.yaml.template** ⚠️ IMPORTANT
- **TEMPLATE ONLY** - Must copy and customize
- Contains placeholders for:
  - JWT secret key
  - Database credentials
  - ACR registry credentials
  - SMTP settings
- Action: `cp k8s/secrets.yaml.template k8s/secrets.yaml` + edit values
- ~40 lines

### **webapp-deployment.yaml**
- Razor Pages frontend deployment
- 2-10 replicas (auto-scaling)
- Health checks (liveness + readiness)
- Resource limits: 250m CPU / 256Mi memory (request)
- Init container waits for API
- Pod anti-affinity for HA
- ~150 lines

### **webapi-deployment.yaml**
- ASP.NET Core API deployment
- 3-15 replicas (auto-scaling)
- Enhanced health checks
- Resource limits: 300m CPU / 512Mi memory (request)
- Database volume support
- Pod anti-affinity for HA
- ~150 lines

### **services.yaml**
- ServiceAccounts with RBAC
- ClusterIP services (internal)
- LoadBalancer services (external, optional)
- Session affinity
- Health check annotations
- ~120 lines

### **ingress.yaml**
- Ingress controller configuration
- Host-based routing (nwsdb.example.com, api.nwsdb.example.com)
- TLS configuration (commented, ready to enable)
- Security headers
- CORS configuration
- NetworkPolicy for traffic control
- ~100 lines

### **hpa.yaml**
- HorizontalPodAutoscaler for WebApp (2-10 replicas)
- HorizontalPodAutoscaler for WebApi (3-15 replicas)
- CPU and memory metrics
- Scale-up/down behavior configuration
- ~90 lines

---

## 🐚 Deployment Scripts

### **deploy.ps1** (PowerShell)
- Cross-platform automation script
- Actions: build, push, deploy, all, status, logs, rollback
- Usage: `.\deploy.ps1 -Action deploy -Environment prod -ImageTag 1.0.0`
- Features:
  - ACR authentication
  - Docker image building
  - Kubernetes deployment
  - Status checking
  - Error handling
  - Color-coded output

### **deploy.sh** (Bash)
- Linux/macOS deployment script
- Same functionality as PowerShell version
- Usage: `./deploy.sh deploy prod 1.0.0`
- Features:
  - ACR authentication
  - Docker image building
  - Kubernetes deployment
  - Status checking
  - Error handling
  - Color-coded output

---

## 🔄 Deployment Workflow

```
1. Local Development
   └─> docker-compose up

2. Build Container Images
   └─> docker build (both WebApp & WebApi)

3. Push to Azure Container Registry (ACR)
   └─> docker push

4. Deploy to Kubernetes
   ├─> Create namespace
   ├─> Create secrets
   ├─> Deploy ConfigMap
   ├─> Deploy WebApi
   ├─> Deploy WebApp
   ├─> Create Services
   ├─> Create Ingress
   └─> Enable HPA

5. Verify & Monitor
   └─> Watch pods, check logs, verify health
```

---

## ✅ Pre-Deployment Checklist

- [ ] Have Azure subscription and ACR setup
- [ ] Have Kubernetes cluster (AKS) ready
- [ ] Read SETUP_SUMMARY.md
- [ ] Read KUBERNETES_QUICK_REFERENCE.md
- [ ] Copy secrets.yaml.template to secrets.yaml
- [ ] Populate secrets.yaml with real values:
  - [ ] JWT secret key
  - [ ] Database credentials
  - [ ] ACR username and password
  - [ ] SMTP settings (if needed)
- [ ] Update k8s/ingress.yaml domain names
- [ ] Set environment variables:
  - [ ] ACR_USERNAME
  - [ ] ACR_PASSWORD
- [ ] Build and push container images
- [ ] Run deployment script or kubectl apply

---

## 🚀 Quick Commands

```bash
# Local development
docker-compose up -d
docker-compose logs -f
docker-compose down

# Build images
docker build -f src/NWSDB.WebApp/Dockerfile -t nwsdbacr.azurecr.io/nwsdb-webapp:1.0.0 .
docker build -f src/NWSDB.WebApi/Dockerfile -t nwsdbacr.azurecr.io/nwsdb-webapi:1.0.0 .

# Push to ACR
docker push nwsdbacr.azurecr.io/nwsdb-webapp:1.0.0
docker push nwsdbacr.azurecr.io/nwsdb-webapi:1.0.0

# Deploy to Kubernetes
# Option 1: Using script
.\deploy.ps1 -Action deploy
# Option 2: Using kubectl
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/webapi-deployment.yaml
kubectl apply -f k8s/webapp-deployment.yaml
kubectl apply -f k8s/services.yaml
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml

# Check status
kubectl get all -n nwsdb
kubectl get pods -n nwsdb
kubectl get svc -n nwsdb

# View logs
kubectl logs -f deployment/nwsdb-webapp -n nwsdb
kubectl logs -f deployment/nwsdb-webapi -n nwsdb
```

---

## 🎯 Resource Specifications

### WebApp (Frontend)
- **Min Replicas**: 2
- **Max Replicas**: 10
- **CPU Request**: 250m
- **CPU Limit**: 500m
- **Memory Request**: 256Mi
- **Memory Limit**: 512Mi
- **Auto-scale at**: CPU 70%, Memory 80%

### WebApi (Backend)
- **Min Replicas**: 3
- **Max Replicas**: 15
- **CPU Request**: 300m
- **CPU Limit**: 1000m
- **Memory Request**: 512Mi
- **Memory Limit**: 1Gi
- **Auto-scale at**: CPU 60%, Memory 75%

---

## 🔐 Security Features

✅ **Implemented**:
- Non-root user containers (UID 1000)
- Read-only root filesystems
- Dropped capabilities
- Resource limits/requests
- Health checks (liveness + readiness)
- Pod anti-affinity
- RBAC with ServiceAccounts
- NetworkPolicy for traffic control
- Security context

✅ **Recommended for Production**:
- Azure Key Vault integration
- Pod Security Policies
- Image vulnerability scanning
- TLS encryption
- Audit logging
- Regular security updates

---

## 📊 What You Get

This complete setup provides:

✅ **Development**: Docker Compose for local testing
✅ **Container Images**: Multi-stage optimized Dockerfiles
✅ **Kubernetes Deployment**: Production-ready manifests
✅ **High Availability**: Pod anti-affinity, multiple replicas
✅ **Auto-Scaling**: Horizontal Pod Autoscaler configured
✅ **Health Checks**: Liveness and readiness probes
✅ **Security**: Non-root users, RBAC, NetworkPolicy
✅ **Monitoring**: Prometheus scrape annotations
✅ **Documentation**: Comprehensive guides and references
✅ **Automation**: PowerShell and Bash deployment scripts

---

## 📞 Getting Help

1. **Quick Questions**: Check KUBERNETES_QUICK_REFERENCE.md
2. **Setup Issues**: Check SETUP_SUMMARY.md checklist
3. **Detailed Guide**: Read KUBERNETES_DEPLOYMENT_GUIDE.md
4. **Specific Problems**: See "Troubleshooting" section in deploy guide
5. **Common Commands**: Use deployment scripts

---

## 🎓 Learn More

- [Kubernetes Official Docs](https://kubernetes.io/docs/)
- [Azure Kubernetes Service (AKS)](https://docs.microsoft.com/azure/aks/)
- [ASP.NET Core on Kubernetes](https://docs.microsoft.com/dotnet/architecture/containerized-lifecycle/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Helm Package Manager](https://helm.sh/) - For advanced deployments

---

**Status**: ✅ Ready for Deployment

All files created and configured. Follow SETUP_SUMMARY.md for next steps!

---

*Generated for .NET 9 NWSDB System with Razor Pages and ASP.NET Core API*
*Kubernetes 1.25+ | Docker 20.10+ | Azure Recommended*
