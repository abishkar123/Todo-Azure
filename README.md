# TodoApp - ASP.NET Core 9 MVC with MongoDB Atlas

A simple Todo List application built with ASP.NET Core MVC 9 and MongoDB Atlas, featuring a clean Bootstrap 5 UI.

## Project Structure

```
test-todo/
├── README.md
├── test-todo.sln
└── src/
    └── TodoApp/
        ├── Controllers/      # MVC Controllers
        ├── Models/           # Data models
        ├── Repositories/     # Data access layer
        ├── Views/            # Razor views
        ├── wwwroot/          # Static files (CSS, JS)
        ├── aks/              # Kubernetes manifests
        ├── Dockerfile        # Container definition
        ├── Program.cs        # Application entry point
        └── appsettings.json  # Configuration
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A MongoDB Atlas account (free tier works fine)
- (For AKS deployment) Azure CLI, kubectl, Docker

## Configure MongoDB Atlas

1. Create a cluster in [MongoDB Atlas](https://cloud.mongodb.com/).
2. Under **Database Access**, create a user with read/write permissions.
3. Under **Network Access**, allow your IP (or `0.0.0.0/0` for development).
4. Copy your **Connection String** (choose "Connect your application" > C# / .NET).

Open `src/TodoApp/appsettings.json` and replace the placeholder:

```json
"MongoSettings": {
  "ConnectionString": "mongodb+srv://<user>:<password>@cluster.mongodb.net/?retryWrites=true&w=majority",
  "DatabaseName": "TodoDb",
  "CollectionName": "Todos"
}
```

## Run the Application

```bash
cd src/TodoApp
dotnet restore
dotnet run
```

The app will start at `http://localhost:5000` (or the port shown in the console).

## Features

- Create, read, update, delete todos (CRUD)
- Mark todos as Done/Open
- Responsive Bootstrap 5 UI

---

## Kubernetes Deployment (AKS)

### Infrastructure (from Terraform)

| Resource           | Value                   |
| ------------------ | ----------------------- |
| AKS Cluster        | `aks-app-aue`           |
| Resource Group     | `rg-aks-test-project`   |
| ACR                | `bdlndacr01.azurecr.io` |
| Location           | Australia East          |
| Kubernetes Version | 1.33.5                  |

### Setup Steps

#### 1. Get AKS Credentials

```bash
az aks get-credentials --resource-group rg-aks-test-project --name aks-app-aue
```

#### 2. Attach ACR to AKS (if not done via Terraform)

```bash
az aks update --resource-group rg-aks-test-project --name aks-app-aue --attach-acr bdlndacr01
```

#### 3. Build and Push Docker Image

```bash
# Login to ACR
az acr login --name bdlndacr01

# Build and push image
cd src/TodoApp
docker build -t bdlndacr01.azurecr.io/todoapp:latest .
docker push bdlndacr01.azurecr.io/todoapp:latest
```

#### 4. Update Secrets

Edit `src/TodoApp/aks/secret.yaml` with your actual MongoDB connection string:

```yaml
stringData:
  MongoSettings__ConnectionString: "your-actual-connection-string"
  MongoSettings__DatabaseName: "TodoDb"
  MongoSettings__CollectionName: "Todos"
```

> **Security Note**: For production, consider using Azure Key Vault with the CSI Secrets Store Driver instead of Kubernetes secrets.

#### 5. Deploy to AKS

**Option A: Using Kustomize**

```bash
kubectl apply -k src/TodoApp/aks/
```

**Option B: Apply manifests individually**

```bash
kubectl apply -f src/TodoApp/aks/namespace.yaml
kubectl apply -f src/TodoApp/aks/secret.yaml
kubectl apply -f src/TodoApp/aks/deployment.yaml
kubectl apply -f src/TodoApp/aks/service.yaml
kubectl apply -f src/TodoApp/aks/loadbalancer-service.yaml
kubectl apply -f src/TodoApp/aks/hpa.yaml
```

### AKS Manifests Overview

| File                        | Description                               |
| --------------------------- | ----------------------------------------- |
| `namespace.yaml`            | Creates the `todoapp` namespace           |
| `secret.yaml`               | MongoDB connection secrets                |
| `deployment.yaml`           | App deployment with 2 replicas            |
| `service.yaml`              | ClusterIP service for internal access     |
| `loadbalancer-service.yaml` | Internal load balancer (private AKS)      |
| `hpa.yaml`                  | Horizontal Pod Autoscaler (2-10 replicas) |
| `kustomization.yaml`        | Kustomize configuration                   |

### Verify Deployment

```bash
# Check pods
kubectl get pods -n todoapp

# Check services
kubectl get svc -n todoapp

# Get load balancer IP
kubectl get svc todoapp-loadbalancer -n todoapp -o jsonpath='{.status.loadBalancer.ingress[0].ip}'

# View logs
kubectl logs -l app=todoapp -n todoapp

# Describe deployment
kubectl describe deployment todoapp -n todoapp
```

### Scaling

The HPA will automatically scale between 2-10 replicas based on CPU/memory utilization.

Manual scaling:

```bash
kubectl scale deployment todoapp -n todoapp --replicas=5
```

### Cleanup

```bash
kubectl delete -k src/TodoApp/aks/
# or
kubectl delete namespace todoapp
```
