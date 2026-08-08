using System.Security.Cryptography;
using System.Text;

namespace Identity_service.Services;

internal static class ResetTokenHash
{
    public static string Create(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
