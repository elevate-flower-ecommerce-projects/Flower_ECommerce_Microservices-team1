using Address___Store_Coverage_Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Address___Store_Coverage_Service.Persistence;

public interface IAddressDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public sealed class AddressDataSeeder(AddressDbContext dbContext) : IAddressDataSeeder
{
    private static readonly StoreCoverageArea[] CoverageAreas =
    [
        new()
        {
            Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
            StoreId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
            City = "Cairo",
            Area = "Nasr City",
            MinLat = 30.020000m,
            MaxLat = 30.090000m,
            MinLng = 31.300000m,
            MaxLng = 31.390000m
        },
        new()
        {
            Id = Guid.Parse("70000000-0000-0000-0000-000000000002"),
            StoreId = Guid.Parse("60000000-0000-0000-0000-000000000002"),
            City = "Cairo",
            Area = "Maadi",
            MinLat = 29.940000m,
            MaxLat = 30.000000m,
            MinLng = 31.220000m,
            MaxLng = 31.310000m
        }
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var area in CoverageAreas)
        {
            if (await dbContext.StoreCoverageAreas.AnyAsync(existing => existing.Id == area.Id, cancellationToken))
                continue;

            dbContext.StoreCoverageAreas.Add(area);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
