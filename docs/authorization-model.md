# FgaAuthorizationModel

`FgaAuthorizationModel` manages authorization models within an OpenFGA store.

Authorization models define the types, relations, and permissions used by OpenFGA when evaluating authorization requests.

## Prerequisites

Before creating an `FgaAuthorizationModel` resource:

* A connected OpenFGA instance must be available
* A corresponding `FgaStore` resource must exist
* The target store must be successfully reconciled

## Example

```yaml
apiVersion: openfga.sathiyaraman-m.com/v1
kind: FgaAuthorizationModel
metadata:
  name: my-authorization-model
spec:
  storeRef:
    name: my-store
  modelJson: |
    {
      ...
    }
```

## Reconciliation

When an `FgaAuthorizationModel` resource is reconciled, the operator:

1. Resolves the target store
2. Uploads the authorization model to OpenFGA
3. Records the resulting model identifier in status
4. Updates reconciliation status

## Model Versioning

OpenFGA authorization models are immutable.

When the model definition changes, the operator uploads a new authorization model version to the store.

Existing model versions remain available in OpenFGA.

For this reason, updating an `FgaAuthorizationModel` resource does not modify an existing model. Instead, it creates a new model version.

## Supported Format

The operator currently accepts authorization models in JSON format.

If your model is written in OpenFGA DSL format, convert it to JSON before using it in an `FgaAuthorizationModel` resource.

For example:

```bash
fga model transform --file model.fga
```

See the OpenFGA documentation for additional model authoring guidance.

## Status

The operator updates the resource status with information about the uploaded authorization model and reconciliation state.

Example:

```yaml
status:
  storeId: 01HXXXXXXXXXXXXXXX
  modelId: 01JXXXXXXXXXXXXXXX
  observedModelHash: ... # SHA256 hash of the model definition used to detect changes
  conditions:
    ...
```

The exact status fields may vary between releases.

## Deletion Behavior

Deleting an `FgaAuthorizationModel` resource does not remove previously uploaded models from OpenFGA.

This behavior matches OpenFGA's immutable authorization model design.

Authorization models remain available until the corresponding store is deleted.

## Related Resources

* [FgaConnectionConfig](connection-config.md)
* [FgaStore](store.md)
* [FgaTupleSet](tupleset.md)
* [Concepts](concepts.md)
