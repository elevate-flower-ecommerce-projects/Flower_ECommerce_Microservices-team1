using System.Collections.Concurrent;

namespace Identity_service.Infrastructure.Implementations.Services;

/// <summary>Process-local protection for account and IP bursts; Identity supplies the persisted account lockout.</summary>
public sealed class AdminLoginAttemptGuard : IAdminLoginAttemptGuard
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private const int MaxFailures = 5;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _attempts = new(StringComparer.OrdinalIgnoreCase);

    public bool IsBlocked(string email, string? ipAddress) =>
        IsBlocked(Key("account", email)) || IsBlocked(Key("ip", ipAddress));

    public void RegisterFailure(string email, string? ipAddress)
    {
        Register(Key("account", email));
        Register(Key("ip", ipAddress));
    }

    public void ResetAccount(string email) => _attempts.TryRemove(Key("account", email), out _);

    private bool IsBlocked(string key)
    {
        if (!_attempts.TryGetValue(key, out var failures)) return false;
        RemoveExpired(failures);
        return failures.Count >= MaxFailures;
    }

    private void Register(string key)
    {
        var failures = _attempts.GetOrAdd(key, _ => new ConcurrentQueue<DateTime>());
        RemoveExpired(failures);
        failures.Enqueue(DateTime.UtcNow);
    }

    private static void RemoveExpired(ConcurrentQueue<DateTime> failures)
    {
        var cutoff = DateTime.UtcNow - Window;
        while (failures.TryPeek(out var attemptedOn) && attemptedOn < cutoff)
            failures.TryDequeue(out _);
    }

    private static string Key(string prefix, string? value) => $"{prefix}:{value?.Trim().ToUpperInvariant() ?? "unknown"}";
}
