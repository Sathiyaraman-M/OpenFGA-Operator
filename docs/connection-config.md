# FgaConnectionConfig

`FgaConnectionConfig` defines how the operator connects to an OpenFGA instance.

Other resources, such as `FgaStore`, reference a connection configuration to communicate with OpenFGA.

## Example

```yaml
apiVersion: openfga.sathiyaraman-m.com/v1
kind: FgaConnectionConfig
metadata:
  name: my-connection-config
spec:
  apiUrl: http://openfga.openfga.svc.cluster.local:8080
```

## Referencing a Connection Configuration

Other resources reference a connection configuration by name.

For example:

```yaml
spec:
  connectionConfigRef:
    name: my-connection-config
```

The referenced `FgaConnectionConfig` must exist before dependent resources can be reconciled successfully.

## Reconciliation

During reconciliation, the operator:

1. Resolves the referenced connection configuration
2. Connects to the configured OpenFGA instance
3. Performs the requested operation

If the connection configuration is invalid or the OpenFGA instance is unreachable, reconciliation will fail and the resource status will be updated accordingly.

> [!NOTE]
> A single `FgaConnectionConfig` resource alone does not do anything. It merely defines connection details that other resources can reference. It doesn't even have any status fields.

## Updating a Connection Configuration

Changes to a connection configuration are automatically picked up during subsequent reconciliations.

Resources that reference the configuration will begin using the updated connection details.

> [!WARNING]
> Updating a connection configuration can impact all dependent resources.

## Related Resources

* [FgaStore](store.md)
* [FgaAuthorizationModel](authorization-model.md)
* [FgaTupleSet](tupleset.md)
* [Concepts](concepts.md)
