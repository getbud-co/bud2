
namespace Bud.Infrastructure.Persistence;

public sealed class EfUnitOfWork(
    ApplicationDbContext dbContext,
    IDomainEventDispatcher? domainEventDispatcher = null) : IUnitOfWork
{
    private bool _isCommitInProgress;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_isCommitInProgress)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        _isCommitInProgress = true;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var domainEventSources = dbContext.ChangeTracker
                .Entries()
                .Select(e => e.Entity)
                .OfType<IHasDomainEvents>()
                .Where(entity => entity.DomainEvents.Count > 0)
                .Distinct()
                .ToList();

            var domainEvents = domainEventSources
                .SelectMany(entity => entity.DomainEvents)
                .ToArray();

            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var domainEventSource in domainEventSources)
            {
                domainEventSource.ClearDomainEvents();
            }

            if (domainEventDispatcher is not null && domainEvents.Length > 0)
            {
                await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            _isCommitInProgress = false;
        }
    }
}
