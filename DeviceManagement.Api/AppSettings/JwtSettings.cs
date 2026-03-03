using System.Diagnostics.CodeAnalysis;

namespace DeviceManagement.Api.AppSettings;

[ExcludeFromCodeCoverage]
public class JwtSettings
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Secret { get; set; }
    public int? ExpirationInMinutes { get; set; } = 30; // Default to 30 minutes if not set
}