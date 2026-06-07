## Example

This folder has following manifests:
- `connectionConfig.yaml`: This manifest has the connection configuration to connect to the OpenFGA instance.
- `store.yaml`: This manifest has the store configuration to create a store in the OpenFGA instance.
- `model.yaml`: This manifest has the model (in JSON format) to create a model in the OpenFGA instance.
- `tupleset.yaml`: This manifest has the tuple set which has bunch of tuples to be added to the OpenFGA instance.

### Directions to use

- Make sure you have OpenFGA instance running and accessible from the target cluster where you are running your operator.
- The Operator uses the `metadata.name` from `AuthorizationStore` manifest to create a store in OpenFGA instance.
  - So, make sure to update the `metadata.name` field in `store.yaml` manifest before applying it.
  - You can't change the `metadata.name` field in `store.yaml` manifest after applying it once.
- Deleting the `AuthorizationStore` resource will NOT delete the store in OpenFGA instance.
  - You will have to delete the store manually from OpenFGA instance if you want to delete it.
- The Operator currently doesn't support directly using the DSL format for the Model manifest.
  - You will have to convert the DSL format to JSON format and then use it in the `model.yaml` manifest.
  - You can use the OpenFGA CLI tool to convert the DSL format to JSON format. Check `fga model transform`.
- Any change in the `model.yaml` manifest will create a new version of the same Model.
- Similar to `AuthorizationStore` resource, deleting the `Model` resource will NOT delete the model in OpenFGA instance
  - Since OpenFGA models are immutable, you can't delete them, even from OpenFGA instance, unless the entire store is deleted.
- Use `tupleset.yaml` manifest to add pre-defined tuples to the OpenFGA instance.
- You can have multiple `tupleset.yaml` manifests to the same OpenFGA store.
- Unlike `AuthorizationStore` and `AuthorizationModel` resources, deleting the `TupleSet` resource will delete the tuples from the OpenFGA instance.
