# OpenFGA KubeOps Operator

This repository contains an OpenFGA operator implemented with KubeOps. This operator is to manage OpenFGA artifacts in Kubernetes way.

> [!NOTE]
> This operator doesn't deploy or manage the OpenFGA instance itself. It assumes you have an OpenFGA instance running and accessible from your Kubernetes cluster. The operator focuses on managing OpenFGA stores, models, and tuples as Kubernetes custom resources.

## Prerequisites

- .NET 10 SDK
- Docker
- kubectl
- helm (Optional)
- A running `kind` cluster with OpenFGA installed

## Install an OpenFGA instance in your cluster

Run the following command to install OpenFGA in your cluster using Helm:

```sh
helm repo add openfga https://openfga.github.io/helm-charts
helm repo update
helm install openfga openfga/openfga --namespace openfga --create-namespace
```

## Build & Deploy (local kind cluster)

Run these commands from the repository root:

1. Install the KubeOps CLI (global dotnet tool)

```sh
dotnet tool install --global KubeOps.Cli
```

2. Build the project

```sh
dotnet build
```

3. Build the operator Docker image

```sh
docker build -t openfga-operator:v1alpha .
```

4. Load the image into your `kind` cluster

```sh
kind load docker-image openfga-operator:v1alpha
```

5. Generate operator manifests with the KubeOps CLI

```sh
cd src/OpenFga.KubeOps
kubeops generate operator openfga-operator ./OpenFga.KubeOps.csproj \
  --out Outputs \
  --docker-image openfga-operator \
  --docker-image-tag v1alpha \
  --clear-out
```

This generates YAML/Kustomize outputs in `src/OpenFga.KubeOps/Outputs`.

6. Apply the generated manifests

```sh
kubectl apply -k Outputs
```

7. Return to repository root and apply example CRs

```sh
cd ../..
kubectl apply -f examples
```

## What these steps do

- Build the .NET operator binary (`dotnet build`).
- Package the operator into a Docker image and load that image into your `kind` cluster so Pods can use it without pushing to a registry.
- Use the KubeOps CLI to generate Kubernetes manifests (Deployment, RBAC, CRDs, etc.) tailored to the project and the specified Docker image/tag.
- Apply the manifests and example custom resources to create the operator and example workloads in the cluster.

## Cleanup

To remove the operator and example resources:

```sh
kubectl delete -k src/OpenFga.KubeOps/Outputs
kubectl delete -f examples
```
