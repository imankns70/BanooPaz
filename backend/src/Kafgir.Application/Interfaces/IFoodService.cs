using Kafgir.Contracts.Foods;

namespace Kafgir.Application.Interfaces;

public interface IFoodService
{
    Task<IReadOnlyList<FoodDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FoodDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FoodDto> CreateAsync(CreateFoodRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateFoodRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
