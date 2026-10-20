# NWSDB Kubernetes Deployment - Quick Reference

## 📋 Files Created

### Docker Configuration
- **src/NWSDB.WebApp/Dockerfile** - Multi-stage build for Razor Pages frontend
- **src/NWSDB.WebApi/Dockerfile** - Multi-stage build for ASP.NET Core API
- **.dockerignore** - Optimized Docker build context
- **docker-compose.yml** - Local development environment

### Kubernetes Manifests (k8s/)
- **namespace.yaml** - NWSDB namespace for resource isolation
- **configmap.yaml** - Non-sensitive application configuration
- **secrets.yaml.template** - Template for sensitive data (customize before deployment)
- **webapp-deployment.yaml** - Razor Pages frontend (2-10 replicas)
- **webapi-deployment.yaml** - ASP.NET Core API backend (3-15 replicas)
- **services.yaml** - Service definitions, RBAC, and ServiceAccounts
- **ingress.yaml** - Ingress routing and NetworkPolicy
- **hpa.yaml** - Horizontal Pod Autoscaler for both services

### Documentation
- **KUBERNETES_DEPLOYMENT_GUIDE.md** - Complete deployment guide with troubleshooting

---

## 🚀 Quick Start

### 1. Local Development (Docker Compose)
```bash
cd /path/to/NWSDB-system-main
docker-compose up -d

# Access applications
# WebApp: http://localhost:5000
# WebApi: http://localhost:5001
```

### 2. Build Container Images for Azure
```bash
az acr login --name nwsdbacr

# Build images
az acr build --registry nwsdbacr --image nwsdb-webapp:1.0.0 --file src/NWSDB.WebApp/Dockerfile .
az acr build --registry nwsdbacr --image nwsdb-webapi:1.0.0 --file src/NWSDB.WebApi/Dockerfile .
```

### 3. Deploy to Kubernetes (AKS)
```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Create ACR secret
kubectl create secret docker-registry acr-secret \
  --docker-server=nwsdbacr.azurecr.io \
  --docker-username=$ACR_USERNAME \
  --docker-password=$ACR_PASSWORD \
  -n nwsdb

# Copy and edit secrets template
cp k8s/secrets.yaml.template k8s/secrets.yaml
# Edit k8s/secrets.yaml with your actual credentials

# Deploy all resources
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/webapi-deployment.yaml
kubectl apply -f k8s/webapp-deployment.yaml
kubectl apply -f k8s/services.yaml
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml

# Verify deployment
kubectl get all -n nwsdb
```

---

## 🔧 Configuration

### Update Settings
Edit `k8s/configmap.yaml` for application settings:
- ASPNETCORE_ENVIRONMENT
- Logging levels
- Feature flags
- Database connection strings

### Update Secrets
Edit `k8s/secrets.yaml` for sensitive data:
- JWT secret key
- Database credentials
- SMTP settings
- ACR credentials

**Security**: Base64 encode secrets and consider using:
- Azure Key Vault with CSI driver
- Sealed Secrets for GitOps
- HashiCorp Vault

---

## 📊 Monitoring

### Check Status
```bash
kubectl get pods -n nwsdb
kubectl get hpa -n nwsdb
kubectl get svc -n nwsdb
kubectl get ingress -n nwsdb
```

### View Logs
```bash
kubectl logs -f deployment/nwsdb-webapp -n nwsdb
kubectl logs -f deployment/nwsdb-webapi -n nwsdb
```

### Health Checks
```bash
# WebApp
kubectl exec <webapp-pod> -n nwsdb -- curl http://localhost:8080/

# WebApi
kubectl exec <webapi-pod> -n nwsdb -- curl http://localhost:8080/health
```

---

## 🔄 Autoscaling Configuration

### WebApp
- **Min Replicas**: 2
- **Max Replicas**: 10
- **CPU Target**: 70%
- **Memory Target**: 80%

### WebApi
- **Min Replicas**: 3
- **Max Replicas**: 15
- **CPU Target**: 60%
- **Memory Target**: 75%

Monitor HPA:
```bash
kubectl get hpa -n nwsdb -w
```

---

## 🔐 Security Features

✅ **Implemented**:
- Non-root containers (UID 1000)
- Read-only root filesystem
- Resource limits and requests
- Security context with dropped capabilities
- RBAC with service accounts
- Network policies
- Health checks (liveness & readiness)

---

## 📈 Scaling & Performance

### Manual Scaling
```bash
kubectl scale deployment nwsdb-webapp --replicas=5 -n nwsdb
kubectl scale deployment nwsdb-webapi --replicas=8 -n nwsdb
```

### Update Image
```bash
kubectl set image deployment/nwsdb-webapp \
  webapp=nwsdbacr.azurecr.io/nwsdb-webapp:1.1.0 \
  -n nwsdb
```

### Rolling Restart
```bash
kubectl rollout restart deployment/nwsdb-webapp -n nwsdb
```

### Rollback
```bash
kubectl rollout undo deployment/nwsdb-webapp -n nwsdb
```

---

## 🗄️ Database Persistence

### Current (Development)
Uses `emptyDir` volumes - data lost when pod restarts.

### Production Setup
Update `webapi-deployment.yaml` to use PersistentVolumeClaim:

```yaml
volumes:
- name: database
  persistentVolumeClaim:
	claimName: nwsdb-database-pvc
```

Or use managed database services:
- Azure SQL Database
- Azure Database for PostgreSQL
- Amazon RDS
- Google Cloud SQL

---

## 🔗 Ingress Configuration

### Edit Domain Names
Update `k8s/ingress.yaml`:
- Replace `nwsdb.example.com` with your domain
- Replace `api.nwsdb.example.com` with your API domain

### Enable HTTPS
Uncomment TLS section and configure certificate:
```bash
# Using cert-manager with Let's Encrypt
helm repo add jetstack https://charts.jetstack.io
helm install cert-manager jetstack/cert-manager -n cert-manager --create-namespace
```

---

## 🆘 Troubleshooting

See **KUBERNETES_DEPLOYMENT_GUIDE.md** for detailed troubleshooting:

### Common Issues
- Pod not starting → Check image availability and resource limits
- Service not accessible → Verify endpoints and network policies
- Ingress not working → Check ingress controller and DNS
- Database errors → Verify volume mounts and permissions

### Debug Commands
```bash
# Pod details
kubectl describe pod <pod-name> -n nwsdb

# Events
kubectl get events -n nwsdb --sort-by='.lastTimestamp'

# Execute shell
kubectl exec -it <pod-name> -n nwsdb -- bash

# Port forward
kubectl port-forward svc/nwsdb-webapp 8080:8080 -n nwsdb
```

---

## 📞 Support

For detailed instructions, see:
- **KUBERNETES_DEPLOYMENT_GUIDE.md** - Complete guide
- [Kubernetes Docs](https://kubernetes.io/docs/)
- [AKS Docs](https://docs.microsoft.com/azure/aks/)
- [ASP.NET Core Docs](https://docs.microsoft.com/dotnet/core/)

---

## ✅ Environment Checklist

Before deploying to production:

- [ ] Update image tags in Kubernetes manifests
- [ ] Configure real database (not SQLite)
- [ ] Set strong secrets in secrets.yaml
- [ ] Configure ingress with real domain names
- [ ] Enable TLS/HTTPS
- [ ] Set up monitoring (Prometheus/Grafana)
- [ ] Configure logging (ELK/Loki)
- [ ] Test health endpoints
- [ ] Load test the system
- [ ] Set up backup strategy
- [ ] Configure auto-scaling metrics
- [ ] Enable RBAC and network policies
- [ ] Scan container images for vulnerabilities
- [ ] Set up CI/CD pipeline

---

**Ready to deploy!** 🚀
