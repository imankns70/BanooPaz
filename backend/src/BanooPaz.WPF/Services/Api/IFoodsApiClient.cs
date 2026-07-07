using BanooPaz.Contracts.Foods;

namespace BanooPaz.WPF.Services.Api;

public interface IFoodsApiClient
{
    Task<IReadOnlyList<FoodDto>> GetFoodsAsync(CancellationToken cancellationToken = default);
    Task<FoodDto?> GetFoodAsync(int id, CancellationToken cancellationToken = default);
    Task<FoodDto> CreateFoodAsync(CreateFoodRequest request, CancellationToken cancellationToken = default);
    Task UpdateFoodAsync(int id, UpdateFoodRequest request, CancellationToken cancellationToken = default);
    Task SetFoodActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
