# FgaStore

`FgaStore` represents an OpenFGA store managed by the operator.

Stores are the top-level container for authorization models and tuples. Before managing models or tuples, a store must exist.

## Prerequisites

Before creating an `FgaStore` resource:

* An OpenFGA instance must be running and accessible
* A corresponding `FgaConnectionConfig` resource must exist

## Example

```yaml
apiVersion: openfga.sathiyaraman-m.com/v1
kind: FgaStore
metadata:
  name: my-store
spec:
  connectionConfigRef:
    name: my-connection-config
```

## Store Naming

The operator uses the Kubernetes resource name (`metadata.name`) as the store name in OpenFGA.

For example:

```yaml
metadata:
  name: my-store
```

creates an OpenFGA store named `my-store`.

Choose the store name carefully before creating the resource.

## Reconciliation

When an `FgaStore` resource is created, the operator:

1. Connects to the configured OpenFGA instance
2. Creates the store if it does not already exist
3. Records the OpenFGA store identifier in resource status
4. Continuously reconciles the resource state

## Status

The operator updates the resource status with information about the OpenFGA store and reconciliation state.

Example:

```yaml
status:
  storeId: 01JXXXXXXXXXXXXXXX
  conditions:
    ...
```

The exact status fields may vary between releases.

## Deletion Behavior

Deleting an `FgaStore` resource does **not** delete the corresponding OpenFGA store.

This behavior is intentional and helps prevent accidental deletion of authorization data.

If a store is no longer needed, it must be removed manually from OpenFGA.

## Related Resources

* [FgaConnectionConfig](connection-config.md)
* [FgaAuthorizationModel](authorization-model.md)
* [FgaTupleSet](tupleset.md)
* [Concepts](concepts.md)
