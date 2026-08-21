using Address___Store_Coverage_Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Address___Store_Coverage_Service.Persistence;

public interface IAddressDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public sealed class AddressDataSeeder(
    AddressDbContext dbContext,
    IHostEnvironment environment) : IAddressDataSeeder
{
    private const string Scrum23TestUserId = "30000000-0000-0000-0000-000000000023";

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

    private static readonly UserAddress[] Scrum23TestAddresses =
    [
        new()
        {
            Id = Guid.Parse("80000000-0000-0000-0000-000000000001"),
            UserId = Scrum23TestUserId,
            RecipientName = "SCRUM-23 Test Customer",
            Phone = "01012345678",
            AddressLine = "23 Abbas El Akkad Street, Building 4",
            City = "Cairo",
            Area = "Nasr City",
            Lat = 30.056100m,
            Lng = 31.330000m,
            Label = "SCRUM-23 Default",
            ServingStoreId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
            IsServiceable = true,
            IsDefault = true,
            CreatedAtUtc = new DateTime(2026, 1, 20, 9, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = Guid.Parse("80000000-0000-0000-0000-000000000002"),
            UserId = Scrum23TestUserId,
            RecipientName = "SCRUM-23 Test Customer",
            Phone = "01012345678",
            AddressLine = "9 Road 9, near Maadi Metro Station",
            City = "Cairo",
            Area = "Maadi",
            Lat = 29.960200m,
            Lng = 31.256900m,
            Label = "SCRUM-23 Update",
            ServingStoreId = Guid.Parse("60000000-0000-0000-0000-000000000002"),
            IsServiceable = true,
            IsDefault = false,
            CreatedAtUtc = new DateTime(2026, 1, 21, 9, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = Guid.Parse("80000000-0000-0000-0000-000000000003"),
            UserId = Scrum23TestUserId,
            RecipientName = "SCRUM-23 Test Customer",
            Phone = "01012345678",
            AddressLine = "15 Street 10, Maadi",
            City = "Cairo",
            Area = "Maadi",
            Lat = 29.970000m,
            Lng = 31.270000m,
            Label = "SCRUM-23 Delete",
            ServingStoreId = Guid.Parse("60000000-0000-0000-0000-000000000002"),
            IsServiceable = true,
            IsDefault = false,
            CreatedAtUtc = new DateTime(2026, 1, 22, 9, 0, 0, DateTimeKind.Utc)
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

        if (environment.IsDevelopment())
        {
            foreach (var address in Scrum23TestAddresses)
            {
                if (await dbContext.UserAddresses.AnyAsync(existing => existing.Id == address.Id, cancellationToken))
                    continue;

                dbContext.UserAddresses.Add(address);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
