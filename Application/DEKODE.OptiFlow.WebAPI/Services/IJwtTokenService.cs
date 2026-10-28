namespace DEKODE.OptiFlow.WebAPI.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(string username, string role);
}
