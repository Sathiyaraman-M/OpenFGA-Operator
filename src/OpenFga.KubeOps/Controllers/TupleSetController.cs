using k8s.Models;
using KubeOps.Abstractions.Rbac;
using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Services;

namespace OpenFga.KubeOps.Controllers;

[EntityRbac(typeof(V1TupleSet), Verbs = RbacVerb.All)]
public sealed class TupleSetController(TupleSetService tupleSetService, ILogger<TupleSetController> logger) : IEntityController<V1TupleSet>
{
    public async Task<ReconciliationResult<V1TupleSet>> ReconcileAsync(V1TupleSet entity, CancellationToken cancellationToken)
    {
        try
        {
            var isReconcilationSuccessful = await tupleSetService.ReconcileTupleSetAsync(entity, cancellationToken);
            if (!isReconcilationSuccessful)
            {
                return ReconciliationResult<V1TupleSet>.Failure(
                    entity: entity,
                    errorMessage: "Reconcilation was only partially successful. Please check the logs for more details.",
                    requeueAfter: TimeSpan.FromSeconds(30)
                );
            }
        }
        catch (ConnectionConfigNotFoundException e)
        {
            logger.LogError(e, "Error while reconciling OpenFGA Tuple Set {}", entity.Name());
            return ReconciliationResult<V1TupleSet>.Failure(entity, e.Message, e);
        }
        catch (AuthorizationStoreNotFoundException e)
        {
            logger.LogError(e, "Error while reconciling OpenFGA Tuple Set {}", entity.Name());
            return ReconciliationResult<V1TupleSet>.Failure(entity, e.Message, e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong while reconciling OpenFGA Tuple Set {}", entity.Name());
            return ReconciliationResult<V1TupleSet>.Failure(entity, e.Message, e);
        }

        return ReconciliationResult<V1TupleSet>.Success(entity);
    }

    public async Task<ReconciliationResult<V1TupleSet>> DeletedAsync(V1TupleSet entity, CancellationToken cancellationToken)
    {
        try
        {
            await tupleSetService.DeleteTupleSetAsync(entity, cancellationToken);
        }
        catch (ConnectionConfigNotFoundException e)
        {
            logger.LogError(e, "Error while deleting OpenFGA Tuple Set {}", entity.Name());
            return ReconciliationResult<V1TupleSet>.Failure(entity, e.Message, e);
        }
        catch (AuthorizationStoreNotFoundException e)
        {
            logger.LogError(e, "Error while deleting OpenFGA Tuple Set {}", entity.Name());
            return ReconciliationResult<V1TupleSet>.Failure(entity, e.Message, e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong while deleting OpenFGA Tuple Set {}", entity.Name());
            return ReconciliationResult<V1TupleSet>.Failure(entity, e.Message, e);
        }

        return ReconciliationResult<V1TupleSet>.Success(entity);
    }
}
