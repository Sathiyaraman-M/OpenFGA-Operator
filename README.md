# OpenFGA K8s Operator

Kubernetes operator for managing OpenFGA stores, authorization models, and tuples using Kubernetes custom resources.

> [!WARNING]
> This operator is currently in alpha. APIs and CRDs may change between releases.

> [!NOTE]
> This is a community project and is not affiliated with or endorsed by OpenFGA.

## Why?

> Inspired by Terraform-style reconciliation for OpenFGA resources, but designed to fit into Kubernetes-native workflows.

OpenFGA artifacts such as authorization models and bootstrap tuples are often managed separately from application deployments through custom scripts or Terraform pipelines.

This operator allows you to manage OpenFGA resources declaratively using Kubernetes manifests, alongside your application deployments. 

If you are already using any GitOps workflow, it fits right in. The operator continuously reconciles the desired state defined in your manifests, with the OpenFGA instance.

## Features

- Create and manage OpenFGA stores using Kubernetes custom resources
- Upload and version OpenFGA authorization models
- Manage bootstrap tuples declaratively
- Safe tuple deletion through operator-owned state tracking (Runtime tuples are untouched)
- Continuous reconciliation of desired state
- GitOps-friendly

## Custom Resources

The operator provides the following custom resources:

| Resource                | Purpose                                                  |
| ----------------------- | -------------------------------------------------------- |
| `FgaConnectionConfig`   | Defines how the operator connects to an OpenFGA instance |
| `FgaStore`              | Represents an OpenFGA store                              |
| `FgaAuthorizationModel` | Manages authorization models within a store              |
| `FgaTupleSet`           | Manages operator-owned tuples within a store             |

These resources can be combined to declaratively manage OpenFGA artifacts using Kubernetes manifests.

See the [`examples/`](examples/) directory for a complete working configuration.

## Installation

Install the operator using Helm:

```bash
helm install openfga-operator oci://ghcr.io/sathiyaraman-m/charts/openfga-operator \
  --version 1.0.0-alpha1 \
  --namespace openfga-system \
  --create-namespace
```

> [!NOTE]
> The operator doesn't provision OpenFGA instances. You must have an OpenFGA instance running and accessible from your Kubernetes cluster before using the operator.

## Quick Start

The repository contains a complete example showing how to:

- Configure a connection to an OpenFGA instance
- Create an OpenFGA store
- Upload an authorization model
- Manage bootstrap tuples

See the examples directory for detailed instructions and manifests:

```
examples/
├── model.fga                   # Example authorization model for reference
├── connectionConfig.yaml
├── store.yaml
├── model.yaml
├── tupleset.yaml
└── README.md
```

Apply the example resources after installing the operator:

```bash
kubectl apply -f examples/
```

See [`examples/README.md`](examples/README.md) for more info.

## Documentation

Additional documentation is available in the [docs](docs/) directory.

## Development

For local development, testing, and building from source, see the development documentation.

## Contributing

Contributions, bug reports, and feature requests are welcome.

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Licensed under the MIT License. See [LICENSE](LICENSE).
