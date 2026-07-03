using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Foods;
using BanooPaz.Domain.Entities;

namespace BanooPaz.Application.Services;

public sealed class FoodService(IFoodRepository foodRepository, IUnitOfWork unitOfWork) : IFoodService
{
    public async Task<IReadOnlyList<FoodDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var foods = await foodRepository.GetAllAsync(cancellationToken);
        return foods.Select(Map).ToList();
    }

    public async Task<FoodDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var food = await foodRepository.GetByIdAsync(id, cancellationToken);
        return food is null ? null : Map(food);
    }

    public async Task<FoodDto> CreateAsync(
        CreateFoodRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request.Name, request.DefaultPrice);

        var food = new Food
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            DefaultPrice = request.DefaultPrice,
            ImageUrl = request.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await foodRepository.AddAsync(food, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(food);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateFoodRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request.Name, request.DefaultPrice);

        var food = await foodRepository.GetByIdAsync(id, cancellationToken);
        if (food is null)
        {
            return false;
        }

        food.Name = request.Name.Trim();
        food.Description = request.Description;
        food.DefaultPrice = request.DefaultPrice;
        food.ImageUrl = request.ImageUrl;
        food.IsActive = request.IsActive;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var food = await foodRepository.GetByIdAsync(id, cancellationToken);
        if (food is null)
        {
            return false;
        }

        food.IsActive = isActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(string name, decimal defaultPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Food name is required.");
        }

        if (name.Trim().Length > 150)
        {
            throw new ArgumentException("Food name cannot exceed 150 characters.");
        }

        if (defaultPrice < 0)
        {
            throw new ArgumentException("Default price cannot be negative.");
        }
    }

    private static FoodDto Map(Food food) => new()
    {
        Id = food.Id,
        Name = food.Name,
        Description = food.Description,
        DefaultPrice = food.DefaultPrice,
        ImageUrl = food.ImageUrl,
        IsActive = food.IsActive
    };
}
