using DEKODE.OptiFlow.Domain.Interfaces.Auth;
using DEKODE.OptiFlow.DTO.Auth;
using DEKODE.OptiFlow.DTO.Common;
using DEKODE.OptiFlow.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DEKODE.OptiFlow.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthenticationService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    /// <summary>
    /// Authenticates a client against the domain authentication service and generates a valid JWT Bearer token.
    /// (Eliminates hardcoded credentials and relies on IAuthenticationService with cryptographic password verification).
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<object>.Fail("Username and password are required."));
        }

        var user = await _authService.ValidateCredentialsAsync(request.Username, request.Password, cancellationToken);
        if (user == null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid credentials provided."));
        }

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Username, user.Role);

        var responseDto = new AuthResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAt,
            Username = user.Username,
            Role = user.Role
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(responseDto, "Authentication successful."));
    }
}
