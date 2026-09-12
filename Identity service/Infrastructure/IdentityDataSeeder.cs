namespace Identity_service.Infrastructure;

public interface IIdentityDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}

public sealed class IdentityDataSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext dbContext,
    IConfiguration configuration) : IIdentityDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedRoleAsync(DefaultRoles.Admin.Name, isDefault: false, cancellationToken);
        await SeedRoleAsync(ApplicationRoleNames.Customer, isDefault: true, cancellationToken);
        await SeedRoleAsync(ApplicationRoleNames.Driver, isDefault: false, cancellationToken);

        var admins = configuration
            .GetSection("Seed:Admins")
            .Get<List<SeedUser>>() ?? [];

        foreach (var admin in admins)
        {
            await SeedUserAsync(admin, DefaultRoles.Admin.Name, createCustomerProfile: false, cancellationToken);
        }

        var customers = configuration
            .GetSection("Seed:Customers")
            .Get<List<SeedUser>>() ?? [];

        foreach (var customer in customers)
        {
            await SeedUserAsync(customer, ApplicationRoleNames.Customer, createCustomerProfile: true, cancellationToken);
        }

        await SeedNotificationsAsync(cancellationToken);

        var applicants = configuration
            .GetSection("Seed:DriverApplicants")
            .Get<List<SeedDriverApplicant>>() ?? [];

        foreach (var applicant in applicants)
        {
            await SeedDriverApplicantAsync(applicant, cancellationToken);
        }
    }

    private async Task SeedRoleAsync(
        string roleName,
        bool isDefault,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
            IsDefault = isDefault,
            IsDeleted = false
        });

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Unable to seed the {roleName} role: {string.Join(" ", result.Errors.Select(error => error.Description))}");
        }
    }

    private async Task SeedDriverApplicantAsync(
        SeedDriverApplicant applicant,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(applicant.Email)
            || string.IsNullOrWhiteSpace(applicant.Password)
            || string.IsNullOrWhiteSpace(applicant.NationalId))
        {
            return;
        }

        var email = applicant.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = applicant.Phone,
                FirstName = applicant.FirstName,
                LastName = applicant.LastName,
                Gender = applicant.Gender
            };

            var created = await userManager.CreateAsync(user, applicant.Password);
            if (!created.Succeeded)
                throw new InvalidOperationException(string.Join(" ", created.Errors.Select(error => error.Description)));
        }
        else if (user.Gender is null && applicant.Gender is not null)
        {
            // Drivers seeded before Gender was supported were created without one; fill it in
            // so profile screens do not show a blank gender.
            user.Gender = applicant.Gender;
            var updated = await userManager.UpdateAsync(user);
            if (!updated.Succeeded)
                throw new InvalidOperationException(string.Join(" ", updated.Errors.Select(error => error.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, ApplicationRoleNames.Driver))
        {
            var roleResult = await userManager.AddToRoleAsync(user, ApplicationRoleNames.Driver);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }

        if (!await dbContext.DriverProfiles.AnyAsync(profile => profile.UserId == user.Id, cancellationToken))
        {
            dbContext.DriverProfiles.Add(new DriverProfile
            {
                UserId = user.Id,
                NationalId = applicant.NationalId,
                PlateNumber = applicant.PlateNumber,
                VehicleType = applicant.VehicleType
            });
        }

        if (!await dbContext.DriverApplications.AnyAsync(application => application.UserId == user.Id, cancellationToken))
        {
            dbContext.DriverApplications.Add(new DriverApplication
            {
                UserId = user.Id,
                Status = applicant.Status
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUserAsync(
        SeedUser seedUser,
        string roleName,
        bool createCustomerProfile,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(seedUser.Email)
            || string.IsNullOrWhiteSpace(seedUser.Password))
        {
            return;
        }

        var email = seedUser.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = string.IsNullOrWhiteSpace(seedUser.Id)
                    ? Guid.CreateVersion7().ToString()
                    : seedUser.Id,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = seedUser.Phone,
                FirstName = seedUser.FirstName,
                LastName = seedUser.LastName,
                Gender = seedUser.Gender,
                IsDisabled = false
            };

            var created = await userManager.CreateAsync(user, seedUser.Password);
            if (!created.Succeeded)
                throw new InvalidOperationException(string.Join(" ", created.Errors.Select(error => error.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }

        if (createCustomerProfile
            && !await dbContext.CustomerProfiles.AnyAsync(profile => profile.UserId == user.Id, cancellationToken))
        {
            dbContext.CustomerProfiles.Add(new CustomerProfile
            {
                UserId = user.Id
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedNotificationsAsync(CancellationToken cancellationToken)
    {
        var customer = await userManager.FindByEmailAsync("scrum23.addresses@flower.local");
        if (customer is null)
            return;

        var notifications = new[]
        {
            new UserNotification
            {
                Id = Guid.Parse("94000000-0000-0000-0000-000000000001"),
                UserId = customer.Id,
                Title = "New offer",
                Body = "Fresh bouquets are available today with a limited discount.",
                Type = "offer",
                DeepLink = "/catalog/products?sortBy=Discount",
                IsRead = false,
                CreatedAtUtc = new DateTime(2026, 9, 6, 10, 0, 0, DateTimeKind.Utc)
            },
            new UserNotification
            {
                Id = Guid.Parse("94000000-0000-0000-0000-000000000002"),
                UserId = customer.Id,
                Title = "New offer",
                Body = "A new flower collection has been added to the catalog.",
                Type = "offer",
                DeepLink = "/catalog/home/layout",
                IsRead = false,
                CreatedAtUtc = new DateTime(2026, 9, 5, 11, 30, 0, DateTimeKind.Utc)
            },
            new UserNotification
            {
                Id = Guid.Parse("94000000-0000-0000-0000-000000000003"),
                UserId = customer.Id,
                Title = "Remember",
                Body = "You still have saved addresses ready for fast checkout.",
                Type = "reminder",
                DeepLink = "/users/me/addresses",
                IsRead = true,
                CreatedAtUtc = new DateTime(2026, 9, 4, 9, 15, 0, DateTimeKind.Utc),
                ReadAtUtc = new DateTime(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc)
            }
        };

        foreach (var notification in notifications)
        {
            if (!await dbContext.UserNotifications.AnyAsync(existing => existing.Id == notification.Id, cancellationToken))
                dbContext.UserNotifications.Add(notification);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class SeedDriverApplicant
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FirstName { get; set; } = "Seed";
        public string LastName { get; set; } = "Driver";
        public Gender? Gender { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; } = VehicleType.Motorcycle;
        public DriverApplicationStatus Status { get; set; } = DriverApplicationStatus.PendingReview;
    }

    private sealed class SeedUser
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
    }
}
