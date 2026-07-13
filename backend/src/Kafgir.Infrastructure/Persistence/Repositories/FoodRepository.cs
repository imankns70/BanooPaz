using Kafgir.Application.Interfaces;
using Kafgir.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kafgir.Infrastructure.Persistence.Repositories;

public sealed class FoodRepository(KafgirDbContext dbContext) : IFoodRepository
{
    public async Task<IReadOnlyList<Food>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Foods
            .AsNoTracking()
            .OrderBy(food => food.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Food>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Foods
            .AsNoTracking()
            .Where(food => food.IsActive)
            .OrderBy(food => food.Name)
            .ToListAsync(cancellationToken);

    public Task<Food?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Foods.SingleOrDefaultAsync(food => food.Id == id, cancellationToken);

    public async Task AddAsync(Food food, CancellationToken cancellationToken = default) =>
        await dbContext.Foods.AddAsync(food, cancellationToken);
}
