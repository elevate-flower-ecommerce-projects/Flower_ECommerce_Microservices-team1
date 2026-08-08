using System.Security.Cryptography;
using System.Text;

namespace Identity_service.Infrastructure.Implementations.Services;

public static class RefreshTokenProtector
{
    public static string Generate() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
