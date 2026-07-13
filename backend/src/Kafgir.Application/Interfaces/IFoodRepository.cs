using Kafgir.Domain.Entities;

namespace Kafgir.Application.Interfaces;

public interface IFoodRepository
{
    Task<IReadOnlyList<Food>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Food>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Food?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Food food, CancellationToken cancellationToken = default);
}
