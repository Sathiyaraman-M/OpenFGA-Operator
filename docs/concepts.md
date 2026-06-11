# Concepts

This document describes the core concepts and reconciliation model used by OpenFGA K8s Operator.

## Overview

OpenFGA K8s Operator manages OpenFGA artifacts using Kubernetes custom resources.

The operator continuously reconciles the desired state defined in Kubernetes with the actual state in OpenFGA.

```text
Kubernetes Resources
        ↓
OpenFGA K8s Operator
        ↓
      OpenFGA
```

The goal is to allow OpenFGA resources to be managed using the same GitOps workflow as application deployments.

## Reconciliation

The operator follows the standard Kubernetes reconciliation pattern.

During reconciliation, the operator:

1. Reads the desired state from Kubernetes resources
2. Reads the current state from OpenFGA
3. Calculates the required changes
4. Applies those changes
5. Updates resource status

This process repeats continuously to ensure that OpenFGA remains aligned with the declared Kubernetes resources.

## Managed Resources

The operator currently manages:

* OpenFGA Stores
* OpenFGA Authorization Models
* OpenFGA Tuples

Each resource type has different lifecycle characteristics.

## Store Lifecycle

`FgaStore` resources create and reference stores in OpenFGA.

Deleting an `FgaStore` resource does not delete the corresponding OpenFGA store.

This behavior is intentional and helps prevent accidental data loss.

Stores must be deleted manually from OpenFGA when no longer required.

## Authorization Models

OpenFGA authorization models are immutable.

When an `FgaAuthorizationModel` resource changes, the operator uploads a new model version to the store.

Deleting an `FgaAuthorizationModel` resource does not remove previously uploaded models from OpenFGA.

This behavior matches OpenFGA's model lifecycle.

## Tuple Ownership

OpenFGA may contain tuples from multiple sources.

Examples include:

### Operator-managed tuples

* Bootstrap permissions
* Initial role assignments
* GitOps-managed relationships

### Runtime-managed tuples

* Application-created relationships
* User-generated sharing permissions
* Dynamically created access grants

The operator must never interfere with runtime-managed tuples.

## Safe Tuple Deletion

The operator only deletes tuples that it previously created and manages.

This is achieved by tracking operator-owned state and reconciling against that state during updates.

As a result:

* Desired tuples are created when missing
* Removed tuples are deleted
* Unrelated runtime-generated tuples are preserved

This allows declarative tuple management without risking accidental removal of application-managed data.

## Status

Each resource exposes status information describing the current reconciliation state.

Depending on the resource type, status may include:

* Reconciliation conditions
* OpenFGA identifiers
* Current model identifiers
* Managed tuple metadata

Status fields should be considered operator-owned and are updated automatically during reconciliation.
