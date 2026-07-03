using BanooPaz.Domain.Entities;

namespace BanooPaz.UnitTests;

public sealed class DailyMenuItemTests
{
    [Fact]
    public void RemainingPortions_subtracts_sold_portions_from_capacity()
    {
        var item = new DailyMenuItem
        {
            CapacityPortions = 25,
            SoldPortions = 7
        };

        Assert.Equal(18, item.RemainingPortions);
    }
}
