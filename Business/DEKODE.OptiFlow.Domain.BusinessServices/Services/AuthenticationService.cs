using System.Security.Cryptography;
using System.Text;
using DEKODE.OptiFlow.Domain.Interfaces.Auth;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Domain authentication service implementing secure credential verification with PBKDF2/SHA256 password hashing.
/// Decouples authentication from HTTP controllers and allows seamless swapping for Postgres user tables or external IdPs.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private record UserRecord(string Username, string Role, string DisplayName, string PasswordHash, string Salt);

    // In-memory credential store with cryptographically salted hashes for testing / default setup
    private readonly Dictionary<string, UserRecord> _users = new(StringComparer.OrdinalIgnoreCase);

    public AuthenticationService()
    {
        // Seed default development accounts with salted hashes
        SeedUser("admin", "Admin", "OptiFlow Administrator", "optiflow2026");
        SeedUser("planner", "User", "Merchandising Planner", "planner2026");
    }

    public Task<AuthUser?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Task.FromResult<AuthUser?>(null);

        if (!_users.TryGetValue(username, out var user))
            return Task.FromResult<AuthUser?>(null);

        bool isValid = VerifyPassword(password, user.PasswordHash, user.Salt);
        if (!isValid)
            return Task.FromResult<AuthUser?>(null);

        return Task.FromResult<AuthUser?>(new AuthUser
        {
            Username = user.Username,
            Role = user.Role,
            DisplayName = user.DisplayName
        });
    }

    private void SeedUser(string username, string role, string displayName, string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
        string salt = Convert.ToBase64String(saltBytes);
        string hash = HashPassword(password, saltBytes);
        _users[username] = new UserRecord(username, role, displayName, hash, salt);
    }

    private static string HashPassword(string password, byte[] salt)
    {
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash, string saltString)
    {
        byte[] salt = Convert.FromBase64String(saltString);
        string computedHash = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
