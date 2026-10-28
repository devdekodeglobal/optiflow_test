using System.ComponentModel.DataAnnotations;

namespace DEKODE.OptiFlow.WebAPI.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT SecretKey is required.")]
    [MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters (256 bits).")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required.")]
    public string Issuer { get; set; } = "OptiFlowAPI";

    [Required(ErrorMessage = "JWT Audience is required.")]
    public string Audience { get; set; } = "OptiFlowClients";

    [Range(1, 1440, ErrorMessage = "JWT ExpiryMinutes must be between 1 and 1440 (24 hours).")]
    public int ExpiryMinutes { get; set; } = 180;
}
