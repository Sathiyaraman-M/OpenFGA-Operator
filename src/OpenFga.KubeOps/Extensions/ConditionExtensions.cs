using k8s.Models;

namespace OpenFga.KubeOps.Extensions;

public static class ConditionExtensions
{
    extension(V1Condition)
    {
        public static V1Condition New(string type, string status, string reason, string message) => new()
        {
            Type = type,
            Status = status,
            Reason = reason,
            Message = message,
            LastTransitionTime = DateTime.UtcNow
        };
    }
}
