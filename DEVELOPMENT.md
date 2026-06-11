# Development

This document describes how to set up a local development environment for OpenFGA K8s Operator.

## Prerequisites

The following tools are required:

* .NET 10 SDK
* Docker
* kubectl
* kind
* Helm

## Create a Local Cluster

Create a Kind cluster:

```bash
kind create cluster
```

## Install OpenFGA

Install OpenFGA into the cluster using Helm:

```bash
helm repo add openfga https://openfga.github.io/helm-charts
helm repo update

helm install openfga openfga/openfga \
  --namespace openfga \
  --create-namespace
```

Verify the installation:

```bash
kubectl get pods -n openfga
```

## Build the Project

From the repository root:

```bash
dotnet build
```

## Build the Operator Image

Build the operator container image:

```bash
docker build -t openfga-operator:dev .
```

Load the image into the Kind cluster:

```bash
kind load docker-image openfga-operator:dev
```

## Generate Kubernetes Manifests

Install the KubeOps CLI:

```bash
dotnet tool install --global KubeOps.Cli
```

Generate manifests:

```bash
cd src/OpenFga.KubeOps

kubeops generate operator openfga-operator ./OpenFga.KubeOps.csproj \
  --out Outputs \
  --docker-image openfga-operator \
  --docker-image-tag dev \
  --clear-out
```

## Deploy the Operator

Apply the generated manifests:

```bash
kubectl apply -k src/OpenFga.KubeOps/Outputs
```

Verify the operator is running:

```bash
kubectl get pods -A
```

## Deploy Example Resources

Apply the example manifests:

```bash
kubectl apply -f examples/
```

Check the created resources:

```bash
kubectl get fgaconnectionconfigs
kubectl get fgastores
kubectl get fgaauthorizationmodels
kubectl get fgatuplesets
```

Inspect resource status:

```bash
kubectl describe fgastore <name>
```

## Cleanup

Remove the example resources:

```bash
kubectl delete -f examples/
```

> [!NOTE]
> Removing the store resource will not delete the underlying OpenFGA store or its data, but it will remove the Kubernetes resource and stop the operator from managing it. Also, the same applies for the authorization model, unless the store is manually deleted.

Remove the operator:

```bash
kubectl delete -k src/OpenFga.KubeOps/Outputs
```

Delete the Kind cluster:

```bash
kind delete cluster
```

## Development vs Release Artifacts

The project maintains a Helm chart for installation by end users.

Unfortunately, the KubeOps CLI doesn't provide a way to generate the Helm Chart. So, the helm chart for end users is assembled by hand, from the generated manifests.

If you make changes to the operator, you will need to regenerate the manifests and then update the Helm chart accordingly.
