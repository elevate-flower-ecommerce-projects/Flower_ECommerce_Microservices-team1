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
        await SeedCustomerRoleAsync(cancellationToken);

        var applicants = configuration
            .GetSection("Seed:DriverApplicants")
            .Get<List<SeedDriverApplicant>>() ?? [];

        foreach (var applicant in applicants)
        {
            await SeedDriverApplicantAsync(applicant, cancellationToken);
        }
    }

    private async Task SeedCustomerRoleAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await roleManager.RoleExistsAsync(ApplicationRoleNames.Customer))
            return;

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = ApplicationRoleNames.Customer,
            IsDefault = true,
            IsDeleted = false
        });

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Unable to seed the Customer role: {string.Join(" ", result.Errors.Select(error => error.Description))}");
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
                LastName = applicant.LastName
            };

            var created = await userManager.CreateAsync(user, applicant.Password);
            if (!created.Succeeded)
                throw new InvalidOperationException(string.Join(" ", created.Errors.Select(error => error.Description)));
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

    private sealed class SeedDriverApplicant
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FirstName { get; set; } = "Seed";
        public string LastName { get; set; } = "Driver";
        public string NationalId { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; } = VehicleType.Motorcycle;
        public DriverApplicationStatus Status { get; set; } = DriverApplicationStatus.PendingReview;
    }
}
