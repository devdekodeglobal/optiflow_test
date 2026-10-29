using System.Security.Cryptography;
using System.Text;
using DEKODE.OptiFlow.Domain.Interfaces.Auth;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Domain authentication service implementing secure credential verification with PBKDF2/SHA256 password hashing
/// backed by the database via IUserRepository.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;

    public AuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<AuthUser?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalizedUsername = username.Trim().ToUpperInvariant();
        var user = await _userRepository.GetByNormalizedUsernameAsync(normalizedUsername, cancellationToken);
        if (user == null || !user.IsActive)
            return null;

        // Check for lockout
        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
            return null;

        bool isValid = VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        if (!isValid)
        {
            await _userRepository.RecordFailedLoginAsync(user.Id, cancellationToken);
            return null;
        }

        await _userRepository.RecordSuccessfulLoginAsync(user.Id, cancellationToken);

        return new AuthUser
        {
            Username = user.Username,
            Role = user.Role,
            DisplayName = user.DisplayName
        };
    }

    public static string HashPassword(string password, byte[] salt)
    {
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        byte[] salt = Convert.FromBase64String(storedSalt);
        string computedHash = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
