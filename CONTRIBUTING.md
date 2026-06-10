# Contributing

Thank you for your interest in contributing to OpenFGA K8s Operator.

## Reporting Issues

If you encounter a bug, have a feature request, or have questions about the project, please open an issue first.

When reporting bugs, include:

* Operator version
* Kubernetes version
* OpenFGA version
* Relevant manifests
* Logs or error messages

## Development Setup

See [DEVELOPMENT.md](DEVELOPMENT.md) for instructions on setting up a local development environment, building the project, and testing changes.

## Pull Requests

Before submitting a pull request:

* Ensure the project builds successfully
* Update documentation when introducing user-facing changes
* Add or update examples when introducing new functionality

Please keep pull requests focused on a single change whenever possible. 

There are no tests in the project yet, so any PR that adds tests will be appreciated. Until we have a test suite, please do test your changes manually in a local cluster before submitting a PR.

## Development Principles

The project aims to provide a Kubernetes-native and GitOps-friendly experience for managing OpenFGA resources.

When contributing, prefer solutions that:

* Follow Kubernetes reconciliation patterns
* Maintain declarative desired-state management
* Preserve safe ownership boundaries for managed tuples
* Avoid impacting runtime-managed OpenFGA data

## License

By contributing to this project, you agree that your contributions will be licensed under the project's MIT License.
