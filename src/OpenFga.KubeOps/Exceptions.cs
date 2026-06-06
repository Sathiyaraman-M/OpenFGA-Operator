namespace OpenFga.KubeOps;

public abstract class KubeOpsException(string message) : Exception(message);

public class ConnectionConfigNotFoundException(string configName) : KubeOpsException($"Conection Config with name '{configName}' not found");

public class AuthorizationStoreNotFoundException(string storeName) : KubeOpsException($"Authorization Store with name '{storeName}' not found");
