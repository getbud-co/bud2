
namespace Bud.Infrastructure.Persistence;

public sealed class EfUnitOfWork(
    ApplicationDbContext dbContext) : IUnitOfWork
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
            await dbContext.SaveChangesAsync(cancellationToken);

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
