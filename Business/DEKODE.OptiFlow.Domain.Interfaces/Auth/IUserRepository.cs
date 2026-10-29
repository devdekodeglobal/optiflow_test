namespace DEKODE.OptiFlow.Domain.Interfaces.Auth;

public interface IUserRepository
{
    Task<AuthUserWithCredentials?> GetByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task RecordSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RecordFailedLoginAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class AuthUserWithCredentials
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
}

