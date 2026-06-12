# FgaTupleSet

`FgaTupleSet` manages a collection of tuples within an OpenFGA store.

Tuple sets are typically used for bootstrap permissions, role assignments, and other relationships that should be managed declaratively through Kubernetes manifests.

## Prerequisites

Before creating an `FgaTupleSet` resource:

* A connected OpenFGA instance must be available
* A corresponding `FgaStore` resource must exist
* The target store must be successfully reconciled
* An authorization model should already be available in the target store

## Example

```yaml
apiVersion: openfga.sathiyaraman-m.com/v1
kind: FgaTupleSet
metadata:
  name: bootstrap-tuples
spec:
  storeRef:
    name: my-store
  tuples:
    - user: user:anne
      relation: admin
      object: organization:acme
```

## Reconciliation

When an `FgaTupleSet` resource is reconciled, the operator:

1. Resolves the target store
2. Reads the desired tuples from the resource
3. Creates missing tuples in OpenFGA
4. Removes tuples that were previously managed by this resource but are no longer desired
5. Updates reconciliation status

## Ownership Model

OpenFGA stores may contain tuples from multiple sources.

Examples include:

### Operator-managed tuples

* Bootstrap permissions
* Initial role assignments
* GitOps-managed relationships

### Runtime-managed tuples

* Application-created relationships
* User-generated sharing permissions
* Dynamically assigned permissions

The operator only manages tuples that belong to the `FgaTupleSet` resource.

Runtime-generated tuples are never modified or removed.

For more details, see [Concepts](concepts.md).

## Safe Tuple Deletion

When tuples are removed from an `FgaTupleSet` resource, the operator removes only the tuples previously managed by that resource.

For example:

### Desired State

```text
A
B
```

### Previously Managed

```text
A
B
C
```

### Result

```text
Delete C
```

Unrelated tuples in the store are left untouched.

This allows tuple sets to be managed declaratively without risking accidental removal of application-managed data.

## Multiple Tuple Sets

Multiple `FgaTupleSet` resources can target the same store.

This can be useful when different teams, applications, or deployment units manage different sets of bootstrap permissions.

Each tuple set is reconciled independently.

## Status

The operator updates the resource status with reconciliation information and conditions.

Example:

```yaml
status:
  storeId: 01HXXXXXXXXXXXXXXX
  managedTupleStates:
    - hash: ... # SHA256 hash of the tuple for change detection
      user: user:anne
      relation: admin
      object: organization:acme
  conditions:
    ...
```

The exact status fields may vary between releases.

## Deletion Behavior

Deleting an `FgaTupleSet` resource removes the tuples that were previously managed by that resource.

Tuples managed by other tuple sets remain unchanged.

Runtime-generated tuples remain unchanged.

## Related Resources

* [FgaConnectionConfig](connection-config.md)
* [FgaStore](store.md)
* [FgaAuthorizationModel](authorization-model.md)
* [Concepts](concepts.md)
