using OpenFga.Sdk.Exceptions;

namespace OpenFga.KubeOps;

public abstract class KubeOpsException(string message) : Exception(message);

public class StoreCreationFailedException(string storeName, ApiException exception) : KubeOpsException($"Failed to create Authorization Store {storeName}: {exception.Message}");

public class StoreQueryFailedException(string storeName, ApiException exception) : KubeOpsException($"Failed to query Authorization Store {storeName}: {exception.Message}");

public class MultipleStoresFoundException(string storeName) : KubeOpsException($"Multiple Authorization Stores with name '{storeName}' found");

public class AuthorizationModelUpdateFailedException(string storeName, ApiException exception) : KubeOpsException($"Failed to update Authorization Model for Store {storeName}: {exception.Message}");

public class TuplesWriteFailedException(string storeName, ApiException exception) : KubeOpsException($"Failed to write tuples for Store {storeName}: {exception.Message}");

public class ConnectionConfigNotFoundException(string configName) : KubeOpsException($"Conection Config with name '{configName}' not found");

public class AuthorizationStoreNotFoundException(string storeName) : KubeOpsException($"Authorization Store with name '{storeName}' not found");

public class StoreManifestNotFoundException(string storeName) : KubeOpsException($"Manifest for Authorization Store with name '{storeName}' not found");

public class ModelTransformationFailedException(string details) : KubeOpsException($"Model transformation failed: {details}");