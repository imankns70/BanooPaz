using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Persistence.Repositories;

public sealed class FoodRepository(BanooPazDbContext dbContext) : IFoodRepository
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
