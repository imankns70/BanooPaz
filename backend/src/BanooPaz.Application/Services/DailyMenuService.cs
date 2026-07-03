using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Menus;
using BanooPaz.Domain.Entities;

namespace BanooPaz.Application.Services;

public sealed class DailyMenuService(
    IDailyMenuRepository dailyMenuRepository,
    IFoodRepository foodRepository,
    IUnitOfWork unitOfWork) : IDailyMenuService
{
    public async Task<DailyMenuDto?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var menu = await dailyMenuRepository.GetByDateAsync(date, cancellationToken);
        return menu is null ? null : Map(menu);
    }

    public async Task<DailyMenuDto> CreateOrUpdateAsync(
        CreateOrUpdateDailyMenuRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var menu = await dailyMenuRepository.GetByDateAsync(request.MenuDate, cancellationToken);
        if (menu is null)
        {
            menu = new DailyMenu
            {
                MenuDate = request.MenuDate,
                IsOpen = request.IsOpen,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            };
            await dailyMenuRepository.AddAsync(menu, cancellationToken);
        }
        else
        {
            menu.IsOpen = request.IsOpen;
            menu.Note = request.Note;
        }

        foreach (var itemRequest in request.Items)
        {
            var food = await foodRepository.GetByIdAsync(itemRequest.FoodId, cancellationToken)
                ?? throw new ArgumentException($"Food with id {itemRequest.FoodId} was not found.");

            var existingItem = FindExistingItem(menu, itemRequest);
            if (existingItem is null)
            {
                menu.Items.Add(new DailyMenuItem
                {
                    FoodId = itemRequest.FoodId,
                    Food = food,
                    Price = itemRequest.Price,
                    CapacityPortions = itemRequest.CapacityPortions,
                    IsAvailable = itemRequest.IsAvailable,
                    CreatedAt = DateTime.UtcNow
                });
                continue;
            }

            if (existingItem.FoodId != itemRequest.FoodId)
            {
                throw new ArgumentException("An existing daily menu item's food cannot be changed.");
            }

            if (itemRequest.CapacityPortions < existingItem.SoldPortions)
            {
                throw new ArgumentException(
                    $"Capacity for food id {itemRequest.FoodId} cannot be less than its sold portions.");
            }

            existingItem.Price = itemRequest.Price;
            existingItem.CapacityPortions = itemRequest.CapacityPortions;
            existingItem.IsAvailable = itemRequest.IsAvailable;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(menu);
    }

    public async Task<bool> SetItemAvailabilityAsync(
        int dailyMenuItemId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        var item = await dailyMenuRepository.GetItemByIdAsync(dailyMenuItemId, cancellationToken);
        if (item is null)
        {
            return false;
        }

        item.IsAvailable = isAvailable;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DailyMenuItem? FindExistingItem(
        DailyMenu menu,
        UpsertDailyMenuItemRequest request)
    {
        if (request.Id.HasValue)
        {
            return menu.Items.SingleOrDefault(item => item.Id == request.Id.Value)
                ?? throw new ArgumentException(
                    $"Daily menu item with id {request.Id.Value} does not belong to this menu.");
        }

        return menu.Items.SingleOrDefault(item => item.FoodId == request.FoodId);
    }

    private static void ValidateRequest(CreateOrUpdateDailyMenuRequest request)
    {
        if (request.MenuDate == default)
        {
            throw new ArgumentException("Menu date is required.");
        }

        if (request.Items is null)
        {
            throw new ArgumentException("Menu items cannot be null.");
        }

        var duplicateFoodId = request.Items
            .GroupBy(item => item.FoodId)
            .FirstOrDefault(group => group.Count() > 1)?.Key;

        if (duplicateFoodId.HasValue)
        {
            throw new ArgumentException($"Food id {duplicateFoodId.Value} appears more than once.");
        }

        foreach (var item in request.Items)
        {
            if (item.FoodId <= 0)
            {
                throw new ArgumentException("Food id must be greater than zero.");
            }

            if (item.Price < 0)
            {
                throw new ArgumentException($"Price for food id {item.FoodId} cannot be negative.");
            }

            if (item.CapacityPortions < 0)
            {
                throw new ArgumentException($"Capacity for food id {item.FoodId} cannot be negative.");
            }
        }
    }

    private static DailyMenuDto Map(DailyMenu menu) => new()
    {
        Id = menu.Id,
        MenuDate = menu.MenuDate,
        IsOpen = menu.IsOpen,
        Note = menu.Note,
        Items = menu.Items
            .OrderBy(item => item.Id)
            .Select(item => new DailyMenuItemDto
            {
                Id = item.Id,
                FoodId = item.FoodId,
                FoodName = item.Food.Name,
                FoodDescription = item.Food.Description,
                ImageUrl = item.Food.ImageUrl,
                Price = item.Price,
                CapacityPortions = item.CapacityPortions,
                SoldPortions = item.SoldPortions,
                RemainingPortions = item.RemainingPortions,
                IsAvailable = item.IsAvailable
            })
            .ToList()
    };
}
