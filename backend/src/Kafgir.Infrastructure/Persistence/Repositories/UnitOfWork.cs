using Kafgir.Application.Interfaces;

namespace Kafgir.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork(KafgirDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
