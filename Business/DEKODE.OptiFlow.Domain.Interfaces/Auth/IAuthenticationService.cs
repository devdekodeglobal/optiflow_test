namespace DEKODE.OptiFlow.Domain.Interfaces.Auth;

public class AuthUser
{
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
    public string DisplayName { get; init; } = string.Empty;
}

public interface IAuthenticationService
{
    Task<AuthUser?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
}
