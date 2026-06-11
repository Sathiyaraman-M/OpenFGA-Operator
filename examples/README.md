# Example

This example demonstrates how to:

1. Connect to an OpenFGA instance
2. Create an OpenFGA store
3. Upload an authorization model
4. Manage bootstrap tuples

## Prerequisites

Before applying these manifests:

* Ensure OpenFGA K8s Operator is installed in your cluster.
* Ensure an OpenFGA instance is running and accessible from the cluster
* Review and update the manifests to match your environment

## Resources

This example contains the following files:

| File                    | Purpose                                                             |
| ----------------------- | ------------------------------------------------------------------- |
| `connectionConfig.yaml` | Connection details used by the operator to communicate with OpenFGA |
| `store.yaml`            | Creates an OpenFGA store                                            |
| `model.yaml`            | Uploads an authorization model to the store                         |
| `tupleset.yaml`         | Creates a set of managed tuples                                     |
| `model.fga`             | Example model in OpenFGA DSL format for reference                   |

## Apply the Example

Apply all resources:

```bash
kubectl apply -f examples/
```

## Verify Reconciliation

Verify that the resources were created successfully:

```bash
kubectl get fgaconnectionconfigs
kubectl get fgastores
kubectl get fgaauthorizationmodels
kubectl get fgatuplesets
```

Inspect the status of a resource:

```bash
kubectl describe fgastore <name>
```

The operator will reconcile the resources and create the corresponding artifacts in OpenFGA.

## Important Behaviors

### Store Names

The operator uses the `metadata.name` of the `FgaStore` resource as the store name in OpenFGA.

Before applying the example:

* Update the `metadata.name` field in `store.yaml` if desired
* Avoid changing the name after the store has been created

### Store Deletion

Deleting an `FgaStore` resource does not delete the corresponding store in OpenFGA.

Stores must be removed manually from OpenFGA if no longer needed.

### Authorization Models

The operator currently accepts authorization models in JSON format.

If your model is written in OpenFGA DSL format, convert it before using it in `model.yaml`.

For example:

```bash
fga model transform --file model.fga
```

Any change to `model.yaml` results in a new authorization model version being uploaded to the store.

Deleting an `FgaAuthorizationModel` resource does not remove previously uploaded models from OpenFGA. OpenFGA authorization models are immutable and remain available until the store itself is deleted.

### Tuple Sets

`FgaTupleSet` resources manage operator-owned tuples.

You can create multiple tuple sets for the same store.

Adding or modifying or deleting tuples in a tuple set will update the tuples in OpenFGA accordingly.

Deleting an `FgaTupleSet` resource removes all the tuples in the set from OpenFGA.
