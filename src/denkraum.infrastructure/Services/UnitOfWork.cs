using Denkraum.Application.Abstractions;
using Denkraum.Infrastructure.Persistence;

namespace Denkraum.Infrastructure.Services;

public sealed class UnitOfWork(DenkraumDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
