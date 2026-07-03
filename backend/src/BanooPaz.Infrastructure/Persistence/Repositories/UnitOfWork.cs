using BanooPaz.Application.Interfaces;

namespace BanooPaz.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork(BanooPazDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
