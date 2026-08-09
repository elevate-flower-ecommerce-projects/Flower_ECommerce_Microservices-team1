namespace Identity_service.Abstractions.Seeding;

public class DefaultRoles
{
    public partial class Admin
    {
        public const string Id = "b0d60c5c-4d20-4991-9171-772a0a8bd2f8";
        public const string Name = "Admin";
        public const string ConcurrencyStamp = "aaa623d1-2a70-49e8-96e3-53bd3380149a";
    }
    public partial class Customer
    {
        public const string Id = "23c617eb-34dd-41ca-b15a-b5630999daaa";
        public const string Name = "Customer";
        public const string ConcurrencyStamp = "ffc2d9b2-b2f9-4bcc-af93-eec61e521c87";
    }
}
