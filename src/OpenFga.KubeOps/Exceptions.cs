using OpenFga.KubeOps.Models;
using OpenFga.Sdk.Exceptions;

namespace OpenFga.KubeOps;

public abstract class KubeOpsException(string message) : Exception(message);

public class StoreCreationFailedException(string storeName, ApiException exception) : KubeOpsException($"Failed to create Authorization Store {storeName}: {exception.Message}");

public class StoreQueryFailedException(string storeName, ApiException exception) : KubeOpsException($"Failed to query Authorization Store {storeName}: {exception.Message}");

public class AuthorizationModelUpdateFailedException(StoreId storeId, ApiException exception) : KubeOpsException($"Failed to update Authorization Model for Store {storeId}: {exception.Message}");

public class ConnectionConfigNotFoundException(string configName) : KubeOpsException($"Conection Config with name '{configName}' not found");

public class AuthorizationStoreNotFoundException(string storeName) : KubeOpsException($"Authorization Store with name '{storeName}' not found");
