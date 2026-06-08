using k8s.Models;

namespace OpenFga.KubeOps.Extensions;

public static class ConditionExtensions
{
    extension(IList<V1Condition> conditions)
    {
        public void SetCondition(string type, string status, string reason, string message)
        {
            var existingCondition = conditions.FirstOrDefault(c => c.Type == type);
            if (existingCondition != null)
            {
                existingCondition.Status = status;
                existingCondition.Reason = reason;
                existingCondition.Message = message;
                existingCondition.LastTransitionTime = DateTime.UtcNow;
            }
            else
            {
                conditions.Add(new V1Condition
                {
                    Type = type,
                    Status = status,
                    Reason = reason,
                    Message = message,
                    LastTransitionTime = DateTime.UtcNow
                });
            }
        }
    }

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
